using Microsoft.Win32;
using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.IO;
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
	/// Interaction logic for ChangeNickname.xaml
	/// </summary>
	public partial class EditGameTrainerWindow : Window {

		private string trainerName;
		private Genders trainerGender;
		private IGameSave gameSave;

		public EditGameTrainerWindow(IGameSave gameSave) {
			InitializeComponent();
			this.gameSave = gameSave;
			this.trainerName = gameSave.TrainerName;
			this.trainerGender = gameSave.TrainerGender;
			this.textBoxName.Text = trainerName;

			UpdateTrainerImage();

			this.labelTrainerID.Content = gameSave.TrainerID.ToString("00000");
			this.labelSecretID.Content = gameSave.SecretID.ToString("00000");

			radioButtonMale.IsChecked = gameSave.TrainerGender == Genders.Male;
			radioButtonFemale.IsChecked = gameSave.TrainerGender == Genders.Female;

			if (gameSave.IsJapanese) {
				this.textBoxName.MaxLength = 5;
			}

			this.textBoxName.Focus();
			this.textBoxName.SelectAll();

			if (gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD) {
				radioButtonMale.IsEnabled = false;
				radioButtonFemale.IsEnabled = false;
			}

			this.Title = "Edit " + gameSave.GameType.ToString() + " Trainer";
		}

		private void UpdateTrainerImage() {
			BitmapSource trainerImage = PokeManager.TrainerImage;
			if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire) {
				trainerImage = ResourceDatabase.GetImageFromName("RubySapphire" + trainerGender.ToString());
			}
			else if (gameSave.GameType == GameTypes.Emerald) {
				trainerImage = ResourceDatabase.GetImageFromName("Emerald" + trainerGender.ToString());
			}
			else if (gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen) {
				trainerImage = ResourceDatabase.GetImageFromName("FireRedLeafGreen" + trainerGender.ToString());
			}
			else if (gameSave.GameType == GameTypes.Colosseum) {
				trainerImage = ResourceDatabase.GetImageFromName("ColosseumMale");
			}
			else if (gameSave.GameType == GameTypes.XD) {
				trainerImage = ResourceDatabase.GetImageFromName("XDMale");
			}
			imageTrainer.Width = Math.Min(90, trainerImage.PixelWidth);
			imageTrainer.Height = Math.Min(138, trainerImage.PixelHeight);
			imageTrainer.Source = trainerImage;
		}

		private void OnNameChanged(object sender, TextChangedEventArgs e) {
			trainerName = textBoxName.Text;
		}

		private void OnMaleGenderClicked(object sender, RoutedEventArgs e) {
			radioButtonMale.IsChecked = true;
			radioButtonFemale.IsChecked = false;
			trainerGender = Genders.Male;
			UpdateTrainerImage();
		}

		private void OnFemaleGenderClicked(object sender, RoutedEventArgs e) {
			radioButtonMale.IsChecked = false;
			radioButtonFemale.IsChecked = true;
			trainerGender = Genders.Female;
			UpdateTrainerImage();
		}

		public static bool? ShowDialog(Window owner, IGameSave gameSave) {
			EditGameTrainerWindow window = new EditGameTrainerWindow(gameSave);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OnSaveClicked(object sender, RoutedEventArgs e) {
			byte[] trainerNameBytes = GBACharacterEncoding.GetBytes(trainerName, 7);
			string newTrainerName = GBACharacterEncoding.GetString(trainerNameBytes);
			bool setName = newTrainerName != gameSave.TrainerName;
			bool setGender = trainerGender != gameSave.TrainerGender;

			if (setName || setGender) {
				MessageBoxResult result = TriggerMessageBox.Show(this, "Warning: Renaming a trainer or changing their gender will also change the OT Trainer of every Pokémon caught by this trainer in all saves. Are you sure you want to continue?", "Warning", MessageBoxButton.YesNo);

				if (result == MessageBoxResult.Yes) {
					if (setName)
						gameSave.TrainerName = newTrainerName;
					if (setGender)
						gameSave.TrainerGender = trainerGender;

					for (int i = -1; i < PokeManager.NumGameSaves; i++) {
						IGameSave newGameSave = PokeManager.GetGameSaveAt(i);
						if (newGameSave is ManagerGameSave) {
							for (int j = 0; j < (newGameSave as ManagerGameSave).NumPokePCRows; j++) {
								foreach (IPokemon pokemon in (newGameSave as ManagerGameSave).GetPokePCRow(j)) {
									if (pokemon.TrainerID == gameSave.TrainerID && pokemon.SecretID == gameSave.SecretID) {
										if (setName)
											pokemon.TrainerName = newTrainerName;
										if (setGender)
											pokemon.TrainerGender = trainerGender;
									}
								}
							}
						}
						else {
							foreach (IPokemon pokemon in newGameSave.PokePC) {
								if (pokemon.TrainerID == gameSave.TrainerID && pokemon.SecretID == gameSave.SecretID) {
									if (setName)
										pokemon.TrainerName = newTrainerName;
									if (setGender)
										pokemon.TrainerGender = trainerGender;
								}
							}
						}
					}
					gameSave.IsChanged = true;
					PokeManager.RefreshUI();
				}
			}
			DialogResult = true;
		}
	}
}
