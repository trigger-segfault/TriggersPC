using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
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
using System.Windows.Threading;

namespace PokemonManager.Windows {

	enum PokedexViewModes {
		All,
		Owned,
		Seen,
		Missing,
		Living
	}

	public partial class PokedexViewer : UserControl {

		private readonly string[] GBAComboNames = new string[] {
			"All Pokémon", "Owned Pokémon", "Seen Pokémon", "Missing Pokémon", "Living Pokédex"
		};
		private readonly string[] ColosseumComboNames = new string[] {
			"All Pokémon", "Registered Pokémon", "Seen Pokémon", "Missing Pokémon", "Living Pokédex"
		};
		private readonly string[] XDComboNames = new string[] {
			"All Pokémon", "Registered Pokémon", "Missing Pokémon", "Living Pokédex"
		};

		private IGameSave gameSave;
		private int childrenHeight;
		private PokedexViewModes viewMode;
		private bool[] livingFlags;
		private bool[] unownLivingFlags;
		private bool[] deoxysLivingFlags;
		private bool loaded;

		public PokedexViewer() {
			InitializeComponent();


			this.labelPokedexName.Content = "";
			loaded = true;
		}

		public void RefreshUI() {
			if (gameSave != null) {
				childrenHeight = 0;
				stackPanelStats.Children.Clear();

				int livingCount = 0;
				int unownCount = 0;
				livingFlags = new bool[386];
				unownLivingFlags = new bool[28];
				deoxysLivingFlags = new bool[4];

				if (gameSave is ManagerGameSave) {
					for (int j = 0; j < (gameSave as ManagerGameSave).NumPokePCRows; j++) {
						foreach (IPokemon pokemon in (gameSave as ManagerGameSave).GetPokePCRow(j)) {
							int dexID = pokemon.DexID;
							if (dexID == 0 || pokemon.IsEgg)
								continue;
							if (!livingFlags[dexID - 1]) {
								livingCount++;
								livingFlags[dexID - 1] = true;
							}
							if (dexID == 201) { // Unown Living Flags
								int formID = pokemon.FormID;
								if (!unownLivingFlags[formID]) {
									unownCount++;
									unownLivingFlags[formID] = true;
								}
							}
							if (dexID == 386) { // Unown Living Flags
								int formID = pokemon.FormID;
								if (!deoxysLivingFlags[formID])
									deoxysLivingFlags[formID] = true;
							}
						}
					}
				}
				else {
					foreach (IPokemon pokemon in gameSave.PokePC) {
						int dexID = pokemon.DexID;
						if (dexID == 0 || pokemon.IsEgg)
							continue;
						if (!livingFlags[dexID - 1]) {
							livingCount++;
							livingFlags[dexID - 1] = true;
						}
						if (dexID == 201) { // Unown Living Flags
							int formID = pokemon.FormID;
							if (!unownLivingFlags[formID]) {
								unownCount++;
								unownLivingFlags[formID] = true;
							}
						}
						if (dexID == 386) { // Unown Living Flags
							int formID = pokemon.FormID;
							if (!deoxysLivingFlags[formID])
								deoxysLivingFlags[formID] = true;
						}
					}
				}
				comboBoxViewType.Items.Clear();
				viewMode = PokedexViewModes.All;
				if (gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD) {
					loaded = false;
					for (int i = 0; i < ColosseumComboNames.Length && gameSave.GameType == GameTypes.Colosseum; i++)
						comboBoxViewType.Items.Add(ColosseumComboNames[i]);
					for (int i = 0; i < XDComboNames.Length && gameSave.GameType == GameTypes.XD; i++)
						comboBoxViewType.Items.Add(XDComboNames[i]);
					comboBoxViewType.SelectedIndex = 0;
					loaded = true;
					this.labelPokedexName.Content = "Strategy Memo";

					AddStat("Pokémon Living", livingCount.ToString());
					AddStat("Pokémon Registed", gameSave.PokemonOwned.ToString());
					if (gameSave.GameType == GameTypes.Colosseum)
						AddStat("Pokémon Seen", gameSave.PokemonSeen.ToString());
					AddSeparator();
					if (gameSave.GameType == GameTypes.Colosseum) {
						AddStat("Snagged Pokémon", ((GCGameSave)gameSave).SnaggedPokemon.ToString());
						AddStat("Purified Pokémon", ((GCGameSave)gameSave).PurifiedPokemon.ToString());
					}
					else {
						AddStat("Snagged Pokémon", "Unknown");
						AddStat("Purified Pokémon", "Unknown");
					}
				}
				else {
					loaded = false;
					for (int i = 0; i < GBAComboNames.Length; i++)
						comboBoxViewType.Items.Add(GBAComboNames[i]);
					comboBoxViewType.SelectedIndex = 0;
					loaded = true;
					if (gameSave.HasNationalPokedex)
						this.labelPokedexName.Content = "National Pokédex";
					else
						this.labelPokedexName.Content = "Pokédex";
					
					int kantoOwned = 0, kantoSeen = 0;
					int johtoOwned = 0, johtoSeen = 0;
					int hoennOwned = 0, hoennSeen = 0;
					for (ushort i = 1; i <= 386; i++) {
						if (i <= 151) {
							kantoOwned += (gameSave.IsPokemonOwned(i) ? 1 : 0);
							kantoSeen += (gameSave.IsPokemonSeen(i) ? 1 : 0);
						}
						else if (i <= 251) {
							johtoOwned += (gameSave.IsPokemonOwned(i) ? 1 : 0);
							johtoSeen += (gameSave.IsPokemonSeen(i) ? 1 : 0);
						}
						else {
							hoennOwned += (gameSave.IsPokemonOwned(i) ? 1 : 0);
							hoennSeen += (gameSave.IsPokemonSeen(i) ? 1 : 0);
						}
					}
					int owned = kantoOwned + johtoOwned + hoennOwned;
					int seen = kantoSeen + johtoSeen + hoennSeen;

					AddStat("Pokémon Living", livingCount.ToString());
					AddStat("Pokémon Owned", owned.ToString());
					if (gameSave.GameType != GameTypes.Any)
						AddStat("Pokémon Seen", seen.ToString());
					string oakStatus = "Get Working";
					if (livingCount == 386 && unownCount == 28)
						oakStatus = "Living Pokédex";
					else if (owned == 386)
						oakStatus = "Complete" + (unownCount == 28 ? "+" : "");
					else if (owned > 300)
						oakStatus = "Almost There";
					else if (owned > 150)
						oakStatus = "Some Progress";
					AddStat("Oak's Opinion", oakStatus);
					AddSeparator();
					if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire || gameSave.GameType == GameTypes.Emerald) {
						AddStat("Hoenn Owned", hoennOwned.ToString());
						if (gameSave.GameType != GameTypes.Any)
							AddStat("Hoenn Seen", hoennSeen.ToString());
						AddStat("Kanto Owned", kantoOwned.ToString());
						if (gameSave.GameType != GameTypes.Any)
							AddStat("Kanto Seen", kantoSeen.ToString());
						AddStat("Johto Owned", johtoOwned.ToString());
						if (gameSave.GameType != GameTypes.Any)
							AddStat("Johto Seen", johtoSeen.ToString());
					}
					else {
						AddStat("Kanto Owned", kantoOwned.ToString());
						if (gameSave.GameType != GameTypes.Any)
							AddStat("Kanto Seen", kantoSeen.ToString());
						AddStat("Johto Owned", johtoOwned.ToString());
						if (gameSave.GameType != GameTypes.Any)
							AddStat("Johto Seen", johtoSeen.ToString());
						AddStat("Hoenn Owned", hoennOwned.ToString());
						if (gameSave.GameType != GameTypes.Any)
							AddStat("Hoenn Seen", hoennSeen.ToString());
					}
					AddSeparator();
					AddStat("Unown Forms", unownCount.ToString());
				}
				FillPokedex();
				gridStats.Height = 41 + childrenHeight;
				stackPanelStats.Height = childrenHeight;
				gridPokemonInfo.Margin = new Thickness(235, 10 + gridStats.Height + 5, 0, 0);
			}
		}

