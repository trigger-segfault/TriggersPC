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
	public class GBAPokePC : IPokePC {

		#region Members

		private IGameSave gameSave;
		private byte[] raw;
		private GBAPokeBox[] boxes;
		private GBAPokeParty party;
		private int currentBox;
		private GBADaycare daycare;

		#endregion

		public GBAPokePC(IGameSave gameSave) {
			this.gameSave	= gameSave;
			this.raw		= null;
			this.boxes		= null;
			this.party		= null;
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
			get { return party; }
		}
		public IDaycare Daycare {
			get { return daycare; }
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

		public void AddParty(byte[] data) {
			this.party = new GBAPokeParty(this, data);
		}
		public void AddDaycare(byte[] data, GameCodes gameCode) {
			this.daycare = new GBADaycare(this, data, gameCode);
		}
		public void ApplyGameType(GameTypes gameType) {
			foreach (IPokeBox pokeBox in boxes) {
				for (int i = 0; i < pokeBox.NumSlots; i++) {
					if (pokeBox[i] != null)
						pokeBox[i].GameType = gameType;
				}
			}
			foreach (IPokemon pokemon in party)
				pokemon.GameType = gameType;
		}

		#endregion

		#region Loading/Saving

		public void Load(byte[] data) {
			this.raw = data;
			this.boxes = new GBAPokeBox[14];
			this.currentBox = (int)LittleEndian.ToUInt32(raw, 0);
			for (int i = 0; i < 14; i++) {
				byte[] name = ByteHelper.SubByteArray(33604 + i * 9, raw, 9);
				byte[] storage = ByteHelper.SubByteArray(4 + i * 2400, raw, 2400);
				byte wallpaper = ByteHelper.SubByteArray(33730 + i, raw, 1)[0];
				boxes[i] = new GBAPokeBox(this, (byte)i, name, (PokeBoxWallpapers)wallpaper, storage);
			}
		}
		public byte[] GetFinalData() {
			LittleEndian.WriteUInt32((uint)currentBox, raw, 0);
			List<byte[]> boxNames = new List<byte[]>(14);
			List<byte[]> boxStorage = new List<byte[]>(14);
			List<byte> boxWallpapers = new List<byte>(14);
			foreach (GBAPokeBox box in boxes) {
				boxNames.Add(box.GetRawNameData());
				boxWallpapers.Add((byte)box.Wallpaper);
				boxStorage.Add(box.GetFinalStorageData());
			}
			List<byte> data = new List<byte>();
			data.AddRange(raw.Take<byte>(4));
			foreach (byte[] bytes in boxStorage)
				data.AddRange(bytes);
			foreach (byte[] bytes in boxNames)
				data.AddRange(bytes);
			data.AddRange(boxWallpapers);
			raw = data.ToArray();
			return raw;
		}

		#endregion
	}
}
