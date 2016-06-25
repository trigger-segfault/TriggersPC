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
	/// Interaction logic for ControlsWindow.xaml
	/// </summary>
	public partial class ControlsWindow : Window {
		public ControlsWindow() {
			InitializeComponent();
		}

		public static void ShowDialog(Window window) {
			ControlsWindow dialog = new ControlsWindow();
			dialog.Owner = window;
			dialog.ShowDialog();
		}
	}
}
