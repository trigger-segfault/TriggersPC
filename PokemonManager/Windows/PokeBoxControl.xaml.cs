using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Windows.Controls;
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

	public enum PokeBoxControlModes {
		ViewOnly,
		MovePokemon,
		Custom
	}

	public class PokemonSelectedEventArgs {
		public IPokeContainer PokeContainer { get; set; }
		public int Index { get; set; }
		public IPokemon Pokemon { get; set; }
	}


	public delegate void PokemonSelectedEventHandler(object sender, PokemonSelectedEventArgs e);

	public class PokeBoxTagStructure {
		public PokeBoxControl Control;
		public int BoxIndex;

		public PokeBoxTagStructure(PokeBoxControl control, int boxIndex) {
			this.Control = control;
			this.BoxIndex = boxIndex;
		}
	}

	public partial class PokeBoxControl : UserControl {

		private Point[] PurifierSlotOffsets = {
			new Point(1, 1),
			new Point(1, 0),
			new Point(2, 1),
			new Point(1, 2),
			new Point(0, 1)
		};

		private PokeBoxControl master;
		private List<PokeBoxControl> slaves;
		private UIElement mouseMoveTarget;
		private bool eventRegistered;
		private bool event2Registered;
		private int gameIndex;

		private IPokeContainer pokeContainer;
		private int hoverIndex;
		private PokeBoxControlModes mode;
		private bool canChangePickupMode;
		private bool pickupMode;
		private bool summaryMode;
		private int selectedIndex;

		// Party Elements
		private List<Image> partyImages;
		private List<Rectangle> partyShadowMasks;
		private List<Image> partySlotImages;
		private List<Rectangle> partyClickAreas;
		private List<Image> partyEggMarkers;

		// Daycare Elements
		private List<Image> daycareImages;
		private List<Rectangle> daycareShadowMasks;
		private List<Image> daycareSlotImages;
		private List<Image> daycareSlotCoverImages;
		private List<Rectangle> daycareClickAreas;

		// Purifier Elements
		private List<Image> purifierImages;
		private List<Rectangle> purifierShadowMasks;
		private List<Image> purifierSlotImages;
		private List<Rectangle> purifierClickAreas;
		private int chamberIndex;

		// Box Elements
		private List<Image> boxImages;
		private List<Rectangle> boxShadowMasks;
		private List<Rectangle> boxSelectMasks;
		private List<Rectangle> boxClickAreas;
		private List<Image> boxEggMarkers;

		// Pokemon Moving
		private DragAdorner pickupDragAdorner;
		private Grid pickupElement;
		private Rectangle pickupShadowMask;
		private Image pickupBoxImage;
		private Label pickupCount;
		private bool movingPokemon;
		private UIElement adornerContainer;

		private IPokemonViewer pokemonViewer;

		// Pokemon Context Menu
		private ContextMenu contextMenu;

		private ContextMenu boxContextMenu;

		public event PokemonSelectedEventHandler PokemonSelected;

		public PokeBoxControl() {
			InitializeComponent();

			this.imageDaycareSelector.Visibility = Visibility.Hidden;
			this.imageBoxSelector.Visibility = Visibility.Hidden;
			this.imagePartySelector.Visibility = Visibility.Hidden;
			this.imagePurifierSelector.Visibility = Visibility.Hidden;
			this.pokeContainer = null;
			this.labelBoxName.Content = "";
			gridParty.Visibility = Visibility.Hidden;
			gridBox.Visibility = Visibility.Hidden;

			rectEditBox.Opacity = 0;

			this.canChangePickupMode = false;
			this.pickupMode = false;
			this.mode = PokeBoxControlModes.ViewOnly;
			this.hoverIndex = -1;
			this.slaves = new List<PokeBoxControl>();
			this.mouseMoveTarget = this;
			this.summaryMode = false;

			if (!DesignerProperties.GetIsInDesignMode(this)) {
				CreateContextMenu();
				CreateBoxContextMenu();
				CreatePickupElement();
				CreatePartyElements();
				CreateDaycareElements();
				CreatePurifierElements();
				CreateBoxElements();
				rectEditBox.Opacity = 0;
			}

			this.imageDaycareHighlighter.Visibility = Visibility.Hidden;
			this.imageBoxHighlighter.Visibility = Visibility.Hidden;
			this.imagePartyHighlighter.Visibility = Visibility.Hidden;
			this.imagePurifierHighlighter.Visibility = Visibility.Hidden;
		}

		#region Master/Slaves

		public PokeBoxControl Master {
			get { return master; }
			set { master = value; }
		}
		public void AddSlave(PokeBoxControl slave) {
			slave.master = this;
			slave.Mode = mode;
			slave.IsPickupMode = pickupMode;
			slaves.Add(slave);
		}
		public void RemoveSlave(PokeBoxControl slave) {
			slaves.Remove(slave);
		}
		public void ClearSlaves() {
			slaves.Clear();
		}

		#endregion

		#region Modes

		public PokeBoxControlModes Mode {
			get {
				return mode;
			}
			set {
				mode = value;
				if (mode == PokeBoxControlModes.MovePokemon)
					this.canChangePickupMode = true;
				rectEditBox.ContextMenu = (mode == PokeBoxControlModes.MovePokemon ? boxContextMenu : null);
				contextMenu.Visibility = (mode == PokeBoxControlModes.MovePokemon ? Visibility.Visible : Visibility.Hidden);
				foreach (PokeBoxControl slave in slaves)
					slave.Mode = mode;
			}
		}
		public bool IsSummaryMode {
			get { return summaryMode; }
			set {
				summaryMode = value;
				foreach (PokeBoxControl slave in slaves)
					slave.IsSummaryMode = value;
				imagePartySelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				imageBoxSelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				imageDaycareSelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				imagePurifierSelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				if (summaryMode && hoverIndex != -1)
					pokemonViewer.LoadPokemon(pokeContainer[hoverIndex]);
			}
		}
		public bool IsPickupMode {
			get { return pickupMode; }
			set {
				pickupMode = value;
				foreach (PokeBoxControl slave in slaves)
					slave.IsPickupMode = value;
				imagePartySelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				imageBoxSelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				imageDaycareSelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
				imagePurifierSelector.Source = ResourceDatabase.GetImageFromName("BoxSelector" + (pickupMode ? "Move" : "Summary") + (summaryMode ? "Hover" : ""));
			}
		}
		public bool CanChangePickupMode {
			get { return canChangePickupMode; }
			set {
				canChangePickupMode = value;
				foreach (PokeBoxControl slave in slaves)
					slave.canChangePickupMode = value;
			}
		}
		public int ChamberIndex {
			get { return chamberIndex; }
			set { chamberIndex = value; }
		}

		#endregion

		#region Events and Targets

		public UIElement MouseMoveTarget {
			get { return mouseMoveTarget; }
			set {
				if (mouseMoveTarget != null && eventRegistered) {
					mouseMoveTarget.PreviewMouseMove -= OnPreviewMouseMove;
					eventRegistered = false;
				}
				mouseMoveTarget = value;
				if (mouseMoveTarget != null) {
					mouseMoveTarget.PreviewMouseMove += OnPreviewMouseMove;
					eventRegistered = true;
				}
			}
		}
		public IPokemonViewer PokemonViewer {
			get { return pokemonViewer; }
			set { pokemonViewer = value; }
		}
		private void OnPokemonSelected(PokemonSelectedEventArgs e) {
			if (PokemonSelected != null) {
				PokemonSelected(this, e);
			}
		}

		#endregion

		#region Private Helpers

		private bool IsViewingParty {
			get { return pokeContainer.Type == ContainerTypes.Party; }
		}
		private bool IsViewingBox {
			get { return pokeContainer.Type == ContainerTypes.Box; }
		}
		private bool IsViewingDaycare {
			get { return pokeContainer.Type == ContainerTypes.Daycare; }
		}
		private bool IsViewingPurifier {
			get { return pokeContainer.Type == ContainerTypes.Purifier; }
		}
		private IPokeBox PokeBox {
			get { return pokeContainer as IPokeBox; }
		}
		private IDaycare Daycare {
			get { return pokeContainer as IDaycare; }
		}

		private bool IsMovingPokemon {
			get { return (movingPokemon || (master != null && master.IsMovingPokemon)); }
		}

		#endregion

		#region Loading/Unloading

		public void UnhighlightPokemon() {
			foreach (PokeBoxControl slave in slaves)
				slave.UnhighlightPokemon();
			if (imageBoxHighlighter.Visibility == Visibility.Visible)
				this.imageBoxHighlighter.Visibility = Visibility.Hidden;
			if (imagePartyHighlighter.Visibility == Visibility.Visible)
				this.imagePartyHighlighter.Visibility = Visibility.Hidden;
			if (imagePurifierHighlighter.Visibility == Visibility.Visible)
				this.imagePurifierHighlighter.Visibility = Visibility.Hidden;
			if (imageDaycareHighlighter.Visibility == Visibility.Visible)
				this.imageDaycareHighlighter.Visibility = Visibility.Hidden;
		}

		public void LoadBox(IPokeContainer container, int gameIndex) {
			this.pokeContainer = container;
			this.gameIndex = gameIndex;
			/*if (imageBoxHighlighter.Visibility == Visibility.Visible)
				this.imageBoxHighlighter.Visibility = Visibility.Hidden;
			if (imagePartyHighlighter.Visibility == Visibility.Visible)
				this.imagePartyHighlighter.Visibility = Visibility.Hidden;
			if (imageDaycareHighlighter.Visibility == Visibility.Visible)
				this.imageDaycareHighlighter.Visibility = Visibility.Hidden;*/
			RefreshUI(true);
		}
		public void UnloadBox() {
			this.hoverIndex = -1;
			/*if (imageBoxHighlighter.Visibility == Visibility.Visible)
				this.imageBoxHighlighter.Visibility = Visibility.Hidden;
			if (imagePartyHighlighter.Visibility == Visibility.Visible)
				this.imagePartyHighlighter.Visibility = Visibility.Hidden;
			if (imageDaycareHighlighter.Visibility == Visibility.Visible)
				this.imageDaycareHighlighter.Visibility = Visibility.Hidden;*/
			this.pokeContainer = null;
			this.labelBoxName.Content = "";

			foreach (Image image in boxImages)
				image.Source = null;
			foreach (Rectangle mask in boxShadowMasks)
				mask.Visibility = Visibility.Hidden;
			foreach (Rectangle select in boxSelectMasks)
				select.Visibility = Visibility.Hidden;
			foreach (Image egg in boxEggMarkers)
				egg.Visibility = Visibility.Hidden;
			foreach (Image image in partyImages)
				image.Source = null;
			foreach (Rectangle mask in partyShadowMasks)
				mask.Visibility = Visibility.Hidden;
			foreach (Image slot in partySlotImages)
				slot.Visibility = Visibility.Hidden;
			foreach (Image egg in partyEggMarkers)
				egg.Visibility = Visibility.Hidden;
			foreach (Image image in daycareImages)
				image.Source = null;
			foreach (Rectangle mask in daycareShadowMasks)
				mask.Visibility = Visibility.Hidden;
			foreach (Image slot in daycareSlotImages)
				slot.Visibility = Visibility.Hidden;
			foreach (Image image in purifierImages)
				image.Source = null;
			foreach (Rectangle mask in purifierShadowMasks)
				mask.Visibility = Visibility.Hidden;
			foreach (Image slot in purifierSlotImages)
				slot.Visibility = Visibility.Hidden;

			gridDaycare.Visibility = Visibility.Hidden;
			gridParty.Visibility = Visibility.Hidden;
			gridBox.Visibility = Visibility.Hidden;
			gridPurifier.Visibility = Visibility.Hidden;

			rectEditBox.Opacity = 0;
		}

		#endregion

		#region Event Hooking

		private void OnLoaded(object sender, RoutedEventArgs e) {
			if (!DesignerProperties.GetIsInDesignMode(this)) {
				if (!eventRegistered) {
					mouseMoveTarget.PreviewMouseMove += OnPreviewMouseMove;
					//mouseMoveTarget.PreviewKeyDown += OnKeyDown;
					eventRegistered = true;
				}
				if (!event2Registered && master == null) {
					if (Window.GetWindow(this) != null) {
						Window.GetWindow(this).PreviewKeyDown += OnKeyDown;
						Window.GetWindow(this).PreviewKeyUp += OnKeyUp;
						event2Registered = true;
					}
				}

				// Do this to make sure the pickup element draws correctly the first time.
				// This was the best method that worked.
				if (InitializeAdornerLayer())
					RemoveAdornerLayer();
			}
		}
		private void OnUnloaded(object sender, RoutedEventArgs e) {
			if (eventRegistered) {
				mouseMoveTarget.PreviewMouseMove -= OnPreviewMouseMove;
				//mouseMoveTarget.PreviewKeyDown -= OnKeyDown;
				eventRegistered = false;
			}
			if (event2Registered) {
				if (Window.GetWindow(this) != null) {
					Window.GetWindow(this).PreviewKeyDown -= OnKeyDown;
					Window.GetWindow(this).PreviewKeyUp -= OnKeyUp;
					event2Registered = false;
				}
			}
		}

		#endregion

		#region Implemented

		private bool RevealEggs {
			get {
				if (pokeContainer.PokePC is ManagerPokePC && (pokeContainer.PokePC as ManagerPokePC).RevealEggs)
					return true;
				return PokeManager.Settings.RevealEggs;
			}
		}

		private bool IsLivingDex {
			get {
				if (pokeContainer.PokePC is ManagerPokePC)
					return (pokeContainer.PokePC as ManagerPokePC).IsLivingDex;
				else if (pokeContainer.GameIndex >= 0)
					return PokeManager.GetGameSaveFileInfoAt(pokeContainer.GameIndex).IsLivingDex;
				return false;
			}
		}
		private bool IsUnownLivingBox {
			get {
				return (IsLivingDex && PokeBox.Name.ToLower() == "unown");
			}
		}
		private int LivingDexStartNumber {
			get {
				if (IsLivingDex && PokeBox.Name.Length >= 3) {
					int outNum;
					int dashIndex = PokeBox.Name.IndexOf('-');
					if (dashIndex > 0) {
						if (int.TryParse(PokeBox.Name.Substring(0, dashIndex), out outNum))
							return outNum;
					}
				}
				return -1;
			}
		}
		private int LivingDexEndNumber {
			get {
				if (IsLivingDex && PokeBox.Name.Length >= 3) {
					int outNum;
					int dashIndex = PokeBox.Name.IndexOf('-');
					if (dashIndex > 0 && dashIndex + 1 < PokeBox.Name.Length) {
						if (int.TryParse(PokeBox.Name.Substring(dashIndex + 1, PokeBox.Name.Length - dashIndex - 1), out outNum))
							return outNum;
					}
				}
				return -1;
			}
		}

		public void RefreshUI(bool onlyThis = false) {
			if (pokeContainer == null) {
				UnloadBox();
				return;
			}
			if (IsViewingPurifier) {
				this.pokeContainer = ((XDPokePC)pokeContainer.PokePC).GetChamber(chamberIndex);
				this.labelChamberNumber.Content = (chamberIndex + 1).ToString();
			}
			if (movingPokemon && !PokeManager.IsHoldingPokemon) {
				RemoveAdornerLayer();
				movingPokemon = false;
			}

			if (!onlyThis) {
				foreach (PokeBoxControl slave in slaves) {
					slave.RefreshUI();
				}
			}

			if (IsViewingParty) {
				gridParty.Visibility = Visibility.Visible;
				gridDaycare.Visibility = Visibility.Hidden;
				gridBox.Visibility = Visibility.Hidden;
				gridPurifier.Visibility = Visibility.Hidden;

				if (hoverIndex != -1) {
					imagePartySelector.Margin = new Thickness(31 + (hoverIndex % 3) * 32, 51 + (hoverIndex / 3) * 25, 0, 0);
					imagePartySelector.Visibility = Visibility.Visible;
				}
				else {
					imagePartySelector.Visibility = Visibility.Hidden;
				}

				for (int i = 0; i < pokeContainer.NumSlots; i++) {
					if (pokeContainer[i] != null) {
						partySlotImages[i].Visibility = Visibility.Visible;
						if (RevealEggs && pokeContainer[i].IsEgg) {
							if (PokeManager.IsAprilFoolsMode)
								partyImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(41, false);
							else
								partyImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(pokeContainer[i].DexID, false, pokeContainer[i].FormID);
							partyEggMarkers[i].Source = ResourceDatabase.GetImageFromName((PokeManager.Settings.UseNewBoxSprites ? "New" : "") + "SideEgg");
							partyEggMarkers[i].Visibility = Visibility.Visible;
						}
						else {
							if (PokeManager.IsAprilFoolsMode && !pokeContainer[i].IsEgg)
								partyImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(41, pokeContainer[i].IsShiny);
							else
								partyImages[i].Source = pokeContainer[i].BoxSprite;
							partyEggMarkers[i].Visibility = Visibility.Hidden;
						}
						if (pokeContainer[i].IsShadowPokemon) {
							partyShadowMasks[i].OpacityMask = new ImageBrush(partyImages[i].Source);
							partyShadowMasks[i].Visibility = Visibility.Visible;
						}
						else {
							partyShadowMasks[i].Visibility = Visibility.Hidden;
						}
					}
					else {
						partyImages[i].Source = null;
						partyShadowMasks[i].OpacityMask = null;
						partySlotImages[i].Visibility = Visibility.Hidden;
						partyShadowMasks[i].Visibility = Visibility.Hidden;
						partyEggMarkers[i].Visibility = Visibility.Hidden;
					}
				}
			}
			else if (IsViewingPurifier) {
				gridPurifier.Visibility = Visibility.Visible;
				gridParty.Visibility = Visibility.Hidden;
				gridDaycare.Visibility = Visibility.Hidden;
				gridBox.Visibility = Visibility.Hidden;

				if (hoverIndex != -1) {
					imagePurifierSelector.Margin = new Thickness(31 + PurifierSlotOffsets[hoverIndex].X * 32, 32 + PurifierSlotOffsets[hoverIndex].Y * 25, 0, 0);
					imagePurifierSelector.Visibility = Visibility.Visible;
				}
				else {
					imagePurifierSelector.Visibility = Visibility.Hidden;
				}

				for (int i = 0; i < pokeContainer.NumSlots; i++) {
					if (pokeContainer[i] != null) {
						purifierSlotImages[i].Visibility = Visibility.Visible;
						if (PokeManager.IsAprilFoolsMode && !pokeContainer[i].IsEgg)
							purifierImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(41, pokeContainer[i].IsShiny);
						else
							purifierImages[i].Source = pokeContainer[i].BoxSprite;
						if (pokeContainer[i].IsShadowPokemon) {
							purifierShadowMasks[i].OpacityMask = new ImageBrush(purifierImages[i].Source);
							purifierShadowMasks[i].Visibility = Visibility.Visible;
						}
						else {
							purifierShadowMasks[i].Visibility = Visibility.Hidden;
						}
					}
					else {
						purifierImages[i].Source = null;
						purifierShadowMasks[i].OpacityMask = null;
						purifierSlotImages[i].Visibility = Visibility.Hidden;
						purifierShadowMasks[i].Visibility = Visibility.Hidden;
					}
				}
			}
			else if (IsViewingDaycare) {
				gridParty.Visibility = Visibility.Hidden;
				gridDaycare.Visibility = Visibility.Visible;
				gridBox.Visibility = Visibility.Hidden;
				gridPurifier.Visibility = Visibility.Hidden;

				if (hoverIndex != -1) {
					imageDaycareSelector.Margin = new Thickness(31 + (hoverIndex % 3) * 32, 61, 0, 0);
					imageDaycareSelector.Visibility = Visibility.Visible;
				}
				else {
					imageDaycareSelector.Visibility = Visibility.Hidden;
				}

				for (int i = 0; i < 3; i++) {
					if (i < pokeContainer.NumSlots) {
						daycareSlotCoverImages[i].Visibility = Visibility.Hidden;
						daycareClickAreas[i].IsEnabled = true;
						if (pokeContainer[i] != null) {

							if (PokeManager.IsAprilFoolsMode && !pokeContainer[i].IsEgg)
								daycareImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(41, pokeContainer[i].IsShiny);
							else
								daycareImages[i].Source = pokeContainer[i].BoxSprite;
							daycareSlotImages[i].Visibility = Visibility.Visible;
							if (pokeContainer[i].IsShadowPokemon) {
								daycareShadowMasks[i].OpacityMask = new ImageBrush(daycareImages[i].Source);
								daycareShadowMasks[i].Visibility = Visibility.Visible;
							}
							else {
								daycareShadowMasks[i].Visibility = Visibility.Hidden;
							}
						}
						else {
							daycareImages[i].Source = null;
							daycareShadowMasks[i].OpacityMask = null;
							daycareSlotImages[i].Visibility = Visibility.Hidden;
							daycareShadowMasks[i].Visibility = Visibility.Hidden;
						}
					}
					else {
						daycareSlotCoverImages[i].Visibility = Visibility.Visible;
						daycareClickAreas[i].IsEnabled = false;
						daycareImages[i].Source = null;
						daycareShadowMasks[i].OpacityMask = null;
						daycareSlotImages[i].Visibility = Visibility.Hidden;
						daycareShadowMasks[i].Visibility = Visibility.Hidden;
					}
				}
			}
			else if (IsViewingBox) {
				gridParty.Visibility = Visibility.Hidden;
				gridDaycare.Visibility = Visibility.Hidden;
				gridBox.Visibility = Visibility.Visible;
				gridPurifier.Visibility = Visibility.Hidden;

				if (hoverIndex != -1) {
					imageBoxSelector.Margin = new Thickness(1 + (hoverIndex % 6) * 24, 5 + (hoverIndex / 6) * 24, 0, 0);
					imageBoxSelector.Visibility = Visibility.Visible;
				}
				else {
					imageBoxSelector.Visibility = Visibility.Hidden;
				}

				this.labelBoxName.Content = PokeBox.Name;

				imageWallpaper.Source = PokeBox.WallpaperImage;

				for (int i = 0; i < pokeContainer.NumSlots; i++) {
					if (pokeContainer[i] != null) {
						boxImages[i].Opacity = 1;
						if (RevealEggs && pokeContainer[i].IsEgg) {
							if (PokeManager.IsAprilFoolsMode)
								boxImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(41, false);
							else
								boxImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(pokeContainer[i].DexID, false, pokeContainer[i].FormID);
							boxEggMarkers[i].Source = ResourceDatabase.GetImageFromName((PokeManager.Settings.UseNewBoxSprites ? "New" : "") + "SideEgg");
							boxEggMarkers[i].Visibility = Visibility.Visible;
						}
						else {
							if (PokeManager.IsAprilFoolsMode && !pokeContainer[i].IsEgg)
								boxImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(41, pokeContainer[i].IsShiny);
							else
								boxImages[i].Source = pokeContainer[i].BoxSprite;
							boxEggMarkers[i].Visibility = Visibility.Hidden;
						}
						if (pokeContainer[i].IsShadowPokemon) {
							boxShadowMasks[i].OpacityMask = new ImageBrush(boxImages[i].Source);
							boxShadowMasks[i].Visibility = Visibility.Visible;
						}
						else {
							boxShadowMasks[i].Visibility = Visibility.Hidden;
						}
						if (PokeManager.IsPokemonSelected(pokeContainer[i])) {
							boxSelectMasks[i].OpacityMask = new ImageBrush(boxImages[i].Source);
							boxSelectMasks[i].Visibility = Visibility.Visible;
						}
						else {
							boxSelectMasks[i].OpacityMask = null;
							boxSelectMasks[i].Visibility = Visibility.Hidden;
						}
					}
					else {
						boxEggMarkers[i].Visibility = Visibility.Hidden;
						boxImages[i].Opacity = 1;
						boxImages[i].Source = null;
						boxShadowMasks[i].OpacityMask = null;
						boxShadowMasks[i].Visibility = Visibility.Hidden;
						boxSelectMasks[i].OpacityMask = null;
						boxSelectMasks[i].Visibility = Visibility.Hidden;
						double livingOpacity = 0.4;
						if (IsUnownLivingBox && i < 28) {
							boxImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID(201, false, (byte)i);
							boxImages[i].Opacity = livingOpacity;
						}
						else {
							int start = LivingDexStartNumber;
							int end = LivingDexEndNumber;
							if (start != -1 && end != -2) {
								start = Math.Max(0, Math.Min(386, start));
								end = Math.Max(start, Math.Min(386, end));
								if (start + i <= end) {
									boxImages[i].Source = PokemonDatabase.GetPokemonBoxImageFromDexID((ushort)(start + i), false);
									boxImages[i].Opacity = livingOpacity;
								}
							}
						}
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

		#endregion

		private void OnKeyDown(object sender, KeyEventArgs e) {
			if (IsVisible && !e.IsRepeat) {
				if (e.Key == Key.R) {
					if (canChangePickupMode) {
						if (master != null)
							master.IsPickupMode = !master.IsPickupMode;
						else
							IsPickupMode = !IsPickupMode;
					}
				}
				else if (e.Key == Key.T) {
					IsSummaryMode = !IsSummaryMode;
				}
				else if (e.Key == Key.LeftShift) {
					//if (master != null)
					//	master.IsSummaryMode = !master.IsSummaryMode;
					//else
						IsSummaryMode = true;
				}
			}
		}
		private void OnKeyUp(object sender, KeyEventArgs e) {
			if (IsVisible && !e.IsRepeat) {
				if (e.Key == Key.LeftShift) {
					IsSummaryMode = false;
				}
			}
		}

		#region Interfacing

		public void EnableAdorner() {
			if (!movingPokemon && PokeManager.IsHoldingPokemon) {
				pickupBoxImage.Source = PokeManager.HoldPokemon.Pokemon.BoxSprite;
				ImageBrush brush = new ImageBrush(PokeManager.HoldPokemon.Pokemon.BoxSprite);
				brush.Stretch = Stretch.None;
				pickupShadowMask.OpacityMask = brush;
				// Until WPF stops being a bitch with the random stretching we'll have to hide the shadow mask
				pickupShadowMask.Visibility = Visibility.Hidden;// (PokeManager.HoldPokemon.Pokemon.IsShadowPokemon ? Visibility.Visible : Visibility.Hidden);
				pickupCount.Visibility = (PokeManager.IsHoldingSelection ? Visibility.Visible : Visibility.Hidden);
				pickupCount.Content = PokeManager.NumSelectedPokemon.ToString();
				InitializeAdornerLayer();
				movingPokemon = true;
				OnPreviewMouseMove(null, null);
			}
		}
		public void DisableAdorner() {
			if (movingPokemon) {
				RemoveAdornerLayer();
				movingPokemon = false;
			}
		}

		#endregion

		private void TrySelectPokemon(PokeBoxTagStructure tag) {
			IPokemon pkm = tag.Control.pokeContainer[tag.BoxIndex];
			if (mode == PokeBoxControlModes.MovePokemon && pkm != null) {
				bool safeToSelect = true;
				// Make sure we don't select every pokemon in a party. Otherwise we could move them all outside of the party and make the game unsafe.
				if (tag.Control.IsViewingParty && !PokeManager.IsPokemonSelected(pkm)) {
					safeToSelect = false;
					foreach (IPokemon partyPkm in tag.Control.pokeContainer) {
						if (!PokeManager.IsPokemonSelected(partyPkm) && partyPkm != pkm) {
							safeToSelect = true;
							break;
						}
					}
				}
				// Note: Temporarily dissallowing party selection
				if (IsViewingBox) {
					PokeManager.TogglePokemonSelection(pkm);
					PokeManager.RefreshUI();
					/*if (PokeManager.IsPokemonSelected(pkm)) {
						if (tag.Control.IsViewingParty) {
							tag.Control.partySelectMasks[tag.BoxIndex].OpacityMask = new ImageBrush(pkm.BoxSprite);
							tag.Control.partySelectMasks[tag.BoxIndex].Visibility = Visibility.Visible;
						}
						else {
							tag.Control.boxSelectMasks[tag.BoxIndex].OpacityMask = new ImageBrush(pkm.BoxSprite);
							tag.Control.boxSelectMasks[tag.BoxIndex].Visibility = Visibility.Visible;
						}
					}
					else {
						if (tag.Control.IsViewingParty) {
							tag.Control.partySelectMasks[tag.BoxIndex].OpacityMask = null;
							tag.Control.partySelectMasks[tag.BoxIndex].Visibility = Visibility.Hidden;
						}
						else {
							tag.Control.boxSelectMasks[tag.BoxIndex].OpacityMask = null;
							tag.Control.boxSelectMasks[tag.BoxIndex].Visibility = Visibility.Hidden;
						}
					}*/
				}
			}
		}
		private void TryMovePokemon(PokeBoxTagStructure tag) {
			IPokemon pkm = tag.Control.pokeContainer[tag.BoxIndex];
			if (mode == PokeBoxControlModes.MovePokemon) {
				if (PokeManager.IsHoldingPokemon) {
					if (!PokeManager.CanSafelyPlaceHeldUnknownItem(tag.Control.pokeContainer)) {
						MessageBoxResult unknownItemResult = TriggerMessageBox.Show(Window.GetWindow(this), "A Pokémon that you are holding is holding an Unknown Item. Sending it to a different game may cause problems. Are you sure you want to place it?", "Unknown Item", MessageBoxButton.YesNo);
						if (unknownItemResult == MessageBoxResult.No)
							return;
					}
					if (!PokeManager.CanPlaceShadowPokemon(tag.Control.pokeContainer)) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot move Shadow Pokémon to a different game until they have been purified", "Can't Place");
					}
					else if (!PokeManager.CanPlaceEgg(tag.Control.pokeContainer)) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot move eggs to Colosseum or XD", "Can't Place");
					}
					else if (tag.Control.IsViewingPurifier && tag.BoxIndex != 0 && PokeManager.HoldPokemon.Pokemon.IsShadowPokemon) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot put Shadow Pokémon in that Purifier slot", "No Shadow Pokémon");
					}
					else if (tag.Control.IsViewingPurifier && tag.BoxIndex == 0 && !PokeManager.HoldPokemon.Pokemon.IsShadowPokemon) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Only Shadow Pokémon can go in that Purifier slot", "No Regular Pokémon");
					}
					else if (pkm == null) {
						if (PokeManager.IsHoldingSelection) {
							if (tag.Control.IsViewingPurifier) {
								TriggerMessageBox.Show(Window.GetWindow(this), "Cannot place a selection in the Purifier", "No Selections Allowed");
							}
							else if (tag.Control.IsViewingDaycare) {
								TriggerMessageBox.Show(Window.GetWindow(this), "Cannot place a selection in the Daycare", "No Selections Allowed");
							}
							else if (tag.Control.IsViewingParty) {
								TriggerMessageBox.Show(Window.GetWindow(this), "Cannot place a selection in your party", "No Selections Allowed");
							}
							else if (tag.Control.pokeContainer.PokePC.HasRoomForPokemon(PokeManager.NumSelectedPokemon)) {
								PokeManager.PlaceSelection(tag.Control.pokeContainer, tag.BoxIndex);
								RefreshUI();
								PokemonViewer.RefreshUI();
								PokeManager.ManagerWindow.RefreshSearchResultsUI();
								PokeManager.ManagerWindow.RefreshStoredPokemon();
							}
							else {
								TriggerMessageBox.Show(Window.GetWindow(this), "Not enough room to place the selected Pokémon", "No Room");
							}
						}
						else if (tag.Control.IsViewingDaycare && PokeManager.HoldPokemon.Pokemon.IsShadowPokemon && tag.Control.pokeContainer.GameType == GameTypes.XD) {
							TriggerMessageBox.Show(Window.GetWindow(this), "Cannot put Shadow Pokémon in the daycare in XD", "No Shadow Pokémon");
						}
						else if (tag.Control.IsViewingDaycare && PokeManager.HoldPokemon.Pokemon.IsEgg) {
							TriggerMessageBox.Show(Window.GetWindow(this), "Cannot put Eggs in the daycare", "No Eggs");
						}
						else {
							PokeManager.PlacePokemon(tag.Control.pokeContainer, tag.BoxIndex);
							RefreshUI();
							PokemonViewer.RefreshUI();
							PokeManager.ManagerWindow.RefreshSearchResultsUI();
							PokeManager.ManagerWindow.RefreshStoredPokemon();
						}
					}
					else if (!PokeManager.CanSwitchShadowPokemon(pkm)) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot switch with that Pokémon. There is nowhere to safely force drop the Shadow Pokémon", "Nowhere to Drop");
					}
					else if (!PokeManager.CanSwitchEgg(pkm)) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot switch with that Pokémon. There is nowhere to safely force drop the Egg", "Nowhere to Drop");
					}
					else if (PokeManager.CanSwitchPokemon(pkm)) {
						MessageBoxResult daycareResult = MessageBoxResult.OK;
						bool refreshAll = false;
						if (tag.Control.IsViewingDaycare) {
							if (PokeManager.HoldPokemon.Pokemon.IsShadowPokemon && tag.Control.pokeContainer.GameType == GameTypes.XD) {
								TriggerMessageBox.Show(Window.GetWindow(this), "Cannot put Shadow Pokémon in the daycare in XD", "No Shadow Pokémon");
								daycareResult = MessageBoxResult.Cancel;
							}
							else if (tag.Control.IsViewingDaycare && PokeManager.HoldPokemon.Pokemon.IsEgg) {
								TriggerMessageBox.Show(Window.GetWindow(this), "Cannot put Eggs in the daycare", "No Eggs");
								daycareResult = MessageBoxResult.Cancel;
							}
							else {
								uint cost = tag.Control.Daycare.GetWithdrawCost(tag.BoxIndex);
								if (cost != 0) {
									if (pkm.GameSave.Money >= cost) {
										daycareResult = TriggerMessageBox.Show(Window.GetWindow(this), "If you would like to take back " + pkm.Nickname + " from the daycare, it will cost $" + cost.ToString("#,0"), "Daycare Fee", MessageBoxButton.OKCancel);
										if (daycareResult == MessageBoxResult.OK)
											pkm.GameSave.Money -= cost;
									}
									else {
										TriggerMessageBox.Show(Window.GetWindow(this), "You do not have enough Pokédollars to withdraw " + pkm.Nickname + ". You need $" + cost.ToString("#,0"), "Not Enough Money");
										daycareResult = MessageBoxResult.Cancel;
									}
								}
								if (daycareResult == MessageBoxResult.OK && pkm.PokeContainer is GBADaycare) {
									GBADaycare daycare = (GBADaycare)pkm.PokeContainer;
									if (daycare.HasLearnedNewMoves(pkm.ContainerIndex)) {
										daycareResult = TriggerMessageBox.Show(Window.GetWindow(this), "Would you like to keep the new moves " + pkm.Nickname + " has learned in the daycare?", "Keep New Moves", MessageBoxButton.YesNo);
										if (daycareResult == MessageBoxResult.No) {
											daycare.CancelLearnedMoves(pkm.ContainerIndex);
											PokemonViewer.RefreshUI();
										}
									}
								}
							}
						}

						if (daycareResult != MessageBoxResult.Cancel) {
							IPokemon switchPokemon = PokeManager.HoldPokemon.Pokemon;
							PokeManager.SwitchPokemon(pkm);
							if (refreshAll) {
								PokeManager.RefreshUI();
							}
							else {
								RefreshUI();
								PokemonViewer.RefreshUI();
								PokeManager.ManagerWindow.RefreshSearchResultsUI();
							}
						}
					}
					else if (PokeManager.IsPartyHoldingMail(pkm.PokeContainer)) {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot switch with that Pokémon. One of the Pokémon in your party is holding mail. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Pickup");
					}
					else {
						TriggerMessageBox.Show(Window.GetWindow(this), "Cannot switch with that Pokémon. It is the last valid Pokémon in your party", "Can't Pickup");
					}
				}
				else if (pkm != null) {
					MessageBoxResult daycareResult = MessageBoxResult.OK;
					if (tag.Control.IsViewingDaycare) {
						uint cost = tag.Control.Daycare.GetWithdrawCost(tag.BoxIndex);
						if (cost != 0) {
							if (pkm.GameSave.Money >= cost) {
								daycareResult = TriggerMessageBox.Show(Window.GetWindow(this), "If you would like to take back " + pkm.Nickname + " from the daycare, it will cost $" + cost.ToString("#,0"), "Daycare Fee", MessageBoxButton.OKCancel);
								if (daycareResult == MessageBoxResult.OK)
									pkm.GameSave.Money -= cost;
							}
							else {
								TriggerMessageBox.Show(Window.GetWindow(this), "You do not have enough Pokédollars to withdraw " + pkm.Nickname + ". You need $" + cost.ToString("#,0"), "Not Enough Money");
								daycareResult = MessageBoxResult.Cancel;
							}
						}
						if (daycareResult == MessageBoxResult.OK && pkm.PokeContainer is GBADaycare) {
							GBADaycare daycare = (GBADaycare)pkm.PokeContainer;
							if (daycare.HasLearnedNewMoves(pkm.ContainerIndex)) {
								daycareResult = TriggerMessageBox.Show(Window.GetWindow(this), "Would you like to keep the new moves " + pkm.Nickname + " has learned in the daycare?", "Keep New Moves", MessageBoxButton.YesNo);
								if (daycareResult == MessageBoxResult.No) {
									daycare.CancelLearnedMoves(pkm.ContainerIndex);
									PokemonViewer.RefreshUI();
								}
							}
						}
					}

					if (daycareResult != MessageBoxResult.Cancel) {
						if (PokeManager.IsPokemonSelected(pkm))
							PokeManager.PickupSelection(this);
						else if (PokeManager.CanPickupPokemon(pkm))
							PokeManager.PickupPokemon(pkm, this);
						else if (PokeManager.IsPartyHoldingMail(pkm.PokeContainer))
							TriggerMessageBox.Show(Window.GetWindow(this), "Cannot pickup that Pokémon. One of the Pokémon in your party is holding mail. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Pickup");
						else
							TriggerMessageBox.Show(Window.GetWindow(this), "Cannot pickup that Pokémon. It is the last valid Pokémon in your party", "Can't Pickup");
						RefreshUI();
					}
				}
			}
		}

		private void OnBoxSlotClicked(object sender, MouseButtonEventArgs e) {
			if (pokeContainer == null)
				return;

			if (master != null) {
				master.OnBoxSlotClicked(sender, e);
				return;
			}
			this.imageDaycareHighlighter.Visibility = Visibility.Hidden;
			this.imageBoxHighlighter.Visibility = Visibility.Hidden;
			this.imagePartyHighlighter.Visibility = Visibility.Hidden;
			this.imagePurifierHighlighter.Visibility = Visibility.Hidden;

			foreach (PokeBoxControl slave in slaves) {
				slave.imageDaycareHighlighter.Visibility = Visibility.Hidden;
				slave.imageBoxHighlighter.Visibility = Visibility.Hidden;
				slave.imagePartyHighlighter.Visibility = Visibility.Hidden;
				slave.imagePurifierHighlighter.Visibility = Visibility.Hidden;
			}

			Rectangle selector = sender as Rectangle;
			PokeBoxTagStructure tag = selector.Tag as PokeBoxTagStructure;
			IPokemon pkm = tag.Control.pokeContainer[tag.BoxIndex];
			if (e.ChangedButton == MouseButton.Left) {
				if (pkm != null && pokemonViewer != null)
					pokemonViewer.LoadPokemon(pkm);

				PokemonSelectedEventArgs args = new PokemonSelectedEventArgs();
				args.PokeContainer = tag.Control.pokeContainer;
				args.Index = tag.BoxIndex;
				args.Pokemon = pkm;
				OnPokemonSelected(args);

				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
					TrySelectPokemon(tag);
				}
				else {
					if (!PokeManager.IsHoldingPokemon && (pkm == null || !PokeManager.IsPokemonSelected(pkm) || !pickupMode)) {
						PokeManager.ClearSelectedPokemon();
						RefreshUI();
					}
					if (pickupMode)
						TryMovePokemon(tag);
				}
			}
			else if (e.ChangedButton == MouseButton.Middle) {
				if (canChangePickupMode) {
					IsPickupMode = !IsPickupMode;
				}
			}
		}
		private void OnBoxSlotEnter(object sender, MouseEventArgs e) {
			if (pokeContainer == null)
				return;
			Rectangle selector = sender as Rectangle;
			int newIndex = ((PokeBoxTagStructure)selector.Tag).BoxIndex;
			if (hoverIndex != newIndex) {
				hoverIndex = newIndex;
				if (IsViewingDaycare) {
					imageDaycareSelector.Margin = new Thickness(31 + (newIndex % 3) * 32, 61 + (newIndex / 3) * 25, 0, 0);
					imageDaycareSelector.Visibility = Visibility.Visible;
				}
				else if (IsViewingParty) {
					imagePartySelector.Margin = new Thickness(31 + (newIndex % 3) * 32, 51 + (newIndex / 3) * 25, 0, 0);
					imagePartySelector.Visibility = Visibility.Visible;
				}
				else if (IsViewingPurifier) {
					imagePurifierSelector.Margin = new Thickness(31 + PurifierSlotOffsets[newIndex].X * 32, 32 + PurifierSlotOffsets[newIndex].Y * 25, 0, 0);
					imagePurifierSelector.Visibility = Visibility.Visible;
				}
				else if (IsViewingBox) {
					imageBoxSelector.Margin = new Thickness(1 + (newIndex % 6) * 24, 5 + (newIndex / 6) * 24, 0, 0);
					imageBoxSelector.Visibility = Visibility.Visible;
				}
				if (summaryMode && pokemonViewer != null && pokemonViewer.ViewedPokemon != pokeContainer[hoverIndex]) {
					pokemonViewer.LoadPokemon(pokeContainer[hoverIndex]);
				}
			}
		}
		private void OnBoxSlotLeave(object sender, MouseEventArgs e) {
			if (pokeContainer == null)
				return;
			Rectangle selector = sender as Rectangle;
			int index = ((PokeBoxTagStructure)selector.Tag).BoxIndex;
			if (hoverIndex == index) {
				hoverIndex = -1;
				imageDaycareSelector.Visibility = Visibility.Hidden;
				imageBoxSelector.Visibility = Visibility.Hidden;
				imagePartySelector.Visibility = Visibility.Hidden;
				imagePurifierSelector.Visibility = Visibility.Hidden;
			}
		}

		private void OnPreviewMouseMove(object sender, MouseEventArgs e) {
			if (movingPokemon) {
				Point ptCursor = new Point();
				int finalHoverIndex = hoverIndex;
				PokeBoxControl finalHoverer = this;
				foreach (PokeBoxControl slave in slaves) {
					if (slave.hoverIndex != -1) {
						finalHoverIndex = slave.hoverIndex;
						finalHoverer = slave;
						break;
					}
				}

				if (finalHoverIndex != -1) {
					if (finalHoverer.IsViewingDaycare)
						ptCursor =  finalHoverer.partyClickAreas[finalHoverIndex].TranslatePoint(new Point(16, 19), adornerContainer);
					else if (finalHoverer.IsViewingParty)
						ptCursor =  finalHoverer.partyClickAreas[finalHoverIndex].TranslatePoint(new Point(16, 9), adornerContainer);
					else if (finalHoverer.IsViewingPurifier)
						ptCursor =  finalHoverer.purifierClickAreas[finalHoverIndex].TranslatePoint(new Point(16, 9), adornerContainer);
					else if (finalHoverer.IsViewingBox)
						ptCursor =  finalHoverer.boxClickAreas[finalHoverIndex].TranslatePoint(new Point(12, 8), adornerContainer);
				}
				else {
					ptCursor = MouseUtilities.GetMousePosition(adornerContainer);
				}
				

				double left = ptCursor.X - 16;
				double top = ptCursor.Y - 24;

				this.pickupDragAdorner.SetOffsets(left, top);
			}
		}

		private void OnEditBoxClicked(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left && mode == PokeBoxControlModes.MovePokemon) {
				OnBoxContextMenuOpening(null, null);
				rectEditBox.ContextMenu.IsOpen = true;
			}
		}

		private void OnEditBoxEnter(object sender, MouseEventArgs e) {
			if (mode == PokeBoxControlModes.MovePokemon) {
				rectEditBox.Opacity = 0.35;
				labelBoxName.Foreground = new SolidColorBrush(Color.FromRgb(0, 220, 220));
			}
		}
		private void OnEditBoxLeave(object sender, MouseEventArgs e) {
			if (mode == PokeBoxControlModes.MovePokemon) {
				rectEditBox.Opacity = 0;
				labelBoxName.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			}
		}

		private bool InitializeAdornerLayer() {
			// Create a brush which will paint the ListViewItem onto
			// a visual in the adorner layer.
			VisualBrush brush = new VisualBrush(pickupElement);

			adornerContainer = (UIElement)this.Parent;

			// Create an element which displays the source item while it is dragged.
			this.pickupDragAdorner = new DragAdorner(adornerContainer, pickupElement.RenderSize, brush);
			this.pickupDragAdorner.UseLayoutRounding = true;
			this.pickupDragAdorner.SnapsToDevicePixels = true;

			AdornerLayer layer = AdornerLayer.GetAdornerLayer(adornerContainer);
			if (layer != null) {
				layer.Add(pickupDragAdorner);
				return true;
			}
			return false;
		}

		private void RemoveAdornerLayer() {
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornerContainer);
			if (adornerLayer != null) {
				adornerLayer.Remove(pickupDragAdorner);
				pickupDragAdorner = null;
			}
		}


		public void HighlightPokemon(IPokemon pokemon) {


			int index = pokemon.PokeContainer.IndexOf(pokemon);
			if (IsViewingDaycare) {
				Rectangle selector = daycareClickAreas[index];
				imageDaycareHighlighter.Margin = new Thickness(31 + (index % 3) * 32, 51 + (index / 3) * 25, 0, 0);
				imageDaycareHighlighter.Visibility = Visibility.Visible;
				this.imageBoxHighlighter.Visibility = Visibility.Hidden;
				this.imagePartyHighlighter.Visibility = Visibility.Hidden;
				this.imagePurifierHighlighter.Visibility = Visibility.Hidden;
			}
			else if (IsViewingParty) {
				Rectangle selector = partyClickAreas[index];
				imagePartyHighlighter.Margin = new Thickness(31 + (index % 3) * 32, 51 + (index / 3) * 25, 0, 0);
				imagePartyHighlighter.Visibility = Visibility.Visible;
				this.imageBoxHighlighter.Visibility = Visibility.Hidden;
				this.imageDaycareHighlighter.Visibility = Visibility.Hidden;
				this.imagePurifierHighlighter.Visibility = Visibility.Hidden;
			}
			else if (IsViewingPurifier) {
				Rectangle selector = partyClickAreas[index];
				imagePurifierHighlighter.Margin = new Thickness(31 + PurifierSlotOffsets[index].X * 32, 32 + PurifierSlotOffsets[index].Y * 25, 0, 0);
				imagePurifierHighlighter.Visibility = Visibility.Visible;
				this.imageBoxHighlighter.Visibility = Visibility.Hidden;
				this.imageDaycareHighlighter.Visibility = Visibility.Hidden;
				this.imagePartyHighlighter.Visibility = Visibility.Hidden;
			}
			else if (IsViewingBox) {
				Rectangle selector = boxClickAreas[index];
				imageBoxHighlighter.Margin = new Thickness(1 + (index % 6) * 24, 5 + (index / 6) * 24, 0, 0);
				imageBoxHighlighter.Visibility = Visibility.Visible;
				this.imagePartyHighlighter.Visibility = Visibility.Hidden;
				this.imageDaycareHighlighter.Visibility = Visibility.Hidden;
				this.imagePurifierHighlighter.Visibility = Visibility.Hidden;
			}
			pokemonViewer.LoadPokemon(pokemon);
		}

		private void OnPurifierPreviousClicked(object sender, RoutedEventArgs e) {
			chamberIndex--;
			if (chamberIndex < 0)
				chamberIndex = 8;
			UnhighlightPokemon();
			PokemonSelectedEventArgs args = new PokemonSelectedEventArgs();
			args.Index = 0;
			args.PokeContainer = pokeContainer;
			args.Pokemon = pokeContainer[0];
			OnPokemonSelected(args);
			RefreshUI(true);
		}

		private void OnPurifierNextClicked(object sender, RoutedEventArgs e) {
			chamberIndex++;
			if (chamberIndex >= 9)
				chamberIndex = 0;
			UnhighlightPokemon();
			PokemonSelectedEventArgs args = new PokemonSelectedEventArgs();
			args.Index = 0;
			args.PokeContainer = pokeContainer;
			args.Pokemon = pokeContainer[0];
			OnPokemonSelected(args);
			RefreshUI(true);
		}

		#region Context Menus

		private void OnPokemonContextMenuOpening(object sender, ContextMenuEventArgs e) {
			Rectangle selector = sender as Rectangle;
			PokeBoxTagStructure tag = selector.Tag as PokeBoxTagStructure;
			IPokemon pkm = pokeContainer[tag.BoxIndex];
			selectedIndex = tag.BoxIndex;

			((MenuItem)contextMenu.Items[0]).Tag = tag;
			((MenuItem)contextMenu.Items[1]).Tag = tag;
			//((MenuItem)contextMenu.Items[2]).Tag = tag;
			((MenuItem)contextMenu.Items[2]).Tag = tag;
			((MenuItem)contextMenu.Items[4]).Tag = tag;

			((MenuItem)contextMenu.Items[0]).IsEnabled = true;
			if (pkm == null) {
				if (IsMovingPokemon)
					((MenuItem)contextMenu.Items[0]).Header = "Place";
				else
					((MenuItem)contextMenu.Items[0]).IsEnabled = false;
			}
			else {
				if (IsMovingPokemon)
					((MenuItem)contextMenu.Items[0]).Header = "Switch";
				else
					((MenuItem)contextMenu.Items[0]).Header = "Move";
			}
			((MenuItem)contextMenu.Items[1]).IsEnabled = pkm != null;
			((MenuItem)contextMenu.Items[2]).IsEnabled = true;
			//((MenuItem)contextMenu.Items[2]).IsEnabled = false;
			((MenuItem)contextMenu.Items[2]).Header = "Send To";
			if (!IsMovingPokemon) {
				if (pkm != null) {
					//((MenuItem)contextMenu.Items[2]).Header = (pkm.IsHoldingItem ? "Take Item" : "Give Item");
					//((MenuItem)contextMenu.Items[2]).IsEnabled = !pkm.IsHoldingMail;
				}

				if (pkm == null)
					((MenuItem)contextMenu.Items[2]).Header = "Send From";
				else if (PokeManager.IsPokemonSelected(pkm))
					((MenuItem)contextMenu.Items[2]).Header = "Send All To";
				else if (!pkm.IsShadowPokemon)
					((MenuItem)contextMenu.Items[2]).Header = "Send To";
				else
					((MenuItem)contextMenu.Items[2]).IsEnabled = false;
			}
			else {
				//((MenuItem)contextMenu.Items[2]).IsEnabled = false;
				((MenuItem)contextMenu.Items[2]).IsEnabled = false;
			}
			((MenuItem)contextMenu.Items[4]).IsEnabled = (pkm != null && !pkm.IsShadowPokemon);

			if (pkm != null && pkm.IsInDaycare) {
				((MenuItem)contextMenu.Items[0]).IsEnabled = false;
				//((MenuItem)contextMenu.Items[2]).IsEnabled = false;
				((MenuItem)contextMenu.Items[2]).IsEnabled = false;
				((MenuItem)contextMenu.Items[4]).IsEnabled = false;
			}
		}
		private void OnBoxContextMenuOpening(object sender, ContextMenuEventArgs e) {
			//((MenuItem)boxContextMenu.Items[0]).IsEnabled = (pokeContainer.GameType != GameTypes.PokemonBox);
			//((MenuItem)boxContextMenu.Items[2]).IsEnabled = (pokeContainer.GameIndex != -1);
			//((MenuItem)boxContextMenu.Items[2]).Header = "Send All To " + PokeManager.Settings.ManagerNickname;
		}
		private void OnContextMenuSendBoxTo(object sender, RoutedEventArgs e) {
			PokemonSelectedEventArgs args = new PokemonSelectedEventArgs();
			master.OnPokemonSelected(args);

			PokeManager.DropAll();
			PokeManager.ClearSelectedPokemon();
			ManagerPokePC pokePC = PokeManager.ManagerGameSave.PokePC as ManagerPokePC;
			for (int i = 0; i < pokePC.NumBoxes; i++) {
				if (pokePC[i].IsEmpty) {
					for (int j = 0; j < 30; j++) {
						pokePC[i][j] = pokeContainer[j];
						pokeContainer[j] = null;
					}
					break;
				}
			}
			RefreshUI();
		}
		private void OnContextMenuEditBoxClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.ClearSelectedPokemon();
			EditBoxWindow.ShowDialog(Window.GetWindow(this), (IPokeBox)pokeContainer);
		}
		private void OnContextMenuSelectAllClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.ClearSelectedPokemon();
			foreach (IPokemon pokemon in pokeContainer) {
				PokeManager.SelectPokemon(pokemon);
			}
			PokeManager.RefreshUI();
		}
		private void OnContextMenuReleaseAllClicked(object sender, RoutedEventArgs e) {
			PokeManager.DropAll();
			PokeManager.ClearSelectedPokemon();
			MessageBoxResult result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to release every Pokémon in this box?", "Release Box", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				PokeManager.ClearSelectedPokemon();
				foreach (IPokemon pokemon in pokeContainer) {
					if (pokemon.IsHoldingItem)
						PokeManager.ManagerGameSave.Inventory.Items[pokemon.HeldItemData.PocketType].AddItem(pokemon.HeldItemID, 1);
					if (PokeManager.IsPokemonSelected(pokemon))
						PokeManager.UnselectPokemon(pokemon);
					pokemon.PokeContainer.Remove(pokemon);
					pokemon.IsReleased = true;
				}
				PokeManager.RefreshUI();
			}
		}

		private void OnContextMenuGiveClicked(object sender, RoutedEventArgs e) {
			PokeManager.ClearSelectedPokemon();
			IPokemon pkm = pokeContainer[selectedIndex];
			if (pkm.IsHoldingItem) {
				PokeManager.ManagerGameSave.Inventory.Items[pkm.HeldItemData.PocketType].AddItem(pkm.HeldItemID, 1);
				TriggerMessageBox.Show(Window.GetWindow(this), "Took " + pkm.HeldItemData.Name + " from " + pkm.Nickname, "Took Item");
				pkm.HeldItemID = 0;
				PokeManager.RefreshUI();
			}
			else {
				Item item = GiveItemWindow.ShowDialog(Window.GetWindow(this), PokeManager.GetGameSaveAt(gameIndex));
				if (item != null) {
					item.Pocket.TossItemAt(item.Pocket.IndexOf(item), 1);
					pkm.HeldItemID = item.ID;
					PokeManager.RefreshUI();
				}
			}
		}
		private void OnContextMenuMoveClicked(object sender, RoutedEventArgs e) {
			if (master != null) {
				master.OnContextMenuMoveClicked(sender, e);
			}
			else  {
				PokeBoxTagStructure tag = ((MenuItem)sender).Tag as PokeBoxTagStructure;
				TryMovePokemon(tag);
			}
		}
		private void OnContextMenuSummaryClicked(object sender, RoutedEventArgs e) {
			if (pokeContainer[selectedIndex] != null)
				pokemonViewer.LoadPokemon(pokeContainer[selectedIndex]);
		}
		private void OnContextMenuSendToClicked(object sender, RoutedEventArgs e) {
			PokemonSelectedEventArgs args = new PokemonSelectedEventArgs();
			if (master != null)
				master.OnPokemonSelected(args);

			if (pokeContainer[selectedIndex] != null && PokeManager.IsPokemonSelected(pokeContainer[selectedIndex])) {
				if (PokeManager.SelectionHasShadowPokemon) {
					TriggerMessageBox.Show(Window.GetWindow(this), "Cannot send Shadow Pokémon to other games", "Can't Send");
				}
				else {
					SendPokemonToWindow.ShowSendMultiDialog(Window.GetWindow(this), pokeContainer.GameIndex);
					PokeManager.RefreshUI();
				}
			}
			else if (pokeContainer[selectedIndex] != null) {
				if (PokeManager.CanPickupPokemon(pokeContainer[selectedIndex])) {
					SendPokemonToWindow.ShowSendToDialog(Window.GetWindow(this), pokeContainer.GameIndex, pokeContainer[selectedIndex]);
					PokeManager.RefreshUI();
				}
				else if (PokeManager.IsPartyHoldingMail(pokeContainer)) {
					TriggerMessageBox.Show(Window.GetWindow(this), "Cannot send that Pokémon. One of the Pokémon in your party is holding mail. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Send");
				}
				else {
					TriggerMessageBox.Show(Window.GetWindow(this), "Cannot send that Pokémon. It is the last valid Pokémon in your party", "Can't Send");
				}
			}
			else {
				SendPokemonToWindow.ShowSendFromDialog(Window.GetWindow(this), pokeContainer.GameIndex, pokeContainer, selectedIndex);
				PokeManager.RefreshUI();
			}
		}
		private void OnContextMenuReleaseClicked(object sender, RoutedEventArgs e) {
			if (PokeManager.CanPickupPokemon(pokeContainer[selectedIndex])) {
				MessageBoxResult result = MessageBoxResult.Yes;
				if (PokeManager.Settings.ReleaseConfirmation)
					result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to release " + pokeContainer[selectedIndex].Nickname + "?", "Release Pokemon", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes) {
					if (pokeContainer[selectedIndex].IsHoldingItem)
						PokeManager.ManagerGameSave.Inventory.Items[pokeContainer[selectedIndex].HeldItemData.PocketType].AddItem(pokeContainer[selectedIndex].HeldItemID, 1);
					// Be freeeeeeeeeeee (in the void forever)
					if (PokeManager.IsPokemonSelected(pokeContainer[selectedIndex]))
						PokeManager.UnselectPokemon(pokeContainer[selectedIndex]);
					pokeContainer[selectedIndex].IsReleased = true;
					pokeContainer[selectedIndex] = null;
					PokeManager.RefreshUI();
				}
			}
			else if (PokeManager.IsPartyHoldingMail(pokeContainer)) {
				TriggerMessageBox.Show(Window.GetWindow(this), "Cannot release that Pokémon. One of the Pokémon in your party is holding mail. To remove the mail goto the mailbox tab and click Take Mail From Party", "Can't Release");
			}
			else {
				TriggerMessageBox.Show(Window.GetWindow(this), "Cannot release that Pokémon. It is the last valid Pokémon in your party", "Can't Release");
			}
			
		}

		#endregion

		#region Creation

		private void CreatePickupElement() {
			pickupElement = new Grid();
			pickupElement.Width = 100;
			pickupElement.Height = 36;
			pickupElement.HorizontalAlignment = HorizontalAlignment.Left;
			pickupElement.VerticalAlignment = VerticalAlignment.Top;
			pickupElement.UseLayoutRounding = true;
			pickupElement.SnapsToDevicePixels = true;
			pickupElement.IsHitTestVisible = false;
			//pickupElement.Margin = new Thickness();

			Grid grid = new Grid();
			grid.HorizontalAlignment = HorizontalAlignment.Left;
			grid.VerticalAlignment = VerticalAlignment.Top;
			grid.Width = 32;
			grid.Height = 32;
			grid.Margin = new Thickness();

			Image shadow = new Image();
			shadow.HorizontalAlignment = HorizontalAlignment.Left;
			shadow.VerticalAlignment = VerticalAlignment.Top;
			shadow.SnapsToDevicePixels = true;
			shadow.UseLayoutRounding = true;
			shadow.Stretch = Stretch.None;
			shadow.Width = 100;
			shadow.Height = 36;
			shadow.Source = ResourceDatabase.GetImageFromName("BoxPokemonShadow");

			pickupBoxImage = new Image();
			pickupBoxImage.HorizontalAlignment = HorizontalAlignment.Left;
			pickupBoxImage.VerticalAlignment = VerticalAlignment.Top;
			pickupBoxImage.SnapsToDevicePixels = true;
			pickupBoxImage.UseLayoutRounding = true;
			pickupBoxImage.Stretch = Stretch.None;
			pickupBoxImage.Width = 100;
			pickupBoxImage.Height = 32;

			// Used for shadow pokemon tints
			pickupShadowMask = new Rectangle();
			pickupShadowMask.HorizontalAlignment = HorizontalAlignment.Left;
			pickupShadowMask.VerticalAlignment = VerticalAlignment.Top;
			pickupShadowMask.Width = 100;
			pickupShadowMask.Height = 32;
			pickupShadowMask.StrokeThickness = 0;
			pickupShadowMask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));

			pickupCount = new Label();
			pickupCount.HorizontalAlignment = HorizontalAlignment.Left;
			pickupCount.VerticalAlignment = VerticalAlignment.Top;
			pickupCount.Background = new SolidColorBrush(Color.FromRgb(240, 240, 248));
			pickupCount.FontSize = 10;
			pickupCount.Margin = new Thickness(22, 6, 0, 0);
			pickupCount.FontWeight = FontWeights.Bold;
			pickupCount.Padding = new Thickness(2, 0, 2, 1);
			pickupCount.BorderThickness = new Thickness(1, 1, 1, 1);
			pickupCount.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

			//grid.Children.Add(pickupBoxImage);
			//grid.Children.Add(pickupShadowMask);
			pickupElement.Children.Add(shadow);
			pickupElement.Children.Add(pickupBoxImage);
			pickupElement.Children.Add(pickupShadowMask);
			//pickupElement.Children.Add(grid);
			pickupElement.Children.Add(pickupCount);
		}
		private void CreatePickupElement2() {
			pickupElement = new Grid();
			pickupElement.Width = 100;
			pickupElement.Height = 36;
			//pickupElement.HorizontalAlignment = HorizontalAlignment.Left;
			//pickupElement.VerticalAlignment = VerticalAlignment.Top;
			pickupElement.UseLayoutRounding = true;
			pickupElement.SnapsToDevicePixels = true;
			pickupElement.IsHitTestVisible = false;

			Image shadow = new Image();
			shadow.HorizontalAlignment = HorizontalAlignment.Left;
			shadow.VerticalAlignment = VerticalAlignment.Top;
			shadow.SnapsToDevicePixels = true;
			shadow.UseLayoutRounding = true;
			shadow.Stretch = Stretch.None;
			shadow.Width = 100;
			shadow.Height = 36;
			shadow.Source = ResourceDatabase.GetImageFromName("BoxPokemonShadow");

			Image image = new Image();
			image.HorizontalAlignment = HorizontalAlignment.Left;
			image.VerticalAlignment = VerticalAlignment.Top;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;
			image.Stretch = Stretch.None;
			image.Width = 100;
			image.Height = 32;

			// Used for shadow pokemon tints
			Rectangle mask = new Rectangle();
			mask.HorizontalAlignment = HorizontalAlignment.Left;
			mask.VerticalAlignment = VerticalAlignment.Top;
			mask.Width = 100;
			mask.Height = 32;
			mask.StrokeThickness = 0;
			mask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));

			Label count = new Label();
			count.HorizontalAlignment = HorizontalAlignment.Left;
			count.VerticalAlignment = VerticalAlignment.Top;
			count.Background = new SolidColorBrush(Color.FromRgb(240, 240, 248));
			count.FontSize = 10;
			count.Margin = new Thickness(22, 6, 0, 0);
			count.FontWeight = FontWeights.Bold;
			count.Padding = new Thickness(2, 0, 2, 1);
			count.BorderThickness = new Thickness(1, 1, 1, 1);
			count.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

			pickupElement.Children.Add(shadow);
			pickupElement.Children.Add(image);
			pickupElement.Children.Add(mask);
			pickupElement.Children.Add(count);
		}
		private void CreatePartyElements() {
			this.partyImages = new List<Image>();
			this.partyShadowMasks = new List<Rectangle>();
			this.partySlotImages = new List<Image>();
			this.partyClickAreas = new List<Rectangle>();
			this.partyEggMarkers = new List<Image>();

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
					slotImage.Source = ResourceDatabase.GetImageFromName("PartySlot");
					slotImage.Visibility = Visibility.Hidden;
					gridParty.Children.Insert(gridParty.Children.Count - 2, slotImage);
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
					gridParty.Children.Insert(gridParty.Children.Count - 2, image);

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
					gridParty.Children.Insert(gridParty.Children.Count - 2, mask);

					Image egg = new Image();
					egg.Stretch = Stretch.None;
					egg.SnapsToDevicePixels = true;
					egg.UseLayoutRounding = true;
					egg.Width = 9;
					egg.Height = 11;
					egg.Margin = new Thickness(30 + j * 32 + 20, 49 + i * 25 + 21, 0, 0);
					egg.HorizontalAlignment = HorizontalAlignment.Left;
					egg.VerticalAlignment = VerticalAlignment.Top;
					gridParty.Children.Insert(gridParty.Children.Count - 2, egg);

					partyImages.Add(image);
					partyShadowMasks.Add(mask);
					partyEggMarkers.Add(egg);
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
					clickArea.Tag = new PokeBoxTagStructure(this, i * 3 + j);
					clickArea.Margin = new Thickness(30 + j * 32, 56 + i * 25, 0, 0);
					clickArea.HorizontalAlignment = HorizontalAlignment.Left;
					clickArea.VerticalAlignment = VerticalAlignment.Top;
					clickArea.PreviewMouseDown += OnBoxSlotClicked;
					clickArea.MouseEnter += OnBoxSlotEnter;
					clickArea.MouseLeave += OnBoxSlotLeave;
					clickArea.ContextMenu = contextMenu;
					clickArea.ContextMenuOpening += OnPokemonContextMenuOpening;
					partyClickAreas.Add(clickArea);
					gridParty.Children.Insert(gridParty.Children.Count - 2, clickArea);
				}
			}
		}
		private void CreateDaycareElements() {
			this.daycareImages = new List<Image>();
			this.daycareShadowMasks = new List<Rectangle>();
			this.daycareSlotImages = new List<Image>();
			this.daycareSlotCoverImages = new List<Image>();
			this.daycareClickAreas = new List<Rectangle>();

			for (int j = 0; j < 3; j++) {
				Image slotCoverImage = new Image();
				slotCoverImage.Stretch = Stretch.None;
				slotCoverImage.SnapsToDevicePixels = true;
				slotCoverImage.UseLayoutRounding = true;
				slotCoverImage.Width = 30;
				slotCoverImage.Height = 23;
				slotCoverImage.Margin = new Thickness(31 + j * 32, 67, 0, 0);
				slotCoverImage.HorizontalAlignment = HorizontalAlignment.Left;
				slotCoverImage.VerticalAlignment = VerticalAlignment.Top;
				slotCoverImage.Source = ResourceDatabase.GetImageFromName("DaycareUnusedSlot");
				slotCoverImage.Visibility = Visibility.Hidden;
				gridDaycare.Children.Insert(gridDaycare.Children.Count - 2, slotCoverImage);
				daycareSlotCoverImages.Add(slotCoverImage);
			}
			for (int j = 0; j < 3; j++) {
				Image slotImage = new Image();
				slotImage.Stretch = Stretch.None;
				slotImage.SnapsToDevicePixels = true;
				slotImage.UseLayoutRounding = true;
				slotImage.Width = 30;
				slotImage.Height = 23;
				slotImage.Margin = new Thickness(31 + j * 32, 67, 0, 0);
				slotImage.HorizontalAlignment = HorizontalAlignment.Left;
				slotImage.VerticalAlignment = VerticalAlignment.Top;
				slotImage.Source = ResourceDatabase.GetImageFromName("PartySlot");
				slotImage.Visibility = Visibility.Hidden;
				gridDaycare.Children.Insert(gridDaycare.Children.Count - 2, slotImage);
				daycareSlotImages.Add(slotImage);
			}

			for (int j = 0; j < 3; j++) {
				Image image = new Image();
				image.Stretch = Stretch.None;
				image.SnapsToDevicePixels = true;
				image.UseLayoutRounding = true;
				image.Width = 32;
				image.Height = 32;
				image.Margin = new Thickness(30 + j * 32, 59, 0, 0);
				image.HorizontalAlignment = HorizontalAlignment.Left;
				image.VerticalAlignment = VerticalAlignment.Top;
				gridDaycare.Children.Insert(gridDaycare.Children.Count - 2, image);

				// Used for shadow pokemon tints
				Rectangle mask = new Rectangle();
				mask.Width = 32;
				mask.Height = 32;
				mask.Margin = new Thickness(30 + j * 32, 59, 0, 0);
				mask.HorizontalAlignment = HorizontalAlignment.Left;
				mask.VerticalAlignment = VerticalAlignment.Top;
				mask.StrokeThickness = 0;
				mask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));
				mask.Visibility = Visibility.Hidden;
				gridDaycare.Children.Insert(gridDaycare.Children.Count - 2, mask);

				daycareImages.Add(image);
				daycareShadowMasks.Add(mask);
			}
			for (int j = 0; j < 3; j++) {
				Rectangle clickArea = new Rectangle();
				clickArea.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				clickArea.Stroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
				clickArea.Width = 32;
				clickArea.Height = 25;
				clickArea.Opacity = 0;
				clickArea.Tag = new PokeBoxTagStructure(this, j);
				clickArea.Margin = new Thickness(30 + j * 32, 66, 0, 0);
				clickArea.HorizontalAlignment = HorizontalAlignment.Left;
				clickArea.VerticalAlignment = VerticalAlignment.Top;
				clickArea.PreviewMouseDown += OnBoxSlotClicked;
				clickArea.MouseEnter += OnBoxSlotEnter;
				clickArea.MouseLeave += OnBoxSlotLeave;
				clickArea.ContextMenu = contextMenu;
				clickArea.ContextMenuOpening += OnPokemonContextMenuOpening;
				daycareClickAreas.Add(clickArea);
				gridDaycare.Children.Insert(gridDaycare.Children.Count - 2, clickArea);
			}
		}
		private void CreatePurifierElements() {
			this.purifierImages = new List<Image>();
			this.purifierShadowMasks = new List<Rectangle>();
			this.purifierSlotImages = new List<Image>();
			this.purifierClickAreas = new List<Rectangle>();

			for (int i = 0; i < 5; i++) {
				Image slotImage = new Image();
				slotImage.Stretch = Stretch.None;
				slotImage.SnapsToDevicePixels = true;
				slotImage.UseLayoutRounding = true;
				slotImage.Width = 30;
				slotImage.Height = 23;
				slotImage.Margin = new Thickness(31 + PurifierSlotOffsets[i].X * 32, 38 + PurifierSlotOffsets[i].Y * 25, 0, 0);
				slotImage.HorizontalAlignment = HorizontalAlignment.Left;
				slotImage.VerticalAlignment = VerticalAlignment.Top;
				slotImage.Source = ResourceDatabase.GetImageFromName("PartySlot");
				slotImage.Visibility = Visibility.Hidden;
				gridPurifier.Children.Insert(gridPurifier.Children.Count - 2, slotImage);
				purifierSlotImages.Add(slotImage);
			}

			for (int i = 0; i < 5; i++) {
				Image image = new Image();
				image.Stretch = Stretch.None;
				image.SnapsToDevicePixels = true;
				image.UseLayoutRounding = true;
				image.Width = 32;
				image.Height = 32;
				image.Margin = new Thickness(30 + PurifierSlotOffsets[i].X * 32, 30 + PurifierSlotOffsets[i].Y * 25, 0, 0);
				image.HorizontalAlignment = HorizontalAlignment.Left;
				image.VerticalAlignment = VerticalAlignment.Top;
				gridPurifier.Children.Insert(gridPurifier.Children.Count - 2, image);

				// Used for shadow pokemon tints
				Rectangle mask = new Rectangle();
				mask.Width = 32;
				mask.Height = 32;
				mask.Margin = new Thickness(30 + PurifierSlotOffsets[i].X * 32, 30 + PurifierSlotOffsets[i].Y * 25, 0, 0);
				mask.HorizontalAlignment = HorizontalAlignment.Left;
				mask.VerticalAlignment = VerticalAlignment.Top;
				mask.StrokeThickness = 0;
				mask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));
				mask.Visibility = Visibility.Hidden;
				gridPurifier.Children.Insert(gridPurifier.Children.Count - 2, mask);

				purifierImages.Add(image);
				purifierShadowMasks.Add(mask);
			}
			for (int i = 0; i < 5; i++) {
				Rectangle clickArea = new Rectangle();
				clickArea.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				clickArea.Stroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
				clickArea.Width = 32;
				clickArea.Height = 25;
				clickArea.Opacity = 0;
				clickArea.Tag = new PokeBoxTagStructure(this, i);
				clickArea.Margin = new Thickness(30 + PurifierSlotOffsets[i].X * 32, 37 + PurifierSlotOffsets[i].Y * 25, 0, 0);
				clickArea.HorizontalAlignment = HorizontalAlignment.Left;
				clickArea.VerticalAlignment = VerticalAlignment.Top;
				clickArea.PreviewMouseDown += OnBoxSlotClicked;
				clickArea.MouseEnter += OnBoxSlotEnter;
				clickArea.MouseLeave += OnBoxSlotLeave;
				clickArea.ContextMenu = contextMenu;
				clickArea.ContextMenuOpening += OnPokemonContextMenuOpening;
				purifierClickAreas.Add(clickArea);
				gridPurifier.Children.Insert(gridPurifier.Children.Count - 2, clickArea);
			}
		}
		private void CreateBoxElements() {
			this.boxImages = new List<Image>();
			this.boxShadowMasks = new List<Rectangle>();
			this.boxSelectMasks = new List<Rectangle>();
			this.boxClickAreas = new List<Rectangle>();
			this.boxEggMarkers = new List<Image>();

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
					image.IsHitTestVisible = false;
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 2, image);

					// Used for shadow pokemon tints
					Rectangle shadowMask = new Rectangle();
					shadowMask.Width = 32;
					shadowMask.Height = 32;
					shadowMask.Margin = new Thickness(j * 24, i * 24, 0, 0);
					shadowMask.HorizontalAlignment = HorizontalAlignment.Left;
					shadowMask.VerticalAlignment = VerticalAlignment.Top;
					shadowMask.StrokeThickness = 0;
					shadowMask.Fill = new SolidColorBrush(Color.FromArgb(70, 128, 112, 184));
					shadowMask.Visibility = Visibility.Hidden;
					shadowMask.IsHitTestVisible = false;
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 2, shadowMask);

					Image egg = new Image();
					egg.Stretch = Stretch.None;
					egg.SnapsToDevicePixels = true;
					egg.UseLayoutRounding = true;
					egg.Width = 9;
					egg.Height = 11;
					egg.Margin = new Thickness(j * 24 + 20, i * 24 + 21, 0, 0);
					egg.HorizontalAlignment = HorizontalAlignment.Left;
					egg.VerticalAlignment = VerticalAlignment.Top;
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 2, egg);

					// Used for multi select highlighting
					Rectangle selectMask = new Rectangle();
					selectMask.Width = 32;
					selectMask.Height = 32;
					selectMask.Margin = new Thickness(j * 24, i * 24, 0, 0);
					selectMask.HorizontalAlignment = HorizontalAlignment.Left;
					selectMask.VerticalAlignment = VerticalAlignment.Top;
					selectMask.StrokeThickness = 0;
					selectMask.Fill = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
					selectMask.Visibility = Visibility.Hidden;
					selectMask.IsHitTestVisible = false;
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 2, selectMask);

					boxImages.Add(image);
					boxShadowMasks.Add(shadowMask);
					boxEggMarkers.Add(egg);
					boxSelectMasks.Add(selectMask);
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
					clickArea.Tag = new PokeBoxTagStructure(this, i * 6 + j);
					clickArea.Margin = new Thickness(4 + j * 24, 8 + i * 24, 0, 0);
					clickArea.HorizontalAlignment = HorizontalAlignment.Left;
					clickArea.VerticalAlignment = VerticalAlignment.Top;
					clickArea.PreviewMouseDown += OnBoxSlotClicked;
					clickArea.MouseEnter += OnBoxSlotEnter;
					clickArea.MouseLeave += OnBoxSlotLeave;
					clickArea.ContextMenu = contextMenu;
					clickArea.ContextMenuOpening += OnPokemonContextMenuOpening;
					boxClickAreas.Add(clickArea);
					gridBoxPokemon.Children.Insert(gridBoxPokemon.Children.Count - 2, clickArea);
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
			//MenuItem give = new MenuItem();
			//give.Header = "Give";
			//give.Click += OnContextMenuGiveClicked;
			MenuItem sendTo = new MenuItem();
			sendTo.Header = "Send To";
			sendTo.Click += OnContextMenuSendToClicked;
			Separator separator = new Separator();
			MenuItem release = new MenuItem();
			release.Header = "Release";
			release.Click += OnContextMenuReleaseClicked;

			contextMenu.Items.Add(move);
			contextMenu.Items.Add(summary);
			//contextMenu.Items.Add(give);
			contextMenu.Items.Add(sendTo);
			contextMenu.Items.Add(separator);
			contextMenu.Items.Add(release);
		}
		private void CreateBoxContextMenu() {
			boxContextMenu = new ContextMenu();

			MenuItem edit = new MenuItem();
			edit.Header = "Edit Box";
			edit.Click += OnContextMenuEditBoxClicked;
			MenuItem selectAll = new MenuItem();
			selectAll.Header = "Select All";
			selectAll.Click += OnContextMenuSelectAllClicked;
			//MenuItem sendBoxTo = new MenuItem();
			//sendBoxTo.Header = "Send Box To ";
			//sendBoxTo.Click += OnContextMenuSendBoxTo;
			Separator separator = new Separator();
			MenuItem releaseAll = new MenuItem();
			releaseAll.Header = "Release All";
			releaseAll.Click += OnContextMenuReleaseAllClicked;

			boxContextMenu.Items.Add(edit);
			boxContextMenu.Items.Add(selectAll);
			//boxContextMenu.Items.Add(sendBoxTo);
			boxContextMenu.Items.Add(separator);
			boxContextMenu.Items.Add(releaseAll);
		}

		#endregion
	}
}
