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
	public class GameTypeResults {
		public GameTypes GameType { get; set; }
		public bool IsJapanese { get; set; }
	}

	public partial class SelectGameTypeFullWindow : Window {

		private GameTypes result;
		private bool isJapanese;

		public SelectGameTypeFullWindow(bool isJapanese) {
			InitializeComponent();

			this.buttonRuby.Tag			= GameTypes.Ruby;
			this.buttonSapphire.Tag		= GameTypes.Sapphire;
			this.buttonEmerald.Tag		= GameTypes.Emerald;
			this.buttonFireRed.Tag		= GameTypes.FireRed;
			this.buttonLeafGreen.Tag	= GameTypes.LeafGreen;

			this.isJapanese = isJapanese;
			this.checkBoxJapanese.IsChecked = isJapanese;
		}


		public static GameTypeResults ShowDialog(Window owner, bool isJapanese) {
			SelectGameTypeFullWindow form = new SelectGameTypeFullWindow(isJapanese);
			form.Owner = owner;
			form.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var dialogResult = form.ShowDialog();

			if (dialogResult.HasValue && dialogResult.Value) {
				GameTypeResults results = new GameTypeResults();
				results.GameType = form.result;
				results.IsJapanese = form.isJapanese;
				return results;
			}
			return null;
		}

		private void OnButtonClicked(object sender, RoutedEventArgs e) {
			result = (GameTypes)(sender as Button).Tag;
			DialogResult = true;
			Close();
		}

		private void OnJapaneseChecked(object sender, RoutedEventArgs e) {
			isJapanese = checkBoxJapanese.IsChecked.Value;
		}
	}
}
