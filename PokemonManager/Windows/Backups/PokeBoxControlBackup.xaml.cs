using PokemonManager.Game;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WPF.JoshSmith.Adorners;
using WPF.JoshSmith.Controls.Utilities;

namespace PokemonManager.Windows {

	public enum PokeBoxControlModesBackup {
		ViewOnly,
		MovePokemon,
		SelectPlacement
	}

	public class PokeBoxTagStructureBackup {
		public PokeBoxControlBackup Control;
		public int BoxIndex;

		public PokeBoxTagStructureBackup(PokeBoxControlBackup control, int boxIndex) {
			this.Control = control;
			this.BoxIndex = boxIndex;
		}
	}

	public partial class PokeBoxControlBackup : UserControl {

		private PokeBoxControlBackup master;
		private List<PokeBoxControlBackup> slaves;

		private IPokeContainer pokeContainer;
		private int hoverIndex;
		private PokeBoxControlModes mode;
		private bool pickupMode;
		private int selectedIndex;

		// Party Elements
		private List<Image> partyImages;
		private List<Rectangle> partyImageMasks;
		private List<Image> partySlotImages;
		private List<Rectangle> partyClickAreas;


		// Box Elements
		private List<Image> boxImages;
		private List<Rectangle> boxImageMasks;
		private List<Rectangle> boxClickAreas;

		// Pokemon Moving
		private DragAdorner pickupDragAdorner;
		private Grid pickupElement;
		private bool movingPokemon;
		private UIElement adornerContainer;

		private PokemonViewer pokemonViewer;

		// Pokemon Context Menu
		private ContextMenu contextMenu;

