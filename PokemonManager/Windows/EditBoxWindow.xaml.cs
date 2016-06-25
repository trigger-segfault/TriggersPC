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
	public partial class EditBoxWindow : Window {

		private static readonly string[] RSNames = new string[]{
			"Forest", "City", "Desert", "Savanna",
			"Crag", "Volcano", "Snow", "Cave",
			"Beach", "Seafloor", "River", "Sky",
			"Polkadot", "PokéCenter", "Machine", "Simple"
		};
		private static readonly string[] BoxNames = new string[]{
			"Forest", "City", "Desert", "Savanna",
			"Crag", "Volcano", "Snow", "Cave",
			"Beach", "Seafloor", "River", "Sky",
			"Polkadot", "PokéCenter", "Machine", "Simple",
			"Flower", "Tile", "Carpet", "Ruin"
		};
		private static readonly string[] EmeraldNames = new string[]{
			"Forest", "City", "Desert", "Savanna",
			"Crag", "Volcano", "Snow", "Cave",
			"Beach", "Seafloor", "River", "Sky",
			"Polkadot", "PokéCenter", "Machine", "Simple",
			"Special"
		};
		private static readonly string[] FRLGNames = new string[]{
			"Forest", "City", "Desert", "Savanna",
			"Crag", "Volcano", "Snow", "Cave",
			"Beach", "Seafloor", "River", "Sky",
			"Stars", "PokéCenter", "Tiles", "Simple"
		};
		private static readonly string[] ColosseumNames = new string[]{
			"Forest", "Ocean", "Desert"
		};
		private static readonly string[] XDNames = new string[]{
			"Desert", "Polkadot", "Tiles", "Forest",
			"City", "Cave", "Seafloor", "Volcano"
		};
		private static readonly string[] ManagerNames = new string[]{
			"Forest (RS)", "Forest (E)", "City (RS)", "City (E)",
			"Desert (RS)", "Desert (E)", "Savanna (RS)", "Savanna (E)",
			"Crag (RS)", "Crag (E)", "Volcano (RS)", "Volcano (E)",
			"Snow (RS)", "Snow (E)", "Cave (RS)", "Cave (E)",
			"Beach (RS)", "Beach (E)", "Seafloor (RS)", "Seafloor (E)",
			"River (RS)", "River (E)", "Sky (RS)", "Sky (E)",
			"Polkadot (RS)", "Polkadot (E)", "Stars (FRLG)",
			"PokéCenter (RS)", "PokéCenter (E)", "PokéCenter (FRLG)",
			"Machine (RS)", "Machine (E)", "Tiles (FRLG)",
			"Simple (RS)", "Simple (E)", "Simple (FRLG)",
			"Ocean (RS)", "Ocean (E)",
			"Flower (Box)", "Tile (Box)", "Carpet (Box)", "Ruin (Box)"
		};

		private IPokeBox pokeBox;
		private string newName;
		private byte newWallpaper;

		private string newWallpaperName;
		private bool newUsingCustomWallpaper;
		private int customWallpaperStartIndex;

		public EditBoxWindow(IPokeBox pokeBox) {
			InitializeComponent();
			if (pokeBox is BoxPokeBox) {
				this.newName = ((BoxPokeBox)pokeBox).RealName;
				this.pokeBox = ((BoxPokeBox)pokeBox).PrimaryBox;
			}
			else {
				this.newName = pokeBox.Name;
				this.pokeBox = pokeBox;
			}
			this.buttonQuotes1.Content = "“";
			this.buttonQuotes2.Content = "”";

			string[] wallpaperNames = null;
			if (pokeBox.GameType == GameTypes.Ruby || pokeBox.GameType == GameTypes.Sapphire)
				wallpaperNames = RSNames;
			else if (pokeBox.GameType == GameTypes.Emerald)
				wallpaperNames = EmeraldNames;
			else if (pokeBox.GameType == GameTypes.FireRed || pokeBox.GameType == GameTypes.LeafGreen)
				wallpaperNames = FRLGNames;
			else if (pokeBox.GameType == GameTypes.PokemonBox)
				wallpaperNames = BoxNames;
			else if (pokeBox.GameType == GameTypes.Colosseum)
				wallpaperNames = ColosseumNames;
			else if (pokeBox.GameType == GameTypes.XD)
				wallpaperNames = XDNames;
			else if (pokeBox.GameType == GameTypes.Any)
				wallpaperNames = ManagerNames;

			this.newWallpaper = Math.Min((byte)(wallpaperNames.Length - 1), (byte)pokeBox.Wallpaper);
			foreach (string wallpaperName in wallpaperNames) {
				comboBoxWallpaper.Items.Add(wallpaperName);
			}

			if (pokeBox is ManagerPokeBox) {
				newUsingCustomWallpaper = ManagerPokeBox.UsingCustomWallpaper;
				newWallpaperName = ManagerPokeBox.WallpaperName;
				customWallpaperStartIndex = comboBoxWallpaper.Items.Count;
				if (!ManagerPokeBox.UsingCustomWallpaper) {
					ManagerPokeBoxWallpapers outResult;
					if (Enum.TryParse<ManagerPokeBoxWallpapers>(ManagerPokeBox.WallpaperName, true, out outResult)) {
						this.comboBoxWallpaper.SelectedIndex = (byte)outResult;
					}
					else {
						this.comboBoxWallpaper.SelectedIndex = (byte)ManagerPokeBoxWallpapers.SimpleRS;
					}
				}
				for (int i = 0; i < PokemonDatabase.NumCustomWallpapers; i++) {
					string wallpaperName = PokemonDatabase.GetCustomWallpaperNameAt(i);
					comboBoxWallpaper.Items.Add(wallpaperName);
					if (ManagerPokeBox.UsingCustomWallpaper && ManagerPokeBox.WallpaperName.ToLower() == wallpaperName.ToLower()) {
						this.comboBoxWallpaper.SelectedIndex = customWallpaperStartIndex + i;
					}
				}
			}
			else {
				this.comboBoxWallpaper.SelectedIndex = newWallpaper;
			}

			if (pokeBox.GameType == GameTypes.Colosseum || pokeBox.GameType == GameTypes.XD)
				comboBoxWallpaper.IsEnabled = false;

			// FREEEEDOOMMMMM
			if (pokeBox.GameType == GameTypes.Any) {
				this.textBoxName.MaxLength = 20;
			}
			else if (pokeBox.PokePC.GameSave.IsJapanese) {
				this.buttonDots.Visibility = Visibility.Hidden;
				this.buttonQuotes1.Visibility = Visibility.Hidden;
				this.buttonQuotes2.Visibility = Visibility.Hidden;
				this.buttonSingleQuote1.Visibility = Visibility.Hidden;
				this.buttonSingleQuote2.Visibility = Visibility.Hidden;
				//this.textBoxName.MaxLength = 5;
			}


			this.textBoxName.Text = newName;
			this.textBoxName.Focus();
			textBoxName.SelectAll();
		}

		private ManagerPokeBox ManagerPokeBox {
			get { return pokeBox as ManagerPokeBox; }
		}

		private void OKClicked(object sender, RoutedEventArgs e) {
			pokeBox.Name = newName;
			pokeBox.Wallpaper = (PokeBoxWallpapers)newWallpaper;
			if (pokeBox is ManagerPokeBox) {
				ManagerPokeBox.UsingCustomWallpaper = newUsingCustomWallpaper;
				ManagerPokeBox.WallpaperName = newWallpaperName;
			}
			PokeManager.RefreshUI();
			DialogResult = true;
		}

		private void OnNicknameChanged(object sender, TextChangedEventArgs e) {
			newName = textBoxName.Text;
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

		public static bool? ShowDialog(Window owner, IPokeBox pokeBox) {
			EditBoxWindow window = new EditBoxWindow(pokeBox);
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OnEnterPressed(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				DialogResult = true;
				newName = textBoxName.Text;
				pokeBox.Name = newName;
				pokeBox.Wallpaper = (PokeBoxWallpapers)newWallpaper;
				if (pokeBox is ManagerPokeBox) {
					ManagerPokeBox.UsingCustomWallpaper = newUsingCustomWallpaper;
					ManagerPokeBox.WallpaperName = newWallpaperName;
				}
				PokeManager.RefreshUI();
				Close();
			}
		}

		private void OnWallpaperSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (comboBoxWallpaper.SelectedIndex != -1) {
				int newIndex = comboBoxWallpaper.SelectedIndex;
				newWallpaper = (byte)comboBoxWallpaper.SelectedIndex;
				if (pokeBox is ManagerPokeBox) {
					if (newIndex < customWallpaperStartIndex) {
						newUsingCustomWallpaper = false;
						newWallpaperName = ((ManagerPokeBoxWallpapers)newIndex).ToString();
					}
					else {
						newUsingCustomWallpaper = true;
						newWallpaperName = PokemonDatabase.GetCustomWallpaperNameAt(newIndex - customWallpaperStartIndex);
					}
				}
				if (pokeBox.GameType == GameTypes.Ruby || pokeBox.GameType == GameTypes.Sapphire)
					imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperRS" + ((PokeBoxWallpapers)newWallpaper).ToString(), "WallpaperDefault");
				else if (pokeBox.GameType == GameTypes.PokemonBox)
					imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperRS" + ((PokemonBoxPokeBoxWallpapers)newWallpaper).ToString(), "WallpaperDefault");
				else if (pokeBox.GameType == GameTypes.Emerald)
					imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperEmerald" + ((PokeBoxWallpapers)newWallpaper).ToString(), "WallpaperDefault");
				else if (pokeBox.GameType == GameTypes.FireRed || pokeBox.GameType == GameTypes.LeafGreen)
					imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperFRLG" + ((PokeBoxWallpapers)newWallpaper).ToString(), "WallpaperDefault");
				else if (pokeBox.GameType == GameTypes.Colosseum)
					imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperColosseum" + ((ColosseumPokeBoxWallpapers)newWallpaper).ToString() + "E", "WallpaperDefault");
				else if (pokeBox.GameType == GameTypes.XD)
					imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperXD" + ((XDPokeBoxWallpapers)newWallpaper).ToString(), "WallpaperDefault");
				else if (pokeBox.GameType == GameTypes.Any) {
					if (newUsingCustomWallpaper) {
						BitmapSource bitmap = PokemonDatabase.GetCustomWallpaper(newWallpaperName);
						if (bitmap != null)
							imageWallpaper.Source = bitmap;
						else
							imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperDefault");
					}
					else {
						imageWallpaper.Source = ResourceDatabase.GetImageFromName("WallpaperManager" + newWallpaperName, "WallpaperDefault");
					}
				}
			}
		}
	}
}
