using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public interface IPokemon {

		#region Basic

		byte[] Raw { get; }
		IPokeContainer PokeContainer { get; set; }
		IPokePC PokePC { get; }
		int ContainerIndex { get; }
		IGameSave GameSave { get; }
		GameTypes GameType { get; set; }
		bool IsValid { get; }
		bool IsMoving { get; set; }
		bool IsReleased { get; set; }
		bool IsInDaycare { get; }
		PokemonFinder PokemonFinder { get; set; }

		#endregion

		#region Lookup Data Structures

		PokemonData PokemonData { get; }
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

		#region Pokemon Info

		uint Personality { get; set; }
		ushort SpeciesID { get; set; }
		ushort DexID { get; set; }
		byte FormID { get; }
		byte DeoxysForm { get; set; }
		bool HasForm { get; }
		byte NatureID { get; }
		Genders Gender { get; }
		bool IsSecondAbility { get; }
		bool IsSecondAbility2 { get; set; }
		bool IsShiny { get; }
		bool IsEgg { get; }
		PokemonTypes HiddenPowerType { get; }
		byte HiddenPowerDamage { get; }
		bool WurpleIsCascoon { get; }

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
		PokerusStrainVariations PokerusStrainVariation { get; }
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
		void RestoreHealth();

		#endregion

		#region Functions

		void RecalculateStats();
		byte[] GetFinalData();

		#endregion

		#region Converting

		IPokemon Clone();
		GBAPokemon CreateGBAPokemon(GameTypes gameType, bool passFinder = true);
		BoxPokemon CreateBoxPokemon(bool passFinder = true);
		ColosseumPokemon CreateColosseumPokemon(GCRegions currentRegion, bool passFinder = true);
		XDPokemon CreateXDPokemon(GCRegions currentRegion, bool passFinder = true);

		#endregion
	}
}
