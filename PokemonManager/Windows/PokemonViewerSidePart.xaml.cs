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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for PokemonViewerSidePart.xaml
	/// </summary>
	public partial class PokemonViewerSidePart : UserControl {

		private IPokemon pokemon;

		public PokemonViewerSidePart() {
			InitializeComponent();
		}

		public void RefreshUI() {
			if (pokemon != null)
				LoadPokemon(pokemon);
		}
		public void UnloadPokemon() {
			this.pokemon = null;
			this.imagePokemon.Source = null;
			this.rectShadowMask.Visibility = Visibility.Hidden;
			this.imageShadowAura.Visibility = Visibility.Hidden;
			this.imageShinyStar.Visibility = Visibility.Hidden;
			this.imageBallCaught.Source = null;

			this.labelNickname.Content = "";
			this.labelSpecies.Content = "";
			this.labelLevel.Content = "";
			this.labelGender.Content = "";

			Brush unmarkedBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
			markCircle.Foreground = unmarkedBrush;
			markSquare.Foreground = unmarkedBrush;
			markTriangle.Foreground = unmarkedBrush;
			markHeart.Foreground = unmarkedBrush;

			imageHeldItem.Source = null;
		}

		public void LoadPokemon(IPokemon pokemon) {
			this.pokemon = pokemon;
			this.imagePokemon.Source = pokemon.Sprite;
			if (pokemon.IsShadowPokemon) {
				this.rectShadowMask.OpacityMask = new ImageBrush(this.imagePokemon.Source);
				this.rectShadowMask.Visibility = Visibility.Visible;
				this.imageShadowAura.Visibility = Visibility.Visible;
			}
			else {
				this.rectShadowMask.Visibility = Visibility.Hidden;
				this.imageShadowAura.Visibility = Visibility.Hidden;
			}
			this.imageShinyStar.Visibility = (pokemon.IsShiny && (!pokemon.IsEgg || !PokeManager.Settings.MysteryEggs) ? Visibility.Visible : Visibility.Hidden);
			this.imageBallCaught.Source = (!pokemon.IsEgg ? PokemonDatabase.GetBallCaughtImageFromID(pokemon.BallCaughtID) : null);

			if (pokemon.IsEgg)
				this.labelNickname.Content = "EGG";
			else
				this.labelNickname.Content = pokemon.Nickname;
			if (pokemon.HasForm)
				this.labelSpecies.Content = pokemon.PokemonFormData.Name;
			else
				this.labelSpecies.Content = pokemon.PokemonData.Name;
			if (pokemon.DexID == 265 && (!pokemon.IsEgg || !PokeManager.Settings.MysteryEggs))
				this.labelSpecies.Content += " " + (pokemon.WurpleIsCascoon ? "(Cas)" : "(Sil)");

			this.labelLevel.Content = "Lv " + pokemon.Level.ToString();
			if (pokemon.IsEgg && PokeManager.Settings.MysteryEggs) {
				this.labelGender.Content = "";
			}
			else if (pokemon.Gender == Genders.Male) {
				this.labelGender.Content = "♂";
				this.labelGender.Foreground = new SolidColorBrush(Color.FromRgb(0, 136, 184));
			}
			else if (pokemon.Gender == Genders.Female) {
				this.labelGender.Content = "♀";
				this.labelGender.Foreground = new SolidColorBrush(Color.FromRgb(184, 88, 80));
			}
			else {
				this.labelGender.Content = "";
			}

			Brush unmarkedBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
			Brush markedBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			markCircle.Foreground = (pokemon.IsCircleMarked ? markedBrush : unmarkedBrush);
			markSquare.Foreground = (pokemon.IsSquareMarked ? markedBrush : unmarkedBrush);
			markTriangle.Foreground = (pokemon.IsTriangleMarked ? markedBrush : unmarkedBrush);
			markHeart.Foreground = (pokemon.IsHeartMarked ? markedBrush : unmarkedBrush);

			if (pokemon.IsHoldingItem)
				imageHeldItem.Source = ItemDatabase.GetItemImageFromID(pokemon.HeldItemID);
			else
				imageHeldItem.Source = null;
		}
	}
}
