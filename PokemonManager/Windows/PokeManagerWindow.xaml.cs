using Microsoft.Win32;
using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.PokemonStructures.Events;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class PokeManagerWindow : Window {

		private DispatcherTimer timer;
		private IGameSave gameSave;
		private int gameIndex;
		private Generations gen;
		private int selectedTab;

		private bool loaded;

		public int GameIndex {
			get { return gameIndex; }
		}

		public Window PokemonSearchWindow { get; set; }

		public PokeManagerWindow() {

			this.UseLayoutRounding = true;
			PokeManager.Initialize(this);

			this.gameIndex = 0;
			this.loaded = false;
			this.gameSave = null;
			this.gen = Generations.Gen3;
			this.selectedTab = 0;

			this.timer = new DispatcherTimer();
			this.timer.Interval = TimeSpan.FromSeconds(5);
			this.timer.Tick += OnTick;
			this.timer.Start();

			#region Stuff

			/*Gen3GBAGameSave rbySave = new Gen3GBAGameSave(System.IO.Path.Combine("Saves", "Ruby Hack.sav"));

			foreach (IBlockData blockData in rbySave.MostRecentSave.BlockDataCollection) {

				if (blockData.SectionID >= SectionTypes.PCBufferA)
					continue;
				for (int i = 0; i < SectionIDTable.GetContents(blockData.SectionID); i++) {

					if (blockData.Raw[i] == 0x55 && blockData.Raw[i + 1] == 0x55) {
						Console.WriteLine(i);
						//804 NatPokedexBC (0x420)
					}
				}
			}

			Gen3GBAGameSave frSave = new Gen3GBAGameSave(System.IO.Path.Combine("Saves", "FireRed Hack.sav"));

			foreach (IBlockData blockData in frSave.MostRecentSave.BlockDataCollection) {

				if (blockData.SectionID >= SectionTypes.PCBufferA)
					continue;
				for (int i = 0; i < SectionIDTable.GetContents(blockData.SectionID); i++) {

					if (blockData.Raw[i] == 0x55 && blockData.Raw[i + 1] == 0x55) {
						Console.WriteLine(i);
						//100 NatPokedexBC
						//3940 TeamAndItems (Save == 3968) 3940-132=3808
					}
				}
			}*/

			/*Dictionary<SectionTypes, List<DifferenceData>> differenceData = new Dictionary<SectionTypes, List<DifferenceData>>();

			foreach (SectionTypes sectionType in Enum.GetValues(typeof(SectionTypes))) {
				IBlockData least = emSave.LeastRecentSave.BlockDataCollection.GetBlockData(sectionType);
				IBlockData most = emSave.MostRecentSave.BlockDataCollection.GetBlockData(sectionType);
				differenceData.Add(sectionType, new List<DifferenceData>());

				DifferenceData difference = null;
				for (int i = 0; i < SectionIDTable.GetContents(sectionType); i++) {
					if (least.Raw[i] != most.Raw[i]) {
						if (difference == null)
							difference = new DifferenceData(i);
						else
							difference.Length++;
					}
					else if (difference != null) {
						differenceData[sectionType].Add(difference);
						difference = null;
					}
				}
			}*/


			/*int saveIndex = 0;
			while (saveIndex != -1) {
				BlockDataCollection bdcLeast = emSave.LeastRecentSave.BlockDataCollection;
				BlockDataCollection bdcMost = emSave.MostRecentSave.BlockDataCollection;
				Console.WriteLine(saveIndex);
				emSave.Save(System.IO.Path.Combine("Saves", "Tests", saveIndex.ToString() + ".sav"));
				saveIndex++;
				emSave = new Gen3GBAGameSave(System.IO.Path.Combine("Saves", "POKEMON EMER-7b058a7aea5bfbb352026727ebd87e17.sav"));
			}*/

			/*Dictionary<string, GameTypes> gameTypeTable = new Dictionary<string, GameTypes>() {
				{"RUBY", GameTypes.Ruby},
				{"SAPP", GameTypes.Sapphire},
				{"EMER", GameTypes.Emerald},
				{"FIRE", GameTypes.FireRed},
				{"LEAF", GameTypes.LeafGreen}
			};
			string[] files = Directory.GetFiles("Saves");
			foreach (string file in files) {
				foreach (KeyValuePair<string, GameTypes> pair in gameTypeTable) {
					if (System.IO.Path.GetFileName(file).Contains(pair.Key) && System.IO.Path.GetExtension(file) == ".sav") {
						PokeManager.AddGameSave(file, pair.Value);
					}
				}
			}*/
			/*PokeManager.AddGameSave(System.IO.Path.Combine("Saves", "01-GC6E-pokemon_colosseum.gci"));
			PokeManager.AddGameSave(System.IO.Path.Combine("Saves", "01-GC6E-pokemon_colosseum1.gci"));
			PokeManager.AddGameSave(System.IO.Path.Combine("Saves", "01-GC6E-pokemon_colosseum2.gci"));
			PokeManager.AddGameSave(System.IO.Path.Combine("Saves", "01-GC6E-pokemon_colosseum3.gci"));*/

			#endregion

			InitializeComponent();

			UpdateSettingsMenuItems();

			this.loaded = true;

			if (PokeManager.Settings.DefaultStartupSize.Width != 0 && PokeManager.Settings.DefaultStartupSize.Height != 0) {
				this.Width = PokeManager.Settings.DefaultStartupSize.Width;
				this.Height = PokeManager.Settings.DefaultStartupSize.Height;
			}

			PokeManager.IsReloading = true;
			LoadGame(PokeManager.Settings.IsValidDefaultGame ? PokeManager.Settings.DefaultGame : -1);
			PokeManager.IsReloading = false;

			if (((TabItem)tabControl.Items[PokeManager.Settings.DefaultStartupTab]).Visibility == Visibility.Visible)
				Dispatcher.BeginInvoke((Action)(() => tabControl.SelectedIndex = PokeManager.Settings.DefaultStartupTab));
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			if (PokeManager.Settings.StartupMirageIslandCheck) {
				List<GamePokemonSearchResults> mirageIslandResults = CheckAllMirageIslandValues();
				int gameCount = 0;
				int pkmCount = 0;
				foreach (GamePokemonSearchResults results in mirageIslandResults) {
					if (results.ValidPokemon.Count > 0)
						gameCount++;
					pkmCount += results.ValidPokemon.Count;
				}

				if (gameCount > 0) {
					TriggerMessageBox.Show(this, "Found " + pkmCount + " Pokémon matching the Mirage Island values for " + gameCount + " game" + (gameCount != 1 ? "s" : "") + ". If you have not played a game since its internal clock hit midnight then its results will be incorrect.", "Mirage Island Results");
					PokemonSearchWindow = GamePokemonSearchResultsWindow.Show(this, mirageIslandResults);
				}
			}
			if (PokeManager.Settings.StartupShinyEggsCheck) {
				List<GamePokemonSearchResults> mirageIslandResults = CheckAllEggsForShininess();
				int gameCount = 0;
				int pkmCount = 0;
				foreach (GamePokemonSearchResults results in mirageIslandResults) {
					if (results.ValidPokemon.Count > 0)
						gameCount++;
					pkmCount += results.ValidPokemon.Count;
				}

				if (gameCount > 0) {
					TriggerMessageBox.Show(this, "Found " + pkmCount + " Pokémon Eggs that will be shiny when hatched from " + gameCount + " game" + (gameCount != 1 ? "s" : ""), "Egg Shininess Results");
					PokemonSearchWindow = GamePokemonSearchResultsWindow.Show(this, mirageIslandResults, "Egg Shininess Results");
				}
			}
			if (PokeManager.Settings.StartupPokerusCheck) {
				List<PokerusStrain> strains = CheckAllPokemonForPokerus();

				if (strains.Count > 0) {
					string strainList = "";
					foreach (PokerusStrain strain in strains)
						strainList += "\n" + strain.ToString();
					TriggerMessageBox.Show(this, "Found " + strains.Count + " new Pokérus Strains. The following strains will be added to " +
						PokeManager.Settings.ManagerNickname + ". Use them to infect other Pokémon\n" + strainList, "Pokérus Strain Results");
					PokeManager.PokerusStrains.AddRange(strains);
					PokeManager.PokerusStrains.Sort((strain1, strain2) => (strain1.Order - strain2.Order));
					PokeManager.ManagerGameSave.IsChanged = true;
					if (PokeManager.PokerusStrains.Count == 15) {
						TriggerMessageBox.Show(this, "Congratulations, you have collected all 15 Pokérus Strains!", "Pokérus Collection");
					}
				}
			}
			//SecretBaseViewer sb = new SecretBaseViewer();
			//sb.Show();
			/*TestWindow test = new TestWindow();
			test.Show();*/
		}

		private void OnTick(object sender, EventArgs e) {
			// Don't do this while dialog windows are opened.
			if (this.OwnedWindows.Count == 0) {
				bool changesMade = false;
				List<IGameSave> changedGames = new List<IGameSave>();
				for (int i = 0; i < PokeManager.NumGameSaves; i++) {
					GameSaveFileInfo gameSaveFile = PokeManager.GetGameSaveFileInfoAt(i);
					if (gameSaveFile.IsFileModified) {
						// Update this so the program doesn't keep bugging the user if they choose to ignore this issue.
						gameSaveFile.FileInfo = new FileInfo(gameSaveFile.FilePath);
						changesMade = true;
						changedGames.Add(gameSaveFile.GameSave);
						break;
					}
				}
				if (changesMade) {
					MessageBoxResult result = TriggerMessageBox.Show(this,
						"Changes have been made to one or more of the loaded save files outside of Trigger's PC, would you like to reload all saves?" +
						"\n\nWarning:If you have saved changes to the modified games since running the games you could end up with save conflicts if you do reload",
						"Changes Detected", MessageBoxButton.YesNo
					);
					if (result == MessageBoxResult.Yes && PokeManager.IsChanged)
						result = TriggerMessageBox.Show(this, "You have unsaved changes. Are you sure you want to reload everything? Any changes made will be lost", "Unsaved Changes", MessageBoxButton.YesNo);

					if (result == MessageBoxResult.Yes) {
						PokeManager.ReloadEverything();
					}
					else {
						foreach (IGameSave gameSave in changedGames) {
							gameSave.IsChanged = true;
						}
					}
				}
			}
		}

		#region Interface

		public void RefreshUI() {
			this.pokemonViewer.RefreshUI();
			this.trainerInfo.RefreshUI();
			comboBoxGameSaves.RefreshGameSaves();
			pokemonViewer.RefreshGameSaves();

			BitmapSource trainerImage = ResourceDatabase.GetImageFromName("WorldRubySapphireMale");
			if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire)
				trainerImage = ResourceDatabase.GetImageFromName("WorldRubySapphire" + gameSave.TrainerGender.ToString());
			else if (gameSave.GameType == GameTypes.Emerald)
				trainerImage = ResourceDatabase.GetImageFromName("WorldEmerald" + gameSave.TrainerGender.ToString());
			else if (gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen)
				trainerImage = ResourceDatabase.GetImageFromName("WorldFireRedLeafGreen" + gameSave.TrainerGender.ToString());
			else if (gameSave.GameType == GameTypes.Colosseum)
				trainerImage = ResourceDatabase.GetImageFromName("WorldColosseumMale");
			else if (gameSave.GameType == GameTypes.XD)
				trainerImage = ResourceDatabase.GetImageFromName("WorldXDMale");
			else if (gameSave.GameType == GameTypes.Any)
				trainerImage = ResourceDatabase.GetImageFromName("WorldManager" + gameSave.TrainerGender.ToString());
			(menuItemEditTrainer.Icon as Image).Source = trainerImage;

			if (PokemonSearchWindow != null) {
				if (PokemonSearchWindow is WindowSearchPokemonSettings)
					(PokemonSearchWindow as WindowSearchPokemonSettings).RefreshUI();
				else if (PokemonSearchWindow is GamePokemonSearchResultsWindow)
					(PokemonSearchWindow as GamePokemonSearchResultsWindow).RefreshUI();
				else if (PokemonSearchWindow is ItemSearchResultsWindow)
					(PokemonSearchWindow as ItemSearchResultsWindow).RefreshUI();
			}
		}

		public void RefreshSearchResultsUI() {
			pokemonViewer.RefreshPokemonViewer();
			if (PokemonSearchWindow != null) {
				if (PokemonSearchWindow is WindowSearchPokemonSettings)
					(PokemonSearchWindow as WindowSearchPokemonSettings).RefreshUI();
				else if (PokemonSearchWindow is GamePokemonSearchResultsWindow)
					(PokemonSearchWindow as GamePokemonSearchResultsWindow).RefreshUI();
				else if (PokemonSearchWindow is ItemSearchResultsWindow)
					(PokemonSearchWindow as ItemSearchResultsWindow).RefreshUI();
			}
		}
		public void Reload() {
			UpdateSettingsMenuItems();
			pokemonViewer.Reload();
			if (PokemonSearchWindow != null)
				PokemonSearchWindow.Close();

			loaded = false;
			comboBoxGameSaves.ReloadGameSaves();
			comboBoxGameSaves.SelectedGameIndex = PokeManager.Settings.IsValidDefaultGame ? PokeManager.Settings.DefaultGame : -1;
			loaded = true;
			LoadGame(PokeManager.Settings.IsValidDefaultGame ? PokeManager.Settings.DefaultGame : -1);
		}
		public void ReloadGames() {
			PokeManager.IsReloading = true;
			if (PokemonSearchWindow != null)
				PokemonSearchWindow.Close();
			pokemonViewer.Reload();
			loaded = false;
			comboBoxGameSaves.ReloadGameSaves();
			comboBoxGameSaves.SelectedGameIndex = PokeManager.Settings.IsValidDefaultGame ? PokeManager.Settings.DefaultGame : -1;
			loaded = true;
			LoadGame(PokeManager.Settings.IsValidDefaultGame ? PokeManager.Settings.DefaultGame : -1);
			PokeManager.IsReloading = false;
		}
		public void FinishActions() {
			this.pokemonViewer.FinishActions();
		}

		public void ReloadRowComboBoxes() {
			pokemonViewer.ReloadRowComboBoxes();
		}

		#endregion

		#region Game Saves and Generations

		public void LoadGame(int gameIndex) {
			try {
				PokeManager.DropSelection();
				PokeManager.DropPokemon();
				PokeManager.ClearSelectedPokemon();
				comboBoxGameSaves.SelectedIndex = gameIndex + 1;
				gameSave = PokeManager.GetGameSaveAt(gameIndex);

				this.gameIndex = gameIndex;

				BitmapSource trainerImage = ResourceDatabase.GetImageFromName("WorldRubySapphireMale");
				if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire)
					trainerImage = ResourceDatabase.GetImageFromName("WorldRubySapphire" + gameSave.TrainerGender.ToString());
				else if (gameSave.GameType == GameTypes.Emerald)
					trainerImage = ResourceDatabase.GetImageFromName("WorldEmerald" + gameSave.TrainerGender.ToString());
				else if (gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen)
					trainerImage = ResourceDatabase.GetImageFromName("WorldFireRedLeafGreen" + gameSave.TrainerGender.ToString());
				else if (gameSave.GameType == GameTypes.Colosseum)
					trainerImage = ResourceDatabase.GetImageFromName("WorldColosseumMale");
				else if (gameSave.GameType == GameTypes.XD)
					trainerImage = ResourceDatabase.GetImageFromName("WorldXDMale");
				else if (gameSave.GameType == GameTypes.Any)
					trainerImage = ResourceDatabase.GetImageFromName("WorldManager" + gameSave.TrainerGender.ToString());
				(menuItemEditTrainer.Icon as Image).Source = trainerImage;

				int eventCount = 0;
				for (int i = 0; i < PokeManager.NumEvents && gameSave.GameType != GameTypes.Any; i++) {
					EventDistribution eventDist = PokeManager.GetEventAt(i);
					if (eventDist.AllowedGames.HasFlag((GameTypeFlags)(1 << ((int)gameSave.GameType - 1))))
						eventCount++;
				}
				this.tabEventViewer.Visibility = (eventCount > 0 ? Visibility.Visible : Visibility.Collapsed);

				if (gameSave.GameType == GameTypes.PokemonBox) {
					this.tabPokemon.Visibility = Visibility.Visible;
					this.tabTrainerInfo.Visibility = Visibility.Collapsed;
					this.tabItems.Visibility = Visibility.Collapsed;
					this.tabDexViewer.Visibility = Visibility.Collapsed;
					this.tabMailbox.Visibility = Visibility.Collapsed;
					this.tabDecorations.Visibility = Visibility.Collapsed;
					this.tabSecretBases.Visibility = Visibility.Collapsed;
					this.eventViewer.LoadEvents(gameSave);
					if (tabControl.SelectedItem != null && ((TabItem)tabControl.SelectedItem).Visibility != Visibility.Visible || (tabControl.SelectedIndex == 7 && eventCount == 0))
						Dispatcher.BeginInvoke((Action)(() => tabControl.SelectedIndex = 1));

					pokemonViewer.LoadGame(gameSave);
					menuItemEditTrainer.IsEnabled = false;
				}
				else {
					//this.tabSecretBases.Visibility = Visibility.Collapsed;
					if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire || gameSave.GameType == GameTypes.Emerald || gameSave.GameType == GameTypes.Any) {
						this.tabSecretBases.Visibility = Visibility.Visible;
						this.secretBaseManager.LoadGame(gameSave);
					}
					else {
						this.tabSecretBases.Visibility = Visibility.Collapsed;
					}
					menuItemEditTrainer.IsEnabled = true;
					this.tabTrainerInfo.Visibility = Visibility.Visible;
					this.tabItems.Visibility = Visibility.Visible;
					this.tabDexViewer.Visibility = Visibility.Visible;
					this.eventViewer.LoadEvents(gameSave);
					inventoryViewer.LoadInventory(gameSave.Inventory);
					pokemonViewer.LoadGame(gameSave);
					trainerInfo.LoadGameSave(gameSave);
					if (gameSave.Inventory.Decorations != null)
						decorationInventoryViewer.LoadInventory(gameSave.Inventory);
					if (gameSave.Mailbox != null)
						mailViewer.LoadMailbox(gameSave.Mailbox);
					else
						mailViewer.UnloadMailbox();
					dexViewer.LoadGameSave(gameSave);
					if (gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD)
						tabDexViewer.Header = "Strategy Memo";
					else
						tabDexViewer.Header = "Pokédex";
					tabMailbox.Visibility = (gameSave.Mailbox != null ? Visibility.Visible : Visibility.Collapsed);
					tabDecorations.Visibility = (gameSave.Inventory.Decorations != null ? Visibility.Visible : Visibility.Collapsed);
					if (tabControl.SelectedItem != null && ((TabItem)tabControl.SelectedItem).Visibility != Visibility.Visible || (tabControl.SelectedIndex == 7 && eventCount == 0))
						Dispatcher.BeginInvoke((Action)(() => tabControl.SelectedIndex = 0));
					if (tabControl.SelectedIndex == 6)
						dexViewer.RefreshUI();
				}
			}
			catch (Exception ex) {
				if (gameIndex == -1) {
					ErrorMessageBox.Show(ex);
				}
				else {
					MessageBoxResult result = TriggerMessageBox.Show(this, "An error occured while displaying this game save. The save may be corrupted or this may have been an error on Trigger's PC's end. Would you like to view the full exception?", "Load Error", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes)
						ErrorMessageBox.Show(ex);
					LoadGame(-1);
				}
			}
		}
		private void OnGenerationsSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (loaded) {
				gameIndex = -1;
				LoadGame(gameIndex);
			}
		}
		private void OnGameSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (loaded) {
				int newGameIndex = comboBoxGameSaves.SelectedGameIndex;
				if (newGameIndex != -2 && newGameIndex != gameIndex) {
					LoadGame(newGameIndex);
				}
			}
		}
		private void OnSaveClicked(object sender, RoutedEventArgs e) {
			PokeManager.SaveEverything();
		}

		public void RefreshStoredPokemon() {
			pokemonViewer.RefreshStoredPokemon();
		}

		#endregion

		#region Settings

		private void UseFrontSpritesChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.UsedFrontSpritesType = (FrontSpriteSelectionTypes)Enum.Parse(typeof(FrontSpriteSelectionTypes), (sender as MenuItem).Tag as string);
			menuItemUseRSEFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.RSE;
			menuItemUseFRLGFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.FRLG;
			menuItemUseCustomFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.Custom;
			menuItemUseSelectionFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.Selection;
		}

		private void UseBoxSpritesChecked(object sender, RoutedEventArgs e) {
			string type = (sender as MenuItem).Tag as string;
			if (type == "3")
				PokeManager.Settings.UseNewBoxSprites = false;
			else
				PokeManager.Settings.UseNewBoxSprites = true;
			menuItemUseGen3BoxSprites.IsChecked = !PokeManager.Settings.UseNewBoxSprites;
			menuItemUseGen6BoxSprites.IsChecked = PokeManager.Settings.UseNewBoxSprites;
		}
		private void OnMakeBackupsChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.MakeBackups = ((MenuItem)sender).IsChecked;
		}
		private void OnTossConfirmationChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.TossConfirmation = ((MenuItem)sender).IsChecked;
		}
		private void OnReleaseConfirmationChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.ReleaseConfirmation = ((MenuItem)sender).IsChecked;
		}

		#endregion

		#region Menu Items

		private void OnGiveNicknameClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.RefreshUI();
			string newNickname = AddSaveNicknameWindow.ShowDialog(this, PokeManager.GetGameSaveFileInfoNickname(gameIndex));
			if (newNickname != null) {
				PokeManager.SetGameSaveFileInfoNickname(gameIndex, newNickname);
			}
			comboBoxGameSaves.RefreshGameSaves();
			pokemonViewer.RefreshGameSaves();
		}

		private void OnChangeGameTypeClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.RefreshUI();
			IGameSave save = PokeManager.GetGameSaveAt(gameIndex);
			if (save.GameType == GameTypes.Ruby || save.GameType == GameTypes.Sapphire) {
				var gameType = SelectGameTypeWindow.ShowDialog(this, GameTypes.Ruby, GameTypes.Sapphire);
				if (gameType.HasValue && gameType.Value != GameTypes.Any)
					PokeManager.SetGameTypeOfGameSaveAt(gameIndex, gameType.Value);
			}
			else if (save.GameType == GameTypes.FireRed || save.GameType == GameTypes.LeafGreen) {
				var gameType = SelectGameTypeWindow.ShowDialog(this, GameTypes.FireRed, GameTypes.LeafGreen);
				if (gameType.HasValue && gameType.Value != GameTypes.Any)
					PokeManager.SetGameTypeOfGameSaveAt(gameIndex, gameType.Value);
			}
			else {
				// No need
				TriggerMessageBox.Show(this, "Only one game type applies to this save");
			}
		}
		private void OnJapaneseChecked(object sender, RoutedEventArgs e) {
			PokeManager.GetGameSaveFileInfoAt(gameIndex).IsJapanese = ((MenuItem)sender).IsChecked;
			gameSave.IsJapanese = ((MenuItem)sender).IsChecked;
			gameSave.IsChanged = true;
			comboBoxGameSaves.RefreshGameSaves();
			pokemonViewer.RefreshGameSaves();
		}

		#endregion

		private void OnWindowClosing(object sender, CancelEventArgs e) {
			if (PokeManager.Settings.CloseConfirmation && PokeManager.IsChanged) {
				MessageBoxResult result = TriggerMessageBox.Show(this, "You have unsaved changes, would you like to save before closing?", "Save Changes", MessageBoxButton.YesNoCancel);

				if (result == MessageBoxResult.Yes)
					PokeManager.SaveEverything();
				else if (result == MessageBoxResult.Cancel)
					e.Cancel = true;
			}
		}

		private void OnControlsClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.RefreshUI();
			ControlsWindow.ShowDialog(Window.GetWindow(this));
		}

		private void OnManageBoxesClicked(object sender, RoutedEventArgs e) {
			PokeManager.FinishActions();
			ManageBoxesWindow.ShowDialog(Window.GetWindow(this));
		}

		private void OnManageSavesClicked(object sender, RoutedEventArgs e) {
			PokeManager.FinishActions();
			SaveManager.ShowDialog(this);
		}

		private void OnCloseClicked(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnReloadSavesClicked(object sender, RoutedEventArgs e) {
			PokeManager.FinishActions();
			MessageBoxResult result = TriggerMessageBox.Show(this, "Are you sure you want to reload everything? Any changes made will be lost", "Reload Saves", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes)
				PokeManager.ReloadEverything();
		}

		private void OnCloseConfirmationChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.CloseConfirmation = menuItemCloseConfirmation.IsChecked;
		}

		private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void OnVolumeClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.RefreshUI();
			VolumeWindow.ShowDialog(this);
			menuItemVolume.Header = "Volume : " + (int)(PokeManager.Settings.Volume * 100) + "%" + (PokeManager.Settings.IsMuted ? " (Muted)" : "");
		}

		private void OnCreditsClicked(object sender, RoutedEventArgs e) {
			CreditsWindow.Show(this);
		}

		private void OnEditFrontSpritesSelectionClicked(object sender, RoutedEventArgs e) {
			SpriteCustomizationWindow.Show(this);
		}

		private void OnAboutClicked(object sender, RoutedEventArgs e) {
			AboutWindow.Show(this);
		}

		private void OnEditTrainerClicked(object sender, RoutedEventArgs e) {
			if (gameIndex == -1)
				EditTrainerWindow.ShowDialog(this);
			else
				EditGameTrainerWindow.ShowDialog(this, gameSave);
		}

		private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (selectedTab != tabControl.SelectedIndex) {
				selectedTab = tabControl.SelectedIndex;
				if (selectedTab == 6) {
					dexViewer.RefreshUI();
				}
			}
		}

		public List<GamePokemonSearchResults> CheckAllMirageIslandValues() {
			List<GamePokemonSearchResults> mirageIslandResults = new List<GamePokemonSearchResults>();
			List<ushort> mirageValues = new List<ushort>();
			for (int i = 0; i < PokeManager.NumGameSaves; i++) {
				//GameSaveFileInfo
				IGameSave gameSave = PokeManager.GetGameSaveAt(i);
				if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire || gameSave.GameType == GameTypes.Emerald) {
					mirageIslandResults.Add(new GamePokemonSearchResults(gameSave));
					mirageValues.Add(((GBAGameSave)gameSave).MirageIslandValue);
				}
			}

			for (int i = 0; i < PokeManager.NumGameSaves; i++) {
				IGameSave gameSave = PokeManager.GetGameSaveAt(i);
				if (gameSave is ManagerGameSave) {
					for (int j = 0; j < (gameSave as ManagerGameSave).NumPokePCRows; j++) {
						foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(j)) {
							ushort miragePID = (ushort)pokemon.Personality;
							for (int k = 0; k < mirageIslandResults.Count; k++) {
								if (miragePID == mirageValues[k])
									mirageIslandResults[k].ValidPokemon.Add(pokemon);
							}
						}
					}
				}
				else {
					foreach (IPokemon pokemon in gameSave.PokePC) {
						ushort miragePID = (ushort)pokemon.Personality;
						for (int j = 0; j < mirageIslandResults.Count; j++) {
							if (miragePID == mirageValues[j])
								mirageIslandResults[j].ValidPokemon.Add(pokemon);
						}
					}
				}
			}
			for (int i = 0; i < mirageIslandResults.Count; i++) {
				if (mirageIslandResults[i].ValidPokemon.Count == 0) {
					mirageIslandResults.RemoveAt(i);
					i--;
				}
			}
			return mirageIslandResults;
		}
		public List<GamePokemonSearchResults> CheckAllEggsForShininess() {
			List<GamePokemonSearchResults> shinyEggResults = new List<GamePokemonSearchResults>();
			for (int i = 0; i < PokeManager.NumGameSaves; i++) {
				//GameSaveFileInfo
				IGameSave gameSave = PokeManager.GetGameSaveAt(i);
				if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire || gameSave.GameType == GameTypes.Emerald ||
					gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen) {
					shinyEggResults.Add(new GamePokemonSearchResults(gameSave));
				}
			}

			for (int i = 0; i < PokeManager.NumGameSaves; i++) {
				IGameSave gameSave = PokeManager.GetGameSaveAt(i);
				if (gameSave is ManagerGameSave) {
					for (int j = 0; j < (gameSave as ManagerGameSave).NumPokePCRows; j++) {
						foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(j)) {
							if (!pokemon.IsEgg)
								continue;
							byte[] pidBytes = BitConverter.GetBytes(pokemon.Personality);
							ushort pid1 = BitConverter.ToUInt16(pidBytes, 0);
							ushort pid2 = BitConverter.ToUInt16(pidBytes, 2);
							foreach (GamePokemonSearchResults results in shinyEggResults) {
								if (((uint)results.GameSave.TrainerID ^ (uint)results.GameSave.SecretID ^ (uint)pid1 ^ (uint)pid2) < 8) {
									results.ValidPokemon.Add(pokemon);
								}
							}
						}
					}
				}
				else {
					foreach (IPokemon pokemon in gameSave.PokePC) {
						if (!pokemon.IsEgg)
							continue;
						byte[] pidBytes = BitConverter.GetBytes(pokemon.Personality);
						ushort pid1 = BitConverter.ToUInt16(pidBytes, 0);
						ushort pid2 = BitConverter.ToUInt16(pidBytes, 2);
						foreach (GamePokemonSearchResults results in shinyEggResults) {
							if (((uint)results.GameSave.TrainerID ^ (uint)results.GameSave.SecretID ^ (uint)pid1 ^ (uint)pid2) < 8) {
								results.ValidPokemon.Add(pokemon);
							}
						}
					}
				}
			}
			for (int i = 0; i < shinyEggResults.Count; i++) {
				if (shinyEggResults[i].ValidPokemon.Count == 0) {
					shinyEggResults.RemoveAt(i);
					i--;
				}
			}
			return shinyEggResults;
		}
		public List<PokerusStrain> CheckAllPokemonForPokerus() {
			List<PokerusStrain> foundStrains = new List<PokerusStrain>();
			
			for (int i = 0; i < PokeManager.NumGameSaves; i++) {
				IGameSave gameSave = PokeManager.GetGameSaveAt(i);
				if (gameSave is ManagerGameSave) {
					for (int j = 0; j < (gameSave as ManagerGameSave).NumPokePCRows; j++) {
						foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(j)) {
							PokerusStrain strain = new PokerusStrain(pokemon.PokerusStrain);
							if (pokemon.PokerusStrain != 0 && pokemon.PokerusStatus == PokerusStatuses.Infected &&
								!PokeManager.PokerusStrains.Contains(strain) &&
								!foundStrains.Contains(strain)) {
									foundStrains.Add(strain);
							}
						}
					}
				}
				else {
					foreach (IPokemon pokemon in gameSave.PokePC) {
						PokerusStrain strain = new PokerusStrain(pokemon.PokerusStrain);
						if (pokemon.PokerusStrain != 0 && pokemon.PokerusStatus == PokerusStatuses.Infected &&
							!PokeManager.PokerusStrains.Contains(strain) &&
							!foundStrains.Contains(strain)) {
							foundStrains.Add(strain);
						}
					}
				}
			}
			foundStrains.Sort((strain1, strain2) => (strain1.Order - strain2.Order));
			return foundStrains;
		}

		private void OnMirageIslandClicked(object sender, RoutedEventArgs e) {
			CheckAllMirageIslandValues();
		}

		private void OnBoxRowClicked(object sender, RoutedEventArgs e) {
			int count = int.Parse((sender as MenuItem).Tag as string);
			PokeManager.Settings.NumBoxRows = count;
			menuItemBoxRows1.IsChecked = PokeManager.Settings.NumBoxRows == 1;
			menuItemBoxRows2.IsChecked = PokeManager.Settings.NumBoxRows == 2;
			menuItemBoxRows3.IsChecked = PokeManager.Settings.NumBoxRows == 3;
			pokemonViewer.RefreshUI();
		}

		private void OnDuplicateBoxRowsChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.AllowDoubleBoxRows = menuItemDoubleBoxRows.IsChecked;
			pokemonViewer.OnGameSwitch();
		}

		private void OnSearchPokemonClicked(object sender, RoutedEventArgs e) {
			if (PokemonSearchWindow != null && !(PokemonSearchWindow is WindowSearchPokemonSettings)) {
				PokemonSearchWindow.Close();
				PokemonSearchWindow = null;
			}

			if (PokemonSearchWindow != null)
				PokemonSearchWindow.Focus();
			else
				PokemonSearchWindow = WindowSearchPokemonSettings.Show(this);
		}
		private void OnMirageScanClicked(object sender, RoutedEventArgs e) {
			if (PokemonSearchWindow != null) {
				PokemonSearchWindow.Close();
				PokemonSearchWindow = null;
			}

			List<GamePokemonSearchResults> mirageIslandResults = CheckAllMirageIslandValues();
			int gameCount = 0;
			int pkmCount = 0;
			foreach (GamePokemonSearchResults results in mirageIslandResults) {
				if (results.ValidPokemon.Count > 0)
					gameCount++;
				pkmCount += results.ValidPokemon.Count;
			}

			if (gameCount > 0) {
				TriggerMessageBox.Show(this, "Found " + pkmCount + " Pokémon matching the Mirage Island values for " + gameCount + " game" + (gameCount != 1 ? "s" : "") + ". If you have not played a game since its internal clock hit midnight then its results will be incorrect.", "Mirage Island Results");
				PokemonSearchWindow = GamePokemonSearchResultsWindow.Show(this, mirageIslandResults);
			}
			else {
				TriggerMessageBox.Show(this, "No matching Pokémon found to visit Mirage Island", "Mirage Island Results");
			}
		}
		private void OnEggShininessScanClicked(object sender, RoutedEventArgs e) {
			if (PokemonSearchWindow != null) {
				PokemonSearchWindow.Close();
				PokemonSearchWindow = null;
			}

			List<GamePokemonSearchResults> mirageIslandResults = CheckAllEggsForShininess();
			int gameCount = 0;
			int pkmCount = 0;
			foreach (GamePokemonSearchResults results in mirageIslandResults) {
				if (results.ValidPokemon.Count > 0)
					gameCount++;
				pkmCount += results.ValidPokemon.Count;
			}

			if (gameCount > 0) {
				TriggerMessageBox.Show(this, "Found " + pkmCount + " Pokémon Eggs that will be shiny when hatched from " + gameCount + " game" + (gameCount != 1 ? "s" : ""), "Egg Shininess Results");
				PokemonSearchWindow = GamePokemonSearchResultsWindow.Show(this, mirageIslandResults, "Egg Shininess Results");
			}
			else {
				TriggerMessageBox.Show(this, "No Pokémon Eggs will be shiny when hatched from any game", "Egg Shininess Results");
			}
		}


		public void GotoPokemon(IPokemon pokemon) {

			if (PokeManager.IsHoldingPokemon) {
				PokeManager.DropAll();
				PokeManager.RefreshUI();
			}

			if (gameIndex != pokemon.GameSave.GameIndex)
				LoadGame(pokemon.GameSave.GameIndex);

			pokemonViewer.GotoPokemon(pokemon);
			if (tabControl.SelectedIndex != 1)
				Dispatcher.BeginInvoke((Action)(() => tabControl.SelectedIndex = 1));
		}

		private void OnMirageIslandChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.StartupMirageIslandCheck = menuItemStartupMirageIsland.IsChecked;
		}

		private void OnShinyEggsChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.StartupShinyEggsCheck = menuItemStartupShinyEggs.IsChecked;
		}

		private void OnForceSaveChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.AlwaysSaveAllSaves = menuItemForceSave.IsChecked;
		}

		private void OnMysteryEggsChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.MysteryEggs = menuItemMysteryEggs.IsChecked;
		}

		private void UpdateSettingsMenuItems() {

			menuItemUseRSEFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.RSE;
			menuItemUseFRLGFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.FRLG;
			menuItemUseCustomFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.Custom;
			menuItemUseSelectionFrontSprites.IsChecked = PokeManager.Settings.UsedFrontSpritesType == FrontSpriteSelectionTypes.Selection;
			menuItemUseGen3BoxSprites.IsChecked = !PokeManager.Settings.UseNewBoxSprites;
			menuItemUseGen6BoxSprites.IsChecked = PokeManager.Settings.UseNewBoxSprites;
			menuItemMakeBackups.IsChecked = PokeManager.Settings.MakeBackups;
			menuItemCloseConfirmation.IsChecked = PokeManager.Settings.CloseConfirmation;
			menuItemTossConfirmation.IsChecked = PokeManager.Settings.TossConfirmation;
			menuItemReleaseConfirmation.IsChecked = PokeManager.Settings.ReleaseConfirmation;
			menuItemVolume.Header = "Volume : " + (int)(PokeManager.Settings.Volume * 100) + "%" + (PokeManager.Settings.IsMuted ? " (Muted)" : "");
			menuItemBoxRows1.IsChecked = PokeManager.Settings.NumBoxRows == 1;
			menuItemBoxRows2.IsChecked = PokeManager.Settings.NumBoxRows == 2;
			menuItemBoxRows3.IsChecked = PokeManager.Settings.NumBoxRows == 3;
			menuItemDoubleBoxRows.IsChecked = PokeManager.Settings.AllowDoubleBoxRows;

			menuItemForceSave.IsChecked = PokeManager.Settings.AlwaysSaveAllSaves;
			menuItemMysteryEggs.IsChecked = PokeManager.Settings.MysteryEggs;
			menuItemKeepMissingFiles.IsChecked = PokeManager.Settings.KeepMissingFiles;

			menuItemStartupMirageIsland.IsChecked = PokeManager.Settings.StartupMirageIslandCheck;
			menuItemStartupShinyEggs.IsChecked = PokeManager.Settings.StartupShinyEggsCheck;
			menuItemStartupPokerus.IsChecked = PokeManager.Settings.StartupPokerusCheck;
			menuItemRevealEggs.IsChecked = PokeManager.Settings.RevealEggs;

			menuItemAutoSortItems.IsChecked = PokeManager.Settings.AutoSortItems;
		}

		private void OnRevealEggsChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.RevealEggs = menuItemRevealEggs.IsChecked;
		}

		private void OnAutoSortItemsChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.AutoSortItems = menuItemAutoSortItems.IsChecked;
			inventoryViewer.RefreshUI();
		}

		private void OnStartupPokerusChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.StartupPokerusCheck = menuItemStartupPokerus.IsChecked;
		}

		private void OnPokerusScanClicked(object sender, RoutedEventArgs e) {
			List<PokerusStrain> strains = CheckAllPokemonForPokerus();

			if (strains.Count > 0) {
				string strainList = "";
				foreach (PokerusStrain strain in strains)
					strainList += "\n" + strain.ToString();
				TriggerMessageBox.Show(this, "Found " + strains.Count + " new Pokérus Strains. The following strains will be added to " +
					PokeManager.Settings.ManagerNickname + ". Use them to infect other Pokémon\n" + strainList, "Pokérus Strain Results");
				PokeManager.PokerusStrains.AddRange(strains);
				PokeManager.PokerusStrains.Sort((strain1, strain2) => (strain1.Order - strain2.Order));
				PokeManager.ManagerGameSave.IsChanged = true;
				if (PokeManager.PokerusStrains.Count == 15) {
					TriggerMessageBox.Show(this, "Congratulations, you have collected all 15 Pokérus Strains!", "Pokérus Collection");
				}
				PokeManager.RefreshUI();
			}
			else {
				TriggerMessageBox.Show(this, "No new Pokérus Strains found", "Pokérus Strain Results");
			}
		}

		private void OnSetCurrentDefaultsClicked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.DisableChangesWhileLoading = true;
			PokeManager.Settings.DefaultStartupSize = new Size(Width, Height);
			PokeManager.Settings.DefaultStartupTab = tabControl.SelectedIndex;
			PokeManager.Settings.DefaultGame = gameIndex;
			PokeManager.Settings.DefaultBoxRow1 = pokemonViewer.CurrentRow1;
			PokeManager.Settings.DefaultBoxGame2 = pokemonViewer.CurrentGame2;
			PokeManager.Settings.DefaultBoxRow2 = pokemonViewer.CurrentRow2;
			PokeManager.Settings.DefaultBoxGame3 = pokemonViewer.CurrentGame3;
			PokeManager.Settings.DefaultBoxRow3 = pokemonViewer.CurrentRow3;
			PokeManager.Settings.DisableChangesWhileLoading = false;
			PokeManager.SaveSettings();
		}

		private void OnKeepMissingFilesChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.KeepMissingFiles = menuItemKeepMissingFiles.IsChecked;
		}

		private void OnSearchItemsClicked(object sender, RoutedEventArgs e) {
			if (PokemonSearchWindow != null && !(PokemonSearchWindow is ItemSearchResultsWindow)) {
				PokemonSearchWindow.Close();
				PokemonSearchWindow = null;
			}

			if (PokemonSearchWindow != null) {
				PokemonSearchWindow.Focus();
				((ItemSearchResultsWindow)PokemonSearchWindow).SetDecorationMode(false);
			}
			else
				PokemonSearchWindow = ItemSearchResultsWindow.Show(this, false);
		}

		private void OnSearchDecorationsClicked(object sender, RoutedEventArgs e) {
			if (PokemonSearchWindow != null && !(PokemonSearchWindow is ItemSearchResultsWindow)) {
				PokemonSearchWindow.Close();
				PokemonSearchWindow = null;
			}

			if (PokemonSearchWindow != null) {
				PokemonSearchWindow.Focus();
				((ItemSearchResultsWindow)PokemonSearchWindow).SetDecorationMode(true);
			}
			else
				PokemonSearchWindow = ItemSearchResultsWindow.Show(this, true);
		}

		public void GotoItem(int gameIndex, ItemTypes pocket, ushort itemID) {
			if (this.gameIndex != gameIndex)
				LoadGame(gameIndex);
			Dispatcher.BeginInvoke((Action)(() => tabControl.SelectedIndex = 2));
			inventoryViewer.GotoItem(pocket, itemID);
		}
		public void GotoDecoration(int gameIndex, DecorationTypes pocket, byte decorationID) {
			if (this.gameIndex != gameIndex)
				LoadGame(gameIndex);
			Dispatcher.BeginInvoke((Action)(() => tabControl.SelectedIndex = 3));
			decorationInventoryViewer.GotoDecoration(pocket, decorationID);
		}
	}
}
