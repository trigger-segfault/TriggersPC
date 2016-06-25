using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
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
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class ChangeMarkingsWindow : Window {

		private MarkingFlags markings;
		private IPokemon pokemon;
		private Brush unmarkedBrush;
		private Brush markedBrush;
		private Brush unmarkedHoverBrush;
		private Brush markedHoverBrush;

		public ChangeMarkingsWindow(IPokemon pokemon) {
			InitializeComponent();
			this.markings = pokemon.Markings;
			this.pokemon = pokemon;

			this.circleHitBox.Tag = this.markCircle;
			this.squareHitBox.Tag = this.markSquare;
			this.triangleHitBox.Tag = this.markTriangle;
			this.heartHitBox.Tag = this.markHeart;
			this.markCircle.Tag = MarkingFlags.Circle;
			this.markSquare.Tag = MarkingFlags.Square;
			this.markTriangle.Tag = MarkingFlags.Triangle;
			this.markHeart.Tag = MarkingFlags.Heart;

			this.markedBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			this.markedHoverBrush = new SolidColorBrush(Color.FromRgb(120, 120, 120));
			this.unmarkedBrush = new SolidColorBrush(Color.FromRgb(170, 170, 170));
			this.unmarkedHoverBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));

			this.markCircle.Foreground = (pokemon.IsCircleMarked ? markedBrush : unmarkedBrush);
			this.markSquare.Foreground = (pokemon.IsSquareMarked ? markedBrush : unmarkedBrush);
			this.markTriangle.Foreground = (pokemon.IsTriangleMarked ? markedBrush : unmarkedBrush);
			this.markHeart.Foreground = (pokemon.IsHeartMarked ? markedBrush : unmarkedBrush);
		}

		public static bool? ShowDialog(Window owner, IPokemon pokemon) {
			ChangeMarkingsWindow window = new ChangeMarkingsWindow(pokemon);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			pokemon.Markings = markings;
			PokeManager.RefreshUI();
			DialogResult = true;
		}


		private void OnMarkingClicked(object sender, MouseButtonEventArgs e) {
			MarkingFlags flag = (MarkingFlags)((Label)((Rectangle)sender).Tag).Tag;
			if (markings.HasFlag(flag)) {
				markings &= ~flag;
				((Label)((Rectangle)sender).Tag).Foreground = unmarkedHoverBrush;
			}
			else {
				markings |= flag;
				((Label)((Rectangle)sender).Tag).Foreground = markedHoverBrush;
			}
		}
		private void OnMarkingMouseEnter(object sender, MouseEventArgs e) {
			if (markings.HasFlag((MarkingFlags)((Label)((Rectangle)sender).Tag).Tag))
				((Label)((Rectangle)sender).Tag).Foreground = markedHoverBrush;
			else
				((Label)((Rectangle)sender).Tag).Foreground = unmarkedHoverBrush;
		}
		private void OnMarkingMouseLeave(object sender, MouseEventArgs e) {
			if (markings.HasFlag((MarkingFlags)((Label)((Rectangle)sender).Tag).Tag))
				((Label)((Rectangle)sender).Tag).Foreground = markedBrush;
			else
				((Label)((Rectangle)sender).Tag).Foreground = unmarkedBrush;
		}
	}
}
