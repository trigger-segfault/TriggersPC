using PokemonManager.Game;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {

	public partial class DecorationViewerTab : UserControl {

		private DecorationPocket pocket;
		private Decoration selectedDecoration;
		private int selectedIndex;
		private ContextMenu contextMenu;
		private int selectionIsInUse;
		private int selectionMax;

		public DecorationViewerTab() {
			InitializeComponent();

			CreateContextMenu();

			labelDecorationName.Content = "";
			textBlockDecorationDescription.Text = "";
		}

		public DecorationPocket Container {
			get { return pocket; }
		}
		private List<Decoration> SelectedDecorations {
			get {
				List<Decoration> selectedDecorations = new List<Decoration>();
				for (int i = 0; i < listViewItems.SelectedItems.Count; i++) {
					selectedDecorations.Add((listViewItems.SelectedItems[i] as ListViewItem).Tag as Decoration);
				}
				return selectedDecorations;
			}
		}
		private bool HasSelection {
			get { return listViewItems.SelectedItems.Count > 1; }
		}
		public void GotoDecoration(byte decorationID) {
			int index = -1;
			for (int i = 0; i < pocket.SlotsUsed; i++) {
				if (pocket[i].ID == decorationID) {
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

		public void LoadPocket(DecorationPocket pocket) {
			this.pocket = pocket;
			pocket.AddListViewItem += OnAddListViewItem;
			pocket.UpdateListViewItem += OnUpdateListViewItem;
			pocket.RemoveListViewItem += OnRemoveListViewItem;
			listViewItems.ItemsSource = pocket.ListViewItems;
			buySellInfo.UnloadBuySellInfo();
			UpdateDetails();

			pocket.RepopulateListView();
		}
		public void UnloadPocket() {
			listViewItems.ItemsSource = null;
			pocket.AddListViewItem -= OnAddListViewItem;
			pocket.UpdateListViewItem -= OnUpdateListViewItem;
			pocket.RemoveListViewItem -= OnRemoveListViewItem;
			buySellInfo.UnloadBuySellInfo();
		}

		private void OnDecorationListSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int index = listViewItems.SelectedIndex;
			if (index < pocket.SlotsUsed) {
				if (index != -1)
					selectedIndex = index;
				selectedDecoration = pocket[selectedIndex];
				if (selectedIndex != -1) {
					DecorationData decorationData = pocket[selectedIndex].DecorationData;
					BitmapSource image = ItemDatabase.GetDecorationFullSizeImageFromID(decorationData.ID);
					imageDecoration.Source = image;
					imageDecoration.Width = image.Width;
					imageDecoration.Height = image.Height;
					labelDecorationName.Content = decorationData.Name;
					textBlockDecorationDescription.Text = decorationData.Description;
					buySellInfo.LoadBuySellInfo(decorationData);
				}
				else {
					labelDecorationName.Content = "";
					textBlockDecorationDescription.Text = "";
					imageDecoration.Source = null;
					buySellInfo.UnloadBuySellInfo();
				}
			}
		}
		private void OnDecorationPutAway(object sender, EventArgs e) {
			if (HasSelection) {
				MessageBoxResult result = TriggerMessageBox.Show(Window.GetWindow(this), "Would you like to put away the selected decorations?", "Put Away Decoration Selection", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					foreach (Decoration decoration in SelectedDecorations) {
						if (pocket.Inventory.IsDecorationInUse(pocket.IndexOf(decoration), pocket.PocketType)) {
							pocket.Inventory.PutAwayDecoration(pocket.IndexOf(decoration), pocket.PocketType);
						}
					}
				}
			}
			else {
				DecorationUsages usage = pocket.Inventory.GetDecorationUsage(selectedIndex, pocket.PocketType);
				MessageBoxResult result = MessageBoxResult.No;
				string usageString = (usage == DecorationUsages.SecretBase ? "Secret Base" : "Bedroom");
				if (usage != DecorationUsages.Unused)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Put away the " + selectedDecoration.DecorationData.Name + " in your " + usageString + "?", "Put Away", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					pocket.Inventory.PutAwayDecoration(selectedIndex, pocket.PocketType);
			}
		}

		private void OnDecorationToss(object sender, EventArgs e) {
			if (HasSelection) {
				var results = AdvancedSendSelectionToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, selectionMax, "Toss Decoration Selection", pocket.PocketType, true);
				if (results != null) {
					MessageBoxResult result = MessageBoxResult.Yes;
					if (selectionIsInUse != 0)
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Some of the selected decorations are in use. Would you like to put them away?", "Decoration Selection In Use", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes && PokeManager.Settings.TossConfirmation)
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + listViewItems.SelectedItems.Count + " decorations?", "Toss Decoration Selection", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes) {
						foreach (Decoration decoration in SelectedDecorations) {
							if (pocket.Inventory.IsDecorationInUse(pocket.IndexOf(decoration), pocket.PocketType)) {
								pocket.Inventory.PutAwayDecoration(pocket.IndexOf(decoration), pocket.PocketType);
							}
							int finalCount = results.GetFinalCount(decoration.Count);
							if (finalCount > 0) {
								pocket.TossDecorationAt(pocket.IndexOf(decoration), (uint)finalCount);
							}
						}
					}
				}
			}
			else {
				if (selectedDecoration.Count == 1) {
					OnDecorationTossAll(sender, e);
				}
				else {
					var results = AdvancedSendSingleToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, (int)selectedDecoration.Count, "Toss Decoration", pocket.PocketType, true);
					//int? count = ItemCountWindow.ShowDialog(Window.GetWindow(this), "Toss", 1, (int)selectedDecoration.Count);
					if (results != null) {
						MessageBoxResult result = MessageBoxResult.Yes;
						if (pocket.AreAllDecorationsOfIDInUse(selectedDecoration.ID)) {
							DecorationUsages usage = pocket.Inventory.GetDecorationUsage(selectedIndex, pocket.PocketType);
							string usageString = (usage == DecorationUsages.SecretBase ? "Secret Base" : "Bedroom");
							result = TriggerMessageBox.Show(Window.GetWindow(this), "Put away the " + selectedDecoration.DecorationData.Name + " in your " + usageString + "?", "Put Away", MessageBoxButton.YesNo);
						}
						if (result == MessageBoxResult.Yes && PokeManager.Settings.TossConfirmation) {
							if (results.Count == 1)
								result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedDecoration.DecorationData.Name + "?", "Toss Decoration", MessageBoxButton.YesNo);
							else
								result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + results.Count + " " + selectedDecoration.DecorationData.Name + "s?", "Toss Decoration", MessageBoxButton.YesNo);
						}
						if (result == MessageBoxResult.Yes)
							pocket.TossDecorationAt(selectedIndex, (uint)results.Count);
					}
				}
			}
		}

		private void OnDecorationTossAll(object sender, EventArgs e) {
			if (HasSelection) {
				MessageBoxResult result = MessageBoxResult.Yes;
				if (selectionIsInUse != 0)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Some of the selected decorations are in use. Would you like to put them away?", "Decoration Selection In Use", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes && PokeManager.Settings.TossConfirmation)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + listViewItems.SelectedItems.Count + " decorations?", "Toss Decoration Selection", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					foreach (Decoration decoration in SelectedDecorations) {
						if (pocket.Inventory.IsDecorationInUse(pocket.IndexOf(decoration), pocket.PocketType)) {
							pocket.Inventory.PutAwayDecoration(pocket.IndexOf(decoration), pocket.PocketType);
						}
						pocket.TossDecorationAt(pocket.IndexOf(decoration), decoration.Count);
					}
				}
			}
			else {
				MessageBoxResult result = MessageBoxResult.Yes;
				if (pocket.AreAllDecorationsOfIDInUse(selectedDecoration.ID)) {
					DecorationUsages usage = pocket.Inventory.GetDecorationUsage(selectedIndex, pocket.PocketType);
					string usageString = (usage == DecorationUsages.SecretBase ? "Secret Base" : "Bedroom");
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Put away the " + selectedDecoration.DecorationData.Name + " in your " + usageString + "?", "Put Away", MessageBoxButton.YesNo);
				}
				if (result == MessageBoxResult.Yes && PokeManager.Settings.TossConfirmation) {
					if (selectedDecoration.Count == 1)
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedDecoration.DecorationData.Name + "?", "Toss Decoration", MessageBoxButton.YesNo);
					else
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedDecoration.Count + " " + selectedDecoration.DecorationData.Name + "s?", "Toss Decoration", MessageBoxButton.YesNo);
				}
				if (result == MessageBoxResult.Yes)
					pocket.TossDecorationAt(selectedIndex);
			}
		}

		private void OnDecorationSendTo(object sender, EventArgs e) {
			if (HasSelection) {
				var results = AdvancedSendSelectionToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, selectionMax, "Send Decoration Selection", pocket.PocketType, false);
				if (results != null) {
					MessageBoxResult result = MessageBoxResult.Yes;
					if (selectionIsInUse != 0)
						result = TriggerMessageBox.Show(Window.GetWindow(this), "Some of the selected decorations are in use. Would you like to put them away?", "Decoration Selection In Use", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes) {
						bool noRoom = false;
						foreach (Decoration decoration in SelectedDecorations) {
							if (pocket.Inventory.IsDecorationInUse(pocket.IndexOf(decoration), pocket.PocketType)) {
								pocket.Inventory.PutAwayDecoration(pocket.IndexOf(decoration), pocket.PocketType);
							}
							int finalCount = results.GetFinalCount(decoration.Count);
							if (finalCount > 0) {
								if (PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Decorations[pocket.PocketType].HasRoomForDecoration(decoration.ID, (uint)finalCount)) {
									PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Decorations[pocket.PocketType].AddDecoration(decoration.ID, (uint)finalCount);
									pocket.TossDecorationAt(pocket.IndexOf(decoration), (uint)finalCount);
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
			else {
				var results = AdvancedSendSingleToWindow.ShowDialog(Window.GetWindow(this), pocket.GameSave.GameIndex, (int)selectedDecoration.Count, "Send Decoration", pocket.PocketType, false);
				//SendDecorationToResult result = SendDecorationToWindow.ShowDialog(Window.GetWindow(this), pocket.Inventory.GameIndex, selectedDecoration.ID, selectedDecoration.Count);
				if (results != null) {
					bool cancel = false;
					if (pocket.AreAllDecorationsOfIDInUse(selectedDecoration.ID)) {
						DecorationUsages usage = pocket.Inventory.GetDecorationUsage(selectedIndex, pocket.PocketType);
						string usageString = (usage == DecorationUsages.SecretBase ? "Secret Base" : "Bedroom");
						MessageBoxResult result2 = TriggerMessageBox.Show(Window.GetWindow(this), "Put away the " + selectedDecoration.DecorationData.Name + " in your " + usageString + "?", "Put Away", MessageBoxButton.YesNo);
						if (result2 == MessageBoxResult.No)
							cancel = true;
					}
					if (!cancel) {
						if (PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Decorations[pocket.PocketType].HasRoomForDecoration(selectedDecoration.ID, (uint)results.Count)) {
							PokeManager.GetGameSaveAt(results.GameIndex).Inventory.Decorations[pocket.PocketType].AddDecoration(selectedDecoration.ID, (uint)results.Count);
							pocket.TossDecorationAt(selectedIndex, (uint)results.Count);
						}
						else {
							// No room for item
							TriggerMessageBox.Show(Window.GetWindow(this), "No room for that decoration", "No Room");
						}
					}
				}
			}
		}

		public void UpdateDetails() {
			labelPocket.Content = ItemDatabase.GetDecorationContainerName(pocket.PocketType) + "   " + pocket.SlotsUsed + "/" + (pocket.TotalSlots == 0 ? "∞" : pocket.TotalSlots.ToString());
		}

		private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
			if (selectedDecoration != null) {
				((MenuItem)contextMenu.Items[0]).IsEnabled = true;
				((MenuItem)contextMenu.Items[3]).IsEnabled = true;
				((MenuItem)contextMenu.Items[4]).IsEnabled = true;
				if (HasSelection) {
					selectionIsInUse = -1;
					selectionMax = 0;
					foreach (Decoration decoration in SelectedDecorations) {
						selectionMax = Math.Max(selectionMax, (int)decoration.Count);
						if (pocket.Inventory.IsDecorationInUse(pocket.IndexOf(decoration), decoration.DecorationData.DecorationType)) {
							if (selectionIsInUse == -1)
								selectionIsInUse = 1;
							else if (selectionIsInUse == 0)
								selectionIsInUse = 2;
						}
						else {
							if (selectionIsInUse == -1)
								selectionIsInUse = 0;
							else if (selectionIsInUse == 1)
								selectionIsInUse = 2;
						}
					}
					((MenuItem)contextMenu.Items[1]).IsEnabled = selectionIsInUse != 0;
				}
				else {
					((MenuItem)contextMenu.Items[1]).IsEnabled = pocket.Inventory.IsDecorationInUse(selectedIndex, pocket.PocketType);
				}
			}
			else {
				((MenuItem)contextMenu.Items[0]).IsEnabled = false;
				((MenuItem)contextMenu.Items[1]).IsEnabled = false;
				((MenuItem)contextMenu.Items[3]).IsEnabled = false;
				((MenuItem)contextMenu.Items[4]).IsEnabled = false;
			}
			((MenuItem)contextMenu.Items[4]).Visibility = (pocket.MaxStackSize == 0 ? Visibility.Visible : Visibility.Collapsed);
		}

		public void OnUpdateListViewItem(object sender, DecorationPocketEventArgs e) {
			((Image)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[0]).Source = ItemDatabase.GetDecorationImageFromID(e.Decoration.ID);
			((TextBlock)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[1]).Text = ItemDatabase.GetDecorationFromID(e.Decoration.ID).Name;
			DecorationUsages usage = pocket.Inventory.GetDecorationUsage(e.Index, pocket.PocketType);
			if (usage == DecorationUsages.SecretBase)
				((Image)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[2]).Source = ResourceDatabase.GetImageFromName("DecorationSecretBase");
			else if (usage == DecorationUsages.Bedroom)
				((Image)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[2]).Source = ResourceDatabase.GetImageFromName("DecorationBedroom");
			else
				((Image)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[2]).Source = null;
			((TextBlock)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[4]).Text = e.Decoration.Count.ToString();
			int usageCount = pocket.Inventory.GetNumDecorationsWithIDInUse(e.Decoration.ID);
			((TextBlock)((DockPanel)pocket.ListViewItems[e.Index].Content).Children[3]).Text = (usageCount > 0 && pocket.MaxStackSize == 0 ? usageCount.ToString() : "");
			pocket.ListViewItems[e.Index].Tag = e.Decoration;
		}
		public void OnAddListViewItem(object sender, DecorationPocketEventArgs e) {
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.SnapsToDevicePixels = true;
			listViewItem.UseLayoutRounding = true;
			DockPanel dockPanel = new DockPanel();
			dockPanel.Width = 300;

			Image image = new Image();
			image.Source = ItemDatabase.GetDecorationImageFromID(e.Decoration.ID);
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;

			TextBlock itemName = new TextBlock();
			itemName.VerticalAlignment = VerticalAlignment.Center;
			itemName.Text = ItemDatabase.GetDecorationFromID(e.Decoration.ID).Name;
			itemName.TextTrimming = TextTrimming.CharacterEllipsis;
			itemName.Margin = new Thickness(4, 0, 0, 0);

			Image usedImage = new Image();
			usedImage.Stretch = Stretch.None;
			usedImage.SnapsToDevicePixels = true;
			usedImage.UseLayoutRounding = true;
			usedImage.HorizontalAlignment = HorizontalAlignment.Left;
			usedImage.Margin = new Thickness(6, 0, 0, 0);
			DecorationUsages usage = pocket.Inventory.GetDecorationUsage(e.Index, pocket.PocketType);
			if (usage == DecorationUsages.SecretBase)
				usedImage.Source = ResourceDatabase.GetImageFromName("DecorationSecretBase");
			else if (usage == DecorationUsages.Bedroom)
				usedImage.Source = ResourceDatabase.GetImageFromName("DecorationBedroom");

			TextBlock usedCount = new TextBlock();
			usedCount.VerticalAlignment = VerticalAlignment.Center;
			usedCount.Margin = new Thickness(4, 0, 0, 0);
			int usageCount = pocket.Inventory.GetNumDecorationsWithIDInUse(e.Decoration.ID);
			usedCount.Text = (usageCount > 0 && pocket.MaxStackSize == 0 ? usageCount.ToString() : "");

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
			itemCount.Text = e.Decoration.Count.ToString();

			listViewItem.Content = dockPanel;
			pocket.ListViewItems.Insert(e.Index, listViewItem);
			dockPanel.Children.Add(image);
			dockPanel.Children.Add(itemName);
			dockPanel.Children.Add(usedImage);
			dockPanel.Children.Add(usedCount);
			dockPanel.Children.Add(itemCount);
			dockPanel.Children.Add(itemX);
			if (pocket.MaxStackSize != 0) {
				itemCount.Visibility = Visibility.Hidden;
				itemX.Visibility = Visibility.Hidden;
			}
			else {
				usedCount.Visibility = Visibility.Hidden;
			}

			listViewItem.ContextMenuOpening += OnContextMenuOpening;
			listViewItem.ContextMenu = contextMenu;


			DockPanel.SetDock(image, Dock.Left);
			DockPanel.SetDock(itemCount, Dock.Right);

			listViewItem.Tag = e.Decoration;
			UpdateDetails();
		}
		public void OnRemoveListViewItem(object sender, DecorationPocketEventArgs e) {
			if (e.Index == selectedIndex) {
				selectedDecoration = null;
				selectedIndex = -1;
			}
			pocket.ListViewItems.RemoveAt(e.Index);
			
			UpdateDetails();
		}

		private void CreateContextMenu() {
			contextMenu = new ContextMenu();

			MenuItem menuItemSendTo = new MenuItem();
			menuItemSendTo.Header = "Send To";
			menuItemSendTo.Click += OnDecorationSendTo;
			contextMenu.Items.Add(menuItemSendTo);

			MenuItem menuItemPutAway = new MenuItem();
			menuItemPutAway.Header = "Put Away";
			menuItemPutAway.Click += OnDecorationPutAway;
			contextMenu.Items.Add(menuItemPutAway);

			contextMenu.Items.Add(new Separator());

			MenuItem menuItemToss = new MenuItem();
			menuItemToss.Header = "Toss";
			menuItemToss.Click += OnDecorationToss;
			contextMenu.Items.Add(menuItemToss);

			MenuItem menuItemTossAll = new MenuItem();
			menuItemTossAll.Header = "Toss All";
			menuItemTossAll.Click += OnDecorationTossAll;
			contextMenu.Items.Add(menuItemTossAll);
		}
	}
}