		public PokeBoxControlBackup() {
			InitializeComponent();

			this.pickupMode = false;
			this.mode = PokeBoxControlModes.ViewOnly;
			this.hoverIndex = -1;

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				CreateContextMenu();
				CreatePickupElement();
				CreatePartyElements();
				CreateBoxElements();

				this.imagePartyWindow.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "PartyWindow.png")));
				this.imagePartySelector.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "Selector.png")));
				this.imageBoxSelector.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "Selector.png")));
			}
		}

		public PokeBoxControlBackup Master {
			get { return master; }
			set {
				master = value;
				if (master != null) {

				}
			}
		}

		public void AddSlave(PokeBoxControl boxControl) {
			boxControl.master = this;
			slaves.Add(boxControl);
		}
		public void ClearSlaves() {
			slaves.Clear();
		}

		public PokeBoxControlModes Mode {
			get {
				return mode;
			}
			set {
				mode = value;
				if (mode == PokeBoxControlModes.SelectPlacement) {
					pickupMode = true;
					imagePartySelector.Source = ResourceDatabase.GetImageFromName("BoxSelectorMove");
					imageBoxSelector.Source = ResourceDatabase.GetImageFromName("BoxSelectorMove");
					contextMenu.Visibility = Visibility.Hidden;
				}
				contextMenu.Visibility = (mode == PokeBoxControlModes.MovePokemon ? Visibility.Visible : Visibility.Hidden);
			}
		}
		public PokemonViewer PokemonViewer {
			get { return pokemonViewer; }
			set { pokemonViewer = value; }
		}
		private bool IsViewingParty {
			get { return pokeContainer is IPokeParty; }
		}
		private IPokeBox PokeBox {
			get { return pokeContainer as IPokeBox; }
		}

		public void LoadBox(IPokeContainer container) {
			this.pokeContainer = container;

			RefreshUI();
		}

		public void UnloadBox() {
			this.hoverIndex = -1;
			this.imageBoxSelector.Visibility = Visibility.Hidden;
			this.imagePartySelector.Visibility = Visibility.Hidden;
			this.pokeContainer = null;
			this.labelBoxName.Content = "";

			foreach (Image image in boxImages)
				image.Source = null;
			foreach (Rectangle mask in boxImageMasks)
				mask.Visibility = Visibility.Hidden;
			foreach (Image image in partyImages)
				image.Source = null;
			foreach (Rectangle mask in partyImageMasks)
				mask.Visibility = Visibility.Hidden;
			foreach (Image slot in partySlotImages)
				slot.Visibility = Visibility.Hidden;

			gridParty.Visibility = Visibility.Hidden;
			gridBox.Visibility = Visibility.Hidden;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			if (!DesignerProperties.GetIsInDesignMode(this)) {
				((UIElement)Parent).PreviewMouseMove += OnPreviewMouseMove;

				// Do this to make sure the pickup element draws correctly the first time.
				// This was the best method that worked.
				InitializeAdornerLayer();
				RemoveAdornerLayer();
			}
		}

		public void RefreshUI() {
			if (pokeContainer == null) {
				UnloadBox();
				return;
			}

			if (IsViewingParty) {
				gridParty.Visibility = Visibility.Visible;
				gridBox.Visibility = Visibility.Hidden;

				if (hoverIndex != -1) {
					imagePartySelector.Margin = new Thickness(31 + (hoverIndex % 3) * 32, 51 + (hoverIndex / 3) * 25, 0, 0);
					imagePartySelector.Visibility = Visibility.Visible;
				}
				else {
					imagePartySelector.Visibility = Visibility.Hidden;
				}

				for (int i = 0; i < pokeContainer.NumSlots; i++) {
					if (pokeContainer[i] != null) {
						partyImages[i].Source = pokeContainer[i].BoxSprite;
						partySlotImages[i].Visibility = Visibility.Visible;
						if (pokeContainer[i].IsShadowPokemon) {
							partyImageMasks[i].OpacityMask = new ImageBrush(pokeContainer[i].BoxSprite);
							partyImageMasks[i].Visibility = Visibility.Visible;
						}
						else {
							partyImageMasks[i].Visibility = Visibility.Hidden;
						}
					}
					else {
						partyImages[i].Source = null;
						partyImageMasks[i].OpacityMask = null;
						partySlotImages[i].Visibility = Visibility.Hidden;
						partyImageMasks[i].Visibility = Visibility.Hidden;
					}
				}
			}
			else {
				gridParty.Visibility = Visibility.Hidden;
				gridBox.Visibility = Visibility.Visible;

				if (hoverIndex != -1) {
					imageBoxSelector.Margin = new Thickness(1 + (hoverIndex % 6) * 24, 5 + (hoverIndex / 6) * 24, 0, 0);
					imageBoxSelector.Visibility = Visibility.Visible;
				}
				else {
					imageBoxSelector.Visibility = Visibility.Hidden;
				}

				this.labelBoxName.Content = PokeBox.Name;

				if (pokeContainer.GameType == GameTypes.Ruby || pokeContainer.GameType == GameTypes.Sapphire)
					imageWallpaper.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources/Wallpapers", "RubySapphire", PokeBox.Wallpaper.ToString() + ".png")));
				else if (pokeContainer.GameType == GameTypes.Emerald)
					imageWallpaper.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources/Wallpapers", "Emerald", PokeBox.Wallpaper.ToString() + ".png")));
				else if (pokeContainer.GameType == GameTypes.FireRed || pokeContainer.GameType == GameTypes.LeafGreen)
					imageWallpaper.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources/Wallpapers", "FireRedLeafGreen", PokeBox.Wallpaper.ToString() + ".png")));
				else if (pokeContainer.GameType == GameTypes.Colosseum)
					imageWallpaper.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources/Wallpapers", "Colosseum", PokeBox.Wallpaper.ToString() + "RS.png")));
				else
					imageWallpaper.Source = null;

				for (int i = 0; i < pokeContainer.NumSlots; i++) {
					if (pokeContainer[i] != null) {
						boxImages[i].Source = pokeContainer[i].BoxSprite;
						if (pokeContainer[i].IsShadowPokemon) {
							boxImageMasks[i].OpacityMask = new ImageBrush(pokeContainer[i].BoxSprite);
							boxImageMasks[i].Visibility = Visibility.Visible;
						}
						else {
							boxImageMasks[i].Visibility = Visibility.Hidden;
						}
					}
					else {
						boxImages[i].Source = null;
						boxImageMasks[i].OpacityMask = null;
						boxImageMasks[i].Visibility = Visibility.Hidden;
					}
				}
			}
		}
		public void FinishActions() {
			if (movingPokemon) {
				RemoveAdornerLayer();
				movingPokemon = false;
			}
		}

		private void OnBoxSlotClicked(object sender, MouseButtonEventArgs e) {
			if (pokeContainer == null)
				return;

			if (master != null)
				master.OnBoxSlotClicked(sender, e);

			Rectangle selector = sender as Rectangle;
			IGen3Pokemon pkm = pokeContainer[(int)selector.Tag];
			if (e.ChangedButton == MouseButton.Left) {
				if (pkm != null && pokemonViewer != null)
					pokemonViewer.LoadPokemon(pkm);
				if (pickupMode) {
					if (mode == PokeBoxControlModes.MovePokemon) {
						MovePokemon(pkm, pokeContainer, hoverIndex);
					}
					else if (mode == PokeBoxControlModes.SelectPlacement && pkm == null) {
						MovePokemon(pkm, pokeContainer, hoverIndex);
						Window.GetWindow(this).DialogResult = true;
						Window.GetWindow(this).Close();
					}
				}
			}
			else if (e.ChangedButton == MouseButton.Middle) {
				if (mode != PokeBoxControlModes.SelectPlacement) {
					pickupMode = !pickupMode;
					if (pickupMode) {
						imagePartySelector.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "Selector2.png")));
						imageBoxSelector.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "Selector2.png")));
					}
					else {
						imagePartySelector.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "Selector.png")));
						imageBoxSelector.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "Selector.png")));
					}
				}
			}
		}
		private void OnBoxSlotEnter(object sender, MouseEventArgs e) {
			if (pokeContainer == null)
				return;

			Rectangle selector = sender as Rectangle;
			int newIndex = (int)selector.Tag;
			if (hoverIndex != newIndex) {
				hoverIndex = newIndex;
				if (IsViewingParty) {
					imagePartySelector.Margin = new Thickness(31 + (newIndex % 3) * 32, 51 + (newIndex / 3) * 25, 0, 0);
					imagePartySelector.Visibility = Visibility.Visible;
				}
				else {
					imageBoxSelector.Margin = new Thickness(1 + (newIndex % 6) * 24, 5 + (newIndex / 6) * 24, 0, 0);
					imageBoxSelector.Visibility = Visibility.Visible;
				}
			}
		}
		private void OnBoxSlotLeave(object sender, MouseEventArgs e) {
			if (pokeContainer == null)
				return;

			Rectangle selector = sender as Rectangle;
			int index = (int)selector.Tag;
			if (hoverIndex == index) {
				hoverIndex = -1;
				imageBoxSelector.Visibility = Visibility.Hidden;
				imagePartySelector.Visibility = Visibility.Hidden;
			}
		}

		private void OnPreviewMouseMove(object sender, MouseEventArgs e) {
			if (movingPokemon) {
				Point ptCursor = new Point();
				if (hoverIndex != -1) {
					if (IsViewingParty)
						ptCursor =  partyClickAreas[hoverIndex].TranslatePoint(new Point(16, 9), adornerContainer);
					else
						ptCursor =  boxClickAreas[hoverIndex].TranslatePoint(new Point(12, 8), adornerContainer);
				}
				else {
					ptCursor = MouseUtilities.GetMousePosition(adornerContainer);
				}
				

				double left = ptCursor.X - 16;
				double top = ptCursor.Y - 24;

				this.pickupDragAdorner.SetOffsets(left, top);
			}
		}

		private void OnPokemonContextMenuOpening(object sender, ContextMenuEventArgs e) {
			Rectangle selector = sender as Rectangle;
			IGen3Pokemon pkm = pokeContainer[(int)selector.Tag];
			selectedIndex = (int)selector.Tag;

			((MenuItem)contextMenu.Items[0]).IsEnabled = true;
			if (pkm == null) {
				if (movingPokemon)
					((MenuItem)contextMenu.Items[0]).Header = "Place";
				else
					((MenuItem)contextMenu.Items[0]).IsEnabled = false;
			}
			else {
				if (movingPokemon)
					((MenuItem)contextMenu.Items[0]).Header = "Switch";
				else
					((MenuItem)contextMenu.Items[0]).Header = "Move";
			}
			((MenuItem)contextMenu.Items[1]).IsEnabled = pkm != null;
			((MenuItem)contextMenu.Items[2]).IsEnabled = (pkm != null && !movingPokemon && !pkm.IsShadowPokemon);
			((MenuItem)contextMenu.Items[3]).IsEnabled = (pkm != null && !pkm.IsShadowPokemon);
		}

		public void MovePokemon(IGen3Pokemon pokemon, IPokeContainer container, int index) {
			if (movingPokemon) {
				RemoveAdornerLayer();
				movingPokemon = false;
			}
			PokeManager.PickupPokemon(container, index);
			if (PokeManager.IsHoldingPokemon) {
				InitializeAdornerLayer();
				RemoveAdornerLayer();

				((Image)pickupElement.Children[1]).Source = PokeManager.HoldingPokemon.BoxSprite;
				((Rectangle)pickupElement.Children[2]).OpacityMask = new ImageBrush(PokeManager.HoldingPokemon.BoxSprite);
				((Rectangle)pickupElement.Children[2]).Visibility = (PokeManager.HoldingPokemon.IsShadowPokemon ? Visibility.Visible : Visibility.Hidden);
				InitializeAdornerLayer();
				movingPokemon = true;
				OnPreviewMouseMove(null, null);
			}
			RefreshUI();
		}

		private void InitializeAdornerLayer() {
			// Create a brush which will paint the ListViewItem onto
			// a visual in the adorner layer.
			VisualBrush brush = new VisualBrush(pickupElement);

			adornerContainer = (UIElement)this.Parent;

			// Create an element which displays the source item while it is dragged.
			this.pickupDragAdorner = new DragAdorner(adornerContainer, pickupElement.RenderSize, brush);
			this.pickupDragAdorner.UseLayoutRounding = true;
			this.pickupDragAdorner.SnapsToDevicePixels = true;

			AdornerLayer layer = AdornerLayer.GetAdornerLayer(adornerContainer);
			layer.Add(pickupDragAdorner);
		}

		private void RemoveAdornerLayer() {
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornerContainer);
			if (adornerLayer != null) {
				adornerLayer.Remove(pickupDragAdorner);
				pickupDragAdorner = null;
			}
		}

		private void CreatePickupElement() {
			pickupElement = new Grid();
			pickupElement.Width = 32;
			pickupElement.Height = 36;
			pickupElement.HorizontalAlignment = HorizontalAlignment.Left;
			pickupElement.VerticalAlignment = VerticalAlignment.Top;
			pickupElement.UseLayoutRounding = true;
			pickupElement.SnapsToDevicePixels = true;
			pickupElement.IsHitTestVisible = false;

			Image shadow = new Image();
			shadow.HorizontalAlignment = HorizontalAlignment.Left;
			shadow.VerticalAlignment = VerticalAlignment.Top;
			shadow.SnapsToDevicePixels = true;
			shadow.UseLayoutRounding = true;
			shadow.Stretch = Stretch.None;
			shadow.Width = 32;
			shadow.Height = 36;
			shadow.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "PokemonShadow.png")));

			Image image = new Image();
			image.HorizontalAlignment = HorizontalAlignment.Left;
			image.VerticalAlignment = VerticalAlignment.Top;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;
			image.Stretch = Stretch.None;
			image.Width = 32;
			image.Height = 32;

			// Used for shadow pokemon tints
			Rectangle mask = new Rectangle();
			mask.HorizontalAlignment = HorizontalAlignment.Left;
			mask.VerticalAlignment = VerticalAlignment.Top;
			mask.Width = 32;
			mask.Height = 32;
			mask.HorizontalAlignment = HorizontalAlignment.Left;
			mask.VerticalAlignment = VerticalAlignment.Top;
			mask.StrokeThickness = 0;
			mask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));

			pickupElement.Children.Add(shadow);
			pickupElement.Children.Add(image);
			pickupElement.Children.Add(mask);
		}

		private void CreatePartyElements() {
			this.partyImages = new List<Image>();
			this.partyImageMasks = new List<Rectangle>();
			this.partySlotImages = new List<Image>();
			this.partyClickAreas = new List<Rectangle>();

			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 3; j++) {
					Image slotImage = new Image();
					slotImage.Stretch = Stretch.None;
					slotImage.SnapsToDevicePixels = true;
					slotImage.UseLayoutRounding = true;
					slotImage.Width = 30;
					slotImage.Height = 23;
					slotImage.Margin = new Thickness(31 + j * 32, 57 + i * 25, 0, 0);
					slotImage.HorizontalAlignment = HorizontalAlignment.Left;
					slotImage.VerticalAlignment = VerticalAlignment.Top;
					slotImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, "../../Resources", "PartySlot.png")));
					slotImage.Visibility = Visibility.Hidden;
					gridParty.Children.Insert(gridParty.Children.Count - 1, slotImage);
					partySlotImages.Add(slotImage);
				}
			}

			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 3; j++) {
					Image image = new Image();
					image.Stretch = Stretch.None;
					image.SnapsToDevicePixels = true;
					image.UseLayoutRounding = true;
					image.Width = 32;
					image.Height = 32;
					image.Margin = new Thickness(30 + j * 32, 49 + i * 25, 0, 0);
					image.HorizontalAlignment = HorizontalAlignment.Left;
					image.VerticalAlignment = VerticalAlignment.Top;
					gridParty.Children.Insert(gridParty.Children.Count - 1, image);

					// Used for shadow pokemon tints
					Rectangle mask = new Rectangle();
					mask.Width = 32;
					mask.Height = 32;
					mask.Margin = new Thickness(30 + j * 32, 49 + i * 25, 0, 0);
					mask.HorizontalAlignment = HorizontalAlignment.Left;
					mask.VerticalAlignment = VerticalAlignment.Top;
					mask.StrokeThickness = 0;
					mask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));
					mask.Visibility = Visibility.Hidden;
					gridParty.Children.Insert(gridParty.Children.Count - 1, mask);

					partyImages.Add(image);
					partyImageMasks.Add(mask);
				}
			}
			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 3; j++) {
					Rectangle clickArea = new Rectangle();
					clickArea.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
					clickArea.Stroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
					clickArea.Width = 32;
					clickArea.Height = 25;
					clickArea.Opacity = 0;
					clickArea.Tag = i * 3 + j;
					clickArea.Margin = new Thickness(30 + j * 32, 56 + i * 25, 0, 0);
					clickArea.HorizontalAlignment = HorizontalAlignment.Left;
					clickArea.VerticalAlignment = VerticalAlignment.Top;
					clickArea.PreviewMouseDown += OnBoxSlotClicked;
					clickArea.MouseEnter += OnBoxSlotEnter;
					clickArea.MouseLeave += OnBoxSlotLeave;
					clickArea.ContextMenu = contextMenu;
					clickArea.ContextMenuOpening += OnPokemonContextMenuOpening;
					partyClickAreas.Add(clickArea);
					gridParty.Children.Insert(gridParty.Children.Count - 1, clickArea);
				}
			}
		}

		private void CreateBoxElements() {
			this.boxImages = new List<Image>();
			this.boxImageMasks = new List<Rectangle>();
			this.boxClickAreas = new List<Rectangle>();

			for (int i = 0; i < 5; i++) {
				for (int j = 0; j < 6; j++) {
					Image image = new Image();
					image.Stretch = Stretch.None;
					image.SnapsToDevicePixels = true;
					image.UseLayoutRounding = true;
					image.Width = 32;
					image.Height = 32;
					image.Margin = new Thickness(j * 24, i * 24, 0, 0);
					image.HorizontalAlignment = HorizontalAlignment.Left;
					image.VerticalAlignment = VerticalAlignment.Top;
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 1, image);

					// Used for shadow pokemon tints
					Rectangle mask = new Rectangle();
					mask.Width = 32;
					mask.Height = 32;
					mask.Margin = new Thickness(j * 24, i * 24, 0, 0);
					mask.HorizontalAlignment = HorizontalAlignment.Left;
					mask.VerticalAlignment = VerticalAlignment.Top;
					mask.StrokeThickness = 0;
					mask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));
					mask.Visibility = Visibility.Hidden;
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 1, mask);

					boxImages.Add(image);
					boxImageMasks.Add(mask);
				}
			}
			for (int i = 0; i < 5; i++) {
				for (int j = 0; j < 6; j++) {
					Rectangle clickArea = new Rectangle();
					clickArea.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
					clickArea.Stroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
					clickArea.Width = 24;
					clickArea.Height = 24;
					clickArea.Opacity = 0;
					clickArea.Tag = i * 6 + j;
					clickArea.Margin = new Thickness(4 + j * 24, 8 + i * 24, 0, 0);
					clickArea.HorizontalAlignment = HorizontalAlignment.Left;
					clickArea.VerticalAlignment = VerticalAlignment.Top;
					clickArea.PreviewMouseDown += OnBoxSlotClicked;
					clickArea.MouseEnter += OnBoxSlotEnter;
					clickArea.MouseLeave += OnBoxSlotLeave;
					clickArea.ContextMenu = contextMenu;
					clickArea.ContextMenuOpening += OnPokemonContextMenuOpening;
					boxClickAreas.Add(clickArea);
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 1, clickArea);
				}
			}
		}

		private void CreateContextMenu() {
			contextMenu = new ContextMenu();

			MenuItem move = new MenuItem();
			move.Header = "Move";
			move.Click += OnContextMenuMoveClicked;
			MenuItem summary = new MenuItem();
			summary.Header = "Summary";
			summary.Click += OnContextMenuSummaryClicked;
			MenuItem sendTo = new MenuItem();
			sendTo.Header = "Send To...";
			sendTo.Click += OnContextMenuSendToClicked;
			MenuItem release = new MenuItem();
			release.Header = "Release";
			release.Click += OnContextMenuReleaseClicked;

			contextMenu.Items.Add(move);
			contextMenu.Items.Add(summary);
			contextMenu.Items.Add(sendTo);
			contextMenu.Items.Add(release);
		}

		private void OnContextMenuMoveClicked(object sender, RoutedEventArgs e) {
			MovePokemon(pokeContainer[selectedIndex], pokeContainer, selectedIndex);
		}
		private void OnContextMenuSummaryClicked(object sender, RoutedEventArgs e) {
			if (pokeContainer[selectedIndex] != null)
				pokemonViewer.LoadPokemon(pokeContainer[selectedIndex]);
		}
		private void OnContextMenuSendToClicked(object sender, RoutedEventArgs e) {
			//PokeManager.PickupPokemon(pokeBox, selectedIndex);
			var result = SendPokemonToWindow.ShowDialog(Window.GetWindow(this), pokeContainer[selectedIndex], pokeContainer, selectedIndex, -2);
			if (!result.HasValue && !result.Value) {
				PokeManager.DropPokemon();
			}
			RefreshUI();
		}
		private void OnContextMenuReleaseClicked(object sender, RoutedEventArgs e) {
			MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Window.GetWindow(this), "Are you sure you want to release this Pokemon?", "Release Pokemon", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				// Be freeeeeeeeeeee (in the void forever)
				pokeContainer[selectedIndex] = null;
				RefreshUI();
			}
		}
	}
}
