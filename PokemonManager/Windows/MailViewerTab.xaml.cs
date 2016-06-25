using PokemonManager.Game;
using PokemonManager.Items;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.JoshSmith.ServiceProviders.UI;

namespace PokemonManager.Windows {

	public class MailPositionInfo {
		public bool Black { get; set; }
		public int MessageX { get; set; }
		public int MessageY { get; set; }
		public int FromX { get; set; }
		public int FromY { get; set; }
		public int FromLength { get; set; }
		public int PokemonX { get; set; }
		public int PokemonY { get; set; }

		public MailPositionInfo(bool black, int mX, int mY, int fX, int fY, int fL, int pX = -1, int pY = -1) {
			this.Black		= black;
			this.MessageX	= mX;
			this.MessageY	= mY;
			this.FromX		= fX;
			this.FromY		= fY;
			this.FromLength	= fL;
			this.PokemonX	= pX;
			this.PokemonY	= pY;
		}
	}

	public partial class MailViewerTab : UserControl {

		private static Dictionary<ushort, MailPositionInfo> mailPositions = new Dictionary<ushort, MailPositionInfo>() {
			{121, new MailPositionInfo(true, 38, 30, 104, 114, 104)},
			{122, new MailPositionInfo(false, 38, 28, 109, 117, 102)},
			{123, new MailPositionInfo(true, 38, 24, 119, 119, 90)},
			{124, new MailPositionInfo(false, 38, 27, 109, 112, 102)},
			{125, new MailPositionInfo(false, 38, 28, 112, 117, 96)},
			{126, new MailPositionInfo(true, 38, 24, 119, 116, 98)},
			{127, new MailPositionInfo(false, 38, 28, 116, 119, 96, 72, 104)},
			{128, new MailPositionInfo(false, 38, 32, 113, 120, 102)},
			{129, new MailPositionInfo(true, 38, 28, 107, 116, 106)},
			{130, new MailPositionInfo(true, 38, 28, 111, 116, 98, 16, 104)},
			{131, new MailPositionInfo(true, 38, 34, 112, 124, 104)},
			{132, new MailPositionInfo(true, 38, 24, 109, 116, 102)}
		};


		private Mailbox mailbox;
		private Mail selectedMail;
		private int selectedIndex;

		private ListViewDragDropManager<ListViewItem> dropManager;

		public MailViewerTab() {
			InitializeComponent();

			this.imageMailBackground.Source = null;
			this.labelFrom.Content = "";
			for (int i = 0; i < 5; i++)
				(this.stackPanelMessageLines.Children[i] as Label).Content = "";

			this.imageItem.Source = null;
			this.imagePokemon.Source = null;

			this.labelItemName.Content = "";
			this.labelPokemon.Content = "";
			this.labelTrainerID.Content = "";
			this.labelSecretID.Content = "";
		}

		public void LoadMailbox(Mailbox mailbox) {
			UnloadMailbox();
			this.mailbox = mailbox;
			this.selectedIndex = -1;
			mailbox.AddListViewItem += OnAddListViewItem;
			mailbox.RemoveListViewItem += OnRemoveListViewItem;
			mailbox.MoveListViewItem += OnMoveListViewItem;
			listViewItems.ItemsSource = mailbox.ListViewItems;
			dropManager = new ListViewDragDropManager<ListViewItem>(listViewItems);
			dropManager.ProcessDrop += OnProcessDrop;
			UpdateDetails();

			mailbox.RepopulateListView();

			int numPartyMail = 0;
			for (int i = 0; i < 6; i++) {
				if (i < mailbox.PartyMailCount && mailbox[i, true] != null)
					numPartyMail++;
			}
			buttonTakeMail.IsEnabled = numPartyMail > 0;
		}
		public void UnloadMailbox() {
			listViewItems.ItemsSource = null;
			if (mailbox != null) {
				dropManager.ListView = null;
				dropManager.ProcessDrop -= OnProcessDrop;
				mailbox.AddListViewItem -= OnAddListViewItem;
				mailbox.RemoveListViewItem -= OnRemoveListViewItem;
				mailbox.MoveListViewItem -= OnMoveListViewItem;
				mailbox = null;
				dropManager = null;
			}
		}

