using PokemonManager.Game;
using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {

	public partial class ItemViewerTab : UserControl {

		private ItemPocket pocket;
		private Item selectedItem;
		private int selectedIndex;
		private ContextMenu contextMenu;
		private int selectionIsImportant;
		private int selectionMax;

		private ListViewDragDropManager<ListViewItem> dropManager;

		public ItemViewerTab() {
			InitializeComponent();

			labelItemName.Content = "";
			textBlockItemDescription.Text = "";

			CreateContextMenu();
		}

		public ItemPocket Pocket {
			get { return pocket; }
		}
		public void RefreshUI() {
			gridSortButton.Visibility = (pocket.IsOrdered || pocket.PocketType == ItemTypes.KeyItems ? Visibility.Hidden : Visibility.Visible);
		}
		public void GotoItem(ushort itemID) {
			int index = -1;
			for (int i = 0; i < pocket.SlotsUsed; i++) {
				if (pocket[i].ID == itemID) {
					index = i;
					break;
				}
			}
			listViewItems.SelectedIndex = index;
			// Hackish thing to make sure the list view is always scrolled at the bottom when adding a new box
			//http://stackoverflow.com/questions/211971/scroll-wpf-listview-to-specific-line
			/*VirtualizingStackPanel vsp =  
					(VirtualizingStackPanel)typeof(ItemsControl).InvokeMember("_itemsHost",
				BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic, null,
				listViewItems, null);
			double scrollHeight = vsp.ScrollOwner.ScrollableHeight;
			double offset = scrollHeight * index / listViewItems.Items.Count;
			vsp.SetVerticalOffset(offset);*/

			if (index != -1) {
				listViewItems.ScrollIntoView(listViewItems.SelectedItem);
				((Control)listViewItems.SelectedItem).Focus();
			}
		}

		public void LoadPocket(ItemPocket pocket) {
			this.pocket = pocket;
			pocket.AddListViewItem += OnAddListViewItem;
			pocket.UpdateListViewItem += OnUpdateListViewItem;
			pocket.RemoveListViewItem += OnRemoveListViewItem;
			pocket.MoveListViewItem += OnMoveListViewItem;
			listViewItems.ItemsSource = pocket.ListViewItems;
			dropManager = new ListViewDragDropManager<ListViewItem>(listViewItems);
			dropManager.ProcessDrop += OnProcessDrop;
			UpdateDetails();
			buySellInfo.UnloadBuySellInfo();

			pocket.RepopulateListView();

			gridSortButton.Visibility = (pocket.IsOrdered || pocket.PocketType == ItemTypes.KeyItems ? Visibility.Hidden : Visibility.Visible);
		}
		public void UnloadPocket() {
			listViewItems.ItemsSource = null;
			dropManager.ListView = null;
			dropManager.ProcessDrop -= OnProcessDrop;
			pocket.AddListViewItem -= OnAddListViewItem;
			pocket.UpdateListViewItem -= OnUpdateListViewItem;
			pocket.RemoveListViewItem -= OnRemoveListViewItem;
			pocket.MoveListViewItem -= OnMoveListViewItem;
			buySellInfo.UnloadBuySellInfo();
		}

		private void OnItemListSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int index = listViewItems.SelectedIndex;
			if (index < pocket.SlotsUsed) {
				if (index != -1)
					selectedIndex = index;
				selectedItem = pocket[selectedIndex];
				if (selectedIndex != -1 && selectedIndex < pocket.SlotsUsed) {
					ItemData itemData = pocket[selectedIndex].ItemData;
					imageItem.Source = ItemDatabase.GetItemImageFromID(itemData.ID);
					labelItemName.Content = itemData.Name;
					textBlockItemDescription.Text = itemData.Description;
					buySellInfo.LoadBuySellInfo(itemData);
				}
				else {
					selectedIndex = -1;
					labelItemName.Content = "";
					textBlockItemDescription.Text = "";
					imageItem.Source = null;
					buySellInfo.UnloadBuySellInfo();
				}
			}
		}

		private void OnDeposit(object sender, EventArgs e) {
			if (HasSelection) {
				string deposit = (pocket.PocketType == ItemTypes.PC ? "Withdraw" : "Deposit");
				var results = AdvancedSendSelectionToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, selectionMax, deposit + " Item Selection", pocket.PocketType == ItemTypes.PC ? ItemTypes.Any : ItemTypes.PC, true);
				if (results != null) {
					bool noRoom = false;
					foreach (Item item in SelectedItems) {
						ItemTypes pocketType = (pocket.PocketType == ItemTypes.PC ? item.ItemData.PocketType : ItemTypes.PC);
						int finalCount = results.GetFinalCount(item.Count);
						if (finalCount > 0) {
							if (pocket.Inventory[pocketType].HasRoomForItem(item.ID, (uint)finalCount)) {
								pocket.Inventory[pocketType].AddItem(item.ID, (uint)finalCount);
								pocket.TossItemAt(pocket.IndexOf(item), (uint)finalCount);
							}
							else {
								noRoom = true;
							}
						}
					}
					if (noRoom) {
						TriggerMessageBox.Show(Window.GetWindow(this), "The pocket filled up before all of the selection could be sent", "No Room");
					}
				}
			}
			else {
				if (selectedItem.Count == 1) {
					OnDepositAll(null, null);
				}
				else {
					string deposit = (pocket.PocketType == ItemTypes.PC ? "Withdraw" : "Deposit");
					var results = AdvancedSendSingleToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, (int)selectedItem.Count, deposit + " Item", pocket.PocketType == ItemTypes.PC ? selectedItem.ItemData.PocketType : ItemTypes.PC, true);

					//int? count = ItemCountWindow.ShowDialog(Window.GetWindow(this), pocket.PocketType == ItemTypes.PC ? "Withdraw" : "Deposit", 1, (int)selectedItem.Count);
					if (results != null) {
						ItemTypes pocketType = (pocket.PocketType == ItemTypes.PC ? selectedItem.ItemData.PocketType : ItemTypes.PC);
						if (pocket.Inventory[pocketType].HasRoomForItem(selectedItem.ID, (uint)results.Count)) {
							pocket.Inventory[pocketType].AddItem(selectedItem.ID, (uint)results.Count);
							pocket.TossItemAt(selectedIndex, (uint)results.Count);
						}
						else {
							// No room for item
							TriggerMessageBox.Show(Window.GetWindow(this), "No room for that item", "No Room");
						}
					}
				}
			}
		}

		private void OnDepositAll(object sender, EventArgs e) {
			if (HasSelection) {
				bool noRoom = false;
				foreach (Item item in SelectedItems) {
					ItemTypes pocketType = (pocket.PocketType == ItemTypes.PC ? item.ItemData.PocketType : ItemTypes.PC);
					if (pocket.Inventory[pocketType].HasRoomForItem(item.ID, item.Count)) {
						pocket.Inventory[pocketType].AddItem(item.ID, item.Count);
						pocket.TossItemAt(pocket.IndexOf(item), item.Count);
					}
					else {
						noRoom = true;
					}
				}
				if (noRoom) {
					TriggerMessageBox.Show(Window.GetWindow(this), "The pocket filled up before all of the selection could be sent", "No Room");
				}
			}
			else {
				ItemTypes pocketType = (pocket.PocketType == ItemTypes.PC ? selectedItem.ItemData.PocketType : ItemTypes.PC);
				if (pocket.Inventory[pocketType].HasRoomForItem(selectedItem.ID, selectedItem.Count)) {
					pocket.Inventory[pocketType].AddItem(selectedItem.ID, selectedItem.Count);
					pocket.TossItemAt(selectedIndex);
				}
				else {
					// No room for item
					TriggerMessageBox.Show(Window.GetWindow(this), "No room for that item", "No Room");
				}
			}
			
		}

		private void OnGive(object sender, EventArgs e) {
			SendPokemonToWindow.ShowGiveItemDialog(Window.GetWindow(this), selectedItem);
		}

		private void OnToss(object sender, EventArgs e) {
			MessageBoxResult result = MessageBoxResult.Yes;
			if (HasSelection) {
				var results = AdvancedSendSelectionToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, selectionMax, "Toss Item Selection", ItemTypes.TheVoid, true);
				if (results != null) {
					if (PokeManager.Settings.TossConfirmation)
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + listViewItems.SelectedItems.Count + " items?", "Toss Items", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes) {
						foreach (Item item in SelectedItems) {
							int finalCount = results.GetFinalCount(item.Count);
							if (finalCount > 0) {
								pocket.TossItemAt(pocket.IndexOf(item), (uint)finalCount);
							}
						}
					}
				}
			}
			else {
				if (selectedItem.Count == 1) {
					OnTossAll(sender, e);
				}
				else {
					var results = AdvancedSendSingleToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, (int)selectedItem.Count, "Toss Item", ItemTypes.TheVoid, true);
					//int? count = ItemCountWindow.ShowDialog(Window.GetWindow(this), "Toss", 1, (int)selectedItem.Count);
					if (results != null) {
						if (PokeManager.Settings.TossConfirmation) {
							if (results.Count == 1)
								result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedItem.ItemData.Name + "?", "Toss Item", MessageBoxButton.YesNo);
							else
								result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + results.Count + " " + selectedItem.ItemData.Name + "s?", "Toss Item", MessageBoxButton.YesNo);
						}
						if (result == MessageBoxResult.Yes)
							pocket.TossItemAt(selectedIndex, (uint)results.Count);
					}
				}
			}
		}

		private void OnTossAll(object sender, EventArgs e) {
			MessageBoxResult result = MessageBoxResult.Yes;
			if (HasSelection) {
				if (PokeManager.Settings.TossConfirmation)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + listViewItems.SelectedItems.Count + " items?", "Toss Items", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					foreach (Item item in SelectedItems) {
						pocket.TossItemAt(pocket.IndexOf(item), item.Count);
					}
				}
			}
			else {
				if (PokeManager.Settings.TossConfirmation) {
					if (selectedItem.Count == 1)
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedItem.ItemData.Name + "?", "Toss Item", MessageBoxButton.YesNo);
					else
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedItem.Count + " " + selectedItem.ItemData.Name + "s?", "Toss Item", MessageBoxButton.YesNo);
				}
				if (result == MessageBoxResult.Yes)
					pocket.TossItemAt(selectedIndex);
			}
		}

		private void OnSendTo(object sender, EventArgs e) {
			if (HasSelection) {
				if (selectionIsImportant == 1) {
					foreach (Item item in SelectedItems) {
						ItemPocket pocket = PokeManager.ManagerGameSave.Inventory.Items[item.ItemData.PocketType];
						if (pocket.GetCountOfID(item.ID) == 0)
							pocket.AddItem(item.ID, 1);
					}
				}
				else {
					var results = AdvancedSendSelectionToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, selectionMax, "Send Item Selection", SelectedItems[0].ItemData.PocketType, false);
					if (results != null) {
						bool noRoom = false;
						foreach (Item item in SelectedItems) {
							int finalCount = results.GetFinalCount(item.Count);
							if (finalCount > 0) {
								if (PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Items[results.Pocket].HasRoomForItem(item.ID, (uint)finalCount)) {
									PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Items[results.Pocket].AddItem(item.ID, (uint)finalCount);
									pocket.TossItemAt(pocket.IndexOf(item), (uint)finalCount);
								}
								else {
									noRoom = true;
								}
							}
						}
						if (noRoom) {
							TriggerMessageBox.Show(Window.GetWindow(this), "The pocket filled up before all of the selection could be sent", "No Room");
						}
					}
				}
			}
			else if (selectedItem.ItemData.IsImportant) {
				ItemPocket pocket = PokeManager.ManagerGameSave.Inventory.Items[selectedItem.ItemData.PocketType];
				if (pocket.GetCountOfID(selectedItem.ID) == 0)
					pocket.AddItem(selectedItem.ID, 1);
			}
			else {
				var results = AdvancedSendSingleToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, (int)selectedItem.Count, "Send Item", selectedItem.ItemData.PocketType, false);
				//SendItemToResult result = SendItemToWindow.ShowDialog(Window.GetWindow(this), pocket.Inventory.GameIndex, selectedItem.ID, selectedItem.Count);
				if (results != null) {
					if (PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Items[results.Pocket].HasRoomForItem(selectedItem.ID, (uint)results.Count)) {
						PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Items[results.Pocket].AddItem(selectedItem.ID, (uint)results.Count);
						pocket.TossItemAt(selectedIndex, (uint)results.Count);
					}
					else {
						// No room for item
						TriggerMessageBox.Show(Window.GetWindow(this), "No room for that item", "No Room");
					}
				}
			}
		}

		public void UpdateDetails() {
			labelPocket.Content = ItemDatabase.GetPocketName(pocket.PocketType) + "   " + pocket.SlotsUsed + "/" + (pocket.TotalSlots == 0 ? "∞" : pocket.TotalSlots.ToString());
		}

		public void OnUpdateListViewItem(object sender, ItemPocketEventArgs e) {
			((Image)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[0]).Source = ItemDatabase.GetItemImageFromID(e.Item.ID);
			((TextBlock)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[1]).Text = (pocket.PocketType == ItemTypes.Berries ? "No" + (e.Item.ID - 132).ToString("00") + " " : "") + e.Item.ItemData.Name;
			((TextBlock)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[2]).Text = e.Item.Count.ToString();
			pocket.ListViewItems[e.Index].Tag = e.Item;
		}
		public void OnAddListViewItem(object sender, ItemPocketEventArgs e) {
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.SnapsToDevicePixels = true;
			listViewItem.UseLayoutRounding = true;
			DockPanel dockPanel = new DockPanel();
			dockPanel.Width = 300;

			Image image = new Image();
			image.Source = ItemDatabase.GetItemImageFromID(e.Item.ID);
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;

			TextBlock itemName = new TextBlock();
			itemName.VerticalAlignment = VerticalAlignment.Center;
			itemName.Text = (pocket.PocketType == ItemTypes.Berries ? "No" + (e.Item.ID - 132).ToString("00") + " " : "") + e.Item.ItemData.Name;
			itemName.TextTrimming = TextTrimming.CharacterEllipsis;
			itemName.Margin = new Thickness(4, 0, 0, 0);

			TextBlock itemX = new TextBlock();
			itemX.VerticalAlignment	= VerticalAlignment.Center;
			itemX.HorizontalAlignment = HorizontalAlignment.Right;
			itemX.TextAlignment = TextAlignment.Right;
			itemX.Text = "x";
			itemX.Width = Double.NaN;
			itemX.MinWidth = 10;
			itemX.Visibility = (e.Item.ItemData.IsImportant ? Visibility.Hidden : Visibility.Visible);

			TextBlock itemCount = new TextBlock();
			itemCount.VerticalAlignment	= VerticalAlignment.Center;
			itemCount.HorizontalAlignment = HorizontalAlignment.Right;
			itemCount.TextAlignment = TextAlignment.Right;
			itemCount.Width = 30;
			itemCount.Text = e.Item.Count.ToString();
			itemCount.Visibility = (e.Item.ItemData.IsImportant ? Visibility.Hidden : Visibility.Visible);

			listViewItem.Content = dockPanel;
			pocket.ListViewItems.Insert(e.Index, listViewItem);
			dockPanel.Children.Add(image);
			dockPanel.Children.Add(itemName);
			dockPanel.Children.Add(itemCount);
			dockPanel.Children.Add(itemX);

			listViewItem.ContextMenu = contextMenu;
			listViewItem.ContextMenuOpening += OnContextMenuOpening;

			DockPanel.SetDock(image, Dock.Left);
			DockPanel.SetDock(itemCount, Dock.Right);

			listViewItem.Tag = e.Item;

			UpdateDetails();
		}
		public void OnRemoveListViewItem(object sender, ItemPocketEventArgs e) {
			if (e.Index == selectedIndex) {
				selectedItem = null;
				selectedIndex = -1;
			}
			pocket.ListViewItems.RemoveAt(e.Index);

			UpdateDetails();
		}
		public void OnMoveListViewItem(object sender, ItemPocketEventArgs e) {
			ItemPocket pocket = sender as ItemPocket;
			pocket.ListViewItems.Move(e.OldIndex, e.NewIndex);
		}
		public void OnProcessDrop(object sender, ProcessDropEventArgs<ListViewItem> e) {
			if (e.OldIndex > -1) {
				pocket.MoveItem(e.OldIndex, e.NewIndex);
			}

			e.Effects = DragDropEffects.Move;

		}


		private List<Item> SelectedItems {
			get {
				List<Item> selectedItems = new List<Item>();
				for (int i = 0; i < listViewItems.SelectedItems.Count; i++) {
					selectedItems.Add((listViewItems.SelectedItems[i] as ListViewItem).Tag as Item);
				}
				return selectedItems;
			}
		}
		private bool HasSelection {
			get { return listViewItems.SelectedItems.Count > 1; }
		}

		private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
			if (selectedItem != null && selectedItem.ItemData.ID != 0) {
				if (HasSelection) {
					selectionIsImportant = -1;
					selectionMax = 0;
					bool roomInCollection = false;
					bool unknownItem = false;
					ItemTypes itemType = SelectedItems[0].ItemData.PocketType;
					foreach (Item item in SelectedItems) {
						selectionMax = Math.Max(selectionMax, (int)item.Count);
						if (item.ItemData.ID == 0) {
							unknownItem = true;
						}
						else if (item.ItemData.IsImportant) {
							if (selectionIsImportant == -1)
								selectionIsImportant = 1;
							else if (selectionIsImportant == 0)
								selectionIsImportant = 2;
							if (PokeManager.ManagerGameSave.Inventory.Items[item.ItemData.PocketType].GetCountOfID(item.ID) == 0)
								roomInCollection = true;
						}
						else {
							if (selectionIsImportant == -1)
								selectionIsImportant = 0;
							else if (selectionIsImportant == 1)
								selectionIsImportant = 2;
						}
						if (item.ItemData.PocketType != itemType) {
							itemType = ItemTypes.Unknown;
						}
					}

					((MenuItem)contextMenu.Items[0]).Header = (pocket.PocketType == ItemTypes.PC ? "Withdraw" : "Deposit");
					((MenuItem)contextMenu.Items[1]).Header = (pocket.PocketType == ItemTypes.PC ? "Withdraw All" : "Deposit All");
					((MenuItem)contextMenu.Items[2]).Header = (selectionIsImportant == 1 ? "Add to Collection" : "Send To");

					((MenuItem)contextMenu.Items[0]).IsEnabled = !unknownItem && selectionIsImportant == 0 && pocket.Inventory.ContainsPocket(ItemTypes.PC);
					((MenuItem)contextMenu.Items[1]).IsEnabled = !unknownItem && selectionIsImportant == 0 && pocket.Inventory.ContainsPocket(ItemTypes.PC);
					((MenuItem)contextMenu.Items[2]).IsEnabled = !unknownItem && itemType != ItemTypes.Unknown && (selectionIsImportant == 0 || (roomInCollection && selectionIsImportant == 1));
					((MenuItem)contextMenu.Items[3]).IsEnabled = false;
					((MenuItem)contextMenu.Items[5]).IsEnabled = !unknownItem && (selectionIsImportant == 0 || pocket.GameSave.GameIndex == -1);
					((MenuItem)contextMenu.Items[6]).IsEnabled = !unknownItem && (selectionIsImportant == 0 || pocket.GameSave.GameIndex == -1);
				}
				else {
					((MenuItem)contextMenu.Items[0]).Header = (pocket.PocketType == ItemTypes.PC ? "Withdraw" : "Deposit");
					((MenuItem)contextMenu.Items[1]).Header = (pocket.PocketType == ItemTypes.PC ? "Withdraw All" : "Deposit All");
					((MenuItem)contextMenu.Items[2]).Header = (selectedItem.ItemData.IsImportant ? "Add to Collection" : "Send To");

					((MenuItem)contextMenu.Items[0]).IsEnabled = !selectedItem.ItemData.IsImportant && pocket.Inventory.ContainsPocket(ItemTypes.PC);
					((MenuItem)contextMenu.Items[1]).IsEnabled = !selectedItem.ItemData.IsImportant && pocket.Inventory.ContainsPocket(ItemTypes.PC);
					((MenuItem)contextMenu.Items[2]).IsEnabled = (!selectedItem.ItemData.IsImportant || PokeManager.ManagerGameSave.Inventory.Items[selectedItem.ItemData.PocketType].GetCountOfID(selectedItem.ID) == 0);
					((MenuItem)contextMenu.Items[3]).IsEnabled = !selectedItem.ItemData.IsImportant && (selectedItem.ID < 121 || (selectedItem.ID > 132 && selectedItem.ID < 500));
					((MenuItem)contextMenu.Items[5]).IsEnabled = (!selectedItem.ItemData.IsImportant || pocket.GameSave.GameIndex == -1);
					((MenuItem)contextMenu.Items[6]).IsEnabled = (!selectedItem.ItemData.IsImportant || pocket.GameSave.GameIndex == -1);
				}
			}
			else {
				((MenuItem)contextMenu.Items[0]).IsEnabled = false;
				((MenuItem)contextMenu.Items[1]).IsEnabled = false;
				((MenuItem)contextMenu.Items[2]).IsEnabled = false;
				((MenuItem)contextMenu.Items[3]).IsEnabled = false;
				((MenuItem)contextMenu.Items[5]).IsEnabled = false;
				((MenuItem)contextMenu.Items[6]).IsEnabled = false;
			}
		}

		private void CreateContextMenu() {
			contextMenu = new ContextMenu();

			MenuItem menuItem = new MenuItem();
			menuItem.Header = "Deposit";
			menuItem.Click += OnDeposit;
			contextMenu.Items.Add(menuItem);

			menuItem = new MenuItem();
			menuItem.Header = "Deposit All";
			menuItem.Click += OnDepositAll;
			contextMenu.Items.Add(menuItem);

			menuItem = new MenuItem();
			menuItem.Header = "Send To";
			menuItem.Click += OnSendTo;
			contextMenu.Items.Add(menuItem);

			menuItem = new MenuItem();
			menuItem.Header = "Give";
			menuItem.Click += OnGive;
			contextMenu.Items.Add(menuItem);

			contextMenu.Items.Add(new Separator());

			menuItem = new MenuItem();
			menuItem.Header = "Toss";
			menuItem.Click += OnToss;
			contextMenu.Items.Add(menuItem);

			menuItem = new MenuItem();
			menuItem.Header = "Toss All";
			menuItem.Click += OnTossAll;
			contextMenu.Items.Add(menuItem);
		}

		private void OnSortClicked(object sender, RoutedEventArgs e) {
			pocket.Sort();
		}
	}
}
