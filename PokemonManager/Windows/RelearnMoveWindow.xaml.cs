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
	public partial class RelearnMoveWindow : Window {

		private IPokemon pokemon;
		private int selectedIndex;
		private Move selectedMove;
		private bool contestMode;
		private MoveData currentMoveData;

		public RelearnMoveWindow(IPokemon pokemon) {
			InitializeComponent();
			this.pokemon = pokemon;

			ushort[] moves = PokemonDatabase.GetRelearnableMoves(pokemon);

			foreach (ushort moveID in moves) {
				Move move = new Move(moveID);
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
				listViewMoves.Items.Add(listViewItem);
			}

			this.labelMoveAccuracy.Content = "";
			this.labelMovePower.Content = "";
			this.labelMoveCategory.Content = "";
			this.labelMoveAppeal.Content = "";
			this.labelMoveJam.Content = "";
			this.textBlockMoveDescription.Text = "";
			buttonOpenMoveInBulbapedia.Visibility = Visibility.Hidden;
		}

		public static bool? ShowDialog(Window owner, IPokemon pokemon) {
			RelearnMoveWindow window = new RelearnMoveWindow(pokemon);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			ushort[] validItemIDs = new ushort[] { 103, 104, 111 };

			Item item = SelectItemWindow.ShowDialog(this, validItemIDs, new ItemTypes[]{ ItemTypes.Items }, "Hand Over Valuable", "Hand Over", true);
			if (item != null) {
				if (item.ID == 103 && item.Count < 2) {
					TriggerMessageBox.Show(this, "2 " + item.ItemData.Name + "s are needed to learn a move. You don't have enough", "Not Enough");
				}
				else {
					if (pokemon.NumMoves == 4) {
						var result = LearnMoveWindow.ShowDialog(this, pokemon, selectedMove.ID);
						if (result.HasValue && result.Value) {
							item.Pocket.TossItemAt(item.Pocket.IndexOf(item), (uint)(item.ID == 103 ? 2 : 1));
							TriggerMessageBox.Show(this, pokemon.Nickname + " learned " + selectedMove.MoveData.Name + "!", "Move Learned");
							PokeManager.RefreshUI();
							DialogResult = true;
						}
					}
					else {
						pokemon.SetMoveAt(pokemon.NumMoves, selectedMove);
						item.Pocket.TossItemAt(item.Pocket.IndexOf(item), (uint)(item.ID == 103 ? 2 : 1));
						TriggerMessageBox.Show(this, pokemon.Nickname + " learned " + selectedMove.MoveData.Name + "!", "Move Learned");
						PokeManager.RefreshUI();
						DialogResult = true;
					}
				}
			}
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
				selectedMove = (Move)(listViewMoves.Items[selectedIndex] as ListViewItem).Tag;
				this.labelMovePower.Content = (selectedMove.MoveData.Power != 0 ? selectedMove.MoveData.Power.ToString() : "---");
				this.labelMoveAccuracy.Content = (selectedMove.MoveData.Accuracy != 0 ? selectedMove.MoveData.Accuracy.ToString() : "---");
				this.labelMoveCategory.Content = selectedMove.MoveData.Category.ToString();
				this.labelMoveAppeal.Content = selectedMove.MoveData.Appeal;
				this.labelMoveJam.Content = selectedMove.MoveData.Jam;
				this.textBlockMoveDescription.Text = (contestMode ? selectedMove.MoveData.ContestDescription : selectedMove.MoveData.Description);
				currentMoveData = selectedMove.MoveData;
				buttonOpenMoveInBulbapedia.Visibility = Visibility.Visible;
			}
			buttonTeachMove.IsEnabled = selectedIndex != -1;
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
			string url = "http://bulbapedia.bulbagarden.net/wiki/" + currentMoveData.Name + "_(move)";
			System.Diagnostics.Process.Start(url);
		}
	}
}
