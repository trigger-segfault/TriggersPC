using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public abstract class PokemonNew {

		#region Members

		protected byte[] raw;

		protected IPokeContainer container;
		protected GameTypes gameType;
		protected byte deoxysForm;

		#endregion

		protected PokemonNew() {
			this.deoxysForm = byte.MaxValue;
			this.gameType = GameTypes.Any;
			this.container = null;
			this.raw = null;
		}

		#region Basic

		public byte[] Raw {
			get { return raw; }
		}
		public IPokeContainer PokeContainer {
			get { return container; }
			set { container = value; }
		}
		public IPokePC PokePC {
			get { return (container != null ? container.PokePC : null); }
		}
		public int ContainerIndex {
			get { return container.IndexOf(this); }
		}
		public IGameSave GameSave {
			get { return (container != null ? container.GameSave : null); }
		}
		public GameTypes GameType {
			get { return (container != null ? container.GameType : GameTypes.Unknown); }
		}
		bool IsValid { get; }

		#endregion

		#region Lookup Data Structures

		public PokemonData PokemonData {
			get { return PokemonDatabase.GetPokemonFromID(this.SpeciesID); }
		}
		PokemonFormData PokemonFormData { get; }
		NatureData NatureData { get; }
		AbilityData AbilityData { get; }
		ItemData BallCaughtItemData { get; }
		ItemData HeldItemData { get; }
		MoveData Move1Data { get; }
		MoveData Move2Data { get; }
		MoveData Move3Data { get; }
		MoveData Move4Data { get; }
		MoveData GetMoveDataAt(int index);
		BitmapSource Sprite { get; }
		BitmapImage BoxSprite { get; }

		#endregion

		#region Implemented

		#region Pokemon Info

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
					switch (GameType) {
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
				if (GameSave != null && GameType == GameTypes.Any)
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
		public bool IsShiny {
			get { return ((uint)TrainerID ^ (uint)SecretID ^ (uint)(Personality & 0xFFFF) ^ (uint)((Personality >> 16) & 0xFFFF)) < 8; }
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

		#region Personalization Info

		public bool HasNickname {
			get { return Nickname != PokemonData.Name.ToUpper(); }
		}
		public bool IsHoldingItem {
			get { return HeldItemID != 0; }
		}
		public bool IsHoldingMail {
			get { return HeldItemID >= 121 && HeldItemID <= 132; }
		}
		public bool IsCircleMarked {
			get { return Markings.HasFlag(MarkingFlags.Circle); }
			set {
				if (value) Markings |= MarkingFlags.Circle;
				else Markings &= ~MarkingFlags.Circle;
			}
		}
		public bool IsSquareMarked {
			get { return Markings.HasFlag(MarkingFlags.Square); }
			set {
				if (value) Markings |= MarkingFlags.Square;
				else Markings &= ~MarkingFlags.Square;
			}
		}
		public bool IsTriangleMarked {
			get { return Markings.HasFlag(MarkingFlags.Triangle); }
			set {
				if (value) Markings |= MarkingFlags.Triangle;
				else Markings &= ~MarkingFlags.Triangle;
			}
		}
		public bool IsHeartMarked {
			get { return Markings.HasFlag(MarkingFlags.Heart); }
			set {
				if (value) Markings |= MarkingFlags.Heart;
				else Markings &= ~MarkingFlags.Heart;
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

		#region Pokerus Info

		public PokerusStatuses PokerusStatus {
			get {
				if (PokerusStrain != 0)
					return (PokerusDaysRemaining != 0 ? PokerusStatuses.Infected : PokerusStatuses.Cured);
				else
					return (PokerusDaysRemaining != 0 ? PokerusStatuses.Invalid : PokerusStatuses.None);
			}
		}
		public PokerusStrainTypes PokerusStrainType {
			get { return (PokerusStrain != 0 ? (PokerusStrainTypes)(PokerusStrain % 4) : PokerusStrainTypes.None); }
		}

		#endregion

		#region Functions

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

		#endregion

		#endregion

		#region Unimplemented


		#region Pokemon Info

		public abstract uint Personality { get; set; }
		public abstract ushort SpeciesID { get; set; }
		bool IsSecondAbility { get; }
		bool IsSecondAbility2 { get; set; }
		bool IsEgg { get; }

		#endregion

		#region Met Info

		string TrainerName { get; set; }
		Genders TrainerGender { get; set; }
		ushort TrainerID { get; set; }
		ushort SecretID { get; set; }
		byte BallCaughtID { get; set; }
		byte LevelMet { get; set; }
		ushort MetLocationID { get; set; }
		bool IsFatefulEncounter { get; set; }
		GameOrigins GameOrigin { get; set; }
		Languages Language { get; set; }

		#endregion

		#region Personalization Info

		string Nickname { get; set; }
		bool HasNickname { get; }
		ushort HeldItemID { get; set; }
		bool IsHoldingItem { get; }
		MarkingFlags Markings { get; set; }
		bool IsCircleMarked { get; set; }
		bool IsSquareMarked { get; set; }
		bool IsTriangleMarked { get; set; }
		bool IsHeartMarked { get; set; }
		bool IsHoldingMail { get; }

		#endregion

		#region Stats Info

		byte Level { get; set; }
		uint Experience { get; set; }
		byte Friendship { get; set; }

		ushort HP { get; set; }
		ushort Attack { get; set; }
		ushort Defense { get; set; }
		ushort Speed { get; set; }
		ushort SpAttack { get; set; }
		ushort SpDefense { get; set; }

		byte HPEV { get; set; }
		byte AttackEV { get; set; }
		byte DefenseEV { get; set; }
		byte SpeedEV { get; set; }
		byte SpAttackEV { get; set; }
		byte SpDefenseEV { get; set; }

		byte HPIV { get; set; }
		byte AttackIV { get; set; }
		byte DefenseIV { get; set; }
		byte SpeedIV { get; set; }
		byte SpAttackIV { get; set; }
		byte SpDefenseIV { get; set; }

		#endregion

		#region Status Info

		ushort CurrentHP { get; set; }
		StatusConditionFlags StatusCondition { get; set; }
		byte TurnsOfSleepRemaining { get; set; }
		byte TurnsOfBadPoison { get; set; }
		PokerusStatuses PokerusStatus { get; }
		PokerusStrainTypes PokerusStrainType { get; }
		byte PokerusStrain { get; set; }
		byte PokerusDaysRemaining { get; set; }
		byte PokerusRemaining { get; set; }

		#endregion

		#region Contest Info

		byte Coolness { get; set; }
		byte Beauty { get; set; }
		byte Cuteness { get; set; }
		byte Smartness { get; set; }
		byte Toughness { get; set; }
		byte Feel { get; set; }

		byte CoolRibbonCount { get; set; }
		byte BeautyRibbonCount { get; set; }
		byte CuteRibbonCount { get; set; }
		byte SmartRibbonCount { get; set; }
		byte ToughRibbonCount { get; set; }
		bool HasChampionRibbon { get; set; }
		bool HasWinningRibbon { get; set; }
		bool HasVictoryRibbon { get; set; }
		bool HasArtistRibbon { get; set; }
		bool HasEffortRibbon { get; set; }
		bool HasMarineRibbon { get; set; }
		bool HasLandRibbon { get; set; }
		bool HasSkyRibbon { get; set; }
		bool HasCountryRibbon { get; set; }
		bool HasNationalRibbon { get; set; }
		bool HasEarthRibbon { get; set; }
		bool HasWorldRibbon { get; set; }

		#endregion

		#region Move Info

		byte NumMoves { get; }

		ushort Move1ID { get; set; }
		ushort Move2ID { get; set; }
		ushort Move3ID { get; set; }
		ushort Move4ID { get; set; }
		byte Move1PP { get; set; }
		byte Move2PP { get; set; }
		byte Move3PP { get; set; }
		byte Move4PP { get; set; }
		byte Move1PPUpsUsed { get; set; }
		byte Move2PPUpsUsed { get; set; }
		byte Move3PPUpsUsed { get; set; }
		byte Move4PPUpsUsed { get; set; }
		byte Move1TotalPP { get; }
		byte Move2TotalPP { get; }
		byte Move3TotalPP { get; }
		byte Move4TotalPP { get; }

		Move GetMoveAt(int index);
		void SetMoveAt(int index, Move move);
		ushort GetMoveIDAt(int index);
		void SetMoveIDAt(int index, ushort id);
		byte GetMovePPAt(int index);
		void SetMovePPAt(int index, byte pp);
		byte GetMovePPUpsUsedAt(int index);
		void SetMovePPUpsUsedAt(int index, byte ppUpsUsed);
		byte GetMoveTotalPPAt(int index);

		#endregion

		#region Shadow Pokemon Info

		ushort ShadowPokemonID { get; set; }
		bool IsShadowPokemon { get; }
		bool IsPurifiedShadowPokemon { get; }
		int Purification { get; set; }
		uint HeartGauge { get; }
		uint ExperienceStored { get; set; }

		#endregion

		#region Modification Functions

		void WipeEVs();
		void RemoveNickname();
		void InfectWithPokerus(byte strain);
		void CurePokerus();
		void SetHiddenPowerType(PokemonTypes type);
		void SetHiddenPowerDamage(byte damage);

		#endregion

		#region Functions

		void RecalculateStats();
		byte[] GetFinalData();

		#endregion

		#region Converting

		IPokemon Clone();
		GBAPokemon CreateGBAPokemon(GameTypes gameType);
		BoxPokemon CreateBoxPokemon();
		ColosseumPokemon CreateColosseumPokemon(GCRegions currentRegion);
		XDPokemon CreateXDPokemon(GCRegions currentRegion);

		#endregion

		#endregion
	}
}
