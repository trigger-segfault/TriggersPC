using PokemonManager.Game;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for WindowSearchPokemonSettings.xaml
	/// </summary>
	public partial class WindowSearchPokemonSettings : Window {

		private PokemonSearchSettings search;

		public PokemonSearchResults ResultsWindow { get; set; }

		public PokemonSearchSettings SearchSettings {
			get { return search; }
			set {
				if (value != null)
					search = value;
				else
					search = new PokemonSearchSettings();
				LoadSearchSettings();
			}
		}


		private List<IPokemon> results;

		public List<IPokemon> Results {
			get { return results; }
			set {
				if (value != null)
					results = value;
				else
					results = new List<IPokemon>();
			}
		}

		private bool loaded;
		public bool IsClosed { get; set; }


		public WindowSearchPokemonSettings() {
			InitializeComponent();
			loaded = false;

			search = new PokemonSearchSettings();

			PopulateComboBoxes();
			LoadSearchSettings();

			loaded = true;
		}

		public void RefreshUI() {
			ResultsWindow.RefreshUI();
		}

		public static WindowSearchPokemonSettings Show(Window owner, PokemonSearchSettings search = null, List<IPokemon> results = null) {
			WindowSearchPokemonSettings window = new WindowSearchPokemonSettings();
			window.SearchSettings = search;
			window.Results = results;
			window.Owner = owner;
			window.Show();

			return window;
		}

		private void PopulateComboBoxes() {
			bool oldLoaded = loaded;
			loaded = false;

			AddComparisonComboBoxItems(comboBoxHatchCounterComparison);
			AddComparisonComboBoxItems(comboBoxStatComparison);
			AddComparisonComboBoxItems(comboBoxConditionComparison);
			AddComparisonComboBoxItems(comboBoxIVComparison);
			AddComparisonComboBoxItems(comboBoxEVComparison);
			AddComparisonComboBoxItems(comboBoxLevelComparison);
			AddComparisonComboBoxItems(comboBoxFriendshipComparison);
			AddComparisonComboBoxItems(comboBoxHiddenPowerDamageComparison);

			comboBoxSpecies.Items.Clear();
			for (ushort i = 1; i <= 386; i++)
				AddComboBoxItem(comboBoxSpecies, PokemonDatabase.GetPokemonFromDexID(i).Name, (int)i);

			comboBoxType.Items.Clear();
			for (PokemonTypes i = PokemonTypes.Normal; i <= PokemonTypes.Dark; i++)
				AddComboBoxItem(comboBoxType, i.ToString(), (int)i);

			comboBoxNature.Items.Clear();
			for (byte i = 0; i <= 24; i++)
				AddComboBoxItem(comboBoxNature, PokemonDatabase.GetNatureFromID(i).Name, (int)i);

			comboBoxAbility.Items.Clear();
			for (byte i = 1; i <= 76; i++)
				AddComboBoxItem(comboBoxAbility, PokemonDatabase.GetAbilityFromID(i).Name, (int)i);

			comboBoxHeldItem.Items.Clear();
			AddComboBoxItem(comboBoxHeldItem, "Any", 0);
			for (int i = 1; ItemDatabase.GetItemAt(i) != null; i++) {
				ItemData item = ItemDatabase.GetItemAt(i);
				if (!item.IsImportant && item.ID < 500)
					AddComboBoxItem(comboBoxHeldItem, item.Name, (int)item.ID);
			}

			comboBoxPokeBall.Items.Clear();
			AddComboBoxItem(comboBoxPokeBall, "Poké Ball", 4);
			AddComboBoxItem(comboBoxPokeBall, "Great Ball", 3);
			AddComboBoxItem(comboBoxPokeBall, "Ultra Ball", 2);
			AddComboBoxItem(comboBoxPokeBall, "Master Ball", 1);
			AddComboBoxItem(comboBoxPokeBall, "Safari Ball", 5);
			AddComboBoxItem(comboBoxPokeBall, "Timer Ball", 10);
			AddComboBoxItem(comboBoxPokeBall, "Repeat Ball", 9);
			AddComboBoxItem(comboBoxPokeBall, "Net Ball", 6);
			AddComboBoxItem(comboBoxPokeBall, "Dive Ball", 7);
			AddComboBoxItem(comboBoxPokeBall, "Nest Ball", 8);
			AddComboBoxItem(comboBoxPokeBall, "Premier Ball", 12);
			AddComboBoxItem(comboBoxPokeBall, "Luxury Ball", 11);


			comboBoxRibbon.Items.Clear();
			for (PokemonRibbons i = PokemonRibbons.Any; i <= PokemonRibbons.World; i++) {
				// Add spaces to the names
				string name = i.ToString();
				StringBuilder newName = new StringBuilder();
				foreach (char c in name) {
					if (char.IsUpper(c) && newName.Length > 0)
						newName.Append(' ');
					newName.Append(c);
				}
				AddComboBoxItem(comboBoxRibbon, newName.ToString(), (int)i);
			}


			comboBoxPokerus.Items.Clear();
			AddComboBoxItem(comboBoxPokerus, "None", (int)PokerusStatuses.None);
			AddComboBoxItem(comboBoxPokerus, "Infected", (int)PokerusStatuses.Infected);
			AddComboBoxItem(comboBoxPokerus, "Cured", (int)PokerusStatuses.Cured);

			ComboBox[] statComboBoxes = new ComboBox[] { comboBoxStatType, comboBoxIVType, comboBoxEVType };
			for (int i = 0; i < 3; i++) {
				statComboBoxes[i].Items.Clear();
				AddComboBoxItem(statComboBoxes[i], "HP", (int)StatTypes.HP);
				AddComboBoxItem(statComboBoxes[i], "Attack", (int)StatTypes.Attack);
				AddComboBoxItem(statComboBoxes[i], "Defense", (int)StatTypes.Defense);
				AddComboBoxItem(statComboBoxes[i], "Sp Attack", (int)StatTypes.SpAttack);
				AddComboBoxItem(statComboBoxes[i], "Sp Defense", (int)StatTypes.SpDefense);
				AddComboBoxItem(statComboBoxes[i], "Speed", (int)StatTypes.Speed);
				AddComboBoxItem(statComboBoxes[i], "Any", (int)StatTypes.Any);
				AddComboBoxItem(statComboBoxes[i], "All", (int)StatTypes.All);
				AddComboBoxItem(statComboBoxes[i], "Total", (int)StatTypes.Total);
			}

			comboBoxConditionType.Items.Clear();
			AddComboBoxItem(comboBoxConditionType, "Cool", (int)ConditionTypes.Cool);
			AddComboBoxItem(comboBoxConditionType, "Beauty", (int)ConditionTypes.Beauty);
			AddComboBoxItem(comboBoxConditionType, "Cute", (int)ConditionTypes.Cute);
			AddComboBoxItem(comboBoxConditionType, "Smart", (int)ConditionTypes.Smart);
			AddComboBoxItem(comboBoxConditionType, "Tough", (int)ConditionTypes.Tough);
			AddComboBoxItem(comboBoxConditionType, "Feel", (int)ConditionTypes.Feel);
			AddComboBoxItem(comboBoxConditionType, "Any", (int)ConditionTypes.Any);
			AddComboBoxItem(comboBoxConditionType, "All", (int)ConditionTypes.All);
			AddComboBoxItem(comboBoxConditionType, "Total", (int)ConditionTypes.Total);

			comboBoxGender.Items.Clear();
			AddComboBoxItem(comboBoxGender, "Male", (int)Genders.Male);
			AddComboBoxItem(comboBoxGender, "Female", (int)Genders.Female);
			AddComboBoxItem(comboBoxGender, "Genderless", (int)Genders.Genderless);

			ComboBox[] moveComboBoxes = new ComboBox[] { comboBoxMove1, comboBoxMove2, comboBoxMove3, comboBoxMove4 };
			for (int i = 0; i < 4; i++) {
				moveComboBoxes[i].Items.Clear();
				AddComboBoxItem(moveComboBoxes[i], "None", 0);
				for (ushort j = 1; j <= 354; j++)
					AddComboBoxItem(moveComboBoxes[i], PokemonDatabase.GetMoveFromID(j).Name, (int)j);
			}

			comboBoxHiddenPowerType.Items.Clear();
			for (PokemonTypes i = PokemonTypes.Fighting; i <= PokemonTypes.Dark; i++)
				AddComboBoxItem(comboBoxHiddenPowerType, i.ToString(), (int)i);


			comboBoxEggGroup.Items.Clear();
			for (EggGroups i = EggGroups.Field; i <= EggGroups.Ditto; i++) {
				AddComboBoxItem(comboBoxEggGroup, PokemonDatabase.GetEggGroupName(i), (int)i);
			}

			comboBoxEggMode.Items.Clear();
			AddComboBoxItem(comboBoxEggMode, "Include Eggs", (int)EggModes.IncludeEggs);
			AddComboBoxItem(comboBoxEggMode, "Exclude Eggs", (int)EggModes.ExcludeEggs);
			AddComboBoxItem(comboBoxEggMode, "Only Eggs", (int)EggModes.OnlyEggs);

			comboBoxShinyMode.Items.Clear();
			AddComboBoxItem(comboBoxShinyMode, "Include Shinies", (int)ShinyModes.IncludeShinies);
			AddComboBoxItem(comboBoxShinyMode, "Exclude Shinies", (int)ShinyModes.ExcludeShinies);
			AddComboBoxItem(comboBoxShinyMode, "Only Shinies", (int)ShinyModes.OnlyShinies);

			comboBoxShadowMode.Items.Clear();
			AddComboBoxItem(comboBoxShadowMode, "Include Shadow", (int)ShadowModes.IncludeShadow);
			AddComboBoxItem(comboBoxShadowMode, "Exclude Shadow", (int)ShadowModes.ExcludeShadow);
			AddComboBoxItem(comboBoxShadowMode, "Only Shadow", (int)ShadowModes.OnlyShadow);

			comboBoxSortMethod.Items.Clear();
			AddComboBoxItem(comboBoxSortMethod, "None", (int)SortMethods.None);
			AddComboBoxItem(comboBoxSortMethod, "Dex Number", (int)SortMethods.DexNumber);
			AddComboBoxItem(comboBoxSortMethod, "Alphabetical", (int)SortMethods.Alphabetical);
			AddComboBoxItem(comboBoxSortMethod, "Level", (int)SortMethods.Level);
			AddComboBoxItem(comboBoxSortMethod, "Friendship", (int)SortMethods.Friendship);
			AddComboBoxItem(comboBoxSortMethod, "Hatch Counter", (int)SortMethods.HatchCounter);
			AddComboBoxItem(comboBoxSortMethod, "Hidden Power Damage", (int)SortMethods.HiddenPowerDamage);
			AddComboBoxItem(comboBoxSortMethod, "Total Stats", (int)SortMethods.TotalStats);
			AddComboBoxItem(comboBoxSortMethod, "Total IVs", (int)SortMethods.TotalIVs);
			AddComboBoxItem(comboBoxSortMethod, "Total EVs", (int)SortMethods.TotalEVs);
			AddComboBoxItem(comboBoxSortMethod, "Total Conditions", (int)SortMethods.TotalCondition);
			AddComboBoxItem(comboBoxSortMethod, "Ribbon Count", (int)SortMethods.RibbonCount);
			AddComboBoxItem(comboBoxSortMethod, "HP", (int)SortMethods.HP);
			AddComboBoxItem(comboBoxSortMethod, "Attack", (int)SortMethods.Attack);
			AddComboBoxItem(comboBoxSortMethod, "Defense", (int)SortMethods.Defense);
			AddComboBoxItem(comboBoxSortMethod, "Sp Attack", (int)SortMethods.SpAttack);
			AddComboBoxItem(comboBoxSortMethod, "Sp Defense", (int)SortMethods.SpDefense);
			AddComboBoxItem(comboBoxSortMethod, "Speed", (int)SortMethods.Speed);
			AddComboBoxItem(comboBoxSortMethod, "HP IV", (int)SortMethods.HPIV);
			AddComboBoxItem(comboBoxSortMethod, "Attack IV", (int)SortMethods.AttackIV);
			AddComboBoxItem(comboBoxSortMethod, "Defense IV", (int)SortMethods.DefenseIV);
			AddComboBoxItem(comboBoxSortMethod, "Sp Attack IV", (int)SortMethods.SpAttackIV);
			AddComboBoxItem(comboBoxSortMethod, "Sp Defense IV", (int)SortMethods.SpDefenseIV);
			AddComboBoxItem(comboBoxSortMethod, "Speed IV", (int)SortMethods.SpeedIV);
			AddComboBoxItem(comboBoxSortMethod, "HP EV", (int)SortMethods.HPEV);
			AddComboBoxItem(comboBoxSortMethod, "Attack EV", (int)SortMethods.AttackEV);
			AddComboBoxItem(comboBoxSortMethod, "Defense EV", (int)SortMethods.DefenseEV);
			AddComboBoxItem(comboBoxSortMethod, "Sp Attack EV", (int)SortMethods.SpAttackEV);
			AddComboBoxItem(comboBoxSortMethod, "Sp Defense EV", (int)SortMethods.SpDefenseEV);
			AddComboBoxItem(comboBoxSortMethod, "Speed EV", (int)SortMethods.SpeedEV);
			AddComboBoxItem(comboBoxSortMethod, "Coolness", (int)SortMethods.Coolness);
			AddComboBoxItem(comboBoxSortMethod, "Beauty", (int)SortMethods.Beauty);
			AddComboBoxItem(comboBoxSortMethod, "Cuteness", (int)SortMethods.Cuteness);
			AddComboBoxItem(comboBoxSortMethod, "Smartness", (int)SortMethods.Smartness);
			AddComboBoxItem(comboBoxSortMethod, "Toughness", (int)SortMethods.Toughness);
			AddComboBoxItem(comboBoxSortMethod, "Feel", (int)SortMethods.Feel);

			comboBoxSortOrder.Items.Clear();
			AddComboBoxItem(comboBoxSortOrder, "Highest to Lowest", (int)SortOrders.HighestToLowest);
			AddComboBoxItem(comboBoxSortOrder, "Lowest to Highest", (int)SortOrders.LowestToHighest);

			comboBoxSearchMode.Items.Clear();
			AddComboBoxItem(comboBoxSearchMode, "New Search", (int)SearchModes.NewSearch);
			AddComboBoxItem(comboBoxSearchMode, "Add Results", (int)SearchModes.AddResults);
			AddComboBoxItem(comboBoxSearchMode, "Refine Results", (int)SearchModes.RefineResults);

			loaded = oldLoaded;
		}

		private void AddComparisonComboBoxItems(ComboBox comboBox) {
			comboBox.Items.Clear();
			AddComboBoxItem(comboBox, "Equal To", (int)ComparisonTypes.Equal);
			AddComboBoxItem(comboBox, "Not Equal To", (int)ComparisonTypes.NotEqual);
			AddComboBoxItem(comboBox, "Greater Than", (int)ComparisonTypes.GreaterThan);
			AddComboBoxItem(comboBox, "Less Than", (int)ComparisonTypes.LessThan);
		}

		private void AddComboBoxItem(ComboBox comboBox, string name, int tag) {
			ComboBoxItem item = new ComboBoxItem();
			item.Content = name;
			item.Tag = tag;
			comboBox.Items.Add(item);
		}


		private int GetComboBoxValue(ComboBox comboBox) {
			return (int)(comboBox.SelectedItem as ComboBoxItem).Tag;
		}

		private void LoadSearchSettings() {
			bool oldLoaded = loaded;
			loaded = false;

			#region Pokemon

			checkBoxSpecies.IsChecked = search.SpeciesEnabled;
			SetComboBoxToTag(comboBoxSpecies, (int)search.SpeciesValue);

			checkBoxType.IsChecked = search.TypeEnabled;
			SetComboBoxToTag(comboBoxType, (int)search.TypeValue);

			checkBoxNature.IsChecked = search.NatureEnabled;
			SetComboBoxToTag(comboBoxNature, (int)search.NatureValue);

			checkBoxAbility.IsChecked = search.AbilityEnabled;
			SetComboBoxToTag(comboBoxAbility, (int)search.AbilityValue);

			checkBoxHeldItem.IsChecked = search.HeldItemEnabled;
			SetComboBoxToTag(comboBoxHeldItem, (int)search.HeldItemValue);

			checkBoxPokeBall.IsChecked = search.PokeBallEnabled;
			SetComboBoxToTag(comboBoxPokeBall, (int)search.PokeBallValue);

			checkBoxRibbon.IsChecked = search.RibbonEnabled;
			SetComboBoxToTag(comboBoxRibbon, (int)search.RibbonValue);

			checkBoxPokerus.IsChecked = search.PokerusEnabled;
			SetComboBoxToTag(comboBoxPokerus, (int)search.PokerusValue);

			checkBoxGender.IsChecked = search.GenderEnabled;
			SetComboBoxToTag(comboBoxGender, (int)search.GenderValue);

			checkBoxHatchCounter.IsChecked = search.HatchCounterEnabled;
			SetComboBoxToTag(comboBoxHatchCounterComparison, (int)search.HatchCounterComparison);
			numericHatchCounterValue.Value = (int)search.HatchCounterValue;

			#endregion

			#region Stats

			checkBoxStat.IsChecked = search.StatEnabled;
			SetComboBoxToTag(comboBoxStatType, (int)search.Stat);
			SetComboBoxToTag(comboBoxStatComparison, (int)search.StatComparison);
			numericStatValue.Value = (int)search.StatValue;

			checkBoxCondition.IsChecked = search.ConditionEnabled;
			SetComboBoxToTag(comboBoxConditionType, (int)search.Condition);
			SetComboBoxToTag(comboBoxConditionComparison, (int)search.ConditionComparison);
			numericConditionValue.Value = (int)search.ConditionValue;

			checkBoxIV.IsChecked = search.IVEnabled;
			SetComboBoxToTag(comboBoxIVType, (int)search.IV);
			SetComboBoxToTag(comboBoxIVComparison, (int)search.IVComparison);
			numericIVValue.Value = (int)search.IVValue;

			checkBoxEV.IsChecked = search.EVEnabled;
			SetComboBoxToTag(comboBoxEVType, (int)search.EV);
			SetComboBoxToTag(comboBoxEVComparison, (int)search.EVComparison);
			numericEVValue.Value = (int)search.EVValue;

			checkBoxLevel.IsChecked = search.LevelEnabled;
			SetComboBoxToTag(comboBoxLevelComparison, (int)search.LevelComparison);
			numericLevelValue.Value = (int)search.LevelValue;

			checkBoxFriendship.IsChecked = search.FriendshipEnabled;
			SetComboBoxToTag(comboBoxFriendshipComparison, (int)search.FriendshipComparison);
			numericFriendshipValue.Value = (int)search.FriendshipValue;

			#endregion

			#region Moves

			checkBoxMoves.IsChecked = search.MovesEnabled;
			SetComboBoxToTag(comboBoxMove1, (int)search.Move1Value);
			SetComboBoxToTag(comboBoxMove2, (int)search.Move2Value);
			SetComboBoxToTag(comboBoxMove3, (int)search.Move3Value);
			SetComboBoxToTag(comboBoxMove4, (int)search.Move4Value);

			checkBoxHiddenPowerDamage.IsChecked = search.HiddenPowerDamageEnabled;
			SetComboBoxToTag(comboBoxHiddenPowerDamageComparison, (int)search.HiddenPowerDamageComparison);
			numericHiddenPowerDamageValue.Value = (int)search.HiddenPowerDamageValue;

			checkBoxHiddenPowerType.IsChecked = search.HiddenPowerTypeEnabled;
			SetComboBoxToTag(comboBoxHiddenPowerType, (int)search.HiddenPowerTypeValue);

			checkBoxEggGroup.IsChecked = search.EggGroupEnabled;
			SetComboBoxToTag(comboBoxEggGroup, (int)search.EggGroupValue);

			#endregion

			#region Misc

			SetComboBoxToTag(comboBoxEggMode, (int)search.EggMode);
			SetComboBoxToTag(comboBoxShinyMode, (int)search.ShinyMode);
			SetComboBoxToTag(comboBoxShadowMode, (int)search.ShadowMode);

			#endregion

			#region Search

			SetComboBoxToTag(comboBoxSortMethod, (int)search.SortMethod);
			SetComboBoxToTag(comboBoxSortOrder, (int)search.SortOrder);
			SetComboBoxToTag(comboBoxSearchMode, (int)search.SearchMode);

			checkBoxGame.IsChecked = search.GameEnabled;
			comboBoxGame.SelectedGameIndex = search.GameIndex;

			#endregion

			loaded = oldLoaded;
		}

		private void OnResetClicked(object sender, RoutedEventArgs e) {
			search = new PokemonSearchSettings();
			LoadSearchSettings();
		}

		private void SetComboBoxToTag(ComboBox comboBox, int tag) {
			foreach (object item in comboBox.Items) {
				if ((int)(item as ComboBoxItem).Tag == tag) {
					comboBox.SelectedItem = item;
					return;
				}
			}
			comboBox.SelectedIndex = 0;
		}

		private void OnCloseClicked(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnSearchClicked(object sender, RoutedEventArgs e) {
			PokemonSearch searcher = new PokemonSearch();
			searcher.Search = search;
			searcher.Results = results;
			searcher.SearchPokemon();
			if (ResultsWindow != null && !ResultsWindow.IsClosed)
				ResultsWindow.ShowResults(searcher.Results);
			else
				ResultsWindow = PokemonSearchResults.Show(Owner, searcher.Results);
		}

		private void OnSpeciesChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.SpeciesEnabled = checkBoxSpecies.IsChecked.Value;
		}
		private void OnPokerusChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.PokerusEnabled = checkBoxPokerus.IsChecked.Value;
		}
		private void OnNatureChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.NatureEnabled = checkBoxNature.IsChecked.Value;
		}
		private void OnAbilityChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.AbilityEnabled = checkBoxAbility.IsChecked.Value;
		}
		private void OnHeldItemChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.HeldItemEnabled = checkBoxHeldItem.IsChecked.Value;
		}
		private void OnPokeBallChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.PokeBallEnabled = checkBoxPokeBall.IsChecked.Value;
		}

		private void OnStatChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.StatEnabled = checkBoxStat.IsChecked.Value;
		}
		private void OnConditionChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.ConditionEnabled = checkBoxCondition.IsChecked.Value;
		}
		private void OnIVChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.IVEnabled = checkBoxIV.IsChecked.Value;
		}
		private void OnEVChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.EVEnabled = checkBoxEV.IsChecked.Value;
		}
		private void OnLevelChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.LevelEnabled = checkBoxLevel.IsChecked.Value;
		}
		private void OnFriendshipChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.FriendshipEnabled = checkBoxFriendship.IsChecked.Value;
		}

		private void OnMovesChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.MovesEnabled = checkBoxMoves.IsChecked.Value;
		}
		private void OnHiddenPowerDamageChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.HiddenPowerDamageEnabled = checkBoxHiddenPowerDamage.IsChecked.Value;
		}
		private void OnHiddenPowerTypeChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.HiddenPowerTypeEnabled = checkBoxHiddenPowerType.IsChecked.Value;
		}
		private void OnEggGroupChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.EggGroupEnabled = checkBoxEggGroup.IsChecked.Value;
		}


		private void OnSpeciesChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.SpeciesValue = (ushort)GetComboBoxValue(comboBoxSpecies);
			search.SpeciesEnabled = true;
			checkBoxSpecies.IsChecked = true;
		}
		private void OnPokerusChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.PokerusValue = (PokerusStatuses)GetComboBoxValue(comboBoxPokerus);
			search.PokerusEnabled = true;
			checkBoxPokerus.IsChecked = true;
		}
		private void OnNatureChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.NatureValue = (byte)GetComboBoxValue(comboBoxNature);
			search.NatureEnabled = true;
			checkBoxNature.IsChecked = true;
		}
		private void OnAbilityChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.AbilityValue = (byte)GetComboBoxValue(comboBoxAbility);
			search.AbilityEnabled = true;
			checkBoxAbility.IsChecked = true;
		}
		private void OnHeldItemChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.HeldItemValue = (ushort)GetComboBoxValue(comboBoxHeldItem);
			search.HeldItemEnabled = true;
			checkBoxHeldItem.IsChecked = true;
		}
		private void OnPokeBallChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.PokeBallValue = (byte)GetComboBoxValue(comboBoxPokeBall);
			search.PokeBallEnabled = true;
			checkBoxPokeBall.IsChecked = true;
		}

		private void OnStatComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.StatComparison = (ComparisonTypes)GetComboBoxValue(comboBoxStatComparison);
			search.StatEnabled = true;
			checkBoxStat.IsChecked = true;
		}

		private void OnConditionComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.ConditionComparison = (ComparisonTypes)GetComboBoxValue(comboBoxConditionComparison);
			search.ConditionEnabled = true;
			checkBoxCondition.IsChecked = true;
		}

		private void OnIVComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.IVComparison = (ComparisonTypes)GetComboBoxValue(comboBoxIVComparison);
			search.IVEnabled = true;
			checkBoxIV.IsChecked = true;
		}

		private void OnEVComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.EVComparison = (ComparisonTypes)GetComboBoxValue(comboBoxEVComparison);
			search.EVEnabled = true;
			checkBoxEV.IsChecked = true;
		}

		private void OnLevelComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.LevelComparison = (ComparisonTypes)GetComboBoxValue(comboBoxLevelComparison);
			search.LevelEnabled = true;
			checkBoxLevel.IsChecked = true;
		}

		private void OnFriendshipComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.FriendshipComparison = (ComparisonTypes)GetComboBoxValue(comboBoxFriendshipComparison);
			search.FriendshipEnabled = true;
			checkBoxFriendship.IsChecked = true;
		}

		private void OnHiddenPowerDamageComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.HiddenPowerDamageComparison = (ComparisonTypes)GetComboBoxValue(comboBoxHiddenPowerDamageComparison);
			search.HiddenPowerDamageEnabled = true;
			checkBoxHiddenPowerDamage.IsChecked = true;
		}

		private void OnStatTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.Stat = (StatTypes)GetComboBoxValue(comboBoxStatType);
			search.StatEnabled = true;
			checkBoxStat.IsChecked = true;
		}

		private void OnConditionTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.Condition = (ConditionTypes)GetComboBoxValue(comboBoxConditionType);
			search.ConditionEnabled = true;
			checkBoxCondition.IsChecked = true;
		}

		private void OnIVTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.IV = (StatTypes)GetComboBoxValue(comboBoxIVType);
			search.IVEnabled = true;
			checkBoxIV.IsChecked = true;
		}

		private void OnEVTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.EV = (StatTypes)GetComboBoxValue(comboBoxEVType);
			search.EVEnabled = true;
			checkBoxEV.IsChecked = true;
		}

		private void OnStatValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.StatValue = (uint)numericStatValue.Value;
			search.StatEnabled = true;
			checkBoxStat.IsChecked = true;
		}

		private void OnConditionValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.ConditionValue = (uint)numericConditionValue.Value;
			search.ConditionEnabled = true;
			checkBoxCondition.IsChecked = true;
		}

		private void OnIVValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.IVValue = (uint)numericIVValue.Value;
			search.IVEnabled = true;
			checkBoxIV.IsChecked = true;
		}

		private void OnEVValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.EVValue = (uint)numericEVValue.Value;
			search.EVEnabled = true;
			checkBoxEV.IsChecked = true;
		}

		private void OnLevelValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.LevelValue = (byte)numericLevelValue.Value;
			search.LevelEnabled = true;
			checkBoxLevel.IsChecked = true;
		}

		private void OnFriendshipValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.FriendshipValue = (byte)numericFriendshipValue.Value;
			search.FriendshipEnabled = true;
			checkBoxFriendship.IsChecked = true;
		}

		private void OnMoveChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			ushort move = (ushort)GetComboBoxValue(sender as ComboBox);
			int tag = int.Parse((sender as ComboBox).Tag as string);
			switch (tag) {
			case 1: search.Move1Value = move; break;
			case 2: search.Move2Value = move; break;
			case 3: search.Move3Value = move; break;
			case 4: search.Move4Value = move; break;
			}
			search.MovesEnabled = true;
			checkBoxMoves.IsChecked = true;
		}

		private void OnHiddenPowerTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.HiddenPowerTypeValue = (PokemonTypes)GetComboBoxValue(comboBoxHiddenPowerType);
			search.HiddenPowerTypeEnabled = true;
			checkBoxHiddenPowerType.IsChecked = true;
		}

		private void OnEggGroupChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.EggGroupValue = (EggGroups)GetComboBoxValue(comboBoxEggGroup);
			search.EggGroupEnabled = true;
			checkBoxEggGroup.IsChecked = true;
		}

		private void OnHiddenPowerDamageValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.HiddenPowerDamageValue = (byte)numericHiddenPowerDamageValue.Value;
			search.HiddenPowerDamageEnabled = true;
			checkBoxHiddenPowerDamage.IsChecked = true;
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (!loaded) return;
			loaded = false;
			if (ResultsWindow != null && !ResultsWindow.IsClosed)
				ResultsWindow.Close();
			PokeManager.ManagerWindow.PokemonSearchWindow = null;
			IsClosed = true;
			PokeManager.ManagerWindow.Focus();
		}

		private void OnSortMethodSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.SortMethod = (SortMethods)GetComboBoxValue(comboBoxSortMethod);
		}

		private void OnSortOrderChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.SortOrder = (SortOrders)GetComboBoxValue(comboBoxSortOrder);
		}

		private void OnSearchModeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.SearchMode = (SearchModes)GetComboBoxValue(comboBoxSearchMode);
		}

		private void OnGameChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.GameEnabled = checkBoxGame.IsChecked.Value;
		}

		private void OnGameChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.GameIndex = comboBoxGame.SelectedGameIndex;
			search.GameEnabled = true;
			checkBoxGame.IsChecked = true;
		}

		private void OnTypeChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.TypeEnabled = checkBoxType.IsChecked.Value;
		}

		private void OnRibbonChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.RibbonEnabled = checkBoxRibbon.IsChecked.Value;
		}

		private void OnTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.TypeValue = (PokemonTypes)GetComboBoxValue(comboBoxType);
			search.TypeEnabled = true;
			checkBoxType.IsChecked = true;
		}

		private void OnRibbonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.RibbonValue = (PokemonRibbons)GetComboBoxValue(comboBoxRibbon);
			search.RibbonEnabled = true;
			checkBoxRibbon.IsChecked = true;
		}

		private void OnEggModeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.EggMode = (EggModes)GetComboBoxValue(comboBoxEggMode);
		}

		private void OnHatchCounterChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.HatchCounterEnabled = checkBoxHatchCounter.IsChecked.Value;
		}

		private void OnHatchCounterComparisonChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.HatchCounterComparison = (ComparisonTypes)GetComboBoxValue(comboBoxHatchCounterComparison);
			search.HatchCounterEnabled = true;
			checkBoxHatchCounter.IsChecked = true;
		}

		private void OnHatchCounterValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.HatchCounterValue = (byte)numericHatchCounterValue.Value;
			search.HatchCounterEnabled = true;
			checkBoxHatchCounter.IsChecked = true;
		}

		private void OnShinyModeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.ShinyMode = (ShinyModes)GetComboBoxValue(comboBoxShinyMode);
		}

		private void OnShadowModeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.ShadowMode = (ShadowModes)GetComboBoxValue(comboBoxShadowMode);
		}

		private void OnGenderChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			search.GenderEnabled = checkBoxGender.IsChecked.Value;
		}

		private void OnGenderChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;
			search.GenderValue = (Genders)GetComboBoxValue(comboBoxGender);
			search.GenderEnabled = true;
			checkBoxGender.IsChecked = true;
		}
	}
}
