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
	/// Interaction logic for PokemonTypeControl.xaml
	/// </summary>
	public partial class PokemonTypeControl : UserControl {

		private PokemonTypes type;

		public PokemonTypeControl() {
			InitializeComponent();

			this.Width = 58;
			this.Height = 19;
		}

		public PokemonTypes Type {
			get { return type; }
			set {
				type = value;

				if (type == PokemonTypes.None) {
					labelTypeName.Content = "???";
				}
				else {
					labelTypeName.Content = type.ToString();//.ToUpper();
				}
				double angle = 90;
				switch (type) {
				case PokemonTypes.Normal:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(168, 168, 120), Color.FromRgb(138, 138, 89), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(121, 121, 78));
					break;
				case PokemonTypes.Fighting:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(192, 48, 40), Color.FromRgb(157, 39, 33), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(130, 33, 27));
					break;
				case PokemonTypes.Flying:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(168, 144, 240), Color.FromRgb(145, 128, 196), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(119, 98, 182));
					break;
				case PokemonTypes.Poison:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(160, 64, 160), Color.FromRgb(128, 51, 128), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(102, 41, 102));
					break;
				case PokemonTypes.Ground:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(224, 192, 104), Color.FromRgb(212, 168, 47), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(170, 134, 35));
					break;
				case PokemonTypes.Rock:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(184, 160, 56), Color.FromRgb(147, 128, 45), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(116, 101, 35));
					break;
				case PokemonTypes.Bug:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(168, 184, 32), Color.FromRgb(141, 154, 27), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(97, 107, 19));
					break;
				case PokemonTypes.None:
				case PokemonTypes.Shadow:
				case PokemonTypes.Ghost:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(112, 88, 152), Color.FromRgb(85, 67, 116), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(65, 51, 89));
					break;
				case PokemonTypes.Steel:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(184, 184, 208), Color.FromRgb(151, 151, 186), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(122, 122, 167));
					break;
				case PokemonTypes.Fire:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(240, 128, 48), Color.FromRgb(221, 102, 16), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(180, 83, 13));
					break;
				case PokemonTypes.Water:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(104, 144, 240), Color.FromRgb(56, 108, 235), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(23, 83, 227));
					break;
				case PokemonTypes.Grass:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(120, 200, 80), Color.FromRgb(92, 169, 53), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(74, 137, 43));
					break;
				case PokemonTypes.Electric:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(248, 208, 48), Color.FromRgb(240, 193, 8), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(193, 155, 7));
					break;
				case PokemonTypes.Psychic:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(248, 88, 136), Color.FromRgb(246, 28, 93), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(214, 9, 69));
					break;
				case PokemonTypes.Ice:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(152, 216, 216), Color.FromRgb(105, 198, 198), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(69, 182, 182));
					break;
				case PokemonTypes.Dragon:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(112, 56, 248), Color.FromRgb(76, 8, 239), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(61, 7, 192));
					break;
				case PokemonTypes.Dark:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(112, 88, 72), Color.FromRgb(81, 63, 52), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(54, 42, 35));
					break;
				}
			}
		}
	}
}
