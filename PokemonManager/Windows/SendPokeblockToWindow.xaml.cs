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
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class SendPokeblockToWindow : Window {

		private int gameIndex;
		private bool loaded;

		public SendPokeblockToWindow(int gameIndex) {
			InitializeComponent();

			loaded = false;

			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				if (i == gameIndex) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
				}

				IGameSave game = PokeManager.GetGameSaveAt(i);
				if (game.GameType != GameTypes.Ruby && game.GameType != GameTypes.Sapphire && game.GameType != GameTypes.Emerald && game.GameType != GameTypes.Any) {
					comboBoxGame.SetGameSaveVisible(i, false);
				}
			}

			this.gameIndex = PokeManager.LastGameInDialogIndex;
			if (this.gameIndex == -2 || !comboBoxGame.IsGameSaveVisible(this.gameIndex)) {
				this.gameIndex = comboBoxGame.SelectedGameIndex;
			}
			else {
				comboBoxGame.SelectedGameIndex = this.gameIndex;
			}

			loaded = true;
			GameChanged(null, null);
		}
		public static int? ShowDialog(Window owner, int gameIndex) {
			SendPokeblockToWindow window = new SendPokeblockToWindow(gameIndex);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result != null && result.Value) {
				return window.gameIndex;
			}
			return null;
		}

		private void GameChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			gameIndex = comboBoxGame.SelectedGameIndex;
			
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
			PokeManager.LastGameInDialogIndex = gameIndex;
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			if (comboBoxGame.SelectedGameIndex == -2) {
				TriggerMessageBox.Show(this, "No available games to send with", "Can't Send");
				Close();
			}
		}
	}
}
