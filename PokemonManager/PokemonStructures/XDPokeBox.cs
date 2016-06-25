using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GC;
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
	public class XDPokeBox : IPokeBox {

		#region Members

		private IPokePC pokePC;
		private byte[] raw;
		private byte boxNumber;
		private PokeBoxWallpapers wallpaper;
		private PokemonStorage pokemonStorage;

		#endregion

		public XDPokeBox(IPokePC pokePC, byte boxNumber, byte[] data) {
			this.pokePC = pokePC;
			this.raw = data;
			this.boxNumber = boxNumber;
			this.wallpaper = (PokeBoxWallpapers)boxNumber;
			this.pokemonStorage = new PokemonStorage(ByteHelper.SubByteArray(20, data, 196 * 30), PokemonFormatTypes.Gen3XD, this);
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
				pokemonStorage[index] = (value != null ? (value is XDPokemon ? value : value.CreateXDPokemon(((GCGameSave)GameSave).CurrentRegion)) : null);
				if (pokemonStorage[index] != null) {
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
			get { return (uint)boxNumber; }
			set {  }
		}
		public string Name {
			get { return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(0, raw, 20), pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English); }
			set {
				if (value == "") {
					if (pokePC.GameSave.IsJapanese)
						ByteHelper.ReplaceBytes(raw, 0, GCCharacterEncoding.GetBytes("ボックス" + (boxNumber + 1).ToString(), 10, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English));
					else
						ByteHelper.ReplaceBytes(raw, 0, GCCharacterEncoding.GetBytes("BOX" + (boxNumber + 1).ToString(), 10, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English));
				}
				else {
					ByteHelper.ReplaceBytes(raw, 0, GCCharacterEncoding.GetBytes(value, 10, pokePC.GameSave.IsJapanese ? Languages.Japanese : Languages.English));
				}
			}
		}
		public PokeBoxWallpapers Wallpaper {
			get { return wallpaper; }
			set {  }
		}
		public BitmapSource WallpaperImage {
			get { return ResourceDatabase.GetImageFromName("WallpaperXD" + ((XDPokeBoxWallpapers)Wallpaper).ToString(), "WallpaperDefault"); }
		}

		#endregion

		#region Saving/Loading

		public byte[] GetFinalData() {
			for (int i = 0; i < 30; i++) {
				XDPokemon pkm = (XDPokemon)pokemonStorage[i];
				if (pkm != null)
					ByteHelper.ReplaceBytes(raw, 20 + i * 196, pkm.GetFinalData());
				else
					ByteHelper.ReplaceBytes(raw, 20 + i * 196, new byte[196]);
			}
			return raw;
		}

		#endregion
	}
}
