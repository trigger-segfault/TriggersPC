using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
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

	public partial class EVTrainingWindow : Window {

		private byte level;

		public EVTrainingWindow(byte maxLevel) {
			InitializeComponent();
			this.level = 95;
			numericLevel.Maximum = maxLevel - 1;
		}

		public static byte? ShowDialog(Window owner, byte maxLevel) {
			EVTrainingWindow window = new EVTrainingWindow(maxLevel);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result.HasValue && result.Value) {
				return window.level;
			}
			return null;
		}
		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
		private void OnLevelChanged(object sender, RoutedEventArgs e) {
			level = (byte)numericLevel.Value;
		}
	}
}
