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
	public partial class SelectGameTypeFullWindow : Window {

		private GameTypes result;

		public SelectGameTypeFullWindow() {
			InitializeComponent();

			this.buttonRuby.Tag			= GameTypes.Ruby;
			this.buttonSapphire.Tag		= GameTypes.Sapphire;
			this.buttonEmerald.Tag		= GameTypes.Emerald;
			this.buttonFireRed.Tag		= GameTypes.FireRed;
			this.buttonLeafGreen.Tag	= GameTypes.LeafGreen;
		}


		public static GameTypes? ShowDialog(Window owner) {
			SelectGameTypeFullWindow form = new SelectGameTypeFullWindow();
			form.Owner = owner;
			form.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var dialogResult = form.ShowDialog();

			if (dialogResult.HasValue && dialogResult.Value) {
				return form.result;
			}
			return null;
		}

		private void OnButtonClicked(object sender, RoutedEventArgs e) {
			result = (GameTypes)(sender as Button).Tag;
			DialogResult = true;
			Close();
		}
	}
}
