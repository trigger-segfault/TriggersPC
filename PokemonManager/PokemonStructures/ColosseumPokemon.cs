using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public class ColosseumPokemon : IPokemon {

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

		public bool IsInDaycare {
			get { return pokeContainer.Type == ContainerTypes.Daycare; }
		}
		private PokemonFinder pokemonFinder;
		public PokemonFinder PokemonFinder {
			get { return pokemonFinder; }
			set {
				pokemonFinder = value;
				pokemonFinder.Pokemon = this;
			}
		}

		private IPokeContainer pokeContainer;
		private GameTypes gameType;
		private byte deoxysForm;
		private byte[] raw;

		#endregion

		public ColosseumPokemon() {
			this.raw = new byte[312];
			this.gameType = GameTypes.Any;
			this.deoxysForm = byte.MaxValue;
			this.pokemonFinder = new PokemonFinder(this);
		}

		public ColosseumPokemon(byte[] data) {
			this.raw = data;
			this.gameType = GameTypes.Any;
			this.deoxysForm = byte.MaxValue;
			this.pokemonFinder = new PokemonFinder(this);
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
			set { gameType = value; }
		}
		public bool IsValid {
			get {
				return  PokemonDatabase.GetPokemonFromID(SpeciesID) != null && SpeciesID != 0 &&
						PokemonDatabase.GetMoveFromID(Move1ID) != null &&
						(PokemonDatabase.GetMoveFromID(Move2ID) != null || Move2ID == 0) &&
						(PokemonDatabase.GetMoveFromID(Move3ID) != null || Move3ID == 0) &&
						(PokemonDatabase.GetMoveFromID(Move4ID) != null || Move4ID == 0) &&
						PokemonDatabase.GetBallCaughtImageFromID(BallCaughtID) != null &&
						ItemDatabase.GetItemFromID(HeldItemID) != null &&
						!IsInvalid;
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
			get { return BigEndian.ToUInt32(raw, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt32(value, raw, 4);
			}
		}
		public ushort SpeciesID {
			get { return BigEndian.ToUInt16(raw, 0); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 0);
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
			get { return raw[204] != 0; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[204] = (byte)(value ? 1 : 0);
			}
		}
		public bool IsEgg {
			get { return raw[203] != 0; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[203] = (byte)(value ? 1 : 0);
			}
		}
		public bool IsShiny {
			get {
				byte[] bytes = BitConverter.GetBytes(Personality);
				return ((uint)TrainerID ^ (uint)SecretID ^ (uint)BitConverter.ToUInt16(bytes, 0) ^ (uint)BitConverter.ToUInt16(bytes, 2)) < 8;
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

		public bool IsInvalid {
			get { return raw[205] != 0; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[205] = (byte)(value ? 1 : 0);
			}
		}

		#endregion

		#region Met Info

		public string TrainerName {
			get { return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(24, raw, 22), Language); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.ReplaceBytes(raw, 24, GCCharacterEncoding.GetBytes(value, 11, Language));
			}
		}
		public Genders TrainerGender {
			get { return (Genders)raw[16]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[16] = (byte)value;
			}
		}
		public ushort TrainerID {
			get { return BigEndian.ToUInt16(raw, 22); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 22);
			}
		}
		public ushort SecretID {
			get { return BigEndian.ToUInt16(raw, 20); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 20);
			}
		}
		public byte BallCaughtID {
			get { return raw[15]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[15] = value;
			}
		}
		public byte LevelMet {
			get { return raw[14]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[14] = value;
			}
		}
		public ushort MetLocationID {
			get { return BigEndian.ToUInt16(raw, 12); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 12);
			}
		}
		public bool IsFatefulEncounter {
			get { return IsObedient && EncounterType == GCEncounterTypes.FatefulEncounter; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				
				IsObedient = value;
				EncounterType = (value ? GCEncounterTypes.FatefulEncounter : GCEncounterTypes.Normal);
			}
		}
		public Languages Language {
			get {
				GCLanguages gcLanguage = (GCLanguages)raw[11];
				switch (gcLanguage) {
				case GCLanguages.Japanese: return Languages.Japanese;
				case GCLanguages.English: return Languages.English;
				case GCLanguages.German: return Languages.German;
				case GCLanguages.French: return Languages.French;
				case GCLanguages.Italian: return Languages.Italian;
				case GCLanguages.Spanish: return Languages.Spanish;
				}
				return Languages.NoLanguage;
			}
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				
				GCLanguages gcLanguage = GCLanguages.NoLanguage;
				switch (value) {
				case Languages.Japanese: gcLanguage = GCLanguages.Japanese; break;
				case Languages.English: gcLanguage = GCLanguages.English; break;
				case Languages.German: gcLanguage = GCLanguages.German; break;
				case Languages.French: gcLanguage = GCLanguages.French; break;
				case Languages.Italian: gcLanguage = GCLanguages.Italian; break;
				case Languages.Spanish: gcLanguage = GCLanguages.Spanish; break;
				}
				raw[11] = (byte)gcLanguage;
			}
		}
		public GameOrigins GameOrigin {
			get {
				GCGameOrigins gcOrigin = (GCGameOrigins)raw[8];
				switch (gcOrigin) {
				case GCGameOrigins.ColosseumBonusDisc: return GameOrigins.ColosseumBonusDisc;
				case GCGameOrigins.ColosseumXD: return GameOrigins.ColosseumXD;
				case GCGameOrigins.Ruby: return GameOrigins.Ruby;
				case GCGameOrigins.Sapphire: return GameOrigins.Sapphire;
				case GCGameOrigins.Emerald: return GameOrigins.Emerald;
				case GCGameOrigins.FireRed: return GameOrigins.FireRed;
				case GCGameOrigins.LeafGreen: return GameOrigins.LeafGreen;
				default: return GameOrigins.Unknown;
				}
			}
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				switch (value) {
				case GameOrigins.ColosseumBonusDisc: raw[8] = (byte)GCGameOrigins.ColosseumBonusDisc; break;
				case GameOrigins.ColosseumXD: raw[8] = (byte)GCGameOrigins.ColosseumXD; break;
				case GameOrigins.Ruby: raw[8] = (byte)GCGameOrigins.Ruby; break;
				case GameOrigins.Sapphire: raw[8] = (byte)GCGameOrigins.Sapphire; break;
				case GameOrigins.Emerald: raw[8] = (byte)GCGameOrigins.Emerald; break;
				case GameOrigins.FireRed: raw[8] = (byte)GCGameOrigins.FireRed; break;
				case GameOrigins.LeafGreen: raw[8] = (byte)GCGameOrigins.LeafGreen; break;
				default: raw[8] = 16; break;
				}
			}
		}

		public GCLanguages GCLanguage {
			get { return (GCLanguages)raw[11]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[11] = (byte)value;
			}
		}
		public GCRegions CurrentRegion {
			get { return (GCRegions)raw[9]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[9] = (byte)value;
			}
		}
		public GCRegions OriginalRegion {
			get { return (GCRegions)raw[10]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[10] = (byte)value;
			}
		}

		#endregion

		#region Personalization Info

		public string Nickname {
			get { return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(46, raw, 22), Language); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value == "")
					value = PokemonData.Name.ToUpper();
				ByteHelper.ReplaceBytes(raw, 46, GCCharacterEncoding.GetBytes(value, 11, Language));
			}
		}
		public bool HasNickname {
			get { return Nickname != PokemonData.Name.ToUpper(); }
		}
		public ushort HeldItemID {
			get { return BigEndian.ToUInt16(raw, 136); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 136);
			}
		}
		public bool IsHoldingItem {
			get { return HeldItemID != 0; }
		}
		public MarkingFlags Markings {
			get { return (MarkingFlags)raw[207]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[207] = (byte)value;
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
			get { return raw[96]; }
			set { raw[96] = value; }
		}
		public uint Experience {
			get { return BigEndian.ToUInt32(raw, 92); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt32(value, raw, 92);
			}
		}
		public byte Friendship {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 176)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 176);
			}
		}

		public ushort HP {
			get { return BigEndian.ToUInt16(raw, 140); }
			set { BigEndian.WriteUInt16(value, raw, 140); }
		}
		public ushort Attack {
			get { return BigEndian.ToUInt16(raw, 142); }
			set { BigEndian.WriteUInt16(value, raw, 142); }
		}
		public ushort Defense {
			get { return BigEndian.ToUInt16(raw, 144); }
			set { BigEndian.WriteUInt16(value, raw, 144); }
		}
		public ushort SpAttack {
			get { return BigEndian.ToUInt16(raw, 146); }
			set { BigEndian.WriteUInt16(value, raw, 146); }
		}
		public ushort SpDefense {
			get { return BigEndian.ToUInt16(raw, 148); }
			set { BigEndian.WriteUInt16(value, raw, 148); }
		}
		public ushort Speed {
			get { return BigEndian.ToUInt16(raw, 150); }
			set { BigEndian.WriteUInt16(value, raw, 150); }
		}

		public byte HPEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 152)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 152);
			}
		}
		public byte AttackEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 154)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 154);
			}
		}
		public byte DefenseEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 156)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 156);
			}
		}
		public byte SpAttackEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 158)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 158);
			}
		}
		public byte SpDefenseEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 160)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 160);
			}
		}
		public byte SpeedEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 162)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 162);
			}
		}

		public byte HPIV {
			get { return (byte)Math.Min((ushort)31, BigEndian.ToUInt16(raw, 164)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 164);
			}
		}
		public byte AttackIV {
			get { return (byte)Math.Min((ushort)31, BigEndian.ToUInt16(raw, 166)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 166);
			}
		}
		public byte DefenseIV {
			get { return (byte)Math.Min((ushort)31, BigEndian.ToUInt16(raw, 168)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 168);
			}
		}
		public byte SpAttackIV {
			get { return (byte)Math.Min((ushort)31, BigEndian.ToUInt16(raw, 170)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 170);
			}
		}
		public byte SpDefenseIV {
			get { return (byte)Math.Min((ushort)31, BigEndian.ToUInt16(raw, 172)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 172);
			}
		}
		public byte SpeedIV {
			get { return (byte)Math.Min((ushort)31, BigEndian.ToUInt16(raw, 174)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 174);
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
			get { return BigEndian.ToUInt16(raw, 120); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 120);
			}
		}
		public ushort Move2ID {
			get { return BigEndian.ToUInt16(raw, 124); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 124);
			}
		}
		public ushort Move3ID {
			get { return BigEndian.ToUInt16(raw, 128); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 128);
			}
		}
		public ushort Move4ID {
			get { return BigEndian.ToUInt16(raw, 132); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 132);
			}
		}
		public byte Move1PP {
			get { return raw[122]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[122] = value;
			}
		}
		public byte Move2PP {
			get { return raw[126]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[126] = value;
			}
		}
		public byte Move3PP {
			get { return raw[130]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[130] = value;
			}
		}
		public byte Move4PP {
			get { return raw[134]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[134] = value;
			}
		}
		public byte Move1PPUpsUsed {
			get { return raw[123]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[123] = value;
			}
		}
		public byte Move2PPUpsUsed {
			get { return raw[127]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[127] = value;
			}
		}
		public byte Move3PPUpsUsed {
			get { return raw[131]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[131] = value;
			}
		}
		public byte Move4PPUpsUsed {
			get { return raw[135]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[135] = value;
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

		#endregion

		#region Contest Info

		public byte Coolness {
			get { return raw[178]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[178] = value;
			}
		}
		public byte Beauty {
			get { return raw[179]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[179] = value;
			}
		}
		public byte Cuteness {
			get { return raw[180]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[180] = value;
			}
		}
		public byte Smartness {
			get { return raw[181]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[181] = value;
			}
		}
		public byte Toughness {
			get { return raw[182]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[182] = value;
			}
		}
		public byte Feel {
			get { return raw[188]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[188] = value;
			}
		}

		public byte CoolRibbonCount {
			get { return Math.Min((byte)4, raw[183]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[183] = Math.Min((byte)4, value);
			}
		}
		public byte BeautyRibbonCount {
			get { return Math.Min((byte)4, raw[184]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[184] = Math.Min((byte)4, value);
			}
		}
		public byte CuteRibbonCount {
			get { return Math.Min((byte)4, raw[185]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[185] = Math.Min((byte)4, value);
			}
		}
		public byte SmartRibbonCount {
			get { return Math.Min((byte)4, raw[186]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[186] = Math.Min((byte)4, value);
			}
		}
		public byte ToughRibbonCount {
			get { return Math.Min((byte)4, raw[187]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[187] = Math.Min((byte)4, value);
			}
		}
		public bool HasChampionRibbon {
			get { return BigEndian.ToBool(raw, 189); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 189);
			}
		}
		public bool HasWinningRibbon {
			get { return BigEndian.ToBool(raw, 190); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 190);
			}
		}
		public bool HasVictoryRibbon {
			get { return BigEndian.ToBool(raw, 191); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 191);
			}
		}
		public bool HasArtistRibbon {
			get { return BigEndian.ToBool(raw, 192); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 192);
			}
		}
		public bool HasEffortRibbon {
			get { return BigEndian.ToBool(raw, 193); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 193);
			}
		}
		public bool HasMarineRibbon {
			get { return BigEndian.ToBool(raw, 194); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 194);
			}
		}
		public bool HasLandRibbon {
			get { return BigEndian.ToBool(raw, 195); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 195);
			}
		}
		public bool HasSkyRibbon {
			get { return BigEndian.ToBool(raw, 196); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 196);
			}
		}
		public bool HasCountryRibbon {
			get { return BigEndian.ToBool(raw, 197); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 197);
			}
		}
		public bool HasNationalRibbon {
			get { return BigEndian.ToBool(raw, 198); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 198);
			}
		}
		public bool HasEarthRibbon {
			get { return BigEndian.ToBool(raw, 199); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 199);
			}
		}
		public bool HasWorldRibbon {
			get { return BigEndian.ToBool(raw, 200); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteBool(value, raw, 200);
			}
		}

		#endregion

		#region Status Info

		public ushort CurrentHP {
			get { return BigEndian.ToUInt16(raw, 138); }
			set { BigEndian.WriteUInt16(value, raw, 138); }
		}
		public StatusConditionFlags StatusCondition {
			get {
				GCStatusConditions gcStatus = (GCStatusConditions)BigEndian.ToUInt16(raw, 101);
				switch (gcStatus) {
				case GCStatusConditions.Poisoned: return StatusConditionFlags.Poisoned;
				case GCStatusConditions.BadlyPoisoned: return StatusConditionFlags.BadlyPoisoned;
				case GCStatusConditions.Paralyzed: return StatusConditionFlags.Paralyzed;
				case GCStatusConditions.Burned: return StatusConditionFlags.Burned;
				case GCStatusConditions.Frozen: return StatusConditionFlags.Frozen;
				case GCStatusConditions.Asleep: return (StatusConditionFlags)(Math.Min((byte)7, TurnsOfSleepRemaining));
				}
				return StatusConditionFlags.None;
			}
			set {
				GCStatusConditions gcStatus = GCStatusConditions.None;
				if (value.HasFlag(StatusConditionFlags.Poisoned))
					gcStatus = GCStatusConditions.Poisoned;
				if (value.HasFlag(StatusConditionFlags.BadlyPoisoned))
					gcStatus = GCStatusConditions.BadlyPoisoned;
				if (value.HasFlag(StatusConditionFlags.Paralyzed))
					gcStatus = GCStatusConditions.Paralyzed;
				if (value.HasFlag(StatusConditionFlags.Burned))
					gcStatus = GCStatusConditions.Burned;
				if (value.HasFlag(StatusConditionFlags.Frozen))
					gcStatus = GCStatusConditions.Frozen;
				if (((byte)value & 0x7) != 0)
					gcStatus = GCStatusConditions.Asleep;

				BigEndian.WriteUInt16((ushort)gcStatus, raw, 101);
			}
		}
		public byte TurnsOfSleepRemaining {
			get { return raw[105]; }
			set { raw[105] = value; }
		}
		public byte TurnsOfBadPoison {
			get { return raw[107]; }
			set { raw[107] = value; }
		}
		public PokerusStatuses PokerusStatus {
			get {
				byte x = ByteHelper.BitsToByte(raw, 202, 4, 4);
				byte y = ByteHelper.BitsToByte(raw, 202, 0, 4);
				if (x != 0)
					return (y != 0 ? PokerusStatuses.Infected : PokerusStatuses.Cured);
				else
					return (y != 0 ? PokerusStatuses.Invalid : PokerusStatuses.None);
			}
		}
		public PokerusStrainTypes PokerusStrainType {
			get { return (PokerusStrain != 0 ? (PokerusStrainTypes)ByteHelper.BitsToByte(raw, 202, 4, 2) : PokerusStrainTypes.None); }
		}
		public PokerusStrainVariations PokerusStrainVariation {
			get { return (PokerusStrain != 0 ? (PokerusStrainVariations)ByteHelper.BitsToByte(raw, 202, 6, 2) : PokerusStrainVariations.None); }
		}
		public byte PokerusStrain {
			get { return ByteHelper.BitsToByte(raw, 202, 4, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, 202, 4, 4, value);
			}
		}
		public byte PokerusDaysRemaining {
			get { return ByteHelper.BitsToByte(raw, 202, 0, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, 202, 0, 4, value);
			}
		}
		public byte PokerusRemaining {
			get { return raw[208]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[208] = value;
			}
		}

		public GCStatusConditions GCStatusCondition {
			get { return (GCStatusConditions)BigEndian.ToUInt16(raw, 101); }
			set { BigEndian.WriteUInt16((ushort)value, raw, 101); }
		}

		#endregion

		#region Shadow Pokemon Info

		public ushort ShadowPokemonID {
			get { return BigEndian.ToUInt16(raw, 216); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 216);
			}
		}
		public bool IsShadowPokemon {
			get { return ShadowPokemonID != 0 && Purification >= 0; }
		}
		public bool IsPurifiedShadowPokemon {
			get { return ShadowPokemonID != 0 && Purification < 0; }
		}
		public int Purification {
			get { return BigEndian.ToSInt32(raw, 220); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteSInt32(value, raw, 220);
			}
		}
		public uint HeartGauge {
			get { return PokemonDatabase.GetHeartGaugeFromID(ShadowPokemonID, GameTypes.Colosseum); }
		}
		public uint ExperienceStored {
			get { return BigEndian.ToUInt32(raw, 224); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt32(value, raw, 224);
			}
		}

		#endregion

		#region Unique Info

		public bool IsObedient {
			get { return BigEndian.ToBool(raw, 284); }
			set { BigEndian.WriteBool(value, raw, 284); }
		}
		public GCEncounterTypes EncounterType {
			get { return (GCEncounterTypes)raw[251]; }
			set { raw[251] = (byte)value; }
		}

		public ushort Unknown0x02 {
			get { return BigEndian.ToUInt16(raw, 0x02); }
		}
		public byte Unknown0x11 {
			get { return raw[0x11]; }
		}
		public byte Unknown0x12 {
			get { return raw[0x12]; }
		}
		public byte Unknown0x13 {
			get { return raw[0x13]; }
		}
		public ushort Unknown0x61 {
			get { return BigEndian.ToUInt16(raw, 0x61); }
		}
		public ushort Unknown0x63 {
			get { return BigEndian.ToUInt16(raw, 0x63); }
		}
		public ushort Unknown0xDA {
			get { return BigEndian.ToUInt16(raw, 0xDA); }
		}
		public byte Unknown0xD1 {
			get { return raw[0xD1]; }
		}
		public ushort Unknown0xD6 {
			get { return BigEndian.ToUInt16(raw, 0xD6); }
		}

		public ushort Unknown0xE4 {
			get { return BigEndian.ToUInt16(raw, 228); }
			set { BigEndian.WriteUInt16(value, raw, 228); }
		}
		public ushort Unknown0xE6 {
			get { return BigEndian.ToUInt16(raw, 230); }
			set { BigEndian.WriteUInt16(value, raw, 230); }
		}
		public byte Unknown0xCE {
			get { return raw[206]; }
		}
		public ushort Unknown0xD2 {
			get { return BigEndian.ToUInt16(raw, 210); }
			set { BigEndian.WriteUInt16(value, raw, 210); }
		}
		public ushort Unknown0xD4 {
			get { return BigEndian.ToUInt16(raw, 212); }
			set { BigEndian.WriteUInt16(value, raw, 212); }
		}

		public uint[] UnknownEnd {
			get {
				uint[] unknownEnd = new uint[15];
				for (int i = 0; i < 15; i++)
					unknownEnd[i] = BigEndian.ToUInt32(raw, 0xFC + i * 4);
				return unknownEnd;
			}
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

		public void RecalculateStats() {
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

		public byte[] GetFinalData() {
			return raw;
		}


		#region Converting

		public IPokemon Clone() {
			return CreateColosseumPokemon(CurrentRegion, false);
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
				pkm.SendingTrainerID = pokeContainer.GameSave.TrainerID;
				pkm.SendingSecretID = pokeContainer.GameSave.SecretID;
			}

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
			pkm.CurrentRegion = currentRegion;
			pkm.OriginalRegion = OriginalRegion;

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
			pkm.EncounterType = EncounterType;
			pkm.IsObedient = IsObedient;
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
			pkm.OriginalRegion = OriginalRegion;

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
			pkm.EncounterType = EncounterType;
			pkm.IsObedient = IsObedient;
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
		public static ColosseumPokemon CreateInvalidPokemon(GameTypes gameType = GameTypes.Any) {
			ColosseumPokemon pkm = new ColosseumPokemon();

			pkm.GameType = gameType;
			pkm.IsInvalid = true;

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
			pkm.EncounterType = GCEncounterTypes.None;
			pkm.IsObedient = false;
			pkm.GameOrigin = GameOrigins.Unknown;
			pkm.Language = Languages.English;

			// Personalization Info
			pkm.Nickname = "INVALID";
			pkm.HeldItemID = 0;

			// Stats Info
			pkm.Experience = 1;
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
