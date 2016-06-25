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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for ChangeNickname.xaml
	/// </summary>
	public partial class AddSaveNicknameWindow : Window {

		private string nickname;

		public AddSaveNicknameWindow(string nickname) {
			InitializeComponent();
			this.nickname = nickname;
			this.textBoxName.Text = nickname;
			this.textBoxName.Focus();
			textBoxName.SelectAll();
		}


		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
			nickname = textBoxName.Text;
		}

		private void OnNicknameChanged(object sender, TextChangedEventArgs e) {
			nickname = textBoxName.Text;
		}

		public static string ShowDialog(Window owner, string nickname) {
			AddSaveNicknameWindow window = new AddSaveNicknameWindow(nickname);
			window.Owner = owner;
			var result = window.ShowDialog();
			if (result != null && result.Value) {
				return window.nickname;
			}
			return null;
		}

		private void OnEnterPressed(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				DialogResult = true;
				nickname = textBoxName.Text;
				Close();
			}
		}
	}
}
