using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
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
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class ChangeDeoxysFormWindow : Window {

		private byte form;
		private IPokemon deoxys;

		public ChangeDeoxysFormWindow(IPokemon deoxys) {
			InitializeComponent();
			this.form = deoxys.DeoxysForm;
			this.deoxys = deoxys;

			AddDeoxysItem(byte.MaxValue, "Game Specific");
			AddDeoxysItem(0, "Normal Form");
			AddDeoxysItem(1, "Attack Form");
			AddDeoxysItem(2, "Defense Form");
			AddDeoxysItem(3, "Speed Form");

			comboBoxForm.SelectedIndex = (form == byte.MaxValue ? 0 : form + 1);
		}

		private void AddDeoxysItem(byte form, string name) {
			string[] formNames = new string[] { "Default", "Normal", "Attack", "Defense", "Speed" };
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation =  Orientation.Horizontal;
			Image image = new Image();
			//(form == byte.MaxValue ? 0 : form)
			image.Source = ResourceDatabase.GetImageFromName("Deoxys" + formNames[form == byte.MaxValue ? 0 : form + 1]);
			image.Stretch = Stretch.None;
			image.SnapsToDevicePixels = true;
			image.UseLayoutRounding = true;
			TextBlock text = new TextBlock();
			text.Margin = new Thickness(6, 0, 0, 0);
			text.Text = name;
			text.VerticalAlignment = VerticalAlignment.Center;
			stackPanel.Children.Add(image);
			stackPanel.Children.Add(text);
			comboBoxForm.Items.Add(stackPanel);
		}

		public static bool? ShowDialog(Window owner, IPokemon deoxys) {
			ChangeDeoxysFormWindow window = new ChangeDeoxysFormWindow(deoxys);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void GameChanged(object sender, SelectionChangedEventArgs e) {
			byte newForm = (byte)((ComboBox)sender).SelectedIndex;
			form = (newForm == 0 ? byte.MaxValue : (byte)(newForm - 1));
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			deoxys.DeoxysForm = form;
			PokeManager.RefreshUI();
			DialogResult = true;
		}
	}
}
