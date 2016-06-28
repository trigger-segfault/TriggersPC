using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WPF.JoshSmith.Adorners;
using WPF.JoshSmith.Controls.Utilities;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for PokemonBoxViewer.xaml
	/// </summary>
	public partial class PokemonBoxViewer : UserControl {

		private IPokePC pokePC;
		private int boxIndex;
		private int rowIndex;
		private ContainerTypes containerMode;
		private PokemonViewer pokemonViewer;
		private List<PokeBoxControl> slaves;
		private int boxesVisible;
		private int gameIndex;
		private List<PokemonBoxViewer> boxSlaves;
		private PokemonBoxViewer master;
		private PokemonBoxViewer focus;
		private PokemonTab pokemonTab;
		private bool supressGameChanged;
		internal bool loaded;
		private IPokemon gotoPokemon;

		public PokemonBoxViewer() {
			InitializeComponent();

			this.boxesVisible = 1;
			this.slaves = new List<PokeBoxControl>();
			this.pokeBoxControlParty.MouseMoveTarget = this;
			this.pokeBoxControlParty.Visibility = Visibility.Hidden;
			this.boxSlaves = new List<PokemonBoxViewer>();
			this.pokeBoxControlMaster.PokemonSelected += OnPokemonSelected;
		}



		public IGameSave GameSave {
			get { return PokeManager.GetGameSaveAt(gameIndex); }
		}

		public PokemonTab PokemonTab {
			get { return pokemonTab; }
			set { pokemonTab = value; }
		}

		public PokemonViewer PokemonViewer {
			get { return pokemonViewer; }
			set { pokemonViewer = value; }
		}

		public ComboBoxGameSaves ComboBoxGames {
			get { return comboBoxGame; }
		}
		public ComboBoxPCRows ComboBoxRows {
			get { return comboBoxRows; }
		}

		public bool SupressIndexChanged {
			get { return supressGameChanged; }
			set { supressGameChanged = value; }
		}

		public void SetAsMaster() {
			comboBoxGame.Visibility = Visibility.Hidden;
			this.comboBoxGame.Visibility = Visibility.Hidden;
		}

		public void GotoPokemon(IPokemon pokemon) {
			if (pokemon.PokeContainer == null || pokemon.ContainerIndex == -1) {
				gotoPokemon = null;
				return;
			}
			if (pokemon.PokeContainer.Type == ContainerTypes.Party) {
				if (containerMode != ContainerTypes.Party) {
					containerMode = ContainerTypes.Party;

					buttonParty.Content = (pokePC.Daycare != null ? "Show Daycare" : "Hide Party");
					pokeBoxControlParty.Visibility = Visibility.Visible;
					pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
					pokeBoxControlParty.HighlightPokemon(pokemon);
				}
			}
			else if (pokemon.PokeContainer.Type == ContainerTypes.Daycare) {
				if (containerMode != ContainerTypes.Daycare) {
					containerMode = ContainerTypes.Daycare;

					buttonParty.Content = "Hide Daycare";
					pokeBoxControlParty.Visibility = Visibility.Visible;
					pokeBoxControlParty.LoadBox(pokePC.Daycare, gameIndex);
					pokeBoxControlParty.HighlightPokemon(pokemon);
				}
			}
			else {
				containerMode = ContainerTypes.Box;
				buttonParty.Content = "Show Party";
				pokeBoxControlParty.Visibility = Visibility.Hidden;
				boxIndex = (int)(pokemon.PokeContainer as IPokeBox).BoxNumber;
				pokePC.CurrentBox = boxIndex;

				pokeBoxControlMaster.LoadBox(GetWrappedBox(boxIndex + BoxOffset), gameIndex);
				for (int i = 0; i < slaves.Count; i++)
					slaves[i].LoadBox(GetWrappedBox(boxIndex + i + 1 + BoxOffset), gameIndex);

				if (BoxOffset == 0)
					pokeBoxControlMaster.HighlightPokemon(pokemon);
				else
					slaves[-BoxOffset - 1].HighlightPokemon(pokemon);
			}
			gotoPokemon = pokemon;
		}

		public PokemonBoxViewer BoxFocus {
			get { return focus; }
			set { focus = value; }
		}
		public bool HasFocus {
			get {
				if (master != null)
					return master.focus == this;
				return focus == this;
			}
		}
		public void SetFocus() {
			if (master != null && master.focus != null && master.focus != this)
				master.focus.UnsetFocus();
			else if (focus != null && focus != this)
				focus.UnsetFocus();
			if (master != null)
				master.focus = this;
			else
				focus = this;
			Background = new LinearGradientBrush(Color.FromRgb(0, 122, 204), Color.FromRgb(238, 238, 238), 0);
		}
		private void UnsetFocus() {
			Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			if (!DesignerProperties.GetIsInDesignMode(this)) {
				this.pokeBoxControlMaster.AddSlave(pokeBoxControlParty);
				this.pokeBoxControlMaster.Mode = PokeBoxControlModes.MovePokemon;
				Window.GetWindow(this).PreviewKeyDown += OnBoxMovementKeyDown;
				this.Loaded -= OnLoaded;

				this.pokeBoxControlMaster.MouseMoveTarget = ((FrameworkElement)this.Parent).Parent as UIElement;
			}
		}
		private int BoxOffset {
			get { return -(boxesVisible - 1) / 2; }
		}

		public void Reload() {
			comboBoxGame.ReloadGameSaves();
			if (pokePC != null) {
				if (boxIndex >= pokePC.NumBoxes) {
					int mod = (boxIndex % (int)pokePC.NumBoxes);
					boxIndex = (mod < 0 ? (mod + (int)pokePC.NumBoxes) : mod);
				}
				UpdateSlaves(ActualWidth);

				pokeBoxControlMaster.LoadBox(GetWrappedBox(boxIndex + BoxOffset), gameIndex);
				for (int i = 0; i < slaves.Count; i++)
					slaves[i].LoadBox(GetWrappedBox(boxIndex + i + 1 + BoxOffset), gameIndex);
				if (containerMode == ContainerTypes.Party)
					pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
				else if (containerMode == ContainerTypes.Daycare)
					pokeBoxControlParty.LoadBox(pokePC.Daycare, gameIndex);
				gotoPokemon = null;
				pokeBoxControlMaster.UnhighlightPokemon();
			}
		}
		public void AddSlave(PokemonBoxViewer boxSlave) {
			this.boxSlaves.Add(boxSlave);
			boxSlave.master = this;
			pokeBoxControlMaster.AddSlave(boxSlave.pokeBoxControlMaster);
			boxSlave.pokeBoxControlMaster.ClearSlaves();
			pokeBoxControlMaster.AddSlave(boxSlave.pokeBoxControlParty);
		}

		public void RefreshUI() {
			if (pokePC != null) {
				if (GameSave is ManagerGameSave) {
					this.pokePC = (GameSave as ManagerGameSave).GetPokePCRow(rowIndex);
					loaded = false;
					comboBoxRows.SelectedIndex = rowIndex;
					loaded = true;
					comboBoxRows.Visibility = Visibility.Visible;
					buttonParty.Visibility = Visibility.Hidden;
				}
				else {
					this.pokePC = GameSave.PokePC;
					comboBoxRows.Visibility = Visibility.Hidden;
					buttonParty.Visibility = Visibility.Visible;
				}

				if (boxIndex >= pokePC.NumBoxes) {
					int mod = (boxIndex % (int)pokePC.NumBoxes);
					boxIndex = (mod < 0 ? (mod + (int)pokePC.NumBoxes) : mod);
				}
				UpdateSlaves(ActualWidth);

				pokeBoxControlMaster.LoadBox(GetWrappedBox(boxIndex + BoxOffset), gameIndex);
				for (int i = 0; i < slaves.Count; i++)
					slaves[i].LoadBox(GetWrappedBox(boxIndex + i + 1 + BoxOffset), gameIndex);
				if (containerMode == ContainerTypes.Party)
					pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
				else if (containerMode == ContainerTypes.Daycare)
					pokeBoxControlParty.LoadBox(pokePC.Daycare, gameIndex);
			}
		}
		public void RefreshGameSaves() {
			comboBoxGame.RefreshGameSaves();
		}
		public void FinishActions() {
			pokeBoxControlMaster.FinishActions();
		}

		public void UnloadPC() {
			pokePC = null;
			pokeBoxControlMaster.UnloadBox();
			foreach (PokeBoxControl slave in slaves)
				slave.UnloadBox();
		}

		public void LoadUI(int newGameIndex = -2, int newRowIndex = -1) {
			if (gameIndex != -2 || rowIndex != -1) {
				loaded = false;
				if (newGameIndex != -2)
					this.gameIndex = newGameIndex;
				if (newRowIndex != -1)
					this.rowIndex = newRowIndex;
				this.comboBoxGame.SelectedGameIndex = this.gameIndex;
				this.comboBoxRows.SelectedIndex = this.rowIndex;
				loaded = true;

				gotoPokemon = null;
				pokeBoxControlMaster.UnhighlightPokemon();
			}

			if (GameSave is ManagerGameSave) {
				this.pokePC = (GameSave as ManagerGameSave).GetPokePCRow(rowIndex);
				comboBoxRows.Visibility = Visibility.Visible;
				buttonParty.Visibility = Visibility.Hidden;
			}
			else {
				this.pokePC = GameSave.PokePC;
				comboBoxRows.Visibility = Visibility.Hidden;
				buttonParty.Visibility = Visibility.Visible;
			}

			if (pokePC == null) {
				this.pokeBoxControlMaster.UnloadBox();
				foreach (PokeBoxControl slave in slaves)
					slave.UnloadBox();
				return;
			}
			this.boxIndex = pokePC.CurrentBox;
			this.pokeBoxControlMaster.PokemonViewer = pokemonViewer;
			this.pokeBoxControlParty.PokemonViewer = pokemonViewer;

			if (this.pokePC.Party == null) {
				containerMode = ContainerTypes.Box;
				buttonParty.Content = "Show Party";
				buttonParty.IsEnabled = false;
				pokeBoxControlParty.Visibility = Visibility.Hidden;
			}
			else {
				buttonParty.IsEnabled = true;
			}

			RefreshUI();
		}

		private void OnPreviousBoxButtonClicked(object sender, RoutedEventArgs e) {
			if (pokePC == null)
				return;

			pokePC.CurrentBox = boxIndex - 1;
			boxIndex = pokePC.CurrentBox;
			pokeBoxControlMaster.LoadBox(GetWrappedBox(boxIndex + BoxOffset), gameIndex);
			for (int i = 0; i < slaves.Count; i++)
				slaves[i].LoadBox(GetWrappedBox(boxIndex + i + 1 + BoxOffset), gameIndex);
			if (containerMode == ContainerTypes.Party)
				pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
			else if (containerMode == ContainerTypes.Daycare)
				pokeBoxControlParty.LoadBox(pokePC.Daycare, gameIndex);
			gotoPokemon = null;
			pokeBoxControlMaster.UnhighlightPokemon();
		}
		private void OnNextBoxButtonClicked(object sender, RoutedEventArgs e) {
			if (pokePC == null)
				return;

			pokePC.CurrentBox = boxIndex + 1;
			boxIndex = pokePC.CurrentBox;
			pokeBoxControlMaster.LoadBox(GetWrappedBox(boxIndex + BoxOffset), gameIndex);
			for (int i = 0; i < slaves.Count; i++)
				slaves[i].LoadBox(GetWrappedBox(boxIndex + i + 1 + BoxOffset), gameIndex);
			if (containerMode == ContainerTypes.Party)
				pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
			else if (containerMode == ContainerTypes.Daycare)
				pokeBoxControlParty.LoadBox(pokePC.Daycare, gameIndex);
			gotoPokemon = null;
			pokeBoxControlMaster.UnhighlightPokemon();

		}
		private void HideParty() {
			pokeBoxControlParty.Visibility = Visibility.Hidden;
			buttonParty.Content = "Show Party";
			containerMode = ContainerTypes.Box;
		}

		private void OnPartyButtonClicked(object sender, RoutedEventArgs e) {
			if (containerMode == ContainerTypes.Box) {
				containerMode = ContainerTypes.Party;
				pokeBoxControlParty.Visibility = Visibility.Visible;
				if (pokePC.Daycare != null)
					buttonParty.Content = "Show Daycare";
				else
					buttonParty.Content = "Hide Party";
			}
			else if (containerMode == ContainerTypes.Party) {
				if (pokePC.Daycare != null) {
					buttonParty.Content = "Hide Daycare";
					containerMode = ContainerTypes.Daycare;
				}
				else {
					buttonParty.Content = "Show Party";
					containerMode = ContainerTypes.Box;
					pokeBoxControlParty.Visibility = Visibility.Hidden;
				}
			}
			else {
				pokeBoxControlParty.Visibility = Visibility.Hidden;
				buttonParty.Content = "Show Party";
				containerMode = ContainerTypes.Box;
			}

			if (containerMode == ContainerTypes.Party)
				pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
			else if (containerMode == ContainerTypes.Daycare)
				pokeBoxControlParty.LoadBox(pokePC.Daycare, gameIndex);
		}

		private IPokeContainer PokeBox {
			get {
				if (containerMode == ContainerTypes.Daycare)
					return pokePC.Daycare;
				else if (containerMode == ContainerTypes.Party)
					return pokePC.Party;
				else
					return pokePC[boxIndex];
			}
		}

		private void OnBoxMovementKeyDown(object sender, KeyEventArgs e) {
			if (IsVisible && HasFocus) {
				if (e.Key == Key.A || e.Key == Key.Left)
					OnPreviousBoxButtonClicked(null, null);
				else if (e.Key == Key.D || e.Key == Key.Right)
					OnNextBoxButtonClicked(null, null);
				else if (pokePC.Party != null && containerMode != ContainerTypes.Daycare && (e.Key == Key.S || e.Key == Key.Down))
					OnPartyButtonClicked(null, null);
				else if (pokePC.Party != null && containerMode != ContainerTypes.Box && (e.Key == Key.W || e.Key == Key.Up))
					HideParty();
			}
		}

		private IPokeContainer GetWrappedBox(int index) {
			int mod = (index % (int)pokePC.NumBoxes);
			index = (mod < 0 ? (mod + (int)pokePC.NumBoxes) : mod);
			return pokePC[index];
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			UpdateSlaves(e.NewSize.Width);
		}

		public void UpdateSlavesWidth() {
			UpdateSlaves(this.ActualWidth);
		}

		private void UpdateSlaves(double newWidth) {
			double sizeAvailable = newWidth - 104;
			double boxWidth = 156;
			double spacing = 6;
			int boxesAvailable = Math.Max(1, Math.Min(pokePC != null ? (int)pokePC.NumBoxes : 1, (int)Math.Floor((sizeAvailable + spacing) / (boxWidth + spacing))));
			if (boxesVisible != boxesAvailable) {
				stackPanelBoxes.Width = boxesAvailable * (boxWidth + spacing) - spacing;

				if (boxesVisible < boxesAvailable) {
					for (int i = boxesVisible; i < boxesAvailable; i++) {
						PokeBoxControl slave = new PokeBoxControl();
						slave.Margin = new Thickness(spacing, 0, 0, 0);
						slave.PokemonViewer = pokemonViewer;
						slaves.Add(slave);
						stackPanelBoxes.Children.Add(slave);
						if (pokeBoxControlMaster.Master != null)
							pokeBoxControlMaster.Master.AddSlave(slave);
						else
							pokeBoxControlMaster.AddSlave(slave);
						slave.MouseMoveTarget = ((FrameworkElement)this.Parent).Parent as UIElement;
					}
				}
				else if (boxesVisible > boxesAvailable) {
					for (int i = boxesVisible - 1; i >= boxesAvailable; i--) {
						if (pokeBoxControlMaster.Master != null)
							pokeBoxControlMaster.Master.RemoveSlave(slaves[i - 1]);
						else
							pokeBoxControlMaster.RemoveSlave(slaves[i - 1]);
						slaves.RemoveAt(i - 1);
						stackPanelBoxes.Children.RemoveAt(i);
					}
				}
				boxesVisible = boxesAvailable;
			}

			if (pokePC != null) {
				pokeBoxControlMaster.LoadBox(GetWrappedBox(boxIndex + BoxOffset), gameIndex);
				for (int i = 0; i < slaves.Count; i++)
					slaves[i].LoadBox(GetWrappedBox(boxIndex + i + 1 + BoxOffset), gameIndex);
				pokeBoxControlMaster.UnhighlightPokemon();
				if (gotoPokemon != null)
					GotoPokemon(gotoPokemon);
				//if (partyMode)
				//	pokeBoxControlParty.LoadBox(pokePC.Party, gameIndex);
			}
		}

		private void OnRowSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (loaded) {
				if (pokePC.GameSave is ManagerGameSave) {
					if (comboBoxRows.SelectedIndex != -1) {
						rowIndex = comboBoxRows.SelectedIndex;
						LoadUI();
						//LoadGame(pokePC.GameSave, pokemonViewer, gameIndex, rowIndex);
						if (pokemonTab != null && !supressGameChanged)
							pokemonTab.OnRowSwitch(this);
					}
				}
				else {
					rowIndex = 0;
				}
			}
		}

		private void OnGameSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (loaded) {
				if (comboBoxGame.SelectedGameIndex != -2) {
					gameIndex = comboBoxGame.SelectedGameIndex;
					LoadUI();
					//LoadGame(comboBoxGame.SelectedGameSave, pokemonViewer, comboBoxGame.SelectedGameIndex);
					if (pokemonTab != null && !supressGameChanged)
						pokemonTab.OnGameSwitch(this);
				}
			}
		}

		private void OnFocusClicked(object sender, RoutedEventArgs e) {
			SetFocus();
		}

		private void OnPokemonSelected(object sender, PokemonSelectedEventArgs e) {
			gotoPokemon = null;
		}

	}
}
