using PokemonManager.Game;
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
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {
	
	public partial class PokeblockViewerTab : UserControl {

		private PokeblockCase blockCase;
		private Pokeblock selectedBlock;
		private int selectedIndex;
		private ContextMenu contextMenu;

		private ListViewDragDropManager<ListViewItem> dropManager;

		public PokeblockViewerTab() {
			InitializeComponent();

			labelPokeblockName.Content = "";
			labelLevel.Content = "";
			labelFeel.Content = "";
			labelLevelText.Content = "";
			labelFeelText.Content = "";

			stackPanelFlavors1.Children.Clear();
			stackPanelFlavors2.Children.Clear();

			CreateContextMenu();
		}

		private List<Pokeblock> SelectedBlocks {
			get {
				List<Pokeblock> selectedBlocks = new List<Pokeblock>();
				for (int i = 0; i < listViewItems.SelectedItems.Count; i++) {
					selectedBlocks.Add((listViewItems.SelectedItems[i] as ListViewItem).Tag as Pokeblock);
				}
				return selectedBlocks;
			}
		}
		private bool HasSelection {
			get { return listViewItems.SelectedItems.Count > 1; }
		}

		public void LoadPokeblockCase(PokeblockCase blockCase) {
			UnloadPokeblockCase();
			this.blockCase = blockCase;
			blockCase.AddListViewItem += OnAddListViewItem;
			blockCase.RemoveListViewItem += OnRemoveListViewItem;
			blockCase.MoveListViewItem += OnMoveListViewItem;
			listViewItems.ItemsSource = blockCase.ListViewItems;
			dropManager = new ListViewDragDropManager<ListViewItem>(listViewItems);
			dropManager.ProcessDrop += OnProcessDrop;
			UpdateDetails();
			stackPanelFlavors1.Children.Clear();
			stackPanelFlavors2.Children.Clear();

			blockCase.RepopulateListView();
		}
		public void UnloadPokeblockCase() {
			listViewItems.ItemsSource = null;
			if (blockCase != null) {
				dropManager.ListView = null;
				dropManager.ProcessDrop -= OnProcessDrop;
				blockCase.AddListViewItem -= OnAddListViewItem;
				blockCase.RemoveListViewItem -= OnRemoveListViewItem;
				blockCase.MoveListViewItem -= OnMoveListViewItem;
				blockCase = null;
				dropManager = null;
			}
			stackPanelFlavors1.Children.Clear();
			stackPanelFlavors2.Children.Clear();
		}

		private class PokeblockFlavor {
			public PokeblockColors Flavor { get; set; }
			public byte Amount { get; set; }
			public PokeblockFlavor(PokeblockColors flavor, byte amount) {
				this.Flavor = flavor;
				this.Amount = amount;
			}
		}

		private void OnItemListSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int index = listViewItems.SelectedIndex;
			if (index < blockCase.SlotsUsed) {
				if (index != -1)
					selectedIndex = index;
				selectedBlock = blockCase.GetPokeblockAt(selectedIndex);
				if (selectedIndex != -1) {
					labelPokeblockName.Content = selectedBlock.Color.ToString() + " Pokéblock";
					imagePokeblock.Source = ItemDatabase.GetPokeblockImageFromColor(selectedBlock.Color, true);
					labelLevel.Content = selectedBlock.Level.ToString();
					labelFeel.Content = selectedBlock.Feel.ToString();
					labelLevelText.Content = "Level";
					labelFeelText.Content = "Feel";

					stackPanelFlavors1.Children.Clear();
					stackPanelFlavors2.Children.Clear();
					List<PokeblockFlavor> flavors = new List<PokeblockFlavor>();
					flavors.Add(new PokeblockFlavor(PokeblockColors.Red, selectedBlock.Spicyness));
					flavors.Add(new PokeblockFlavor(PokeblockColors.Blue, selectedBlock.Dryness));
					flavors.Add(new PokeblockFlavor(PokeblockColors.Pink, selectedBlock.Sweetness));
					flavors.Add(new PokeblockFlavor(PokeblockColors.Green, selectedBlock.Bitterness));
					flavors.Add(new PokeblockFlavor(PokeblockColors.Yellow, selectedBlock.Sourness));

					flavors.Sort((flavor1, flavor2) => (int)flavor2.Amount - (int)flavor1.Amount);
					foreach (PokeblockFlavor flavor in flavors) {
						if (flavor.Amount != 0)
							AddFlavor(flavor.Flavor, flavor.Amount);
					}
				}
				else {
					labelPokeblockName.Content = "";
					labelLevel.Content = "";
					labelFeel.Content = "";
					labelLevelText.Content = "";
					labelFeelText.Content = "";
					imagePokeblock.Source = null;
					stackPanelFlavors1.Children.Clear();
					stackPanelFlavors2.Children.Clear();
				}
			}
		}

		private void AddFlavor(PokeblockColors color, byte amount) {
			Grid grid = new Grid();
			grid.Height = 22;

			Image image = new Image();
			image.Source = ItemDatabase.GetPokeblockImageFromColor(color, false);
			image.Width = 10;
			image.Height = 10;
			image.HorizontalAlignment = HorizontalAlignment.Left;
			image.VerticalAlignment = VerticalAlignment.Center;
			grid.Children.Add(image);
			Label labelFlavor = new Label();
			labelFlavor.Margin = new Thickness(10, 0, 0, 0);
			labelFlavor.Padding = new Thickness(5, 3, 5, 3);
			labelFlavor.FontWeight = FontWeights.Bold;
			labelFlavor.VerticalAlignment = VerticalAlignment.Center;
			labelFlavor.Content = ((PokeblockFlavorTypes)color).ToString() + "/" + ((ConditionTypes)color).ToString();
			grid.Children.Add(labelFlavor);
			Label labelAmount = new Label();
			labelAmount.Margin = new Thickness(10, 0, 0, 0);
			labelAmount.Padding = new Thickness(5, 3, 5, 3);
			labelAmount.FontWeight = FontWeights.Bold;
			labelAmount.Width = double.NaN;
			labelAmount.HorizontalAlignment = HorizontalAlignment.Stretch;
			labelAmount.HorizontalContentAlignment = HorizontalAlignment.Right;
			labelAmount.VerticalAlignment = VerticalAlignment.Center;
			labelAmount.Content = amount.ToString();
			grid.Children.Add(labelAmount);

			if (stackPanelFlavors1.Children.Count < 3) {
				stackPanelFlavors1.Children.Add(grid);
				stackPanelFlavors1.Height = (stackPanelFlavors1.Children.Count == 3 ? 66 : 44);
			}
			else {
				stackPanelFlavors2.Children.Add(grid);
			}
		}

		private void OnPokeblockToss(object sender, EventArgs e) {
			if (HasSelection) {
				MessageBoxResult result = MessageBoxResult.Yes;
				if (result == MessageBoxResult.Yes && PokeManager.Settings.TossConfirmation)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + listViewItems.SelectedItems.Count + " Pokéblocks?", "Toss Pokéblock Selection", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					foreach (Pokeblock block in SelectedBlocks) {
						blockCase.TossPokeblockAt(blockCase.IndexOf(block));
					}
				}
			}
			else {
				MessageBoxResult result = MessageBoxResult.Yes;
				if (PokeManager.Settings.TossConfirmation)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to toss " + selectedBlock.Color + " Pokéblock?", "Toss Pokéblock", MessageBoxButton.YesNo);

				if (result == MessageBoxResult.Yes)
					blockCase.TossPokeblockAt(selectedIndex);
			}
		}

		private void OnPokeblockSendTo(object sender, EventArgs e) {
			int? result = SendPokeblockToWindow.ShowDialog(Window.GetWindow(this), blockCase.Inventory.GameIndex);
			if (result != null) {
				if (HasSelection) {
					bool noRoom = false;
					foreach (Pokeblock block in SelectedBlocks) {
						if (PokeManager.GetGameSaveAt(result.Value).Inventory.Pokeblocks.HasRoomForPokeblock) {
							PokeManager.GetGameSaveAt(result.Value).Inventory.Pokeblocks.AddPokeblock(block);
							blockCase.TossPokeblockAt(blockCase.IndexOf(block));
						}
						else {
							noRoom = true;
						}
					}
					if (noRoom) {
						TriggerMessageBox.Show(Window.GetWindow(this), "The Pokéblock Case filled up before all of the selection could be sent", "No Room");
					}
				}
				else {
					if (PokeManager.GetGameSaveAt(result.Value).Inventory.Pokeblocks.HasRoomForPokeblock) {
						PokeManager.GetGameSaveAt(result.Value).Inventory.Pokeblocks.AddPokeblock(selectedBlock);
						blockCase.TossPokeblockAt(selectedIndex);
					}
					else {
						// No room for item
						TriggerMessageBox.Show(Window.GetWindow(this), "No room for that Pokéblock", "No Room");
					}
				}
			}
		}

		public void UpdateDetails() {
			labelPocket.Content = "Pokéblocks   " + blockCase.SlotsUsed + "/" + (blockCase.TotalSlots == 0 ? "∞" : blockCase.TotalSlots.ToString());
		}

		public void OnAddListViewItem(object sender, PokeblockCaseEventArgs e) {
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.SnapsToDevicePixels = true;
			listViewItem.UseLayoutRounding = true;
			DockPanel dockPanel = new DockPanel();
			dockPanel.Width = 300;

			Image image = new Image();
			image.Source = ItemDatabase.GetPokeblockImageFromColor(e.Pokeblock.Color, true);
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;

			TextBlock blockName = new TextBlock();
			blockName.VerticalAlignment = VerticalAlignment.Center;
			blockName.Text = e.Pokeblock.Color.ToString() + " Pokéblock";
			blockName.TextTrimming = TextTrimming.CharacterEllipsis;
			blockName.Margin = new Thickness(4, 0, 0, 0);

			List<PokeblockFlavor> flavors = new List<PokeblockFlavor>();
			flavors.Add(new PokeblockFlavor(PokeblockColors.Red, e.Pokeblock.Spicyness));
			flavors.Add(new PokeblockFlavor(PokeblockColors.Blue, e.Pokeblock.Dryness));
			flavors.Add(new PokeblockFlavor(PokeblockColors.Pink, e.Pokeblock.Sweetness));
			flavors.Add(new PokeblockFlavor(PokeblockColors.Green, e.Pokeblock.Bitterness));
			flavors.Add(new PokeblockFlavor(PokeblockColors.Yellow, e.Pokeblock.Sourness));

			flavors.Sort((flavor1, flavor2) => (int)flavor2.Amount - (int)flavor1.Amount);

			List<Image> tasteImages = new List<Image>();
			foreach (PokeblockFlavor flavor in flavors) {
				if (flavor.Amount > 0) {
					Image tasteImage = new Image();
					tasteImage.Source = ItemDatabase.GetPokeblockImageFromColor(flavor.Flavor, false);
					tasteImage.Margin = new Thickness(4, 0, 0, 0);
					tasteImage.Stretch = Stretch.None;
					tasteImage.SnapsToDevicePixels = true;
					tasteImage.UseLayoutRounding = true;
					tasteImage.VerticalAlignment = VerticalAlignment.Center;
					tasteImages.Add(tasteImage);
				}
			}
			/*if (e.Pokeblock.Spicyness > 0) {
				Image tasteImage = new Image();
				tasteImage.Source = ItemDatabase.GetPokeblockImageFromColor(PokeblockColors.Red, false);
				tasteImages.Add(tasteImage);
			}
			if (e.Pokeblock.Dryness > 0) {
				Image tasteImage = new Image();
				tasteImage.Source = ItemDatabase.GetPokeblockImageFromColor(PokeblockColors.Blue, false);
				tasteImages.Add(tasteImage);
			}
			if (e.Pokeblock.Sweetness > 0) {
				Image tasteImage = new Image();
				tasteImage.Source = ItemDatabase.GetPokeblockImageFromColor(PokeblockColors.Pink, false);
				tasteImages.Add(tasteImage);
			}
			if (e.Pokeblock.Bitterness > 0) {
				Image tasteImage = new Image();
				tasteImage.Source = ItemDatabase.GetPokeblockImageFromColor(PokeblockColors.Green, false);
				tasteImages.Add(tasteImage);
			}
			if (e.Pokeblock.Sourness > 0) {
				Image tasteImage = new Image();
				tasteImage.Source = ItemDatabase.GetPokeblockImageFromColor(PokeblockColors.Yellow, false);
				tasteImages.Add(tasteImage);
			}
			for (int i = 0; i < tasteImages.Count; i++) {
				tasteImages[i].Margin = new Thickness(4, 0, 0, 0);
				tasteImages[i].Stretch = Stretch.None;
				tasteImages[i].SnapsToDevicePixels = true;
				tasteImages[i].UseLayoutRounding = true;
				tasteImages[i].VerticalAlignment = VerticalAlignment.Center;
			}*/

			TextBlock blockLv = new TextBlock();
			blockLv.VerticalAlignment	= VerticalAlignment.Center;
			blockLv.HorizontalAlignment = HorizontalAlignment.Right;
			blockLv.TextAlignment = TextAlignment.Right;
			blockLv.Text = "Lv";
			blockLv.Width = Double.NaN;
			blockLv.MinWidth = 10;

			TextBlock blockLevel = new TextBlock();
			blockLevel.VerticalAlignment	= VerticalAlignment.Center;
			blockLevel.HorizontalAlignment = HorizontalAlignment.Right;
			blockLevel.TextAlignment = TextAlignment.Right;
			blockLevel.Width = 30;
			blockLevel.Text = e.Pokeblock.Level.ToString();

			listViewItem.Content = dockPanel;
			blockCase.ListViewItems.Insert(e.Index, listViewItem);
			dockPanel.Children.Add(image);
			dockPanel.Children.Add(blockName);
			for (int i = 0; i < tasteImages.Count; i++) {
				dockPanel.Children.Add(tasteImages[i]);
			}
			dockPanel.Children.Add(blockLevel);
			dockPanel.Children.Add(blockLv);

			listViewItem.ContextMenu = contextMenu;
			listViewItem.ContextMenuOpening += OnContextMenuOpening;


			DockPanel.SetDock(image, Dock.Left);
			DockPanel.SetDock(blockLevel, Dock.Right);

			listViewItem.Tag = e.Pokeblock;
			UpdateDetails();
		}
		public void OnRemoveListViewItem(object sender, PokeblockCaseEventArgs e) {
			if (e.Index == selectedIndex) {
				selectedBlock = null;
				selectedIndex = -1;
			}
			blockCase.ListViewItems.RemoveAt(e.Index);

			UpdateDetails();
		}
		public void OnMoveListViewItem(object sender, PokeblockCaseEventArgs e) {
			blockCase.ListViewItems.Move(e.OldIndex, e.NewIndex);
		}
		public void OnProcessDrop(object sender, ProcessDropEventArgs<ListViewItem> e) {
			if (e.OldIndex > -1) {
				blockCase.MovePokeblock(e.OldIndex, e.NewIndex);
			}

			e.Effects = DragDropEffects.Move;

		}

		private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
			if (selectedBlock != null) {
				((MenuItem)contextMenu.Items[0]).IsEnabled = true;
				((MenuItem)contextMenu.Items[2]).IsEnabled = true;
				if (HasSelection) {

				}
				else {

				}
			}
			else {
				((MenuItem)contextMenu.Items[0]).IsEnabled = false;
				((MenuItem)contextMenu.Items[2]).IsEnabled = false;
			}
		}

		private void CreateContextMenu() {
			contextMenu = new ContextMenu();

			MenuItem menuItem = new MenuItem();
			menuItem.Header = "Send To";
			menuItem.Click += OnPokeblockSendTo;
			contextMenu.Items.Add(menuItem);

			contextMenu.Items.Add(new Separator());

			menuItem = new MenuItem();
			menuItem.Header = "Toss";
			menuItem.Click += OnPokeblockToss;
			contextMenu.Items.Add(menuItem);
		}
	}
}