		public void AddStat(string name, string value) {
			Grid grid = new Grid();
			grid.Height = 24;
			childrenHeight += 24;

			Label labelName = new Label();
			labelName.Padding = new Thickness(3, 4, 3, 4);
			labelName.Content = name;
			labelName.FontWeight = FontWeights.Bold;
			grid.Children.Add(labelName);

			Label labelValue = new Label();
			labelValue.Padding = new Thickness(5, 4, 5, 4);
			labelValue.Content = value;
			labelValue.FontWeight = FontWeights.Bold;
			labelValue.Margin = new Thickness(115, 0, 0, 0);
			grid.Children.Add(labelValue);

			stackPanelStats.Children.Add(grid);
		}

		public void AddSeparator() {
			Separator separator = new Separator();
			separator.Height = 9;
			childrenHeight += 9;
			separator.Margin = new Thickness(4, 0, 4, 0);
			separator.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			stackPanelStats.Children.Add(separator);
		}

		public void FillPokedex() {
			listViewPokemon.Items.Clear();
			for (int i = 1; i <= 386; i++) {
				bool living = livingFlags[i - 1];
				bool seen = gameSave.IsPokemonSeen((ushort)i);
				bool owned = gameSave.IsPokemonOwned((ushort)i);

				for (int j = 0; j < 28 && (j == 0 || (viewMode == PokedexViewModes.Living && (i == 201 || (j < 4 && i == 386)))); j++) {
					if (viewMode == PokedexViewModes.Living) {
						if (i == 201)
							living = unownLivingFlags[j];
						if (i == 386)
							living = deoxysLivingFlags[j];
					}

					if ((viewMode == PokedexViewModes.Missing && seen) ||
						(viewMode == PokedexViewModes.Owned && !owned) ||
						(viewMode == PokedexViewModes.Seen && (!seen || owned)) ||
						(viewMode == PokedexViewModes.Living && !living)) {
						continue;
					}

					ListViewItem listViewItem = new ListViewItem();
					Grid grid = new Grid();
					grid.Height = 28;

					byte form = 255;
					if (i == 201) {
						byte val = 0;
						uint personality = gameSave.GetPokedexPokemonPersonality(201);
						val = ByteHelper.SetBits(val, 0, ByteHelper.GetBits(personality, 0, 2));
						val = ByteHelper.SetBits(val, 2, ByteHelper.GetBits(personality, 8, 2));
						val = ByteHelper.SetBits(val, 4, ByteHelper.GetBits(personality, 16, 2));
						val = ByteHelper.SetBits(val, 6, ByteHelper.GetBits(personality, 24, 2));
						form = (byte)(val % 28);
					}

					Image boxImage = new Image();
					boxImage.Source = PokemonDatabase.GetPokemonBoxImageFromDexID((ushort)i, gameSave.IsPokedexPokemonShiny((ushort)i), (viewMode == PokedexViewModes.Living && (i == 201 || i == 386)) ? (byte)j : form);
					boxImage.Width = 32;
					boxImage.Height = 32;
					boxImage.Margin = new Thickness(-2, -7, -2, -2);
					boxImage.HorizontalAlignment = HorizontalAlignment.Left;
					boxImage.VerticalAlignment = VerticalAlignment.Top;
					grid.Children.Add(boxImage);

					if (!seen) {
						Rectangle boxMask = new Rectangle();
						boxMask.Width = 32;
						boxMask.Height = 32;
						boxMask.Margin = new Thickness(-2, -7, -2, -2);
						boxMask.OpacityMask = new ImageBrush(PokemonDatabase.GetPokemonBoxImageFromDexID((ushort)i, false, (viewMode == PokedexViewModes.Living && (i == 201 || i == 386)) ? (byte)j : byte.MaxValue));
						boxMask.Fill = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0));
						boxMask.HorizontalAlignment = HorizontalAlignment.Left;
						boxMask.VerticalAlignment = VerticalAlignment.Top;
						grid.Children.Add(boxMask);
					}

					if (seen) {
						Image ownedImage = new Image();
						ownedImage.Width = 9;
						ownedImage.Height = 9;
						ownedImage.Margin = new Thickness(32, 10, 0, 0);
						ownedImage.Source = ResourceDatabase.GetImageFromName(living ? "PokedexPokeballLiving" : (owned ? "PokedexPokeballOwned" : "PokedexEyeSeen"));
						if (!living && owned && (gameSave.GameType == GameTypes.Colosseum || gameSave.GameType == GameTypes.XD))
							ownedImage.Source = ResourceDatabase.GetImageFromName("PokedexRRegistered");
						ownedImage.HorizontalAlignment = HorizontalAlignment.Left;
						ownedImage.VerticalAlignment = VerticalAlignment.Top;
						ownedImage.Stretch = Stretch.None;
						grid.Children.Add(ownedImage);
					}

					Label dexID = new Label();
					dexID.FontWeight = FontWeights.Bold;
					dexID.VerticalAlignment = VerticalAlignment.Center;
					dexID.Content = "No" + i.ToString("000");
					dexID.Margin = new Thickness(40, 0, 0, 0);
					grid.Children.Add(dexID);

					Label name = new Label();
					name.VerticalAlignment = VerticalAlignment.Center;
					name.Content = PokemonDatabase.GetPokemonFromDexID((ushort)i).Name;
					if (viewMode == PokedexViewModes.Living && (i == 201 || i == 386))
						name.Content = PokemonDatabase.GetPokemonFromDexID((ushort)i).GetForm((byte)j).Name;
					name.Margin = new Thickness(84, 0, 0, 0);
					grid.Children.Add(name);

					listViewItem.Tag = i;
					listViewItem.Content = grid;
					listViewPokemon.Items.Add(listViewItem);
				}
			}
		}

		public void LoadGameSave(IGameSave gameSave) {
			this.gameSave = gameSave;

			RefreshUI();
		}

		private void OnViewTypeChanged(object sender, SelectionChangedEventArgs e) {
			if (loaded && comboBoxViewType.SelectedIndex != -1) {
				viewMode = (PokedexViewModes)(comboBoxViewType.SelectedIndex + (gameSave.GameType == GameTypes.XD && comboBoxViewType.SelectedIndex >= 2 ? 1 : 0));
				FillPokedex();
			}
		}
	}
}
