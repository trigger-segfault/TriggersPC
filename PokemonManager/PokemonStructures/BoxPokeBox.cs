using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
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
	public class BoxPokeBox : IPokeBox {

		#region Members

		private IPokePC pokePC;
		private uint boxNumber;
		private bool boxA;
		private byte[] rawName;
		private PokeBoxWallpapers wallpaper;
		private IPokemon[] pokemonList;

		#endregion

		public BoxPokeBox(IPokePC pokePC, uint boxNumber, byte[] rawName, bool boxA, PokeBoxWallpapers wallpaper, byte[] storage) {
			this.pokePC = pokePC;
			this.boxNumber = boxNumber;
			this.rawName = rawName;
			this.boxA = boxA;
			this.wallpaper = wallpaper;
			this.pokemonList = new IPokemon[30];

			for (int i = 0; i < 30; i++) {
				int boxIndex = (i % 6) + (i / 6) * 12 + (!boxA ? 6 : 0);
				BoxPokemon pkm = new BoxPokemon(ByteHelper.SubByteArray(boxIndex * 84, storage, 84));
				if (pkm.Experience != 0 && pkm.SpeciesID != 0 && pkm.Checksum != 0) {
					if (pkm.IsValid)
						pokemonList[i] = pkm;
					else
						pokemonList[i] = BoxPokemon.CreateInvalidPokemon(pkm);
					pokemonList[i].PokeContainer = this;
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
				pokePC.GameSave.IsChanged = true;
				pokemonList[index] = (value != null ? (value is BoxPokemon ? value : value.CreateBoxPokemon()): null);
				if (pokemonList[index] != null) {
					pokemonList[index].GameType = GameType;
					pokemonList[index].RestoreHealth();
					pokemonList[index].PokeContainer = this;
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
					pokePC.GameSave.IsChanged = true;
					pokemonList[i] = null;
					break;
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
				if (Name == "BOX" + (boxNumber / 2 + 1).ToString()) {
					boxNumber = value;
					rawName = GBACharacterEncoding.GetBytes("BOX" + (boxNumber / 2 + 1).ToString(), 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
				}
			}
		}
		public string RealName {
			get {
				if (rawName[0] == 0 || rawName[0] == byte.MaxValue) {
					if (pokePC.GameSave.IsJapanese)
						return "ボックス" + (boxNumber / 2 + 1);
					return "BOX" + (boxNumber / 2 + 1);
				}
				return GBACharacterEncoding.GetString(rawName, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
			}
		}
		public string Name {
			get {
				if (rawName[0] == 0 || rawName[0] == byte.MaxValue) {
					if (pokePC.GameSave.IsJapanese)
						return "ボックス" + (boxNumber / 2 + 1) + (boxA ? " A" : " B");
					return "BOX" + (boxNumber / 2 + 1) + (boxA ? " A" : " B");
				}
				return GBACharacterEncoding.GetString(rawName, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English) + (boxA ? " A" : " B");
			}
			set {
				if (boxA)
					pokePC[(int)boxNumber + 1].Name = value;
				pokePC.GameSave.IsChanged = true;

				if (value == "") {
					if (pokePC.GameSave.IsJapanese)
						rawName = GBACharacterEncoding.GetBytes("ボックス" + (boxNumber / 2 + 1).ToString(), 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
					else
						rawName = GBACharacterEncoding.GetBytes("BOX" + (boxNumber / 2 + 1).ToString(), 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
				}
				else {
					rawName = GBACharacterEncoding.GetBytes(value, 9, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English);
				}
			}
		}
		public PokeBoxWallpapers Wallpaper {
			get { return wallpaper; }
			set {
				if (boxA)
					pokePC[(int)boxNumber + 1].Wallpaper = value;
				pokePC.GameSave.IsChanged = true;
				wallpaper = value;
			}
		}
		public BitmapSource WallpaperImage {
			get { return ResourceDatabase.GetImageFromName("WallpaperRS" + ((PokemonBoxPokeBoxWallpapers)Wallpaper).ToString(), "WallpaperDefault"); }
		}
		public BoxPokeBox PrimaryBox {
			get {
				if (boxA)
					return this;
				else
					return (BoxPokeBox)pokePC[(int)boxNumber - 1];
			}
		}

		#endregion

		#region Loading/Saving

		public byte[] GetRawNameData() {
			return rawName;
		}

		public void SetFinalStorageData(byte[] data) {
			// Data is the size of the combined 2 boxes A and B

			for (int i = 0; i < 30; i++) {
				int boxIndex = (i % 6) + (i / 6) * 12 + (!boxA ? 6 : 0);

				if (pokemonList[i] != null)
					ByteHelper.ReplaceBytes(data, boxIndex * 84, pokemonList[i].GetFinalData());
				else
					ByteHelper.ReplaceBytes(data, boxIndex * 84, new byte[84]);
			}
		}

		#endregion
	}
}
