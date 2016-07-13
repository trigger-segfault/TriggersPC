using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Items;
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
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class SendCurrencyToWindow : Window {

		private bool sendMode;
		private int amount;
		private int sendGameIndex;
		private int gameIndex;
		private CurrencyTypes currencyType;
		private bool loaded;

		public SendCurrencyToWindow(int gameIndex, CurrencyTypes currencyType, bool sendMode) {
			InitializeComponent();
			upDownCurrencyCount.Maximum = 1000000000;
			upDownCurrencyCount.Minimum = 0;
			upDownCurrencyCount.Value = 1;
			this.loaded = false;

			this.sendMode = sendMode;
			this.sendGameIndex = gameIndex;
			this.currencyType = currencyType;

			labelCurrencyType.Content = CurrencyName;
			if (!sendMode)
				Title = "Send Currency From";
			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				if (i == gameIndex) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
				}

				IGameSave game = (IGameSave)PokeManager.GetGameSaveAt(i);
				if (GetMaxCurrency(i) == 0 || game.GameType == GameTypes.PokemonBox) {
					comboBoxGame.SetGameSaveVisible(i, false);
				}
			}

			this.gameIndex = PokeManager.LastGameInDialogIndex;
			if (this.gameIndex == -2 || !comboBoxGame.IsGameSaveVisible(this.gameIndex)) {
				this.gameIndex = comboBoxGame.SelectedGameIndex;
			}
			else {
				comboBoxGame.SelectedGameIndex = this.gameIndex;
			}

			this.loaded = true;
			GameChanged(null, null);
		}

		private uint GetMaxCurrency(int gameIndex) {
			GameTypes gameType = PokeManager.GetGameSaveAt(gameIndex).GameType;
			if (gameType == GameTypes.Any)
				return 1000000000;
			switch (currencyType) {
			case CurrencyTypes.Money:
				if (gameType == GameTypes.Colosseum || gameType == GameTypes.XD)
					return 99999999;
				return 999999;
			case CurrencyTypes.Coins:
				if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire || gameType == GameTypes.Emerald ||
					gameType == GameTypes.FireRed || gameType == GameTypes.LeafGreen)
					return 9999;
				return 0;
			case CurrencyTypes.BattlePoints:
				if (gameType == GameTypes.Emerald)
					return 9999;
				return 0;
			case CurrencyTypes.PokeCoupons:
				if (gameType == GameTypes.Colosseum || gameType == GameTypes.XD)
					return 9999999;
				return 0;
			case CurrencyTypes.VolcanicAsh:
				if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire || gameType == GameTypes.Emerald)
					return 9999;
				return 0;
			}
			return 0;
		}

		private string CurrencyName {
			get {
				switch (currencyType) {
				case CurrencyTypes.Money: return "Poké Dollars";
				case CurrencyTypes.Coins: return "Coins";
				case CurrencyTypes.BattlePoints: return "Battle Points";
				case CurrencyTypes.PokeCoupons: return "Poké Coupons";
				case CurrencyTypes.VolcanicAsh: return "Volcanic Ash";
				}
				return "";
			}
		}

		private string FormatCurrencyString(uint currency) {
			switch (currencyType) {
			case CurrencyTypes.Money: return "$" + currency.ToString("#,0");
			case CurrencyTypes.Coins: return currency.ToString("#,0") + " Coins";
			case CurrencyTypes.BattlePoints: return currency.ToString("#,0") + " BP";
			case CurrencyTypes.PokeCoupons: return currency.ToString("#,0") + " PC";
			case CurrencyTypes.VolcanicAsh: return currency.ToString("#,0") + " Soot";
			}
			return currency.ToString("#,0");
		}

		public static bool? ShowDialog(Window owner, int gameIndex, CurrencyTypes currencyType, bool sendMode) {
			SendCurrencyToWindow window = new SendCurrencyToWindow(gameIndex, currencyType, sendMode);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OnValueChanged(object sender, RoutedEventArgs e) {
			amount = ((NumericUpDown)sender).Value;
		}

		private bool HasRoomForCurrency(uint amount) {
			int toIndex = (sendMode ? gameIndex : sendGameIndex);
			uint toCurrency = GetCurrency(toIndex);
			try {
				if ((toCurrency + amount) <= GetMaxCurrency(toIndex))
					return true;
			}
			catch (OverflowException) {
				
			}
			return false;
		}

		public int SendToGameIndex {
			get { return (sendMode ? gameIndex : sendGameIndex); }
		}
		public int SendFromGameIndex {
			get { return (sendMode ? sendGameIndex : gameIndex);  }
		}

		public uint GetCurrency(int gameIndex) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
			switch (currencyType) {
			case CurrencyTypes.Money: return gameSave.Money;
			case CurrencyTypes.Coins: return gameSave.Coins;
			case CurrencyTypes.BattlePoints: return gameSave.BattlePoints;
			case CurrencyTypes.PokeCoupons: return gameSave.PokeCoupons;
			case CurrencyTypes.VolcanicAsh: return gameSave.VolcanicAsh;
			}
			return 0;
		}

		public uint ChangeCurrency(int gameIndex, int amountChange) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
			switch (currencyType) {
			case CurrencyTypes.Money: return gameSave.Money = (uint)((int)gameSave.Money + amountChange);
			case CurrencyTypes.Coins: return gameSave.Coins = (ushort)((int)gameSave.Coins + amountChange);
			case CurrencyTypes.BattlePoints: return gameSave.BattlePoints = (ushort)((int)gameSave.BattlePoints + amountChange);
			case CurrencyTypes.PokeCoupons: return gameSave.PokeCoupons = (uint)((int)gameSave.PokeCoupons + amountChange);
			case CurrencyTypes.VolcanicAsh: return gameSave.VolcanicAsh = (ushort)((int)gameSave.VolcanicAsh + amountChange);
			}
			return 0;
		}

		private void GameChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;

			gameIndex = comboBoxGame.SelectedGameIndex;
			if (gameIndex >= -1) {

				int max = Math.Min((int)GetCurrency(SendFromGameIndex), (int)GetMaxCurrency(SendToGameIndex) - (int)GetCurrency(SendToGameIndex));
				if (max <= 0) {
					max = 0;
					upDownCurrencyCount.Value = 0;
					upDownCurrencyCount.Maximum = 0;
				}
				else {
					upDownCurrencyCount.Value = Math.Min(amount, max);
					upDownCurrencyCount.Maximum = max;
				}

				labelName1.Content = PokeManager.GetGameSaveAt(SendFromGameIndex).TrainerName + "'s";
				labelName2.Content = PokeManager.GetGameSaveAt(SendToGameIndex).TrainerName + "'s";

				labelCurrency1.Content = FormatCurrencyString(GetCurrency(SendFromGameIndex));
				labelCurrency2.Content = FormatCurrencyString(GetCurrency(SendToGameIndex));
			}
			else {
				
			}
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			amount = Math.Min(amount, Math.Min((int)GetCurrency(SendFromGameIndex), (int)GetMaxCurrency(SendToGameIndex) - (int)GetCurrency(SendToGameIndex)));
			ChangeCurrency(SendFromGameIndex, -amount);
			ChangeCurrency(SendToGameIndex, amount);

			PokeManager.RefreshUI();

			DialogResult = true;
		}

		private void upDownCurrencyCount_TextInput(object sender, TextCompositionEventArgs e) {
			for (int i = 0; i < e.Text.Length; i++) {
				if (e.Text[i] < '0' || e.Text[i] > '9')
					e.Handled = true;
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			this.upDownCurrencyCount.SelectAll();
			if (comboBoxGame.SelectedGameIndex == -2) {
				TriggerMessageBox.Show(this, "No available games to send with", "Can't Send");
				Close();
			}
			else {
				GameChanged(null, null);
			}
		}
	}
}
