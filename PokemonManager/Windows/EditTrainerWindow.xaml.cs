using Microsoft.Win32;
using PokemonManager.Game;
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
	public partial class EditTrainerWindow : Window {

		private string trainerName;
		private Genders trainerGender;

		public EditTrainerWindow() {
			InitializeComponent();
			this.trainerName = PokeManager.ManagerGameSave.TrainerName;
			this.trainerGender = PokeManager.ManagerGameSave.TrainerGender;
			this.textBoxName.Text = trainerName;
			BitmapSource trainerImage = PokeManager.TrainerImage;
			imageTrainer.Width = Math.Min(90, trainerImage.PixelWidth);
			imageTrainer.Height = Math.Min(138, trainerImage.PixelHeight);
			imageTrainer.Source = trainerImage;
			this.labelTrainerID.Content = PokeManager.ManagerGameSave.TrainerID.ToString("00000");
			this.labelSecretID.Content = PokeManager.ManagerGameSave.SecretID.ToString("00000");

			radioButtonMale.IsChecked = PokeManager.ManagerGameSave.TrainerGender == Genders.Male;
			radioButtonFemale.IsChecked = PokeManager.ManagerGameSave.TrainerGender == Genders.Female;

			this.textBoxName.Focus();
			this.textBoxName.SelectAll();

			this.buttonRemoveImage.IsEnabled = PokeManager.CustomTrainerImage != null;

			this.Title = "Edit " + PokeManager.Settings.ManagerNickname + " Trainer";
		}


		private void OnNameChanged(object sender, TextChangedEventArgs e) {
			trainerName = textBoxName.Text;
		}

		public static bool? ShowDialog(Window owner) {
			EditTrainerWindow window = new EditTrainerWindow();
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OnSetImageClicked(object sender, RoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Image Files (*.png, *.jpg, *.bmp)|*.png;*.bmp;*.jpg|All Files (*.*)|*.*";
			var result = dialog.ShowDialog();
			if (result.HasValue && result.Value) {
				PokeManager.TryCreateDirectory(System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources"));
				PokeManager.TryCreateDirectory(System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Trainer"));
				try {
					BitmapSource bitmap = PokeManager.LoadImage(dialog.FileName);
					try {
						PokeManager.SaveImage(bitmap, System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Trainer", "Trainer.png"));
						PokeManager.CustomTrainerImage = bitmap;
						imageTrainer.Width = Math.Min(90, bitmap.PixelWidth);
						imageTrainer.Height = Math.Min(138, bitmap.PixelHeight);
						imageTrainer.Source = bitmap;
						this.buttonRemoveImage.IsEnabled = PokeManager.CustomTrainerImage != null;
					}
					catch (Exception ex) {
						TriggerMessageBox.Show(this, "Error saving trainer image to Trigger's PC directory", "Image Error");
					}
				}
				catch (Exception ex) {
					TriggerMessageBox.Show(this, "Error loading trainer image", "Image Error");
				}
			}
		}

		private void OnRemoveImageClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.CustomTrainerImage != null) {
				try {
					if (File.Exists(System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Trainer", "Trainer.png")))
						File.Delete(System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Trainer", "Trainer.png"));
				}
				catch (Exception ex) {
					TriggerMessageBox.Show(this, "Error trying to delete custom trainer image file", "File Error");
				}
				PokeManager.CustomTrainerImage = null;
				BitmapSource trainerImage = PokeManager.TrainerImage;
				imageTrainer.Width = Math.Min(90, trainerImage.PixelWidth);
				imageTrainer.Height = Math.Min(138, trainerImage.PixelHeight);
				imageTrainer.Source = trainerImage;
				this.buttonRemoveImage.IsEnabled = false;
			}
		}

		private void OnMaleGenderClicked(object sender, RoutedEventArgs e) {
			radioButtonMale.IsChecked = true;
			radioButtonFemale.IsChecked = false;
			trainerGender = Genders.Male;
			if (PokeManager.CustomTrainerImage == null) {
				BitmapSource trainerImage = ResourceDatabase.GetImageFromName("Manager" + trainerGender.ToString());
				imageTrainer.Width = Math.Min(90, trainerImage.PixelWidth);
				imageTrainer.Height = Math.Min(138, trainerImage.PixelHeight);
				imageTrainer.Source = trainerImage;
			}
		}

		private void OnFemaleGenderClicked(object sender, RoutedEventArgs e) {
			radioButtonMale.IsChecked = false;
			radioButtonFemale.IsChecked = true;
			trainerGender = Genders.Female;
			if (PokeManager.CustomTrainerImage == null) {
				BitmapSource trainerImage = ResourceDatabase.GetImageFromName("Manager" + trainerGender.ToString());
				imageTrainer.Width = Math.Min(90, trainerImage.PixelWidth);
				imageTrainer.Height = Math.Min(138, trainerImage.PixelHeight);
				imageTrainer.Source = trainerImage;
			}
		}

		private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			PokeManager.ManagerGameSave.TrainerGender = trainerGender;
			byte[] trainerNameBytes = GBACharacterEncoding.GetBytes(trainerName, 7);
			PokeManager.ManagerGameSave.TrainerName = GBACharacterEncoding.GetString(trainerNameBytes);
			PokeManager.RefreshUI();
		}
	}
}
