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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for PokerusWindow.xaml
	/// </summary>
	public partial class PokerusWindow : Window {

		PokerusStrain strain;

		public PokerusWindow() {
			InitializeComponent();

			this.strain = PokeManager.PokerusStrains[0];
			this.textBlockDays.Text = "This strain will last for " + ((int)this.strain.Strain + 1).ToString() + " day" + ((int)this.strain.Strain + 1 > 1 ? "s" : "") + ".";

			foreach (PokerusStrain strain in PokeManager.PokerusStrains) {
				ComboBoxItem item = new ComboBoxItem();
				item.Content = strain.ToString();
				item.Tag = strain;
				comboBoxPokerus.Items.Add(item);
			}
			comboBoxPokerus.SelectedIndex = 0;
		}

		public static PokerusStrain? ShowDialog(Window owner) {
			PokerusWindow window = new PokerusWindow();
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result.HasValue && result.Value)
				return window.strain;
			return null;
		}

		private void OKClicked(object sender, RoutedEventArgs e) {

			DialogResult = true;
		}

		private void OnStrainSelectionChanged(object sender, SelectionChangedEventArgs e) {
			strain = (PokerusStrain)((ComboBoxItem)comboBoxPokerus.SelectedItem).Tag;

			this.textBlockDays.Text = "This strain will last for " + ((int)this.strain.Strain + 1).ToString() + " day" + ((int)this.strain.Strain + 1 > 1 ? "s" : "") + ".";
		}
	}
}
