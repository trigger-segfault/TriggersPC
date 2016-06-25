using PokemonManager.Game;
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
	public partial class PokemonBoxViewerBackup : UserControl {


		private GameTypes gameType;
		private IPokePC pokePC;
		private int boxIndex;
		private bool partyMode;
		private PokemonViewer pokemonViewer;

		public PokemonBoxViewerBackup() {
			InitializeComponent();

			this.pokeBoxControl.Mode = PokeBoxControlModes.MovePokemon;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			if (!DesignerProperties.GetIsInDesignMode(this)) {
				Window.GetWindow(this).PreviewKeyDown += OnBoxMovementKeyDown;
				this.Loaded -= OnLoaded;
			}
		}

		public void RefreshUI() {
			if (pokePC != null) {
				LoadPC(pokePC, boxIndex, pokemonViewer, gameType);
			}
		}
		public void FinishActions() {
			pokeBoxControl.FinishActions();
		}

		public void UnloadPC() {
			pokePC = null;
			pokeBoxControl.UnloadBox();
		}

		public void LoadPC(IPokePC pokePC, int boxIndex, PokemonViewer pokemonViewer, GameTypes gameType) {
			if (pokePC == null) {
				this.pokeBoxControl.UnloadBox();
				return;
			}
			this.gameType = gameType;
			this.pokePC = pokePC;
			this.boxIndex = boxIndex;
			this.pokemonViewer = pokemonViewer;

			this.pokeBoxControl.PokemonViewer = pokemonViewer;

			this.pokeBoxControl.LoadBox(PokeBox);
		}
		private void OnPreviousBoxButtonClicked(object sender, RoutedEventArgs e) {
			if (pokePC == null)
				return;

			if (boxIndex == 0)
				boxIndex = pokePC.NumBoxes - 1;
			else
				boxIndex--;
			pokeBoxControl.LoadBox(PokeBox);
		}
		private void OnNextBoxButtonClicked(object sender, RoutedEventArgs e) {
			if (pokePC == null)
				return;

			if (boxIndex == pokePC.NumBoxes - 1)
				boxIndex = 0;
			else
				boxIndex++;
			pokeBoxControl.LoadBox(PokeBox);
		}
		private void OnPartyButtonClicked(object sender, RoutedEventArgs e) {
			partyMode = !partyMode;
			buttonParty.Content = (partyMode ? "Boxes" : "Party");
			pokeBoxControl.LoadBox(PokeBox);
		}

		private IPokeContainer PokeBox {
			get { return (partyMode ? (IPokeContainer)pokePC.Party : (IPokeContainer)pokePC[boxIndex]); }
		}

		private void OnBoxMovementKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.A || e.Key == Key.Left)
				OnPreviousBoxButtonClicked(null, null);
			if (e.Key == Key.D || e.Key == Key.Right)
				OnNextBoxButtonClicked(null, null);
			else if (!partyMode && (e.Key == Key.S || e.Key == Key.Down))
				OnPartyButtonClicked(null, null);
			else if (partyMode && (e.Key == Key.W || e.Key == Key.Up))
				OnPartyButtonClicked(null, null);
		}

	}
}
