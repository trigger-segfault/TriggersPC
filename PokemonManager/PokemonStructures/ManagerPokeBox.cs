using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public class ManagerPokeBox : IPokeBox {

		#region Members

		private IPokePC pokePC;
		private uint boxNumber;
		private string name;
		private PokeBoxWallpapers wallpaper;
		private GBAPokemon[] pokemonList;
		private bool usingCustomWallpaper;
		private string wallpaperName;

		#endregion

		public ManagerPokeBox(IPokePC pokePC, uint boxNumber, string name, PokeBoxWallpapers wallpaper) {
			this.pokePC = pokePC;
			this.boxNumber = boxNumber;
			this.name = name;
			this.wallpaper = wallpaper;
			this.usingCustomWallpaper = false;
			this.wallpaperName = ((ManagerPokeBoxWallpapers)wallpaper).ToString();
			this.pokemonList = new GBAPokemon[30];
		}
		public ManagerPokeBox(IPokePC pokePC, uint version, uint boxNumber, string name, bool usingCustom, string wallpaperName, byte[] storage) {
			this.pokePC = pokePC;
			this.boxNumber = boxNumber;
			this.name = name;
			this.usingCustomWallpaper = usingCustom;
			this.wallpaperName = wallpaperName;
			this.pokemonList = new GBAPokemon[30];

			if (version <= 1) {
				for (int i = 0; i < 30; i++) {
					GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(i * 80, storage, 80));
					if (!ByteHelper.CompareBytes(0, pkm.Raw)) {
						if (pkm.IsValid)
							pokemonList[i] = pkm;
						else
							pokemonList[i] = GBAPokemon.CreateInvalidPokemon(pkm);
						pokemonList[i].PokeContainer = this;
					}
					else {
						pokemonList[i] = null;
					}
				}
			}
			else if (version == 2) {
				for (int i = 0; i < 30; i++) {
					GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(i * 100, storage, 80));
					if (!ByteHelper.CompareBytes(0, pkm.Raw)) {
						if (pkm.IsValid) {
							pokemonList[i] = pkm;
							pkm.DeoxysForm = storage[i * 100 + 80];
						}
						else {
							pokemonList[i] = GBAPokemon.CreateInvalidPokemon(pkm);
						}
						pokemonList[i].PokeContainer = this;
					}
					else {
						pokemonList[i] = null;
					}
				}
			}
		}

		#region Containment Properties

		public IPokePC PokePC {
			get { return pokePC; }
		}
		public IGameSave GameSave {
			get { return pokePC.GameSave; }
		}
		public GameTypes GameType {
			get { return pokePC.GameSave.GameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(pokePC.GameSave); }
		}
		public ContainerTypes Type {
			get { return ContainerTypes.Box; }
		}

		#endregion

		#region Pokemon Accessors

		public IPokemon this[int index] {
			get { return pokemonList[index]; }
			set {
				if (pokePC != null)
					pokePC.GameSave.IsChanged = true;
				pokemonList[index] = (GBAPokemon)(value != null ? (value is GBAPokemon ? value : value.CreateGBAPokemon(pokePC != null ? GameType : GameTypes.Any)) : null);
				if (pokemonList[index] != null) {
					pokemonList[index].PokeContainer = this;
					pokemonList[index].RestoreHealth();
					if (pokePC != null) {
						pokemonList[index].GameType = GameType;
						if (pokePC.GameSave != null)
							pokePC.GameSave.OwnPokemon(pokemonList[index]);
					}
				}
			}
		}
		public IEnumerator<IPokemon> GetEnumerator() {
			return new PokeContainerEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public int IndexOf(IPokemon pokemon) {
			for (int i = 0; i < 30; i++) {
				if (pokemonList[i] == pokemon)
					return i;
			}
			return -1;
		}
		public void Remove(IPokemon pokemon) {
			for (int i = 0; i < 30; i++) {
				if (pokemonList[i] == pokemon) {
					if (pokePC != null)
						pokePC.GameSave.IsChanged = true;
					pokemonList[i] = null;
				}
			}
		}

		#endregion

		#region Box Properties

		public uint NumPokemon {
			get {
				uint count = 0;
				for (int i = 0; i < 30; i++) {
					if (pokemonList[i] != null)
						count++;
				}
				return count;
			}
		}
		public uint NumSlots {
			get { return 30; }
		}
		public bool IsEmpty {
			get { return NumPokemon == 0; }
		}
		public uint BoxNumber {
			get { return boxNumber; }
			set {
				if (name == "BOX" + (boxNumber + 1).ToString()) {
					boxNumber = value;
					name = "BOX" + (boxNumber + 1).ToString();
				}
			}
		}
		public string Name {
			get { return name; }
			set {
				pokePC.GameSave.IsChanged = true;
				name = value;
				if (value == "")
					name = "BOX" + (boxNumber + 1).ToString();
			}
		}
		public bool UsingCustomWallpaper {
			get { return usingCustomWallpaper; }
			set {
				pokePC.GameSave.IsChanged = true;
				usingCustomWallpaper = value;
			}
		}
		public string WallpaperName {
			get { return wallpaperName; }
			set {
				pokePC.GameSave.IsChanged = true;
				wallpaperName = value;
			}
		}
		public PokeBoxWallpapers Wallpaper {
			get { return wallpaper; }
			set { wallpaper = value; }
		}
		public BitmapSource WallpaperImage {
			get {
				if (usingCustomWallpaper) {
					BitmapSource bitmap = PokemonDatabase.GetCustomWallpaper(wallpaperName);
					if (bitmap != null)
						return bitmap;
					return ResourceDatabase.GetImageFromName("WallpaperDefault");
				}
				else {
					return ResourceDatabase.GetImageFromName("WallpaperManager" + wallpaperName, "WallpaperDefault");
				}
			}
		}

		#endregion

		#region Loading/Saving

		public byte[] GetFinalStorageData(uint version) {
			if (version <= 1) {
				List<byte> data = new List<byte>(30 * 80);
				foreach (IPokemon pokemon in pokemonList) {
					if (pokemon != null)
						data.AddRange(pokemon.GetFinalData().Take<byte>(80));
					else
						data.AddRange(new byte[80]);
				}
				return data.ToArray();
			}
			else if (version == 2) {
				List<byte> data = new List<byte>(30 * 100);
				foreach (IPokemon pokemon in pokemonList) {
					if (pokemon != null) {
						data.AddRange(pokemon.GetFinalData().Take<byte>(80));
						data.Add(pokemon.DeoxysForm);
						data.AddRange(new byte[19]);
					}
					else {
						data.AddRange(new byte[100]);
					}
				}
				return data.ToArray();
			}
			return null;
		}

		#endregion
	}
}
