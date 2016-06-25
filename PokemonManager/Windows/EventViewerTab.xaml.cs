using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.PokemonStructures.Events;
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

	public partial class EventViewerTab : UserControl {

		private IGameSave gameSave;
		private EventDistribution selectedEvent;
		private int selectedIndex;

		public EventViewerTab() {
			InitializeComponent();

			this.labelTitle.Content = "";
			this.textBlockDescription.Text = "";
			this.textBlockRequirements.Text = "";
			this.buttonActivate.IsEnabled = false;
		}

		public void LoadEvents(IGameSave gameSave) {
			this.gameSave = gameSave;
			this.selectedIndex = -1;
			this.labelTitle.Content = "";
			this.textBlockDescription.Text = "";
			this.textBlockRequirements.Text = "";
			this.imageSprite.Source = null;
			this.buttonActivate.IsEnabled = false;
			UpdateDetails();
			PopulateEvents();
		}
		public void PopulateEvents() {
			this.listViewItems.Items.Clear();
			for (int i = 0; i < PokeManager.NumEvents; i++) {
				EventDistribution eventDist = PokeManager.GetEventAt(i);
				if (eventDist.AllowedGames.HasFlag((GameTypeFlags)(1 << ((int)gameSave.GameType - 1)))) {
					listViewItems.Items.Add(new EventDisplayItem(eventDist, gameSave));

					/*ListViewItem listViewItem = new ListViewItem();

					StackPanel stackPanel = new StackPanel();
					stackPanel.Orientation = Orientation.Horizontal;

					int width = 32;

					Image image = new Image();
					image.Source = eventDist.SmallSprite;
					if (eventDist.SmallSprite.PixelHeight == 32) {
						image.Width = width;
						image.Height = 32;
						image.Margin = new Thickness(-2, -7, -2, -2);
						image.HorizontalAlignment = HorizontalAlignment.Left;
						image.VerticalAlignment = VerticalAlignment.Top;
					}
					else {
						image.Width = eventDist.SmallSprite.PixelWidth;
						image.Height = eventDist.SmallSprite.PixelHeight;
						image.Margin = new Thickness((28 - image.Width) / 2, (28 - image.Height) / 2, (28 - image.Width) / 2, (28 - image.Height) / 2);
						image.HorizontalAlignment = HorizontalAlignment.Left;
						image.VerticalAlignment = VerticalAlignment.Center;
					}

					Label name = new Label();
					name.VerticalAlignment = VerticalAlignment.Center;
					name.Content = eventDist.Title;
					name.Margin = new Thickness(2, 0, 0, 0);

					Label completed = new Label();
					completed.VerticalAlignment = VerticalAlignment.Center;
					completed.FontWeight = FontWeights.Bold;
					completed.Content = "(Completed)";
					completed.Margin = new Thickness(0, 0, 0, 0);
					completed.Padding = new Thickness(0, 0, 0, 0);
					completed.Visibility = (PokeManager.IsEventCompletedBy(eventDist.ID, gameSave) ? Visibility.Visible : Visibility.Hidden);

					stackPanel.Children.Add(image);
					stackPanel.Children.Add(name);
					stackPanel.Children.Add(completed);

					listViewItem.Content = stackPanel;
					listViewItem.Tag = eventDist;
					listViewItems.Items.Add(listViewItem);*/
				}
			}
		}

		public void UpdateEvents() {
			foreach (object obj in listViewItems.Items) {
				(obj as EventDisplayItem).UpdateDisplay();
			}
		}

		public void UnloadEvents() {

			this.listViewItems.Items.Clear();
			this.labelTitle.Content = "";
			this.textBlockDescription.Text = "";
			this.textBlockRequirements.Text = "";
			this.imageSprite.Source = null;
			this.buttonActivate.IsEnabled = false;
		}

		private void OnItemListSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int index = listViewItems.SelectedIndex;
			if (index < listViewItems.Items.Count) {
				if (index != -1)
					selectedIndex = index;
				if (selectedIndex != -1 && selectedIndex < listViewItems.Items.Count) {
					selectedEvent = (listViewItems.Items[selectedIndex] as EventDisplayItem).Event;
					this.labelTitle.Content = selectedEvent.GetTitle(gameSave) + (PokeManager.IsEventCompletedBy(selectedEvent.ID, gameSave) ? " (Completed)" : "");
					this.textBlockDescription.Text = selectedEvent.GetDescription(gameSave);
					this.textBlockRequirements.Text = selectedEvent.GetRequirements(gameSave);
					this.imageSprite.Width = selectedEvent.BigSprite.PixelWidth;
					this.imageSprite.Height = selectedEvent.BigSprite.PixelHeight;
					this.imageSprite.Source = selectedEvent.BigSprite;
					this.buttonActivate.IsEnabled = !PokeManager.IsEventCompletedBy(selectedEvent.ID, gameSave) && selectedEvent.IsRequirementsFulfilled(gameSave);
				}
				else {
					selectedEvent = null;
					this.labelTitle.Content = "";
					this.textBlockDescription.Text = "";
					this.textBlockRequirements.Text = "";
					this.imageSprite.Source = null;
					this.buttonActivate.IsEnabled = false;
				}
			}
			else {
				selectedEvent = null;
				selectedIndex = -1;
				this.labelTitle.Content = "";
				this.textBlockDescription.Text = "";
				this.textBlockRequirements.Text = "";
				this.imageSprite.Source = null;
				this.buttonActivate.IsEnabled = false;
			}
		}


		private void UpdateDetails() {
			int count = 0;
			int completed = 0;
			for (int i = 0; i < PokeManager.NumEvents; i++) {
				EventDistribution eventDist = PokeManager.GetEventAt(i);
				if (eventDist.AllowedGames.HasFlag((GameTypeFlags)(1 << ((int)gameSave.GameType - 1)))) {
					count++;
					if (PokeManager.IsEventCompletedBy(eventDist.ID, gameSave))
						completed++;
				}
			}
			labelPocket.Content = "Events Completed   " + completed + "/" + count;
		}

		private void OnActivateClicked(object sender, RoutedEventArgs e) {
			if (selectedEvent.HasRoomForReward(gameSave)) {
				this.buttonActivate.IsEnabled = false;
				selectedEvent.GenerateReward(gameSave);
				selectedEvent.GiveReward(gameSave);
				PokeManager.CompleteEventBy(selectedEvent.ID, gameSave);
				UpdateEvents();
			}
			else {
				TriggerMessageBox.Show(Window.GetWindow(this),
					"There's no room for this event, make room in your " +
					(selectedEvent.RewardType == EventRewardTypes.Pokemon ? "PC" : "Bag") + " for the reward", "No Room"
				);
			}
		}
	}
}
