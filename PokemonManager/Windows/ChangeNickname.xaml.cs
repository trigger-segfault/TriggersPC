using PokemonManager.Game;
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

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for ChangeNickname.xaml
	/// </summary>
	public partial class ChangeNicknameWindow : Window {

		private IPokemon pokemon;
		private string nickname;

		public ChangeNicknameWindow(IPokemon pokemon) {
			InitializeComponent();
			this.pokemon = pokemon;
			this.nickname = pokemon.Nickname;
			this.buttonQuotes1.Content = "“";
			this.buttonQuotes2.Content = "”";
			this.textBoxName.Text = nickname;

			if (pokemon.Language == Languages.Japanese) {
				this.textBoxName.MaxLength = 5;
				this.buttonDots.Visibility = Visibility.Hidden;
				this.buttonQuotes1.Visibility = Visibility.Hidden;
				this.buttonQuotes2.Visibility = Visibility.Hidden;
				this.buttonSingleQuote1.Visibility = Visibility.Hidden;
				this.buttonSingleQuote2.Visibility = Visibility.Hidden;
			}

			this.textBoxName.Focus();
			this.textBoxName.SelectAll();
		}


		private void OKClicked(object sender, RoutedEventArgs e) {
			DialogResult = true;
			nickname = textBoxName.Text;
			pokemon.Nickname = nickname;
			Close();
		}

		private void OnNicknameChanged(object sender, TextChangedEventArgs e) {
			nickname = textBoxName.Text;
		}

		private void OnMaleButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("♂");
		}
		private void OnFemaleButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("♀");
		}
		private void OnDotDotDotButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("…");
		}
		private void OnOpenQuotesButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("“");
		}
		private void OnCloseQuotesButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("”");
		}
		private void OnOpenSingleQuoteButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("‘");
		}
		private void OnCloseSingleQuoteButtonClicked(object sender, RoutedEventArgs e) {
			WriteCharacter("’");
		}

		private void WriteCharacter(string character) {
			if (textBoxName.Text.Length < 11) {
				int caretIndex = textBoxName.CaretIndex;
				textBoxName.Text = textBoxName.Text.Insert(textBoxName.CaretIndex, character);
				textBoxName.CaretIndex = caretIndex + 1;
				textBoxName.Focus();
			}
		}

		public static bool? ShowDialog(Window owner, IPokemon pokemon) {
			ChangeNicknameWindow window = new ChangeNicknameWindow(pokemon);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OnEnterPressed(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				DialogResult = true;
				nickname = textBoxName.Text;
				pokemon.Nickname = nickname;
				Close();
			}
		}
	}
}
