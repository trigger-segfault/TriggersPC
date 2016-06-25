using Microsoft.Win32;
using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
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
	public partial class SaveManager : Window {

		private ObservableCollection<ListViewItem> gameSaves;
		private ListViewDragDropManager<ListViewItem> dropManager;
		private int selectedIndex;
		private GameSaveFileInfo selectedGameSave;
		private ContextMenu contextMenu;

		public SaveManager() {
			InitializeComponent();

			this.gameSaves = new ObservableCollection<ListViewItem>();
			this.selectedGameSave = null;
			this.selectedIndex = -1;

			CreateContextMenu();

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				this.listViewGameSaves.ItemsSource = gameSaves;
				this.dropManager = new ListViewDragDropManager<ListViewItem>(listViewGameSaves);

				for (int i = 0; i < PokeManager.NumGameSaves; i++) {
					GameSaveFileInfo gameSave = PokeManager.GetGameSaveFileInfoAt(i);
					ListViewItem listViewItem = new ListViewItem();
					FillListViewItem(gameSave, listViewItem);
					gameSaves.Add(listViewItem);
				}
			}

		}

		public static void ShowDialog(Window window) {
			SaveManager form = new SaveManager();
			form.Owner = window;
			form.ShowDialog();
		}

		private void FillListViewItem(GameSaveFileInfo gameSaveFile, ListViewItem listViewItem) {
			IGameSave gameSave = gameSaveFile.GameSave;

			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Horizontal;

			Image gameImage = new Image();
			BitmapImage bitmap = ResourceDatabase.GetImageFromName(gameSaveFile.GameType.ToString() + "Physical");
			gameImage.Source = bitmap;
			if (bitmap == null)
				gameImage.Width = 20;
			else
				gameImage.Width = bitmap.PixelWidth * (20.0 / bitmap.PixelHeight);
			gameImage.Height = 20;
			gameImage.VerticalAlignment = VerticalAlignment.Center;

			TextBlock gameName = new TextBlock();
			string gameTypeName = (gameSave.GameType == GameTypes.PokemonBox ? "Pokémon Box" : gameSave.GameType.ToString());
			if (gameSaveFile.Nickname != "")
				gameName.Text = gameSaveFile.Nickname + (gameSaveFile.GameType != GameTypes.PokemonBox ? " [" : "");
			else
				gameName.Text = gameTypeName + (gameSaveFile.GameType != GameTypes.PokemonBox ? " [" : "");
			gameName.VerticalAlignment = VerticalAlignment.Center;
			gameName.Margin = new Thickness(5, 0, 0, 0);

			TextBlock trainerName = new TextBlock();
			trainerName.Text = gameSave.TrainerName;
			trainerName.Foreground = new SolidColorBrush(gameSave.TrainerGender == Genders.Male ? Color.FromRgb(32, 128, 248) : (gameSave.TrainerGender == Genders.Female ? Color.FromRgb(248, 24, 168) : Color.FromRgb(0, 0, 0)));
			trainerName.VerticalAlignment = VerticalAlignment.Center;

			TextBlock ending = new TextBlock();
			ending.Text = "]";
			ending.VerticalAlignment = VerticalAlignment.Center;

			stackPanel.Children.Add(gameImage);
			stackPanel.Children.Add(gameName);
			if (gameSaveFile.GameType != GameTypes.PokemonBox) {
				stackPanel.Children.Add(trainerName);
				stackPanel.Children.Add(ending);
			}

			listViewItem.ContextMenu = contextMenu;
			listViewItem.ContextMenuOpening += OnContextMenuOpening;
			listViewItem.Content = stackPanel;
			listViewItem.Tag = gameSaveFile;
		}

		private void OnAddSaveClicked(object sender, RoutedEventArgs e) {

			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Title = "Add Generation III Save";
			fileDialog.Filter = "GBA and GameCube Saves (*.sav, *.gci)|*.sav;*.gci|GBA Saves (*.sav)|*.sav|GameCube Saves (*.gci)|*.gci|All Files (*.*)|*.*";
			var result = fileDialog.ShowDialog(this);
			if (result.HasValue && result.Value) {
				string filePath = fileDialog.FileName;

				filePath = System.IO.Path.GetFullPath(filePath).ToLower();
				foreach (ListViewItem item in gameSaves) {
					GameSaveFileInfo save = item.Tag as GameSaveFileInfo;
					if (filePath == System.IO.Path.GetFullPath(save.FilePath).ToLower()) {
						TriggerMessageBox.Show(this, "This game save already exists", "Already Exists");
						return;
					}
				}

				try {
					FileInfo fileInfo = new FileInfo(filePath);
					GameSaveFileInfo gameSaveFile;
					GameTypes? gameType = GameTypes.Any;
					if (fileInfo.Length == 131072 || fileInfo.Length == 65536 || fileInfo.Length == 139264) {
						gameType = SelectGameTypeFullWindow.ShowDialog(this);
					}
					gameSaveFile = PokeManager.MakeNewGameSaveFileInfo(filePath, gameType.HasValue ? gameType.Value : GameTypes.Any);
					
					ListViewItem listViewItem = new ListViewItem();
					FillListViewItem(gameSaveFile, listViewItem);
					gameSaves.Add(listViewItem);

					listViewGameSaves.SelectedIndex = listViewGameSaves.Items.Count - 1;
					// Hackish thing to make sure the list view is always scrolled at the bottom when adding a new box
					//http://stackoverflow.com/questions/211971/scroll-wpf-listview-to-specific-line
					VirtualizingStackPanel vsp =  
					(VirtualizingStackPanel)typeof(ItemsControl).InvokeMember("_itemsHost",
						BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic, null,
						listViewGameSaves, null);
					double scrollHeight = vsp.ScrollOwner.ScrollableHeight;
					vsp.SetVerticalOffset(vsp.ScrollOwner.ScrollableHeight * 2);
				}
				catch (Exception ex) {
					TriggerMessageBox.Show(this, "Error loading game save file\n\nException:\n" + ex.Message, "Read Error");
				}
			}

		}
		private void OnRemoveSave(object sender, RoutedEventArgs e) {
			MessageBoxResult result = TriggerMessageBox.Show(this, "Are you sure you want to remove this game save?", "Remove Save", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes && selectedGameSave.GameSave.IsChanged)
				result = TriggerMessageBox.Show(this, "Would you like to save changes made to this game before removing it?", "Save Changes", MessageBoxButton.YesNoCancel);

			if (result == MessageBoxResult.Yes && selectedGameSave.GameSave.IsChanged)
				selectedGameSave.GameSave.Save(selectedGameSave.FilePath);

			if (result != MessageBoxResult.Cancel) {
				gameSaves.RemoveAt(selectedIndex);
				if (selectedIndex == listViewGameSaves.Items.Count)
					selectedIndex--;
				if (selectedIndex != -1)
					selectedGameSave = gameSaves[selectedIndex].Tag as GameSaveFileInfo;
				else
					selectedGameSave = null;
				listViewGameSaves.SelectedIndex = selectedIndex;
			}
		}
		private void OnEditNickname(object sender, RoutedEventArgs e) {

			string newNickname = AddSaveNicknameWindow.ShowDialog(this, selectedGameSave.Nickname);
			if (newNickname != null) {
				selectedGameSave.Nickname = newNickname;
				FillListViewItem(selectedGameSave, gameSaves[selectedIndex]);
			}
		}
		private void OnOpenExplorer(object sender, RoutedEventArgs e) {
			Process.Start("explorer.exe", "/select, \"" + selectedGameSave.FilePath + "\"");
		}
		private void OnJapaneseChecked(object sender, RoutedEventArgs e) {
			selectedGameSave.IsJapanese = ((MenuItem)sender).IsChecked;
			selectedGameSave.GameSave.IsJapanese = ((MenuItem)sender).IsChecked;
			selectedGameSave.GameSave.IsChanged = true;
		}
		private void OnLivingDexChecked(object sender, RoutedEventArgs e) {
			selectedGameSave.IsLivingDex = ((MenuItem)sender).IsChecked;
			selectedGameSave.GameSave.IsChanged = true;
		}
		private void OnChangeGameType(object sender, RoutedEventArgs e) {
			if ((selectedGameSave.GameSave.GameType == GameTypes.Ruby || selectedGameSave.GameSave.GameType == GameTypes.Sapphire ||
				selectedGameSave.GameSave.GameType == GameTypes.FireRed || selectedGameSave.GameType == GameTypes.LeafGreen ||
				selectedGameSave.GameSave.GameType == GameTypes.Emerald) && selectedGameSave.GameSave is GBAGameSave) {
				var gameType = SelectGameTypeFullWindow.ShowDialog(this);
				if (gameType.HasValue && gameType.Value != GameTypes.Any) {
					MessageBoxResult result = MessageBoxResult.Yes;
					bool reloadNeeded = false;
					if (((selectedGameSave.GameSave.GameType == GameTypes.Ruby || selectedGameSave.GameSave.GameType == GameTypes.Sapphire) && (gameType != GameTypes.Ruby && gameType != GameTypes.Sapphire)) ||
						((selectedGameSave.GameSave.GameType == GameTypes.FireRed || selectedGameSave.GameSave.GameType == GameTypes.LeafGreen) && (gameType != GameTypes.FireRed && gameType != GameTypes.LeafGreen)) ||
						(selectedGameSave.GameSave.GameType == GameTypes.Emerald && gameType != GameTypes.Emerald)) {
						result = TriggerMessageBox.Show(this, "In order to change the game type to this the save must be reloaded. Are you sure you want to reload this save? Any unsaved changes will be lost.", "Reload Needed", MessageBoxButton.YesNo);
						if (result == MessageBoxResult.Yes)
							reloadNeeded = true;
					}
					if (result == MessageBoxResult.Yes) {
						selectedGameSave.GameType = gameType.Value;
						((GBAGameSave)selectedGameSave.GameSave).GameType = gameType.Value;
						FillListViewItem(selectedGameSave, gameSaves[selectedIndex]);
						if (reloadNeeded) {
							try {
								PokeManager.ReloadGameSave(selectedGameSave);
							}
							catch (Exception ex) {
								TriggerMessageBox.Show(this, "Error loading save after changing game type, this may not be the correct game type for it.", "Load Error");
							}

						}
					}
				}
			}
			else {
				// No need
				TriggerMessageBox.Show(this, "Only one game type applies to this save");
			}
		}

		private void OnWindowClosing(object sender, CancelEventArgs e) {
			List<GameSaveFileInfo> gameSaveFiles = new List<GameSaveFileInfo>();
			foreach (object listViewItem in gameSaves) {
				gameSaveFiles.Add(((ListViewItem)listViewItem).Tag as GameSaveFileInfo);
			}
			PokeManager.SetGameSaveFileInfoList(gameSaveFiles.ToArray());
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int newIndex = listViewGameSaves.SelectedIndex;
			if (newIndex != -1) {
				selectedIndex = newIndex;
				selectedGameSave = gameSaves[selectedIndex].Tag as GameSaveFileInfo;
			}
		}

		private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
			((MenuItem)contextMenu.Items[1]).IsEnabled = (selectedGameSave.GameType == GameTypes.Ruby    || selectedGameSave.GameType == GameTypes.Sapphire ||
														  selectedGameSave.GameType == GameTypes.FireRed || selectedGameSave.GameType == GameTypes.LeafGreen ||
														  selectedGameSave.GameType == GameTypes.Emerald);
			((MenuItem)contextMenu.Items[2]).IsChecked = selectedGameSave.IsJapanese;
			((MenuItem)contextMenu.Items[2]).IsEnabled = selectedGameSave.GameType != GameTypes.Colosseum && selectedGameSave.GameType != GameTypes.XD;
			((MenuItem)contextMenu.Items[3]).IsChecked = selectedGameSave.IsLivingDex;
			((MenuItem)contextMenu.Items[3]).IsEnabled = selectedGameSave.GameType != GameTypes.Colosseum && selectedGameSave.GameType != GameTypes.XD;
		}

		private void CreateContextMenu() {
			contextMenu = new ContextMenu();

			MenuItem editNickname = new MenuItem();
			editNickname.Header = "Edit Nickname";
			editNickname.Click += OnEditNickname;
			contextMenu.Items.Add(editNickname);

			MenuItem changeGameType = new MenuItem();
			changeGameType.Header = "Change Game Type";
			changeGameType.Click += OnChangeGameType;
			contextMenu.Items.Add(changeGameType);

			MenuItem japanese = new MenuItem();
			japanese.Header = "Japanese Game";
			japanese.Click += OnJapaneseChecked;
			japanese.IsCheckable = true;
			contextMenu.Items.Add(japanese);

			MenuItem livingDex = new MenuItem();
			livingDex.Header = "Living Dex";
			livingDex.Click += OnLivingDexChecked;
			livingDex.IsCheckable = true;
			contextMenu.Items.Add(livingDex);

			MenuItem openExplorer = new MenuItem();
			openExplorer.Header = "Open in Explorer";
			openExplorer.Click += OnOpenExplorer;
			contextMenu.Items.Add(openExplorer);

			contextMenu.Items.Add(new Separator());

			MenuItem removeSave = new MenuItem();
			removeSave.Header = "Remove Save";
			removeSave.Click += OnRemoveSave;
			contextMenu.Items.Add(removeSave);
		}
	}
}
