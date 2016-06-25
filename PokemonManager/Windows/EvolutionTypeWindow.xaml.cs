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
	/// Interaction logic for EvolutionTypeWindow.xaml
	/// </summary>
	public partial class EvolutionTypeWindow : Window {

		private int selectedType;
		private bool tradeEvolve;

		public EvolutionTypeWindow(bool tradeEvolve) {
			InitializeComponent();

			selectedType = -1;
			if (tradeEvolve)
				button1.Content = "Trade";
		}

		public static int? ShowDialog(Window owner, bool tradeEvolve) {
			EvolutionTypeWindow window = new EvolutionTypeWindow(tradeEvolve);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result.HasValue && result.Value) {
				return window.selectedType;
			}
			return null;
		}

		private void OnEvolve1Clicked(object sender, RoutedEventArgs e) {
			selectedType = (tradeEvolve ? 1 : 0);
			DialogResult = true;
			Close();
		}
		private void OnEvolve2Clicked(object sender, RoutedEventArgs e) {
			selectedType = 2;
			DialogResult = true;
			Close();
		}
	}
}
