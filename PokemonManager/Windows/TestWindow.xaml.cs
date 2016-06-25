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
	/// Interaction logic for TestWindow.xaml
	/// </summary>
	public partial class TestWindow : Window {

		private int index;

		public TestWindow() {
			InitializeComponent();

			index = 0;

			RefreshUI();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			index++;
			if (index >= SecretBaseDatabase.NumRooms)
				index = 0;

			RefreshUI();
		}

		public void RefreshUI() {
			gridContents.Children.Clear();

			RoomData roomData = SecretBaseDatabase.GetRoomAt(index);


			Image room = new Image();
			room.HorizontalAlignment = HorizontalAlignment.Left;
			room.VerticalAlignment = VerticalAlignment.Top;
			room.Width = roomData.Image.PixelWidth;
			room.Height = roomData.Image.PixelHeight;
			room.Source = roomData.Image;

			gridContents.Children.Add(room);

			for (int x = 0; x < roomData.Width; x++) {
				for (int y = 0; y < roomData.Height; y++) {
					Rectangle type = new Rectangle();
					type.Width = 16;
					type.Height = 16;
					type.HorizontalAlignment = HorizontalAlignment.Left;
					type.VerticalAlignment = VerticalAlignment.Top;
					type.StrokeThickness = 0;
					type.Margin = new Thickness(16 * x, 16 * y, 0, 0);
					type.Fill = GetTypeColor(roomData.PlacementGrid[x, y]);
					gridContents.Children.Add(type);
				}
			}

			Image trainer = new Image();
			trainer.HorizontalAlignment = HorizontalAlignment.Left;
			trainer.VerticalAlignment = VerticalAlignment.Top;
			trainer.Width = 16;
			trainer.Height = 24;
			trainer.Margin = new Thickness(16 * roomData.TrainerX, 16 * roomData.TrainerY - 8, 0, 0);
			trainer.Source = ResourceDatabase.GetImageFromName("MaleSecretBase0");
			gridContents.Children.Add(trainer);
		}

		public Brush GetTypeColor(SecretBasePlacementTypes type) {
			byte opacity = 90;
			Color color = new Color();
			switch (type) {
			case SecretBasePlacementTypes.Blocked: color = Color.FromArgb(opacity, 255, 0, 0); break;
			case SecretBasePlacementTypes.Floor: color = Color.FromArgb(opacity, 255, 255, 255); break;
			case SecretBasePlacementTypes.Wall: color = Color.FromArgb(opacity, 255, 0, 255); break;
			case SecretBasePlacementTypes.Hole: color = Color.FromArgb(opacity, 140, 0, 255); break;
			case SecretBasePlacementTypes.Rock: color = Color.FromArgb(opacity, 0, 255, 255); break;
			case SecretBasePlacementTypes.Reserved: color = Color.FromArgb(opacity, 0, 255, 0); break;
			}
			return new SolidColorBrush(color);
		}
	}
}
