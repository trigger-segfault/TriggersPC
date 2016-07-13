using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WPF.JoshSmith.ServiceProviders.UI;
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class RearangeMovesWindow : Window {

		private IPokemon pokemon;
		private ListViewDragDropManager<ListViewItem> dropManager;
		private ObservableCollection<ListViewItem> moves;
		private int selectedIndex;
		private Move selectedMove;
		private bool contestMode;
		private MoveData currentMoveData;

		public RearangeMovesWindow(IPokemon pokemon) {
			InitializeComponent();
			this.pokemon = pokemon;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem delete = new MenuItem();
			delete.Header = "Delete Move";
			delete.Click += OnDeleteMove;
			delete.IsEnabled = pokemon.NumMoves > 1;
			contextMenu.Items.Add(delete);
			moves = new ObservableCollection<ListViewItem>();
			for (int i = 0; i < pokemon.NumMoves; i++) {
				Move move = pokemon.GetMoveAt(i);
				ListViewItem listViewItem = new ListViewItem();
				Grid grid = new Grid();
				grid.Width = 321;
				grid.Height = 27;
				Rectangle panel = new Rectangle();
				panel.Margin = new Thickness(1, 1, 0, 0);
				panel.Width = 319;
				panel.Height = 23;
				panel.RadiusX = 2;
				panel.RadiusY = 2;
				panel.Fill = new SolidColorBrush(Color.FromRgb(248, 248, 248));
				panel.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				panel.StrokeThickness = 1;
				PokemonTypeControl type = new PokemonTypeControl();
				type.Type = move.MoveData.Type;
				type.Margin = new Thickness(10, 4, 0, 0);
				ConditionTypeControl condition = new ConditionTypeControl();
				condition.Type = move.MoveData.ConditionType;
				condition.Margin = new Thickness(10, 4, 0, 0);
				condition.Visibility = Visibility.Hidden;
				Label name = new Label();
				name.Content = move.MoveData.Name;
				name.Margin = new Thickness(120, 0, 0, 0);
				name.FontWeight = FontWeights.Bold;
				Label pp = new Label();
				pp.Content = (move.MoveData.PP == 0 ? "--" : move.TotalPP.ToString());
				pp.Margin = new Thickness(281, 0, 0, 0);
				pp.FontWeight = FontWeights.Bold;
				grid.Children.Add(panel);
				grid.Children.Add(type);
				grid.Children.Add(condition);
				grid.Children.Add(name);
				grid.Children.Add(pp);
				listViewItem.Content = grid;
				listViewItem.Tag = move;
				moves.Add(listViewItem);
				listViewItem.ContextMenu = contextMenu;
			}

			listViewMoves.ItemsSource = moves;
			dropManager = new ListViewDragDropManager<ListViewItem>(listViewMoves);

			this.labelMoveAccuracy.Content = "";
			this.labelMovePower.Content = "";
			this.labelMoveCategory.Content = "";
			this.labelMoveAppeal.Content = "";
			this.labelMoveJam.Content = "";
			this.textBlockMoveDescription.Text = "";
			buttonOpenMoveInBulbapedia.Visibility = Visibility.Hidden;
		}

		public static bool? ShowDialog(Window owner, IPokemon pokemon) {
			RearangeMovesWindow window = new RearangeMovesWindow(pokemon);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			for (int i = 0; i < 4; i++) {
				if (i < moves.Count)
					pokemon.SetMoveAt(i, (Move)moves[i].Tag);
				else
					pokemon.SetMoveAt(i, new Move());
			}

			PokeManager.RefreshUI();
			DialogResult = true;
		}

		private void OnDeleteMove(object sender, RoutedEventArgs e) {
			if (moves.Count > 1) {
				if (selectedMove.ID == ((Move)moves[selectedIndex].Tag).ID) {
					labelMovePower.Content = "";
					labelMoveAccuracy.Content = "";
					labelMoveCategory.Content = "";
					textBlockMoveDescription.Text = "";
				}
				moves.RemoveAt(selectedIndex);
			}
			if (moves.Count == 1)
				((MenuItem)moves[0].ContextMenu.Items[0]).IsEnabled = false;
		}

		private void OnMoveSelectionChanged(object sender, SelectionChangedEventArgs e) {
			selectedIndex = listViewMoves.SelectedIndex;
			if (selectedIndex == -1) {
				this.labelMovePower.Content = "";
				this.labelMoveAccuracy.Content = "";
				this.labelMoveCategory.Content = "";
				this.labelMoveAppeal.Content = "";
				this.labelMoveJam.Content = "";
				this.textBlockMoveDescription.Text = "";
				buttonOpenMoveInBulbapedia.Visibility = Visibility.Hidden;
			}
			else {
				Move move = (Move)moves[selectedIndex].Tag;
				this.labelMovePower.Content = (move.MoveData.Power != 0 ? move.MoveData.Power.ToString() : "---");
				this.labelMoveAccuracy.Content = (move.MoveData.Accuracy != 0 ? move.MoveData.Accuracy.ToString() : "---");
				this.labelMoveCategory.Content = move.MoveData.Category.ToString();
				this.labelMoveAppeal.Content = move.MoveData.Appeal;
				this.labelMoveJam.Content = move.MoveData.Jam;
				this.textBlockMoveDescription.Text = (contestMode ? move.MoveData.ContestDescription : move.MoveData.Description);
				currentMoveData = move.MoveData;
				buttonOpenMoveInBulbapedia.Visibility = Visibility.Visible;
			}
		}

		private void OnContestModeChecked(object sender, RoutedEventArgs e) {
			contestMode = checkBoxContestMode.IsChecked.Value;

			this.gridAttackMoveStats.Visibility = (contestMode ? Visibility.Hidden : Visibility.Visible);
			this.gridContestMoveStats.Visibility = (contestMode ? Visibility.Visible : Visibility.Hidden);
			foreach (object item in listViewMoves.Items) {
				Grid grid = (item as ListViewItem).Content as Grid;
				(grid.Children[1] as PokemonTypeControl).Visibility = (contestMode ? Visibility.Hidden : Visibility.Visible);
				(grid.Children[2] as ConditionTypeControl).Visibility = (contestMode ? Visibility.Visible : Visibility.Hidden);
			}

			OnMoveSelectionChanged(null, null);

		}
		private void OnOpenMoveInBulbapedia(object sender, RoutedEventArgs e) {
			string url = "http://bulbapedia.bulbagarden.net/wiki/" + currentMoveData.Name + " _(move)";
			System.Diagnostics.Process.Start(url);
		}
	}
}
