using PokemonManager.Game;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
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
	/// Interaction logic for SecretBaseEditTrainerWindow.xaml
	/// </summary>
	public partial class SecretBaseEditTrainerWindow : Window {

		//private string newName;
		private byte[] newNameRaw;
		private Genders newGender;
		private ushort newTrainerID;
		private ushort newSecretID;
		private Languages newLanguage;

		private List<GBAPokemon> newPokemonTeam;

		private SharedSecretBase secretBase;

		private int teamIndex;
		private bool allowNoTeam;
		private bool loaded;
		private bool creating;

		private ContextMenu contextMenu;

		public SecretBaseEditTrainerWindow(SharedSecretBase secretBase, bool allowNoTeam, bool creating) {
			InitializeComponent();

			this.secretBase = secretBase;
			this.newNameRaw = secretBase.TrainerNameRaw;
			this.newGender = secretBase.TrainerGender;
			this.newTrainerID = secretBase.TrainerID;
			this.newSecretID = secretBase.SecretID;
			this.newLanguage = secretBase.Language;
			this.newPokemonTeam = new List<GBAPokemon>();
			this.newPokemonTeam.AddRange(secretBase.PokemonTeam);

			this.allowNoTeam = allowNoTeam;
			this.creating = creating;

			this.textBoxName.Text = secretBase.TrainerName;
			this.numericTrainerID.Value = secretBase.TrainerID;
			this.numericSecretID.Value = secretBase.SecretID;
			this.radioButtonMale.IsChecked = secretBase.TrainerGender == Genders.Male;
			this.radioButtonFemale.IsChecked = secretBase.TrainerGender == Genders.Female;

			for (int i = 0; i < 5; i++)
				this.comboBoxTrainer.Items.Add(new ComboBoxItem());

			int index = 0;
			for (Languages langauge = Languages.Japanese; langauge <= Languages.Spanish; langauge++) {
				if (langauge == Languages.Korean)
					continue;
				ComboBoxItem comboBoxItem = new ComboBoxItem();
				comboBoxItem.Content = langauge.ToString();
				comboBoxItem.Tag = langauge;
				comboBoxLanguage.Items.Add(comboBoxItem);
				if (langauge == secretBase.Language)
					comboBoxLanguage.SelectedIndex = index;
				index++;
			}

			this.comboBoxTrainer.SelectedIndex = (secretBase.TrainerID % 256) % 5;

			RefreshTrainer();
			RefreshTeam();

			this.contextMenu = new ContextMenu();
			MenuItem change = new MenuItem();
			change.Header = "Change";
			change.Click += OnSetPokemon;
			MenuItem remove = new MenuItem();
			remove.Header = "Remove";
			remove.Click += OnRemovePokemon;

			this.contextMenu.Items.Add(change);
			this.contextMenu.Items.Add(remove);

			if (secretBase.Language == Languages.Japanese)
				textBoxName.MaxLength = 5;
			else
				textBoxName.MaxLength = 7;

			loaded = true;
		}

		private string NewName {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(0, newNameRaw, 7), newLanguage); }
		}

		public static bool Show(Window owner, SharedSecretBase secretBase, bool allowNoTeam, bool creating = false) {
			SecretBaseEditTrainerWindow window = new SecretBaseEditTrainerWindow(secretBase, allowNoTeam, creating);
			window.Owner = owner;
			var result = window.ShowDialog();
			return (result.HasValue ? result.Value : false);
		}
		public static bool Show(Window owner, byte locationID, SecretBaseManager manager, bool allowNoTeam) {
			SharedSecretBase secretBase = new SharedSecretBase(locationID, manager);
			return Show(owner, secretBase, allowNoTeam, true);
		}

		private void OnTeamClicked(object sender, MouseButtonEventArgs e) {
			teamIndex = int.Parse((string)((Rectangle)sender).Tag) - 1;
			Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (teamIndex + 1).ToString());

			if (teamIndex < newPokemonTeam.Count) {
				contextMenu.IsOpen = true;
			}
			else {
				OnSetPokemon(null, null);
			}
		}

		private void OnTeamMouseEnter(object sender, MouseEventArgs e) {
			int index = int.Parse((string)((Rectangle)sender).Tag) - 1;
			Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (index + 1).ToString());
			rectTeam.Opacity = 0.5;
		}

		private void OnTeamMouseLeave(object sender, MouseEventArgs e) {
			int index = int.Parse((string)((Rectangle)sender).Tag) - 1;
			Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (index + 1).ToString());
			rectTeam.Opacity = 0;
		}

		private void OnOKClicked(object sender, RoutedEventArgs e) {
			//newNameRaw = GBACharacterEncoding.GetString(GBACharacterEncoding.GetBytes(textBoxName.Text, 7, newLanguage), newLanguage);
			if (NewName.Length == 0) {
				TriggerMessageBox.Show(this, "Trainer name cannot be empty", "Name Required");
			}
			else if (newPokemonTeam.Count != 0 || allowNoTeam) {
				secretBase.Language = newLanguage;
				secretBase.TrainerNameRaw = newNameRaw;
				secretBase.TrainerGender = newGender;
				secretBase.TrainerID = newTrainerID;
				secretBase.SecretID = newSecretID;
				secretBase.PokemonTeam.Clear();
				secretBase.PokemonTeam.AddRange(newPokemonTeam);
				DialogResult = true;
			}
			else {
				TriggerMessageBox.Show(this, "You must add at least one Pokémon to the team for this Secret Base", "Team Required");
			}
		}

		private void OnSetPokemon(object sender, RoutedEventArgs e) {
			IPokemon pokemon = SendPokemonToWindow.ShowSelectDialog(this, true);
			if (pokemon != null) {
				for (int i = 0; i < newPokemonTeam.Count; i++) {
					if (i == teamIndex)
						continue;
					if (pokemon.Personality == newPokemonTeam[i].Personality) {
						TriggerMessageBox.Show(this, "Cannot use more than one Pokémon with the same personality.", "Duplicate Personalities");
						return;
					}
				}
				if (teamIndex < newPokemonTeam.Count)
					newPokemonTeam[teamIndex] = pokemon.CreateGBAPokemon(GameTypes.Any, false);
				else
					newPokemonTeam.Add(pokemon.CreateGBAPokemon(GameTypes.Any, false));
				RefreshTeam();
			}
		}
		private void OnRemovePokemon(object sender, RoutedEventArgs e) {
			if (secretBase.SecretBaseManager != null)
				secretBase.SecretBaseManager.GameSave.IsChanged = true;
			else
				PokeManager.ManagerGameSave.IsChanged = true;
			newPokemonTeam.RemoveAt(teamIndex);
			RefreshTeam();
		}

		private void OnTrainerIDChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			newTrainerID = (ushort)numericTrainerID.Value;
			RefreshTrainer();
			loaded = false;
			comboBoxTrainer.SelectedIndex = (newTrainerID % 256) % 5;
			loaded = true;
		}

		private void OnSecretIDChanged(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			newSecretID = (ushort)numericSecretID.Value;
		}

		private void OnTrainerChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;

			int oldIndex = ((int)newTrainerID % 256) % 5;
			int newIndex = comboBoxTrainer.SelectedIndex;

			if ((int)newTrainerID % 256 == 255 && newIndex != 0)
				newTrainerID -= 5;
			if (newIndex < oldIndex)
				newTrainerID -= (ushort)(oldIndex - newIndex);
			else if (newIndex > oldIndex)
				newTrainerID += (ushort)(newIndex - oldIndex);

			loaded = false;
			numericTrainerID.Value = newTrainerID;
			loaded = true;
			RefreshTrainer();
		}

		private void OnMaleChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			newGender = Genders.Male;
			radioButtonMale.IsChecked = true;
			radioButtonFemale.IsChecked = false;
			RefreshTrainer();
		}

		private void OnFemaleChecked(object sender, RoutedEventArgs e) {
			if (!loaded) return;
			newGender = Genders.Female;
			radioButtonMale.IsChecked = false;
			radioButtonFemale.IsChecked = true;
			RefreshTrainer();
		}

		private void RefreshTeam() {
			for (int i = 0; i < 6; i++) {
				Image imageTeam = (Image)FindName("imageTeam" + (i + 1).ToString());
				Rectangle rectTeam = (Rectangle)FindName("rectTeam" + (i + 1).ToString());
				Label labelTeam = (Label)FindName("labelTeam" + (i + 1).ToString());
				if (i < newPokemonTeam.Count) {
					imageTeam.Source = PokemonDatabase.GetPokemonBoxImageFromDexID(newPokemonTeam[i].DexID, false, newPokemonTeam[i].FormID);
					rectTeam.OpacityMask = new ImageBrush(imageTeam.Source);
					labelTeam.Content = newPokemonTeam[i].Level.ToString();

					ToolTip tooltip = new ToolTip();
					string content = "";
					if (newPokemonTeam[i].IsHoldingItem) {
						content = "Holding: " + newPokemonTeam[i].HeldItemData.Name + "\n";
					}
					content += newPokemonTeam[i].Move1Data.Name;
					for (int j = 1; j < 4; j++) {
						if (newPokemonTeam[i].GetMoveIDAt(j) != 0)
							content += "\n" + newPokemonTeam[i].GetMoveDataAt(j).Name;
					}
					tooltip.Content = content;
					rectTeam.ToolTip = tooltip;
				}
				else {
					imageTeam.Source = null;
					rectTeam.OpacityMask = new ImageBrush(ResourceDatabase.GetImageFromName("TeamBallBackground"));
					rectTeam.ToolTip = null;
					labelTeam.Content = "";
				}
			}
		}
		private void RefreshTrainer() {
			string[] maleTrainers = { "Youngster", "Bug Catcher", "Rich Boy", "Camper", "Cooltrainer" };
			string[] femaleTrainers = { "Lass", "School Kid", "Lady", "Picnicker", "Cooltrainer" };

			for (int i = 0; i < 5; i++) {
				((ComboBoxItem)comboBoxTrainer.Items[i]).Content = (newGender == Genders.Male ? maleTrainers[i] : femaleTrainers[i]);
			}

			imageTrainer.Source = ResourceDatabase.GetImageFromName(newGender.ToString() + "SecretBaseLarge" + (((int)newTrainerID % 256) % 5).ToString());
		}

		private void OnLanguageChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded) return;


			if (newLanguage == Languages.Japanese)
				textBoxName.MaxLength = 5;
			else
				textBoxName.MaxLength = 7;

			newLanguage = (Languages)((ComboBoxItem)comboBoxLanguage.SelectedItem).Tag;
			textBoxName.Text = NewName;
		}

		private void OnNameChanged(object sender, TextChangedEventArgs e) {
			if (!loaded) return;

			newNameRaw = GBACharacterEncoding.GetBytes(textBoxName.Text, 7, newLanguage);
		}

		private void OnNameEndTyping(object sender, RoutedEventArgs e) {
			if (!loaded) return;

			//newNameRaw = GBACharacterEncoding.GetBytes(textBoxName.Text, 7, newLanguage);
			textBoxName.Text = NewName;
		}
	}
}
