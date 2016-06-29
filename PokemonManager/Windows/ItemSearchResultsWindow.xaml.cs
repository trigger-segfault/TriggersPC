using Microsoft.Win32;
using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SaveManager.xaml
	/// </summary>

	class ItemPocketGameTag {
		public int GameIndex { get; set; }
		public ItemTypes ItemType { get; set; }
		public DecorationTypes DecorationType { get; set; }

		public ItemPocketGameTag(int gameIndex) {
			this.GameIndex = gameIndex;
			this.ItemType = ItemTypes.Unknown;
			this.DecorationType = DecorationTypes.Unknown;
		}
		public ItemPocketGameTag(int gameIndex, ItemTypes itemType) {
			this.GameIndex = gameIndex;
			this.ItemType = itemType;
			this.DecorationType = DecorationTypes.Unknown;
		}
		public ItemPocketGameTag(int gameIndex, DecorationTypes decorationType) {
			this.GameIndex = gameIndex;
			this.DecorationType = decorationType;
			this.ItemType = ItemTypes.Unknown;
		}
	}


	public partial class ItemSearchResultsWindow : Window {

		private ItemPocketGameTag selectedTag;

		private PokemonSearchResults resultsWindow;

		public bool IsClosed { get; set; }

		private bool decorationMode;

		private ushort selectedItemID;

		private List<IPokemon> pokemonHoldingItem;

		public ItemSearchResultsWindow(bool decorationMode) {
			InitializeComponent();

			this.selectedTag = null;
			this.decorationMode = decorationMode;
			if (decorationMode)
				Title = "Search Decorations";


			this.listViewResults.Items.Clear();
			this.pokemonHoldingItem = new List<IPokemon>();
			if (!DesignerProperties.GetIsInDesignMode(this)) {
				UpdateDecorationMode();
			}
		}

		public void RefreshUI() {
			resultsWindow.RefreshUI();
		}

		private ItemData SelectedItemData {
			get { return ItemDatabase.GetItemFromID(selectedItemID); }
		}
		private DecorationData SelectedDecorationData {
			get { return ItemDatabase.GetDecorationFromID((byte)selectedItemID); }
		}

		public static ItemSearchResultsWindow Show(Window owner, bool decorationMode) {
			ItemSearchResultsWindow window = new ItemSearchResultsWindow(decorationMode);
			window.Owner = owner;
			window.Show();
			return window;
		}

		private void AddListViewItem(int gameIndex, object pocketType, string pocket, int count) {

			ListViewItem listViewItem = new ListViewItem();
			if (decorationMode)
				listViewItem.Tag = new ItemPocketGameTag(gameIndex, (DecorationTypes)pocketType);
			else
				listViewItem.Tag = new ItemPocketGameTag(gameIndex, (ItemTypes)pocketType);
			listViewItem.SnapsToDevicePixels = true;
			listViewItem.UseLayoutRounding = true;
			DockPanel dockPanel = new DockPanel();
			dockPanel.Width = 250;

			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Horizontal;

			if (gameIndex == -1) {
				TextBlock gameName = new TextBlock();
				gameName.Text = PokeManager.Settings.ManagerNickname;
				gameName.VerticalAlignment = VerticalAlignment.Center;
				stackPanel.Children.Add(gameName);
			}
			else {
				IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);
				GameSaveFileInfo gameSaveFile = PokeManager.GetGameSaveFileInfoAt(gameIndex);

				TextBlock gameName = new TextBlock();
				string gameTypeName = (gameSave.GameType == GameTypes.PokemonBox ? "Pokémon Box" : gameSave.GameType.ToString());
				if (gameSaveFile.Nickname != "")
					gameName.Text = gameSaveFile.Nickname + (gameSaveFile.GameType != GameTypes.PokemonBox ? " [" : "");
				else
					gameName.Text = gameTypeName + (gameSaveFile.GameType != GameTypes.PokemonBox ? " [" : "");
				gameName.VerticalAlignment = VerticalAlignment.Center;
				//gameName.Margin = new Thickness(5, 0, 0, 0);

				TextBlock trainerName = new TextBlock();
				trainerName.Text = gameSave.TrainerName;
				trainerName.Foreground = new SolidColorBrush(gameSave.TrainerGender == Genders.Male ? Color.FromRgb(32, 128, 248) : (gameSave.TrainerGender == Genders.Female ? Color.FromRgb(248, 24, 168) : Color.FromRgb(0, 0, 0)));
				trainerName.VerticalAlignment = VerticalAlignment.Center;

				TextBlock ending = new TextBlock();
				ending.Text = "]";//  " + pocket;
				ending.VerticalAlignment = VerticalAlignment.Center;

				//stackPanel.Children.Add(gameImage);
				stackPanel.Children.Add(gameName);
				if (gameSaveFile.GameType != GameTypes.PokemonBox) {
					stackPanel.Children.Add(trainerName);
					stackPanel.Children.Add(ending);
				}
			}

			TextBlock itemPocket = new TextBlock();
			itemPocket.VerticalAlignment	= VerticalAlignment.Center;
			itemPocket.HorizontalAlignment = HorizontalAlignment.Right;
			itemPocket.TextAlignment = TextAlignment.Right;
			itemPocket.Text = pocket + " ";
			itemPocket.Width = Double.NaN;
			itemPocket.MinWidth = 10;

			TextBlock itemX = new TextBlock();
			itemX.VerticalAlignment	= VerticalAlignment.Center;
			itemX.HorizontalAlignment = HorizontalAlignment.Right;
			itemX.TextAlignment = TextAlignment.Right;
			itemX.Text = "x";
			itemX.MinWidth = 10;

			TextBlock itemCount = new TextBlock();
			itemCount.VerticalAlignment	= VerticalAlignment.Center;
			itemCount.HorizontalAlignment = HorizontalAlignment.Right;
			itemCount.TextAlignment = TextAlignment.Right;
			itemCount.Width = 30;
			itemCount.Text = count.ToString();

			listViewItem.Content = dockPanel;
			listViewResults.Items.Add(listViewItem);
			dockPanel.Children.Add(stackPanel);
			dockPanel.Children.Add(itemCount);
			dockPanel.Children.Add(itemX);
			dockPanel.Children.Add(itemPocket);

			DockPanel.SetDock(itemX, Dock.Right);
			DockPanel.SetDock(itemCount, Dock.Right);
			DockPanel.SetDock(stackPanel, Dock.Left);
		}

		private void OnSeeResultsClicked(object sender, RoutedEventArgs e) {
			if (selectedTag != null) {
				if (selectedTag.GameIndex == -2) {
					if (resultsWindow != null && !resultsWindow.IsClosed)
						resultsWindow.ShowResults(pokemonHoldingItem);
					else
						resultsWindow = PokemonSearchResults.Show(PokeManager.ManagerWindow, pokemonHoldingItem);
				}
				else if (!decorationMode) {
					PokeManager.ManagerWindow.GotoItem(selectedTag.GameIndex, selectedTag.ItemType, selectedItemID);
				}
				else {
					PokeManager.ManagerWindow.GotoDecoration(selectedTag.GameIndex, selectedTag.DecorationType, (byte)selectedItemID);
				}
			}
		}

		public void SetDecorationMode(bool decorationMode) {
			if (decorationMode != this.decorationMode) {
				this.decorationMode = decorationMode;
				UpdateDecorationMode();
			}
		}

		private void UpdateDecorationMode() {
			comboBoxItems.Items.Clear();

			ComboBoxItem comboBoxItem = new ComboBoxItem();
			comboBoxItem.Content = "None";
			comboBoxItem.Tag = (ushort)0;
			comboBoxItems.Items.Add(comboBoxItem);

			if (decorationMode) {
				for (int i = 1; ItemDatabase.GetDecorationAt(i) != null; i++) {
					DecorationData decorationData = ItemDatabase.GetDecorationAt(i);
					comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = decorationData.Name;
					comboBoxItem.Tag = (ushort)decorationData.ID;
					comboBoxItems.Items.Add(comboBoxItem);
				}
			}
			else {
				for (int i = 1; ItemDatabase.GetItemAt(i) != null; i++) {
					ItemData itemData = ItemDatabase.GetItemAt(i);
					comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = itemData.Name;
					comboBoxItem.Tag = itemData.ID;
					comboBoxItems.Items.Add(comboBoxItem);
				}
			}
			comboBoxItems.SelectedIndex = 0;
		}

		private void OnWindowClosing(object sender, CancelEventArgs e) {
			if (resultsWindow != null && !resultsWindow.IsClosed)
				resultsWindow.Close();
			IsClosed = true;
			PokeManager.ManagerWindow.PokemonSearchWindow = null;
			PokeManager.ManagerWindow.Focus();
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int newIndex = listViewResults.SelectedIndex;
			if (newIndex != -1) {
				selectedTag = (ItemPocketGameTag)((Control)listViewResults.SelectedItem).Tag;
				buttonSeeResults.IsEnabled = true;
				if (selectedTag.GameIndex == -2)
					buttonSeeResults.Content = "See Results";
				else
					buttonSeeResults.Content = "Goto";
			}
			else {
				selectedTag = null;
				buttonSeeResults.IsEnabled = false;
			}
		}

		private void OnCloseClicked(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (comboBoxItems.SelectedIndex != -1 && comboBoxItems.SelectedIndex != 0) {
				selectedItemID = (ushort)((ComboBoxItem)comboBoxItems.SelectedItem).Tag;
				UpdateResults();
			}
			else {
				listViewResults.Items.Clear();
				selectedTag = null;
			}
			buttonSeeResults.IsEnabled = false;
		}

		private void OnRefreshClicked(object sender, RoutedEventArgs e) {
			UpdateResults();
		}

		private void UpdateResults() {
			listViewResults.Items.Clear();
			if (selectedItemID == 0)
				return;
			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				IGameSave gameSave = PokeManager.GetGameSaveAt(i);
				if (gameSave.Inventory == null)
					continue;
				if (decorationMode && gameSave.Inventory.Decorations != null) {
					if (gameSave.Inventory.Decorations.ContainsPocket(SelectedDecorationData.DecorationType)) {
						DecorationPocket pocket = gameSave.Inventory.Decorations[SelectedDecorationData.DecorationType];
						int count = 0;
						for (int j = 0; j < pocket.SlotsUsed; j++) {
							if (pocket[j].ID == (byte)selectedItemID)
								count += (int)pocket[j].Count;
						}
						if (count > 0)
							AddListViewItem(i, pocket.PocketType, ItemDatabase.GetDecorationContainerName(pocket.PocketType), count);
					}
				}
				else if (!decorationMode)  {
					ItemPocket pocket;
					int count;
					if (gameSave.Inventory.Items.ContainsPocket(ItemTypes.PC)) {
						pocket = gameSave.Inventory.Items[ItemTypes.PC];
						count = 0;
						for (int j = 0; j < pocket.SlotsUsed; j++) {
							if (pocket[j].ID == (ushort)selectedItemID)
								count += (int)pocket[j].Count;
						}
						if (count > 0)
							AddListViewItem(i, pocket.PocketType, ItemDatabase.GetPocketName(pocket.PocketType), count);
					}
					if (gameSave.Inventory.Items.ContainsPocket(SelectedItemData.PocketType)) {
						pocket = gameSave.Inventory.Items[SelectedItemData.PocketType];
						count = 0;
						for (int j = 0; j < pocket.SlotsUsed; j++) {
							if (pocket[j].ID == (ushort)selectedItemID)
								count += (int)pocket[j].Count;
						}
						if (count > 0)
							AddListViewItem(i, pocket.PocketType, ItemDatabase.GetPocketName(pocket.PocketType), count);
					}
					if (SelectedItemData.HasSubPocket && gameSave.Inventory.Items.ContainsPocket(SelectedItemData.SubPocketType)) {
						pocket = gameSave.Inventory.Items[SelectedItemData.SubPocketType];
						count = 0;
						for (int j = 0; j < pocket.SlotsUsed; j++) {
							if (pocket[j].ID == (ushort)selectedItemID)
								count += (int)pocket[j].Count;
						}
						if (count > 0)
							AddListViewItem(i, pocket.PocketType, ItemDatabase.GetPocketName(pocket.PocketType), count);
					}
				}
			}
			if (!decorationMode) {
				pokemonHoldingItem.Clear();
				for (int i = -1; i < PokeManager.NumGameSaves; i++) {
					IGameSave gameSave = PokeManager.GetGameSaveAt(i);

					if (gameSave is ManagerGameSave) {
						for (int j = 0; j < (gameSave as ManagerGameSave).NumPokePCRows; j++) {
							foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(j)) {
								if (pokemon.HeldItemID == selectedItemID)
									pokemonHoldingItem.Add(pokemon);
							}
						}
					}
					else {
						foreach (IPokemon pokemon in gameSave.PokePC) {
							if (pokemon.HeldItemID == selectedItemID)
								pokemonHoldingItem.Add(pokemon);
						}
					}
				}
				if (pokemonHoldingItem.Count > 0) {

					ListViewItem listViewItem = new ListViewItem();
					listViewItem.Tag = new ItemPocketGameTag(-2);
					listViewItem.SnapsToDevicePixels = true;
					listViewItem.UseLayoutRounding = true;
					DockPanel dockPanel = new DockPanel();
					dockPanel.Width = 250;


					TextBlock itemPocket = new TextBlock();
					itemPocket.VerticalAlignment	= VerticalAlignment.Center;
					itemPocket.Text = "Holding Pokémon";
					itemPocket.MinWidth = 10;

					TextBlock itemX = new TextBlock();
					itemX.VerticalAlignment	= VerticalAlignment.Center;
					itemX.HorizontalAlignment = HorizontalAlignment.Right;
					itemX.TextAlignment = TextAlignment.Right;
					itemX.Text = "x";
					itemX.MinWidth = 10;

					TextBlock itemCount = new TextBlock();
					itemCount.VerticalAlignment	= VerticalAlignment.Center;
					itemCount.HorizontalAlignment = HorizontalAlignment.Right;
					itemCount.TextAlignment = TextAlignment.Right;
					itemCount.Width = 30;
					itemCount.Text = pokemonHoldingItem.Count.ToString();

					listViewItem.Content = dockPanel;
					dockPanel.Children.Add(itemCount);
					dockPanel.Children.Add(itemX);
					dockPanel.Children.Add(itemPocket);

					DockPanel.SetDock(itemX, Dock.Right);
					DockPanel.SetDock(itemCount, Dock.Right);
					DockPanel.SetDock(itemPocket, Dock.Left);

					Separator separator = new Separator();
					separator.Height = 7;
					separator.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
					listViewResults.Items.Add(separator);

					listViewResults.Items.Add(listViewItem);
				}
			}
			buttonSeeResults.IsEnabled = false;
		}
	}
}
