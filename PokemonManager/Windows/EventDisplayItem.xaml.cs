using PokemonManager.Game.FileStructure;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for EventDisplayItem.xaml
	/// </summary>
	public partial class EventDisplayItem : UserControl {

		private EventDistribution eventDist;
		private IGameSave gameSave;

		public EventDisplayItem() {
			InitializeComponent();
		}

		public EventDisplayItem(EventDistribution eventDist, IGameSave gameSave) {
			InitializeComponent();

			this.eventDist = eventDist;
			this.gameSave = gameSave;

			this.labeTitle.Content = eventDist.GetTitle(gameSave) + (PokeManager.IsEventCompletedBy(eventDist.ID, gameSave) ? " (Completed)" : "");
			BitmapSource image = eventDist.SmallSprite;
			this.imageSprite.Width = image.PixelWidth;
			this.imageSprite.Height = image.PixelHeight;
			this.imageSprite.Source = image;
		}

		public void UpdateDisplay() {
			this.labeTitle.Content = eventDist.GetTitle(gameSave) + (PokeManager.IsEventCompletedBy(eventDist.ID, gameSave) ? " (Completed)" : "");
		}

		public EventDistribution Event {
			get { return eventDist; }
		}
	}
}
