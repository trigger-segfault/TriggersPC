using PokemonManager.PokemonStructures;
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
	/// Interaction logic for EditRowWindow.xaml
	/// </summary>
	public partial class EditRowWindow : Window {

		private ManagerPokePC pokePC;

		public EditRowWindow(ManagerPokePC pcToEdit) {
			InitializeComponent();

			this.pokePC = pcToEdit;

			if (pcToEdit != null) {
				textBoxName.Text = pcToEdit.Name;
				checkBoxLivingDex.IsChecked = pcToEdit.IsLivingDex;
				checkBoxRevealEggs.IsChecked = pcToEdit.RevealEggs;
				buttonOK.Content = "Edit Row";
			}
			else {
				textBoxName.Text = "Row " + (PokeManager.ManagerGameSave.NumPokePCRows + 1).ToString();
			}

			textBoxName.Focus();
			textBoxName.SelectAll();
		}

		private void OnOKClicked(object sender, RoutedEventArgs e) {
			if (pokePC != null) {
				pokePC.Name = textBoxName.Text;
				pokePC.IsLivingDex = checkBoxLivingDex.IsChecked.Value;
				pokePC.RevealEggs = checkBoxRevealEggs.IsChecked.Value;
			}
			else {
				PokeManager.ManagerGameSave.AddPokePCRow(textBoxName.Text, checkBoxLivingDex.IsChecked.Value);
			}
			DialogResult = true;
			Close();
		}

		public static bool ShowDialog(Window owner, ManagerPokePC pcToEdit) {
			EditRowWindow window = new EditRowWindow(pcToEdit);
			window.Owner = owner;
			var result = window.ShowDialog();
			return result.HasValue && result.Value;
		}
	}
}
