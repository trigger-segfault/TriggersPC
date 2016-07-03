using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public class BoxPokemon : IPokemon {

		#region Members

		protected bool moving;
		protected bool released;

		public bool IsMoving {
			get { return moving; }
			set { moving = value; }
		}
		public bool IsReleased {
			get { return released; }
			set { released = value; }
		}

		public void RestoreHealth() {
			CurrentHP = HP;
			StatusCondition = StatusConditionFlags.None;
			for (int i = 0; i < NumMoves; i++) {
				SetMovePPAt(i, GetMoveTotalPPAt(i));
			}
		}
		private PokemonFinder pokemonFinder;
		public PokemonFinder PokemonFinder {
			get { return pokemonFinder; }
			set {
				pokemonFinder = value;
				pokemonFinder.Pokemon = this;
			}
		}

		public bool IsInDaycare {
			get { return pokeContainer.Type == ContainerTypes.Daycare; }
		}

		private IPokeContainer pokeContainer;
		private GameTypes gameType;
		private byte deoxysForm;

		private byte growthSubDataOffset;
		private byte attacksSubDataOffset;
		private byte evConditionSubDataOffset;
		private byte miscSubDataOffset;

		private byte[] raw;

		private BoxPokemon invalidBackup;

		#endregion

		public BoxPokemon() {
			this.raw = new byte[104];
			this.gameType = GameTypes.Any;
			this.pokemonFinder = new PokemonFinder(this);

			this.growthSubDataOffset = (byte)(32 + 0);
			this.attacksSubDataOffset = (byte)(32 + 12);
			this.evConditionSubDataOffset = (byte)(32 + 24);
			this.miscSubDataOffset = (byte)(32 + 36);

			this.deoxysForm = byte.MaxValue;
		}

		public BoxPokemon(byte[] data, bool decrypted = false) {
			if (data.Length != 84)
				throw new Exception("Pokemon Box Pokemon data must contain 84 bytes");
			this.gameType = GameTypes.Any;
			this.deoxysForm = byte.MaxValue;
			this.pokemonFinder = new PokemonFinder(this);

			if (decrypted)
				OpenDecryptedData(data);
			else
				OpenEncryptedData(data);
		}

		#region Basic Info

		public byte[] Raw {
			get { return raw; }
		}
		public IPokeContainer PokeContainer {
			get { return pokeContainer; }
			set { pokeContainer = value; }
		}
		public IPokePC PokePC {
			get {
				if (pokeContainer != null)
					return pokeContainer.PokePC;
				return null;
			}
		}
		public int ContainerIndex {
			get { return pokeContainer.IndexOf(this); }
		}
		public IGameSave GameSave {
			get {
				if (pokeContainer != null && pokeContainer.PokePC != null)
					return pokeContainer.PokePC.GameSave;
				return null;
			}
		}
		public GameTypes GameType {
			get { return gameType; }
			set {
				gameType = value;
				// Deoxys Stats change based on it's form which is based on the game
				// Deoxys is basically the whole reason I need to carry around the game type variable everywhere.
				if (DexID == 386)
					RecalculateStats();
			}
		}
		public bool IsValid {
			get {
				return PokemonDatabase.GetPokemonFromID(SpeciesID) != null && SpeciesID != 0 &&
						PokemonDatabase.GetMoveFromID(Move1ID) != null &&
						(PokemonDatabase.GetMoveFromID(Move2ID) != null || Move1ID == 0) &&
						(PokemonDatabase.GetMoveFromID(Move3ID) != null || Move1ID == 0) &&
						(PokemonDatabase.GetMoveFromID(Move4ID) != null || Move1ID == 0) &&
						PokemonDatabase.GetBallCaughtImageFromID(BallCaughtID) != null &&
						ItemDatabase.GetItemFromID(HeldItemID) != null &&
						Checksum != 0;
			}
		}

		#endregion

		#region Lookup Data Structures

		public PokemonData PokemonData {
			get { return PokemonDatabase.GetPokemonFromID(SpeciesID); }
		}
		public PokemonFormData PokemonFormData {
			get {
				if (HasForm)
					return PokemonDatabase.GetPokemonFromID(SpeciesID).GetForm(FormID);
				return null;
			}
		}
		public NatureData NatureData {
			get { return PokemonDatabase.GetNatureFromID(NatureID); }
		}
		public AbilityData AbilityData {
			get { return PokemonDatabase.GetAbilityFromID(!IsSecondAbility2 ? PokemonData.Ability1ID : PokemonData.Ability2ID); }
		}
		public ItemData BallCaughtItemData {
			get { return ItemDatabase.GetItemFromID(BallCaughtID); }
		}
		public ItemData HeldItemData {
			get { return ItemDatabase.GetItemFromID(HeldItemID); }
		}
		public MoveData Move1Data {
			get { return PokemonDatabase.GetMoveFromID(Move1ID); }
		}
		public MoveData Move2Data {
			get { return PokemonDatabase.GetMoveFromID(Move2ID); }
		}
		public MoveData Move3Data {
			get { return PokemonDatabase.GetMoveFromID(Move3ID); }
		}
		public MoveData Move4Data {
			get { return PokemonDatabase.GetMoveFromID(Move4ID); }
		}
		public MoveData GetMoveDataAt(int index) {
			switch (index) {
			case 0: return Move1Data;
			case 1: return Move2Data;
			case 2: return Move3Data;
			case 3: return Move4Data;
			}
			return null;
		}
		public BitmapSource Sprite {
			get {
				if (IsEgg)
					return PokemonDatabase.GetPokemonImageFromDexID(387, false);
				return PokemonDatabase.GetPokemonImageFromDexID(DexID, IsShiny, FormID);
			}
		}
		public BitmapImage BoxSprite {
			get {
				if (IsEgg)
					return PokemonDatabase.GetPokemonBoxImageFromDexID(387, IsShiny);
				return PokemonDatabase.GetPokemonBoxImageFromDexID(DexID, IsShiny, FormID);
			}
		}

		#endregion

		#region Pokemon Info

		public uint Personality {
			get { return LittleEndian.ToUInt32(raw, 0); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt32(value, raw, 0);
			}
		}
		public ushort SpeciesID {
			get { return LittleEndian.ToUInt16(raw, growthSubDataOffset); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, growthSubDataOffset);
			}
		}
		public ushort DexID {
			get { return PokemonData.DexID; }
			set { SpeciesID = PokemonDatabase.GetPokemonFromDexID(value).ID; }
		}
		public byte FormID {
			get {
				if (DexID == 201) { // Unown
					byte val = 0;
					val = ByteHelper.SetBits(val, 0, ByteHelper.GetBits(Personality, 0, 2));
					val = ByteHelper.SetBits(val, 2, ByteHelper.GetBits(Personality, 8, 2));
					val = ByteHelper.SetBits(val, 4, ByteHelper.GetBits(Personality, 16, 2));
					val = ByteHelper.SetBits(val, 6, ByteHelper.GetBits(Personality, 24, 2));
					return (byte)(val % 28);
				}
				if (DexID == 386) { // Deoxys
					// The deoxysForm var is used to override his form
					if (deoxysForm != byte.MaxValue)
						return deoxysForm;
					switch (gameType) {
					case GameTypes.FireRed: return 1;
					case GameTypes.LeafGreen: return 2;
					case GameTypes.Emerald: return 3;
					default: return 0;
					}
				}
				return byte.MaxValue;
			}
		}
		public byte DeoxysForm {
			get { return deoxysForm; }
			set {
				if (GameSave != null && GameSave.GameType == GameTypes.Any)
					GameSave.IsChanged = true;
				deoxysForm = value;
				RecalculateStats();
			}
		}
		public bool HasForm {
			get { return FormID != byte.MaxValue; }
		}
		public byte NatureID {
			get { return (byte)(Personality % 25U); }
		}
		public Genders Gender {
			get {
				if (PokemonData.GenderRatio == 255)
					return Genders.Genderless;
				byte p = (byte)(Personality % 256);
				if (PokemonData.GenderRatio == 254 || p < PokemonData.GenderRatio)
					return Genders.Female;
				else
					return Genders.Male;
			}
		}
		public bool IsSecondAbility {
			get { return (Personality & 0x1) == 0; }
		}
		public bool IsSecondAbility2 {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 4, 31); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 4, 31, value);
			}
		}
		public bool IsShiny {
			get {
				byte[] bytes = BitConverter.GetBytes(Personality);
				return ((uint)TrainerID ^ (uint)SecretID ^ (uint)BitConverter.ToUInt16(bytes, 0) ^ (uint)BitConverter.ToUInt16(bytes, 2)) < 8;
			}
		}
		public bool IsEgg {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 4, 30); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 4, 30, value);
			}
		}
		public PokemonTypes HiddenPowerType {
			get {
				int a = ((int)HPIV        % 2);
				int b = ((int)AttackIV    % 2) * 2;
				int c = ((int)DefenseIV   % 2) * 4;
				int d = ((int)SpeedIV     % 2) * 8;
				int e = ((int)SpAttackIV  % 2) * 16;
				int f = ((int)SpDefenseIV % 2) * 32;
				return (PokemonTypes)((a + b + c + d + e + f) * 15 / 63 + 2); // The plus two is because the types enum is offset by two
			}
		}
		public byte HiddenPowerDamage {
			get {
				int u = (((int)HPIV        / 2) % 2);
				int v = (((int)AttackIV    / 2) % 2) * 2;
				int w = (((int)DefenseIV   / 2) % 2) * 4;
				int x = (((int)SpeedIV     / 2) % 2) * 8;
				int y = (((int)SpAttackIV  / 2) % 2) * 16;
				int z = (((int)SpDefenseIV / 2) % 2) * 32;
				return (byte)((u + v + w + x + y + z) * 40 / 63 + 30);
			}
		}
		public bool WurpleIsCascoon {
			get { return (Personality >> 16) % 10 >= 5; }
		}

		#endregion

		#region Met Info

		public string TrainerName {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(20, raw, 7), Language); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.ReplaceBytes(raw, 20, GBACharacterEncoding.GetBytes(value, 7, Language));
			}
		}
		public Genders TrainerGender {
			get { return (Genders)ByteHelper.BitsToByte(raw, miscSubDataOffset + 2, 15, 1); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 2, 15, 1, (byte)value);
			}
		}
		public ushort TrainerID {
			get { return BitConverter.ToUInt16(raw, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, 4);
			}
		}
		public ushort SecretID {
			get { return LittleEndian.ToUInt16(raw, 6); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, 6);
			}
		}
		public byte LevelMet {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 2, 0, 7); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 2, 0, 7, value);
			}
		}
		public ushort MetLocationID {
			get { return raw[miscSubDataOffset + 1]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[miscSubDataOffset + 1] = (byte)value;
			}
		}
		public bool IsFatefulEncounter {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 31); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 31, value);
			}
		}
		public byte BallCaughtID {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 2, 11, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 2, 11, 4, value);
			}
		}
		public GameOrigins GameOrigin {
			get { return (GameOrigins)ByteHelper.BitsToByte(raw, miscSubDataOffset + 2, 7, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 2, 7, 4, (byte)value);
			}
		}
		public Languages Language {
			get { return (Languages)LittleEndian.ToUInt16(raw, 18); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16((ushort)value, raw, 18);
			}
		}

		#endregion

		#region Personalization Info

		public string Nickname {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(8, raw, 10), Language); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value == "")
					value = PokemonData.Name.ToUpper();
				ByteHelper.ReplaceBytes(raw, 8, GBACharacterEncoding.GetBytes(value, 10, Language));
			}
		}
		public bool HasNickname {
			get { return Nickname != PokemonData.Name.ToUpper(); }
		}
		public ushort HeldItemID {
			get { return LittleEndian.ToUInt16(raw, growthSubDataOffset + 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, growthSubDataOffset + 2);
			}
		}
		public bool IsHoldingItem {
			get { return HeldItemID != 0; }
		}
		public MarkingFlags Markings {
			get { return (MarkingFlags)raw[27]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[27] = (byte)value;
			}
		}
		public bool IsCircleMarked {
			get { return Markings.HasFlag(MarkingFlags.Circle); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value) Markings |= MarkingFlags.Circle;
				else Markings &= ~MarkingFlags.Circle;
			}
		}
		public bool IsSquareMarked {
			get { return Markings.HasFlag(MarkingFlags.Square); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value) Markings |= MarkingFlags.Square;
				else Markings &= ~MarkingFlags.Square;
			}
		}
		public bool IsTriangleMarked {
			get { return Markings.HasFlag(MarkingFlags.Triangle); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value) Markings |= MarkingFlags.Triangle;
				else Markings &= ~MarkingFlags.Triangle;
			}
		}
		public bool IsHeartMarked {
			get { return Markings.HasFlag(MarkingFlags.Heart); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value) Markings |= MarkingFlags.Heart;
				else Markings &= ~MarkingFlags.Heart;
			}
		}
		public bool IsHoldingMail {
			get { return HeldItemID >= 121 && HeldItemID <= 132; }
		}

		#endregion

		#region Stats Info

		public byte Level {
			get { return raw[88]; }
			set { raw[88] = value; }
		}
		public uint Experience {
			get { return LittleEndian.ToUInt32(raw, growthSubDataOffset + 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt32(value, raw, growthSubDataOffset + 4);
			}
		}
		public byte Friendship {
			get { return raw[growthSubDataOffset + 9]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[growthSubDataOffset + 9] = value;
			}
		}

		public ushort HP {
			get { return LittleEndian.ToUInt16(raw, 92); }
			set { LittleEndian.WriteUInt16(value, raw, 92); }
		}
		public ushort Attack {
			get { return LittleEndian.ToUInt16(raw, 94); }
			set { LittleEndian.WriteUInt16(value, raw, 94); }
		}
		public ushort Defense {
			get { return LittleEndian.ToUInt16(raw, 96); }
			set { LittleEndian.WriteUInt16(value, raw, 96); }
		}
		public ushort Speed {
			get { return LittleEndian.ToUInt16(raw, 98); }
			set { LittleEndian.WriteUInt16(value, raw, 98); }
		}
		public ushort SpAttack {
			get { return LittleEndian.ToUInt16(raw, 100); }
			set { LittleEndian.WriteUInt16(value, raw, 100); }
		}
		public ushort SpDefense {
			get { return LittleEndian.ToUInt16(raw, 102); }
			set { LittleEndian.WriteUInt16(value, raw, 102); }
		}

		public byte HPEV {
			get { return raw[evConditionSubDataOffset]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset] = value;
			}
		}
		public byte AttackEV {
			get { return raw[evConditionSubDataOffset + 1]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 1] = value;
			}
		}
		public byte DefenseEV {
			get { return raw[evConditionSubDataOffset + 2]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 2] = value;
			}
		}
		public byte SpeedEV {
			get { return raw[evConditionSubDataOffset + 3]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 3] = value;
			}
		}
		public byte SpAttackEV {
			get { return raw[evConditionSubDataOffset + 4]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 4] = value;
			}
		}
		public byte SpDefenseEV {
			get { return raw[evConditionSubDataOffset + 5]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 5] = value;
			}
		}

		public byte HPIV {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 4, 0, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 4, 0, 5, value);
			}
		}
		public byte AttackIV {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 4, 5, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 4, 5, 5, value);
			}
		}
		public byte DefenseIV {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 4, 10, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 4, 10, 5, value);
			}
		}
		public byte SpeedIV {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 4, 15, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 4, 15, 5, value);
			}
		}
		public byte SpAttackIV {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 4, 20, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 4, 20, 5, value);
			}
		}
		public byte SpDefenseIV {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 4, 25, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 4, 25, 5, value);
			}
		}

		#endregion

		#region Moves Info

		public byte NumMoves {
			get {
				for (int i = 0; i < 4; i++) {
					if (GetMoveIDAt(i) == 0)
						return (byte)i;
				}
				return 4;
			}
		}

		public ushort Move1ID {
			get { return LittleEndian.ToUInt16(raw, attacksSubDataOffset); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, attacksSubDataOffset);
			}
		}
		public ushort Move2ID {
			get { return LittleEndian.ToUInt16(raw, attacksSubDataOffset + 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, attacksSubDataOffset + 2);
			}
		}
		public ushort Move3ID {
			get { return LittleEndian.ToUInt16(raw, attacksSubDataOffset + 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, attacksSubDataOffset + 4);
			}
		}
		public ushort Move4ID {
			get { return LittleEndian.ToUInt16(raw, attacksSubDataOffset + 6); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, attacksSubDataOffset + 6);
			}
		}
		public byte Move1PP {
			get { return raw[attacksSubDataOffset + 8]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[attacksSubDataOffset + 8] = value;
			}
		}
		public byte Move2PP {
			get { return raw[attacksSubDataOffset + 9]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[attacksSubDataOffset + 9] = value;
			}
		}
		public byte Move3PP {
			get { return raw[attacksSubDataOffset + 10]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[attacksSubDataOffset + 10] = value;
			}
		}
		public byte Move4PP {
			get { return raw[attacksSubDataOffset + 11]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[attacksSubDataOffset + 11] = value;
			}
		}
		public byte Move1PPUpsUsed {
			get { return ByteHelper.BitsToByte(raw, growthSubDataOffset + 8, 0, 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, growthSubDataOffset + 8, 0, 2, value);
			}
		}
		public byte Move2PPUpsUsed {
			get { return ByteHelper.BitsToByte(raw, growthSubDataOffset + 8, 2, 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, growthSubDataOffset + 8, 2, 2, value);
			}
		}
		public byte Move3PPUpsUsed {
			get { return ByteHelper.BitsToByte(raw, growthSubDataOffset + 8, 4, 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, growthSubDataOffset + 8, 4, 2, value);
			}
		}
		public byte Move4PPUpsUsed {
			get { return ByteHelper.BitsToByte(raw, growthSubDataOffset + 8, 46, 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, growthSubDataOffset + 8, 6, 2, value);
			}
		}
		public byte Move1TotalPP {
			get { return (byte)(Move1ID != 0 ? ((int)Move1Data.PP + (int)Move1Data.PP / 5 * (int)Move1PPUpsUsed) : 0); }
		}
		public byte Move2TotalPP {
			get { return (byte)(Move2ID != 0 ? ((int)Move2Data.PP + (int)Move2Data.PP / 5 * (int)Move2PPUpsUsed) : 0); }
		}
		public byte Move3TotalPP {
			get { return (byte)(Move3ID != 0 ? ((int)Move3Data.PP + (int)Move3Data.PP / 5 * (int)Move3PPUpsUsed) : 0); }
		}
		public byte Move4TotalPP {
			get { return (byte)(Move4ID != 0 ? ((int)Move4Data.PP + (int)Move4Data.PP / 5 * (int)Move4PPUpsUsed) : 0); }
		}

		public Move GetMoveAt(int index) {
			switch (index) {
			case 0: return new Move(Move1ID, Move1PP, Move1PPUpsUsed);
			case 1: return new Move(Move2ID, Move2PP, Move2PPUpsUsed);
			case 2: return new Move(Move3ID, Move3PP, Move3PPUpsUsed);
			case 3: return new Move(Move4ID, Move4PP, Move4PPUpsUsed);
			}
			return new Move();
		}
		public void SetMoveAt(int index, Move move) {
			switch (index) {
			case 0: Move1ID = move.ID; Move1PPUpsUsed = move.PPUpsUsed; Move1PP = move.PP; break;
			case 1: Move2ID = move.ID; Move2PPUpsUsed = move.PPUpsUsed; Move2PP = move.PP; break;
			case 2: Move3ID = move.ID; Move3PPUpsUsed = move.PPUpsUsed; Move3PP = move.PP; break;
			case 3: Move4ID = move.ID; Move4PPUpsUsed = move.PPUpsUsed; Move4PP = move.PP; break;
			}
		}
		public ushort GetMoveIDAt(int index) {
			switch (index) {
			case 0: return Move1ID;
			case 1: return Move2ID;
			case 2: return Move3ID;
			case 3: return Move4ID;
			}
			return 0;
		}
		public void SetMoveIDAt(int index, ushort id) {
			switch (index) {
			case 0: Move1ID = id; break;
			case 1: Move2ID = id; break;
			case 2: Move3ID = id; break;
			case 3: Move4ID = id; break;
			}
		}
		public byte GetMovePPAt(int index) {
			switch (index) {
			case 0: return Move1PP;
			case 1: return Move2PP;
			case 2: return Move3PP;
			case 3: return Move4PP;
			}
			return 0;
		}
		public void SetMovePPAt(int index, byte pp) {
			switch (index) {
			case 0: Move1PP = pp; break;
			case 1: Move2PP = pp; break;
			case 2: Move3PP = pp; break;
			case 3: Move4PP = pp; break;
			}
		}
		public byte GetMovePPUpsUsedAt(int index) {
			switch (index) {
			case 0: return Move1PPUpsUsed;
			case 1: return Move2PPUpsUsed;
			case 2: return Move3PPUpsUsed;
			case 3: return Move4PPUpsUsed;
			}
			return 0;
		}
		public void SetMovePPUpsUsedAt(int index, byte ppUpsUsed) {
			switch (index) {
			case 0: Move1PPUpsUsed = ppUpsUsed; break;
			case 1: Move2PPUpsUsed = ppUpsUsed; break;
			case 2: Move3PPUpsUsed = ppUpsUsed; break;
			case 3: Move4PPUpsUsed = ppUpsUsed; break;
			}
		}
		public byte GetMoveTotalPPAt(int index) {
			switch (index) {
			case 0: return Move1TotalPP;
			case 1: return Move2TotalPP;
			case 2: return Move3TotalPP;
			case 3: return Move4TotalPP;
			}
			return 0;
		}

		public void MoveMove(int oldIndex, int newIndex) {
			List<Move> moveList = new List<Move>();
			for (int i = 0; i < NumMoves; i++)
				moveList.Add(GetMoveAt(i));
			Move move = moveList[oldIndex];
			moveList.RemoveAt(oldIndex);
			moveList.Insert(newIndex, move);
			for (int i = 0; i < 4; i++) {
				if (i < NumMoves)
					SetMoveAt(i, moveList[i]);
				else
					SetMoveAt(i, new Move());
			}
		}

		public void SwitchMove(int move1Index, int move2Index) {
			List<Move> moveList = new List<Move>();
			for (int i = 0; i < NumMoves; i++)
				moveList.Add(GetMoveAt(i));
			Move move1 = moveList[move1Index];
			moveList[move1Index] = moveList[move2Index];
			moveList[move2Index] = move1;
			for (int i = 0; i < 4; i++) {
				if (i < NumMoves)
					SetMoveAt(i, moveList[i]);
				else
					SetMoveAt(i, new Move());
			}
		}

		public void RemoveMoveAt(int index) {
			List<Move> moveList = new List<Move>();
			for (int i = 0; i < NumMoves; i++)
				moveList.Add(GetMoveAt(i));
			moveList.RemoveAt(index);
			for (int i = 0; i < 4; i++) {
				if (i < NumMoves)
					SetMoveAt(i, moveList[i]);
				else
					SetMoveAt(i, new Move());
			}
		}

		public void AddMove(Move move) {
			if (NumMoves < 4) {
				List<Move> moveList = new List<Move>();
				for (int i = 0; i < NumMoves; i++)
					moveList.Add(GetMoveAt(i));
				moveList.Add(move);
				for (int i = 0; i < 4; i++) {
					if (i < NumMoves)
						SetMoveAt(i, moveList[i]);
					else
						SetMoveAt(i, new Move());
				}
			}
		}
		public void InsertMove(int index, Move move) {
			if (NumMoves < 4) {
				List<Move> moveList = new List<Move>();
				for (int i = 0; i < NumMoves; i++)
					moveList.Add(GetMoveAt(i));
				moveList.Insert(index, move);
				for (int i = 0; i < 4; i++) {
					if (i < NumMoves)
						SetMoveAt(i, moveList[i]);
					else
						SetMoveAt(i, new Move());
				}
			}
		}

		#endregion

		#region Contest Info

		public byte Coolness {
			get { return raw[evConditionSubDataOffset + 6]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 6] = value;
			}
		}
		public byte Beauty {
			get { return raw[evConditionSubDataOffset + 7]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 7] = value;
			}
		}
		public byte Cuteness {
			get { return raw[evConditionSubDataOffset + 8]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 8] = value;
			}
		}
		public byte Smartness {
			get { return raw[evConditionSubDataOffset + 9]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 9] = value;
			}
		}
		public byte Toughness {
			get { return raw[evConditionSubDataOffset + 10]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 10] = value;
			}
		}
		public byte Feel {
			get { return raw[evConditionSubDataOffset + 11]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[evConditionSubDataOffset + 11] = value;
			}
		}

		public byte CoolRibbonCount {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 8, 0, 3); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 8, 0, 3, value);
			}
		}
		public byte BeautyRibbonCount {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 8, 3, 3); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 8, 3, 3, value);
			}
		}
		public byte CuteRibbonCount {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 8, 6, 3); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 8, 6, 3, value);
			}
		}
		public byte SmartRibbonCount {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 8, 9, 3); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 8, 9, 3, value);
			}
		}
		public byte ToughRibbonCount {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset + 8, 12, 3); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset + 8, 12, 3, value);
			}
		}

		public bool HasChampionRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 15); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 15, value);
			}
		}
		public bool HasWinningRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 16); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 16, value);
			}
		}
		public bool HasVictoryRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 17); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 17, value);
			}
		}
		public bool HasArtistRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 18); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 18, value);
			}
		}
		public bool HasEffortRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 19); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 19, value);
			}
		}
		public bool HasMarineRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 20); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 20, value);
			}
		}
		public bool HasLandRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 21); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 21, value);
			}
		}
		public bool HasSkyRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 22); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 22, value);
			}
		}
		public bool HasCountryRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 23); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 23, value);
			}
		}
		public bool HasNationalRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 24); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 24, value);
			}
		}
		public bool HasEarthRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 25); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 25, value);
			}
		}
		public bool HasWorldRibbon {
			get { return ByteHelper.GetBit(raw, miscSubDataOffset + 8, 26); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, miscSubDataOffset + 8, 26, value);
			}
		}

		#endregion

		#region Status Info

		public ushort CurrentHP {
			get { return LittleEndian.ToUInt16(raw, 90); }
			set { LittleEndian.WriteUInt16(value, raw, 90); }
		}
		public StatusConditionFlags StatusCondition {
			get { return (StatusConditionFlags)LittleEndian.ToUInt32(raw, 84); }
			set { LittleEndian.WriteUInt32((uint)value, raw, 84); }
		}
		public byte TurnsOfSleepRemaining {
			get { return 0; }
			set { }
		}
		public byte TurnsOfBadPoison {
			get { return 0; }
			set { }
		}
		public PokerusStatuses PokerusStatus {
			get {
				byte x = ByteHelper.BitsToByte(raw, miscSubDataOffset, 4, 4);
				byte y = ByteHelper.BitsToByte(raw, miscSubDataOffset, 0, 4);
				if (x != 0)
					return (y != 0 ? PokerusStatuses.Infected : PokerusStatuses.Cured);
				else
					return (y != 0 ? PokerusStatuses.Invalid : PokerusStatuses.None);
			}
		}
		public PokerusStrainTypes PokerusStrainType {
			get { return (PokerusStrain != 0 ? (PokerusStrainTypes)ByteHelper.BitsToByte(raw, miscSubDataOffset, 4, 2) : PokerusStrainTypes.None); }
		}
		public PokerusStrainVariations PokerusStrainVariation {
			get { return (PokerusStrain != 0 ? (PokerusStrainVariations)ByteHelper.BitsToByte(raw, miscSubDataOffset, 6, 2) : PokerusStrainVariations.None); }
		}
		public byte PokerusStrain {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset, 4, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset, 4, 4, value);
			}
		}
		public byte PokerusDaysRemaining {
			get { return ByteHelper.BitsToByte(raw, miscSubDataOffset, 0, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, miscSubDataOffset, 0, 4, value);
			}
		}
		public byte PokerusRemaining {
			get { return raw[89]; }
			set { raw[89] = value; }
		}

		#endregion

		#region Unique Info

		public ushort Checksum {
			get { return LittleEndian.ToUInt16(raw, 28); }
			set { LittleEndian.WriteUInt16(value, raw, 28); }
		}
		public ushort Unknown {
			get { return LittleEndian.ToUInt16(raw, 30); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, 30);
			}
		}
		public ushort SendingTrainerID {
			get { return LittleEndian.ToUInt16(raw, 80); }
			set { LittleEndian.WriteUInt16(value, raw, 80); }
		}
		public ushort SendingSecretID {
			get { return LittleEndian.ToUInt16(raw, 82); }
			set { LittleEndian.WriteUInt16(value, raw, 82); }
		}

		#endregion

		#region Shadow Pokemon Info

		public ushort ShadowPokemonID {
			get { return 0; }
			set {  }
		}
		public bool IsShadowPokemon {
			get { return false; }
		}
		public bool IsPurifiedShadowPokemon {
			get { return false; }
		}
		public int Purification {
			get { return 0; }
			set {  }
		}
		public uint HeartGauge {
			get { return 0; }
		}
		public uint ExperienceStored {
			get { return 0; }
			set { }
		}

		#endregion

		#region Reading

		private void OpenEncryptedData(byte[] data) {
			if (data.Length != 84)
				throw new Exception("Pokemon data must contain 84 bytes");
			raw = data;
			byte[] subData = ByteHelper.SubByteArray(32, data, 48);
			SubDataParseWithOrder(subData, SubDataOrder);
			if (data.Length == 84)
				RecalculateStats();
		}
		private void OpenDecryptedData(byte[] data) {
			growthSubDataOffset = (byte)(32 + 0);
			attacksSubDataOffset = (byte)(32 + 12);
			evConditionSubDataOffset = (byte)(32 + 24);
			miscSubDataOffset = (byte)(32 + 36);

			if (data.Length != 84)
				throw new Exception("Pokemon data must contain 84 bytes");
			raw = data;
			if (data.Length == 84)
				RecalculateStats();
		}

		#endregion

		#region Encryption

		private byte[] GrowthSubData {
			get { return ByteHelper.SubByteArray(growthSubDataOffset, raw, 12); }
		}
		private byte[] AttacksSubData {
			get { return ByteHelper.SubByteArray(attacksSubDataOffset, raw, 12); }
		}
		private byte[] EVConditionSubData {
			get { return ByteHelper.SubByteArray(evConditionSubDataOffset, raw, 12); }
		}
		private byte[] MiscSubData {
			get { return ByteHelper.SubByteArray(miscSubDataOffset, raw, 12); }
		}
		public uint FullTrainerID {
			get { return LittleEndian.ToUInt32(raw, 4); }
			set { LittleEndian.WriteUInt32(value, raw, 4); }
		}
		private uint SecurityKey {
			get { return FullTrainerID ^ Personality; }
		}
		private SubDataOrder SubDataOrder {
			get { return (SubDataOrder)(Personality % 24U); }
		}
		private void SubDataParseWithOrder(byte[] subData, SubDataOrder order) {
			switch (order) {
			case SubDataOrder.GAEM:
				SubDataParse(subData, 0, 12, 24, 36); break;
			case SubDataOrder.GAME:
				SubDataParse(subData, 0, 12, 36, 24); break;
			case SubDataOrder.GEAM:
				SubDataParse(subData, 0, 24, 12, 36); break;
			case SubDataOrder.GEMA:
				SubDataParse(subData, 0, 36, 12, 24); break;
			case SubDataOrder.GMAE:
				SubDataParse(subData, 0, 24, 36, 12); break;
			case SubDataOrder.GMEA:
				SubDataParse(subData, 0, 36, 24, 12); break;
			case SubDataOrder.AGEM:
				SubDataParse(subData, 12, 0, 24, 36); break;
			case SubDataOrder.AGME:
				SubDataParse(subData, 12, 0, 36, 24); break;
			case SubDataOrder.AEGM:
				SubDataParse(subData, 24, 0, 12, 36); break;
			case SubDataOrder.AEMG:
				SubDataParse(subData, 36, 0, 12, 24); break;
			case SubDataOrder.AMGE:
				SubDataParse(subData, 24, 0, 36, 12); break;
			case SubDataOrder.AMEG:
				SubDataParse(subData, 36, 0, 24, 12); break;
			case SubDataOrder.EGAM:
				SubDataParse(subData, 12, 24, 0, 36); break;
			case SubDataOrder.EGMA:
				SubDataParse(subData, 12, 36, 0, 24); break;
			case SubDataOrder.EAGM:
				SubDataParse(subData, 24, 12, 0, 36); break;
			case SubDataOrder.EAMG:
				SubDataParse(subData, 36, 12, 0, 24); break;
			case SubDataOrder.EMGA:
				SubDataParse(subData, 24, 36, 0, 12); break;
			case SubDataOrder.EMAG:
				SubDataParse(subData, 36, 24, 0, 12); break;
			case SubDataOrder.MGAE:
				SubDataParse(subData, 12, 24, 36, 0); break;
			case SubDataOrder.MGEA:
				SubDataParse(subData, 12, 36, 24, 0); break;
			case SubDataOrder.MAGE:
				SubDataParse(subData, 24, 12, 36, 0); break;
			case SubDataOrder.MAEG:
				SubDataParse(subData, 36, 12, 24, 0); break;
			case SubDataOrder.MEGA:
				SubDataParse(subData, 24, 36, 12, 0); break;
			case SubDataOrder.MEAG:
				SubDataParse(subData, 36, 24, 12, 0); break;
			}
		}
		private byte[] SubDataMergeWithOrder(byte[] g, byte[] a, byte[] m, byte[] e, SubDataOrder order) {
			switch (order) {
			case SubDataOrder.GAEM:
				return SubDataMerge(g, a, e, m);
			case SubDataOrder.GAME:
				return SubDataMerge(g, a, m, e);
			case SubDataOrder.GEAM:
				return SubDataMerge(g, e, a, m);
			case SubDataOrder.GEMA:
				return SubDataMerge(g, e, m, a);
			case SubDataOrder.GMAE:
				return SubDataMerge(g, m, a, e);
			case SubDataOrder.GMEA:
				return SubDataMerge(g, m, e, a);
			case SubDataOrder.AGEM:
				return SubDataMerge(a, g, e, m);
			case SubDataOrder.AGME:
				return SubDataMerge(a, g, m, e);
			case SubDataOrder.AEGM:
				return SubDataMerge(a, e, g, m);
			case SubDataOrder.AEMG:
				return SubDataMerge(a, e, m, g);
			case SubDataOrder.AMGE:
				return SubDataMerge(a, m, g, e);
			case SubDataOrder.AMEG:
				return SubDataMerge(a, m, e, g);
			case SubDataOrder.EGAM:
				return SubDataMerge(e, g, a, m);
			case SubDataOrder.EGMA:
				return SubDataMerge(e, g, m, a);
			case SubDataOrder.EAGM:
				return SubDataMerge(e, a, g, m);
			case SubDataOrder.EAMG:
				return SubDataMerge(e, a, m, g);
			case SubDataOrder.EMGA:
				return SubDataMerge(e, m, g, a);
			case SubDataOrder.EMAG:
				return SubDataMerge(e, m, a, g);
			case SubDataOrder.MGAE:
				return SubDataMerge(m, g, a, e);
			case SubDataOrder.MGEA:
				return SubDataMerge(m, g, e, a);
			case SubDataOrder.MAGE:
				return SubDataMerge(m, a, g, e);
			case SubDataOrder.MAEG:
				return SubDataMerge(m, a, e, g);
			case SubDataOrder.MEGA:
				return SubDataMerge(m, e, g, a);
			case SubDataOrder.MEAG:
				return SubDataMerge(m, e, a, g);
			default:
				return new byte[48];
			}
		}

		private void SubDataParse(byte[] subData, int growthPos, int attackPos, int evPos, int miscPos) {
			growthSubDataOffset = (byte)(32 + growthPos);
			attacksSubDataOffset = (byte)(32 + attackPos);
			evConditionSubDataOffset = (byte)(32 + evPos);
			miscSubDataOffset = (byte)(32 + miscPos);
			byte[] subDataSection1 = ByteHelper.SubByteArray(growthPos, subData, 12);
			byte[] subDataSection2 = ByteHelper.SubByteArray(attackPos, subData, 12);
			byte[] subDataSection3 = ByteHelper.SubByteArray(evPos, subData, 12);
			byte[] subDataSection4 = ByteHelper.SubByteArray(miscPos, subData, 12);
			ByteHelper.ReplaceBytes(raw, growthSubDataOffset, Decrypt(subDataSection1));
			ByteHelper.ReplaceBytes(raw, attacksSubDataOffset, Decrypt(subDataSection2));
			ByteHelper.ReplaceBytes(raw, evConditionSubDataOffset, Decrypt(subDataSection3));
			ByteHelper.ReplaceBytes(raw, miscSubDataOffset, Decrypt(subDataSection4));
		}
		private byte[] SubDataMerge(byte[] pos1, byte[] pos2, byte[] pos3, byte[] pos4) {
			List<byte> list = new List<byte>(48);
			list.AddRange(pos1);
			list.AddRange(pos2);
			list.AddRange(pos3);
			list.AddRange(pos4);
			return list.ToArray();
		}

		private byte[] Decrypt(byte[] subDataSection) {
			uint securityKey = SecurityKey;
			byte[] bytes1 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 0) ^ securityKey);
			byte[] bytes2 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 4) ^ securityKey);
			byte[] bytes3 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 8) ^ securityKey);
			List<byte> list = new List<byte>(12);
			list.AddRange(bytes1);
			list.AddRange(bytes2);
			list.AddRange(bytes3);
			return list.ToArray();
		}

		private byte[] Encrypt(byte[] subDataSection) {
			uint securityKey = SecurityKey;
			byte[] bytes1 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 0) ^ securityKey);
			byte[] bytes2 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 4) ^ securityKey);
			byte[] bytes3 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 8) ^ securityKey);
			List<byte> list = new List<byte>(12);
			list.AddRange(bytes1);
			list.AddRange(bytes2);
			list.AddRange(bytes3);
			return list.ToArray();
		}

		#endregion

		#region Modification Functions

		public void RemoveNickname() {
			Nickname = PokemonData.Name.ToUpper();
		}
		public void WipeEVs() {
			HPEV = 0;
			AttackEV = 0;
			DefenseEV = 0;
			SpeedEV = 0;
			SpAttackEV = 0;
			SpDefenseEV = 0;
			RecalculateStats();
		}
		public void InfectWithPokerus(byte strain) {
			if (PokerusStatus == PokerusStatuses.None) {
				PokerusStrain = strain;
				PokerusDaysRemaining = (byte)((strain % 4) + 1);
			}
		}
		public void CurePokerus() {
			PokerusDaysRemaining = 0;
		}
		public void SetHiddenPowerType(PokemonTypes type) {
			if (type < PokemonTypes.Fighting || type > PokemonTypes.Dark)
				return; // Invalid Hidden Power type
			// The minus two is because the types enum is offset by two due to including the Unknown and Normal types
			byte typeBits = (byte)(((int)type - 2 - 30) * 63 / 50); // The minimum value for this move type

			// Now we could say that we're done since we have the info to change the IVs.
			// But let's make sure the IVs are as close to the originals as possible.
			BitArray originalTypeBits = GetHiddenPowerTypeBits();
			int minDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(typeBits), originalTypeBits);
			byte minDeviationTypeBits = typeBits;
			while (typeBits + 1 <= 0x3F && GetHiddenPowerType((byte)(typeBits + 1)) == type) {
				typeBits++;
				int newDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(typeBits), originalTypeBits);
				if (newDeviation < minDeviation) {
					minDeviation = newDeviation;
					minDeviationTypeBits = typeBits;
				}
			}

			SetHiddenPowerTypeBits(new BitArray(minDeviationTypeBits));
		}
		public void SetHiddenPowerDamage(byte damage) {
			if (damage < 30 || damage > 70)
				return; // Outside of damage range
			byte damageBits = (byte)(((int)damage - 30) * 63 / 40); // The minimum value for this damage

			// Now we could say that we're done since we have the info to change the IVs.
			// But let's make sure the IVs are as close to the originals as possible.
			BitArray originalDamageBits = GetHiddenPowerDamageBits();
			int minDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(damageBits), originalDamageBits);
			byte minDeviationDamageBits = damageBits;
			while (damageBits + 1 <= 0x3F && GetHiddenPowerDamage((byte)(damageBits + 1)) == damage) {
				damageBits++;
				int newDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(damageBits), originalDamageBits);
				if (newDeviation < minDeviation) {
					minDeviation = newDeviation;
					minDeviationDamageBits = damageBits;
				}
			}

			SetHiddenPowerDamageBits(new BitArray(minDeviationDamageBits));
		}

		#endregion

		#region Functions

		public override string ToString() {
			string nickname = (IsEgg ? "EGG" : Nickname);
			string speciesName = (PokemonData != null ? PokemonData.Name : "Invalid");
			string moves = (Move1Data.ID != 0 ? Move1Data.Name : "Invalid") +
							(Move2Data.ID != 0 ? "," + Move2Data.Name : "") +
							(Move3Data.ID != 0 ? "," + Move3Data.Name : "") +
							(Move4Data.ID != 0 ? "," + Move4Data.Name : "");
			return "GBAPokemon: " + nickname + " (" + speciesName + "), Lv" + Level + ", [" + moves + "]";
		}

		public void RecalculateStats() {
			if (raw.Length == 84) {
				List<byte> list = new List<byte>();
				list.AddRange(raw);
				list.AddRange(new byte[20]);
				raw = list.ToArray();
			}
			if (!IsValid)
				return;

			int baseHP = PokemonData.HP;
			int baseAttack = PokemonData.Attack;
			int baseDefense = PokemonData.Defense;
			int baseSpeed = PokemonData.Speed;
			int baseSpAttack = PokemonData.SpAttack;
			int baseSpDefense = PokemonData.SpDefense;
			// Only needed for Deoxys (Such a troublesome pokemon)
			if (HasForm) {
				baseHP = PokemonFormData.HP;
				baseAttack = PokemonFormData.Attack;
				baseDefense = PokemonFormData.Defense;
				baseSpeed = PokemonFormData.Speed;
				baseSpAttack = PokemonFormData.SpAttack;
				baseSpDefense = PokemonFormData.SpDefense;
			}

			Level = PokemonDatabase.GetLevelFromExperience(PokemonData.ExperienceGroup, Experience);
			PokerusRemaining = byte.MaxValue;
			// Shedinja's DexID (Always has 1 HP)
			HP = (DexID != 292 ? Convert.ToUInt16(Math.Floor(((double)baseHP * 2 + (double)HPIV + (double)HPEV / 4) * (double)Level / 100 + 10 + (double)Level)) : (ushort)1);
			CurrentHP = HP;
			Attack = Convert.ToUInt16(Math.Floor(Math.Floor(((double)baseAttack * 2 + (double)AttackIV + (double)AttackEV / 4) * (double)Level / 100 + 5) * NatureData.AttackModifier));
			Defense = Convert.ToUInt16(Math.Floor(Math.Floor(((double)baseDefense * 2 + (double)DefenseIV + (double)DefenseEV / 4) * (double)Level / 100 + 5) * NatureData.DefenseModifier));
			SpAttack = Convert.ToUInt16(Math.Floor(Math.Floor(((double)baseSpAttack * 2 + (double)SpAttackIV + (double)SpAttackEV / 4) * (double)Level / 100 + 5) * NatureData.SpAttackModifier));
			SpDefense = Convert.ToUInt16(Math.Floor(Math.Floor(((double)baseSpDefense * 2 + (double)SpDefenseIV + (double)SpDefenseEV / 4) * (double)Level / 100 + 5) * NatureData.SpDefenseModifier));
			Speed = Convert.ToUInt16(Math.Floor(Math.Floor(((double)baseSpeed * 2 + (double)SpeedIV + (double)SpeedEV / 4) * (double)Level / 100 + 5) * NatureData.SpeedModifier));
		}

		#region Hidden Power

		private PokemonTypes GetHiddenPowerType(byte typesBits) {
			// The plus two is because the types enum is offset by two due to including Unknown and Normal types
			return (PokemonTypes)((int)typesBits * 15 / 63 + 2);
		}

		private byte GetHiddenPowerDamage(byte damageBits) {
			return (byte)((int)damageBits * 40 / 63 + 30);
		}
		private BitArray GetHiddenPowerTypeBits() {
			BitArray typeBits = new BitArray(6);
			typeBits[0] = ByteHelper.GetBit(HPIV, 0);
			typeBits[1] = ByteHelper.GetBit(AttackIV, 0);
			typeBits[2] = ByteHelper.GetBit(DefenseIV, 0);
			typeBits[3] = ByteHelper.GetBit(SpeedIV, 0);
			typeBits[4] = ByteHelper.GetBit(SpAttackIV, 0);
			typeBits[5] = ByteHelper.GetBit(SpDefenseIV, 0);
			return typeBits;
		}
		private void SetHiddenPowerTypeBits(BitArray typeBits) {
			HPIV		= ByteHelper.SetBit(HPIV, 0, typeBits[0]);
			AttackIV	= ByteHelper.SetBit(AttackIV, 0, typeBits[1]);
			DefenseIV	= ByteHelper.SetBit(DefenseIV, 0, typeBits[2]);
			SpeedIV		= ByteHelper.SetBit(SpeedIV, 0, typeBits[3]);
			SpAttackIV	= ByteHelper.SetBit(SpAttackIV, 0, typeBits[4]);
			SpDefenseIV	= ByteHelper.SetBit(SpDefenseIV, 0, typeBits[5]);
		}
		private BitArray GetHiddenPowerDamageBits() {
			BitArray damageBits = new BitArray(6);
			damageBits[0] = ByteHelper.GetBit(HPIV, 1);
			damageBits[1] = ByteHelper.GetBit(AttackIV, 1);
			damageBits[2] = ByteHelper.GetBit(DefenseIV, 1);
			damageBits[3] = ByteHelper.GetBit(SpeedIV, 1);
			damageBits[4] = ByteHelper.GetBit(SpAttackIV, 1);
			damageBits[5] = ByteHelper.GetBit(SpDefenseIV, 1);
			return damageBits;
		}
		private void SetHiddenPowerDamageBits(BitArray damageBits) {
			HPIV		= ByteHelper.SetBit(HPIV, 1, damageBits[0]);
			AttackIV	= ByteHelper.SetBit(AttackIV, 1, damageBits[1]);
			DefenseIV	= ByteHelper.SetBit(DefenseIV, 1, damageBits[2]);
			SpeedIV		= ByteHelper.SetBit(SpeedIV, 1, damageBits[3]);
			SpAttackIV	= ByteHelper.SetBit(SpAttackIV, 1, damageBits[4]);
			SpDefenseIV	= ByteHelper.SetBit(SpDefenseIV, 1, damageBits[5]);
		}

		#endregion

		#endregion

		#region Saving Functions

		public ushort CalculateChecksum() {
			ushort checksum = 0;
			byte[] numArray = SubDataMerge(GrowthSubData, AttacksSubData, MiscSubData, EVConditionSubData);
			int startIndex = 0;
			while (startIndex < 48) {
				checksum += BitConverter.ToUInt16(numArray, startIndex);
				startIndex += 2;
			}
			return checksum;
		}
		public byte[] GetFinalEncryptedData() {
			if (invalidBackup != null)
				return invalidBackup.GetFinalEncryptedData();
			byte[] rawData = raw.Clone() as byte[];
			byte[] g = Encrypt(GrowthSubData);
			byte[] a = Encrypt(AttacksSubData);
			byte[] e = Encrypt(EVConditionSubData);
			byte[] m = Encrypt(MiscSubData);
			byte[] dataToReplace = SubDataMergeWithOrder(g, a, m, e, SubDataOrder);
			ByteHelper.ReplaceBytes(rawData, 32, dataToReplace);
			LittleEndian.WriteUInt16(CalculateChecksum(), rawData, 28);
			return rawData;
		}
		public byte[] GetFinalDecryptedData() {
			if (invalidBackup != null)
				return invalidBackup.GetFinalDecryptedData();
			LittleEndian.WriteUInt16(CalculateChecksum(), raw, 28);
			return raw;
		}

		public byte[] GetFinalData() {
			return ByteHelper.SubByteArray(0, GetFinalEncryptedData(), 84);
		}

		#endregion

		#region Converting

		public IPokemon Clone() {
			return CreateBoxPokemon(false);
		}
		public GBAPokemon CreateGBAPokemon(GameTypes gameType, bool passFinder = true) {
			GBAPokemon pkm = new GBAPokemon();

			if (passFinder)
				pkm.PokemonFinder = PokemonFinder;
			pkm.GameType = gameType;
			pkm.DeoxysForm = DeoxysForm;
			pkm.Language = Language;

			// Pokemon Info
			pkm.Personality = Personality;
			pkm.SpeciesID = SpeciesID;
			pkm.IsSecondAbility2 = IsSecondAbility2;
			pkm.IsEgg = IsEgg;

			// Met Info
			pkm.TrainerName = TrainerName;
			pkm.TrainerGender = TrainerGender;
			pkm.TrainerID = TrainerID;
			pkm.SecretID = SecretID;
			pkm.BallCaughtID = BallCaughtID;
			pkm.LevelMet = LevelMet;
			pkm.MetLocationID = MetLocationID;
			pkm.IsFatefulEncounter = IsFatefulEncounter;
			pkm.GameOrigin = GameOrigin;

			// Personalization Info
			pkm.Nickname = Nickname;
			pkm.HeldItemID = HeldItemID;
			pkm.Markings = Markings;

			// Stats Info
			pkm.Experience = Experience;
			pkm.Friendship = Friendship;

			pkm.HPEV = HPEV;
			pkm.AttackEV = AttackEV;
			pkm.DefenseEV = DefenseEV;
			pkm.SpeedEV = SpeedEV;
			pkm.SpAttackEV = SpAttackEV;
			pkm.SpDefenseEV = SpDefenseEV;

			pkm.HPIV = HPIV;
			pkm.AttackIV = AttackIV;
			pkm.DefenseIV = DefenseIV;
			pkm.SpeedIV = SpeedIV;
			pkm.SpAttackIV = SpAttackIV;
			pkm.SpDefenseIV = SpDefenseIV;

			// Status Info
			pkm.StatusCondition = StatusConditionFlags.None;
			pkm.TurnsOfSleepRemaining = 0;
			pkm.TurnsOfBadPoison = 0;
			pkm.PokerusStrain = PokerusStrain;
			pkm.PokerusDaysRemaining = PokerusDaysRemaining;
			pkm.PokerusRemaining = PokerusRemaining;

			// Contest Info
			pkm.Coolness = Coolness;
			pkm.Beauty = Beauty;
			pkm.Cuteness = Cuteness;
			pkm.Smartness = Smartness;
			pkm.Toughness = Toughness;
			pkm.Feel = Feel;

			pkm.CoolRibbonCount = CoolRibbonCount;
			pkm.BeautyRibbonCount = BeautyRibbonCount;
			pkm.CuteRibbonCount = CuteRibbonCount;
			pkm.SmartRibbonCount = SmartRibbonCount;
			pkm.ToughRibbonCount = ToughRibbonCount;
			pkm.HasChampionRibbon = HasChampionRibbon;
			pkm.HasWinningRibbon = HasWinningRibbon;
			pkm.HasVictoryRibbon = HasVictoryRibbon;
			pkm.HasArtistRibbon = HasArtistRibbon;
			pkm.HasEffortRibbon = HasEffortRibbon;
			pkm.HasMarineRibbon = HasMarineRibbon;
			pkm.HasLandRibbon = HasLandRibbon;
			pkm.HasSkyRibbon = HasSkyRibbon;
			pkm.HasCountryRibbon = HasCountryRibbon;
			pkm.HasNationalRibbon = HasNationalRibbon;
			pkm.HasEarthRibbon = HasEarthRibbon;
			pkm.HasWorldRibbon = HasWorldRibbon;

			// Move Info
			pkm.SetMoveAt(0, GetMoveAt(0));
			pkm.SetMoveAt(1, GetMoveAt(1));
			pkm.SetMoveAt(2, GetMoveAt(2));
			pkm.SetMoveAt(3, GetMoveAt(3));

			pkm.Checksum = pkm.CalculateChecksum();

			// Recalculate Stats to make sure they're accurate
			pkm.RecalculateStats();

			return pkm;
		}
		public BoxPokemon CreateBoxPokemon(bool passFinder = true) {
			BoxPokemon pkm = new BoxPokemon();

			if (passFinder)
				pkm.PokemonFinder = PokemonFinder;
			pkm.GameType = GameTypes.PokemonBox;
			pkm.DeoxysForm = DeoxysForm;
			pkm.Language = Language;
			if (pokeContainer != null) {
				if (pokeContainer.GameType == GameTypes.PokemonBox) {
					pkm.SendingTrainerID = SendingTrainerID;
					pkm.SendingSecretID = SendingSecretID;
				}
				else {
					pkm.SendingTrainerID = pokeContainer.GameSave.TrainerID;
					pkm.SendingSecretID = pokeContainer.GameSave.SecretID;
				}
			}

			// Pokemon Info
			pkm.Personality = Personality;
			pkm.SpeciesID = SpeciesID;
			pkm.IsSecondAbility2 = IsSecondAbility2;
			pkm.IsEgg = IsEgg;

			// Met Info
			pkm.TrainerName = TrainerName;
			pkm.TrainerGender = TrainerGender;
			pkm.TrainerID = TrainerID;
			pkm.SecretID = SecretID;
			pkm.BallCaughtID = BallCaughtID;
			pkm.LevelMet = LevelMet;
			pkm.MetLocationID = MetLocationID;
			pkm.IsFatefulEncounter = IsFatefulEncounter;
			pkm.GameOrigin = GameOrigin;

			// Personalization Info
			pkm.Nickname = Nickname;
			pkm.HeldItemID = HeldItemID;
			pkm.Markings = Markings;

			// Stats Info
			pkm.Experience = Experience;
			pkm.Friendship = Friendship;

			pkm.HPEV = HPEV;
			pkm.AttackEV = AttackEV;
			pkm.DefenseEV = DefenseEV;
			pkm.SpeedEV = SpeedEV;
			pkm.SpAttackEV = SpAttackEV;
			pkm.SpDefenseEV = SpDefenseEV;

			pkm.HPIV = HPIV;
			pkm.AttackIV = AttackIV;
			pkm.DefenseIV = DefenseIV;
			pkm.SpeedIV = SpeedIV;
			pkm.SpAttackIV = SpAttackIV;
			pkm.SpDefenseIV = SpDefenseIV;

			// Status Info
			pkm.StatusCondition = StatusConditionFlags.None;
			pkm.TurnsOfSleepRemaining = 0;
			pkm.TurnsOfBadPoison = 0;
			pkm.PokerusStrain = PokerusStrain;
			pkm.PokerusDaysRemaining = PokerusDaysRemaining;
			pkm.PokerusRemaining = PokerusRemaining;

			// Contest Info
			pkm.Coolness = Coolness;
			pkm.Beauty = Beauty;
			pkm.Cuteness = Cuteness;
			pkm.Smartness = Smartness;
			pkm.Toughness = Toughness;
			pkm.Feel = Feel;

			pkm.CoolRibbonCount = CoolRibbonCount;
			pkm.BeautyRibbonCount = BeautyRibbonCount;
			pkm.CuteRibbonCount = CuteRibbonCount;
			pkm.SmartRibbonCount = SmartRibbonCount;
			pkm.ToughRibbonCount = ToughRibbonCount;
			pkm.HasChampionRibbon = HasChampionRibbon;
			pkm.HasWinningRibbon = HasWinningRibbon;
			pkm.HasVictoryRibbon = HasVictoryRibbon;
			pkm.HasArtistRibbon = HasArtistRibbon;
			pkm.HasEffortRibbon = HasEffortRibbon;
			pkm.HasMarineRibbon = HasMarineRibbon;
			pkm.HasLandRibbon = HasLandRibbon;
			pkm.HasSkyRibbon = HasSkyRibbon;
			pkm.HasCountryRibbon = HasCountryRibbon;
			pkm.HasNationalRibbon = HasNationalRibbon;
			pkm.HasEarthRibbon = HasEarthRibbon;
			pkm.HasWorldRibbon = HasWorldRibbon;

			// Move Info
			pkm.SetMoveAt(0, GetMoveAt(0));
			pkm.SetMoveAt(1, GetMoveAt(1));
			pkm.SetMoveAt(2, GetMoveAt(2));
			pkm.SetMoveAt(3, GetMoveAt(3));

			pkm.Checksum = pkm.CalculateChecksum();

			// Recalculate Stats to make sure they're accurate
			pkm.RecalculateStats();

			return pkm;
		}
		public ColosseumPokemon CreateColosseumPokemon(GCRegions currentRegion, bool passFinder = true) {
			ColosseumPokemon pkm = new ColosseumPokemon();

			if (passFinder)
				pkm.PokemonFinder = PokemonFinder;
			pkm.GameType = GameTypes.Colosseum;
			pkm.DeoxysForm = DeoxysForm;
			pkm.Language = Language;
			pkm.CurrentRegion = currentRegion;
			if (Language == Languages.Japanese)
				pkm.OriginalRegion = GCRegions.NTSC_J;
			else if (Language == Languages.English)
				pkm.OriginalRegion = GCRegions.NTSC_U;
			else
				pkm.OriginalRegion = GCRegions.PAL;

			// Pokemon Info
			pkm.Personality = Personality;
			pkm.SpeciesID = SpeciesID;
			pkm.IsSecondAbility2 = IsSecondAbility2;

			// Met Info
			pkm.TrainerName = TrainerName;
			pkm.TrainerGender = TrainerGender;
			pkm.TrainerID = TrainerID;
			pkm.SecretID = SecretID;
			pkm.BallCaughtID = BallCaughtID;
			pkm.LevelMet = LevelMet;
			pkm.MetLocationID = MetLocationID;
			pkm.IsFatefulEncounter = IsFatefulEncounter;
			pkm.GameOrigin = GameOrigin;

			// Personalization Info
			pkm.Nickname = Nickname;
			pkm.HeldItemID = HeldItemID;
			pkm.Markings = Markings;

			// Stats Info
			pkm.Experience = Experience;
			pkm.Friendship = Friendship;

			pkm.HPEV = HPEV;
			pkm.AttackEV = AttackEV;
			pkm.DefenseEV = DefenseEV;
			pkm.SpeedEV = SpeedEV;
			pkm.SpAttackEV = SpAttackEV;
			pkm.SpDefenseEV = SpDefenseEV;

			pkm.HPIV = HPIV;
			pkm.AttackIV = AttackIV;
			pkm.DefenseIV = DefenseIV;
			pkm.SpeedIV = SpeedIV;
			pkm.SpAttackIV = SpAttackIV;
			pkm.SpDefenseIV = SpDefenseIV;

			// Status Info
			pkm.StatusCondition = StatusConditionFlags.None;
			pkm.TurnsOfSleepRemaining = 0;
			pkm.TurnsOfBadPoison = 0;
			pkm.PokerusStrain = PokerusStrain;
			pkm.PokerusDaysRemaining = PokerusDaysRemaining;
			pkm.PokerusRemaining = PokerusRemaining;

			// Contest Info
			pkm.Coolness = Coolness;
			pkm.Beauty = Beauty;
			pkm.Cuteness = Cuteness;
			pkm.Smartness = Smartness;
			pkm.Toughness = Toughness;
			pkm.Feel = Feel;

			pkm.CoolRibbonCount = CoolRibbonCount;
			pkm.BeautyRibbonCount = BeautyRibbonCount;
			pkm.CuteRibbonCount = CuteRibbonCount;
			pkm.SmartRibbonCount = SmartRibbonCount;
			pkm.ToughRibbonCount = ToughRibbonCount;
			pkm.HasChampionRibbon = HasChampionRibbon;
			pkm.HasWinningRibbon = HasWinningRibbon;
			pkm.HasVictoryRibbon = HasVictoryRibbon;
			pkm.HasArtistRibbon = HasArtistRibbon;
			pkm.HasEffortRibbon = HasEffortRibbon;
			pkm.HasMarineRibbon = HasMarineRibbon;
			pkm.HasLandRibbon = HasLandRibbon;
			pkm.HasSkyRibbon = HasSkyRibbon;
			pkm.HasCountryRibbon = HasCountryRibbon;
			pkm.HasNationalRibbon = HasNationalRibbon;
			pkm.HasEarthRibbon = HasEarthRibbon;
			pkm.HasWorldRibbon = HasWorldRibbon;

			// Move Info
			pkm.SetMoveAt(0, GetMoveAt(0));
			pkm.SetMoveAt(1, GetMoveAt(1));
			pkm.SetMoveAt(2, GetMoveAt(2));
			pkm.SetMoveAt(3, GetMoveAt(3));

			// Recalculate Stats to make sure they're accurate
			pkm.RecalculateStats();

			return pkm;
		}
		public XDPokemon CreateXDPokemon(GCRegions currentRegion, bool passFinder = true) {
			XDPokemon pkm = new XDPokemon();

			if (passFinder)
				pkm.PokemonFinder = PokemonFinder;
			pkm.GameType = GameTypes.XD;
			pkm.DeoxysForm = DeoxysForm;
			pkm.Language = Language;
			pkm.CurrentRegion = currentRegion;
			if (Language == Languages.Japanese)
				pkm.OriginalRegion = GCRegions.NTSC_J;
			else if (Language == Languages.English)
				pkm.OriginalRegion = GCRegions.NTSC_U;
			else
				pkm.OriginalRegion = GCRegions.PAL;

			// Pokemon Info
			pkm.Personality = Personality;
			pkm.SpeciesID = SpeciesID;
			pkm.IsSecondAbility2 = IsSecondAbility2;
			pkm.IsEgg = IsEgg;

			// Met Info
			pkm.TrainerName = TrainerName;
			pkm.TrainerGender = TrainerGender;
			pkm.TrainerID = TrainerID;
			pkm.SecretID = SecretID;
			pkm.BallCaughtID = BallCaughtID;
			pkm.LevelMet = LevelMet;
			pkm.MetLocationID = MetLocationID;
			pkm.IsFatefulEncounter = IsFatefulEncounter;
			pkm.GameOrigin = GameOrigin;

			// Personalization Info
			pkm.Nickname = Nickname;
			pkm.HeldItemID = HeldItemID;
			pkm.Markings = Markings;

			// Stats Info
			pkm.Experience = Experience;
			pkm.Friendship = Friendship;

			pkm.HPEV = HPEV;
			pkm.AttackEV = AttackEV;
			pkm.DefenseEV = DefenseEV;
			pkm.SpeedEV = SpeedEV;
			pkm.SpAttackEV = SpAttackEV;
			pkm.SpDefenseEV = SpDefenseEV;

			pkm.HPIV = HPIV;
			pkm.AttackIV = AttackIV;
			pkm.DefenseIV = DefenseIV;
			pkm.SpeedIV = SpeedIV;
			pkm.SpAttackIV = SpAttackIV;
			pkm.SpDefenseIV = SpDefenseIV;

			// Status Info
			pkm.StatusCondition = StatusConditionFlags.None;
			pkm.TurnsOfSleepRemaining = 0;
			pkm.TurnsOfBadPoison = 0;
			pkm.PokerusStrain = PokerusStrain;
			pkm.PokerusDaysRemaining = PokerusDaysRemaining;
			pkm.PokerusRemaining = PokerusRemaining;

			// Contest Info
			pkm.Coolness = Coolness;
			pkm.Beauty = Beauty;
			pkm.Cuteness = Cuteness;
			pkm.Smartness = Smartness;
			pkm.Toughness = Toughness;
			pkm.Feel = Feel;

			pkm.CoolRibbonCount = CoolRibbonCount;
			pkm.BeautyRibbonCount = BeautyRibbonCount;
			pkm.CuteRibbonCount = CuteRibbonCount;
			pkm.SmartRibbonCount = SmartRibbonCount;
			pkm.ToughRibbonCount = ToughRibbonCount;
			pkm.HasChampionRibbon = HasChampionRibbon;
			pkm.HasWinningRibbon = HasWinningRibbon;
			pkm.HasVictoryRibbon = HasVictoryRibbon;
			pkm.HasArtistRibbon = HasArtistRibbon;
			pkm.HasEffortRibbon = HasEffortRibbon;
			pkm.HasMarineRibbon = HasMarineRibbon;
			pkm.HasLandRibbon = HasLandRibbon;
			pkm.HasSkyRibbon = HasSkyRibbon;
			pkm.HasCountryRibbon = HasCountryRibbon;
			pkm.HasNationalRibbon = HasNationalRibbon;
			pkm.HasEarthRibbon = HasEarthRibbon;
			pkm.HasWorldRibbon = HasWorldRibbon;

			// Move Info
			pkm.SetMoveAt(0, GetMoveAt(0));
			pkm.SetMoveAt(1, GetMoveAt(1));
			pkm.SetMoveAt(2, GetMoveAt(2));
			pkm.SetMoveAt(3, GetMoveAt(3));

			// Recalculate Stats to make sure they're accurate
			pkm.RecalculateStats();

			return pkm;
		}
		public static BoxPokemon CreateInvalidPokemon(BoxPokemon invalidBackup, GameTypes gameType = GameTypes.Any) {
			BoxPokemon pkm = new BoxPokemon();
			pkm.invalidBackup = invalidBackup;

			pkm.GameType = gameType;

			// Pokemon Info
			pkm.Personality = 0;
			pkm.SpeciesID = 0;
			pkm.IsSecondAbility2 = false;

			// Met Info
			pkm.TrainerName = "INVALID";
			pkm.TrainerGender = Genders.Male;
			pkm.TrainerID = 0;
			pkm.SecretID = 0;
			pkm.BallCaughtID = 1;
			pkm.LevelMet = 0;
			pkm.MetLocationID = 0;
			pkm.IsFatefulEncounter = false;
			pkm.GameOrigin = GameOrigins.Unknown;
			pkm.Language = Languages.English;

			// Personalization Info
			pkm.Nickname = "INVALID";
			pkm.HeldItemID = 0;

			// Stats Info
			pkm.Experience = 0;
			pkm.Friendship = 0;

			pkm.HPEV = 0;
			pkm.AttackEV = 0;
			pkm.DefenseEV = 0;
			pkm.SpeedEV = 0;
			pkm.SpAttackEV = 0;
			pkm.SpDefenseEV = 0;

			pkm.HPIV = 0;
			pkm.AttackIV = 0;
			pkm.DefenseIV = 0;
			pkm.SpeedIV = 0;
			pkm.SpAttackIV = 0;
			pkm.SpDefenseIV = 0;

			// Status Info
			pkm.StatusCondition = StatusConditionFlags.None;
			pkm.TurnsOfSleepRemaining = 0;
			pkm.TurnsOfBadPoison = 0;
			pkm.PokerusStrain = 0;
			pkm.PokerusDaysRemaining = 0;
			pkm.PokerusRemaining = 0;

			// Contest Info
			pkm.Coolness = 0;
			pkm.Beauty = 0;
			pkm.Cuteness = 0;
			pkm.Smartness = 0;
			pkm.Toughness = 0;
			pkm.Feel = 0;

			pkm.CoolRibbonCount = 0;
			pkm.BeautyRibbonCount = 0;
			pkm.CuteRibbonCount = 0;
			pkm.SmartRibbonCount = 0;
			pkm.ToughRibbonCount = 0;
			pkm.HasChampionRibbon = false;
			pkm.HasWinningRibbon = false;
			pkm.HasVictoryRibbon = false;
			pkm.HasArtistRibbon = false;
			pkm.HasEffortRibbon = false;
			pkm.HasMarineRibbon = false;
			pkm.HasLandRibbon = false;
			pkm.HasSkyRibbon = false;
			pkm.HasCountryRibbon = false;
			pkm.HasNationalRibbon = false;
			pkm.HasEarthRibbon = false;
			pkm.HasWorldRibbon = false;

			// Move Info
			pkm.SetMoveAt(0, new Move(165, 0, 0)); // Struggle
			pkm.SetMoveAt(1, new Move());
			pkm.SetMoveAt(2, new Move());
			pkm.SetMoveAt(3, new Move());

			// Recalculate Stats to make sure they're accurate
			pkm.RecalculateStats();

			return pkm;
		}

		#endregion
	}
}
