using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class XDPokePC : IPokePC {

		#region Members

		private IGameSave gameSave;
		private byte[] raw;
		private int currentBox;
		private XDPokeBox[] boxes;
		private XDPokeParty party;
		private XDDaycare daycare;
		private XDPurificationChamber[] chambers;
		private byte[] purifierRaw;
		//private XDPurifier purifier;

		#endregion

		public XDPokePC(IGameSave gameSave) {
			this.gameSave = gameSave;
			this.raw = null;
			this.boxes = new XDPokeBox[8];
			this.currentBox = 0;
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
		public XDPurificationChamber GetChamber(int index) {
			return chambers[index];
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

		public uint NumBoxes {
			get { return (byte)boxes.Length; }
		}
		public int CurrentBox {
			get { return currentBox; }
			set {
				int mod = (value % boxes.Length);
				currentBox = (mod < 0 ? (mod + boxes.Length) : mod);
			}
		}

		#endregion

		#region PC Modifiers

		public void AddParty(byte[] data) {
			this.party = new XDPokeParty(this, data);
		}
		public void AddDaycare(byte[] data) {
			this.daycare = new XDDaycare(this, data);
		}
		public void AddPurifier(byte[] data) {
			this.purifierRaw = data;
			this.chambers = new XDPurificationChamber[9];
			for (int i = 0; i < 9; i++) {
				this.chambers[i] = new XDPurificationChamber(this, (byte)i, ByteHelper.SubByteArray(i * 984, data, 984));
			}
		}
		public byte[] GetPurifierFinalData() {
			for (int i = 0; i < 9; i++) {
				ByteHelper.ReplaceBytes(purifierRaw, i * 984, chambers[i].GetFinalData());
			}
			return purifierRaw;
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
			for (int i = 0; i < 9; i++) {
				foreach (IPokemon pokemon in chambers[i])
					pokemon.GameType = gameType;
			}
		}

		#endregion

		#region Saving/Loading

		public void Load(byte[] data) {
			this.raw = data;
			for (int i = 0; i < 8; i++) {
				boxes[i] = new XDPokeBox(this, (byte)i, ByteHelper.SubByteArray(i * 5900, data, 5900));
			}
		}
		public byte[] GetFinalData() {
			for (int i = 0; i < 8; i++) {
				ByteHelper.ReplaceBytes(raw, i * 5900, boxes[i].GetFinalData());
			}
			return raw;
		}

		#endregion

	}
}
