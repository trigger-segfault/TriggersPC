using Microsoft.Win32;
using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
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

	public class GamePokemonSearchResults {
		public IGameSave GameSave;
		public List<IPokemon> ValidPokemon;

		public GamePokemonSearchResults(IGameSave gameSave) {
			GameSave = gameSave;
			ValidPokemon = new List<IPokemon>();
		}
	}

	public partial class GamePokemonSearchResultsWindow : Window {

		private ObservableCollection<ListViewItem> gameSaves;
		private int selectedIndex;
		private GameSaveFileInfo selectedGameSave;
		private List<GamePokemonSearchResults> mirageIslandResults;

		private PokemonSearchResults resultsWindow;

		public bool IsClosed { get; set; }

		public GamePokemonSearchResultsWindow(List<GamePokemonSearchResults> mirageIslandResults) {
			InitializeComponent();

			this.gameSaves = new ObservableCollection<ListViewItem>();
			this.selectedGameSave = null;
			this.selectedIndex = -1;
			this.mirageIslandResults = mirageIslandResults;


			if (!DesignerProperties.GetIsInDesignMode(this)) {
				this.listViewGameSaves.ItemsSource = gameSaves;

				for (int i = 0; i < PokeManager.NumGameSaves; i++) {
					GameSaveFileInfo gameSave = PokeManager.GetGameSaveFileInfoAt(i);
					if (GetMirageResults(gameSave.GameSave) == null)
						continue;
					ListViewItem listViewItem = new ListViewItem();
					FillListViewItem(gameSave, listViewItem);
					gameSaves.Add(listViewItem);
				}
			}

		}

		public void RefreshUI() {
			resultsWindow.RefreshUI();
		}

		public GamePokemonSearchResults GetMirageResults(IGameSave gameSave) {
			foreach (GamePokemonSearchResults results in mirageIslandResults) {
				if (results.GameSave == gameSave)
					return results;
			}
			return null;
		}

		public static GamePokemonSearchResultsWindow Show(Window window, List<GamePokemonSearchResults> mirageIslandResults, string title = null) {
			GamePokemonSearchResultsWindow form = new GamePokemonSearchResultsWindow(mirageIslandResults);
			if (title != null)
				form.Title = title;
			form.Owner = window;
			form.Show();
			return form;
		}

		private void FillListViewItem(GameSaveFileInfo gameSaveFile, ListViewItem listViewItem) {
			IGameSave gameSave = gameSaveFile.GameSave;

			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Horizontal;

			/*Image gameImage = new Image();
			BitmapImage bitmap = ResourceDatabase.GetImageFromName(gameSaveFile.GameType.ToString() + "Physical");
			gameImage.Source = bitmap;
			if (bitmap == null)
				gameImage.Width = 20;
			else
				gameImage.Width = bitmap.PixelWidth * (20.0 / bitmap.PixelHeight);
			gameImage.Height = 20;
			gameImage.VerticalAlignment = VerticalAlignment.Center;*/

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
			ending.Text = "]";
			ending.VerticalAlignment = VerticalAlignment.Center;

			//stackPanel.Children.Add(gameImage);
			stackPanel.Children.Add(gameName);
			if (gameSaveFile.GameType != GameTypes.PokemonBox) {
				stackPanel.Children.Add(trainerName);
				stackPanel.Children.Add(ending);
			}

			listViewItem.Content = stackPanel;
			listViewItem.Tag = gameSaveFile;
		}

		private void OnSeeResultsClicked(object sender, RoutedEventArgs e) {
			if (selectedGameSave != null) {
				GamePokemonSearchResults results = GetMirageResults(selectedGameSave.GameSave);
				if (results != null) {
					if (resultsWindow != null && !resultsWindow.IsClosed)
						resultsWindow.ShowResults(results.ValidPokemon);
					else
						resultsWindow = PokemonSearchResults.Show(Owner, results.ValidPokemon);
				}
			}
		}

		private void OnWindowClosing(object sender, CancelEventArgs e) {
			if (resultsWindow != null && !resultsWindow.IsClosed)
				resultsWindow.Close();
			IsClosed = true;
			PokeManager.ManagerWindow.PokemonSearchWindow = null;
			PokeManager.ManagerWindow.Focus();
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int newIndex = listViewGameSaves.SelectedIndex;
			if (newIndex != -1) {
				selectedIndex = newIndex;
				selectedGameSave = gameSaves[selectedIndex].Tag as GameSaveFileInfo;
			}
		}

		private void OnCloseClicked(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
