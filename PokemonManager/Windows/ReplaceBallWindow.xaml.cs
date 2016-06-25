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
	public partial class ReplaceBallWindow : Window {

		private IPokemon pokemon;
		private byte ballID;
		private int gameIndex;
		private bool loaded;

		private int selectedIndex;
		private Item selectedItem;
		private Item ballItem;

		public ReplaceBallWindow(IPokemon pokemon) {
			InitializeComponent();
			ballID = 0;
			selectedIndex = -1;
			selectedItem = null;
			this.pokemon = pokemon;


			for (int i = 0; i < PokeManager.NumGameSaves; i++) {
				IGameSave game = (IGameSave)PokeManager.GetGameSaveAt(i);
				if (game.GameType == GameTypes.PokemonBox) {
					comboBoxGame.SetGameSaveVisible(i, false);
				}
			}

			this.gameIndex = PokeManager.LastGameInDialogIndex;
			if (this.gameIndex == -2 || !comboBoxGame.IsGameSaveVisible(this.gameIndex)) {
				comboBoxGame.SelectedIndex = 0;
				this.gameIndex = comboBoxGame.SelectedGameIndex;
			}
			else {
				comboBoxGame.SelectedGameIndex = this.gameIndex;
			}

			loaded = true;
			OnGameSelectionChanged(null, null);
		}

		public static Item ShowDialog(Window owner, IPokemon pokemon) {
			ReplaceBallWindow window = new ReplaceBallWindow(pokemon);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result != null && result.Value) {
				if (window.ballID == byte.MaxValue)
					return null;
				return window.ballItem;
			}
			return null;
		}

		private void OnGameSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			gameIndex = comboBoxGame.SelectedGameIndex;

			ItemPocket pocket = PokeManager.GetGameSaveAt(gameIndex).Inventory.Items[ItemTypes.PokeBalls];

			listViewBalls.Items.Clear();
			selectedIndex = -1;
			selectedItem = null;

			for (int i = 0; i < pocket.SlotsUsed; i++) {
				Item item = pocket[i];
				ListViewItem listViewItem = new ListViewItem();
				listViewItem.SnapsToDevicePixels = true;
				listViewItem.UseLayoutRounding = true;
				DockPanel dockPanel = new DockPanel();
				dockPanel.Width = 170;

				Image image = new Image();
				image.Source = PokemonDatabase.GetBallCaughtImageFromID((byte)item.ID);
				image.Stretch = Stretch.None;
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

				listViewItem.Content = dockPanel;
				listViewBalls.Items.Add(listViewItem);
				dockPanel.Children.Add(image);
				dockPanel.Children.Add(itemName);
				dockPanel.Children.Add(itemCount);
				dockPanel.Children.Add(itemX);

				DockPanel.SetDock(image, Dock.Left);
				DockPanel.SetDock(itemCount, Dock.Right);
			}
		}

		private void OnBallSelectionChanged(object sender, SelectionChangedEventArgs e) {
			selectedIndex = listViewBalls.SelectedIndex;
			if (selectedIndex != -1) {
				ItemPocket pocket = PokeManager.GetGameSaveAt(gameIndex).Inventory.Items[ItemTypes.PokeBalls];
				selectedItem = pocket[selectedIndex];
			}
			else {
				selectedItem = null;
			}
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			ballID = (selectedItem != null ? (byte)selectedItem.ID : byte.MaxValue);
			ballItem = selectedItem;
			PokeManager.LastGameInDialogIndex = gameIndex;
			DialogResult = true;
		}
	}
}
