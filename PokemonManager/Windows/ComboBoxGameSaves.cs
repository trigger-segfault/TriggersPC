using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokemonManager.Windows {
	public class ComboBoxGameSaves : ComboBox {

		private bool suppressSelectionChanged;

		public ComboBoxGameSaves() : base() {
			this.suppressSelectionChanged = false;

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				for (int i = -1; i < PokeManager.NumGameSaves; i++) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					FillComboBoxItem(i, comboBoxItem);
					Items.Add(comboBoxItem);
				}
			}
			else {
				Items.Add("Your PC"); // Tells you what the combo box is in the designer
			}
			SelectedIndex = 0;
		}

		#region Properties

		public bool IsManagerSelected {
			get {
				if (DesignerProperties.GetIsInDesignMode(this))
					return false;
				if (SelectedIndex == -1)
					return false;
				return ((int)((ComboBoxItem)SelectedItem).Tag) == -1;
			}
		}
		public IGameSave SelectedGameSave {
			get {
				if (SelectedIndex == -1 || DesignerProperties.GetIsInDesignMode(this))
					return null;
				int gameIndex = (int)((ComboBoxItem)SelectedItem).Tag;
				return PokeManager.GetGameSaveAt(gameIndex);
			}
		}
		public int SelectedGameIndex {
			get {
				if (SelectedIndex == -1 || DesignerProperties.GetIsInDesignMode(this))
					return -2;
				return (int)((ComboBoxItem)SelectedItem).Tag;
			}
			set {
				SelectedIndex = value + 1;
				/*for (int i = 0; i < Items.Count; i++) {
					if (value == (int)((ComboBoxItem)Items[i]).Tag) {
						SelectedIndex = i;
						break;
					}
				}*/
			}
		}
		public bool SupressSelectionChanged {
			get { return suppressSelectionChanged; }
			set { suppressSelectionChanged = value; }
		}

		#endregion

		public void RefreshGameSaves() {
			for (int i = 0; i < Items.Count; i++) {
				int currentIndex = (int)((ComboBoxItem)Items[i]).Tag;
				ComboBoxItem comboBoxItem = Items[i] as ComboBoxItem;
				FillComboBoxItem(currentIndex, comboBoxItem);
			}
				
		}
		public void ReloadGameSaves(bool keepIndex = false) {
			int previousSelectedIndex = SelectedIndex;
			bool hasPreviousIndex = false;
			Items.Clear();

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				for (int i = -1; i < PokeManager.NumGameSaves; i++) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					FillComboBoxItem(i, comboBoxItem);
					Items.Add(comboBoxItem);
					if (i + 1 == previousSelectedIndex)
						hasPreviousIndex = true;
				}
			}
			if (keepIndex && hasPreviousIndex)
				SelectedIndex = previousSelectedIndex;
			else
				SelectedIndex = 0;
		}
		public void ResetGameSaveVisibility() {
			for (int i = -1; i < PokeManager.NumGameSaves; i++) {
				(Items[i + 1] as ComboBoxItem).Visibility = Visibility.Visible;
				//AddComboBoxItem(i);
			}
			//SelectedIndex = 0;
		}
		public bool IsGameSaveVisible(int gameIndex) {
			return (Items[gameIndex + 1] as ComboBoxItem).Visibility == Visibility.Visible;
			/*foreach (object item in Items) {
				int currentIndex = (int)((ComboBoxItem)item).Tag;
				if (currentIndex == gameIndex)
					return true;
				else if (currentIndex > gameIndex)
					return false;
			}
			return false;*/
		}
		public void SetGameSaveVisible(int gameIndex, bool visible) {
			(Items[gameIndex + 1] as ComboBoxItem).Visibility = (visible ? Visibility.Visible : Visibility.Collapsed);
			if (SelectedIndex == gameIndex + 1 && (SelectedItem as ComboBoxItem).Visibility == Visibility.Collapsed) {
				int newIndex = -1;
				for (int i = 0; i < Items.Count; i++) {
					if ((Items[i] as ComboBoxItem).Visibility == Visibility.Visible) {
						newIndex = i;
						break;
					}
				}
				SelectedIndex = newIndex;
			}
			/*if (visible)
				AddComboBoxItem(gameIndex);
			else
				RemoveComboBoxItem(gameIndex);*/
		}

		#region Private

		private void FillComboBoxItem(int gameIndex, ComboBoxItem comboBoxItem) {
			IGameSave gameSave = PokeManager.GetGameSaveAt(gameIndex);

			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Horizontal;

			/*Image gameImage = new Image();
			BitmapImage bitmap = ResourceDatabase.GetImageFromName(gameSave.GameType.ToString() + "Physical");
			gameImage.Source = bitmap;
			if (bitmap == null)
				gameImage.Width = 20;
			else
				gameImage.Width = bitmap.PixelWidth * (20.0 / bitmap.PixelHeight);
			gameImage.Height = 20;
			gameImage.VerticalAlignment = VerticalAlignment.Center;*/

			TextBlock gameName = new TextBlock();
			string gameTypeName = (gameSave.GameType == GameTypes.PokemonBox ? "Pokémon Box" : gameSave.GameType.ToString());
			if (PokeManager.GetGameSaveFileInfoNickname(gameIndex) != "")
				gameName.Text = PokeManager.GetGameSaveFileInfoNickname(gameIndex) + (gameSave.GameType != GameTypes.PokemonBox && gameSave.GameType != GameTypes.Any ? " [" : "");
			else
				gameName.Text = gameTypeName + (gameSave.GameType != GameTypes.PokemonBox && gameSave.GameType != GameTypes.Any ? " [" : "");
			gameName.VerticalAlignment = VerticalAlignment.Center;
			//gameName.Margin = new Thickness(5, 0, 0, 0);

			TextBlock trainerName = new TextBlock();
			trainerName.Text = gameSave.TrainerName;
			trainerName.Foreground = new SolidColorBrush(gameSave.TrainerGender == Genders.Male ? Color.FromRgb(32, 128, 248) : (gameSave.TrainerGender == Genders.Female ? Color.FromRgb(248, 24, 168) : Color.FromRgb(0, 0, 0)));
			trainerName.VerticalAlignment = VerticalAlignment.Center;

			TextBlock ending = new TextBlock();
			ending.VerticalAlignment = VerticalAlignment.Center;
			ending.Text = "]";

			//stackPanel.Children.Add(gameImage);
			stackPanel.Children.Add(gameName);
			if (gameSave.GameType != GameTypes.PokemonBox && gameSave.GameType != GameTypes.Any) {
				stackPanel.Children.Add(trainerName);
				stackPanel.Children.Add(ending);
			}

			comboBoxItem.Content = stackPanel;
			comboBoxItem.Tag = gameIndex;
		}
		private void AddComboBoxItem(int gameIndex) {
			for (int i = 0; i <= Items.Count; i++) {
				int currentIndex = -2;
				if (i != Items.Count) {
					currentIndex = (int)((ComboBoxItem)Items[i]).Tag;
				}
				if (currentIndex == gameIndex) {
					break; // Already exists
				}
				else if (currentIndex > gameIndex) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					FillComboBoxItem(gameIndex, comboBoxItem);
					Items.Insert(i, comboBoxItem);
				}
			}
		}
		private void RemoveComboBoxItem(int gameIndex) {
			foreach (object item in Items) {
				int currentIndex = (int)((ComboBoxItem)item).Tag;
				if (currentIndex == gameIndex) {
					Items.Remove(item);
					break;
				}
				else if (currentIndex > gameIndex) {
					break;
				}
			}
		}

		#endregion
	}
}
