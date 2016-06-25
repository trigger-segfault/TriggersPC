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
	public class ComboBoxPCRows : ComboBox {

		private bool suppressSelectionChanged;

		public ComboBoxPCRows()
			: base() {
			this.suppressSelectionChanged = false;

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				for (int i = 0; i < PokeManager.ManagerGameSave.NumPokePCRows; i++) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = PokeManager.ManagerGameSave.GetPokePCRow(i).Name;
					Items.Add(comboBoxItem);
				}
			}
			else {
				Items.Add("Row 1"); // Tells you what the combo box is in the designer
			}
			SelectedIndex = 0;
		}

		#region Properties

		public bool SupressSelectionChanged {
			get { return suppressSelectionChanged; }
			set { suppressSelectionChanged = value; }
		}

		#endregion

		public void RefreshRows() {
			for (int i = 0; i < Items.Count; i++) {
				(Items[i] as ComboBoxItem).Content = PokeManager.ManagerGameSave.GetPokePCRow(i).Name;
			}
				
		}
		public void ReloadRows() {
			Items.Clear();

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				for (int i = 0; i < PokeManager.ManagerGameSave.NumPokePCRows; i++) {
					ComboBoxItem comboBoxItem = new ComboBoxItem();
					comboBoxItem.Content = PokeManager.ManagerGameSave.GetPokePCRow(i).Name;
					Items.Add(comboBoxItem);
				}
			}
			SelectedIndex = 0;
		}
		public void ResetRowVisibility() {
			for (int i = 0; i < PokeManager.ManagerGameSave.NumPokePCRows; i++) {
				(Items[i] as ComboBoxItem).Visibility = Visibility.Visible;
			}
		}
		public bool IsRowVisible(int row) {
			return (Items[row] as ComboBoxItem).Visibility == Visibility.Visible;
		}
		public void SetRowVisible(int row, bool visible) {
			(Items[row] as ComboBoxItem).Visibility = (visible ? Visibility.Visible : Visibility.Collapsed);
			if (SelectedIndex == row && (SelectedItem as ComboBoxItem).Visibility == Visibility.Collapsed) {
				int newIndex = -1;
				for (int i = 0; i < Items.Count; i++) {
					if ((Items[i] as ComboBoxItem).Visibility == Visibility.Visible) {
						newIndex = i;
						break;
					}
				}
				SelectedIndex = newIndex;
			}
		}
	}
}
