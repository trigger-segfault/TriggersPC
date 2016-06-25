using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public class GBAPokeBox : IPokeBox {

		#region Members

		private IPokePC pokePC;
		private uint boxNumber;
		private byte[] rawName;
		private PokeBoxWallpapers wallpaper;
		private PokemonStorage pokemonStorage;

		#endregion

		public GBAPokeBox(IPokePC pokePC, uint boxNumber, byte[] rawName, PokeBoxWallpapers wallpaper) {
			this.pokePC = pokePC;
			this.boxNumber = boxNumber;
			this.rawName = rawName;
			this.wallpaper = wallpaper;
			this.pokemonStorage = new PokemonStorage(new byte[2400], PokemonFormatTypes.Gen3GBA, this);
		}
		public GBAPokeBox(IPokePC pokePC, uint boxNumber, byte[] rawName, PokeBoxWallpapers wallpaper, byte[] storage) {
			if (storage.Length != 2400)
				throw new Exception("GBA Storage size should be 2400 bytes");
			this.pokePC = pokePC;
			this.boxNumber = boxNumber;
			this.rawName = rawName;
			this.wallpaper = wallpaper;
			this.pokemonStorage = new PokemonStorage(storage, PokemonFormatTypes.Gen3GBA, this);
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
			get { return pokemonStorage[index]; }
			set {
				pokePC.GameSave.IsChanged = true;
				pokemonStorage[index] = (value != null ? (value is GBAPokemon ? value : value.CreateGBAPokemon(GameType)): null);
				if (pokemonStorage[index] != null) {
					pokemonStorage[index].GameType = GameType;
					pokemonStorage[index].PokeContainer = this;
					pokemonStorage[index].RestoreHealth();
					if (pokePC.GameSave != null)
						pokePC.GameSave.OwnPokemon(pokemonStorage[index]);
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
				if (pokemonStorage[i] == pokemon)
					return i;
			}
			return -1;
		}
		public void Remove(IPokemon pokemon) {
			for (int i = 0; i < 30; i++) {
				if (pokemonStorage[i] == pokemon) {
					pokePC.GameSave.IsChanged = true;
					pokemonStorage[i] = null;
					break;
				}
			}
		}

		#endregion

		#region Box Properties

		public uint NumPokemon {
			get { return pokemonStorage.NumPokemon; }
		}
		public uint NumSlots {
			get { return 30; }
		}
		public bool IsEmpty {
			get { return pokemonStorage.Count == 0; }
		}
		public uint BoxNumber {
			get { return boxNumber; }
			set {
				if (Name == "BOX" + (boxNumber + 1).ToString()) {
					boxNumber = value;
					rawName = GBACharacterEncoding.GetBytes("BOX" + (boxNumber + 1).ToString(), 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
				}
			}
		}
		public string Name {
			get { return GBACharacterEncoding.GetString(rawName, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English); }
			set {
				pokePC.GameSave.IsChanged = true;

				if (value == "") {
					if (pokePC.GameSave.IsJapanese)
						rawName = GBACharacterEncoding.GetBytes("ボックス" + (boxNumber + 1).ToString(), 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
					else
						rawName = GBACharacterEncoding.GetBytes("BOX" + (boxNumber + 1).ToString(), 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
				}
				else {
					rawName = GBACharacterEncoding.GetBytes(value, 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
				}
			}
		}
		public PokeBoxWallpapers Wallpaper {
			get { return wallpaper; }
			set {
				pokePC.GameSave.IsChanged = true;
				wallpaper = value;
			}
		}
		public BitmapSource WallpaperImage {
			get {
				if (GameType == GameTypes.Ruby || GameType == GameTypes.Sapphire || GameType == GameTypes.PokemonBox)
					return ResourceDatabase.GetImageFromName("WallpaperRS" + Wallpaper.ToString(), "WallpaperDefault");
				else if (GameType == GameTypes.Emerald)
					return ResourceDatabase.GetImageFromName("WallpaperEmerald" + Wallpaper.ToString(), "WallpaperDefault");
				else if (GameType == GameTypes.FireRed || GameType == GameTypes.LeafGreen)
					return ResourceDatabase.GetImageFromName("WallpaperFRLG" + Wallpaper.ToString(), "WallpaperDefault");
				return null;
			}
		}

		#endregion

		#region Loading/Saving

		public byte[] GetRawNameData() {
			return rawName;
		}

		public byte[] GetFinalStorageData() {
			return pokemonStorage.GetFinalData();
		}

		#endregion
	}
}
