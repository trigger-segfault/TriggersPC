using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for PokemonTab.xaml
	/// </summary>
	public partial class PokemonTab : UserControl {

		private int visibleBoxViewers;
		private IGameSave gameSave;

		private int GameIndex {
			get { return gameSave.GameIndex; }
		}

		public PokemonTab() {
			InitializeComponent();

			boxViewer1.AddSlave(boxViewer2);
			boxViewer1.AddSlave(boxViewer3);
			boxViewer1.SetAsMaster();
			boxViewer1.SetFocus();
			boxViewer1.LoadUI(-1, 0);
			boxViewer2.LoadUI(-1, 0);
			boxViewer3.LoadUI(-1, 0);
			boxViewer1.PokemonViewer = pokemonViewer;
			boxViewer2.PokemonViewer = pokemonViewer;
			boxViewer3.PokemonViewer = pokemonViewer;

			boxViewer2.Visibility = Visibility.Collapsed;
			boxViewer3.Visibility = Visibility.Collapsed;

			boxViewer1.PokemonTab = this;
			boxViewer2.PokemonTab = this;
			boxViewer3.PokemonTab = this;

			visibleBoxViewers = 1;

			pokemonViewer.UnloadPokemon();
		}

		public void RefreshPokemonViewer() {
			pokemonViewer.RefreshUI();
		}

		public void LoadGame(IGameSave gameSave) {
			this.gameSave = gameSave;
			boxViewer1.LoadUI(gameSave.GameIndex, 0);

			OnGameSwitch(boxViewer1);
			UpdateBoxViewerRows();
			RefreshStoredPokemon();
		}

		public void Reload() {
			pokemonViewer.UnloadPokemon();

			boxViewer1.loaded = false;
			boxViewer2.loaded = false;
			boxViewer3.loaded = false;
			boxViewer1.ComboBoxGames.ReloadGameSaves();
			boxViewer2.ComboBoxGames.ReloadGameSaves();
			boxViewer3.ComboBoxGames.ReloadGameSaves();
			boxViewer1.ComboBoxRows.ReloadRows();
			boxViewer2.ComboBoxRows.ReloadRows();
			boxViewer3.ComboBoxRows.ReloadRows();
			boxViewer1.loaded = true;
			boxViewer2.loaded = true;
			boxViewer3.loaded = true;
			boxViewer1.LoadUI(-1, 0);
			boxViewer2.LoadUI(-1, 0);
			boxViewer3.LoadUI(-1, 0);

			//OnGameSwitch(boxViewer1);

			UpdateBoxViewerRows();
			RefreshStoredPokemon();
		}

		public void RefreshUI() {
			boxViewer1.RefreshUI();
			boxViewer2.RefreshUI();
			boxViewer3.RefreshUI();
			pokemonViewer.RefreshUI();
			RefreshStoredPokemon();
			UpdateBoxViewerRows();
		}

		public void ReloadGames() {
			boxViewer1.loaded = false;
			boxViewer2.loaded = false;
			boxViewer3.loaded = false;
			boxViewer1.ComboBoxGames.ReloadGameSaves();
			boxViewer2.ComboBoxGames.ReloadGameSaves();
			boxViewer3.ComboBoxGames.ReloadGameSaves();
			boxViewer1.ComboBoxRows.ReloadRows();
			boxViewer2.ComboBoxRows.ReloadRows();
			boxViewer3.ComboBoxRows.ReloadRows();
			boxViewer1.loaded = true;
			boxViewer2.loaded = true;
			boxViewer3.loaded = true;
			boxViewer1.LoadUI(-1, 0);
			boxViewer2.LoadUI(-1, 0);
			boxViewer3.LoadUI(-1, 0);

			//OnGameSwitch(boxViewer1);

			UpdateBoxViewerRows();
		}

		private void FindNextRow(PokemonBoxViewer boxViewer, int invalidGameIndex1, int invalidRow1, int invalidGameIndex2, int invalidRow2) {

			int index = boxViewer.ComboBoxGames.SelectedGameIndex;
			int row = boxViewer.ComboBoxRows.SelectedIndex;
			if (PokeManager.GetGameSaveAt(index) is ManagerGameSave) {
				if ((index == invalidGameIndex1 && row == invalidRow1) || (index == invalidGameIndex2 && row == invalidRow2)) {
					for (int j = 0; j < (PokeManager.GetGameSaveAt(index) as ManagerGameSave).NumPokePCRows; j++) {
						if ((index != invalidGameIndex1 || j != invalidRow1) && (index != invalidGameIndex2 || j != invalidRow2)) {
							//boxViewer.LoadGame(boxViewer.ComboBoxGames.SelectedGameSave, pokemonViewer, boxViewer.ComboBoxGames.SelectedGameIndex, j);
							boxViewer.ComboBoxRows.SelectedIndex = j;
							break;
						}
					}
				}
			}
		}

		private void FindNextGameIndex(PokemonBoxViewer boxViewer, int invalidGameIndex1, int invalidRow1, int invalidGameIndex2, int invalidRow2) {
			// Check if we don't need to change things
			if (boxViewer.ComboBoxGames.SelectedGameIndex != -2) {
				int index = boxViewer.ComboBoxGames.SelectedGameIndex;
				int row = boxViewer.ComboBoxRows.SelectedIndex;
				if (boxViewer.ComboBoxGames.SelectedGameSave is ManagerGameSave) {
					if ((index != invalidGameIndex1 || row != invalidRow1) && (index != invalidGameIndex2 || row != invalidRow2)) {
						return;
					}
					else {
						for (int j = 0; j < (PokeManager.GetGameSaveAt(index) as ManagerGameSave).NumPokePCRows; j++) {
							if ((index != invalidGameIndex1 || j != invalidRow1) && (index != invalidGameIndex2 || j != invalidRow2)) {
								boxViewer.ComboBoxGames.SelectedGameIndex = index;
								boxViewer.comboBoxRows.SelectedIndex = j;
								break;
								//boxViewer.LoadGame(PokeManager.GetGameSaveAt(i), pokemonViewer, i, j);
							}
						}
					}
				}
				else if (index != invalidGameIndex1 && index != invalidGameIndex2) {
					return;
				}
			}
			bool finished = false;
			for (int i = -1; i < PokeManager.NumGameSaves && !finished; i++) {
				if (PokeManager.GetGameSaveAt(i) is ManagerGameSave) {
					for (int j = 0; j < (PokeManager.GetGameSaveAt(i) as ManagerGameSave).NumPokePCRows; j++) {
						if ((i != invalidGameIndex1 || j != invalidRow1) && (i != invalidGameIndex2 || j != invalidRow2)) {
							boxViewer.ComboBoxGames.SelectedGameIndex = i;
							boxViewer.comboBoxRows.SelectedIndex = j;
							finished = true;
							break;
							//boxViewer.LoadGame(PokeManager.GetGameSaveAt(i), pokemonViewer, i, j);
						}
					}
				}
				else if (i != invalidGameIndex1 && i != invalidGameIndex2) {
					//boxViewer.LoadGame(PokeManager.GetGameSaveAt(i), pokemonViewer, i);
					boxViewer.ComboBoxGames.SelectedGameIndex = i;
					break;
				}
			}
		}

		public int GetNumRows(int gameIndex) {
			if (PokeManager.GetGameSaveAt(gameIndex) is ManagerGameSave) {
				return (PokeManager.GetGameSaveAt(gameIndex) as ManagerGameSave).NumPokePCRows;
			}
			return 1;
		}
		public int GetNumRows(PokemonBoxViewer viewer) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(GetGameIndex(viewer));
			if (gameSave is ManagerGameSave) {
				return (gameSave as ManagerGameSave).NumPokePCRows;
			}
			return 1;
		}
		public int GetIndex(PokemonBoxViewer viewer) {
			if (viewer == boxViewer1) return 1;
			else if (viewer == boxViewer2) return 2;
			else return 3;
		}
		public int GetGameIndex(PokemonBoxViewer viewer) {
			return (viewer == boxViewer1 ? PokeManager.ManagerWindow.GameIndex : viewer.ComboBoxGames.SelectedGameIndex);
		}
		public int GetRow(PokemonBoxViewer viewer) {
			return Math.Min(GetNumRows(viewer) - 1, viewer.ComboBoxRows.SelectedIndex);
		}
		public bool IsValid(int gameIndex, int row, PokemonBoxViewer check1, PokemonBoxViewer check2 = null) {
			return (gameIndex != GetGameIndex(check1) || row != GetRow(check1)) &&
					(check2 == null || gameIndex != GetGameIndex(check2) || row != GetRow(check2));
		}
		public bool IsValid(PokemonBoxViewer viewer, PokemonBoxViewer check1, PokemonBoxViewer check2 = null) {
			return (GetGameIndex(viewer) != GetGameIndex(check1) || GetRow(viewer) != GetRow(check1)) &&
					(check2 == null || GetGameIndex(viewer) != GetGameIndex(check2) || GetRow(viewer) != GetRow(check2));
		}

		public void ReloadRowComboBoxes() {
			boxViewer1.loaded = false;
			boxViewer2.loaded = false;
			boxViewer3.loaded = false;
			boxViewer1.ComboBoxRows.ReloadRows();
			boxViewer2.ComboBoxRows.ReloadRows();
			boxViewer3.ComboBoxRows.ReloadRows();
			boxViewer1.loaded = true;
			boxViewer2.loaded = true;
			boxViewer3.loaded = true;
			boxViewer1.LoadUI(-2, 0);
			boxViewer2.LoadUI(-2, 0);
			boxViewer3.LoadUI(-2, 0);

			OnGameSwitch(boxViewer1);

			UpdateBoxViewerRows();
			RefreshStoredPokemon();
		}

		public void FindAvailableRow(PokemonBoxViewer viewer, PokemonBoxViewer check1, PokemonBoxViewer check2 = null) {
			if (!IsValid(viewer, check1, check2)) {
				if (GetNumRows(viewer) > 1) {
					for (int newRow = 0; newRow < GetNumRows(viewer); newRow++) {
						if (IsValid(GetGameIndex(viewer), newRow, check1, check2)) {
							//viewer.ComboBoxRows.SelectedIndex = newRow;
							viewer.LoadUI(-2, newRow);
							return;
						}
					}
				}
			}
		}
		public void MakeValid(PokemonBoxViewer viewer, PokemonBoxViewer check1, PokemonBoxViewer check2 = null) {
			if (!IsValid(viewer, check1, check2)) {
				if (GetNumRows(viewer) > 1) {
					for (int newRow = 0; newRow < GetNumRows(viewer); newRow++) {
						if (IsValid(GetGameIndex(viewer), newRow, check1, check2)) {
							//viewer.ComboBoxRows.SelectedIndex = newRow;
							viewer.LoadUI(-2, newRow);
							return;
						}
					}
				}
				for (int newGameIndex = -1; newGameIndex < PokeManager.NumGameSaves; newGameIndex++) {
					if (GetNumRows(newGameIndex) > 1) {
						for (int newRow = 0; newRow < GetNumRows(newGameIndex); newRow++) {
							if (IsValid(newGameIndex, newRow, check1, check2)) {
								//viewer.ComboBoxGames.SelectedGameIndex = newGameIndex;
								//viewer.ComboBoxRows.SelectedIndex = newRow;
								viewer.LoadUI(newGameIndex, newRow);
								return;
							}
						}
					}
					else if (IsValid(newGameIndex, 0, check1, check2)) {
						//viewer.ComboBoxGames.SelectedGameIndex = newGameIndex;
						viewer.LoadUI(newGameIndex);
						return;
					}
				}
			}
		}

		public void OnGameSwitch(PokemonBoxViewer caller = null) {
			if (PokeManager.IsHoldingPokemon || PokeManager.HasSelection) {
				PokeManager.DropAll();
				PokeManager.ClearSelectedPokemon();
				PokeManager.RefreshUI();
			}

			if (!PokeManager.Settings.AllowDoubleBoxRows) {
				if (caller == null)
					caller = boxViewer1;
				int callerIndex = GetIndex(caller);
				PokemonBoxViewer[] priorities = new PokemonBoxViewer[3];
				boxViewer1.SupressIndexChanged = true;
				boxViewer2.SupressIndexChanged = true;
				boxViewer3.SupressIndexChanged = true;

				priorities[0] = boxViewer1;
				if (callerIndex == 1) {
					if (IsValid(boxViewer2, caller) || !IsValid(boxViewer3, caller)) {
						priorities[1] = boxViewer2;
						priorities[2] = boxViewer3;
					}
					else {
						priorities[1] = boxViewer3;
						priorities[2] = boxViewer2;
					}

					boxViewer2.ComboBoxGames.ResetGameSaveVisibility();
					boxViewer3.ComboBoxGames.ResetGameSaveVisibility();
					if (GetNumRows(caller) == 1) {
						boxViewer2.ComboBoxGames.SetGameSaveVisible(GameIndex, false);
						boxViewer3.ComboBoxGames.SetGameSaveVisible(GameIndex, false);
					}
				}
				else if (callerIndex == 2) {
					if (GetNumRows(caller) > 2) {
						priorities[1] = boxViewer3;
						priorities[2] = boxViewer2;
					}
					else {
						priorities[1] = boxViewer2;
						priorities[2] = boxViewer3;
					}
					//FindAvailableRow(priorities[1], priorities[1]);
				}
				else {
					if (GetNumRows(caller) > 2) {
						priorities[1] = boxViewer2;
						priorities[2] = boxViewer3;
					}
					else {
						priorities[1] = boxViewer3;
						priorities[2] = boxViewer2;
					}
					//FindAvailableRow(priorities[1], priorities[1]);
				}

				MakeValid(priorities[1], priorities[0]);
				MakeValid(priorities[2], priorities[0], priorities[1]);

				boxViewer1.SupressIndexChanged = false;
				boxViewer2.SupressIndexChanged = false;
				boxViewer3.SupressIndexChanged = false;
			}
			else {
				boxViewer2.ComboBoxGames.ResetGameSaveVisibility();
				boxViewer3.ComboBoxGames.ResetGameSaveVisibility();
			}
		}

		public void OnRowSwitch(PokemonBoxViewer caller) {
			if (PokeManager.IsHoldingPokemon || PokeManager.HasSelection) {
				PokeManager.DropAll();
				PokeManager.ClearSelectedPokemon();
				PokeManager.RefreshUI();
			}

			if (!PokeManager.Settings.AllowDoubleBoxRows) {
				int callerIndex = GetIndex(caller);
				PokemonBoxViewer[] priorities = new PokemonBoxViewer[3];
				boxViewer1.SupressIndexChanged = true;
				boxViewer2.SupressIndexChanged = true;
				boxViewer3.SupressIndexChanged = true;

				priorities[0] = caller;
				if (callerIndex == 1) {
					if (IsValid(boxViewer2, caller) || !IsValid(boxViewer3, caller)) {
						priorities[1] = boxViewer2;
						priorities[2] = boxViewer3;
					}
					else {
						priorities[1] = boxViewer3;
						priorities[2] = boxViewer2;
					}
				}
				else if (callerIndex == 2) {
					priorities[1] = boxViewer1;
					priorities[2] = boxViewer3;
				}
				else {
					priorities[1] = boxViewer1;
					priorities[2] = boxViewer2;
				}

				MakeValid(priorities[1], priorities[0]);
				MakeValid(priorities[2], priorities[0], priorities[1]);

				boxViewer1.SupressIndexChanged = false;
				boxViewer2.SupressIndexChanged = false;
				boxViewer3.SupressIndexChanged = false;
			}
			else {

			}
		}

		public void RefreshSelectedGameSaves(PokemonBoxViewer refresher = null, bool rowOnly = false) {
			int refreshIndex = 1;
			if (refresher == boxViewer2) refreshIndex = 2;
			if (refresher == boxViewer3) refreshIndex = 3;

			boxViewer2.SupressIndexChanged = true;
			boxViewer3.SupressIndexChanged = true;

			if (refreshIndex == 1) {
				boxViewer2.ComboBoxGames.ResetGameSaveVisibility();
				boxViewer3.ComboBoxGames.ResetGameSaveVisibility();
				
				FindNextGameIndex(boxViewer2, GameIndex, boxViewer1.comboBoxRows.SelectedIndex, boxViewer3.ComboBoxGames.SelectedGameIndex, boxViewer3.comboBoxRows.SelectedIndex);
				FindNextGameIndex(boxViewer3, GameIndex, boxViewer1.comboBoxRows.SelectedIndex, boxViewer2.ComboBoxGames.SelectedGameIndex, boxViewer2.comboBoxRows.SelectedIndex);

				if (!(gameSave is ManagerGameSave) || (gameSave as ManagerGameSave).NumPokePCRows == 1) {
					boxViewer2.ComboBoxGames.SetGameSaveVisible(GameIndex, false);
					boxViewer3.ComboBoxGames.SetGameSaveVisible(GameIndex, false);
				}
			}
			else if (refreshIndex == 2) {
				if (rowOnly) {
					FindNextRow(boxViewer1, refresher.ComboBoxGames.SelectedGameIndex, refresher.comboBoxRows.SelectedIndex, boxViewer3.ComboBoxGames.SelectedGameIndex, boxViewer3.comboBoxRows.SelectedIndex);
				}
				else {
					FindNextRow(refresher, GameIndex, boxViewer1.comboBoxRows.SelectedIndex, -2, -1);
				}
				FindNextGameIndex(boxViewer3, GameIndex, boxViewer1.comboBoxRows.SelectedIndex, boxViewer2.ComboBoxGames.SelectedGameIndex, boxViewer2.comboBoxRows.SelectedIndex);
			}
			else if (refreshIndex == 3) {
				if (rowOnly) {
					FindNextRow(boxViewer1, refresher.ComboBoxGames.SelectedGameIndex, refresher.comboBoxRows.SelectedIndex, -2, -1);
				}
				else {
					FindNextRow(refresher, GameIndex, boxViewer1.comboBoxRows.SelectedIndex, boxViewer2.ComboBoxGames.SelectedGameIndex, boxViewer2.comboBoxRows.SelectedIndex);
				}
				FindNextGameIndex(boxViewer2, GameIndex, boxViewer1.comboBoxRows.SelectedIndex, boxViewer3.ComboBoxGames.SelectedGameIndex, boxViewer3.comboBoxRows.SelectedIndex);
			}

			boxViewer2.SupressIndexChanged = false;
			boxViewer3.SupressIndexChanged = false;
		}

		public void FinishActions() {
			boxViewer1.FinishActions();
			boxViewer2.FinishActions();
			boxViewer3.FinishActions();
		}
		public void RefreshGameSaves() {
			boxViewer1.RefreshGameSaves();
			boxViewer2.RefreshGameSaves();
			boxViewer3.RefreshGameSaves();
		}

		public void RefreshStoredPokemon() {
			int count = 0;
			if (gameSave is ManagerGameSave) {
				for (int i = 0; i < (gameSave as ManagerGameSave).NumPokePCRows; i++) {
					foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(i)) {
						count++;
					}
				}
			}
			else {
				foreach (IPokemon pokemon in gameSave.PokePC) {
					count++;
				}
			}
			labelStoredPokemon.Content = "Pokémon   " + count.ToString();
		}

		private int NumSupportedBoxViewers {
			get { return Math.Min(PokeManager.Settings.NumBoxRows, PokeManager.NumGameSaves + PokeManager.ManagerGameSave.NumPokePCRows); }
		}

		private void UpdateBoxViewerRows(double newHeight = -1) {
			if (newHeight == -1)
				newHeight = gridBoxViewers.ActualHeight;
			int newVisibleBoxViewers = Math.Max(1, (int)newHeight / 181);

			if (NumSupportedBoxViewers >= 2 && newVisibleBoxViewers >= 2) {
				if (visibleBoxViewers < 2) {
					boxViewer2.Visibility = Visibility.Visible;
					boxViewer2.UpdateSlavesWidth();
				}

				if (NumSupportedBoxViewers >= 3 && newVisibleBoxViewers >= 3) {
					if (visibleBoxViewers < 3) {
						boxViewer3.Visibility = Visibility.Visible;
						boxViewer3.UpdateSlavesWidth();
					}
				}
				else {
					newVisibleBoxViewers = 2;
					if (visibleBoxViewers >= 3)
						boxViewer3.Visibility = Visibility.Collapsed;
				}
			}
			else {
				newVisibleBoxViewers = 1;
				if (visibleBoxViewers >= 2)
					boxViewer2.Visibility = Visibility.Collapsed;
				if (visibleBoxViewers >= 3)
					boxViewer3.Visibility = Visibility.Collapsed;
			}

			visibleBoxViewers = newVisibleBoxViewers;
			if ((boxViewer2.HasFocus && visibleBoxViewers < 2) || (boxViewer3.HasFocus && visibleBoxViewers < 3))
				boxViewer1.SetFocus();
		}

		private void OnBoxViewerGridSizeChanged(object sender, SizeChangedEventArgs e) {
			UpdateBoxViewerRows(e.NewSize.Height);
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			UpdateBoxViewerRows();
		}

		public void GotoPokemon(IPokemon pokemon) {
			if (pokemon.GameSave is ManagerGameSave) {
				for (int i = 0; i < (pokemon.GameSave as ManagerGameSave).NumPokePCRows; i++) {
					if ((pokemon.GameSave as ManagerGameSave).GetPokePCRow(i) == pokemon.PokePC) {
						boxViewer1.LoadUI(pokemon.GameSave.GameIndex, i);
						break;
					}
				}
			}
			boxViewer1.GotoPokemon(pokemon);
		}
	}
}
