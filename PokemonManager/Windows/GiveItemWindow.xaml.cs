using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
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
	/// Interaction logic for ReplaceBallWindow.xaml
	/// </summary>
	public partial class GiveItemWindow : Window {

		private int gameIndex;
		private ItemPocket pocket;
		private bool loaded;

		private int selectedIndex;
		private Item selectedItem;

		public GiveItemWindow(IGameSave gameSave) {
			InitializeComponent();
			selectedIndex = -1;
			selectedItem = null;
			this.gameIndex = gameSave.GameIndex;

			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				IGameSave game = PokeManager.GetGameSaveAt(i);
				if (game.GameType == GameTypes.PokemonBox) {
					comboBoxGame.SetGameSaveVisible(i, false);
				}
			}

			if (gameSave.GameType != GameTypes.PokemonBox) {
				comboBoxGame.SelectedGameIndex = gameIndex;
			}
			else {
				comboBoxGame.SelectedGameIndex = -1;
			}

			loaded = true;
			OnGameSelectionChanged(null, null);
		}

		public static Item ShowDialog(Window owner, IGameSave gameSave) {
			GiveItemWindow window = new GiveItemWindow(gameSave);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result != null && result.Value) {
				return window.selectedItem;
			}
			return null;
		}

		private void UpdateItemList() {
			listViewItems.Items.Clear();
			selectedIndex = -1;
			selectedItem = null;

			for (int i = 0; i < pocket.SlotsUsed; i++) {
				Item item = pocket[i];
				if (!item.ItemData.IsImportant && (item.ID < 121 || (item.ID > 132 && item.ID < 500))) {
					ListViewItem listViewItem = new ListViewItem();
					listViewItem.Tag = item;
					listViewItem.SnapsToDevicePixels = true;
					listViewItem.UseLayoutRounding = true;
					DockPanel dockPanel = new DockPanel();
					dockPanel.Width = 170;

					Image image = new Image();
					image.Width = 12;
					image.Height = 12;
					image.Source = ItemDatabase.GetItemImageFromID(item.ID);
					image.Stretch = Stretch.Uniform;
					image.SnapsToDevicePixels = true;
					image.UseLayoutRounding = true;
					image.HorizontalAlignment = HorizontalAlignment.Center;
					image.VerticalAlignment = VerticalAlignment.Center;

					TextBlock itemName = new TextBlock();
					itemName.VerticalAlignment = VerticalAlignment.Center;
					itemName.Text = ItemDatabase.GetItemFromID(item.ID).Name;
					itemName.TextTrimming = TextTrimming.CharacterEllipsis;
					itemName.Margin = new Thickness(4, 0, 0, 0);

					TextBlock itemX = new TextBlock();
					itemX.VerticalAlignment	= VerticalAlignment.Center;
					itemX.HorizontalAlignment = HorizontalAlignment.Right;
					itemX.TextAlignment = TextAlignment.Right;
					itemX.Text = "x";
					itemX.Width = Double.NaN;
					itemX.MinWidth = 10;

					TextBlock itemCount = new TextBlock();
					itemCount.VerticalAlignment	= VerticalAlignment.Center;
					itemCount.HorizontalAlignment = HorizontalAlignment.Right;
					itemCount.TextAlignment = TextAlignment.Right;
					itemCount.Width = 30;
					itemCount.Text = item.Count.ToString();

					listViewItem.ToolTip = item.ItemData.Description;
					listViewItem.Content = dockPanel;
					listViewItems.Items.Add(listViewItem);
					dockPanel.Children.Add(image);
					dockPanel.Children.Add(itemName);

					DockPanel.SetDock(image, Dock.Left);

					if (!item.ItemData.IsImportant) {
						dockPanel.Children.Add(itemCount);
						dockPanel.Children.Add(itemX);
						DockPanel.SetDock(itemCount, Dock.Right);
					}

				}
			}
		}

		/*private void AddPocketListViewItem(ItemTypes pocketType) {
			ComboBoxItem comboBoxItem = new ComboBoxItem();
			comboBoxItem.Content = ItemDatabase.GetPocketName(pocketType);
			comboBoxItem.Tag = pocketType;
			comboBoxPocket.Items.Add(comboBoxItem);
		}*/

		private void TryAddPocket(ItemTypes pocket) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
			if (pocket != ItemTypes.Items || !gameSave.Inventory.Items.ContainsPocket(ItemTypes.InBattle)) {
				if (gameSave.Inventory.Items.ContainsPocket(pocket)) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = ItemDatabase.GetPocketName(pocket);
					comboBoxItem.Tag = pocket;
					comboBoxPocket.Items.Add(comboBoxItem);
				}
			}
		}

		private void OnGameSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			gameIndex = comboBoxGame.SelectedGameIndex;

			comboBoxPocket.Items.Clear();
			TryAddPocket(ItemTypes.PC);
			TryAddPocket(ItemTypes.Items);
			TryAddPocket(ItemTypes.InBattle);
			TryAddPocket(ItemTypes.Valuables);
			TryAddPocket(ItemTypes.Hold);
			TryAddPocket(ItemTypes.Misc);
			TryAddPocket(ItemTypes.PokeBalls);
			TryAddPocket(ItemTypes.Berries);
			TryAddPocket(ItemTypes.TMCase);
			comboBoxPocket.SelectedIndex = (GameSave.Inventory.Items.ContainsPocket(ItemTypes.PC) ? 1 : 0);
			UpdateItemList();
		}

		private IGameSave GameSave {
			get { return PokeManager.GetGameSaveAt(gameIndex); }
		}

		private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			selectedIndex = listViewItems.SelectedIndex;
			if (selectedIndex != -1) {
				selectedItem = ((ListViewItem)listViewItems.Items[selectedIndex]).Tag as Item;
			}
			else {
				selectedItem = null;
			}
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		private void OnPocketSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (gameIndex != -2 && comboBoxPocket.SelectedItem != null) {
				pocket = PokeManager.GetGameSaveAt(gameIndex).Inventory.Items[(ItemTypes)((ComboBoxItem)comboBoxPocket.SelectedItem).Tag];
				UpdateItemList();
			}
		}
	}
}
