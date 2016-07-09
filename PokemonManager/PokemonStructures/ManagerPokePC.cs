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
	public class ManagerPokePC : IPokePC {

		#region Members

		private ManagerGameSave gameSave;
		private List<ManagerPokeBox> boxes;
		private int currentBox;
		private string name;
		private bool livingDexMode;
		private bool revealEggs;

		#endregion

		public ManagerPokePC(ManagerGameSave gameSave, string name, bool livingDexMode = false, bool revealEggs = false) {
			this.gameSave	= gameSave;
			this.boxes	= new List<ManagerPokeBox>();
			this.currentBox	= 0;
			this.name		= name;
			this.livingDexMode = livingDexMode;
			this.revealEggs = revealEggs;
			if (!livingDexMode) {
				for (int i = 0; i < 16; i++) {
					int wallpaper = i * 2 + 1;
					if (i == 13) wallpaper = (int)ManagerPokeBoxWallpapers.PokeCenterE;
					else if (i == 14) wallpaper = (int)ManagerPokeBoxWallpapers.MachineE;
					else if (i == 15) wallpaper = (int)ManagerPokeBoxWallpapers.SimpleE;
					AddBox((PokeBoxWallpapers)wallpaper);
				}
			}
			else {
				for (int i = 1; i <= 386; i += 30) {
					AddBox(i.ToString("000") + "-" + Math.Min(386, (i + 29)).ToString("000"), (PokeBoxWallpapers)ManagerPokeBoxWallpapers.SimpleE);
				}
				AddBox("Unown", (PokeBoxWallpapers)ManagerPokeBoxWallpapers.SimpleE);
			}
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
		public string Name {
			get { return name; }
			set {
				if (value == "")
					name = "Row " + (RowIndex + 1).ToString();
				else
					name = value;
			}
		}
		public int RowIndex {
			get { return gameSave.PokePCRowIndexOf(this); }
		}
		public bool IsLivingDex {
			get { return livingDexMode; }
			set { livingDexMode = value; }
		}
		public bool RevealEggs {
			get { return revealEggs; }
			set { revealEggs = value; }
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
			bool finished = false;

			// Check the first box at the index offset
			if (indexStart > 0) {
				for (int j = indexStart; j < boxes[boxStart].NumSlots && !finished; j++) {
					if (boxes[boxStart][j] == null) {
						boxes[boxStart][j] = pokemon;
						finished = true;
					}
				}
				boxStart++;
			}

			// Check the rest of the boxes
			for (int i = 0; i < boxes.Count && !finished; i++) {
				int boxIndex = (i + boxStart) % boxes.Count;
				for (int j = 0; j < boxes[boxIndex].NumSlots && !finished; j++) {
					if (boxes[boxIndex][j] == null) {
						boxes[boxIndex][j] = pokemon;
						finished = true;
					}
				}
			}
		}

		#endregion

		#region Box Properties

		public uint NumBoxes {
			get { return (uint)boxes.Count; }
		}
		public int CurrentBox {
			get { return currentBox; }
			set {
				int mod = (value % boxes.Count);
				currentBox = (mod < 0 ? (mod + boxes.Count) : mod);
			}
		}

		#endregion

		#region Box Management

		public void AddBox(ManagerPokeBox box) {
			InsertBox(boxes.Count, box);
		}
		public void AddBox(PokeBoxWallpapers wallpaper) {
			AddBox(new ManagerPokeBox(this, (byte)boxes.Count, "BOX" + (boxes.Count + 1).ToString(), wallpaper));
		}
		public void AddBox(string name, PokeBoxWallpapers wallpaper) {
			AddBox(new ManagerPokeBox(this, (byte)boxes.Count, name, wallpaper));
		}
		public void AddBox() {
			byte[] possibilities = new byte[] {
				1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 15,
				(byte)ManagerPokeBoxWallpapers.PokeCenterE, (byte)ManagerPokeBoxWallpapers.MachineE, (byte)ManagerPokeBoxWallpapers.SimpleE
			};
			Random random = new Random((int)DateTime.Now.Ticks);
			byte wallpaper = possibilities[random.Next(possibilities.Length)];
			AddBox(new ManagerPokeBox(this, (byte)boxes.Count, "BOX" + (boxes.Count + 1).ToString(), (PokeBoxWallpapers)wallpaper));
		}

		public void InsertBox(int index) {
			byte[] possibilities = new byte[] {
				1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 15,
				(byte)ManagerPokeBoxWallpapers.PokeCenterE, (byte)ManagerPokeBoxWallpapers.MachineE, (byte)ManagerPokeBoxWallpapers.SimpleE
			};
			Random random = new Random((int)DateTime.Now.Ticks);
			byte wallpaper = possibilities[random.Next(possibilities.Length)];
			InsertBox(index, new ManagerPokeBox(this, (byte)boxes.Count, "BOX" + (boxes.Count + 1).ToString(), (PokeBoxWallpapers)wallpaper));
		}
		public void InsertBox(int index, ManagerPokeBox box) {
			GameSave.IsChanged = true;
			boxes.Insert(index, box);
			for (int i = 0; i < box.NumSlots; i++) {
				if (box[i] != null)
					box[i].GameType = GameTypes.Any;
			}
			UpdateBoxNumbers();
		}
		public void RemoveBoxAt(int index) {
			if (boxes.Count > 1) {
				boxes.RemoveAt(index);
				UpdateBoxNumbers();
			}
		}
		public void MoveBox(int oldIndex, int newIndex) {
			ManagerPokeBox box = boxes[oldIndex];
			boxes.RemoveAt(oldIndex);
			boxes.Insert(newIndex, box);
			UpdateBoxNumbers();
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

		public void Clear() {
			this.boxes		= new List<ManagerPokeBox>();
			this.currentBox	= 0;
			for (int i = 0; i < 16; i++) {
				int wallpaper = i * 2 + 1;
				if (i == 13) wallpaper = (int)ManagerPokeBoxWallpapers.PokeCenterE;
				else if (i == 14) wallpaper = (int)ManagerPokeBoxWallpapers.MachineE;
				else if (i == 15) wallpaper = (int)ManagerPokeBoxWallpapers.SimpleE;
				AddBox((PokeBoxWallpapers)wallpaper);
			}
		}
		public void Load(byte[] data) {
			uint version = 0;
			this.currentBox = (int)LittleEndian.ToUInt32(data, 0);
			this.boxes = new List<ManagerPokeBox>();
			int numBoxes = (int)LittleEndian.ToUInt32(data, 4);
			if (data.Length == 8 + numBoxes * (40 + 1 + 2400)) {
				for (int i = 0; i < numBoxes; i++) {
					string name = ByteHelper.ReadString(data, 8 + i * (40 + 1 + 2400), 20);
					ManagerPokeBoxWallpapers wallpaper = (ManagerPokeBoxWallpapers)data[12 + i * (40 + 1 + 2400) + 40];
					string wallpaperName = wallpaper.ToString();
					byte[] storage = ByteHelper.SubByteArray(8 + i * (40 + 1 + 2400) + 41, data, 2400);
					boxes.Add(new ManagerPokeBox(this, version, (byte)i, name, false, wallpaperName, storage));
				}
			}
			else {
				// We added version info
				version = LittleEndian.ToUInt32(data, 0);
				if (version == 1) {
					this.currentBox = (int)LittleEndian.ToUInt32(data, 4);
					this.boxes = new List<ManagerPokeBox>();
					numBoxes = (int)LittleEndian.ToUInt32(data, 8);
					for (int i = 0; i < numBoxes; i++) {
						string name = ByteHelper.ReadString(data, 12 + i * (40 + 1 + 2400), 20);
						ManagerPokeBoxWallpapers wallpaper = (ManagerPokeBoxWallpapers)data[12 + i * (40 + 1 + 2400) + 40];
						string wallpaperName = wallpaper.ToString();
						byte[] storage = ByteHelper.SubByteArray(12 + i * (40 + 1 + 2400) + 41, data, 2400);
						boxes.Add(new ManagerPokeBox(this, version, (byte)i, name, false, wallpaperName, storage));
					}
				}
				else if (version == 2) {
					this.currentBox = (int)LittleEndian.ToUInt32(data, 4);
					this.boxes = new List<ManagerPokeBox>();
					numBoxes = (int)LittleEndian.ToUInt32(data, 8);
					for (int i = 0; i < numBoxes; i++) {
						int boxPosition = 12 + i * (40 + 1 + 60 + 3000);
						string name = ByteHelper.ReadString(data, boxPosition, 20);
						bool customWallpaper = LittleEndian.ToBool(data, boxPosition + 40);
						string wallpaperName = ByteHelper.ReadString(data, boxPosition + 41, 30);
						byte[] storage = ByteHelper.SubByteArray(12 + i * (40 + 1 + 60 + 3000) + 101, data, 3000);
						boxes.Add(new ManagerPokeBox(this, version, (byte)i, name, customWallpaper, wallpaperName, storage));
					}
				}
			}
		}
		public byte[] GetFinalData() {
			uint version = 2;
			List<byte> data = new List<byte>();
			if (version == 1) {

				data.AddRange(BitConverter.GetBytes(version)); // version
				data.AddRange(BitConverter.GetBytes(currentBox));
				data.AddRange(BitConverter.GetBytes(boxes.Count));
				for (int i = 0; i < boxes.Count; i++) {
					data.AddRange(ByteHelper.GetStringBytes(boxes[i].Name, 20));
					data.Add((byte)boxes[i].Wallpaper);
					data.AddRange(boxes[i].GetFinalStorageData(version));
				}
			}
			else if (version == 2) {
				data.AddRange(BitConverter.GetBytes(version)); // version
				data.AddRange(BitConverter.GetBytes(currentBox));
				data.AddRange(BitConverter.GetBytes(boxes.Count));
				for (int i = 0; i < boxes.Count; i++) {
					data.AddRange(ByteHelper.GetStringBytes(boxes[i].Name, 20));
					data.AddRange(BitConverter.GetBytes(boxes[i].UsingCustomWallpaper));
					data.AddRange(ByteHelper.GetStringBytes(boxes[i].WallpaperName, 30));
					data.AddRange(boxes[i].GetFinalStorageData(version));
				}
			}
			return data.ToArray();
		}

		#endregion

		#region Private Helpers

		private void UpdateBoxNumbers() {
			for (int i = 0; i < boxes.Count; i++) {
				boxes[i].BoxNumber = (uint)i;
			}
		}

		#endregion
	}
}
