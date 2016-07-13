using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for TrainerInfoWindow.xaml
	/// </summary>
	public partial class TrainerInfoViewer : UserControl {

		private IGameSave gameSave;
		private CurrencyTypes selectedCurrency;
		private ContextMenu contextMenu;
		private bool loaded;

		public TrainerInfoViewer() {
			InitializeComponent();

			DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			dispatcherTimer.Tick += OnTick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();

			CreateContextMenu();

			buttonMoney.ContextMenu = contextMenu;
			buttonCoins.ContextMenu = contextMenu;
			buttonBattlePts.ContextMenu = contextMenu;
			buttonCoupons.ContextMenu = contextMenu;
			buttonSoot.ContextMenu = contextMenu;

			gridBadges.Visibility = Visibility.Hidden;
			gridSettings.Visibility = Visibility.Hidden;

		}

		private void AddAlteringCaveItem(ushort dexID) {
			
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation =  Orientation.Horizontal;
			Image image = new Image();
			//(form == byte.MaxValue ? 0 : form)
			image.Source = PokemonDatabase.GetPokemonBoxImageFromDexID(dexID, false);
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;
			image.Margin = new Thickness(0, -7, 0, -1);
			TextBlock text = new TextBlock();
			text.Margin = new Thickness(6, 0, 0, 0);
			text.Text = PokemonDatabase.GetPokemonFromDexID(dexID).Name;
			text.VerticalAlignment = VerticalAlignment.Center;
			stackPanel.Children.Add(image);
			stackPanel.Children.Add(text);
			comboBoxAlteringCave.Items.Add(stackPanel);
		}

		public void CreateContextMenu() {
			contextMenu = new ContextMenu();
			MenuItem sendTo = new MenuItem();
			sendTo.Header = "Send To";
			sendTo.Click += OnSendTo;
			MenuItem sendFrom = new MenuItem();
			sendFrom.Header = "Send From";
			sendFrom.Click += OnSendFrom;
			contextMenu.Items.Add(sendTo);
			contextMenu.Items.Add(sendFrom);
		}

		public void OnTick(object sender, EventArgs e) {
			if (gameSave is ManagerGameSave) {
				ManagerGameSave managerSave = gameSave as ManagerGameSave;
				TimeSpan newPlayTime = managerSave.PlayTime + new TimeSpan((DateTime.Now - managerSave.TimeOfLastSave).Ticks);
				labelPlayTime.Content = ((int)newPlayTime.TotalHours).ToString() + ":" + newPlayTime.Minutes.ToString("00") + ":" + newPlayTime.Seconds.ToString("00");
			}
		}

		public void RefreshUI() {
			if (gameSave != null) {
			loaded = false;
				BitmapSource trainerImage = null;
				if (PokeManager.IsAprilFoolsMode) {
					trainerImage = ResourceDatabase.GetImageFromName("YoungsterMale");
				}
				else if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire) {
					trainerImage = ResourceDatabase.GetImageFromName("RubySapphire" + gameSave.TrainerGender.ToString());
				}
				else if (gameSave.GameType == GameTypes.Emerald) {
					trainerImage = ResourceDatabase.GetImageFromName("Emerald" + gameSave.TrainerGender.ToString());
				}
				else if (gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen) {
					trainerImage = ResourceDatabase.GetImageFromName("FireRedLeafGreen" + gameSave.TrainerGender.ToString());
				}
				else if (gameSave.GameType == GameTypes.Colosseum) {
					trainerImage = ResourceDatabase.GetImageFromName("ColosseumMale");
				}
				else if (gameSave.GameType == GameTypes.XD) {
					trainerImage = ResourceDatabase.GetImageFromName("XDMale");
				}
				else if (gameSave.GameType == GameTypes.Any) {
					trainerImage = PokeManager.TrainerImage;
				}
				if (trainerImage != null) {
					imageTrainer.Width = Math.Min(90, trainerImage.PixelWidth);
					imageTrainer.Height = Math.Min(138, trainerImage.PixelHeight);
				}
				imageTrainer.Source = trainerImage;

				labelTrainerName.Content = gameSave.TrainerName;
				labelTrainerID.Content = gameSave.TrainerID.ToString("00000");
				labelSecretID.Content = gameSave.SecretID.ToString("00000");
				if (gameSave.GameType == GameTypes.Any) {
					ManagerGameSave managerSave = gameSave as ManagerGameSave;
					TimeSpan newPlayTime = managerSave.PlayTime + new TimeSpan((DateTime.Now - managerSave.TimeOfLastSave).Ticks);
					labelPlayTime.Content = ((int)newPlayTime.TotalHours).ToString() + ":" + newPlayTime.Minutes.ToString("00") + ":" + newPlayTime.Seconds.ToString("00");
				}
				else if (gameSave.GameType == GameTypes.XD || gameSave.GameType == GameTypes.Colosseum) {
					labelPlayTime.Content = "---";
				}
				else {
					labelPlayTime.Content = gameSave.HoursPlayed + ":" + gameSave.MinutesPlayed.ToString("00") + ":" + gameSave.SecondsPlayed.ToString("00");
				}
				if (gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD)
					labelPokedexText.Content = "StrMemo";
				else
					labelPokedexText.Content = "Pokédex";
				labelPokedex.Content = gameSave.PokemonOwned.ToString();
				labelMoney.Content = "$" + gameSave.Money.ToString("#,0");
				if (gameSave.GameType != GameTypes.Colosseum && gameSave.GameType != GameTypes.XD)
					labelCoins.Content = gameSave.Coins.ToString("#,0") + " Coins";
				else
					labelCoins.Content = "---";
				if (gameSave.GameType == GameTypes.Any || gameSave.GameType == GameTypes.Emerald)
					labelBattlePts.Content = gameSave.BattlePoints.ToString("#,0") + " BP";
				else
					labelBattlePts.Content = "---";
				if (gameSave.GameType == GameTypes.Any || gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire || gameSave.GameType == GameTypes.Emerald)
					labelSoot.Content = gameSave.VolcanicAsh.ToString("#,0") + " Soot";
				else
					labelSoot.Content = "---";
				if (gameSave.GameType == GameTypes.Any || gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD)
					labelCoupons.Content = gameSave.PokeCoupons.ToString("#,0") + " PC";
				else
					labelCoupons.Content = "---";


				buttonMoney.IsEnabled = true;
				buttonCoins.IsEnabled = (gameSave.GameType != GameTypes.Colosseum && gameSave.GameType != GameTypes.XD);
				buttonBattlePts.IsEnabled = (gameSave.GameType == GameTypes.Any || gameSave.GameType == GameTypes.Emerald);
				buttonCoupons.IsEnabled = (gameSave.GameType == GameTypes.Any || gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD);
				buttonSoot.IsEnabled = (gameSave.GameType == GameTypes.Any || gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire || gameSave.GameType == GameTypes.Emerald);

				gridBadges.Visibility = Visibility.Hidden;
				gridSettings.Visibility = Visibility.Hidden;

				if (gameSave is GBAGameSave) {
					gridBadges.Visibility = Visibility.Visible;

					bool[] badges = (gameSave as GBAGameSave).Badges;
					bool isKanto = gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen;
					for (int i = 0; i < 8; i++) {
						Image imageBadge = (Image)FindName("badge" + (i + 1).ToString());
						if (badges[i])
							imageBadge.Source = ResourceDatabase.GetImageFromName((isKanto ? "Kanto" : "Hoenn") + "Badge" + (i + 1).ToString());
						else
							imageBadge.Source = null;
					}

					if (gameSave.GameType == GameTypes.Emerald || gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen) {
						gridSettings.Visibility = Visibility.Visible;
						comboBoxAlteringCave.Items.Clear();
						AddAlteringCaveItem(41);
						AddAlteringCaveItem(179);
						AddAlteringCaveItem(204);
						AddAlteringCaveItem(228);
						AddAlteringCaveItem(216);
						AddAlteringCaveItem(190);
						AddAlteringCaveItem(213);
						AddAlteringCaveItem(234);
						AddAlteringCaveItem(235);
						comboBoxAlteringCave.SelectedIndex = (int)(gameSave as GBAGameSave).AlteringCavePokemon;

						labelStevenBattleText.IsEnabled = gameSave.GameType == GameTypes.Emerald;
						buttonStevenBattle.IsEnabled = gameSave.GameType == GameTypes.Emerald && (gameSave as GBAGameSave).HasBattledStevenEmerald;
					}
				}
			
				loaded = true;
			}
		}

		public void LoadGameSave(IGameSave gameSave) {
			this.gameSave = gameSave;
			RefreshUI();
		}

		private void OnSendTo(object sender, RoutedEventArgs e) {
			SendCurrencyToWindow.ShowDialog(Window.GetWindow(this), PokeManager.GetIndexOfGame(gameSave), selectedCurrency, true);
		}
		private void OnSendFrom(object sender, RoutedEventArgs e) {
			SendCurrencyToWindow.ShowDialog(Window.GetWindow(this), PokeManager.GetIndexOfGame(gameSave), selectedCurrency, false);
		}
		private void OnSendButtonClicked(object sender, RoutedEventArgs e) {
			selectedCurrency = (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), ((Button)sender).Tag as string);
			((Button)sender).ContextMenu.IsOpen = true;
		}

		private void OnAlteringCaveChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;

			(gameSave as GBAGameSave).AlteringCavePokemon = (AlteringCavePokemon)comboBoxAlteringCave.SelectedIndex;
		}

		private void OnRebattleStevenClicked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			(gameSave as GBAGameSave).HasBattledStevenEmerald = false;
		}

		private void OnSendCurrencyContextMenuOpening(object sender, ContextMenuEventArgs e) {
			selectedCurrency = (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), ((Button)sender).Tag as string);
		}
	}
}
