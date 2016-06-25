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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for InventoryViewer.xaml
	/// </summary>
	public partial class InventoryViewer : UserControl {

		private Inventory inventory;
		private ItemTypes currentPocket;

		private Dictionary<ItemTypes, ItemViewerTab> tabs;
		private PokeblockViewerTab pokeblockTab;
		private int previousPocketIndex;
		private ItemTypes previousPocket;

		public InventoryViewer() {
			InitializeComponent();

			this.tabs = new Dictionary<ItemTypes, ItemViewerTab>();
			this.currentPocket = ItemTypes.PC;
		}

		public void RefreshUI() {
			foreach (KeyValuePair<ItemTypes, ItemViewerTab> pair in tabs) {
				pair.Value.RefreshUI();
			}
		}

		public void LoadGameSave(Inventory inventory) {
			this.inventory = inventory;
			LoadInventory(inventory);
		}

		public void LoadInventory(Inventory inventory) {
			int previousGameIndex = (this.inventory != null ? this.inventory.GameIndex : -1);
			foreach (KeyValuePair<ItemTypes, ItemViewerTab> pair in tabs) {
				pair.Value.UnloadPocket();
			}
			if (pokeblockTab != null) {
				pokeblockTab.UnloadPokeblockCase();
				pokeblockTab = null;
			}

			this.previousPocketIndex = -1;
			this.tabs.Clear();
			this.tabControlPockets.Items.Clear();
			this.inventory	= inventory;
			this.previousPocket = (currentPocket == ItemTypes.Items && previousGameIndex == -1 ? ItemTypes.PC : currentPocket);
			this.currentPocket = ItemTypes.PC;
			TryAddPocket(ItemTypes.PC);
			if (!inventory.Items.ContainsPocket(ItemTypes.InBattle))
				TryAddPocket(ItemTypes.Items);
			TryAddPocket(ItemTypes.InBattle);
			TryAddPocket(ItemTypes.Valuables);
			TryAddPocket(ItemTypes.Hold);
			TryAddPocket(ItemTypes.Misc);
			TryAddPocket(ItemTypes.PokeBalls);
			TryAddPocket(ItemTypes.Berries);
			TryAddPocket(ItemTypes.TMCase);
			TryAddPocket(ItemTypes.KeyItems);
			TryAddPocket(ItemTypes.CologneCase);
			TryAddPocket(ItemTypes.DiscCase);

			if (inventory.Pokeblocks != null) {
				TabItem tabItem = new TabItem();
				tabItem.Tag = ItemTypes.Unknown;
				if (previousPocket == ItemTypes.Unknown)
					previousPocketIndex = tabControlPockets.Items.Count;
				StackPanel stackPanel = new StackPanel();
				stackPanel.SnapsToDevicePixels = true;
				stackPanel.Orientation = Orientation.Horizontal;
				Image headerImage = new Image();
				headerImage.SnapsToDevicePixels = true;
				headerImage.UseLayoutRounding = true;
				headerImage.Stretch = Stretch.None;
				headerImage.Source = ResourceDatabase.GetImageFromName("PokeblockCaseTab");
				TextBlock headerName = new TextBlock();
				headerName.Text = "Pokéblock Case";
				headerName.VerticalAlignment = VerticalAlignment.Center;
				headerName.Margin = new Thickness(2, 0, 0, 0);
				//stackPanel.Children.Add(headerImage);
				//stackPanel.Children.Add(headerName);
				tabItem.Header = headerImage;// stackPanel;// "Pokéblock Case";
				pokeblockTab = new PokeblockViewerTab();
				pokeblockTab.Width = Double.NaN;
				pokeblockTab.Height = Double.NaN;
				tabItem.Content = pokeblockTab;
				tabControlPockets.Items.Add(tabItem);
				pokeblockTab.LoadPokeblockCase(inventory.Pokeblocks);
			}
			this.currentPocket = (ItemTypes)(tabControlPockets.Items[0] as TabItem).Tag;
			if (previousPocketIndex != -1) {
				Dispatcher.BeginInvoke((Action)(() => tabControlPockets.SelectedIndex = previousPocketIndex));
				this.currentPocket = previousPocket;
			}
		}

		public void TryAddPocket(ItemTypes pocketType) {
			if (inventory.Items.ContainsPocket(pocketType)) {
				TabItem tabItem = new TabItem();
				tabItem.Tag = pocketType;
				if (previousPocket == pocketType)
					previousPocketIndex = tabControlPockets.Items.Count;
				StackPanel stackPanel = new StackPanel();
				stackPanel.SnapsToDevicePixels = true;
				stackPanel.Orientation = Orientation.Horizontal;
				Image headerImage = new Image();
				headerImage.SnapsToDevicePixels = true;
				headerImage.UseLayoutRounding = true;
				headerImage.Stretch = Stretch.None;
				headerImage.Source = ResourceDatabase.GetImageFromName(pocketType.ToString() + "Tab");
				TextBlock headerName = new TextBlock();
				headerName.Text = ItemDatabase.GetPocketName(pocketType);
				headerName.VerticalAlignment = VerticalAlignment.Center;
				headerName.Margin = new Thickness(2, 0, 0, 0);
				//stackPanel.Children.Add(headerImage);
				//stackPanel.Children.Add(headerName);
				tabItem.Header = headerImage;// stackPanel;//ItemTable.GetPocketName(pocketType);
				ItemViewerTab inventoryTab = new ItemViewerTab();
				inventoryTab.Width = Double.NaN;
				inventoryTab.Height = Double.NaN;
				tabItem.Content = inventoryTab;
				tabControlPockets.Items.Add(tabItem);
				inventoryTab.LoadPocket(inventory.Items[pocketType]);
				tabs.Add(pocketType, inventoryTab);
				if (currentPocket == ItemTypes.Unknown)
					currentPocket = pocketType;
			}
		}


		#region events

		private void OnTabChanged(object sender, SelectionChangedEventArgs e) {
			if (tabControlPockets.SelectedIndex != -1)
				currentPocket = (ItemTypes)(tabControlPockets.SelectedItem as TabItem).Tag;
		}

		#endregion
	}
}