		private void OnItemListSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int index = listViewItems.SelectedIndex;
			if (index < mailbox.MailboxCount) {
				if (index != -1)
					selectedIndex = index;
				if (selectedIndex != -1 && selectedIndex < mailbox.MailboxCount) {
					selectedMail = mailbox[selectedIndex];
					this.imageMailBackground.Source = ResourceDatabase.GetImageFromName(selectedMail.MailItemData.Name.Replace(" ", ""));

					this.imageItem.Source = ItemDatabase.GetItemImageFromID(selectedMail.MailItemID);
					this.imagePokemon.Source = PokemonDatabase.GetPokemonBoxImageFromDexID(selectedMail.OriginalHolderDexID, false);

					this.labelItemName.Content = (selectedMail.MailItemData != null ? selectedMail.MailItemData.Name : "Unknown Mail");
					this.labelPokemon.Content = (selectedMail.OriginalHolderPokemonData != null ? selectedMail.OriginalHolderPokemonData.Name : "Unknown Pokemon");
					this.labelTrainerID.Content = selectedMail.TrainerID.ToString("00000");
					this.labelSecretID.Content = selectedMail.SecretID.ToString("00000");

					MailPositionInfo positions = mailPositions[121];
					if (mailPositions.ContainsKey(selectedMail.MailItemID))
						positions = mailPositions[selectedMail.MailItemID];

					int textOffset = 4;
					Color black = Color.FromRgb(0, 0, 0);
					Color blackShadow = Color.FromRgb(215, 215, 215);
					Color white = Color.FromRgb(255, 255, 255);
					Color whiteShadow = Color.FromRgb(100, 100, 100);

					this.labelFrom.Margin = new Thickness(positions.FromX - 15, positions.FromY - textOffset, 0, 0);
					this.labelFrom.Width = positions.FromLength + 30;
					this.labelFrom.Content = "From " + selectedMail.TrainerName;
					this.labelFrom.Foreground = new SolidColorBrush(positions.Black ? black : white);
					(this.labelFrom.Effect as DropShadowEffect).Color = (positions.Black ? blackShadow : whiteShadow);

					this.stackPanelMessageLines.Margin = new Thickness(positions.MessageX, positions.MessageY - textOffset, 0, 0);
					string[] lines = selectedMail.Lines;
					for (int i = 0; i < 5; i++) {
						(this.stackPanelMessageLines.Children[i] as Label).Content = (i < lines.Length ? lines[i] : "");
						(this.stackPanelMessageLines.Children[i] as Label).Foreground = new SolidColorBrush(positions.Black ? black : white);
						((this.stackPanelMessageLines.Children[i] as Label).Effect as DropShadowEffect).Color = (positions.Black ? blackShadow : whiteShadow);
					}

					this.imagePokemon2.Source = PokemonDatabase.GetPokemonBoxImageFromDexID(selectedMail.OriginalHolderDexID, false);
					this.imagePokemon2.Margin = new Thickness(positions.PokemonX, positions.PokemonY, 0, 0);
					this.imagePokemon2.Visibility = (positions.PokemonX == -1 ? Visibility.Hidden : Visibility.Visible);
				}
				else {
					this.imageMailBackground.Source = null;

					this.imageItem.Source = null;
					this.imagePokemon.Source = null;

					this.labelItemName.Content = "";
					this.labelPokemon.Content = "";
					this.labelTrainerID.Content = "";
					this.labelSecretID.Content = "";


					this.labelFrom.Content = "";
					for (int i = 0; i < 5; i++)
						(this.stackPanelMessageLines.Children[i] as Label).Content = "";
					this.imagePokemon2.Visibility = Visibility.Hidden;
				}
			}
		}

		private void OnMailReturnToInventory(object sender, EventArgs e) {
			MessageBoxResult result = MessageBoxResult.Yes;
			if (PokeManager.Settings.TossConfirmation)
				result = TriggerMessageBox.Show(Window.GetWindow(this), "Are you sure you want to delete this message and return the " + selectedMail.MailItemData.Name + " to your inventory?", "Return Mail", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes) {
				PokeManager.ManagerGameSave.Inventory.Items[ItemTypes.Items].AddItem(selectedMail.MailItemID, 1);
				mailbox.TossMailAt(selectedIndex);
			}
		}

		private void OnMailSendTo(object sender, EventArgs e) {
			int? result = SendMailToWindow.ShowDialog(Window.GetWindow(this), mailbox.GameSave.GameIndex);
			if (result != null) {
				if (PokeManager.GetGameSaveAt(result.Value).Mailbox.HasRoomForMail) {
					PokeManager.GetGameSaveAt(result.Value).Mailbox.AddMail(selectedMail);
					mailbox.TossMailAt(selectedIndex);
				}
				else {
					// No room for item
					TriggerMessageBox.Show(Window.GetWindow(this), "No room for that mail. Mailbox is full", "No Room");
				}
			}
		}

		private void UpdateDetails() {
			labelPocket.Content = "Mailbox   " + mailbox.MailboxCount + "/" + (mailbox.MailboxSize == 0 ? "∞" : mailbox.MailboxSize.ToString());
		}

