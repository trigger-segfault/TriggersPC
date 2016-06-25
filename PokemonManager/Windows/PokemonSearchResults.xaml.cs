using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for PokemonSearchResults.xaml
	/// </summary>
	public partial class PokemonSearchResults : Window {

		private List<Grid> grids;
		private Grid currentSelection;
		private Grid hoverSelection;
		private Brush resultDisabled;
		private Brush resultHighlight;
		private Brush resultHighlightChecked;
		private Brush resultUnhighlighted;
		private Brush resultUnhighlightChecked;
		private ContextMenu contextMenu;
		private Grid clickSelection;

		public bool IsClosed { get; set; }

		private List<IPokemon> results;

		public PokemonSearchResults() {
			InitializeComponent();

			this.resultDisabled				= new SolidColorBrush(Color.FromRgb(170, 170, 170));
			this.resultUnhighlighted		= new SolidColorBrush(Color.FromRgb(255, 255, 255));
			this.resultHighlight			= new SolidColorBrush(Color.FromRgb(173, 210, 255));
			this.resultUnhighlightChecked	= new SolidColorBrush(Color.FromRgb(23, 108, 211));
			this.resultHighlightChecked		= new SolidColorBrush(Color.FromRgb(79, 149, 234));

			pokemonViewer.UnloadPokemon();

			contextMenu = new ContextMenu();

			MenuItem gotoItem = new MenuItem();
			gotoItem.Header = "Goto Location";
			gotoItem.Click += OnGotoClicked;
			contextMenu.Items.Add(gotoItem);
			//pokemonViewer.DisableEditing = true;
		}

		public void RefreshUI() {
			pokemonViewer.RefreshUI();
		}

		public void ShowResults(List<IPokemon> results) {
			this.results = results;
			PopulateResults(results);
		}

		public static PokemonSearchResults Show(Window owner, List<IPokemon> results) {
			PokemonSearchResults window = new PokemonSearchResults();
			window.results = results;
			window.Owner = owner;
			//results.Sort((p1, p2) => (((int)p2.HPIV + p2.AttackIV + p2.DefenseIV + p2.SpAttackIV + p2.SpDefenseIV + p2.SpeedIV) - ((int)p1.HPIV + p1.AttackIV + p1.DefenseIV + p1.SpAttackIV + p1.SpDefenseIV + p1.SpeedIV)));

			window.PopulateResults(results);
			window.Show();
			return window;
		}

		public void PopulateResults(IEnumerable<IPokemon> results) {
			this.grids = new List<Grid>();
			stackPanelPokemon.Children.Clear();
			int index = 0;
			StackPanel row = null;
			int columns = 14;// (int)(stackPanelPokemon.ActualWidth / 30);
			foreach (IPokemon pokemon in results) {
				if (index % columns == 0) {
					row = new StackPanel();
					row.Orientation = Orientation.Horizontal;
					stackPanelPokemon.Children.Add(row);
					stackPanelPokemon.Height += 30;
				}

				Grid grid = new Grid();
				grid.Width = 30;
				grid.Height = 30;
				grid.MouseEnter += OnMouseEnter;
				grid.MouseLeave += OnMouseLeave;
				grid.PreviewMouseDown += OnMouseClicked;
				grid.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				grid.ToolTip = ""; // Set the tooltip so we can modify it when it opens.
				grid.ContextMenu = contextMenu;

				Image image = new Image();
				image.Source = pokemon.BoxSprite;
				image.Stretch = Stretch.None;
				image.Width = 32;
				image.Height = 32;
				image.Margin = new Thickness(-1, -2, -1, 0);
				image.IsHitTestVisible = false;
				grid.Children.Add(image);

				if (pokemon.IsEgg) {
					image.Source = PokemonDatabase.GetPokemonBoxImageFromDexID(pokemon.DexID, pokemon.IsShiny, pokemon.FormID);

					Image egg = new Image();
					egg.Source = ResourceDatabase.GetImageFromName((PokeManager.Settings.UseNewBoxSprites ? "New" : "") + "SideEgg");
					egg.Stretch = Stretch.None;
					egg.Width = 9;
					egg.Height = 11;
					egg.Margin = new Thickness(19, 19, 0, 0);
					egg.IsHitTestVisible = false;
					grid.Children.Add(egg);
				}


				if (pokemon.IsShadowPokemon) {
					Rectangle shadowMask = new Rectangle();
					shadowMask.Width = 32;
					shadowMask.Height = 32;
					shadowMask.Stroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
					shadowMask.StrokeThickness = 0;
					shadowMask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));
					shadowMask.Margin = new Thickness(-1, -2, -1, 0);
					shadowMask.OpacityMask = new ImageBrush(pokemon.BoxSprite);
					shadowMask.IsHitTestVisible = false;

					grid.Children.Add(shadowMask);
				}

				grid.Tag = pokemon;
				grid.ToolTipOpening += OnTooltipOpening;
				row.Children.Add(grid);

				grids.Add(grid);

				index++;
			}
		}

		private void OnTooltipOpening(object sender, ToolTipEventArgs e) {
			IPokemon pokemon = hoverSelection.Tag as IPokemon;
			if (pokemon.ContainerIndex == -1) {
				pokemon = pokemon.PokemonFinder.Pokemon;
				hoverSelection.Tag = pokemon;
			}

			if (pokemon.IsReleased) {
				hoverSelection.ToolTip = "(Released)";
			}
			else if (pokemon.IsMoving) {
				hoverSelection.ToolTip = "(Moving)";
			}
			else if (pokemon.ContainerIndex == -1) {
				hoverSelection.ToolTip = "(Unable to Find - This shouldn't happen)";
			}
			else {
				StackPanel tooltip = new StackPanel();
				tooltip.Orientation = Orientation.Horizontal;

				TextBlock gameName = new TextBlock();
				string gameTypeName = (pokemon.GameSave.GameType == GameTypes.PokemonBox ? "Pokémon Box" : pokemon.GameSave.GameType.ToString());
				if (PokeManager.GetGameSaveFileInfoNickname(pokemon.GameSave.GameIndex) != "")
					gameName.Text = PokeManager.GetGameSaveFileInfoNickname(pokemon.GameSave.GameIndex) + (pokemon.GameSave.GameType != GameTypes.PokemonBox && pokemon.GameSave.GameType != GameTypes.Any ? " [" : "");
				else
					gameName.Text = gameTypeName + (pokemon.GameSave.GameType != GameTypes.PokemonBox && pokemon.GameSave.GameType != GameTypes.Any ? " [" : " ");
				gameName.VerticalAlignment = VerticalAlignment.Center;

				TextBlock trainerName = new TextBlock();
				trainerName.Text = pokemon.GameSave.TrainerName;
				trainerName.Foreground = new SolidColorBrush(pokemon.GameSave.TrainerGender == Genders.Male ? Color.FromRgb(32, 128, 248) : (pokemon.GameSave.TrainerGender == Genders.Female ? Color.FromRgb(248, 24, 168) : Color.FromRgb(0, 0, 0)));
				trainerName.VerticalAlignment = VerticalAlignment.Center;

				TextBlock ending = new TextBlock();
				ending.VerticalAlignment = VerticalAlignment.Center;
				ending.Text = "]";

				TextBlock storage = new TextBlock();
				storage.VerticalAlignment = VerticalAlignment.Center;
				storage.Text = " (";
				if (pokemon.PokeContainer.Type == ContainerTypes.Box)
					storage.Text += "Box " + ((pokemon.PokeContainer as IPokeBox).BoxNumber + 1).ToString() + " " + (pokemon.PokeContainer as IPokeBox).Name.ToString();
				else if (pokemon.PokeContainer.Type == ContainerTypes.Party)
					storage.Text += "Party";
				else if (pokemon.PokeContainer.Type == ContainerTypes.Daycare)
					storage.Text += "Daycare";
				storage.Text += " Slot " + (pokemon.PokeContainer.IndexOf(pokemon) + 1).ToString() + ")";


				tooltip.Children.Add(gameName);
				if (pokemon.GameSave.GameType != GameTypes.PokemonBox && pokemon.GameSave.GameType != GameTypes.Any) {
					tooltip.Children.Add(trainerName);
					tooltip.Children.Add(ending);
				}
				tooltip.Children.Add(storage);

				hoverSelection.ToolTip = tooltip;
			}
		}

		private void OnMouseClicked(object sender, MouseButtonEventArgs e) {
			Grid grid = sender as Grid;
			IPokemon pokemon = grid.Tag as IPokemon;
			if (pokemon.ContainerIndex == -1) {
				pokemon = pokemon.PokemonFinder.Pokemon;
				grid.Tag = pokemon;
			}

			clickSelection = grid;
			if (!pokemon.IsReleased) {
				if (e.ChangedButton == MouseButton.Left || (e.ChangedButton == MouseButton.Right && currentSelection != grid)) {
					if (currentSelection != null && currentSelection != grid)
						currentSelection.Background = resultUnhighlighted;
					currentSelection = (grid != currentSelection ? grid : null);
					grid.Background = (grid == currentSelection ? resultHighlightChecked : resultHighlight);
					if (currentSelection != null)
						pokemonViewer.LoadPokemon(pokemon);
				}
			}
		}

		private void OnMouseEnter(object sender, MouseEventArgs e) {
			Grid grid = sender as Grid;
			IPokemon pokemon = grid.Tag as IPokemon;
			if (pokemon.ContainerIndex == -1) {
				pokemon = pokemon.PokemonFinder.Pokemon;
				grid.Tag = pokemon;
			}

			if (pokemon.IsReleased) {
				grid.Background = resultDisabled;
				//grid.IsEnabled = false;
				if (hoverSelection == grid)
					hoverSelection = null;
				if (currentSelection == grid) {
					currentSelection = null;
					pokemonViewer.UnloadPokemon();
				}
			}
			else {
				hoverSelection = grid;
				if (currentSelection == null)
					pokemonViewer.LoadPokemon(pokemon);
				if (currentSelection == grid)
					grid.Background = resultHighlightChecked;
				else
					grid.Background = resultHighlight;
			}

			
		}
		private void OnMouseLeave(object sender, MouseEventArgs e) {
			Grid grid = sender as Grid;
			IPokemon pokemon = grid.Tag as IPokemon;
			if (!pokemon.IsReleased) {
				if (currentSelection == grid)
					grid.Background = resultUnhighlightChecked;
				else
					grid.Background = resultUnhighlighted;
			}
		}

		private void OnClosing(object sender, CancelEventArgs e) {
			IsClosed = true;
		}

		private void OnGotoClicked(object sender, RoutedEventArgs e) {
			IPokemon pokemon = clickSelection.Tag as IPokemon;
			if (pokemon.ContainerIndex == -1) {
				pokemon = pokemon.PokemonFinder.Pokemon;
				clickSelection.Tag = pokemon;
			}
			if (pokemon.IsReleased) {
				TriggerMessageBox.Show(this, "Cannot goto this Pokémon, it has been released", "Released Pokémon");
			}
			else if (pokemon.IsMoving) {
				TriggerMessageBox.Show(this, "Cannot goto a Pokémon while it is being moved", "Moving Pokémon");
			}
			else if (pokemon.ContainerIndex == -1) {
				TriggerMessageBox.Show(this, "The Search Results have lost track of " + pokemon.Nickname + " because it has been moved to a different game. This should not happen. Let me know if it does.", "Can't Find Pokémon");
			}
			else {
				PokeManager.ManagerWindow.GotoPokemon(pokemon);
			}
		}
	}
}
