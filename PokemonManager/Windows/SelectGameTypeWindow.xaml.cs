using PokemonManager.Game;
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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for ItemCount.xaml
	/// </summary>
	public partial class SelectGameTypeWindow : Window {

		private GameTypes gameType1;
		private GameTypes gameType2;
		private GameTypes result;

		public SelectGameTypeWindow(GameTypes gameType1, GameTypes gameType2) {
			InitializeComponent();

			this.gameType1 = gameType1;
			this.gameType2 = gameType2;

			imageGame1.Source = ResourceDatabase.GetImageFromName(gameType1.ToString() + "Physical");
			imageGame2.Source = ResourceDatabase.GetImageFromName(gameType2.ToString() + "Physical");
			labelGame1.Content = gameType1.ToString();
			labelGame2.Content = gameType2.ToString();
		}


		public static GameTypes? ShowDialog(Window owner, GameTypes gameType1, GameTypes gameType2) {
			SelectGameTypeWindow form = new SelectGameTypeWindow(gameType1, gameType2);
			form.Owner = owner;
			form.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var dialogResult = form.ShowDialog();

			if (dialogResult.HasValue && dialogResult.Value) {
				return form.result;
			}
			return null;
		}

		private void OnGameType1Clicked(object sender, RoutedEventArgs e) {
			result = gameType1;
			DialogResult = true;
			Close();
		}

		private void OnGameType2Clicked(object sender, RoutedEventArgs e) {
			result = gameType2;
			DialogResult = true;
			Close();
		}
	}
}
