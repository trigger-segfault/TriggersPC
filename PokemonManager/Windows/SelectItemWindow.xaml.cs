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
	public partial class SelectItemWindow : Window {

		private bool loaded;
		private int gameIndex;
		private ItemPocket pocket;
		private ushort[] validItemIDs;

		private int selectedIndex;
		private Item selectedItem;
		private ItemTypes[] pocketTypes;

		private bool pcDefault;

		public SelectItemWindow(ushort[] validItemIDs, ItemTypes[] pocketTypes, string windowName, string buttonName, bool pcDefault) {
			InitializeComponent();
			selectedIndex = -1;
			selectedItem = null;
			this.validItemIDs = validItemIDs;
			this.pocketTypes = pocketTypes;
			this.Title = windowName;
			this.buttonOK.Content = buttonName;
			this.pcDefault = pcDefault;

			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				IGameSave game = PokeManager.GetGameSaveAt(i);
				if (game.GameType == GameTypes.PokemonBox) {
					comboBoxGame.SetGameSaveVisible(i, false);
				}
			}

			comboBoxGame.SelectedGameIndex = -1;

			//comboBoxPocket.Items.Add("Items");
			//comboBoxPocket.SelectedIndex = 0;
			loaded = true;

			OnGameSelectionChanged(null, null);
		}

		public static Item ShowDialog(Window owner, ushort[] validItemIDs, ItemTypes[] pocketTypes, string windowName, string buttonName, bool pcDefault) {
			SelectItemWindow window = new SelectItemWindow(validItemIDs, pocketTypes, windowName, buttonName, pcDefault);
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

			ItemPocket[] pockets = { pocket };
			if (pocket.PocketType == ItemTypes.Items && pocket.Inventory.ContainsPocket(ItemTypes.InBattle)) {
				pockets = new ItemPocket[]{
					pocket.Inventory[ItemTypes.InBattle],
					pocket.Inventory[ItemTypes.Valuables],
					pocket.Inventory[ItemTypes.Misc],
					pocket.Inventory[ItemTypes.Hold]
				};
			}

			foreach (ItemPocket itemPocket in pockets) {
				for (int i = 0; i < itemPocket.SlotsUsed; i++) {
					Item item = itemPocket[i];
					bool isValid = false;
					foreach (ushort id in validItemIDs) {
						if (item.ID == id) {
							isValid = true;
							break;
						}
					}
					if (isValid) {
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
						if (!item.ItemData.IsImportant) {
							dockPanel.Children.Add(itemCount);
							dockPanel.Children.Add(itemX);

							DockPanel.SetDock(itemCount, Dock.Right);
						}
						DockPanel.SetDock(image, Dock.Left);
					}
				}
			}
		}

		private void OnGameSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;

			gameIndex = comboBoxGame.SelectedGameIndex;

			comboBoxPocket.Items.Clear();
			TryAddPocket(ItemTypes.PC);
			foreach (ItemTypes pocketType in pocketTypes) {
				TryAddPocket(pocketType);
			}
			if ((ItemTypes)(comboBoxPocket.Items[0] as ComboBoxItem).Tag == ItemTypes.PC && !pcDefault)
				comboBoxPocket.SelectedIndex = 1;
			else
				comboBoxPocket.SelectedIndex = 0;
			UpdateItemList();
		}

		private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e) {
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
			if (!loaded)
				return;

			if (gameIndex != -2 && comboBoxPocket.SelectedIndex != -1) {
				IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
				pocket = gameSave.Inventory.Items[(ItemTypes)(comboBoxPocket.SelectedItem as ComboBoxItem).Tag];
				UpdateItemList();
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
	}
}
