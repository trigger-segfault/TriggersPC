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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {
	public partial class DecorationInventoryViewer : UserControl {

		private Inventory inventory;
		private DecorationTypes currentPocket;
		private int previousPocketIndex;
		private DecorationTypes previousPocket;
		private SecretBaseViewer secretBaseManager;

		private Dictionary<DecorationTypes, DecorationViewerTab> tabs;

		public DecorationInventoryViewer() {
			InitializeComponent();

			this.tabs = new Dictionary<DecorationTypes, DecorationViewerTab>();
		}

		public void LoadInventory(Inventory inventory) {
			foreach (KeyValuePair<DecorationTypes, DecorationViewerTab> pair in tabs) {
				pair.Value.UnloadPocket();
			}

			this.previousPocketIndex = tabControlPockets.SelectedIndex;
			this.tabs.Clear();
			this.tabControlPockets.Items.Clear();
			this.inventory	= inventory;
			this.previousPocket = currentPocket;
			this.currentPocket = DecorationTypes.Unknown;

			if (inventory == null)
				return;

			TryAddContainer(DecorationTypes.Desk);
			TryAddContainer(DecorationTypes.Chair);
			TryAddContainer(DecorationTypes.Plant);
			TryAddContainer(DecorationTypes.Ornament);
			TryAddContainer(DecorationTypes.Mat);
			TryAddContainer(DecorationTypes.Poster);
			TryAddContainer(DecorationTypes.Doll);
			TryAddContainer(DecorationTypes.Cushion);
			this.currentPocket = (DecorationTypes)(tabControlPockets.Items[0] as TabItem).Tag;
			if (previousPocketIndex != -1) {
				Dispatcher.BeginInvoke((Action)(() => tabControlPockets.SelectedIndex = previousPocketIndex));
				this.currentPocket = previousPocket;
			}
			/*secretBaseManager = new GameSecretBaseManager();
			TabItem secretBasesTab = new TabItem();
			secretBasesTab.Content = secretBaseManager;
			TabItem tabItem = new TabItem();
			Image headerImage = new Image();
			headerImage.SnapsToDevicePixels = true;
			headerImage.UseLayoutRounding = true;
			headerImage.Stretch = Stretch.None;
			headerImage.Source = ResourceDatabase.GetImageFromName("SecretBaseTab");

			tabItem.Header = headerImage;
			secretBaseManager = new GameSecretBaseManager();
			secretBaseManager.Width = Double.NaN;
			secretBaseManager.Height = Double.NaN;
			tabItem.Content = secretBaseManager;
			tabControlPockets.Items.Add(tabItem);
			secretBaseManager.LoadGame(inventory.GameSave);*/
		}

		public void TryAddContainer(DecorationTypes decorationType) {
			if (inventory.Decorations.ContainsPocket(decorationType)) {
				TabItem tabItem = new TabItem();
				tabItem.Tag = decorationType;
				if (previousPocket == decorationType)
					previousPocketIndex = tabControlPockets.Items.Count;
				Image headerImage = new Image();
				headerImage.SnapsToDevicePixels = true;
				headerImage.UseLayoutRounding = true;
				headerImage.Stretch = Stretch.None;
				headerImage.Source = ResourceDatabase.GetImageFromName(decorationType.ToString() + "Tab");

				tabItem.Header = headerImage;
				DecorationViewerTab inventoryTab = new DecorationViewerTab();
				inventoryTab.Width = Double.NaN;
				inventoryTab.Height = Double.NaN;
				tabItem.Content = inventoryTab;
				tabControlPockets.Items.Add(tabItem);
				inventoryTab.LoadPocket(inventory.Decorations[decorationType]);
				tabs.Add(decorationType, inventoryTab);
				if (currentPocket == DecorationTypes.Unknown)
					currentPocket = decorationType;
			}
		}


		#region events

		private void OnTabSelected(object sender, SelectionChangedEventArgs e) {
			if (tabControlPockets.SelectedIndex != -1) {
				if ((tabControlPockets.SelectedItem as TabItem).Tag is DecorationTypes)
					currentPocket = (DecorationTypes)(tabControlPockets.SelectedItem as TabItem).Tag;
				else
					currentPocket = DecorationTypes.Unknown;
			}
		}

		#endregion

	}
}
