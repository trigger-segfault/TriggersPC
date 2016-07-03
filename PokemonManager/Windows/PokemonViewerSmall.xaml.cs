using PokemonManager.Game;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Windows.Controls;
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
	/// Interaction logic for PokemonViewerSmall.xaml
	/// </summary>
	public partial class PokemonViewerSmall : UserControl, IPokemonViewer {

		private IPokemon pokemon;

		public PokemonViewerSmall() {
			InitializeComponent();
			UnloadPokemon();
		}

		public IPokemon ViewedPokemon {
			get { return pokemon; }
		}

		public void RefreshUI() {
			if (pokemon != null)
				LoadPokemon(pokemon);
		}
		public void LoadPokemon(IPokemon pokemon) {
			if (pokemon == null) {
				UnloadPokemon();
				return;
			}

			this.pokemon = pokemon;
			if (PokeManager.IsAprilFoolsMode && !pokemon.IsEgg)
				this.imagePokemon.Source = PokemonDatabase.GetPokemonImageFromDexID(41, pokemon.IsShiny);
			else
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

			if (pokemon.IsEgg)
				this.labelNickname.Content = "EGG";
			else
				this.labelNickname.Content = pokemon.Nickname;
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

		public void UnloadPokemon() {
			imagePokemon.Source = null;
			rectShadowMask.Visibility = Visibility.Hidden;
			imageShadowAura.Visibility = Visibility.Hidden;
			labelNickname.Content = "";
			labelLevel.Content = "";
			labelGender.Content = "";
			Brush unmarkedBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
			markCircle.Foreground = unmarkedBrush;
			markSquare.Foreground = unmarkedBrush;
			markTriangle.Foreground = unmarkedBrush;
			markHeart.Foreground = unmarkedBrush;

			imageHeldItem.Source = null;
		}
	}
}
