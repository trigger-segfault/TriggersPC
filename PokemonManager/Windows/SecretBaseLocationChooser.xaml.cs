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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SecretBaseLocationChooser.xaml
	/// </summary>
	public partial class SecretBaseLocationChooser : Window {
		private SecretBaseManager manager;
		private byte originalLocation;
		private byte location;
		private bool startInvalid;

		public SecretBaseLocationChooser(byte location, SecretBaseManager manager, bool startInvalid) {
			InitializeComponent();

			this.originalLocation = location;
			this.location = (location == 0 ? (byte)181 : location);
			this.manager = manager;
			this.startInvalid = startInvalid;

			for (int i = 0; i < SecretBaseDatabase.NumLocations; i++) {
				LocationData locationData = SecretBaseDatabase.GetLocationAt(i);

				ListViewItem listViewItem = new ListViewItem();
				listViewItem.Tag = locationData.ID;
				StackPanel stackPanel = new StackPanel();
				stackPanel.Orientation = Orientation.Horizontal;
				Grid grid = new Grid();
				grid.Width = 16;
				grid.Height = 16;

				Image type = new Image();
				type.Width = 16;
				type.Height = 16;
				type.HorizontalAlignment = HorizontalAlignment.Center;
				type.VerticalAlignment = VerticalAlignment.Center;
				type.Source = ResourceDatabase.GetImageFromName("SecretBaseType" + locationData.Type.ToString());

				Label layout = new Label();
				layout.Padding = new Thickness(0, 0, 0, 0);
				layout.Width = 16;
				layout.Height = 16;
				layout.HorizontalAlignment = HorizontalAlignment.Center;
				layout.VerticalAlignment = VerticalAlignment.Center;
				layout.HorizontalContentAlignment = HorizontalAlignment.Center;
				layout.VerticalContentAlignment = VerticalAlignment.Center;
				layout.Content = locationData.Layout.ToString();
				layout.FontWeight = FontWeights.Bold;
				//layout.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

				grid.Children.Add(type);
				grid.Children.Add(layout);

				Label label = new Label();
				label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				label.Padding = new Thickness(4, 0, 4, 0);
				label.Content = "Route " + locationData.RouteData.ID;
				if ((startInvalid || originalLocation != locationData.ID) && manager != null && manager.IsLocationInUse(locationData.ID)) {
					label.Foreground = new SolidColorBrush(Color.FromRgb(220, 0, 0));
				}
				else if (OriginalLocationData != null && locationData.Type == OriginalLocationData.Type && locationData.Layout == OriginalLocationData.Layout) {
					label.Foreground = new SolidColorBrush(Color.FromRgb(0, 140, 60));
					label.FontWeight = FontWeights.Bold;
				}

				stackPanel.Children.Add(grid);
				stackPanel.Children.Add(label);
				listViewItem.Content = stackPanel;
				listViewSecretBases.Items.Add(listViewItem);
				if (locationData.ID == this.location) {
					listViewSecretBases.SelectedIndex = listViewSecretBases.Items.Count - 1;
				}
			}
			if (listViewSecretBases.SelectedIndex == -1)
				OnLocationSelected(null, null);
		}

		private void OnOKClicked(object sender, RoutedEventArgs e) {

			DialogResult = true;
		}

		private LocationData LocationData {
			get { return SecretBaseDatabase.GetLocationFromID(location); }
		}
		private LocationData OriginalLocationData {
			get { return SecretBaseDatabase.GetLocationFromID(originalLocation); }
		}

		public static byte? Show(Window owner, byte location, SecretBaseManager manager = null, bool startInvalid = false) {
			SecretBaseLocationChooser window = new SecretBaseLocationChooser(location, manager, startInvalid);
			window.Owner = owner;
			window.ShowDialog();
			if (window.DialogResult.HasValue && window.DialogResult.Value)
				return window.location;
			return null;
		}

		private void OnLocationSelected(object sender, SelectionChangedEventArgs e) {
			if (listViewSecretBases.SelectedIndex != -1) {
				location = (byte)(listViewSecretBases.SelectedItem as ListViewItem).Tag;
				labelRequires.Content = LocationData.Requirements ?? "Nothing";
				locationDisplay.LoadLocation(location);
				roomDisplay.LoadSecretBase(new SharedSecretBase(location, null));
				if ((startInvalid || originalLocation != LocationData.ID) && manager != null && manager.IsLocationInUse(location)) {
					buttonOK.IsEnabled = false;
					labelCantSelect.Visibility = Visibility.Visible;
					labelSameRoom.Visibility = Visibility.Hidden;
				}
				else {
					labelCantSelect.Visibility = Visibility.Hidden;
					buttonOK.IsEnabled = true;
					if (OriginalLocationData != null && LocationData.Type == OriginalLocationData.Type && LocationData.Layout == OriginalLocationData.Layout) {
						labelSameRoom.Visibility = Visibility.Visible;
					}
					else {
						labelSameRoom.Visibility = Visibility.Hidden;
					}
				}
			}
		}
	}
}
