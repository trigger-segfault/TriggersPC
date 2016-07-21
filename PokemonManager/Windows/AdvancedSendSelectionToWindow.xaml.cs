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
	
	public class AdvancedSendSelectionResults {
		public int Max { get; set; }
		public int Min { get; set; }
		public int Keep { get; set; }
		public bool AsPercentage { get; set; }
		public int GameIndex { get; set; }
		public ItemTypes Pocket { get; set; }

		public int GetFinalCount(uint count) {
			int finalCount = 0;
			if (AsPercentage) {
				finalCount = Math.Min((int)count - Keep, (int)((double)count * (double)Max / 100.0));
			}
			else {
				finalCount = Math.Min((int)count - Keep, Max);
			}
			if (finalCount < Min) {
				finalCount = Math.Min((int)count, Min);
			}

			return finalCount;
		}
	}

	public enum SendTypes {
		Item,
		Decoration
	}

	public partial class AdvancedSendSelectionToWindow : Window {

		private int realMax;

		private int percentageMax;
		private int max;
		private int min;
		private int keep;
		private bool maxMode;
		private bool percentage;
		private bool sendAll;

		private int gameIndex;
		private ItemTypes itemType;
		private bool isDecoration;
		private bool loaded = false;
		private ItemTypes pocket;
		private bool withdrawMode;

		public AdvancedSendSelectionToWindow(int gameIndex, int max, string title, object sendType, bool withdrawMode, GameTypeFlags exclusives) {
			InitializeComponent();
			this.loaded = false;
			this.realMax = max;
			this.Title = title;
			this.percentageMax = 100;
			this.max = 1;
			this.min = 1;
			this.keep = 0;
			this.percentage = false;
			this.sendAll = false;
			this.itemType = (sendType is ItemTypes ? (ItemTypes)sendType : ItemTypes.Unknown);
			this.isDecoration = false;
			this.pocket = this.itemType;
			this.maxMode = false;
			this.withdrawMode = withdrawMode;

			if (sendType is DecorationTypes) {
				this.comboBoxPockets.Items.Add(ItemDatabase.GetDecorationContainerName((DecorationTypes)sendType));
				this.comboBoxPockets.SelectedIndex = 0;
				this.comboBoxPockets.IsEnabled = false;
				this.isDecoration = true;
			}
			this.numericMax.Maximum = max;
			this.numericMin.Maximum = max;
			this.numericKeep.Maximum = max;

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
					if (!this.isDecoration && gameSave.GameType != GameTypes.Any && !exclusives.HasFlag((GameTypeFlags)(1 << ((int)gameSave.GameType - 1))))
						comboBoxGames.SetGameSaveVisible(i, false);
					else if (gameSave.GameType == GameTypes.PokemonBox || (this.isDecoration && gameSave.Inventory.Decorations == null))
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

		public static AdvancedSendSelectionResults ShowDialog(Window owner, int gameIndex, int max, string title, object sendType, bool withdrawMode, GameTypeFlags exclusives = GameTypeFlags.AllGen3) {
			AdvancedSendSelectionToWindow window = new AdvancedSendSelectionToWindow(gameIndex, max, title, sendType, withdrawMode, exclusives);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result.HasValue && result.Value) {
				if (!withdrawMode)
					PokeManager.LastGameInDialogIndex = window.gameIndex;
				AdvancedSendSelectionResults results = new AdvancedSendSelectionResults();
				results.Max = (window.sendAll ? window.realMax : (window.percentage ? window.percentageMax : window.max));
				results.Min = (window.sendAll ? 0 : window.min);
				results.Keep = (window.sendAll ? 0 : window.keep);
				results.AsPercentage = (window.sendAll ? false : window.percentage);
				results.GameIndex = window.gameIndex;
				results.Pocket = window.pocket;
				return results;
			}
			return null;
		}

		private IGameSave GameSave {
			get { return PokeManager.GetGameSaveAt(gameIndex); }
		}

		private void OnPercentageChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			this.percentage = this.checkBoxPercentage.IsChecked.Value;

			this.maxMode = false;
			this.labelMax.Content = (percentage ? "Total %" : "Most");
			this.numericMax.Maximum = (percentage ? 100 : realMax);
			this.numericMax.Value = (percentage ? percentageMax : max);
			this.checkBoxMax.IsEnabled = !percentage;
			this.checkBoxMax.IsChecked = false;
		}

		private void OnSendAllChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			this.sendAll = this.checkBoxSendAll.IsChecked.Value;

			this.maxMode = false;
			this.percentage = false;
			this.labelMax.Content = "Most";
			this.checkBoxMax.IsChecked = false;
			this.checkBoxMax.IsEnabled = !percentage;
			this.checkBoxPercentage.IsChecked = false;
			this.checkBoxPercentage.IsEnabled = !sendAll;
			this.numericMax.Value = (sendAll ? realMax : max);
			this.numericMin.Value = (sendAll ? realMax : min);
			this.numericKeep.Value = (sendAll ? 0 : keep);
			this.numericMax.IsEnabled = !sendAll;
			this.numericMin.IsEnabled = !sendAll;
			this.numericKeep.IsEnabled = !sendAll;
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

		private void OnMinValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			min = numericMin.Value;
		}

		private void OnKeepValueChanged(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			keep = numericKeep.Value;
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

		private void OnMaxChecked(object sender, RoutedEventArgs e) {
			this.maxMode = this.checkBoxMax.IsChecked.Value;

			this.numericMax.IsEnabled = !maxMode;
			this.numericMax.Value = (maxMode ? realMax : max);
		}
	}
}
