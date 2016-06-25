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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SecretBaseLocationDisplay.xaml
	/// </summary>
	public partial class SecretBaseLocationDisplay : UserControl {
		public SecretBaseLocationDisplay() {
			InitializeComponent();
		}

		public void LoadLocation(byte id) {
			LocationData locationData = SecretBaseDatabase.GetLocationFromID(id);

			imageRouteSign.Visibility = Visibility.Visible;
			imageLocation.Visibility = Visibility.Visible;
			labelRoute.Visibility = Visibility.Visible;
			imageLocation.Source = locationData.Image;
			imageRouteSign.Source = ResourceDatabase.GetImageFromName("RouteSign" + (locationData.RouteData.ID >= 124 ? "Water" : "Land"));
			labelRoute.Content = "Route " + locationData.RouteData.ID;

			ToolTip routeTooltip = new ToolTip();

			BitmapSource routeImage = locationData.RouteData.Image;

			Grid tooltip = new Grid();
			tooltip.Width = routeImage.PixelWidth / 8 + 10;
			tooltip.Height = routeImage.PixelHeight / 8 + 10;

			Image route = new Image();
			route.HorizontalAlignment = HorizontalAlignment.Left;
			route.VerticalAlignment = VerticalAlignment.Top;
			route.Width = routeImage.PixelWidth / 8;
			route.Height = routeImage.PixelHeight / 8;
			route.Margin = new Thickness(5, 5, 5, 5);
			route.Stretch = Stretch.Uniform;
			route.Source = routeImage;

			Image pinpoint = new Image();
			pinpoint.HorizontalAlignment = HorizontalAlignment.Left;
			pinpoint.VerticalAlignment = VerticalAlignment.Top;
			pinpoint.Width = 12;
			pinpoint.Height = 12;
			pinpoint.Source = ResourceDatabase.GetImageFromName("RouteLocationSelector");
			pinpoint.Margin = new Thickness(locationData.MapX * 2, locationData.MapY * 2, 0, 0);

			tooltip.Children.Add(route);
			tooltip.Children.Add(pinpoint);
			imageLocation.ToolTip = tooltip;
		}

		public void UnloadLocation() {
			imageRouteSign.Visibility = Visibility.Hidden;
			imageLocation.Visibility = Visibility.Hidden;
			labelRoute.Visibility = Visibility.Hidden;
		}
	}
}
