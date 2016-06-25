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
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class SendItemToWindow : Window {

		private uint count;
		private int gameIndex;
		private ItemTypes pocket;
		private List<ItemTypes> pocketTypes;
		private ushort itemID;
		private bool loaded;

		public SendItemToWindow(int gameIndex, ushort id, uint maxCount) {
			InitializeComponent();
			upDownItemCount.Minimum = 0;
			upDownItemCount.Value = 1;
			upDownItemCount.Maximum = (int)maxCount;
			itemID = id;
			pocketTypes = new List<ItemTypes>();
			loaded = false;
			count = 1;

			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				if (i == gameIndex) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
				}

				IGameSave game = PokeManager.GetGameSaveAt(i);
				if (game.GameType != GameTypes.Any && !ItemDatabase.GetItemFromID(id).Exclusives.HasFlag((GameTypeFlags)(1 << ((int)game.GameType - 1)))) {
					comboBoxGame.SetGameSaveVisible(i, false);
				}
			}

			pocketTypes.Clear();
			comboBoxPocket.Items.Clear();
			/*if (this.gameIndex == -1) {
				pocketTypes.Add(ItemDatabase.GetItemFromID(itemID).PocketType);
				comboBoxPocket.Items.Add(ItemDatabase.GetPocketName(ItemDatabase.GetItemFromID(itemID).PocketType));
				comboBoxPocket.SelectedIndex = 0;
			}
			else {
				pocketTypes.Add(ItemTypes.PC);
				pocketTypes.Add(ItemDatabase.GetItemFromID(itemID).PocketType);
				comboBoxPocket.Items.Add(ItemDatabase.GetPocketName(ItemTypes.PC));
				comboBoxPocket.Items.Add(ItemDatabase.GetPocketName(ItemDatabase.GetItemFromID(itemID).PocketType));
				comboBoxPocket.SelectedIndex = 1;
			}*/

			this.gameIndex = PokeManager.LastGameInDialogIndex;
			if (this.gameIndex == -2 || !comboBoxGame.IsGameSaveVisible(this.gameIndex)) {
				//comboBoxGame.SelectedIndex = 0;
				this.gameIndex = comboBoxGame.SelectedGameIndex;
			}
			else {
				comboBoxGame.SelectedGameIndex = this.gameIndex;
			}

			loaded = true;

			GameChanged(null, null);
		}
		public static SendItemToResult ShowDialog(Window owner, int gameIndex, ushort id, uint maxCount) {
			SendItemToWindow window = new SendItemToWindow(gameIndex, id, maxCount);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result != null && result.Value) {
				SendItemToResult sendItemResult = new SendItemToResult();
				sendItemResult.Count = window.count;
				sendItemResult.GameIndex = window.gameIndex;
				sendItemResult.Pocket = window.pocket;
				return sendItemResult;
			}
			return null;
		}

		private void OnValueChanged(object sender, RoutedEventArgs e) {
			count = (uint)((NumericUpDown)sender).Value;
		}

		private void GameChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;

			gameIndex = comboBoxGame.SelectedGameIndex;

			if (gameIndex == -2) {
				
			}
			else {
				comboBoxPocket.Items.Clear();
				TryAddPocket(ItemTypes.PC);
				TryAddPocket(ItemDatabase.GetItemFromID(itemID).PocketType);
				comboBoxPocket.SelectedIndex = 0;
			}
		}

		private void TryAddPocket(ItemTypes pocket) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
			//if (pocket != ItemTypes.Items || !gameSave.Inventory.Items.ContainsPocket(ItemTypes.InBattle)) {
				if (gameSave.Inventory.Items.ContainsPocket(pocket)) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = ItemDatabase.GetPocketName(pocket);
					comboBoxItem.Tag = pocket;
					comboBoxPocket.Items.Add(comboBoxItem);
				}
			//}
		}

		private void PocketChanged(object sender, SelectionChangedEventArgs e) {
			if (gameIndex != -2 && comboBoxPocket.SelectedItem != null) {
				pocket = (ItemTypes)((ComboBoxItem)comboBoxPocket.SelectedItem).Tag;
			}
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
			comboBoxGame.SelectedGameIndex = gameIndex;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			upDownItemCount.SelectAll();
			if (comboBoxGame.SelectedGameIndex == -2) {
				TriggerMessageBox.Show(this, "No available games to send with", "Can't Send");
				Close();
			}
		}
	}

	public class SendItemToResult {

		public uint Count { get; set; }
		public int GameIndex { get; set; }
		public ItemTypes Pocket { get; set; }
	}

}
