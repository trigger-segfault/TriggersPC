using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
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
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {

	public enum SendPokemonModes : byte {
		SendTo,
		SendFrom,
		GiveItem,
		SendMulti,
		SelectPokemon
	}


	public partial class SendPokemonToWindow : Window {

		private bool loaded2;
		private bool loaded;
		private IPokemon pokemon;
		private IPokeContainer container;
		private int containerIndex;
		private IPokePC pokePC;
		private int boxIndex;
		private bool partyMode;
		private int gameIndex;
		private SendPokemonModes mode;
		private Item giveItem;
		private int rowIndex;
		private bool noEggs;

		public SendPokemonToWindow(SendPokemonModes mode, int gameIndex, IPokemon pokemon, IPokeContainer container, int containerIndex, Item giveItem, bool noEggs = false) {
			InitializeComponent();

			loaded2 = false;
			loaded = false;
			this.pokemon = pokemon;
			this.pokeBoxControl.MouseMoveTarget = this;
			this.pokeBoxControlParty.MouseMoveTarget = this;
			this.pokeBoxControl.AddSlave(this.pokeBoxControlParty);
			this.pokeBoxControlParty.Visibility = Visibility.Hidden;
			this.mode = mode;
			this.giveItem = giveItem;
			this.gameIndex = -2;
			this.noEggs = noEggs;

			this.pokeBoxControl.PokemonViewer = pokemonViewer;
			this.pokeBoxControlParty.PokemonViewer = pokemonViewer;
			this.pokeBoxControl.PokemonSelected += OnPokemonSelected;
			this.pokeBoxControlParty.PokemonSelected += OnPokemonSelected;

			if (mode == SendPokemonModes.SendFrom) {
				this.container = container;
				this.containerIndex = containerIndex;
			}
			else if (mode == SendPokemonModes.GiveItem) {
				this.Title = "Give Item";
			}

			for (int i = -1; i < PokeManager.NumGameSaves; i++) {

				IGameSave game = PokeManager.GetGameSaveAt(i);
				if (i == gameIndex && (!(game is ManagerGameSave) || (game as ManagerGameSave).NumPokePCRows == 1)) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
				}
				if (pokemon != null && (mode != SendPokemonModes.GiveItem && (pokemon.IsEgg && (game.GameType == GameTypes.Colosseum || game.GameType == GameTypes.XD)))) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
				}
				else if (mode == SendPokemonModes.SendMulti && PokeManager.SelectionHasEgg && (game.GameType == GameTypes.Colosseum || game.GameType == GameTypes.XD)) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
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

			if (this.gameIndex != -2) {
				boxIndex = comboBoxGame.SelectedGameSave.PokePC.CurrentBox;
				pokePC = PokeManager.GetGameSaveAt(this.gameIndex).PokePC;

				if (pokePC.Party == null) {
					partyMode = false;
					buttonParty.Content = "Show Party";
					buttonParty.IsEnabled = false;
					pokeBoxControlParty.Visibility = Visibility.Hidden;
				}
				else {
					buttonParty.IsEnabled = true;
				}

				pokeBoxControl.LoadBox(pokePC[boxIndex], gameIndex);
			}
			GameChanged(null, null);
		}
		public static bool? ShowSendToDialog(Window owner, int gameIndex, IPokemon pokemon) {
			SendPokemonToWindow window = new SendPokemonToWindow(SendPokemonModes.SendTo, gameIndex, pokemon, null, 0, null);
			window.Owner = owner;
			return window.ShowDialog();
		}
		public static bool? ShowSendFromDialog(Window owner, int gameIndex, IPokeContainer container, int containerIndex) {
			SendPokemonToWindow window = new SendPokemonToWindow(SendPokemonModes.SendFrom, gameIndex, null, container, containerIndex, null);
			window.Owner = owner;
			return window.ShowDialog();
		}
		public static bool? ShowSendMultiDialog(Window owner, int gameIndex) {
			SendPokemonToWindow window = new SendPokemonToWindow(SendPokemonModes.SendMulti, gameIndex, null, null, 0, null);
			window.Owner = owner;
			return window.ShowDialog();
		}
		public static bool? ShowGiveItemDialog(Window owner, Item item) {
			SendPokemonToWindow window = new SendPokemonToWindow(SendPokemonModes.GiveItem, -2, null, null, 0, item);
			window.Owner = owner;
			return window.ShowDialog();
		}
		public static IPokemon ShowSelectDialog(Window owner, bool noEggs) {
			SendPokemonToWindow window = new SendPokemonToWindow(SendPokemonModes.SelectPokemon, -2, null, null, 0, null, noEggs);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result.HasValue && result.Value)
				return window.pokemon;
			return null;
		}

		private IPokeContainer PokeBox {
			get { return (partyMode ? (IPokeContainer)pokePC.Party : (IPokeContainer)pokePC[boxIndex]); }
		}

		private void OnPokemonSelected(object sender, PokemonSelectedEventArgs e) {
			if (mode == SendPokemonModes.GiveItem && ((PokeBoxControl)sender).IsPickupMode && e.Pokemon != null) {
				if (e.Pokemon.IsEgg) {
					TriggerMessageBox.Show(this, "Cannot give items to Eggs", "Can't Hold");
				}
				else if (e.Pokemon.HeldItemID != 0 && e.Pokemon.HeldItemData.ID == 0) {
					TriggerMessageBox.Show(this, "Cannot switch " + e.Pokemon.Nickname + "'s item because it is an Unknown Item", "Unknown Item");
				}
				else if (e.Pokemon.HeldItemID == giveItem.ID) {
					TriggerMessageBox.Show(this, e.Pokemon.Nickname + " is already holding " + e.Pokemon.HeldItemData.Name, "Already Holding");
				}
				else {
					MessageBoxResult result = MessageBoxResult.Yes;
					if (e.Pokemon.IsHoldingMail) {
						result = MessageBoxResult.No;
						TriggerMessageBox.Show(this, "Cannot give " + giveItem.ItemData.Name + " to this Pokémon because it is holding mail. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Give Item");
					}
					else if (e.Pokemon.IsHoldingItem) {
						result = TriggerMessageBox.Show(this, e.Pokemon.Nickname + " is already holding " + e.Pokemon.HeldItemData.Name + ", would you like to switch it with " + giveItem.ItemData.Name + "?", "Switch Items", MessageBoxButton.YesNo);
						if (result == MessageBoxResult.Yes)
							PokeManager.ManagerGameSave.Inventory.Items[e.Pokemon.HeldItemData.PocketType].AddItem(e.Pokemon.HeldItemID, 1);
					}
					if (result == MessageBoxResult.Yes) {
						e.Pokemon.HeldItemID = giveItem.ID;
						giveItem.Pocket.TossItemAt(giveItem.Pocket.IndexOf(giveItem), 1);
						TriggerMessageBox.Show(Window.GetWindow(this), "Gave " + giveItem.ItemData.Name + " to " + e.Pokemon.Nickname, "Gave Item");
						PokeManager.RefreshUI();
						PokeManager.LastGameInDialogIndex = gameIndex;
						DialogResult = true;
					}
				}
			}
			else if (mode == SendPokemonModes.SendTo && e.Pokemon == null) {
				if (!PokeManager.CanSafelyPlaceHeldUnknownItem(e.PokeContainer)) {
					MessageBoxResult unknownItemResult = TriggerMessageBox.Show(Window.GetWindow(this), "The Pokémon that you are trying to send is holding an Unknown Item. Sending it to a different game may cause problems. Are you sure you want to send it?", "Unknown Item", MessageBoxButton.YesNo);
					if (unknownItemResult == MessageBoxResult.No)
						return;
				}
				PokeManager.PlacePokemon(e.PokeContainer, e.Index);
				PokeManager.LastGameInDialogIndex = gameIndex;
				DialogResult = true;
			}
			else if (mode == SendPokemonModes.SendFrom && e.Pokemon != null) {
				if (e.Pokemon.HeldItemID != 0 && e.Pokemon.HeldItemData.ID == 0 && e.Pokemon.GameSave != container.GameSave) {
					MessageBoxResult unknownItemResult = TriggerMessageBox.Show(Window.GetWindow(this), "That Pokémon is holding an Unknown Item. Sending it to a different game may cause problems. Are you sure you want to send it?", "Unknown Item", MessageBoxButton.YesNo);
					if (unknownItemResult == MessageBoxResult.No)
						return;
				}
				if (PokeManager.CanPickupPokemon(e.Pokemon)) {
					PokeManager.PickupPokemon(e.Pokemon, null);
					PokeManager.PlacePokemon(container, containerIndex);
					PokeManager.LastGameInDialogIndex = gameIndex;
					DialogResult = true;
				}
				else if (PokeManager.IsPartyHoldingMail(e.PokeContainer)) {
					TriggerMessageBox.Show(Window.GetWindow(this), "Cannot send that Pokémon. A Pokémon in your party is holding mail. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Send");
				}
				else {
					TriggerMessageBox.Show(Window.GetWindow(this), "Cannot send that Pokémon. It is the last valid Pokémon in your party", "Can't Send");
				}
			}
			else if (mode == SendPokemonModes.SendMulti && e.Pokemon == null) {
				if (!PokeManager.CanSafelyPlaceHeldUnknownItem(e.PokeContainer)) {
					MessageBoxResult unknownItemResult = TriggerMessageBox.Show(Window.GetWindow(this), "A Pokémon that you are trying to send is holding an Unknown Item. Sending it to a different game may cause problems. Are you sure you want to send it?", "Unknown Item", MessageBoxButton.YesNo);
					if (unknownItemResult == MessageBoxResult.No)
						return;
				}
				if (e.PokeContainer.PokePC.HasRoomForPokemon(PokeManager.NumSelectedPokemon)) {
					PokeManager.PlaceSelection(e.PokeContainer, e.Index);
					PokeManager.ClearSelectedPokemon();
					PokeManager.LastGameInDialogIndex = gameIndex;
					DialogResult = true;
				}
				else {
					TriggerMessageBox.Show(Window.GetWindow(this), "Not enough room in game to store all selected Pokémon", "No Room");
				}
			}
			else if (mode == SendPokemonModes.SelectPokemon && e.Pokemon != null && ((PokeBoxControl)sender).IsPickupMode && (!e.Pokemon.IsEgg || !noEggs) && !e.Pokemon.IsShadowPokemon) {
				pokemon = e.Pokemon;
				DialogResult = true;
			}
		}

		private void GameChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			gameIndex = comboBoxGame.SelectedGameIndex;
			rowIndex = 0;
			if (gameIndex == -2) {
				pokeBoxControl.UnloadBox();
			}
			else {
				if (comboBoxGame.SelectedGameSave is ManagerGameSave) {
					buttonParty.Visibility = Visibility.Hidden;
					comboBoxRows.Visibility = Visibility.Visible;
				}
				else {
					buttonParty.Visibility = Visibility.Visible;
					comboBoxRows.Visibility = Visibility.Hidden;
				}
				pokePC = PokeManager.GetGameSaveAt(this.gameIndex).PokePC;
				boxIndex = pokePC.CurrentBox;

				if (pokePC.Party == null) {
					partyMode = false;
					buttonParty.Content = "Show Party";
					buttonParty.IsEnabled = false;
					pokeBoxControlParty.Visibility = Visibility.Hidden;
				}
				else {
					buttonParty.IsEnabled = true;
				}

				pokeBoxControl.LoadBox(pokePC[boxIndex], gameIndex);
				if (partyMode)
					pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
			}
		}

		private void OnPreviousBoxButtonClicked(object sender, RoutedEventArgs e) {
			pokePC.CurrentBox = boxIndex - 1;
			boxIndex = pokePC.CurrentBox;
			pokeBoxControl.LoadBox(pokePC[boxIndex], gameIndex);
			if (partyMode)
				pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
		}

		private void OnNextBoxButtonClicked(object sender, RoutedEventArgs e) {
			pokePC.CurrentBox = boxIndex + 1;
			boxIndex = pokePC.CurrentBox;
			pokeBoxControl.LoadBox(pokePC[boxIndex], gameIndex);
			if (partyMode)
				pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
		}
		private void OnPartyButtonClicked(object sender, RoutedEventArgs e) {
			partyMode = !partyMode;
			buttonParty.Content = (partyMode ? "Hide Party" : "Show Party");

			pokeBoxControlParty.Visibility = (partyMode ? Visibility.Visible : Visibility.Hidden);
			pokeBoxControl.LoadBox(pokePC[boxIndex], gameIndex);
			if (partyMode)
				pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
		}

		private void OnBoxMovementKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.A || e.Key == Key.Left)
				OnPreviousBoxButtonClicked(null, null);
			else if (e.Key == Key.D || e.Key == Key.Right)
				OnNextBoxButtonClicked(null, null);
			else if (pokePC.Party != null && !partyMode && (e.Key == Key.S || e.Key == Key.Down))
				OnPartyButtonClicked(null, null);
			else if (pokePC.Party != null && partyMode && (e.Key == Key.W || e.Key == Key.Up))
				OnPartyButtonClicked(null, null);
		}

		private void AfterLoaded(object sender, MouseEventArgs e) {
			if (!loaded2) {
				if (mode == SendPokemonModes.SendFrom) {
					pokeBoxControl.Mode = PokeBoxControlModes.Custom;
					pokeBoxControl.CanChangePickupMode = true;
					pokeBoxControl.IsPickupMode = true;
					pokeBoxControl.IsSummaryMode = true;
				}
				else if (mode == SendPokemonModes.SendTo) {
					pokeBoxControl.Mode = PokeBoxControlModes.Custom;
					pokeBoxControl.CanChangePickupMode = true;
					pokeBoxControl.IsPickupMode = true;
					pokeBoxControl.IsSummaryMode = true;
					PokeManager.PickupPokemon(pokemon, pokeBoxControl);
				}
				else if (mode == SendPokemonModes.GiveItem) {
					pokeBoxControl.Mode = PokeBoxControlModes.Custom;
					pokeBoxControl.CanChangePickupMode = true;
					pokeBoxControl.IsPickupMode = true;
					pokeBoxControl.IsSummaryMode = true;
				}
				else if (mode == SendPokemonModes.SendMulti) {
					pokeBoxControl.Mode = PokeBoxControlModes.Custom;
					pokeBoxControl.CanChangePickupMode = true;
					pokeBoxControl.IsPickupMode = true;
					pokeBoxControl.IsSummaryMode = true;
					PokeManager.PickupSelection(pokeBoxControl);
				}
				else if (mode == SendPokemonModes.SelectPokemon) {
					pokeBoxControl.Mode = PokeBoxControlModes.Custom;
					pokeBoxControl.CanChangePickupMode = true;
					pokeBoxControl.IsPickupMode = true;
					pokeBoxControl.IsSummaryMode = true;
					PokeManager.PickupSelection(pokeBoxControl);
				}
				loaded2 = true;
			}
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			if (comboBoxGame.SelectedGameIndex == -2) {
				TriggerMessageBox.Show(this, "No available games to send with", "Can't Send");
				Close();
			}
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			PokeManager.DropSelection();
			PokeManager.DropPokemon();
		}

		private void OnRowSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (comboBoxRows.SelectedIndex != -1) {
				rowIndex = comboBoxRows.SelectedIndex;
				pokePC = PokeManager.ManagerGameSave.GetPokePCRow(rowIndex);
				boxIndex = pokePC.CurrentBox;

				pokeBoxControl.LoadBox(pokePC[boxIndex], gameIndex);
			}
		}
	}
}
