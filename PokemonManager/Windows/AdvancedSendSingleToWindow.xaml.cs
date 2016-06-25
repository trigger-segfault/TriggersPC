using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
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

namespace PokemonManager.Windows {

	public class AdvancedSendSingleResults {
		public int Count { get; set; }
		public int GameIndex { get; set; }
		public ItemTypes Pocket { get; set; }
	}

	public partial class AdvancedSendSingleToWindow : Window {

		private int realMax;
		private int max;
		private bool sendAll;

		private int gameIndex;
		private ItemTypes itemType;
		private bool isDecoration;
		private bool loaded = false;
		private ItemTypes pocket;
		private bool withdrawMode;

		public AdvancedSendSingleToWindow(int gameIndex, int max, string title, object sendType, bool withdrawMode) {
			InitializeComponent();
			this.loaded = false;
			this.realMax = max;
			this.Title = title;
			this.max = 1;
			this.sendAll = false;
			this.itemType = (sendType is ItemTypes ? (ItemTypes)sendType : ItemTypes.Unknown);
			this.isDecoration = false;
			this.pocket = this.itemType;
			this.withdrawMode = withdrawMode;

			if (sendType is DecorationTypes) {
				this.comboBoxPockets.Items.Add(ItemDatabase.GetDecorationContainerName((DecorationTypes)sendType));
				this.comboBoxPockets.SelectedIndex = 0;
				this.comboBoxPockets.IsEnabled = false;
				this.isDecoration = true;
			}
			this.numericMax.Maximum = max;

			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				if (withdrawMode) {
					if (i != gameIndex) {
						comboBoxGames.SetGameSaveVisible(i, false);
						continue;
					}
				}
				else {
					if (i == gameIndex) {
						comboBoxGames.SetGameSaveVisible(i, false);
						continue;
					}
					IGameSave gameSave = PokeManager.GetGameSaveAt(i);
					if (gameSave.GameType == GameTypes.PokemonBox || (this.isDecoration && gameSave.Inventory.Decorations == null))
						comboBoxGames.SetGameSaveVisible(i, false);
				}
			}

			if (withdrawMode) {
				this.gameIndex = gameIndex;
				comboBoxGames.SelectedGameIndex = gameIndex;
				comboBoxGames.IsEnabled = false;
				comboBoxPockets.IsEnabled = false;
			}
			else {
				this.gameIndex = PokeManager.LastGameInDialogIndex;
				if (this.gameIndex == -2 || !comboBoxGames.IsGameSaveVisible(this.gameIndex))
					this.gameIndex = comboBoxGames.SelectedGameIndex;
				else
					comboBoxGames.SelectedGameIndex = this.gameIndex;
			}

			this.loaded = true;

			OnGameChanged(null, null);
		}

		public static AdvancedSendSingleResults ShowDialog(Window owner, int gameIndex, int max, string title, object sendType, bool withdrawMode) {
			AdvancedSendSingleToWindow window = new AdvancedSendSingleToWindow(gameIndex, max, title, sendType, withdrawMode);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result.HasValue && result.Value) {
				if (!withdrawMode)
					PokeManager.LastGameInDialogIndex = window.gameIndex;
				AdvancedSendSingleResults results = new AdvancedSendSingleResults();
				results.Count = (window.sendAll ? window.realMax : window.max);
				results.GameIndex = window.gameIndex;
				results.Pocket = window.pocket;
				return results;
			}
			return null;
		}

		private IGameSave GameSave {
			get { return PokeManager.GetGameSaveAt(gameIndex); }
		}

		private void OnGameChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			if (comboBoxGames.SelectedGameIndex != -2) {
				gameIndex = comboBoxGames.SelectedGameIndex;

				if (!isDecoration)
					PopulatePockets();
			}
		}

		private void OnPocketChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			if (comboBoxPockets.SelectedIndex != -1) {
				pocket = (ItemTypes)(comboBoxPockets.SelectedItem as ComboBoxItem).Tag;
			}
		}

		private void PopulatePockets() {
			loaded = false;
			comboBoxPockets.Items.Clear();
			if (withdrawMode) {
				TryAddPocket(itemType);
				comboBoxPockets.SelectedIndex = 0;
			}
			else {
				TryAddPocket(ItemTypes.PC);
				TryAddPocket(itemType);
				comboBoxPockets.SelectedIndex = (GameSave.Inventory.Items.ContainsPocket(ItemTypes.PC) ? 1 : 0);
			}
			loaded = true;
		}

		private void TryAddPocket(ItemTypes pocket) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
			if (gameSave.Inventory.Items.ContainsPocket(pocket)) {
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = ItemDatabase.GetPocketName(pocket);
				comboBoxItem.Tag = pocket;
				comboBoxPockets.Items.Add(comboBoxItem);
			}
		}

		private void OnMaxValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			max = numericMax.Value;
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			PokeManager.LastGameInDialogIndex = gameIndex;
			DialogResult = true;
			Close();
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			numericMax.SelectAll();
			if (comboBoxGames.SelectedGameIndex == -2) {
				TriggerMessageBox.Show(this, "No available games to send with", "Can't Send");
				Close();
			}
		}
		private void OnSendAllChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			this.sendAll = this.checkBoxSendAll.IsChecked.Value;

			this.numericMax.Value = (sendAll ? realMax : max);
			this.numericMax.IsEnabled = !sendAll;
		}
	}
}