		private void OnAddListViewItem(object sender, MailboxEventArgs e) {
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.SnapsToDevicePixels = true;
			listViewItem.UseLayoutRounding = true;
			DockPanel dockPanel = new DockPanel();
			dockPanel.Width = 300;

			Image image = new Image();
			image.Source = ItemDatabase.GetItemImageFromID(e.Mail.MailItemID);
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;

			TextBlock fromName = new TextBlock();
			fromName.VerticalAlignment = VerticalAlignment.Center;
			fromName.Text = "From " + e.Mail.TrainerName;
			fromName.TextTrimming = TextTrimming.CharacterEllipsis;
			fromName.Margin = new Thickness(4, 0, 0, 0);

			/*TextBlock blockLv = new TextBlock();
			blockLv.VerticalAlignment	= VerticalAlignment.Center;
			blockLv.HorizontalAlignment = HorizontalAlignment.Right;
			blockLv.TextAlignment = TextAlignment.Right;
			blockLv.Text = "Lv";
			blockLv.Width = Double.NaN;
			blockLv.MinWidth = 10;

			TextBlock blockLevel = new TextBlock();
			blockLevel.VerticalAlignment	= VerticalAlignment.Center;
			blockLevel.HorizontalAlignment = HorizontalAlignment.Right;
			blockLevel.TextAlignment = TextAlignment.Right;
			blockLevel.Width = 30;
			blockLevel.Text = e.Pokeblock.Level.ToString();*/

			listViewItem.Content = dockPanel;
			mailbox.ListViewItems.Insert(e.Index, listViewItem);
			dockPanel.Children.Add(image);
			dockPanel.Children.Add(fromName);
			//dockPanel.Children.Add(blockLevel);
			//dockPanel.Children.Add(blockLv);


			ContextMenu contextMenu = new ContextMenu();

			MenuItem menuPokeblockSendTo = new MenuItem();
			menuPokeblockSendTo.Header = "Send To";
			menuPokeblockSendTo.Click += OnMailSendTo;
			contextMenu.Items.Add(menuPokeblockSendTo);

			MenuItem menuPokeblockToss = new MenuItem();
			menuPokeblockToss.Header = "Return to Inventory";
			menuPokeblockToss.Click += OnMailReturnToInventory;
			contextMenu.Items.Add(menuPokeblockToss);

			listViewItem.ContextMenu = contextMenu;


			//DockPanel.SetDock(image, Dock.Left);
			//DockPanel.SetDock(blockLevel, Dock.Right);

			UpdateDetails();
		}
		private void OnRemoveListViewItem(object sender, MailboxEventArgs e) {
			if (selectedIndex >= mailbox.MailboxCount) {
				if (mailbox.MailboxCount == 0) {
					selectedIndex = -1;
					listViewItems.SelectedIndex = selectedIndex;
				}
				else {
					selectedIndex--;
					listViewItems.SelectedIndex = selectedIndex;
				}
			}
			mailbox.ListViewItems.RemoveAt(e.Index);
			if (selectedIndex > mailbox.MailboxCount) {
				listViewItems.SelectedIndex = selectedIndex;
			}

			UpdateDetails();
		}
		private void OnMoveListViewItem(object sender, MailboxEventArgs e) {
			mailbox.ListViewItems.Move(e.OldIndex, e.NewIndex);
		}
		private void OnProcessDrop(object sender, ProcessDropEventArgs<ListViewItem> e) {
			if (e.OldIndex > -1) {
				mailbox.MoveMail(e.OldIndex, e.NewIndex);
			}

			e.Effects = DragDropEffects.Move;

		}
		private void OnTakeMailFromParty(object sender, RoutedEventArgs e) {
			int numSent = 0;
			for (int i = 0; i < 6; i++) {
				if (i < mailbox.PartyMailCount && mailbox[i, true] != null) {
					if (mailbox.HasRoomForMail) {
						mailbox.AddMail(mailbox[i, true]);
						mailbox[i, true] = null;
					}
					else {
						PokeManager.ManagerGameSave.Mailbox.AddMail(mailbox[i, true]);
						mailbox[i, true] = null;
						numSent++;
					}
				}
			}
			foreach (IPokemon pokemon in mailbox.GameSave.PokePC.Party) {
				if (pokemon.IsHoldingMail)
					pokemon.HeldItemID = 0;
			}
			if (numSent > 0)
				TriggerMessageBox.Show(Window.GetWindow(this), "The mailbox filled up before all the mail could be stored. The rest was sent to " + PokeManager.Settings.ManagerNickname, "Mailbox Full");
			buttonTakeMail.IsEnabled = false;
		}
	}
}
