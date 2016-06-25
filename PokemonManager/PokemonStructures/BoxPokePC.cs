using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class BoxPokePC : IPokePC {

		#region Members

		private IGameSave gameSave;
		private byte[] raw;
		private BoxPokeBox[] boxes;
		private int currentBox;

		#endregion

		public BoxPokePC(IGameSave gameSave) {
			this.gameSave	= gameSave;
			this.raw		= null;
			this.boxes		= null;
			this.currentBox	= 0;
		}

		#region Containment Properties

		public IGameSave GameSave {
			get { return gameSave; }
		}
		public GameTypes GameType {
			get { return gameSave.GameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(gameSave); }
		}

		#endregion

		#region PC Accessors

		public IPokeParty Party {
			get { return null; }
		}
		public IDaycare Daycare {
			get { return null; }
		}
		public IPokeBox this[int index] {
			get { return boxes[index]; }
		}
		public IEnumerator<IPokemon> GetEnumerator() {
			return new PokePCEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public bool HasRoomForPokemon(int amount = 1) {
			foreach (IPokeBox box in boxes) {
				amount -= (int)(box.NumSlots - box.NumPokemon);
			}
			return amount <= 0;
		}
		public void PlacePokemonInNextAvailableSlot(int boxStart, int indexStart, IPokemon pokemon) {
			
			// Check the first box at the index offset
			if (indexStart > 0) {
				for (int j = indexStart; j < boxes[boxStart].NumSlots; j++) {
					if (boxes[boxStart][j] == null) {
						boxes[boxStart][j] = pokemon;
						return;
					}
				}
				boxStart++;
			}

			// Check the rest of the boxes
			for (int i = 0; i < boxes.Length; i++) {
				int boxIndex = (i + boxStart) % boxes.Length;
				for (int j = 0; j < boxes[boxIndex].NumSlots; j++) {
					if (boxes[boxIndex][j] == null) {
						boxes[boxIndex][j] = pokemon;
						return;
					}
				}
			}
		}

		#endregion

		#region Box Properties

		public int CurrentBox {
			get { return currentBox; }
			set {
				int mod = (value % boxes.Length);
				currentBox = (mod < 0 ? (mod + boxes.Length) : mod);
			}
		}
		public uint NumBoxes {
			get { return (uint)boxes.Length; }
		}

		#endregion

		#region PC Modifiers

		public void ApplyGameType(GameTypes gameType) {
			foreach (IPokeBox pokeBox in boxes) {
				for (int i = 0; i < pokeBox.NumSlots; i++) {
					if (pokeBox[i] != null)
						pokeBox[i].GameType = gameType;
				}
			}
		}

		#endregion

		#region Loading/Saving

		public void Load(byte[] data) {
			this.raw = data;
			this.boxes = new BoxPokeBox[25 * 2]; // Since boxes are still stored in sets of 30 and not 60 in this program
			this.currentBox = (int)(LittleEndian.ToUInt32(data, 0) * 2);
			for (int i = 0; i < 25; i++) {
				byte[] boxSetData = ByteHelper.SubByteArray(4 + i * 60 * 84, data, 60 * 84);
				PokeBoxWallpapers wallpaper = (PokeBoxWallpapers)data[4 + 1500 * 84 + 225 + i];
				byte[] nameRaw = ByteHelper.SubByteArray(4 + 1500 * 84 + i * 9, data, 9);
				boxes[i * 2]		= new BoxPokeBox(this, (byte)(i * 2), nameRaw, true, wallpaper, boxSetData);
				boxes[i * 2 + 1]	= new BoxPokeBox(this, (byte)(i * 2 + 1), nameRaw, false, wallpaper, boxSetData);
			}
		}
		public byte[] GetFinalData() {
			LittleEndian.WriteUInt32((uint)(currentBox / 2), raw, 0);
			for (int i = 0; i < 25; i++) {
				byte[] boxSetData = new byte[60 * 84];
				boxes[i * 2].SetFinalStorageData(boxSetData);
				boxes[i * 2 + 1].SetFinalStorageData(boxSetData);
				ByteHelper.ReplaceBytes(raw, 4 + i * 60 * 84, boxSetData);
				ByteHelper.ReplaceBytes(raw, 4 + 1500 * 84 + i * 9, boxes[i * 2].GetRawNameData());
				raw[4 + 1500 * 84 + 225 + i] = (byte)boxes[i * 2].Wallpaper;
			}
			return raw;
		}

		#endregion
	}
}
