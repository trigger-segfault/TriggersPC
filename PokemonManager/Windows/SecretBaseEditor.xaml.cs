using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
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
	/// Interaction logic for SecretBaseEditor.xaml
	/// </summary>
	enum PlaceModes {
		Place,
		PutAway,
		Move,
		Moving
	}

	public partial class SecretBaseEditor : Window {

		// Debug
		private int gameIndex;

		private List<Image> decorationImages;
		private SecretBase secretBase;
		//private IGameSave gameSave;
		private ToolTip routeTooltip;
		private PlaceModes mode;
		private byte selectedDecorationID;
		private PlacedDecoration hoverDecoration;
		private int hoverX;
		private int hoverY;
		private DecorationPocket pocket;
		private bool showNotes;

		public SecretBaseEditor() {
			InitializeComponent();
			gameIndex = -1;
			this.decorationImages = new List<Image>();

			NextSecretBase();
			imagePlace.Visibility = Visibility.Hidden;
			rectPlaceMask.Visibility = Visibility.Hidden;

			for (DecorationTypes i = DecorationTypes.Desk; i <= DecorationTypes.Cushion; i++) {
				comboBoxPockets.Items.Add(ItemDatabase.GetDecorationContainerName(i));
			}
		}

		private void NextSecretBase() {
			/*IGameSave gameSave;
			do {
				gameIndex++;
				if (gameIndex >= PokeManager.NumGameSaves)
					gameIndex = 0;
				gameSave = PokeManager.GetGameSaveAt(gameIndex);
			} while (!(gameSave is GBAGameSave) || ((GBAGameSave)gameSave).SecretBaseLocation == 0);
			LoadSecretBase(new PlayerSecretBase(gameSave));*/
			gameIndex++;
			if (gameIndex >= SecretBaseDatabase.NumLocations)
				gameIndex = 0;
			LoadSecretBase(new SharedSecretBase(SecretBaseDatabase.GetLocationAt(gameIndex).ID, null));
		}

		public static void Show(Window owner, SecretBase secretBase) {
			SecretBaseEditor window = new SecretBaseEditor();
			window.Owner = owner;
			window.LoadSecretBase(secretBase);
			window.ShowDialog();
		}

		private DecorationData SelectedDecorationData {
			get { return ItemDatabase.GetDecorationFromID(selectedDecorationID); }
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
		private PlaceModes Mode {
			get { return mode; }
			set {
				mode = value;
				buttonPlace.IsChecked = mode == PlaceModes.Place;
				buttonPutAway.IsChecked = mode == PlaceModes.PutAway;
				buttonMove.IsChecked = mode == PlaceModes.Move || mode == PlaceModes.Moving;
			}
		}

		public void LoadSecretBase(SecretBase secretBase) {
			this.secretBase = secretBase;
			foreach (Image image in decorationImages) {
				this.gridRoomContents.Children.Remove(image);
			}

			if (pocket != null)
				pocket = secretBase.GameSave.Inventory.Decorations[pocket.PocketType];
			else
				pocket = secretBase.GameSave.Inventory.Decorations[DecorationTypes.Desk];
			comboBoxPockets.SelectedIndex = (int)DecorationTypes.Desk - 1;
			OnPocketSelectionChanged(null, null);

			imageLocation.Source = LocationData.Image;

			imageTrainer.Margin = new Thickness(16 * RoomData.TrainerX, 16 * RoomData.TrainerY - 8, 0, 0);
			imageTrainer.Source = ResourceDatabase.GetImageFromName(secretBase.TrainerGender.ToString() + "SecretBase" + ((byte)secretBase.TrainerID % 5).ToString());

			BitmapSource roomImage = RoomData.Image;

			gridRoomContents.Width = roomImage.PixelWidth;
			gridRoomContents.Height = roomImage.PixelHeight;
			imageRoom.Width = roomImage.PixelWidth;
			imageRoom.Height = roomImage.PixelHeight;
			imageRoom.Source = roomImage;

			imageRouteSign.Source = ResourceDatabase.GetImageFromName("RouteSign" + (RouteData.ID >= 124 ? "Water" : "Land"));
			labelRoute.Content = "Route " + RouteData.ID;

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
				if (decoration.DecorationData.DataType == DecorationDataTypes.NoteMat && showNotes) {
					image.Source = ResourceDatabase.GetImageFromName("NoteMat" + (decoration.ID - 51).ToString());
				}
				image.Margin = new Thickness(16 * (decoration.X - decorationData.OriginX), 16 * (decoration.Y - decorationData.OriginY), 0, 0);
				image.HorizontalAlignment = HorizontalAlignment.Left;
				image.VerticalAlignment = VerticalAlignment.Top;
				image.Tag = decoration;
				image.PreviewMouseDown += OnDecorationImageMouseDown;
				image.MouseEnter += OnDecorationImageMouseEnter;
				image.MouseLeave += OnDecorationImageMouseLeave;
				image.IsHitTestVisible = mode == PlaceModes.PutAway || mode == PlaceModes.Move;
				decorationImages.Add(image);

				this.gridRoomContents.Children.Add(image);
			}
			OrganizeZIndexes();
			Grid.SetZIndex(imageTrainer, 16 * RoomData.TrainerY);

			routeTooltip = new ToolTip();

			BitmapSource routeImage = RouteData.Image;

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
			pinpoint.Margin = new Thickness(LocationData.MapX * 2, LocationData.MapY * 2, 0, 0);

			tooltip.Children.Add(route);
			tooltip.Children.Add(pinpoint);
			imageLocation.ToolTip = tooltip;

			UpdateDetails();
		}
		private void OnTooltipOpening(object sender, ToolTipEventArgs e) {

		}
		private void UpdateDetails() {
			labelDecorationsInUse.Content = "Decorations   " + secretBase.SlotsUsed.ToString() + "/16";
			if (secretBase.SlotsUsed == 16)
				labelDecorationsInUse.Foreground = new SolidColorBrush(Color.FromRgb(220, 0, 0));
			else
				labelDecorationsInUse.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		}

		public void OrganizeZIndexes() {
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
		}

		private void OnNextClicked(object sender, RoutedEventArgs e) {
			NextSecretBase();
		}

		private void SelectPlaceDecoration(byte id) {
			selectedDecorationID = id;
			if (id == 0) {
				imagePlace.Source = null;
				imagePlace.Visibility = Visibility.Hidden;
			}
			else {
				BitmapSource bitmap = ItemDatabase.GetDecorationFullSizeImageFromID(id);
				imagePlace.Source = bitmap;
				imagePlace.Width = bitmap.PixelWidth;
				imagePlace.Height = bitmap.PixelHeight;
				if (mode == PlaceModes.PutAway)
					Mode = PlaceModes.Place;
				foreach (Image image in decorationImages) {
					image.IsHitTestVisible = false;
				}
			}
		}

		private void OnRoomMouseDown(object sender, MouseButtonEventArgs e) {
			if ((mode == PlaceModes.Place || mode == PlaceModes.Moving) && selectedDecorationID != 0 && secretBase.CanPlaceDecoration(selectedDecorationID, (byte)hoverX, (byte)hoverY)) {
				secretBase.PlaceDecoration(selectedDecorationID, (byte)hoverX, (byte)hoverY);

				BitmapSource decorationImage = ItemDatabase.GetDecorationFullSizeImageFromID(selectedDecorationID);
				Image image = new OpaqueClickableImage();
				image.Width = decorationImage.PixelWidth;
				image.Height = decorationImage.PixelHeight;
				image.Stretch = Stretch.None;
				image.Source = decorationImage;
				if (ItemDatabase.GetDecorationFromID(selectedDecorationID).DataType == DecorationDataTypes.NoteMat && showNotes) {
					image.Source = ResourceDatabase.GetImageFromName("NoteMat" + (selectedDecorationID - 51).ToString());
				}
				image.Margin = new Thickness(16 * (hoverX - SelectedDecorationData.OriginX), 16 * (hoverY - SelectedDecorationData.OriginY), 0, 0);
				image.HorizontalAlignment = HorizontalAlignment.Left;
				image.VerticalAlignment = VerticalAlignment.Top;
				image.Tag = secretBase.PlacedDecorations[secretBase.SlotsUsed - 1];
				image.PreviewMouseDown += OnDecorationImageMouseDown;
				image.MouseEnter += OnDecorationImageMouseEnter;
				image.MouseLeave += OnDecorationImageMouseLeave;
				image.IsHitTestVisible = false;
				decorationImages.Add(image);

				this.gridRoomContents.Children.Add(image);

				OrganizeZIndexes();

				SelectPlaceDecoration(0);
				FillListViewItems();

				if (mode == PlaceModes.Moving) {
					Mode = PlaceModes.Move;
					foreach (Image image2 in decorationImages) {
						image2.IsHitTestVisible = true;
					}
				}
				UpdateDetails();
			}
		}

		private void OnRoomMouseEnter(object sender, MouseEventArgs e) {
			if ((mode == PlaceModes.Place || mode == PlaceModes.Moving) && selectedDecorationID != 0) {
				imagePlace.Visibility = Visibility.Visible;
			}
		}
		private void OnRoomMouseLeave(object sender, MouseEventArgs e) {
			imagePlace.Visibility = Visibility.Hidden;
		}

		private void OnRoomMouseMove(object sender, MouseEventArgs e) {
			if ((mode == PlaceModes.Place || mode == PlaceModes.Moving) && selectedDecorationID != 0) {
				Point point = e.GetPosition(sender as Image);
				hoverX = (int)(point.X / 16);
				hoverY = (int)(point.Y / 16);
				imagePlace.Margin = new Thickness((hoverX - SelectedDecorationData.OriginX) * 16, (hoverY - SelectedDecorationData.OriginY) * 16, 0, 0);
				if (secretBase.CanPlaceDecoration(selectedDecorationID, (byte)hoverX, (byte)hoverY)) {
					imagePlace.Opacity = 1.0;
				}
				else {
					imagePlace.Opacity = 0.65;
				}
			}
		}
		private void OnDecorationImageMouseDown(object sender, MouseButtonEventArgs e) {
			PlacedDecoration decoration = (sender as Image).Tag as PlacedDecoration;
			secretBase.PutAwayDecoration(decoration);
			for (int i = 0; i < decorationImages.Count; i++) {
				if (!secretBase.PlacedDecorations.Contains(decorationImages[i].Tag as PlacedDecoration)) {
					gridRoomContents.Children.Remove(decorationImages[i]);
					decorationImages.RemoveAt(i);
					i--;
				}
			}
			FillListViewItems();

			if (mode == PlaceModes.Move) {
				Mode = PlaceModes.Moving;
				foreach (Image image2 in decorationImages) {
					image2.IsHitTestVisible = false;
				}
				SelectPlaceDecoration(decoration.ID);
			}
			UpdateDetails();
		}

		private void OnDecorationImageMouseEnter(object sender, MouseEventArgs e) {
			PlacedDecoration decoration = (sender as Image).Tag as PlacedDecoration;
			if (hoverDecoration != decoration) {
				BitmapSource bitmap = ItemDatabase.GetDecorationFullSizeImageFromID(decoration.ID);
				rectPlaceMask.Visibility = Visibility.Visible;
				rectPlaceMask.OpacityMask = new ImageBrush(bitmap);
				rectPlaceMask.Width = bitmap.PixelWidth;
				rectPlaceMask.Height = bitmap.PixelHeight;
				rectPlaceMask.Margin = (sender as Image).Margin;
				Panel.SetZIndex(rectPlaceMask, Panel.GetZIndex(sender as Image) + 1);
				hoverDecoration = decoration;
			}
		}
		private void OnDecorationImageMouseLeave(object sender, MouseEventArgs e) {
			PlacedDecoration decoration = (sender as Image).Tag as PlacedDecoration;
			if (hoverDecoration == decoration) {
				rectPlaceMask.Visibility = Visibility.Hidden;
				hoverDecoration = null;
			}
		}

		private void OnDecorationSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (listViewDecorations.SelectedIndex == -1) {
				SelectPlaceDecoration(0);
			}
			else {
				if (mode == PlaceModes.PutAway) {
					Mode = PlaceModes.Place;
					foreach (Image image in decorationImages)
						image.IsHitTestVisible = false;
				}
				else if (mode == PlaceModes.Move) {
					Mode = PlaceModes.Moving;
					foreach (Image image in decorationImages)
						image.IsHitTestVisible = false;
				}
				SelectPlaceDecoration(((listViewDecorations.SelectedItem as Control).Tag as Decoration).ID);
			}
		}

		private void OnPocketSelectionChanged(object sender, SelectionChangedEventArgs e) {
			SelectPlaceDecoration(0);
			if (comboBoxPockets.SelectedIndex != -1) {
				pocket = secretBase.GameSave.Inventory.Decorations[(DecorationTypes)(comboBoxPockets.SelectedIndex + 1)];
				FillListViewItems();
			}
		}

		private void FillListViewItems() {
			listViewDecorations.Items.Clear();
			/*for (byte i = 0; i < pocket.SlotsUsed; i++) {
				int countLeft = (int)pocket[i].Count;
				if (pocket.MaxStackSize == 0) {
					foreach (PlacedDecoration decoration in secretBase.PlacedDecorations) {
						if (decoration.ID == pocket[i].ID)
							countLeft--;
					}
					if (countLeft <= 0)
						continue;
				}
				else {
					if (pocket.Inventory.IsDecorationInUse(i, pocket.PocketType))
						continue;
				}
				AddDecorationListViewItem(pocket[i], countLeft);
			}*/
			if (pocket.MaxStackSize == 0) {
				List<int> decorations = new List<int>();
				for (int i = 0; i <= 120; i++)
					decorations.Add(0);
				for (int i = -1; i < PokeManager.NumGameSaves; i++) {
					IGameSave gameSave = PokeManager.GetGameSaveAt(i);
					if (gameSave.Inventory != null && gameSave.Inventory.Decorations != null) {
						DecorationPocket newPocket = gameSave.Inventory.Decorations[pocket.PocketType];
						for (int j = 0; j < newPocket.SlotsUsed; j++) {
							if (gameSave.Inventory.Decorations.IsDecorationInUse(j, pocket.PocketType))
								continue;
							decorations[newPocket[j].ID] += (int)newPocket[j].Count;
						}
					}
				}
				for (int i = 1; i <= 120; i++) {
					if (decorations[i] == 0)
						continue;
					int countLeft = decorations[i];
					foreach (PlacedDecoration decoration in secretBase.PlacedDecorations) {
						if (decoration.ID == i)
							countLeft--;
					}
					if (countLeft <= 0)
						continue;
					AddDecorationListViewItem(new Decoration((byte)i, (uint)decorations[i], null), countLeft);
				}
			}
			else {
				for (byte i = 0; i < pocket.SlotsUsed; i++) {
					if (pocket.Inventory.IsDecorationInUse(i, pocket.PocketType))
						continue;
					AddDecorationListViewItem(pocket[i]);
				}
			}
		}

		private void AddDecorationListViewItem(Decoration decoration, int countLeft = 1) {
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Tag = decoration;
			listViewItem.SnapsToDevicePixels = true;
			listViewItem.UseLayoutRounding = true;
			DockPanel dockPanel = new DockPanel();
			dockPanel.Width = 170;
				
			Image image = new Image();
			image.Source = ItemDatabase.GetDecorationImageFromID(decoration.ID);
			image.Width = 18;
			image.Height = 18;
			image.Margin = new Thickness(0, 0, 0, 0);
			image.VerticalAlignment = VerticalAlignment.Center;
			image.HorizontalAlignment = HorizontalAlignment.Left;

			TextBlock itemName = new TextBlock();
			itemName.VerticalAlignment = VerticalAlignment.Center;
			itemName.Text = decoration.DecorationData.Name;
			itemName.TextTrimming = TextTrimming.CharacterEllipsis;
			itemName.Margin = new Thickness(4, 0, 0, 0);
			itemName.Padding = new Thickness(5, 0, 5, 0);

			TextBlock itemX = new TextBlock();
			itemX.VerticalAlignment	= VerticalAlignment.Center;
			itemX.HorizontalAlignment = HorizontalAlignment.Right;
			itemX.TextAlignment = TextAlignment.Right;
			itemX.Text = "x";
			itemX.Width = Double.NaN;
			itemX.MinWidth = 10;

			TextBlock itemCount = new TextBlock();
			itemCount.VerticalAlignment	= VerticalAlignment.Center;
			itemCount.HorizontalAlignment = HorizontalAlignment.Right;
			itemCount.TextAlignment = TextAlignment.Right;
			itemCount.Width = 30;
			itemCount.Text = countLeft.ToString();

			listViewItem.ToolTip = decoration.DecorationData.Description;
			listViewItem.Content = dockPanel;
			listViewDecorations.Items.Add(listViewItem);
			dockPanel.Children.Add(image);
			dockPanel.Children.Add(itemName);

			DockPanel.SetDock(image, Dock.Left);
				
			if (pocket.MaxStackSize == 0) {
				dockPanel.Children.Add(itemCount);
				dockPanel.Children.Add(itemX);
				DockPanel.SetDock(itemCount, Dock.Right);
			}
		}

		private void OnShowNoteMatsChecked(object sender, RoutedEventArgs e) {
			showNotes = checkBoxShowNoteMats.IsChecked.Value;

			foreach (Image image in decorationImages) {
				PlacedDecoration decoration = image.Tag as PlacedDecoration;
				if (decoration.DecorationData.DataType == DecorationDataTypes.NoteMat) {
					if (showNotes)
						image.Source = ResourceDatabase.GetImageFromName("NoteMat" + (decoration.ID - 51).ToString());
					else
						image.Source = ItemDatabase.GetDecorationFullSizeImageFromID(decoration.ID);
				}
			}
		}

		private void OnPlaceClicked(object sender, RoutedEventArgs e) {
			Mode = PlaceModes.Place;
			foreach (Image image in decorationImages) {
				image.IsHitTestVisible = false;
			}
			if (mode != PlaceModes.Place) {
				listViewDecorations.SelectedIndex = -1;
			}
		}

		private void OnMoveClicked(object sender, RoutedEventArgs e) {
			foreach (Image image in decorationImages) {
				image.IsHitTestVisible = true;
			}
			if (mode != PlaceModes.Move && mode != PlaceModes.Moving) {
				Mode = PlaceModes.Move;
				listViewDecorations.SelectedIndex = -1;
			}
			else {
				Mode = mode;
			}
		}
		private void OnPutAwayClicked(object sender, RoutedEventArgs e) {
			Mode = PlaceModes.PutAway;
			foreach (Image image in decorationImages) {
				image.IsHitTestVisible = true;
			}
			if (mode != PlaceModes.PutAway) {
				listViewDecorations.SelectedIndex = -1;
				imagePlace.Visibility = Visibility.Hidden;
			}
		}

		private void OnChangeLocationClicked(object sender, RoutedEventArgs e) {

			byte? newLocation = SecretBaseLocationChooser.Show(this, secretBase.LocationID, (secretBase is SharedSecretBase ? ((SharedSecretBase)secretBase).SecretBaseManager : null));
			if (newLocation.HasValue) {
				secretBase.SetNewLocation(newLocation.Value);
				LoadSecretBase(secretBase);
			}
		}
	}
}
