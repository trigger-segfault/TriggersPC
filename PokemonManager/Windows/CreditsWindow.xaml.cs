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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for CreditsWindow.xaml
	/// </summary>
	public partial class CreditsWindow : Window {
		public CreditsWindow() {
			InitializeComponent();
		}

		public static void Show(Window window) {
			CreditsWindow form = new CreditsWindow();
			form.Owner = window;
			form.ShowDialog();
		}

		private void OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
			System.Diagnostics.Process.Start((sender as Hyperlink).NavigateUri.ToString());
		}
	}
}
