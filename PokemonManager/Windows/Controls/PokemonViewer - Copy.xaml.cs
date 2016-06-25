using PokemonManager.Game;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Windows.Controls;
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

		public PokemonViewer() {
			InitializeComponent();
		}

		public void RefreshUI() {
			if (pokemon != null)
				LoadPokemon(pokemon);
		}

		public void UnloadPokemon() {
			this.currentMoveIndex = -1;
			this.pokemon = null;
			this.imagePokemon.Source = PokemonDatabase.GetPokemonImageFromDexID(0, false);
			this.imagePokemon2.Source = PokemonDatabase.GetPokemonImageFromDexID(0, false);
			this.rectShadowMask.Visibility = Visibility.Hidden;
			this.rectShadowMask2.Visibility = Visibility.Hidden;
			this.imageShadowAura.Visibility = Visibility.Hidden;
			this.imageShadowAura2.Visibility = Visibility.Hidden;
			this.imageShinyStar.Visibility = Visibility.Hidden;
			this.imageShinyStar2.Visibility = Visibility.Hidden;
			this.imageBallCaught.Source = null;
			this.imageBallCaught2.Source = null;

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
			this.labelSpeciesNumber.Content = "No. 000";


			this.labelHPIV.Content			= "0";
			this.labelAttackIV.Content		= "0";
			this.labelDefenseIV.Content		= "0";
			this.labelSpAttackIV.Content	= "0";
			this.labelSpDefenseIV.Content	= "0";
			this.labelSpeedIV.Content		= "0";

			this.labelHPEV.Content			= "0";
			this.labelAttackEV.Content		= "0";
			this.labelDefenseEV.Content		= "0";
			this.labelSpAttackEV.Content	= "0";
			this.labelSpDefenseEV.Content	= "0";
			this.labelSpeedEV.Content		= "0";

			this.labelHPStat.Content		= "0";
			this.labelAttackStat.Content	= "0";
			this.labelDefenseStat.Content	= "0";
			this.labelSpAttackStat.Content	= "0";
			this.labelSpDefenseStat.Content	= "0";
			this.labelSpeedStat.Content		= "0";

			this.labelCool.Content			= "0";
			this.labelBeauty.Content		= "0";
			this.labelCute.Content			= "0";
			this.labelSmart.Content			= "0";
			this.labelTough.Content			= "0";
			this.labelFeel.Content			= "0";

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
			this.type2Pokemon.Visibility = Visibility.Hidden;

			Brush unmarkedBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
			markCircle.Foreground = unmarkedBrush;
			markSquare.Foreground = unmarkedBrush;
			markTriangle.Foreground = unmarkedBrush;
			markHeart.Foreground = unmarkedBrush;

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
			ButtonEVs.IsEnabled = false;

			buttonMarkings.IsEnabled = false;
			buttonMoves.IsEnabled = false;
			buttonGive.IsEnabled = false;
			buttonTake.IsEnabled = false;
		}

		public void LoadPokemon(IPokemon pokemon) {
			this.gameIndex = PokeManager.GetIndexOfGame(pokemon.GameSave);
			this.currentMoveIndex = -1;
			this.pokemon = pokemon;
			this.imagePokemon.Source = pokemon.Sprite;
			this.imagePokemon2.Source = pokemon.Sprite;
			if (pokemon.IsShadowPokemon) {
				this.rectShadowMask.OpacityMask = new ImageBrush(this.imagePokemon.Source);
				this.rectShadowMask2.OpacityMask = new ImageBrush(this.imagePokemon.Source);
				this.rectShadowMask.Visibility = Visibility.Visible;
				this.rectShadowMask2.Visibility = Visibility.Visible;
				this.imageShadowAura.Visibility = Visibility.Visible;
				this.imageShadowAura2.Visibility = Visibility.Visible;
			}
			else {
				this.rectShadowMask.Visibility = Visibility.Hidden;
				this.rectShadowMask2.Visibility = Visibility.Hidden;
				this.imageShadowAura.Visibility = Visibility.Hidden;
				this.imageShadowAura2.Visibility = Visibility.Hidden;
			}
			this.imageShinyStar.Visibility = (pokemon.IsShiny ? Visibility.Visible : Visibility.Hidden);
			this.imageShinyStar2.Visibility = (pokemon.IsShiny ? Visibility.Visible : Visibility.Hidden);
			this.imageBallCaught.Source = PokemonDatabase.GetBallCaughtImageFromID(pokemon.BallCaughtID);
			this.imageBallCaught2.Source = PokemonDatabase.GetBallCaughtImageFromID(pokemon.BallCaughtID);


			if (pokemon.IsEgg) {
				this.labelNickname.Content = "EGG";
			}
			else {
				this.labelNickname.Content = pokemon.Nickname;
			}
			this.labelLevel.Content = "Lv " + pokemon.Level.ToString();
			if (pokemon.Gender == Genders.Male) {
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
			this.labelSpeciesNumber.Content = "No. " + pokemon.DexID.ToString("000");

			this.labelHPIV.Content			= pokemon.HPIV.ToString();
			this.labelAttackIV.Content		= pokemon.AttackIV.ToString();
			this.labelDefenseIV.Content		= pokemon.DefenseIV.ToString();
			this.labelSpAttackIV.Content	= pokemon.SpAttackIV.ToString();
			this.labelSpDefenseIV.Content	= pokemon.SpDefenseIV.ToString();
			this.labelSpeedIV.Content		= pokemon.SpeedIV.ToString();

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

			this.labelCool.Content			= pokemon.Coolness.ToString();
			this.labelBeauty.Content		= pokemon.Beauty.ToString();
			this.labelCute.Content			= pokemon.Cuteness.ToString();
			this.labelSmart.Content			= pokemon.Smartness.ToString();
			this.labelTough.Content			= pokemon.Toughness.ToString();
			this.labelFeel.Content			= pokemon.Feel.ToString();

			if (pokemon.Move1ID != 0) {
				this.labelMove1Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move1ID).Name;
				this.labelMove1PP.Content = pokemon.Move1TotalPP.ToString();
				this.typeMove1.Type = PokemonDatabase.GetMoveFromID(pokemon.Move1ID).Type;
				this.typeMove1.Visibility = Visibility.Visible;
				this.labelContestMove1Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move1ID).Name;
				this.labelContestMove1PP.Content = (pokemon.Move1Data.PP == 0 ? "--" : pokemon.Move1TotalPP.ToString());
				this.conditionMove1.Type = PokemonDatabase.GetMoveFromID(pokemon.Move1ID).ConditionType;
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
				this.labelMove2Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move2ID).Name;
				this.labelMove2PP.Content = pokemon.Move2TotalPP.ToString();
				this.typeMove2.Type = PokemonDatabase.GetMoveFromID(pokemon.Move2ID).Type;
				this.typeMove2.Visibility = Visibility.Visible;
				this.labelContestMove2Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move2ID).Name;
				this.labelContestMove2PP.Content = (pokemon.Move2Data.PP == 0 ? "--" : pokemon.Move2TotalPP.ToString());
				this.conditionMove2.Type = PokemonDatabase.GetMoveFromID(pokemon.Move2ID).ConditionType;
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
				this.labelMove3Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move3ID).Name;
				this.labelMove3PP.Content = pokemon.Move3TotalPP.ToString();
				this.typeMove3.Type = PokemonDatabase.GetMoveFromID(pokemon.Move3ID).Type;
				this.typeMove3.Visibility = Visibility.Visible;
				this.labelContestMove3Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move3ID).Name;
				this.labelContestMove3PP.Content = (pokemon.Move3Data.PP == 0 ? "--" : pokemon.Move3TotalPP.ToString());
				this.conditionMove3.Type = PokemonDatabase.GetMoveFromID(pokemon.Move3ID).ConditionType;
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
				this.labelMove4Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move4ID).Name;
				this.labelMove4PP.Content = pokemon.Move4TotalPP.ToString();
				this.typeMove4.Type = PokemonDatabase.GetMoveFromID(pokemon.Move4ID).Type;
				this.typeMove4.Visibility = Visibility.Visible;
				this.labelContestMove4Name.Content = PokemonDatabase.GetMoveFromID(pokemon.Move4ID).Name;
				this.labelContestMove4PP.Content = (pokemon.Move4Data.PP == 0 ? "--" : pokemon.Move4TotalPP.ToString());
				this.conditionMove4.Type = PokemonDatabase.GetMoveFromID(pokemon.Move4ID).ConditionType;
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


			this.labelNature.Content = pokemon.NatureData.Name;
			this.labelNatureRaised.Content = "+" + pokemon.NatureData.RaisedStat.ToString();
			this.labelNatureLowered.Content = "-" + pokemon.NatureData.LoweredStat.ToString();

			this.labelAbility.Content = pokemon.AbilityData.Name;
			this.textBlockAbilityDescription.Text = pokemon.AbilityData.Description;

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

			this.labelPokerus.Content = pokemon.PokerusStatus.ToString();

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
			if (pokemon.PokemonData.HasTwoTypes) {
				this.type2Pokemon.Type = pokemon.PokemonData.Type2;
				this.type2Pokemon.Visibility = Visibility.Visible;
			}
			else {
				this.type2Pokemon.Visibility = Visibility.Hidden;
			}

			Brush unmarkedBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
			Brush markedBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			markCircle.Foreground = (pokemon.IsCircleMarked ? markedBrush : unmarkedBrush);
			markSquare.Foreground = (pokemon.IsSquareMarked ? markedBrush : unmarkedBrush);
			markTriangle.Foreground = (pokemon.IsTriangleMarked ? markedBrush : unmarkedBrush);
			markHeart.Foreground = (pokemon.IsHeartMarked ? markedBrush : unmarkedBrush);

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

			buttonBall.IsEnabled = !pokemon.IsEgg;
			buttonNickname.IsEnabled = !pokemon.IsEgg && !pokemon.IsShadowPokemon;
			buttonDeoxys.IsEnabled = pokemon.DexID == 386;
			ButtonEVs.IsEnabled = !pokemon.IsEgg && !pokemon.IsShadowPokemon;

			buttonMarkings.IsEnabled = true;
			buttonMoves.IsEnabled = !pokemon.IsEgg && !pokemon.IsShadowPokemon;
			buttonGive.IsEnabled = !pokemon.IsEgg;
			buttonTake.IsEnabled = pokemon.IsHoldingItem;

			SetCurrentMove(-1);
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
				moveData = PokemonDatabase.GetMoveFromID(pokemon.Move1ID);
				rectMove1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove1.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove1.StrokeThickness = 2;
				rectContestMove1.StrokeThickness = 2;
			}
			else if (moveIndex == 1) {
				moveData = PokemonDatabase.GetMoveFromID(pokemon.Move2ID);
				rectMove2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove2.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove2.StrokeThickness = 2;
				rectContestMove2.StrokeThickness = 2;
			}
			else if (moveIndex == 2) {
				moveData = PokemonDatabase.GetMoveFromID(pokemon.Move3ID);
				rectMove3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectContestMove3.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				rectMove3.StrokeThickness = 2;
				rectContestMove3.StrokeThickness = 2;
			}
			else if (moveIndex == 3) {
				moveData = PokemonDatabase.GetMoveFromID(pokemon.Move4ID);
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
			if (pokemon.BallCaughtID == 1 || pokemon.BallCaughtID == 5 || pokemon.BallCaughtID == 11) {
				MessageBoxResult boxResult = TriggerMessageBox.Show(Window.GetWindow(this), pokemon.Nickname + " is caught in a rare ball, are you sure you want to replace it?", "Replace Ball", MessageBoxButton.YesNo);
				if (boxResult == MessageBoxResult.No)
					return;
			}
			byte? result = ReplaceBallWindow.ShowDialog(Window.GetWindow(this), pokemon);
			if (result.HasValue) {
				if (result.Value == 1 || result.Value == 11) {
					MessageBoxResult boxResult = TriggerMessageBox.Show(Window.GetWindow(this), ItemDatabase.GetItemFromID(result.Value).Name + " is a rare ball, are you sure you want to use it?", "Replace Ball", MessageBoxButton.YesNo);
					if (boxResult == MessageBoxResult.No)
						return;
				}
				pokemon.BallCaughtID = result.Value;
				this.imageBallCaught.Source = PokemonDatabase.GetBallCaughtImageFromID(pokemon.BallCaughtID);
				this.imageBallCaught2.Source = PokemonDatabase.GetBallCaughtImageFromID(pokemon.BallCaughtID);
			}
		}

		private void OnWipeEVsClick(object sender, RoutedEventArgs e) {
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
		}

		private void OnChangeNicknameClicked(object sender, RoutedEventArgs e) {
			labelNickname.Content = pokemon.Nickname;
			string result = ChangeNicknameWindow.ShowDialog(Window.GetWindow(this), pokemon.Nickname);
			if (result != null) {
				if (result == "")
					pokemon.RemoveNickname();
				else
					pokemon.Nickname = result;
				labelNickname.Content = pokemon.Nickname;
			}
		}

		private void OnChangeFormClicked(object sender, RoutedEventArgs e) {
			if (pokemon != null && pokemon.DexID == 386) {
				ChangeDeoxysFormWindow.ShowDialog(Window.GetWindow(this), pokemon);
			}
		}

		private void OnChangeMarkingsClicked(object sender, RoutedEventArgs e) {
			if (pokemon != null) {
				ChangeMarkingsWindow.ShowDialog(Window.GetWindow(this), pokemon);
			}
		}

		private void OnRearangeMovesClicked(object sender, RoutedEventArgs e) {
			if (pokemon != null) {
				RearangeMovesWindow.ShowDialog(Window.GetWindow(this), pokemon);
			}
		}
	}
}
