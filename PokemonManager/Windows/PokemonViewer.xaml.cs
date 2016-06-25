using PokemonManager.Game;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.PokemonStructures.Events;
using PokemonManager.Util;
using PokemonManager.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for PokemonViewer.xaml
	/// </summary>
	public partial class PokemonViewer : UserControl, IPokemonViewer {

		private IPokemon pokemon;
		private int currentMoveIndex;
		private int gameIndex;
		private MediaPlayer playerCry;

		public PokemonViewer() {
			InitializeComponent();

			playerCry = new MediaPlayer();
			DisableEditing = false;

			for (int i = 1; ; i++) {
				ToggleButton button = this.FindName("buttonSettings" + i.ToString()) as ToggleButton;
				Grid grid = FindName("gridSettings" + i.ToString()) as Grid;

				if (button == null)
					break;

				button.IsChecked = (i == 1);
				grid.Visibility = (i == 1 ? Visibility.Visible : Visibility.Hidden);
			}
		}

		public IPokemon ViewedPokemon {
			get { return pokemon; }
		}

		public bool DisableEditing { get; set; }

		public void RefreshUI() {
			if (pokemon != null)
				LoadPokemon(pokemon);
			RefreshUISideParts();
		}

		public void RefreshUISideParts() {
			foreach (TabItem tab in tabControl.Items) {
				((PokemonViewerSidePart)((Grid)tab.Content).Children[0]).RefreshUI();
			}
		}
		public void LoadSideParts() {
			foreach (TabItem tab in tabControl.Items) {
				((PokemonViewerSidePart)((Grid)tab.Content).Children[0]).LoadPokemon(pokemon);
			}
		}
		public void UnloadSideParts() {
			foreach (TabItem tab in tabControl.Items) {
				((PokemonViewerSidePart)((Grid)tab.Content).Children[0]).UnloadPokemon();
			}
		}

		public void UnloadPokemon() {
			this.currentMoveIndex = -1;
			this.pokemon = null;

			UnloadSideParts();

			this.labelNickname.Content = "";
			this.labelLevel.Content = "";
			this.labelGender.Content = "";
			this.labelOTName.Content = "";
			this.labelOTID.Content = "";
			this.labelSecretID.Content = "";
			this.labelMetAtLevel.Content = "";
			this.labelMetAtLocation.Content = "";
			this.labelGame.Content = "";

			this.labelNature.Content = "";
			this.labelNatureRaised.Content = "";
			this.labelNatureLowered.Content = "";
			this.labelHeldItem.Content = "";
			this.imageHeldItem.Source = null;
			this.labelSpeciesName.Content = "";
			this.labelSpeciesNumber.Content = "No.";
			this.labelEggGroup1.Content = "";
			this.labelEggGroup2.Content = "";


			this.labelHPIV.Content			= "0";
			this.labelAttackIV.Content		= "0";
			this.labelDefenseIV.Content		= "0";
			this.labelSpAttackIV.Content	= "0";
			this.labelSpDefenseIV.Content	= "0";
			this.labelSpeedIV.Content		= "0";
			this.labelIVTotal.Content		= "0";

			this.labelHPEV.Content			= "0";
			this.labelAttackEV.Content		= "0";
			this.labelDefenseEV.Content		= "0";
			this.labelSpAttackEV.Content	= "0";
			this.labelSpDefenseEV.Content	= "0";
			this.labelSpeedEV.Content		= "0";
			this.labelEVTotal.Content		= "0";

			this.labelHPStat.Content		= "0";
			this.labelAttackStat.Content	= "0";
			this.labelDefenseStat.Content	= "0";
			this.labelSpAttackStat.Content	= "0";
			this.labelSpDefenseStat.Content	= "0";
			this.labelSpeedStat.Content		= "0";
			this.labelStatTotal.Content		= "0";

			this.labelCool.Content			= "0";
			this.labelBeauty.Content		= "0";
			this.labelCute.Content			= "0";
			this.labelSmart.Content			= "0";
			this.labelTough.Content			= "0";
			this.labelFeel.Content			= "0";
			this.labelConditionTotal.Content = "0";

			this.labelMove1Name.Content = "-";
			this.labelMove1PP.Content = "--";
			this.typeMove1.Visibility = Visibility.Hidden;
			this.labelContestMove1Name.Content = "-";
			this.labelContestMove1PP.Content = "--";
			this.conditionMove1.Visibility = Visibility.Hidden;
			this.labelMove2Name.Content = "-";
			this.labelMove2PP.Content = "--";
			this.typeMove2.Visibility = Visibility.Hidden;
			this.labelContestMove2Name.Content = "-";
			this.labelContestMove2PP.Content = "--";
			this.conditionMove2.Visibility = Visibility.Hidden;
			this.labelMove3Name.Content = "-";
			this.labelMove3PP.Content = "--";
			this.typeMove3.Visibility = Visibility.Hidden;
			this.labelContestMove3Name.Content = "-";
			this.labelContestMove3PP.Content = "--";
			this.conditionMove3.Visibility = Visibility.Hidden;
			this.labelMove4Name.Content = "-";
			this.labelMove4PP.Content = "--";
			this.typeMove4.Visibility = Visibility.Hidden;
			this.labelContestMove4Name.Content = "-";
			this.labelContestMove4PP.Content = "--";
			this.conditionMove4.Visibility = Visibility.Hidden;

			this.labelExperienceText.Content = "Experience";
			this.labelNextLevelText.Content = "Next Level";
			this.labelExperienceText2.Content = "";
			this.labelTotalExperience2.Content = "";
			this.labelTotalExperience.Content = "";
			this.labelNextLevel.Content = "";
			this.rectExperienceBar.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100));

			this.rectPurificationBorder1.Visibility = Visibility.Hidden;
			this.rectPurificationBorder2.Visibility = Visibility.Hidden;

			this.labelPokerus.Content = "";


			this.type1Pokemon.Type = PokemonTypes.None;
			//this.type2Pokemon.Type = PokemonTypes.None;
			this.type1Pokemon.Visibility = Visibility.Hidden;
			this.type2Pokemon.Visibility = Visibility.Hidden;

			stackPanelRibbons.Children.Clear();

			this.labelFriendship.Content = "";
			this.labelFriendshipText.Content = "Friendship";
			this.labelMetAtText.Content = "Met At";
			this.labelAbility.Content = "";
			this.textBlockAbilityDescription.Text = "";
			this.textBlockContestMoveDescription.Text = "";
			this.textBlockMoveDescription.Text = "";
			this.textBlockRibbonDescription.Text = "";
			this.labelMoveJam.Content = "";
			this.labelMoveAppeal.Content = "";
			this.labelMoveCategory.Content = "";
			this.labelMovePower.Content = "";
			this.labelMoveAccuracy.Content = "";

			buttonBall.IsEnabled = false;
			buttonNickname.IsEnabled = false;
			buttonDeoxys.IsEnabled = false;
			buttonWipeEVs.IsEnabled = false;
			buttonRoundEVs.IsEnabled = false;

			buttonMarkings.IsEnabled = false;
			buttonMoves.IsEnabled = false;
			buttonGive.IsEnabled = false;
			buttonEvolve.IsEnabled = false;
			buttonRelearnMove.IsEnabled = false;
			buttonTeachTM.IsEnabled = false;
			buttonFixRoamingIVs.IsEnabled = false;
			buttonEffortRibbon.IsEnabled = false;
			buttonLevelDown.IsEnabled = false;
			buttonInfect.IsEnabled = false;

			buttonEVTraining.IsEnabled = false;

			this.typeHiddenPower.Visibility = Visibility.Hidden;
			this.labelHiddenPowerDamage.Content = "";
			this.labelPersonality.Content = "";
		}

		public void LoadPokemon(IPokemon pokemon) {
			if (pokemon == null) {
				UnloadPokemon();
				return;
			}
			if (pokemon.ContainerIndex == -1) {
				pokemon = pokemon.PokemonFinder.Pokemon;
			}
			this.pokemon = pokemon;

			LoadSideParts();

			if (pokemon.IsEgg) {
				this.labelNickname.Content = "EGG";
			}
			else {
				this.labelNickname.Content = pokemon.Nickname;
			}
			this.labelLevel.Content = "Lv " + pokemon.Level.ToString();
			if (pokemon.IsEgg && PokeManager.Settings.MysteryEggs) {
				this.labelGender.Content = "";
			}
			else if (pokemon.Gender == Genders.Male) {
				this.labelGender.Content = "♂";
				this.labelGender.Foreground = new SolidColorBrush(Color.FromRgb(0, 136, 184));
			}
			else if (pokemon.Gender == Genders.Female) {
				this.labelGender.Content = "♀";
				this.labelGender.Foreground = new SolidColorBrush(Color.FromRgb(184, 88, 80));
			}
			else {
				this.labelGender.Content = "";
			}

			if (pokemon.IsEgg) {
				this.labelOTName.Content = "?????";
				this.labelOTName.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				this.labelOTID.Content = "?????";
				this.labelSecretID.Content = "?????";
			}
			else {
				this.labelOTName.Content = pokemon.TrainerName;
				if (pokemon.TrainerGender == Genders.Male)
					this.labelOTName.Foreground = new SolidColorBrush(Color.FromRgb(0, 136, 184));
				else if (pokemon.TrainerGender == Genders.Female)
					this.labelOTName.Foreground = new SolidColorBrush(Color.FromRgb(184, 88, 80));
				this.labelOTID.Content = pokemon.TrainerID.ToString("00000");
				this.labelSecretID.Content = pokemon.SecretID.ToString("00000");
			}

			if (pokemon.HasForm)
				this.labelSpeciesName.Content = pokemon.PokemonFormData.Name;
			else
				this.labelSpeciesName.Content = pokemon.PokemonData.Name;
			if (pokemon.DexID == 265 && (!pokemon.IsEgg || !PokeManager.Settings.MysteryEggs))
				this.labelSpeciesName.Content += " " + (pokemon.WurpleIsCascoon ? "(Cas)" : "(Sil)");
			this.labelSpeciesNumber.Content = "No. " + pokemon.DexID.ToString("000");

			this.labelEggGroup1.Content = PokemonDatabase.GetEggGroupName(pokemon.PokemonData.EggGroup1);
			if (pokemon.PokemonData.HasTwoEggGroups)
				this.labelEggGroup2.Content = PokemonDatabase.GetEggGroupName(pokemon.PokemonData.EggGroup2);
			else
				this.labelEggGroup2.Content = "";

			if (pokemon.IsEgg && PokeManager.Settings.MysteryEggs) {
				string statString = "??";
				string statStringTotal = "???";
				this.labelHPIV.Content			= statString;
				this.labelAttackIV.Content		= statString;
				this.labelDefenseIV.Content		= statString;
				this.labelSpAttackIV.Content	= statString;
				this.labelSpDefenseIV.Content	= statString;
				this.labelSpeedIV.Content		= statString;
				this.labelIVTotal.Content		= statStringTotal;

				this.labelHPEV.Content			= statString;
				this.labelAttackEV.Content		= statString;
				this.labelDefenseEV.Content		= statString;
				this.labelSpAttackEV.Content	= statString;
				this.labelSpDefenseEV.Content	= statString;
				this.labelSpeedEV.Content		= statString;
				this.labelEVTotal.Content		= statStringTotal;

				this.labelHPStat.Content		= statString;
				this.labelAttackStat.Content	= statString;
				this.labelDefenseStat.Content	= statString;
				this.labelSpAttackStat.Content	= statString;
				this.labelSpDefenseStat.Content	= statString;
				this.labelSpeedStat.Content		= statString;
				this.labelStatTotal.Content		= statStringTotal;

				this.labelCool.Content			= statString;
				this.labelBeauty.Content		= statString;
				this.labelCute.Content			= statString;
				this.labelSmart.Content			= statString;
				this.labelTough.Content			= statString;
				this.labelFeel.Content			= statString;
				this.labelConditionTotal.Content = statStringTotal;

				this.typeHiddenPower.Visibility = Visibility.Visible;
				this.typeHiddenPower.Type = PokemonTypes.None;
				this.labelHiddenPowerDamage.Content = "??";

				this.labelPersonality.Content = "????????";

				this.labelNature.Content = "?????";
				this.labelNatureRaised.Content = "+???";
				this.labelNatureLowered.Content = "-???";

				this.labelAbility.Content = "?????";
				this.textBlockAbilityDescription.Text = "";

				this.labelPokerus.Content = "???";
				this.labelPokerus.ToolTip = null;
			}
			else {
				this.labelHPIV.Content			= pokemon.HPIV.ToString();
				this.labelAttackIV.Content		= pokemon.AttackIV.ToString();
				this.labelDefenseIV.Content		= pokemon.DefenseIV.ToString();
				this.labelSpAttackIV.Content	= pokemon.SpAttackIV.ToString();
				this.labelSpDefenseIV.Content	= pokemon.SpDefenseIV.ToString();
				this.labelSpeedIV.Content		= pokemon.SpeedIV.ToString();
				this.labelIVTotal.Content =
				((int)pokemon.HPIV + (int)pokemon.AttackIV + (int)pokemon.DefenseIV +
				(int)pokemon.SpAttackIV + (int)pokemon.SpDefenseIV + (int)pokemon.SpeedIV).ToString();

				this.labelHPEV.Content			= pokemon.HPEV.ToString();
				this.labelAttackEV.Content		= pokemon.AttackEV.ToString();
				this.labelDefenseEV.Content		= pokemon.DefenseEV.ToString();
				this.labelSpAttackEV.Content	= pokemon.SpAttackEV.ToString();
				this.labelSpDefenseEV.Content	= pokemon.SpDefenseEV.ToString();
				this.labelSpeedEV.Content		= pokemon.SpeedEV.ToString();
				this.labelEVTotal.Content =
				((int)pokemon.HPEV + (int)pokemon.AttackEV + (int)pokemon.DefenseEV +
				(int)pokemon.SpAttackEV + (int)pokemon.SpDefenseEV + (int)pokemon.SpeedEV).ToString();

				this.labelHPStat.Content		= pokemon.HP.ToString();
				this.labelAttackStat.Content	= pokemon.Attack.ToString();
				this.labelDefenseStat.Content	= pokemon.Defense.ToString();
				this.labelSpAttackStat.Content	= pokemon.SpAttack.ToString();
				this.labelSpDefenseStat.Content	= pokemon.SpDefense.ToString();
				this.labelSpeedStat.Content		= pokemon.Speed.ToString();
				this.labelStatTotal.Content =
				((int)pokemon.HP + (int)pokemon.Attack + (int)pokemon.Defense +
				(int)pokemon.SpAttack + (int)pokemon.SpDefense + (int)pokemon.Speed).ToString();

				this.labelCool.Content			= pokemon.Coolness.ToString();
				this.labelBeauty.Content		= pokemon.Beauty.ToString();
				this.labelCute.Content			= pokemon.Cuteness.ToString();
				this.labelSmart.Content			= pokemon.Smartness.ToString();
				this.labelTough.Content			= pokemon.Toughness.ToString();
				this.labelFeel.Content			= pokemon.Feel.ToString();
				this.labelConditionTotal.Content =
				((int)pokemon.Coolness + (int)pokemon.Beauty + (int)pokemon.Cuteness + (int)pokemon.Smartness + (int)pokemon.Toughness).ToString();

				this.typeHiddenPower.Visibility = Visibility.Visible;
				this.typeHiddenPower.Type = pokemon.HiddenPowerType;
				this.labelHiddenPowerDamage.Content = pokemon.HiddenPowerDamage.ToString();
				this.labelPersonality.Content = pokemon.Personality.ToString();

				this.labelNature.Content = pokemon.NatureData.Name;
				this.labelNatureRaised.Content = "+" + pokemon.NatureData.RaisedStat.ToString();
				this.labelNatureLowered.Content = "-" + pokemon.NatureData.LoweredStat.ToString();

				this.labelAbility.Content = pokemon.AbilityData.Name;
				this.textBlockAbilityDescription.Text = pokemon.AbilityData.Description;

				this.labelPokerus.Content = pokemon.PokerusStatus.ToString();

				if (pokemon.PokerusStatus == PokerusStatuses.None) {
					this.labelPokerus.ToolTip = null;
				}
				else if (pokemon.PokerusStatus != PokerusStatuses.Invalid) {
					ToolTip tooltip = new ToolTip();
					string content = "";
					content += "Days Remaining: " + pokemon.PokerusDaysRemaining.ToString();
					content += "\n" + new PokerusStrain(pokemon.PokerusStrain).ToString();

					tooltip.Content = content;
					this.labelPokerus.ToolTip = tooltip;
				}
			}

			if (pokemon.Move1ID != 0) {
				ushort moveID = pokemon.Move1ID;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.Colosseum)
					moveID = 355;
				else if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove1ID != 0)
					moveID = ((XDPokemon)pokemon).ShadowMove1ID;
				MoveData moveData = PokemonDatabase.GetMoveFromID(moveID);
				this.labelMove1Name.Content = moveData.Name;
				this.labelMove1PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move1TotalPP.ToString());
				this.typeMove1.Type = moveData.Type;
				this.typeMove1.Visibility = Visibility.Visible;
				this.labelContestMove1Name.Content = moveData.Name;
				this.labelContestMove1PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move1TotalPP.ToString());
				this.conditionMove1.Type = moveData.ConditionType;
				this.conditionMove1.Visibility = Visibility.Visible;
			}
			else {
				this.labelMove1Name.Content = "-";
				this.labelMove1PP.Content = "--";
				this.typeMove1.Visibility = Visibility.Hidden;
				this.labelContestMove1Name.Content = "-";
				this.labelContestMove1PP.Content = "--";
				this.conditionMove1.Visibility = Visibility.Hidden;
			}
			if (pokemon.Move2ID != 0) {
				ushort moveID = pokemon.Move2ID;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove2ID != 0)
					moveID = ((XDPokemon)pokemon).ShadowMove2ID;
				MoveData moveData = PokemonDatabase.GetMoveFromID(moveID);
				this.labelMove2Name.Content = moveData.Name;
				this.labelMove2PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move2TotalPP.ToString());
				this.typeMove2.Type = moveData.Type;
				this.typeMove2.Visibility = Visibility.Visible;
				this.labelContestMove2Name.Content = moveData.Name;
				this.labelContestMove2PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move2TotalPP.ToString());
				this.conditionMove2.Type = moveData.ConditionType;
				this.conditionMove2.Visibility = Visibility.Visible;
			}
			else {
				this.labelMove2Name.Content = "-";
				this.labelMove2PP.Content = "--";
				this.typeMove2.Visibility = Visibility.Hidden;
				this.labelContestMove2Name.Content = "-";
				this.labelContestMove2PP.Content = "--";
				this.conditionMove2.Visibility = Visibility.Hidden;
			}
			if (pokemon.Move3ID != 0) {
				ushort moveID = pokemon.Move3ID;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove3ID != 0)
					moveID = ((XDPokemon)pokemon).ShadowMove3ID;
				MoveData moveData = PokemonDatabase.GetMoveFromID(moveID);
				this.labelMove3Name.Content = moveData.Name;
				this.labelMove3PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move3TotalPP.ToString());
				this.typeMove3.Type = moveData.Type;
				this.typeMove3.Visibility = Visibility.Visible;
				this.labelContestMove3Name.Content = moveData.Name;
				this.labelContestMove3PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move3TotalPP.ToString());
				this.conditionMove3.Type = moveData.ConditionType;
				this.conditionMove3.Visibility = Visibility.Visible;
			}
			else {
				this.labelMove3Name.Content = "-";
				this.labelMove3PP.Content = "--";
				this.typeMove3.Visibility = Visibility.Hidden;
				this.labelContestMove3Name.Content = "-";
				this.labelContestMove3PP.Content = "--";
				this.conditionMove3.Visibility = Visibility.Hidden;
			}
			if (pokemon.Move4ID != 0) {
				ushort moveID = pokemon.Move4ID;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove4ID != 0)
					moveID = ((XDPokemon)pokemon).ShadowMove4ID;
				MoveData moveData = PokemonDatabase.GetMoveFromID(moveID);
				this.labelMove4Name.Content = moveData.Name;
				this.labelMove4PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move4TotalPP.ToString());
				this.typeMove4.Type = moveData.Type;
				this.typeMove4.Visibility = Visibility.Visible;
				this.labelContestMove4Name.Content = moveData.Name;
				this.labelContestMove4PP.Content = (moveData.PP == 0 ? "--" : pokemon.Move4TotalPP.ToString());
				this.conditionMove4.Type = moveData.ConditionType;
				this.conditionMove4.Visibility = Visibility.Visible;
			}
			else {
				this.labelMove4Name.Content = "-";
				this.labelMove4PP.Content = "--";
				this.typeMove4.Visibility = Visibility.Hidden;
				this.labelContestMove4Name.Content = "-";
				this.labelContestMove4PP.Content = "--";
				this.conditionMove4.Visibility = Visibility.Hidden;
			}

			if (pokemon.IsHoldingItem) {
				this.imageHeldItem.Source = ItemDatabase.GetItemImageFromID(pokemon.HeldItemID);
				this.labelHeldItem.Content = ItemDatabase.GetItemFromID(pokemon.HeldItemID).Name;
			}
			else {
				this.imageHeldItem.Source = null;
				this.labelHeldItem.Content = "None";
			}
			if (pokemon.IsEgg) {
				this.labelFriendshipText.Content = "Hatch Counter";
				this.labelFriendship.Content = pokemon.Friendship.ToString() + " Cycles";
			}
			else {
				this.labelFriendshipText.Content = "Friendship";
				this.labelFriendship.Content = pokemon.Friendship.ToString();
			}

			if (pokemon.IsShadowPokemon) {
				uint totalPurification = pokemon.HeartGauge;
				uint levelStartExp = PokemonDatabase.GetExperienceFromLevel(pokemon.PokemonData.ExperienceGroup, pokemon.Level);
				this.labelNextLevelText.Content = "Exp Stored";
				this.labelNextLevel.Content = pokemon.ExperienceStored.ToString() + " (+" + (PokemonDatabase.GetLevelFromExperience(pokemon.PokemonData.ExperienceGroup, pokemon.Experience + pokemon.ExperienceStored) - pokemon.Level).ToString() + ")";
				this.labelTotalExperience.Content = pokemon.Experience.ToString();
				this.labelExperienceText2.Content = "Heart Gauge";
				this.labelTotalExperience2.Content = pokemon.Purification.ToString();

				double ratio = (double)Math.Max(0, pokemon.Purification) / (double)totalPurification;
				LinearGradientBrush purBrush = new LinearGradientBrush();
				purBrush.GradientStops.Add(new GradientStop(Color.FromRgb(160, 72, 220), 0));
				purBrush.GradientStops.Add(new GradientStop(Color.FromRgb(160, 72, 220), ratio));
				purBrush.GradientStops.Add(new GradientStop(Color.FromRgb(225, 235, 250), ratio));
				purBrush.GradientStops.Add(new GradientStop(Color.FromRgb(225, 235, 250), 1));
				this.rectExperienceBar.Fill = purBrush;

				this.rectPurificationBorder1.Visibility = Visibility.Visible;
				this.rectPurificationBorder2.Visibility = Visibility.Visible;
			}
			else {
				this.labelExperienceText.Content = "Experience";
				this.labelNextLevelText.Content = "Next Level";
				this.labelExperienceText2.Content = "";
				this.labelTotalExperience2.Content = "";
				uint levelStartExp = PokemonDatabase.GetExperienceFromLevel(pokemon.PokemonData.ExperienceGroup, pokemon.Level);
				uint levelNextExp = PokemonDatabase.GetExperienceFromLevel(pokemon.PokemonData.ExperienceGroup, (byte)Math.Min(100, (int)pokemon.Level + 1));
				this.labelTotalExperience.Content = pokemon.Experience.ToString();
				this.labelNextLevel.Content = (levelNextExp - levelStartExp).ToString();

				double ratio = (double)(pokemon.Experience - levelStartExp) / (double)(levelNextExp - levelStartExp);
				LinearGradientBrush expBrush = new LinearGradientBrush();
				expBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 160, 220), 0));
				expBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 160, 220), ratio));
				expBrush.GradientStops.Add(new GradientStop(Color.FromRgb(100, 100, 100), ratio));
				expBrush.GradientStops.Add(new GradientStop(Color.FromRgb(100, 100, 100), 1));
				this.rectExperienceBar.Fill = expBrush;

				this.rectPurificationBorder1.Visibility = Visibility.Hidden;
				this.rectPurificationBorder2.Visibility = Visibility.Hidden;
			}

			if (pokemon.IsEgg) {
				this.labelMetAtText.Content = "Waiting to Hatch";
				this.labelMetAtLevel.Content = "";
				this.labelMetAtLocation.Content = "";
				this.labelGame.Content = "";
			}
			else {
				if (pokemon.LevelMet == 0) {
					this.labelMetAtText.Content = "Hatched At";
					this.labelMetAtLevel.Content = "Lv 5";
				}
				else {
					this.labelMetAtText.Content = (pokemon.IsFatefulEncounter ? "Fateful Encounter At" : "Met At");
					this.labelMetAtLevel.Content = "Lv " + pokemon.LevelMet.ToString();
				}
				this.labelMetAtLocation.Content = (pokemon.GameOrigin == GameOrigins.ColosseumXD ? "Orre Region" : PokemonDatabase.GetMetLocationFromID(pokemon.MetLocationID));
				this.labelGame.Content = pokemon.GameOrigin.ToString() + " (" + pokemon.Language.ToString() + ")";
			}

			this.type1Pokemon.Type = pokemon.PokemonData.Type1;
			this.type1Pokemon.Visibility = Visibility.Visible;
			if (pokemon.PokemonData.HasTwoTypes) {
				this.type2Pokemon.Type = pokemon.PokemonData.Type2;
				this.type2Pokemon.Visibility = Visibility.Visible;
			}
			else {
				this.type2Pokemon.Visibility = Visibility.Hidden;
			}

			stackPanelRibbons.Children.Clear();
			for (int i = 0; i < pokemon.CoolRibbonCount; i++)
				AddRibbon("COOL-" + (i + 1).ToString());
			for (int i = 0; i < pokemon.BeautyRibbonCount; i++)
				AddRibbon("BEAUTY-" + (i + 1).ToString());
			for (int i = 0; i < pokemon.CuteRibbonCount; i++)
				AddRibbon("CUTE-" + (i + 1).ToString());
			for (int i = 0; i < pokemon.SmartRibbonCount; i++)
				AddRibbon("SMART-" + (i + 1).ToString());
			for (int i = 0; i < pokemon.ToughRibbonCount; i++)
				AddRibbon("TOUGH-" + (i + 1).ToString());
			/*for (int i = 0; i < 4; i++)
				AddRibbon("COOL-" + (i + 1).ToString());
			for (int i = 0; i < 4; i++)
				AddRibbon("BEAUTY-" + (i + 1).ToString());
			for (int i = 0; i < 4; i++)
				AddRibbon("CUTE-" + (i + 1).ToString());
			for (int i = 0; i < 4; i++)
				AddRibbon("SMART-" + (i + 1).ToString());
			for (int i = 0; i < 4; i++)
				AddRibbon("TOUGH-" + (i + 1).ToString());*/
			/*AddRibbon("CHAMPION");
			AddRibbon("WINNING");
			AddRibbon("VICTORY");
			AddRibbon("ARTIST");
			AddRibbon("EFFORT");
			AddRibbon("MARINE");
			AddRibbon("LAND");
			AddRibbon("SKY");
			AddRibbon("COUNTRY");
			AddRibbon("NATIONAL");
			AddRibbon("EARTH");
			AddRibbon("WORLD");*/
			if (pokemon.HasChampionRibbon) AddRibbon("CHAMPION");
			if (pokemon.HasWinningRibbon) AddRibbon("WINNING");
			if (pokemon.HasVictoryRibbon) AddRibbon("VICTORY");
			if (pokemon.HasArtistRibbon) AddRibbon("ARTIST");
			if (pokemon.HasEffortRibbon) AddRibbon("EFFORT");
			if (pokemon.HasMarineRibbon) AddRibbon("MARINE");
			if (pokemon.HasLandRibbon) AddRibbon("LAND");
			if (pokemon.HasSkyRibbon) AddRibbon("SKY");
			if (pokemon.HasCountryRibbon) AddRibbon("COUNTRY");
			if (pokemon.HasNationalRibbon) AddRibbon("NATIONAL");
			if (pokemon.HasEarthRibbon) AddRibbon("EARTH");
			if (pokemon.HasWorldRibbon) AddRibbon("WORLD");

			buttonBall.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg;
			buttonNickname.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon;
			buttonDeoxys.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && pokemon.DexID == 386;
			buttonWipeEVs.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon &&
				((int)pokemon.HPEV + (int)pokemon.AttackEV + (int)pokemon.DefenseEV +
				(int)pokemon.SpAttackEV + (int)pokemon.SpDefenseEV + (int)pokemon.SpeedEV) > 0;
			buttonRoundEVs.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon &&
				(pokemon.HPEV > 252 || pokemon.AttackEV > 252 || pokemon.DefenseEV > 252 ||
				pokemon.SpAttackEV > 252 || pokemon.SpDefenseEV > 252 || pokemon.SpeedEV > 252);

			buttonMarkings.IsEnabled = !DisableEditing && !pokemon.IsInDaycare;
			buttonMoves.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon;
			if (pokemon.IsHoldingItem) {
				buttonGive.Content = "Take Item";
				buttonGive.IsEnabled = !DisableEditing && !pokemon.IsInDaycare;
			}
			else {
				buttonGive.Content = "Give Item";
				buttonGive.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg;
			}
			bool evolutionLevelUp = PokemonDatabase.GetEvolutionOnLevelUp(pokemon) != null;
			bool evolutionNow = PokemonDatabase.GetEvolutionNow(pokemon) != null;
			bool evolutionItem = PokemonDatabase.GetEvolutionsOnItem(pokemon).Length > 0;

			int evolutionTypeCount = 0;
			if (evolutionLevelUp) evolutionTypeCount++;
			if (evolutionNow) evolutionTypeCount++;
			if (evolutionItem) evolutionTypeCount++;

			buttonEvolve.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon;
			if (evolutionTypeCount > 1 || evolutionLevelUp) {
				buttonEvolve.Content = "Evolve";
			}
			else if (evolutionNow) {
				buttonEvolve.Content = "Trade Evolve";
			}
			else if (evolutionItem) {
				buttonEvolve.Content = "Item Evolve";
			}
			else {
				buttonEvolve.IsEnabled = false;
				buttonEvolve.Content = "Evolve";
			}
			buttonRelearnMove.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon && PokemonDatabase.GetRelearnableMoves(pokemon).Length > 0;
			buttonTeachTM.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon && PokemonDatabase.GetTeachableMachineMoveItems(pokemon).Length > 0;
			buttonFixRoamingIVs.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon && PokemonDatabase.IsPokemonWithRoamingIVGlitch(pokemon);
			buttonEffortRibbon.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && 
				((int)pokemon.HPEV + (int)pokemon.AttackEV + (int)pokemon.DefenseEV +
				(int)pokemon.SpAttackEV + (int)pokemon.SpDefenseEV + (int)pokemon.SpeedEV) >= 510 && !pokemon.HasEffortRibbon;
			buttonInfect.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && pokemon.PokerusStatus == PokerusStatuses.None && PokeManager.PokerusStrains.Count > 0;

			ushort[] validLevelDownDexIDs = new ushort[]{
				/*Ditto*/ 132, /*Legendary Birds*/ 144, 145, 146, /*Mews*/ 150, 151,
				/*Unown*/ 201, /*Legendary Dogs*/ 243, 244, 245, /*Lugia/Ho-Oh*/ 249, 250, /*Celebi*/ 251,
				/*Regis*/ 377, 378, 379, /*Latios/as*/ 380, 381, /*Weather Trio*/ 382, 383, 384, /*Jirachi/Deoxys*/ 385, 386
			};
			bool isValidLevelDownDexID = false;
			ushort dexID = pokemon.DexID;
			foreach (ushort levelDownDexID in validLevelDownDexIDs) {
				if (dexID == levelDownDexID) {
					isValidLevelDownDexID = true;
					break;
				}
			}
			buttonLevelDown.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon && isValidLevelDownDexID && pokemon.Level > 5;

			buttonEVTraining.IsEnabled = !DisableEditing && !pokemon.IsInDaycare && !pokemon.IsEgg && !pokemon.IsShadowPokemon && pokemon.Level > 95;

			SetCurrentMove(-1);
			Random random = new Random((int)DateTime.Now.Ticks);
		}

		private void AddRibbon(string id) {
			Image image = new Image();
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;
			image.Source = ResourceDatabase.GetImageFromName("Ribbon" + id);// PokemonDatabase.GetRibbonImageFromID(id);
			image.IsHitTestVisible = false;
			StackPanel stackPanelImage = new StackPanel();
			stackPanelImage.Orientation = Orientation.Horizontal;
			stackPanelImage.Children.Add(image);
			stackPanelImage.MouseLeftButtonDown += ribbon_MouseLeftButtonDown;
			stackPanelImage.Background = new SolidColorBrush(Color.FromRgb(244, 244, 245));
			stackPanelImage.Tag = id;

			if (stackPanelRibbons.Children.Count == 0 || ((StackPanel)stackPanelRibbons.Children[stackPanelRibbons.Children.Count - 1]).Children.Count == 9) {
				StackPanel stackPanel = new StackPanel();
				stackPanel.Orientation = Orientation.Horizontal;
				stackPanel.Children.Add(stackPanelImage);
				stackPanelRibbons.Children.Add(stackPanel);
			}
			else {
				((StackPanel)stackPanelRibbons.Children[stackPanelRibbons.Children.Count - 1]).Children.Add(stackPanelImage);
			}
		}

		private void ribbon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			for (int i = 0; i < stackPanelRibbons.Children.Count; i++) {
				for (int j = 0; j < ((StackPanel)stackPanelRibbons.Children[i]).Children.Count; j++) {
					((StackPanel)((StackPanel)stackPanelRibbons.Children[i]).Children[j]).Background = new SolidColorBrush(Color.FromRgb(244, 244, 245));
				}
			}
			((StackPanel)sender).Background = new SolidColorBrush(Color.FromRgb(60, 120, 170));
			RibbonData ribbon = PokemonDatabase.GetRibbonFromID(((StackPanel)sender).Tag as string);
			textBlockRibbonDescription.Text = ribbon.Name + " - " + ribbon.Description;
		}

		public void SetCurrentMove(int moveIndex) {
			MoveData moveData = null;
			currentMoveIndex = moveIndex;
			rectMove1.Stroke = new SolidColorBrush(Color.FromRgb(70, 95, 191));
			rectMove2.Stroke = new SolidColorBrush(Color.FromRgb(70, 95, 191));
			rectMove3.Stroke = new SolidColorBrush(Color.FromRgb(70, 95, 191));
			rectMove4.Stroke = new SolidColorBrush(Color.FromRgb(70, 95, 191));
			rectContestMove1.Stroke = new SolidColorBrush(Color.FromRgb(141, 77, 185));
			rectContestMove2.Stroke = new SolidColorBrush(Color.FromRgb(141, 77, 185));
			rectContestMove3.Stroke = new SolidColorBrush(Color.FromRgb(141, 77, 185));
			rectContestMove4.Stroke = new SolidColorBrush(Color.FromRgb(141, 77, 185));
			rectMove1.StrokeThickness = 1;
			rectMove2.StrokeThickness = 1;
			rectMove3.StrokeThickness = 1;
			rectMove4.StrokeThickness = 1;
			rectContestMove1.StrokeThickness = 1;
			rectContestMove2.StrokeThickness = 1;
			rectContestMove3.StrokeThickness = 1;
			rectContestMove4.StrokeThickness = 1;
			if (moveIndex == -1) {
				this.textBlockMoveDescription.Text = "";
				this.labelMovePower.Content = "";
				this.labelMoveAccuracy.Content = "";
				this.labelMoveCategory.Content = "";
				this.textBlockContestMoveDescription.Text = "";
				this.labelMoveAppeal.Content = "";
				this.labelMoveJam.Content = "";
				return;
			}
			else if (moveIndex == 0) {
				moveData = pokemon.Move1Data;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.Colosseum)
					moveData = PokemonDatabase.GetMoveFromID(355);
				else if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove1ID != 0)
					moveData = PokemonDatabase.GetMoveFromID(((XDPokemon)pokemon).ShadowMove1ID);
				rectMove1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove1.StrokeThickness = 2;
				rectContestMove1.StrokeThickness = 2;
			}
			else if (moveIndex == 1) {
				moveData = pokemon.Move2Data;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove2ID != 0)
					moveData = PokemonDatabase.GetMoveFromID(((XDPokemon)pokemon).ShadowMove2ID);
				rectMove2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove2.StrokeThickness = 2;
				rectContestMove2.StrokeThickness = 2;
			}
			else if (moveIndex == 2) {
				moveData = pokemon.Move3Data;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove3ID != 0)
					moveData = PokemonDatabase.GetMoveFromID(((XDPokemon)pokemon).ShadowMove3ID);
				rectMove3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove3.StrokeThickness = 2;
				rectContestMove3.StrokeThickness = 2;
			}
			else if (moveIndex == 3) {
				moveData = pokemon.Move4Data;
				if (pokemon.IsShadowPokemon && pokemon.GameType == GameTypes.XD && ((XDPokemon)pokemon).ShadowMove4ID != 0)
					moveData = PokemonDatabase.GetMoveFromID(((XDPokemon)pokemon).ShadowMove4ID);
				rectMove4.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove4.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove4.StrokeThickness = 2;
				rectContestMove4.StrokeThickness = 2;
			}
			this.textBlockMoveDescription.Text = moveData.Description;
			this.labelMovePower.Content = (moveData.Power != 0 ? moveData.Power.ToString() : "---");
			this.labelMoveAccuracy.Content = (moveData.Accuracy != 0 ? moveData.Accuracy.ToString() : "---");
			this.labelMoveCategory.Content = moveData.Category.ToString();
			this.textBlockContestMoveDescription.Text = moveData.ContestDescription;
			this.labelMoveAppeal.Content = moveData.Appeal.ToString();
			this.labelMoveJam.Content = moveData.Jam.ToString();
		}

		private void rectMove1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			SetCurrentMove(0);
		}

		private void rectMove2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			SetCurrentMove(1);
		}

		private void rectMove3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			SetCurrentMove(2);
		}

		private void rectMove4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			SetCurrentMove(3);
		}

		private void OnReplaceBallClick(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			if (pokemon.BallCaughtID == 1 || pokemon.BallCaughtID == 11) {
				MessageBoxResult boxResult = TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " is caught in a " + pokemon.BallCaughtItemData.Name + ", are you sure you want to replace it?", "Replace Ball", MessageBoxButton.YesNo);
				if (boxResult == MessageBoxResult.No)
					return;
			}
			Item ballItem = ReplaceBallWindow.ShowDialog(Window.GetWindow(this), pokemon);
			if (ballItem != null) {
				if (ballItem.ID == 1 || ballItem.ID == 11) {
					MessageBoxResult boxResult = TriggerMessageBox.Show(Window.GetWindow(this), ballItem.ItemData.Name + " is a rare ball, are you sure you want to use it?", "Replace Ball", MessageBoxButton.YesNo);
					if (boxResult == MessageBoxResult.No)
						return;
				}
				pokemon.BallCaughtID = (byte)ballItem.ID;
				ballItem.Pocket.TossItemAt(ballItem.Pocket.IndexOf(ballItem), 1);

				foreach (TabItem tab in tabControl.Items) {
					((PokemonViewerSidePart)((Grid)tab.Content).Children[0]).RefreshUI();
				}
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
			}
		}

		private void OnWipeEVsClick(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			MessageBoxResult boxResult = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to wipe " + pokemon.Nickname + "'s EV's?", "Wipe EVs", MessageBoxButton.YesNo);
			if (boxResult == MessageBoxResult.No)
				return;
			pokemon.WipeEVs();

			this.labelHPEV.Content			= pokemon.HPEV.ToString();
			this.labelAttackEV.Content		= pokemon.AttackEV.ToString();
			this.labelDefenseEV.Content		= pokemon.DefenseEV.ToString();
			this.labelSpAttackEV.Content	= pokemon.SpAttackEV.ToString();
			this.labelSpDefenseEV.Content	= pokemon.SpDefenseEV.ToString();
			this.labelSpeedEV.Content		= pokemon.SpeedEV.ToString();

			this.labelHPStat.Content		= pokemon.HP.ToString();
			this.labelAttackStat.Content	= pokemon.Attack.ToString();
			this.labelDefenseStat.Content	= pokemon.Defense.ToString();
			this.labelSpAttackStat.Content	= pokemon.SpAttack.ToString();
			this.labelSpDefenseStat.Content	= pokemon.SpDefense.ToString();
			this.labelSpeedStat.Content		= pokemon.Speed.ToString();
			PokeManager.ManagerWindow.RefreshSearchResultsUI();
			TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + "'s EVs have been wiped", "EVs Wiped");
		}

		private void OnChangeNicknameClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			labelNickname.Content = pokemon.Nickname;
			var result = ChangeNicknameWindow.ShowDialog(Window.GetWindow(this), pokemon);
			if (result.HasValue && result.Value) {
				labelNickname.Content = pokemon.Nickname;
				RefreshUISideParts();
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
			}
		}

		private void OnChangeFormClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			if (pokemon != null && pokemon.DexID == 386) {
				ChangeDeoxysFormWindow.ShowDialog(Window.GetWindow(this), pokemon);
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
			}
		}

		private void OnChangeMarkingsClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			if (pokemon != null) {
				ChangeMarkingsWindow.ShowDialog(Window.GetWindow(this), pokemon);
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
			}
		}

		private void OnRearangeMovesClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			if (pokemon != null) {
				RearangeMovesWindow.ShowDialog(Window.GetWindow(this), pokemon);
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
			}
		}

		private void OnEvolveClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}

			EvolutionData evolutionLevelUp = PokemonDatabase.GetEvolutionOnLevelUp(pokemon);
			EvolutionData evolutionNow = PokemonDatabase.GetEvolutionNow(pokemon);
			EvolutionData[] itemEvolutions = PokemonDatabase.GetEvolutionsOnItem(pokemon);

			int evolutionTypeCount = 0;
			int evolutionType = -1;
			if (evolutionLevelUp != null) { evolutionTypeCount++; evolutionType = 0; }
			if (evolutionNow != null) { evolutionTypeCount++; evolutionType = 1; }
			if (itemEvolutions.Length > 0) { evolutionTypeCount++; evolutionType = 2; }


			if (evolutionTypeCount > 1) {
				var result = EvolutionTypeWindow.ShowDialog(Window.GetWindow(this), evolutionNow != null);
				if (result.HasValue && result.Value != -1)
					evolutionType = result.Value;
				else
					evolutionType = -1;
			}
			if (evolutionType == 0) {
				if (EvolutionWindow.ShowDialog(Window.GetWindow(this), pokemon, evolutionLevelUp.DexID)) {
					pokemon.GameSave.SetPokemonOwned(pokemon.DexID, true);
					PokeManager.RefreshUI();
				}
			}
			else if (evolutionType == 1) {
				if (EvolutionWindow.ShowDialog(Window.GetWindow(this), pokemon, evolutionNow.DexID)) {
					pokemon.GameSave.SetPokemonOwned(pokemon.DexID, true);
					PokeManager.RefreshUI();
				}
			}
			else if (evolutionType == 2) {
				ushort[] itemIDs = PokemonDatabase.GetEvolutionItemIDs(pokemon);
				Item item = SelectItemWindow.ShowDialog(Window.GetWindow(this), itemIDs, new ItemTypes[] { ItemTypes.Items }, "Select Evolution Item", "Evolve", true);
				if (item != null) {
					if (EvolutionWindow.ShowDialog(Window.GetWindow(this), pokemon, PokemonDatabase.GetEvolutionFromItemID(pokemon, item.ID).DexID)) {
						item.Pocket.TossItemAt(item.Pocket.IndexOf(item), 1);
						pokemon.GameSave.SetPokemonOwned(pokemon.DexID, true);
						PokeManager.RefreshUI();
					}
				}
			}
		}

		private void OnPlayCry(object sender, RoutedEventArgs e) {
			if (pokemon != null && pokemon.SpeciesID != 0) {
				string cryFile = PokemonDatabase.FindCryFile(pokemon.DexID);
				if (cryFile != null) {
					playerCry.Volume = PokeManager.Settings.Volume;
					playerCry.Open(new Uri(cryFile));
					playerCry.Play();
				}
				else {
					TriggerMessageBox.Show(Window.GetWindow(this), "You can supply Pokémon Cries as files for Trigger's PC to use. Place a folder called Cries in the path Resources/Pokemon/ and make sure they are numbered in the format \"12\" or \"012\". The files may be wav or mp3.", "About Cries");
				}
			}
		}

		private void OnGiveItemClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			if (pokemon.IsHoldingMail) {
				TriggerMessageBox.Show(Window.GetWindow(this), "Cannot take mail from Pokémon. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Give Item");
			}
			else if (pokemon.IsHoldingItem) {
				MessageBoxResult result = MessageBoxResult.Yes;
				string sentTo = "";
				if (pokemon.GameSave.Inventory.Items[pokemon.HeldItemData.PocketType].HasRoomForItem(pokemon.HeldItemID, 1)) {
					pokemon.GameSave.Inventory.Items[pokemon.HeldItemData.PocketType].AddItem(pokemon.HeldItemID, 1);
					if (pokemon.GameSave.GameType == GameTypes.Any)
						sentTo = PokeManager.Settings.ManagerNickname;
					else
						sentTo = pokemon.GameSave.TrainerName + "'s Bag";
				}
				else if (pokemon.GameSave.Inventory.Items.ContainsPocket(ItemTypes.PC) && pokemon.GameSave.Inventory.Items[ItemTypes.PC].HasRoomForItem(pokemon.HeldItemID, 1)) {
					result = TriggerMessageBox.Show(Window.GetWindow(this), "There is no room in your bag to store " + pokemon.HeldItemData.Name + ". Where would you like to send it?", "No Room", MessageBoxButton.YesNoCancel, "Your PC", "Game's PC");
					
					if (result == MessageBoxResult.Yes) {
						PokeManager.ManagerGameSave.Inventory.Items[pokemon.HeldItemData.PocketType].AddItem(pokemon.HeldItemID, 1);
						sentTo = PokeManager.Settings.ManagerNickname;
					}
					else if (result == MessageBoxResult.No) {
						pokemon.GameSave.Inventory.Items[ItemTypes.PC].AddItem(pokemon.HeldItemID, 1);
						sentTo = pokemon.GameSave.TrainerName + "'s PC";
					}
				}
				else {
					result = TriggerMessageBox.Show(Window.GetWindow(this), "There is no room in your bag or PC to store " + pokemon.HeldItemData.Name + ". Would you like to send it to " + PokeManager.Settings.ManagerNickname + "?", "No Room", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes) {
						PokeManager.ManagerGameSave.Inventory.Items[pokemon.HeldItemData.PocketType].AddItem(pokemon.HeldItemID, 1);
						sentTo = PokeManager.Settings.ManagerNickname;
					}
					else {
						result = MessageBoxResult.Cancel;
					}
				}
				if (result != MessageBoxResult.Cancel) {
					TriggerMessageBox.Show(Window.GetWindow(this), "Took " + pokemon.HeldItemData.Name + " from " + pokemon.Nickname + " and sent it to " + sentTo, "Took Item");
					pokemon.HeldItemID = 0;
					PokeManager.RefreshUI();
				}
			}
			else {
				Item item = GiveItemWindow.ShowDialog(Window.GetWindow(this), pokemon.GameSave);
				if (item != null) {
					item.Pocket.TossItemAt(item.Pocket.IndexOf(item), 1);
					pokemon.HeldItemID = item.ID;
					PokeManager.RefreshUI();
				}
			}
		}

		private void OnPersonalityClicked(object sender, MouseButtonEventArgs e) {
			if (pokemon != null) {
				Clipboard.SetText(pokemon.Personality.ToString());
			}
		}

		private void OnRelearnMoveClicked(object sender, RoutedEventArgs e) {
			RelearnMoveWindow.ShowDialog(Window.GetWindow(this), pokemon);
		}

		private void OnTeachTMClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}
			ushort[] validItemIDs = PokemonDatabase.GetTeachableMachineMoveItems(pokemon);
			Item item = SelectItemWindow.ShowDialog(Window.GetWindow(this), validItemIDs, new ItemTypes[] { ItemTypes.TMCase }, "Teach Machine", "Teach", false);
			if (item != null) {
				Move move = new Move(PokemonDatabase.GetMoveFromMachine(item.ID));
				if (pokemon.NumMoves == 4) {
					var result = LearnMoveWindow.ShowDialog(Window.GetWindow(this), pokemon, move.ID);
					if (result.HasValue && result.Value) {
						if (!item.ItemData.IsImportant)
							item.Pocket.TossItemAt(item.Pocket.IndexOf(item), 1);
						TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " learned " + move.MoveData.Name + "!", "Move Learned");
						RefreshUI();
						PokeManager.ManagerWindow.RefreshSearchResultsUI();
					}
				}
				else {
					pokemon.SetMoveAt(pokemon.NumMoves, move);
					if (!item.ItemData.IsImportant)
						item.Pocket.TossItemAt(item.Pocket.IndexOf(item), 1);
					TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " learned " + move.MoveData.Name + "!", "Move Learned");
					RefreshUI();
					PokeManager.ManagerWindow.RefreshSearchResultsUI();
				}
			}
		}

		private void OnFixRoamingIVs(object sender, RoutedEventArgs e) {
			Random random = new Random((int)DateTime.Now.Ticks);
			// The while loop is here due to the 1 in 4,194,304 chance that the IV's are generated the same.
			// Might as well generate them again since we'll never be able to tell if the user has used this or not.
			while (PokemonDatabase.IsPokemonWithRoamingIVGlitch(pokemon)) {
				byte newAttackIV = (byte)(random.Next(32) & 0x18); // (Ignore the part of the attack IV that's intact.);
				byte newDefenseIV = (byte)random.Next(32);
				byte newSpAttackIV = (byte)random.Next(32);
				byte newSpDefenseIV = (byte)random.Next(32);
				byte newSpeedIV = (byte)random.Next(32);
				pokemon.AttackIV |= newAttackIV;
				pokemon.DefenseIV = newDefenseIV;
				pokemon.SpAttackIV = newSpAttackIV;
				pokemon.SpDefenseIV = newSpDefenseIV;
				pokemon.SpeedIV = newSpeedIV;
			}
			RefreshUI();
			PokeManager.ManagerWindow.RefreshSearchResultsUI();
			TriggerMessageBox.Show(Window.GetWindow(this), "Remaining IVs were generated for " + pokemon.Nickname + "", "IVs Fixed");
		}

		private void OnEffortRibbonlicked(object sender, RoutedEventArgs e) {
			pokemon.HasEffortRibbon = true;
			RefreshUI();
			TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " has been awarded a new Effort Ribbon", "Awarded Ribbon");
		}

		private void OnRoundEVsClicked(object sender, RoutedEventArgs e) {
			List<StatTypes> removedFrom = new List<StatTypes>();
			int removedCount = 0;
			if (pokemon.HPEV > 252) {
				removedFrom.Add(StatTypes.HP);
				removedCount = pokemon.HPEV - 252;
				pokemon.HPEV = 252;
			}
			if (pokemon.AttackEV > 252) {
				removedFrom.Add(StatTypes.Attack);
				removedCount = pokemon.AttackEV - 252;
				pokemon.AttackEV = 252;
			}
			if (pokemon.DefenseEV > 252) {
				removedFrom.Add(StatTypes.Defense);
				removedCount = pokemon.DefenseEV - 252;
				pokemon.DefenseEV = 252;
			}
			if (pokemon.SpAttackEV > 252) {
				removedFrom.Add(StatTypes.SpAttack);
				removedCount = pokemon.SpAttackEV - 252;
				pokemon.SpAttackEV = 252;
			}
			if (pokemon.SpDefenseEV > 252) {
				removedFrom.Add(StatTypes.SpDefense);
				removedCount = pokemon.SpDefenseEV - 252;
				pokemon.SpDefenseEV = 252;
			}
			if (pokemon.SpeedEV > 252) {
				removedFrom.Add(StatTypes.Speed);
				removedCount = pokemon.SpeedEV - 252;
				pokemon.SpeedEV = 252;
			}

			string removedString = "";
			if (removedFrom.Count == 1) {
				removedString = removedFrom[0].ToString();
			}
			else if (removedFrom.Count == 2) {
				removedString = removedFrom[0].ToString() + " and " + removedFrom[1].ToString();
			}
			else {
				removedString = removedFrom[0].ToString();
				for (int i = 1; i < removedFrom.Count - 1; i++)
					removedString += ", " + removedFrom[i].ToString();
				removedString += ", and " + removedFrom[removedFrom.Count - 1].ToString();
			}

			TriggerMessageBox.Show(Window.GetWindow(this), "Removed " + removedCount.ToString() + " leftover EV" + (removedCount != 1 ? "s" : "") + " from " + pokemon.Nickname + "'s " + removedString, "Rounded EVs");
			RefreshUI();
			PokeManager.ManagerWindow.RefreshSearchResultsUI();
		}

		private void OnSettingsChangeClicked(object sender, RoutedEventArgs e) {
			int index = int.Parse((sender as ToggleButton).Tag as string);
			for (int i = 1; ; i++) {
				ToggleButton button = this.FindName("buttonSettings" + i.ToString()) as ToggleButton;
				Grid grid = FindName("gridSettings" + i.ToString()) as Grid;

				if (button == null)
					break;

				button.IsChecked = (i == index);
				grid.Visibility = (i == index ? Visibility.Visible : Visibility.Hidden);
			}
		}
		private void OnLevelDownClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}

			MessageBoxResult result = TriggerMessageBox.Show(Window.GetWindow(this), "This will level your Pokemon down to Lv 5 and completely replace its moves with moves it would know at Lv 5. Are you sure you want to continue?", "Level Down", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				// Set Level
				pokemon.Experience = PokemonDatabase.GetExperienceFromLevel(pokemon.PokemonData.ExperienceGroup, 5);
				pokemon.RecalculateStats();

				// Set Moves
				pokemon.SetMoveAt(0, new Move());
				pokemon.SetMoveAt(1, new Move());
				pokemon.SetMoveAt(2, new Move());
				pokemon.SetMoveAt(3, new Move());

				ushort[] moves = PokemonDatabase.GetMovesLearnedAtLevelRange(pokemon, 1, 5);

				// Teach the Pokemon all the new moves
				foreach (ushort moveID in moves) {
					if (!PokemonDatabase.PokemonHasMove(pokemon, moveID)) {
						if (pokemon.NumMoves < 4) {
							pokemon.SetMoveAt(pokemon.NumMoves, new Move(moveID));
						}
						else {
							for (int i = 0; i < 3; i++) {
								pokemon.SetMoveAt(i, pokemon.GetMoveAt(i + 1));
							}
							pokemon.SetMoveAt(3, new Move(moveID));
						}
					}
				}

				RefreshUI();
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
				TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " has been leveled down to Lv 5", "Level Down");
			}
		}

		private void OnInfectClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}

			PokerusStrain? strain = PokerusWindow.ShowDialog(Window.GetWindow(this));
			if (strain.HasValue && strain.Value.Value != 0) {
				pokemon.PokerusStrain = strain.Value.Value;
				pokemon.PokerusRemaining = (byte)((int)strain.Value.Strain + 1);
				pokemon.PokerusDaysRemaining = (byte)((int)strain.Value.Strain + 1);
				RefreshUI();
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
				TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " has been infected with Pokérus " + strain.ToString(), "Pokémon Infected");
			}
		}

		private void OnEVTrainingClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}

			var result = EVTrainingWindow.ShowDialog(Window.GetWindow(this), pokemon.Level);

			if (result.HasValue) {
				pokemon.Experience = PokemonDatabase.GetExperienceFromLevel(pokemon.PokemonData.ExperienceGroup, result.Value);
				pokemon.RecalculateStats();
				RefreshUI();
				PokeManager.ManagerWindow.RefreshSearchResultsUI();
				TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " has been leveled down to Lv " + result.Value, "EV Training");
			}
		}
	}
}
