using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {

	public enum ComparisonTypes {
		None,
		Equal,
		NotEqual,
		GreaterThan,
		LessThan
	}

	public enum SortMethods {
		None,
		DexNumber,
		Alphabetical,
		Level,
		Friendship,
		HatchCounter,
		HiddenPowerDamage,
		TotalStats,
		TotalIVs,
		TotalEVs,
		TotalCondition,
		RibbonCount,
		HP,
		Attack,
		Defense,
		SpAttack,
		SpDefense,
		Speed,
		HPIV,
		AttackIV,
		DefenseIV,
		SpAttackIV,
		SpDefenseIV,
		SpeedIV,
		HPEV,
		AttackEV,
		DefenseEV,
		SpAttackEV,
		SpDefenseEV,
		SpeedEV,
		Coolness,
		Beauty,
		Cuteness,
		Smartness,
		Toughness,
		Feel
	}

	public enum PokemonRibbons {
		Any,
		Uncommon,
		CoolNormalRank,
		CoolSuperRank,
		CoolHyperRank,
		CoolMasterRank,
		BeautyNormalRank,
		BeautySuperRank,
		BeautyHyperRank,
		BeautyMasterRank,
		CuteNormalRank,
		CuteSuperRank,
		CuteHyperRank,
		CuteMasterRank,
		SmartNormalRank,
		SmartSuperRank,
		SmartHyperRank,
		SmartMasterRank,
		ToughNormalRank,
		ToughSuperRank,
		ToughHyperRank,
		ToughMasterRank,
		Champion,
		Winning,
		Victory,
		Artist,
		Effort,
		Marine,
		Land,
		Sky,
		Country,
		National,
		Earth,
		World
	}

	public enum EggModes {
		IncludeEggs,
		ExcludeEggs,
		OnlyEggs
	}
	public enum ShinyModes {
		IncludeShinies,
		ExcludeShinies,
		OnlyShinies
	}
	public enum ShadowModes {
		IncludeShadow,
		ExcludeShadow,
		OnlyShadow
	}

	public enum SortOrders {
		HighestToLowest,
		LowestToHighest
	}

	public enum SearchModes {
		NewSearch,
		AddResults,
		RefineResults
	}


	public class PokemonSearchSettings {

		#region Pokemon
		
		public bool SpeciesEnabled { get; set; }
		public ushort SpeciesValue { get; set; }

		public bool TypeEnabled { get; set; }
		public PokemonTypes TypeValue { get; set; }

		public bool NatureEnabled { get; set; }
		public byte NatureValue { get; set; }

		public bool AbilityEnabled { get; set; }
		public byte AbilityValue { get; set; }

		public bool HeldItemEnabled { get; set; }
		public ushort HeldItemValue { get; set; }

		public bool PokeBallEnabled { get; set; }
		public byte PokeBallValue { get; set; }

		public bool RibbonEnabled { get; set; }
		public PokemonRibbons RibbonValue { get; set; }

		public bool PokerusEnabled { get; set; }
		public PokerusStatuses PokerusValue { get; set; }

		public bool GenderEnabled { get; set; }
		public Genders GenderValue { get; set; }

		public bool HatchCounterEnabled { get; set; }
		public ComparisonTypes HatchCounterComparison { get; set; }
		public byte HatchCounterValue { get; set; }

		#endregion

		#region Stats
		
		public bool StatEnabled { get; set; }
		public StatTypes Stat { get; set; }
		public ComparisonTypes StatComparison { get; set; }
		public uint StatValue { get; set; }
		
		public bool ConditionEnabled { get; set; }
		public ConditionTypes Condition { get; set; }
		public ComparisonTypes ConditionComparison { get; set; }
		public uint ConditionValue { get; set; }
		
		public bool IVEnabled { get; set; }
		public StatTypes IV { get; set; }
		public ComparisonTypes IVComparison { get; set; }
		public uint IVValue { get; set; }
		
		public bool EVEnabled { get; set; }
		public StatTypes EV { get; set; }
		public ComparisonTypes EVComparison { get; set; }
		public uint EVValue { get; set; }
		
		public bool LevelEnabled { get; set; }
		public ComparisonTypes LevelComparison { get; set; }
		public byte LevelValue { get; set; }

		public bool FriendshipEnabled { get; set; }
		public ComparisonTypes FriendshipComparison { get; set; }
		public byte FriendshipValue { get; set; }

		#endregion

		#region Moves

		public bool MovesEnabled { get; set; }
		public ushort Move1Value { get; set; }
		public ushort Move2Value { get; set; }
		public ushort Move3Value { get; set; }
		public ushort Move4Value { get; set; }

		public bool HiddenPowerDamageEnabled { get; set; }
		public ComparisonTypes HiddenPowerDamageComparison { get; set; }
		public byte HiddenPowerDamageValue { get; set; }
		public bool HiddenPowerTypeEnabled { get; set; }
		public PokemonTypes HiddenPowerTypeValue { get; set; }
		
		public bool EggGroupEnabled { get; set; }
		public EggGroups EggGroupValue { get; set; }

		#endregion

		#region Misc

		public EggModes EggMode { get; set; }
		public ShinyModes ShinyMode { get; set; }
		public ShadowModes ShadowMode { get; set; }

		#endregion

		#region Search

		public bool GameEnabled { get; set; }
		public int GameIndex { get; set; }

		public SortMethods SortMethod { get; set; }
		public SortOrders SortOrder { get; set; }
		public SearchModes SearchMode { get; set; }

		#endregion

		public PokemonSearchSettings() {
			SpeciesValue = 1;

			TypeValue = PokemonTypes.Normal;
			NatureValue = 0;
			AbilityValue = 1;
			HeldItemValue = 0;
			PokeBallValue = 4;
			PokerusValue = PokerusStatuses.None;
			RibbonValue = PokemonRibbons.Any;

			Stat = StatTypes.HP;
			IV = StatTypes.HP;
			EV = StatTypes.HP;
			Condition = ConditionTypes.Cool;
			StatComparison = ComparisonTypes.Equal;
			IVComparison = ComparisonTypes.Equal;
			EVComparison = ComparisonTypes.Equal;
			ConditionComparison = ComparisonTypes.Equal;
			LevelComparison = ComparisonTypes.Equal;
			FriendshipComparison = ComparisonTypes.Equal;
			LevelValue = 1;

			Move1Value = 0;
			Move2Value = 0;
			Move3Value = 0;
			Move4Value = 0;

			HiddenPowerDamageComparison = ComparisonTypes.Equal;
			HiddenPowerDamageValue = 30;
			HiddenPowerTypeValue = PokemonTypes.Fighting;

			EggGroupValue = EggGroups.Field;

			SortMethod = SortMethods.None;
			SortOrder = SortOrders.HighestToLowest;
			SearchMode = SearchModes.NewSearch;

			GameIndex = -1;
		}
	}



	public class PokemonSearch {

		public PokemonSearchSettings Search { get; set; }

		public List<IPokemon> Results { get; set; }

		public void SearchPokemon() {
			
			if (Search.SearchMode == SearchModes.NewSearch) {
				Results.Clear();
			}
			else {
				// Make sure to recapture the location and final class of every pokemon
				for (int i = 0; i < Results.Count; i++) {
					if (Results[i].ContainerIndex == -1)
						Results[i] = Results[i].PokemonFinder.Pokemon;
				}
			}

			if (Search.SearchMode == SearchModes.RefineResults) {
				List<IPokemon> previousResults = Results;
				Results = new List<IPokemon>();
				foreach (IPokemon pokemon in previousResults) {
					if (Search.GameEnabled && Search.GameIndex != pokemon.GameSave.GameIndex)
						continue;

					if (IsPokemonValid(pokemon))
						Results.Add(pokemon);
				}
			}
			else {
				for (int i = -1; i < PokeManager.NumGameSaves; i++) {
					IGameSave gameSave = PokeManager.GetGameSaveAt(i);
					if (Search.GameEnabled && Search.GameIndex != i)
						continue;

					if (gameSave is ManagerGameSave) {
						for (int j = 0; j < (gameSave as ManagerGameSave).NumPokePCRows; j++) {
							foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(j)) {
								if (IsPokemonValid(pokemon) && (Search.SearchMode == SearchModes.NewSearch || !Results.Contains(pokemon)))
									Results.Add(pokemon);
							}
						}
					}
					else {
						foreach (IPokemon pokemon in gameSave.PokePC) {
							if (IsPokemonValid(pokemon) && (Search.SearchMode == SearchModes.NewSearch || !Results.Contains(pokemon)))
								Results.Add(pokemon);
						}
					}
				}
			}

			// Sorting time
			if (Search.SortMethod != SortMethods.None) {
				if (Search.SortMethod == SortMethods.Alphabetical) {
					if (Search.SortOrder == SortOrders.LowestToHighest)
						Results.Sort((p1, p2) => (string.Compare(p1.PokemonData.Name, p2.PokemonData.Name, true)));
					else
						Results.Sort((p1, p2) => (string.Compare(p2.PokemonData.Name, p1.PokemonData.Name, true)));
				}
				else {
					if (Search.SortOrder == SortOrders.LowestToHighest)
						Results.Sort((p1, p2) => (int)(GetSortMethodValue(p1) - GetSortMethodValue(p2)));
					else
						Results.Sort((p1, p2) => (int)(GetSortMethodValue(p2) - GetSortMethodValue(p1)));
				}
			}
		}

		public PokemonSearch() {
			Search = new PokemonSearchSettings();
			Results = new List<IPokemon>();
		}


		public bool IsPokemonValid(IPokemon pokemon) {

			bool valid = true;

			#region Pokemon

			if (Search.SpeciesEnabled)
				CompareValues(ref valid, pokemon.DexID == Search.SpeciesValue);

			if (Search.TypeEnabled)
				CompareValues(ref valid, pokemon.PokemonData.Type1 == Search.TypeValue || pokemon.PokemonData.Type2 == Search.TypeValue);

			if (Search.NatureEnabled)
				CompareValues(ref valid, pokemon.NatureID == Search.NatureValue);
			
			if (Search.AbilityEnabled)
				CompareValues(ref valid, pokemon.AbilityData.ID == Search.AbilityValue);

			if (Search.HeldItemEnabled) {
				if (Search.HeldItemValue == 0) // Any
					CompareValues(ref valid, pokemon.HeldItemID != 0);
				else
					CompareValues(ref valid, pokemon.HeldItemID == Search.HeldItemValue);
			}
			
			if (Search.PokeBallEnabled)
				CompareValues(ref valid, pokemon.BallCaughtID == Search.PokeBallValue);

			if (Search.RibbonEnabled)
				CompareValues(ref valid, PokemonHasRibbon(pokemon, Search.RibbonValue));

			if (Search.PokerusEnabled)
				CompareValues(ref valid, pokemon.PokerusStatus == Search.PokerusValue);

			if (Search.GenderEnabled)
				CompareValues(ref valid, pokemon.Gender == Search.GenderValue);

			if (Search.HatchCounterEnabled) {
				if (pokemon.IsEgg)
					CompareValues(ref valid, Search.HatchCounterComparison, pokemon.Friendship, Search.HatchCounterValue);
			}

			#endregion

			#region Stats

			if (Search.StatEnabled) {
				if (Search.Stat == StatTypes.Any || Search.Stat == StatTypes.All)
					CompareValues(ref valid, Search.StatComparison, GetPokemonStats(pokemon), Search.StatValue, Search.Stat == StatTypes.All);
				else
					CompareValues(ref valid, Search.StatComparison, GetPokemonStat(pokemon, Search.Stat), Search.StatValue);
			}

			if (Search.ConditionEnabled) {
				if (Search.Condition == ConditionTypes.Any || Search.Condition == ConditionTypes.All)
					CompareValues(ref valid, Search.ConditionComparison, GetPokemonConditions(pokemon), Search.ConditionValue, Search.Condition == ConditionTypes.All);
				else
					CompareValues(ref valid, Search.ConditionComparison, GetPokemonCondition(pokemon, Search.Condition), Search.ConditionValue);
			}

			if (Search.IVEnabled) {
				if (Search.IV == StatTypes.Any || Search.IV == StatTypes.All)
					CompareValues(ref valid, Search.IVComparison, GetPokemonIVs(pokemon), Search.IVValue, Search.IV == StatTypes.All);
				else
					CompareValues(ref valid, Search.IVComparison, GetPokemonIV(pokemon, Search.IV), Search.IVValue);
			}

			if (Search.EVEnabled) {
				if (Search.EV == StatTypes.Any || Search.EV == StatTypes.All)
					CompareValues(ref valid, Search.EVComparison, GetPokemonEVs(pokemon), Search.EVValue, Search.EV == StatTypes.All);
				else
					CompareValues(ref valid, Search.EVComparison, GetPokemonEV(pokemon, Search.EV), Search.EVValue);
			}

			if (Search.LevelEnabled)
				CompareValues(ref valid, Search.LevelComparison, pokemon.Level, Search.LevelValue);

			if (Search.FriendshipEnabled) {
				if (!pokemon.IsEgg)
					CompareValues(ref valid, Search.FriendshipComparison, pokemon.Friendship, Search.FriendshipValue);
			}

			#endregion

			#region Moves
			
			if (Search.MovesEnabled) {
				if (Search.Move1Value != 0)
					CompareValues(ref valid, PokemonHasMove(pokemon, Search.Move1Value));

				if (Search.Move2Value != 0)
					CompareValues(ref valid, PokemonHasMove(pokemon, Search.Move2Value));

				if (Search.Move3Value != 0)
					CompareValues(ref valid, PokemonHasMove(pokemon, Search.Move3Value));

				if (Search.Move4Value != 0)
					CompareValues(ref valid, PokemonHasMove(pokemon, Search.Move4Value));
			}

			if (Search.HiddenPowerDamageEnabled)
				CompareValues(ref valid, Search.HiddenPowerDamageComparison, pokemon.HiddenPowerDamage, Search.HiddenPowerDamageValue);

			if (Search.HiddenPowerTypeEnabled)
				CompareValues(ref valid, pokemon.HiddenPowerType == Search.HiddenPowerTypeValue);

			if (Search.EggGroupEnabled)
				CompareValues(ref valid, pokemon.PokemonData.EggGroup1 == Search.EggGroupValue || pokemon.PokemonData.EggGroup2 == Search.EggGroupValue);

			#endregion

			#region Misc

			if (Search.EggMode != EggModes.IncludeEggs)
				CompareValues(ref valid, pokemon.IsEgg == (Search.EggMode == EggModes.OnlyEggs));

			if (Search.ShinyMode != ShinyModes.IncludeShinies)
				CompareValues(ref valid, pokemon.IsShiny == (Search.ShinyMode == ShinyModes.OnlyShinies));

			if (Search.ShadowMode != ShadowModes.IncludeShadow)
				CompareValues(ref valid, pokemon.IsShadowPokemon == (Search.ShadowMode == ShadowModes.OnlyShadow));

			#endregion

			return valid;
		}

		private bool PokemonHasMove(IPokemon pokemon, ushort move) {
			for (int i = 0; i < pokemon.NumMoves; i++) {
				if (pokemon.GetMoveIDAt(i) == move)
					return true;
			}
			return false;
		}

		private long GetPokemonCondition(IPokemon pokemon, ConditionTypes condition) {
			switch (condition) {
			case ConditionTypes.Cool: return pokemon.Coolness;
			case ConditionTypes.Beauty: return pokemon.Beauty;
			case ConditionTypes.Cute: return pokemon.Cuteness;
			case ConditionTypes.Smart: return pokemon.Smartness;
			case ConditionTypes.Tough: return pokemon.Toughness;
			case ConditionTypes.Feel: return pokemon.Feel;
			case ConditionTypes.Total: return ((int)pokemon.Coolness + pokemon.Beauty + pokemon.Cuteness + pokemon.Smartness + pokemon.Toughness);
			}
			return 0;
		}

		private long[] GetPokemonConditions(IPokemon pokemon) {
			long[] list = new long[5];
			list[0] = pokemon.Coolness;
			list[1] = pokemon.Beauty;
			list[2] = pokemon.Cuteness;
			list[3] = pokemon.Smartness;
			list[4] = pokemon.Toughness;
			return list;
		}

		private long GetPokemonStat(IPokemon pokemon, StatTypes stat) {
			switch (stat) {
			case StatTypes.HP: return pokemon.HP;
			case StatTypes.Attack: return pokemon.Attack;
			case StatTypes.Defense: return pokemon.Defense;
			case StatTypes.SpAttack: return pokemon.SpAttack;
			case StatTypes.SpDefense: return pokemon.SpDefense;
			case StatTypes.Speed: return pokemon.Speed;
			case StatTypes.Total: return ((int)pokemon.HP + pokemon.Attack + pokemon.Defense + pokemon.SpAttack + pokemon.SpDefense + pokemon.Speed);
			}
			return 0;
		}

		private long[] GetPokemonStats(IPokemon pokemon) {
			long[] list = new long[6];
			list[0] = pokemon.HP;
			list[1] = pokemon.Attack;
			list[2] = pokemon.Defense;
			list[3] = pokemon.SpAttack;
			list[4] = pokemon.SpDefense;
			list[5] = pokemon.Speed;
			return list;
		}
		
		private long GetPokemonIV(IPokemon pokemon, StatTypes stat) {
			switch (stat) {
			case StatTypes.HP: return pokemon.HPIV;
			case StatTypes.Attack: return pokemon.AttackIV;
			case StatTypes.Defense: return pokemon.DefenseIV;
			case StatTypes.SpAttack: return pokemon.SpAttackIV;
			case StatTypes.SpDefense: return pokemon.SpDefenseIV;
			case StatTypes.Speed: return pokemon.SpeedIV;
			case StatTypes.Total: return ((int)pokemon.HPIV + pokemon.AttackIV + pokemon.DefenseIV + pokemon.SpAttackIV + pokemon.SpDefenseIV + pokemon.SpeedIV);
			}
			return 0;
		}

		private long[] GetPokemonIVs(IPokemon pokemon) {
			long[] list = new long[6];
			list[0] = pokemon.HPIV;
			list[1] = pokemon.AttackIV;
			list[2] = pokemon.DefenseIV;
			list[3] = pokemon.SpAttackIV;
			list[4] = pokemon.SpDefenseIV;
			list[5] = pokemon.SpeedIV;
			return list;
		}

		private long GetPokemonEV(IPokemon pokemon, StatTypes stat) {
			switch (stat) {
			case StatTypes.HP: return pokemon.HPEV;
			case StatTypes.Attack: return pokemon.AttackEV;
			case StatTypes.Defense: return pokemon.DefenseEV;
			case StatTypes.SpAttack: return pokemon.SpAttackEV;
			case StatTypes.SpDefense: return pokemon.SpDefenseEV;
			case StatTypes.Speed: return pokemon.SpeedEV;
			case StatTypes.Total: return ((int)pokemon.HPEV + pokemon.AttackEV + pokemon.DefenseEV + pokemon.SpAttackEV + pokemon.SpDefenseEV + pokemon.SpeedEV);
			}
			return 0;
		}

		private long[] GetPokemonEVs(IPokemon pokemon) {
			long[] list = new long[6];
			list[0] = pokemon.HPEV;
			list[1] = pokemon.AttackEV;
			list[2] = pokemon.DefenseEV;
			list[3] = pokemon.SpAttackEV;
			list[4] = pokemon.SpDefenseEV;
			list[5] = pokemon.SpeedEV;
			return list;
		}

		private bool PokemonHasRibbon(IPokemon pokemon, PokemonRibbons ribbon) {
			switch (ribbon) {
			case PokemonRibbons.Any:
				return (pokemon.CoolRibbonCount > 0 || pokemon.BeautyRibbonCount > 0 || pokemon.CuteRibbonCount > 0 || pokemon.SmartRibbonCount > 0 || pokemon.ToughRibbonCount > 0) ||
						(pokemon.HasChampionRibbon || pokemon.HasWinningRibbon || pokemon.HasVictoryRibbon || pokemon.HasArtistRibbon || pokemon.HasEffortRibbon) ||
						(pokemon.HasMarineRibbon || pokemon.HasLandRibbon || pokemon.HasSkyRibbon || pokemon.HasCountryRibbon || pokemon.HasNationalRibbon) ||
						(pokemon.HasEarthRibbon || pokemon.HasWorldRibbon);

			case PokemonRibbons.Uncommon:
				return (pokemon.CoolRibbonCount > 0 || pokemon.BeautyRibbonCount > 0 || pokemon.CuteRibbonCount > 0 || pokemon.SmartRibbonCount > 0 || pokemon.ToughRibbonCount > 0) ||
						(pokemon.HasWinningRibbon || pokemon.HasVictoryRibbon || pokemon.HasArtistRibbon || pokemon.HasMarineRibbon || pokemon.HasLandRibbon) ||
						(pokemon.HasSkyRibbon || pokemon.HasCountryRibbon || pokemon.HasEarthRibbon || pokemon.HasWorldRibbon);

			case PokemonRibbons.CoolNormalRank: return pokemon.CoolRibbonCount >= 1;
			case PokemonRibbons.CoolSuperRank: return pokemon.CoolRibbonCount >= 2;
			case PokemonRibbons.CoolHyperRank: return pokemon.CoolRibbonCount >= 3;
			case PokemonRibbons.CoolMasterRank: return pokemon.CoolRibbonCount >= 4;

			case PokemonRibbons.BeautyNormalRank: return pokemon.BeautyRibbonCount >= 1;
			case PokemonRibbons.BeautySuperRank: return pokemon.BeautyRibbonCount >= 2;
			case PokemonRibbons.BeautyHyperRank: return pokemon.BeautyRibbonCount >= 3;
			case PokemonRibbons.BeautyMasterRank: return pokemon.BeautyRibbonCount >= 4;

			case PokemonRibbons.CuteNormalRank: return pokemon.CuteRibbonCount >= 1;
			case PokemonRibbons.CuteSuperRank: return pokemon.CuteRibbonCount >= 2;
			case PokemonRibbons.CuteHyperRank: return pokemon.CuteRibbonCount >= 3;
			case PokemonRibbons.CuteMasterRank: return pokemon.CuteRibbonCount >= 4;

			case PokemonRibbons.SmartNormalRank: return pokemon.SmartRibbonCount >= 1;
			case PokemonRibbons.SmartSuperRank: return pokemon.SmartRibbonCount >= 2;
			case PokemonRibbons.SmartHyperRank: return pokemon.SmartRibbonCount >= 3;
			case PokemonRibbons.SmartMasterRank: return pokemon.SmartRibbonCount >= 4;

			case PokemonRibbons.ToughNormalRank: return pokemon.ToughRibbonCount >= 1;
			case PokemonRibbons.ToughSuperRank: return pokemon.ToughRibbonCount >= 2;
			case PokemonRibbons.ToughHyperRank: return pokemon.ToughRibbonCount >= 3;
			case PokemonRibbons.ToughMasterRank: return pokemon.ToughRibbonCount >= 4;

			case PokemonRibbons.Champion: return pokemon.HasChampionRibbon;
			case PokemonRibbons.Winning: return pokemon.HasWinningRibbon;
			case PokemonRibbons.Victory: return pokemon.HasVictoryRibbon;
			case PokemonRibbons.Artist: return pokemon.HasArtistRibbon;
			case PokemonRibbons.Effort: return pokemon.HasEffortRibbon;

			case PokemonRibbons.Marine: return pokemon.HasMarineRibbon;
			case PokemonRibbons.Land: return pokemon.HasLandRibbon;
			case PokemonRibbons.Sky: return pokemon.HasSkyRibbon;
			case PokemonRibbons.Country: return pokemon.HasCountryRibbon;
			case PokemonRibbons.National: return pokemon.HasNationalRibbon;
			case PokemonRibbons.Earth: return pokemon.HasEarthRibbon;
			case PokemonRibbons.World: return pokemon.HasWorldRibbon;
			}

			return false;
		}

		private void CompareValues(ref bool valid, bool comparison) {
			if (!comparison)
				valid = false;
		}



		private void CompareValues(ref bool valid, ComparisonTypes comparison, long originalValue, long testValue) {
			bool result = true;
			switch (comparison) {
			case ComparisonTypes.Equal: result = originalValue == testValue; break;
			case ComparisonTypes.NotEqual: result = originalValue != testValue; break;
			case ComparisonTypes.GreaterThan: result = originalValue > testValue; break;
			case ComparisonTypes.LessThan: result = originalValue < testValue; break;
			}
			if (!result)
				valid = false;
		}
		public void CompareValues(ref bool valid, ComparisonTypes comparison, long[] originalValues, long testValue, bool all) {
			bool result = all;
			bool result2 = false;
			if (originalValues.Length > 0) {
				foreach (long originalValue in originalValues) {
					switch (comparison) {
					case ComparisonTypes.Equal: result2 = originalValue == testValue; break;
					case ComparisonTypes.NotEqual: result2 = originalValue != testValue; break;
					case ComparisonTypes.GreaterThan: result2 = originalValue > testValue; break;
					case ComparisonTypes.LessThan: result2 = originalValue < testValue; break;
					}
					if (result2 && !all) {
						result = true;
						break;
					}
					else if (!result2 && all) {
						result = false;
						break;
					}
				}
			}
			if (!result)
				valid = false;
		}

		private long GetSortMethodValue(IPokemon pokemon) {
			switch (Search.SortMethod) {
			case SortMethods.DexNumber: return pokemon.DexID;
			case SortMethods.Level: return pokemon.Level;
			case SortMethods.Friendship: return (!pokemon.IsEgg ? pokemon.Friendship : 70);
			case SortMethods.HatchCounter: return (pokemon.IsEgg ? pokemon.Friendship : 0);
			case SortMethods.HiddenPowerDamage: return pokemon.HiddenPowerDamage;
			case SortMethods.TotalStats: return ((int)pokemon.HP + pokemon.Attack + pokemon.Defense + pokemon.SpAttack + pokemon.SpDefense + pokemon.Speed);
			case SortMethods.TotalIVs: return ((int)pokemon.HPIV + pokemon.AttackIV + pokemon.DefenseIV + pokemon.SpAttackIV + pokemon.SpDefenseIV + pokemon.SpeedIV);
			case SortMethods.TotalEVs: return ((int)pokemon.HPEV + pokemon.AttackEV + pokemon.DefenseEV + pokemon.SpAttackEV + pokemon.SpDefenseEV + pokemon.SpeedEV);
			case SortMethods.TotalCondition: return ((int)pokemon.Coolness + pokemon.Beauty + pokemon.Cuteness + pokemon.Smartness + pokemon.Toughness);
			case SortMethods.RibbonCount: return GetRibbonCount(pokemon);
			case SortMethods.HP: return pokemon.HP;
			case SortMethods.Attack: return pokemon.Attack;
			case SortMethods.Defense: return pokemon.Defense;
			case SortMethods.SpAttack: return pokemon.SpAttack;
			case SortMethods.SpDefense: return pokemon.SpDefense;
			case SortMethods.Speed: return pokemon.Speed;
			case SortMethods.HPIV: return pokemon.HP;
			case SortMethods.AttackIV: return pokemon.AttackIV;
			case SortMethods.DefenseIV: return pokemon.DefenseIV;
			case SortMethods.SpAttackIV: return pokemon.SpAttackIV;
			case SortMethods.SpDefenseIV: return pokemon.SpDefenseIV;
			case SortMethods.SpeedIV: return pokemon.SpeedIV;
			case SortMethods.HPEV: return pokemon.HPEV;
			case SortMethods.AttackEV: return pokemon.AttackEV;
			case SortMethods.DefenseEV: return pokemon.DefenseEV;
			case SortMethods.SpAttackEV: return pokemon.SpAttackEV;
			case SortMethods.SpDefenseEV: return pokemon.SpDefenseEV;
			case SortMethods.SpeedEV: return pokemon.SpeedEV;
			case SortMethods.Coolness: return pokemon.Coolness;
			case SortMethods.Beauty: return pokemon.Beauty;
			case SortMethods.Cuteness: return pokemon.Cuteness;
			case SortMethods.Smartness: return pokemon.Smartness;
			case SortMethods.Toughness: return pokemon.Toughness;
			case SortMethods.Feel: return pokemon.Feel;
			}
			return 0;
		}

		private long GetRibbonCount(IPokemon pokemon) {
			int count = 0;
			count += pokemon.CoolRibbonCount;
			count += pokemon.BeautyRibbonCount;
			count += pokemon.CuteRibbonCount;
			count += pokemon.SmartRibbonCount;
			count += pokemon.ToughRibbonCount;

			if (pokemon.HasChampionRibbon) count++;
			if (pokemon.HasWinningRibbon) count++;
			if (pokemon.HasVictoryRibbon) count++;
			if (pokemon.HasArtistRibbon) count++;
			if (pokemon.HasEffortRibbon) count++;

			if (pokemon.HasMarineRibbon) count++;
			if (pokemon.HasLandRibbon) count++;
			if (pokemon.HasSkyRibbon) count++;
			if (pokemon.HasCountryRibbon) count++;
			if (pokemon.HasNationalRibbon) count++;
			if (pokemon.HasEarthRibbon) count++;
			if (pokemon.HasWorldRibbon) count++;

			return count;
		}
	}
}
