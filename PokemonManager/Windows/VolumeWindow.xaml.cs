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
	/// Interaction logic for VolumeWindow.xaml
	/// </summary>
	public partial class VolumeWindow : Window {

		private bool newMuted;
		private double newVolume;

		public VolumeWindow() {
			InitializeComponent();
			sliderVolume.Value = PokeManager.Settings.Volume * 100.0;
			checkboxMuted.IsChecked = PokeManager.Settings.IsMuted;
			newMuted = PokeManager.Settings.IsMuted;
			newVolume = PokeManager.Settings.Volume;
			labelVolume.Content = "Volume: " + ((int)(newVolume * 100)).ToString() + "%";
		}

		public static void ShowDialog(Window window) {
			VolumeWindow form = new VolumeWindow();
			form.Owner = window;
			form.ShowDialog();
		}

		private void OnMuteChecked(object sender, RoutedEventArgs e) {
			newMuted = checkboxMuted.IsChecked.Value;
		}

		private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			newVolume = sliderVolume.Value / 100.0;
			labelVolume.Content = "Volume: " + ((int)(newVolume * 100)).ToString() + "%";
		}

		private void OnOKClicked(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			PokeManager.Settings.DisableChangesWhileLoading = true;
			PokeManager.Settings.IsMuted = newMuted;
			PokeManager.Settings.Volume = newVolume;
			PokeManager.Settings.DisableChangesWhileLoading = false;
			PokeManager.SaveSettings();
		}
	}
}
