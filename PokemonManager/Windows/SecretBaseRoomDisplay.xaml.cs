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
	/// Interaction logic for SecretBaseDisplay.xaml
	/// </summary>
	public partial class SecretBaseRoomDisplay : UserControl {

		private List<Image> decorationImages;
		private SecretBase secretBase;

		public SecretBaseRoomDisplay() {
			InitializeComponent();
			decorationImages = new List<Image>();
		}

		private LocationData LocationData {
			get { return secretBase.LocationData; }
		}
		private RoomData RoomData {
			get { return secretBase.RoomData; }
		}
		private RouteData RouteData {
			get { return secretBase.RouteData; }
		}

		public void UnloadSecretBase() {
			rectRoomBackground.Visibility = Visibility.Hidden;
			gridRoomContents.Visibility = Visibility.Hidden;
		}

		public void LoadSecretBase(SecretBase secretBase) {
			this.secretBase = secretBase;
			foreach (Image image in decorationImages) {
				this.gridRoomContents.Children.Remove(image);
			}
			gridRoomContents.Visibility = Visibility.Visible;

			imageTrainer.Margin = new Thickness(16 * RoomData.TrainerX, 16 * RoomData.TrainerY - 8, 0, 0);
			imageTrainer.Source = ResourceDatabase.GetImageFromName(secretBase.TrainerGender.ToString() + "SecretBase" + ((byte)secretBase.TrainerID % 5).ToString());
			imageTrainer.Visibility = (secretBase.IsPlayerSecretBase ? Visibility.Hidden : Visibility.Visible);

			BitmapSource roomImage = RoomData.Image;

			gridRoomContents.Width = roomImage.PixelWidth;
			gridRoomContents.Height = roomImage.PixelHeight;
			imageRoom.Width = roomImage.PixelWidth;
			imageRoom.Height = roomImage.PixelHeight;
			imageRoom.Source = roomImage;

			/*if (RoomData.Type == SecretBaseRoomTypes.Tree || RoomData.Type == SecretBaseRoomTypes.Shrub) {
				rectRoomBackground.Visibility = Visibility.Hidden;
			}
			else {*/
				rectRoomBackground.Margin = new Thickness((1 - RoomData.Width % 2) * -8, (1 - RoomData.Height % 2) * -8, 0, 0);
				rectRoomBackground.Visibility = Visibility.Visible;
				(rectRoomBackground.Fill as ImageBrush).ImageSource = RoomData.BackgroundImage;
			//}

			foreach (PlacedDecoration decoration in secretBase.PlacedDecorations) {
				DecorationData decorationData = decoration.DecorationData;
				BitmapSource decorationImage = ItemDatabase.GetDecorationFullSizeImageFromID(decoration.ID);
				Image image = new OpaqueClickableImage();
				image.Width = decorationImage.PixelWidth;
				image.Height = decorationImage.PixelHeight;
				image.Stretch = Stretch.None;
				image.Source = decorationImage;
				image.Margin = new Thickness(16 * (decoration.X - decorationData.OriginX), 16 * (decoration.Y - decorationData.OriginY), 0, 0);
				image.HorizontalAlignment = HorizontalAlignment.Left;
				image.VerticalAlignment = VerticalAlignment.Top;
				image.Tag = decoration;
				decorationImages.Add(image);

				this.gridRoomContents.Children.Add(image);
			}
			
			foreach (Image image in decorationImages) {
				PlacedDecoration decoration = image.Tag as PlacedDecoration;
				int zIndex = 16 * decoration.Y;
				if (decoration.DecorationData.DecorationType == DecorationTypes.Doll) {
					if (decoration.ID >= 111) // Big Doll
						zIndex += 160;
					else
						zIndex += 162;
				}
				else if (decoration.DecorationData.DecorationType == DecorationTypes.Cushion) {
					zIndex += 162;
				}
				Grid.SetZIndex(image, zIndex);
			}

			Grid.SetZIndex(imageTrainer, 16 * RoomData.TrainerY);
		}
	}
}
