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
	public partial class ItemCountWindow : Window {

		int? result;

		public ItemCountWindow(string title, int current, int max) {
			InitializeComponent();
			this.Title = title;
			numericUpDown.Minimum = 0;
			numericUpDown.Value = current;
			numericUpDown.Maximum = max;
			result = null;

			numericUpDown.SelectAll();
		}


		public static int? ShowDialog(Window owner, string title, int current, int max) {
			ItemCountWindow form = new ItemCountWindow(title, current, max);
			form.Owner = owner;
			form.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var dialogResult = form.ShowDialog();

			if (dialogResult != null && dialogResult.Value) {
				return form.result;
			}
			return null;
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			Application curApp = Application.Current;
			Window mainWindow = curApp.MainWindow;
			this.Left = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
			this.Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;

			numericUpDown.Focusable = true;
			numericUpDown.Focus();
			numericUpDown.SelectAll();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			
		}

		private void OnOKClicked(object sender, RoutedEventArgs e) {
			result = numericUpDown.Value;
			DialogResult = true;
		}

		private void OnValueChanged(object sender, RoutedEventArgs e) {
			result = numericUpDown.Value;
		}
	}
}
