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
	public partial class ConditionTypeControl : UserControl {

		private ConditionTypes type;

		public ConditionTypeControl() {
			InitializeComponent();

			this.Width = 58;
			this.Height = 19;
		}

		public ConditionTypes Type {
			get { return type; }
			set {
				type = value;

				if (type == ConditionTypes.None) {
					labelTypeName.Content = "???";
				}
				else {
					labelTypeName.Content = type.ToString();//.ToUpper();
				}
				double angle = 90;
				switch (type) {
				case ConditionTypes.None:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(112, 88, 152), Color.FromRgb(85, 67, 116), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(65, 51, 89));
					break;
				case ConditionTypes.Cool:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(240, 128, 48), Color.FromRgb(221, 102, 16), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(180, 83, 13));
					break;
				case ConditionTypes.Beauty:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(104, 144, 240), Color.FromRgb(56, 108, 235), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(23, 83, 227));
					break;
				case ConditionTypes.Smart:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(120, 200, 80), Color.FromRgb(92, 169, 53), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(74, 137, 43));
					break;
				case ConditionTypes.Tough:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(248, 208, 48), Color.FromRgb(240, 193, 8), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(193, 155, 7));
					break;
				case ConditionTypes.Cute:
					rectTypeColor.Fill = new LinearGradientBrush(Color.FromRgb(249, 140, 255), Color.FromRgb(245, 64, 255), angle);
					rectTypeColor.Stroke = new SolidColorBrush(Color.FromRgb(193, 7, 155));
					break;
				}
			}
		}
	}
}
