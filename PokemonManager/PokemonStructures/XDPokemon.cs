using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
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
	public class XDPokemon : IPokemon {

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

		private XDPokemon invalidBackup;

		#endregion

		public XDPokemon() {
			this.raw = new byte[196];
			this.gameType = GameTypes.Any;
			this.deoxysForm = byte.MaxValue;
			this.pokemonFinder = new PokemonFinder(this);
		}

		public XDPokemon(byte[] data) {
			this.raw = data;
			this.gameType = GameTypes.Any;
			this.deoxysForm = byte.MaxValue;
			this.pokemonFinder = new PokemonFinder(this);
		}

		public void RestoreHealth() {
			CurrentHP = HP;
			StatusCondition = StatusConditionFlags.None;
			for (int i = 0; i < NumMoves; i++) {
				SetMovePPAt(i, GetMoveTotalPPAt(i));
			}
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
				return PokemonDatabase.GetPokemonFromID(SpeciesID) != null && SpeciesID != 0 &&
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
				if (IsShadowPokemon && DexID == 249) // Shadow Lugia
					return ResourceDatabase.GetImageFromName("XD001FrontSprite");
				if (IsEgg)
					return PokemonDatabase.GetPokemonImageFromDexID(387, false);
				if (DexID == 327)
					return PokemonDatabase.GetSpindaSprite(Personality, IsShiny);
				return PokemonDatabase.GetPokemonImageFromDexID(DexID, IsShiny, FormID);
			}
		}
		public BitmapImage BoxSprite {
			get {
				if (IsShadowPokemon && DexID == 249) // Shadow Lugia
					return ResourceDatabase.GetImageFromName("XD001BoxSprite");
				if (IsEgg)
					return PokemonDatabase.GetPokemonBoxImageFromDexID(387, IsShiny);
				return PokemonDatabase.GetPokemonBoxImageFromDexID(DexID, IsShiny, FormID);
			}
		}

		#endregion

		#region Pokemon Info

		public uint Personality {
			get { return BigEndian.ToUInt32(raw, 40); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt32(value, raw, 40);
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
			get { return ByteHelper.GetBit(raw, 29, 6); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, 29, 6, value);
			}
		}
		public bool IsEgg {
			get { return ByteHelper.GetBit(raw, 29, 7); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, 29, 7, value);
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
			get { return ByteHelper.GetBit(raw, 29, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBit(raw, 29, 5, value);
			}
		}

		#endregion

		#region Met Info

		public string TrainerName {
			get { return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(56, raw, 22), Language); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.ReplaceBytes(raw, 56, GCCharacterEncoding.GetBytes(value, 11, Language));
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
			get { return BigEndian.ToUInt16(raw, 38); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 38);
			}
		}
		public ushort SecretID {
			get { return BigEndian.ToUInt16(raw, 36); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 36);
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
			get { return BigEndian.ToUInt16(raw, 8); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 8);
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
				GCLanguages gcLanguage = (GCLanguages)raw[55];
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
				raw[55] = (byte)gcLanguage;
			}
		}
		public GameOrigins GameOrigin {
			get {
				GCGameOrigins gcOrigin = (GCGameOrigins)raw[52];
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
				case GameOrigins.ColosseumBonusDisc: raw[52] = (byte)GCGameOrigins.ColosseumBonusDisc; break;
				case GameOrigins.ColosseumXD: raw[52] = (byte)GCGameOrigins.ColosseumXD; break;
				case GameOrigins.Ruby: raw[52] = (byte)GCGameOrigins.Ruby; break;
				case GameOrigins.Sapphire: raw[52] = (byte)GCGameOrigins.Sapphire; break;
				case GameOrigins.Emerald: raw[52] = (byte)GCGameOrigins.Emerald; break;
				case GameOrigins.FireRed: raw[52] = (byte)GCGameOrigins.FireRed; break;
				case GameOrigins.LeafGreen: raw[52] = (byte)GCGameOrigins.LeafGreen; break;
				default: raw[52] = 16; break;
				}
			}
		}

		public GCLanguages GCLanguage {
			get { return (GCLanguages)raw[55]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[55] = (byte)value;
			}
		}
		public GCRegions CurrentRegion {
			get { return (GCRegions)raw[53]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[53] = (byte)value;
			}
		}
		public GCRegions OriginalRegion {
			get { return (GCRegions)raw[54]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[54] = (byte)value;
			}
		}

		#endregion

		#region Personalization Info

		public string Nickname {
			get { return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(78, raw, 22), Language); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (value == "")
					value = PokemonData.Name.ToUpper();
				ByteHelper.ReplaceBytes(raw, 78, GCCharacterEncoding.GetBytes(value, 11, Language));
			}
		}
		public bool HasNickname {
			get { return Nickname != PokemonData.Name.ToUpper(); }
		}
		public ushort HeldItemID {
			get { return BigEndian.ToUInt16(raw, 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 2);
			}
		}
		public bool IsHoldingItem {
			get { return HeldItemID != 0; }
		}
		public MarkingFlags Markings {
			get { return (MarkingFlags)raw[20]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[20] = (byte)value;
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
			get { return raw[17]; }
			set { raw[17] = value; }
		}
		public uint Experience {
			get { return BigEndian.ToUInt32(raw, 32); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt32(value, raw, 32);
			}
		}
		public byte Friendship {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 6)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 6);
			}
		}

		public ushort HP {
			get { return BigEndian.ToUInt16(raw, 144); }
			set { BigEndian.WriteUInt16(value, raw, 144); }
		}
		public ushort Attack {
			get { return BigEndian.ToUInt16(raw, 146); }
			set { BigEndian.WriteUInt16(value, raw, 146); }
		}
		public ushort Defense {
			get { return BigEndian.ToUInt16(raw, 148); }
			set { BigEndian.WriteUInt16(value, raw, 148); }
		}
		public ushort SpAttack {
			get { return BigEndian.ToUInt16(raw, 150); }
			set { BigEndian.WriteUInt16(value, raw, 150); }
		}
		public ushort SpDefense {
			get { return BigEndian.ToUInt16(raw, 152); }
			set { BigEndian.WriteUInt16(value, raw, 152); }
		}
		public ushort Speed {
			get { return BigEndian.ToUInt16(raw, 154); }
			set { BigEndian.WriteUInt16(value, raw, 154); }
		}

		public byte HPEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 156)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 156);
			}
		}
		public byte AttackEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 158)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 158);
			}
		}
		public byte DefenseEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 160)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 160);
			}
		}
		public byte SpAttackEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 162)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 162);
			}
		}
		public byte SpDefenseEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 164)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 164);
			}
		}
		public byte SpeedEV {
			get { return (byte)Math.Min(byte.MaxValue, BigEndian.ToUInt16(raw, 166)); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 166);
			}
		}

		public byte HPIV {
			get { return (byte)Math.Min((ushort)31, raw[168]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[168] = value;
			}
		}
		public byte AttackIV {
			get { return (byte)Math.Min((ushort)31, raw[169]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[169] = value;
			}
		}
		public byte DefenseIV {
			get { return (byte)Math.Min((ushort)31, raw[170]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[170] = value;
			}
		}
		public byte SpAttackIV {
			get { return (byte)Math.Min((ushort)31, raw[171]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[171] = value;
			}
		}
		public byte SpDefenseIV {
			get { return (byte)Math.Min((ushort)31, raw[172]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[172] = value;
			}
		}
		public byte SpeedIV {
			get { return (byte)Math.Min((ushort)31, raw[173]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[173] = value;
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
			get { return BigEndian.ToUInt16(raw, 128); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 128);
			}
		}
		public ushort Move2ID {
			get { return BigEndian.ToUInt16(raw, 132); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 132);
			}
		}
		public ushort Move3ID {
			get { return BigEndian.ToUInt16(raw, 136); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 136);
			}
		}
		public ushort Move4ID {
			get { return BigEndian.ToUInt16(raw, 140); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 140);
			}
		}
		public byte Move1PP {
			get { return raw[130]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[130] = value;
			}
		}
		public byte Move2PP {
			get { return raw[134]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[134] = value;
			}
		}
		public byte Move3PP {
			get { return raw[138]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[138] = value;
			}
		}
		public byte Move4PP {
			get { return raw[142]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[142] = value;
			}
		}
		public byte Move1PPUpsUsed {
			get { return raw[131]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[131] = value;
			}
		}
		public byte Move2PPUpsUsed {
			get { return raw[135]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[135] = value;
			}
		}
		public byte Move3PPUpsUsed {
			get { return raw[139]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[139] = value;
			}
		}
		public byte Move4PPUpsUsed {
			get { return raw[143]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[143] = value;
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
			get { return raw[174]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[174] = value;
			}
		}
		public byte Beauty {
			get { return raw[175]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[175] = value;
			}
		}
		public byte Cuteness {
			get { return raw[176]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[176] = value;
			}
		}
		public byte Smartness {
			get { return raw[177]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[177] = value;
			}
		}
		public byte Toughness {
			get { return raw[178]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[178] = value;
			}
		}
		public byte Feel {
			get { return raw[18]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[18] = value;
			}
		}

		public byte CoolRibbonCount {
			get { return Math.Min((byte)4, raw[179]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[179] = Math.Min((byte)4, value);
			}
		}
		public byte BeautyRibbonCount {
			get { return Math.Min((byte)4, raw[180]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[180] = Math.Min((byte)4, value);
			}
		}
		public byte CuteRibbonCount {
			get { return Math.Min((byte)4, raw[181]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[181] = Math.Min((byte)4, value);
			}
		}
		public byte SmartRibbonCount {
			get { return Math.Min((byte)4, raw[182]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[182] = Math.Min((byte)4, value);
			}
		}
		public byte ToughRibbonCount {
			get { return Math.Min((byte)4, raw[183]); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[183] = Math.Min((byte)4, value);
			}
		}
		public bool HasChampionRibbon {
			get { return BigEndian.GetBit(raw, 124, 0); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 0, value);
			}
		}
		public bool HasWinningRibbon {
			get { return BigEndian.GetBit(raw, 124, 1); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 1, value);
			}
		}
		public bool HasVictoryRibbon {
			get { return BigEndian.GetBit(raw, 124, 2); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 2, value);
			}
		}
		public bool HasArtistRibbon {
			get { return BigEndian.GetBit(raw, 124, 3); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 3, value);
			}
		}
		public bool HasEffortRibbon {
			get { return BigEndian.GetBit(raw, 124, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 4, value);
			}
		}
		public bool HasMarineRibbon {
			get { return BigEndian.GetBit(raw, 124, 5); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 5, value);
			}
		}
		public bool HasLandRibbon {
			get { return BigEndian.GetBit(raw, 124, 6); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 6, value);
			}
		}
		public bool HasSkyRibbon {
			get { return BigEndian.GetBit(raw, 124, 7); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 7, value);
			}
		}
		public bool HasCountryRibbon {
			get { return BigEndian.GetBit(raw, 124, 8); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 8, value);
			}
		}
		public bool HasNationalRibbon {
			get { return BigEndian.GetBit(raw, 124, 9); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 9, value);
			}
		}
		public bool HasEarthRibbon {
			get { return BigEndian.GetBit(raw, 124, 10); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 10, value);
			}
		}
		public bool HasWorldRibbon {
			get { return BigEndian.GetBit(raw, 124, 11); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.SetBit(raw, 124, 11, value);
			}
		}

		#endregion

		#region Status Info

		public ushort CurrentHP {
			get { return BigEndian.ToUInt16(raw, 4); }
			set { BigEndian.WriteUInt16(value, raw, 4); }
		}
		public StatusConditionFlags StatusCondition {
			get {
				// TODO: See why this is labeled as a ushort when the values take up both bytes between it.
				GCStatusConditions gcStatus = (GCStatusConditions)raw[22];// BigEndian.ToUInt16(raw, 22);
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

				raw[22] = (byte)gcStatus;
				//BigEndian.WriteUInt16((ushort)gcStatus, raw, 22);
			}
		}
		public byte TurnsOfSleepRemaining {
			get { return raw[24]; }
			set { raw[24] = value; }
		}
		public byte TurnsOfBadPoison {
			get { return raw[23]; }
			set { raw[23] = value; }
		}
		public PokerusStatuses PokerusStatus {
			get {
				byte x = ByteHelper.BitsToByte(raw, 19, 4, 4);
				byte y = ByteHelper.BitsToByte(raw, 19, 0, 4);
				if (x != 0)
					return (y != 0 ? PokerusStatuses.Infected : PokerusStatuses.Cured);
				else
					return (y != 0 ? PokerusStatuses.Invalid : PokerusStatuses.None);
			}
		}
		public PokerusStrainTypes PokerusStrainType {
			get { return (PokerusStrain != 0 ? (PokerusStrainTypes)ByteHelper.BitsToByte(raw, 19, 4, 2) : PokerusStrainTypes.None); }
		}
		public PokerusStrainVariations PokerusStrainVariation {
			get { return (PokerusStrain != 0 ? (PokerusStrainVariations)ByteHelper.BitsToByte(raw, 19, 6, 2) : PokerusStrainVariations.None); }
		}
		public byte PokerusStrain {
			get { return ByteHelper.BitsToByte(raw, 19, 4, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, 19, 4, 4, value);
			}
		}
		public byte PokerusDaysRemaining {
			get { return ByteHelper.BitsToByte(raw, 19, 0, 4); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				ByteHelper.SetBits(raw, 19, 0, 4, value);
			}
		}
		public byte PokerusRemaining {
			get { return raw[21]; }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				raw[21] = value;
			}
		}

		public GCStatusConditions GCStatusCondition {
			get { return (GCStatusConditions)BigEndian.ToUInt16(raw, 22); }
			set { BigEndian.WriteUInt16((ushort)value, raw, 22); }
		}

		#endregion

		#region Shadow Pokemon Info

		public ushort ShadowPokemonID {
			get { return BigEndian.ToUInt16(raw, 186); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				BigEndian.WriteUInt16(value, raw, 186);
			}
		}
		public bool IsShadowPokemon {
			//get { return ShadowPokemonID != 0 && Purification > 0; }
			get { return ShadowPokemonID != 0 && (((GCGameSave)GameSave).HasShadowInfo(Personality) ? !((GCGameSave)GameSave).GetShadowInfo(Personality).IsPurified : false); }
		}
		public bool IsPurifiedShadowPokemon {
			get { return ShadowPokemonID != 0 && Purification <= 0; }
		}
		public int Purification {
			get { return (((GCGameSave)GameSave).HasShadowInfo(Personality) ? ((GCGameSave)GameSave).GetShadowInfo(Personality).Purification : 0); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (((GCGameSave)GameSave).HasShadowInfo(Personality))
					((GCGameSave)GameSave).GetShadowInfo(Personality).Purification = value;
			}
		}
		public uint HeartGauge {
			get { return (ShadowPokemonID > 0 && ShadowPokemonID <= 83 ? PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).HeartGauge : 0); }
		}
		public uint ExperienceStored {
			get { return (((GCGameSave)GameSave).HasShadowInfo(Personality) ? ((GCGameSave)GameSave).GetShadowInfo(Personality).ExperienceStored : 0); }
			set {
				if (GameSave != null)
					GameSave.IsChanged = true;
				if (((GCGameSave)GameSave).HasShadowInfo(Personality))
					((GCGameSave)GameSave).GetShadowInfo(Personality).ExperienceStored = value;
			}
		}

		public ushort ShadowMove1ID {
			get {
				if (IsShadowPokemon) {
					if (PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID) != null && PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs.Length >= 1)
						return PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs[0];
				}
				return 0;
			}
		}
		public ushort ShadowMove2ID {
			get {
				if (IsShadowPokemon) {
					if (PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID) != null && PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs.Length >= 2)
						return PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs[1];
				}
				return 0;
			}
		}
		public ushort ShadowMove3ID {
			get {
				if (IsShadowPokemon) {
					if (PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID) != null && PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs.Length >= 3)
						return PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs[2];
				}
				return 0;
			}
		}
		public ushort ShadowMove4ID {
			get {
				if (IsShadowPokemon) {
					if (PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID) != null && PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs.Length == 4)
						return PokemonDatabase.GetShadowPokemonDataFromID(ShadowPokemonID).ShadowMovesIDs[3];
				}
				return 0;
			}
		}

		#endregion

		#region Unique Info

		public bool IsObedient {
			get { return BigEndian.ToBool(raw, 48); }
			set { BigEndian.WriteBool(value, raw, 48); }
		}
		public GCEncounterTypes EncounterType {
			get { return (GCEncounterTypes)raw[51]; }
			set { raw[51] = (byte)value; }
		}
		public bool IsNotTradableInGame {
			get { return ByteHelper.GetBit(raw, 29, 4); }
			set { ByteHelper.SetBit(raw, 29, 4, value); }
		}
		public bool IsCaught {
			get { return ByteHelper.GetBit(raw, 29, 2); }
			set { ByteHelper.SetBit(raw, 29, 2, value); }
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
			if (invalidBackup != null)
				return invalidBackup.raw;
			return raw;
		}

		#region Converting

		public IPokemon Clone() {
			return CreateXDPokemon(CurrentRegion, false);
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
		public static XDPokemon CreateInvalidPokemon(XDPokemon invalidBackup, GameTypes gameType = GameTypes.Any) {
			XDPokemon pkm = new XDPokemon();
			pkm.invalidBackup = invalidBackup;

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
