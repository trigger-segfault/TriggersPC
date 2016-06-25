using Microsoft.Win32;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for GameSecretBaseManager.xaml
	/// </summary>
	public partial class SecretBaseViewer : UserControl {

		private IGameSave gameSave;
		private SecretBase secretBase;

		public SecretBaseViewer() {
			InitializeComponent();
		}

		public void LoadGame(IGameSave gameSave) {
			this.gameSave = gameSave;

			AddListViewItems();
			roomDisplay.UnloadSecretBase();
			locationDisplay.UnloadLocation();
			labelOwner.Content = "";
			labelTrainerID.Content = "";
			labelSecretID.Content = "";
			labelRequires.Content = "";
			gridTeam.Visibility = Visibility.Hidden;
			OnSecretBaseSelected(null, null);

			/*this.buttonSend.Visibility		= (IsGBAGame ? Visibility.Visible : Visibility.Hidden);
			this.buttonRegister.Visibility	= (IsGBAGame ? Visibility.Visible : Visibility.Hidden);
			this.buttonImport.Visibility	= (IsGBAGame ? Visibility.Hidden : Visibility.Visible);
			this.buttonExport.Visibility	= (IsGBAGame ? Visibility.Hidden : Visibility.Visible);*/
		}

		private bool IsGBAGame {
			get { return gameSave is GBAGameSave; }
		}
		private GBAGameSave GBAGameSave {
			get { return gameSave as GBAGameSave; }
		}
		private SecretBaseManager SecretBaseManager {
			get { return GBAGameSave.SecretBaseManager; }
		}

		private void OnSecretBaseSelected(object sender, SelectionChangedEventArgs e) {
			if (listViewSecretBases.SelectedIndex != -1)
				secretBase = (listViewSecretBases.SelectedItem as ListViewItem).Tag as SecretBase;

			buttonAdd.IsEnabled = (!IsGBAGame || GBAGameSave.SecretBaseManager.SharedSecretBases.Count < 19);
			buttonImport.IsEnabled = (!IsGBAGame || GBAGameSave.SecretBaseManager.SharedSecretBases.Count < 19);
			if (listViewSecretBases.SelectedIndex == -1 || secretBase == null) {
				roomDisplay.UnloadSecretBase();
				locationDisplay.UnloadLocation();
				rectRoomDisplay.Visibility = Visibility.Hidden;
				roomDisplay.Visibility = Visibility.Hidden;
				locationDisplay.Visibility = Visibility.Hidden;
				labelOwner.Content = "";
				labelTrainerID.Content = "";
				labelSecretID.Content = "";
				labelRequires.Content = "";
				gridTeam.Visibility = Visibility.Visible;
				secretBase = null;
				buttonRemove.IsEnabled = false;
				buttonEditBase.IsEnabled = listViewSecretBases.SelectedIndex != -1;
				buttonEditTrainer.IsEnabled = false;
				buttonRegister.IsEnabled = false;
				buttonExport.IsEnabled = false;
				buttonSend.IsEnabled = false;
				for (int i = 0; i < 6; i++) {
					Image imageTeam = (Image)FindName("imageTeam" + (i + 1).ToString());
					Label labelTeam = (Label)FindName("labelTeam" + (i + 1).ToString());
					imageTeam.Source = null;
					labelTeam.Content = "";
				}
			}
			else {
				buttonRemove.IsEnabled = true;
				buttonEditBase.IsEnabled = true;
				buttonExport.IsEnabled = true;
				buttonSend.IsEnabled = true;
				rectRoomDisplay.Visibility = Visibility.Visible;
				roomDisplay.Visibility = Visibility.Visible;
				locationDisplay.Visibility = Visibility.Visible;
				roomDisplay.LoadSecretBase(secretBase);
				locationDisplay.LoadLocation(secretBase.LocationID);
				labelOwner.Content = secretBase.TrainerName;
				labelTrainerID.Content = secretBase.TrainerID.ToString("00000");
				labelSecretID.Content = secretBase.SecretID.ToString("00000");
				labelRequires.Content = (secretBase.LocationData.Requirements ?? "Nothing");
				if (secretBase.IsPlayerSecretBase) {
					buttonRegister.IsEnabled = false;
					buttonEditTrainer.IsEnabled = false;
				}
				else {
					buttonEditTrainer.IsEnabled = true;
					buttonRegister.IsEnabled = IsGBAGame;
					buttonRegister.Content = ((SharedSecretBase)secretBase).IsRegistered ? "Unregister" : "Register";

				}
				gridTeam.Visibility = Visibility.Visible;
				for (int i = 0; i < 6; i++) {
					Image imageTeam = (Image)FindName("imageTeam" + (i + 1).ToString());
					Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (i + 1).ToString());
					Label labelTeam = (Label)FindName("labelTeam" + (i + 1).ToString());
					if (i < secretBase.PokemonTeam.Count) {
						imageTeam.Source = PokemonDatabase.GetPokemonBoxImageFromDexID(secretBase.PokemonTeam[i].DexID, false, secretBase.PokemonTeam[i].FormID);
						labelTeam.Content = secretBase.PokemonTeam[i].Level.ToString();

						rectTeam.Visibility = Visibility.Visible;
						ToolTip tooltip = new ToolTip();
						string content = "";
						if (secretBase.PokemonTeam[i].IsHoldingItem) {
							content = "Holding: " + secretBase.PokemonTeam[i].HeldItemData.Name + "\n";
						}
						content += secretBase.PokemonTeam[i].Move1Data.Name;
						for (int j = 1; j < 4; j++) {
							if (secretBase.PokemonTeam[i].GetMoveIDAt(j) != 0)
								content += "\n" + secretBase.PokemonTeam[i].GetMoveDataAt(j).Name;
						}
						tooltip.Content = content;
						rectTeam.ToolTip = tooltip;
					}
					else {
						imageTeam.Source = null;
						rectTeam.Visibility = Visibility.Hidden;
						rectTeam.ToolTip = null;
						labelTeam.Content = "";
					}
				}
			}
		}

		private void AddListViewItems() {
			listViewSecretBases.Items.Clear();
			if (IsGBAGame) {
				if (GBAGameSave.SecretBaseLocation != 0)
					AddListViewItem(new PlayerSecretBase(gameSave));
				else
					AddListViewItem(null);

				Separator separator = new Separator();
				separator.Height = 7;
				separator.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				listViewSecretBases.Items.Add(separator);

				foreach (SharedSecretBase secretBase in (gameSave as GBAGameSave).SecretBaseManager.SharedSecretBases) {
					AddListViewItem(secretBase);
				}
			}
			else {
				foreach (SharedSecretBase secretBase in PokeManager.SecretBases) {
					AddListViewItem(secretBase);
				}
			}
		}

		private void AddListViewItem(SecretBase secretBase) {
			ListViewItem listViewItem = new ListViewItem();
			FillListViewItem(listViewItem, secretBase);
			listViewSecretBases.Items.Add(listViewItem);
		}

		private void UpdateListViewItems() {
			int index = 0;
			foreach (object item in listViewSecretBases.Items) {
				if (item is ListViewItem) {
					if (IsGBAGame && index == 0 && (((ListViewItem)item).Tag as SecretBase) == null)
						FillListViewItem((ListViewItem)item, new PlayerSecretBase(gameSave));
					else
						FillListViewItem((ListViewItem)item);
				}
				index++;
			}
		}

		private void FillListViewItem(ListViewItem listViewItem, SecretBase secretBase = null) {
			if (secretBase == null)
				secretBase = listViewItem.Tag as SecretBase;
			else
				listViewItem.Tag = secretBase;

			if (secretBase == null) {
				listViewItem.Content = "No Player Secret Base";
			}
			else {
				StackPanel stackPanel = new StackPanel();
				stackPanel.Orientation = Orientation.Horizontal;
				Grid grid = new Grid();
				grid.Width = 16;
				grid.Height = 18;

				Image type = new Image();
				type.Width = 16;
				type.Height = 16;
				type.HorizontalAlignment = HorizontalAlignment.Center;
				type.VerticalAlignment = VerticalAlignment.Center;
				type.Source = ResourceDatabase.GetImageFromName("SecretBaseType" + secretBase.LocationData.Type.ToString());

				Label layout = new Label();
				layout.Padding = new Thickness(0, 0, 0, 0);
				layout.Width = 16;
				layout.Height = 16;
				layout.HorizontalAlignment = HorizontalAlignment.Center;
				layout.VerticalAlignment = VerticalAlignment.Center;
				layout.HorizontalContentAlignment = HorizontalAlignment.Center;
				layout.VerticalContentAlignment = VerticalAlignment.Center;
				layout.Content = secretBase.LocationData.Layout.ToString();
				layout.FontWeight = FontWeights.Bold;
				//layout.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

				grid.Children.Add(type);
				grid.Children.Add(layout);

				Label label = new Label();
				label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				label.Padding = new Thickness(4, 0, 4, 0);
				label.Content = "Route " + secretBase.RouteData.ID + " - " + secretBase.TrainerName + "";

				Image registered = new Image();
				registered.Width = 9;
				registered.Height = 9;
				registered.HorizontalAlignment = HorizontalAlignment.Center;
				registered.VerticalAlignment = VerticalAlignment.Center;
				registered.Source = ResourceDatabase.GetImageFromName("PokedexRRegistered");
				registered.Visibility = Visibility.Hidden;
				if (!secretBase.IsPlayerSecretBase && ((SharedSecretBase)secretBase).IsRegistered)
					registered.Visibility = Visibility.Visible;

				stackPanel.Children.Add(grid);
				stackPanel.Children.Add(label);
				stackPanel.Children.Add(registered);
				listViewItem.Content = stackPanel;
			}
		}

		private void OnTeamMouseEnter(object sender, MouseEventArgs e) {
			int index = int.Parse((string)((Rectangle)sender).Tag) - 1;
			Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (index + 1).ToString());
			rectTeam.Opacity = 0.5;
		}

		private void OnTeamMouseLeave(object sender, MouseEventArgs e) {
			int index = int.Parse((string)((Rectangle)sender).Tag) - 1;
			Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (index + 1).ToString());
			rectTeam.Opacity = 0;
		}

		private void OnEditClicked(object sender, RoutedEventArgs e) {
			if (secretBase == null && IsGBAGame && listViewSecretBases.SelectedIndex == 0) {
				var location = SecretBaseLocationChooser.Show(Window.GetWindow(this), 0, IsGBAGame ? GBAGameSave.SecretBaseManager : null);
				if (location.HasValue && location.Value != 0) {
					GBAGameSave.SecretBaseLocation = location.Value;
					UpdateListViewItems();
					OnSecretBaseSelected(null, null);
				}
			}
			else {
				SecretBaseEditor.Show(Window.GetWindow(this), secretBase);
				//UpdateListViewItems();
				int newIndex;
				if (!secretBase.IsPlayerSecretBase) {
					if (IsGBAGame) {
						GBAGameSave.SecretBaseManager.Sort();
						newIndex = 2 + GBAGameSave.SecretBaseManager.SharedSecretBases.IndexOf((SharedSecretBase)secretBase);
					}
					else {
						PokeManager.SortSecretBases();
						newIndex = PokeManager.SecretBases.IndexOf((SharedSecretBase)secretBase);
					}
					AddListViewItems();
					listViewSecretBases.SelectedIndex = newIndex;
					OnSecretBaseSelected(null, null);
				}
				else {
					UpdateListViewItems();
					OnSecretBaseSelected(null, null);
				}
			}
		}

		private void OnRegisterClicked(object sender, RoutedEventArgs e) {
			if (SecretBaseManager.RegistrationCount < 10 || ((SharedSecretBase)secretBase).IsRegistered) {
				((SharedSecretBase)secretBase).IsRegistered = !((SharedSecretBase)secretBase).IsRegistered;

				buttonRegister.Content = ((SharedSecretBase)secretBase).IsRegistered ? "Unregister" : "Register";
				UpdateListViewItems();
			}
			else {
				TriggerMessageBox.Show(Window.GetWindow(this), "You can only have 10 Secret Bases registered per game.", "Can't Register");
			}
		}

		private void OnAddClicked(object sender, RoutedEventArgs e) {
			if (!IsGBAGame || GBAGameSave.SecretBaseManager.SharedSecretBases.Count < 19) {
				var location = SecretBaseLocationChooser.Show(Window.GetWindow(this), 0, IsGBAGame ? GBAGameSave.SecretBaseManager : null);
				if (location.HasValue && location.Value != 0) {
					SharedSecretBase newSecretBase = new SharedSecretBase(location.Value, null);
					if (IsGBAGame) {
						bool result = SecretBaseEditTrainerWindow.Show(Window.GetWindow(this), newSecretBase, false);
						if (result) {
							GBAGameSave.SecretBaseManager.AddSecretBase(newSecretBase);
							AddListViewItems();
							for (int i = 0; i < GBAGameSave.SecretBaseManager.SharedSecretBases.Count; i++) {
								if (GBAGameSave.SecretBaseManager.SharedSecretBases[i].LocationID == newSecretBase.LocationID) {
									listViewSecretBases.SelectedIndex = i + 2;
									break;
								}
							}
						}
					}
					else {

					}
				}
			}
			else {
				TriggerMessageBox.Show(Window.GetWindow(this), "This game already has the maximum amount of 19 shared Secret Bases", "Can't Import");
			}
		}

		private void OnRemoveClicked(object sender, RoutedEventArgs e) {
			if (secretBase.IsPlayerSecretBase) {
				MessageBoxResult result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to remove the player's Secret Base?", "Remove Secret Base", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					GBAGameSave.SecretBaseLocation = 0;
				}
			}
			else {
				MessageBoxResult result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to remove this Secret Base?", "Remove Secret Base", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					if (IsGBAGame) {
						GBAGameSave.SecretBaseManager.SharedSecretBases.Remove((SharedSecretBase)secretBase);
						GBAGameSave.IsChanged = true;
					}
					else {
						PokeManager.SecretBases.Remove((SharedSecretBase)secretBase);
						PokeManager.ManagerGameSave.IsChanged = true;
					}
				}
			}
			AddListViewItems();
		}

		private void OnSendToClicked(object sender, RoutedEventArgs e) {
			if (secretBase.HasTeam) {
				var gameIndex = SecretBaseSendToWindow.ShowDialog(Window.GetWindow(this), gameSave.GameIndex);

				if (gameIndex.HasValue) {
					if (gameIndex.Value == -1) {
						PokeManager.AddSecretBase(secretBase);
					}
					else {
						GBAGameSave sendToGameSave = (GBAGameSave)PokeManager.GetGameSaveAt(gameIndex.Value);
						if (sendToGameSave.SecretBaseManager.SharedSecretBases.Count < 19) {
							if (sendToGameSave.SecretBaseManager.IsLocationInUse(secretBase.LocationID)) {
								MessageBoxResult result2 = TriggerMessageBox.Show(Window.GetWindow(this), "Cannot send this Secret Base because its location is already in use. Would you like to select a new location?", "Location in Use", MessageBoxButton.YesNo);
								if (result2 == MessageBoxResult.Yes) {
									byte? location = SecretBaseLocationChooser.Show(Window.GetWindow(this), secretBase.LocationID, sendToGameSave.SecretBaseManager, true);
									if (location.HasValue && location.Value != 0 && !sendToGameSave.SecretBaseManager.IsLocationInUse(location.Value)) {
										SharedSecretBase newSecretBase;
										if (secretBase.IsPlayerSecretBase)
											newSecretBase = new SharedSecretBase((PlayerSecretBase)secretBase, null);
										else
											newSecretBase = new SharedSecretBase((SharedSecretBase)secretBase, null);
										newSecretBase.SetNewLocation(location.Value);
										sendToGameSave.SecretBaseManager.AddSecretBase((SharedSecretBase)newSecretBase);
									}
								}
							}
							else {
								if (secretBase.IsPlayerSecretBase)
									sendToGameSave.SecretBaseManager.AddSecretBase((PlayerSecretBase)secretBase);
								else
									sendToGameSave.SecretBaseManager.AddSecretBase((SharedSecretBase)secretBase);
							}
						}
						else {
							TriggerMessageBox.Show(Window.GetWindow(this), "This game already has the maximum amount of 19 shared Secret Bases", "Can't Send");
						}
					}
				}
			}
			else {
				TriggerMessageBox.Show(Window.GetWindow(this), "You cannot send a Secret Base without a Pokémon Team", "Can't Send");
			}
		}

		private void OnEditTrainerClicked(object sender, RoutedEventArgs e) {
			bool result = SecretBaseEditTrainerWindow.Show(Window.GetWindow(this), (SharedSecretBase)secretBase, IsGBAGame);
			if (result) {
				UpdateListViewItems();
				OnSecretBaseSelected(null, null);
			}
		}

		private void OnExportClicked(object sender, RoutedEventArgs e) {
			SharedSecretBase finalSecretBase;
			if (secretBase.IsPlayerSecretBase)
				finalSecretBase = new SharedSecretBase((PlayerSecretBase)secretBase, null);
			else
				finalSecretBase = new SharedSecretBase((SharedSecretBase)secretBase, null);
			if (IsGBAGame && secretBase.IsPlayerSecretBase) {
				MessageBoxResult result2 = TriggerMessageBox.Show(Window.GetWindow(this), "Would you like to edit your Secret Base's trainer before exporting?", "Edit Trainer", MessageBoxButton.YesNo);
				if (result2 == MessageBoxResult.Yes) {
					finalSecretBase = new SharedSecretBase((PlayerSecretBase)secretBase, null);

					SecretBaseEditTrainerWindow.Show(Window.GetWindow(this), finalSecretBase, true);
				}
			}
			SaveFileDialog fileDialog = new SaveFileDialog();
			fileDialog.Title = "Export Secret Base";
			fileDialog.AddExtension = true;
			fileDialog.DefaultExt = ".scrtb";
			fileDialog.FileName = finalSecretBase.TrainerName + "'s Base";
			fileDialog.Filter = "Secret Base Files (*.scrtb)|*.scrtb|All Files (*.*)|*.*";
			var result = fileDialog.ShowDialog(Window.GetWindow(this));
			if (result.HasValue && result.Value) {
				string filePath = fileDialog.FileName;
				filePath = System.IO.Path.GetFullPath(filePath);

				try {
					File.WriteAllBytes(filePath, finalSecretBase.GetFinalData());
				}
				catch (Exception ex) {
					MessageBoxResult result2 = TriggerMessageBox.Show(Window.GetWindow(this), "Error saving Secret Base file. Would you like to see the error?", "Export Error", MessageBoxButton.YesNo);
					if (result2 == MessageBoxResult.Yes)
						ErrorMessageBox.Show(ex);
				}
			}
		}

		private void OnImportClicked(object sender, RoutedEventArgs e) {
			if (!IsGBAGame || GBAGameSave.SecretBaseManager.SharedSecretBases.Count < 19) {
				OpenFileDialog fileDialog = new OpenFileDialog();
				fileDialog.Title = "Import Secret Base";
				fileDialog.Filter = "Secret Base Files (*.scrtb)|*.scrtb|All Files (*.*)|*.*";
				var result = fileDialog.ShowDialog(Window.GetWindow(this));
				if (result.HasValue && result.Value) {
					string filePath = fileDialog.FileName;
					filePath = System.IO.Path.GetFullPath(filePath);

					try {
						FileInfo fileInfo = new FileInfo(filePath);
						if (fileInfo.Length != 160) {
							TriggerMessageBox.Show(Window.GetWindow(this), "Cannot import Secret Base. A Secret Base file must be 160 bytes in size", "Import Error");
						}
						else {
							SharedSecretBase newSecretBase = new SharedSecretBase(File.ReadAllBytes(filePath), null);
							foreach (PlacedDecoration decoration in newSecretBase.PlacedDecorations) {
								if (decoration.DecorationData == null)
									throw new Exception("Invalid Decoration ID in Secret Base. ID=" + decoration.ID);
							}
							if (IsGBAGame) {
								bool added = false;
								if (GBAGameSave.SecretBaseManager.IsLocationInUse(newSecretBase.LocationID)) {
									MessageBoxResult result2 = TriggerMessageBox.Show(Window.GetWindow(this), "Cannot import this Secret Base because its location is already in use. Would you like to select a new location?", "Location in Use", MessageBoxButton.YesNo);
									if (result2 == MessageBoxResult.Yes) {
										byte? location = SecretBaseLocationChooser.Show(Window.GetWindow(this), newSecretBase.LocationID, GBAGameSave.SecretBaseManager, true);
										if (location.HasValue && location.Value != 0 && !GBAGameSave.SecretBaseManager.IsLocationInUse(location.Value)) {
											newSecretBase.SetNewLocation(location.Value);
											added = true;
										}
									}
								}
								else {
									added = true;
								}
								if (added && newSecretBase.PokemonTeam.Count == 0) {
									MessageBoxResult result3 = TriggerMessageBox.Show(Window.GetWindow(this), "This Secret Base has no Pokemon in its Team. You will need to add at least one Pokémon in order to import this Secret Base. Would you like to continue?", "No Team", MessageBoxButton.YesNo);
									if (result3 == MessageBoxResult.Yes) {
										added = SecretBaseEditTrainerWindow.Show(Window.GetWindow(this), newSecretBase, false);
									}
								}
								if (added) {
									newSecretBase = GBAGameSave.SecretBaseManager.AddSecretBase(newSecretBase);
									AddListViewItems();
									listViewSecretBases.SelectedIndex = 2 + GBAGameSave.SecretBaseManager.SharedSecretBases.IndexOf(newSecretBase);
									/*for (int i = 0; i < GBAGameSave.SecretBaseManager.SharedSecretBases.Count; i++) {
										if (GBAGameSave.SecretBaseManager.SharedSecretBases[i] == newSecretBase) {
											listViewSecretBases.SelectedIndex = i + 2;
											break;
										}
									}*/
								}
							}
							else {
								newSecretBase = PokeManager.AddSecretBase(newSecretBase);
								AddListViewItems();
								listViewSecretBases.SelectedIndex = PokeManager.SecretBases.IndexOf(newSecretBase);
								/*for (int i = 0; i < PokeManager.SecretBases.Count; i++) {
									if (PokeManager.SecretBases[i] == newSecretBase) {
										 = i;
										break;
									}
								}*/
							}
						}
					}
					catch (Exception ex) {
						MessageBoxResult result2 = TriggerMessageBox.Show(Window.GetWindow(this), "Error reading Secret Base file. Would you like to see the error?", "Import Error", MessageBoxButton.YesNo);
						if (result2 == MessageBoxResult.Yes)
							ErrorMessageBox.Show(ex);
					}
				}
			}
			else {
				TriggerMessageBox.Show(Window.GetWindow(this), "This game already has the maximum amount of 19 shared Secret Bases", "Can't Import");
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			OnSecretBaseSelected(null, null);
		}
	}
}
