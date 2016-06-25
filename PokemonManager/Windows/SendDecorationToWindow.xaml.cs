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
	public partial class SendDecorationToWindow : Window {

		private uint count;
		private int gameIndex;
		private DecorationTypes decorationType;
		private byte decorationID;
		private bool loaded;

		public SendDecorationToWindow(int gameIndex, byte id, uint maxCount) {
			InitializeComponent();
			upDownItemCount.Minimum = 0;
			upDownItemCount.Value = 1;
			upDownItemCount.Maximum = (int)maxCount;
			count = 1;
			decorationID = id;
			loaded = false;


			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				if (i == gameIndex) {
					comboBoxGame.SetGameSaveVisible(i, false);
					continue;
				}

				IGameSave game = PokeManager.GetGameSaveAt(i);
				if (game.GameType != GameTypes.Any && game.GameType != GameTypes.Ruby && game.GameType != GameTypes.Sapphire && game.GameType != GameTypes.Emerald) {
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
		public static SendDecorationToResult ShowDialog(Window owner, int gameIndex, byte id, uint maxCount) {
			SendDecorationToWindow window = new SendDecorationToWindow(gameIndex, id, maxCount);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result != null && result.Value) {
				SendDecorationToResult sendDecorationResult = new SendDecorationToResult();
				sendDecorationResult.Count = window.count;
				sendDecorationResult.GameIndex = window.gameIndex;
				return sendDecorationResult;
			}
			return null;
		}

		private void OnValueChanged(object sender, RoutedEventArgs e) {
			count = (uint)((NumericUpDown)sender).Value;
		}

		private void GameChanged(object sender, SelectionChangedEventArgs e) {
			gameIndex = comboBoxGame.SelectedGameIndex;
			if (!loaded)
				return;
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
			comboBoxGame.SelectedGameIndex = gameIndex;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			this.upDownItemCount.SelectAll();
			if (comboBoxGame.SelectedGameIndex == -2) {
				TriggerMessageBox.Show(this, "No available games to send with", "Can't Send");
				Close();
			}
		}
	}

	public class SendDecorationToResult {

		public uint Count { get; set; }
		public int GameIndex { get; set; }
	}

}
