using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class DecorationInventory {

		#region Members

		private Inventory inventory;
		private Dictionary<DecorationTypes, DecorationPocket> pockets;
		private List<PlacedDecoration> secretBaseDecorations;
		private List<PlacedDecoration> bedroomDecorations;

		#endregion

		public DecorationInventory(Inventory inventory) {
			this.inventory				= inventory;
			this.pockets				= new Dictionary<DecorationTypes, DecorationPocket>();
			this.secretBaseDecorations	= new List<PlacedDecoration>();
			this.bedroomDecorations		= new List<PlacedDecoration>();
		}

		#region Containment Properties

		public Inventory Inventory {
			get { return inventory; }
		}
		public IGameSave GameSave {
			get { return inventory.GameSave; }
		}
		public GameTypes GameType {
			get { return inventory.GameSave.GameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(inventory.GameSave); }
		}
		public Generations Generation {
			get { return inventory.GameSave.Generation; }
		}
		public Platforms Platform {
			get { return inventory.GameSave.Platform; }
		}

		#endregion

		#region Decoration Pockets

		public DecorationPocket this[DecorationTypes pocketType] {
			get {
				if (pockets.ContainsKey(pocketType))
					return pockets[pocketType];
				return null;
			}
		}
		public void AddPocket(DecorationTypes pocketType, uint pocketSize, uint maxStackSize) {
			pockets.Add(pocketType, new DecorationPocket(this, pocketType, pocketSize, maxStackSize));
		}
		public bool ContainsPocket(DecorationTypes pocketType) {
			return pockets.ContainsKey(pocketType);
		}
		public void Clear() {
			foreach (KeyValuePair<DecorationTypes, DecorationPocket> container in pockets) {
				container.Value.Clear();
			}
		}

		#endregion

		#region Placed Decoration Properties

		public List<PlacedDecoration> SecretBaseDecorations {
			get { return secretBaseDecorations; }
			set { secretBaseDecorations = value; }
		}
		public List<PlacedDecoration> BedroomDecorations {
			get { return bedroomDecorations; }
			set { bedroomDecorations = value; }
		}

		public uint TotalSecretBaseDecorationSlots {
			get { return 16; }
		}
		public uint SecretBaseDecorationSlotsUsed {
			get { return (uint)secretBaseDecorations.Count; }
		}
		public uint SecretBaseDecorationSlotsLeft {
			get { return (uint)(16 - secretBaseDecorations.Count); }
		}
		public uint TotalBedroomDecorationSlots {
			get { return 16; }
		}
		public uint BedroomDecorationSlotsUsed {
			get { return (uint)bedroomDecorations.Count; }
		}
		public uint BedroomDecorationSlotsLeft {
			get { return (uint)(16 - bedroomDecorations.Count); }
		}

		#endregion

		#region Placed Decoration Management

		public PlacedDecoration GetDecorationInSecretBaseAt(int index) {
			return secretBaseDecorations[index];
		}
		public PlacedDecoration GetDecorationInBedroomAt(int index) {
			return bedroomDecorations[index];
		}
		public void PlaceDecorationInSecretBase(byte id, byte x, byte y) {
			if (secretBaseDecorations.Count < 16) {
				GameSave.IsChanged = true;
				secretBaseDecorations.Add(new PlacedDecoration(id, x, y));
				pockets[ItemDatabase.GetDecorationFromID(id).DecorationType].UpdateListViewItems();
			}
		}
		public void PlaceDecorationInBedroom(byte id, byte x, byte y) {
			if (bedroomDecorations.Count < 16) {
				GameSave.IsChanged = true;
				bedroomDecorations.Add(new PlacedDecoration(id, x, y));
				pockets[ItemDatabase.GetDecorationFromID(id).DecorationType].UpdateListViewItems();
			}
		}
		public bool CanPlaceDecorationInSecretBase(byte id, byte x, byte y) {
			if (secretBaseDecorations.Count >= 16)
				return false;
			SecretBasePlacementTypes[,] finalGrid = FinalPlacementGrid;
			DecorationData decorationData = ItemDatabase.GetDecorationFromID(id);
			for (int x2 = 0; x2 < decorationData.Width; x2++) {
				for (int y2 = 0; y2 < decorationData.Height; y2++) {
					int finalX = x - decorationData.OriginX + x2;
					int finalY = y - decorationData.OriginY + y2;
					if (finalX < 0 || finalY < 0 || finalX >= RoomData.Width || finalY >= RoomData.Height)
						return false;
					SecretBasePlacementTypes place = decorationData.PlacementGrid[x2, y2];
					if (!CanPlaceTypeOn(finalGrid[finalX, finalY], place))
						return false;
				}
			}
			return true;
		}
		public void PutAwayDecorationInSecretBaseAt(int index) {
			GameSave.IsChanged = true;
			byte id = secretBaseDecorations[index].ID;
			secretBaseDecorations.RemoveAt(index);
			RemoveInvalidDecorations();
			foreach (KeyValuePair<DecorationTypes, DecorationPocket> pair in pockets) {
				pair.Value.UpdateListViewItems();
			}
		}
		public void PutAwayDecorationInBedroomAt(int index) {
			GameSave.IsChanged = true;
			byte id = bedroomDecorations[index].ID;
			bedroomDecorations.RemoveAt(index);
			RemoveInvalidDecorations();
			pockets[ItemDatabase.GetDecorationFromID(id).DecorationType].UpdateListViewItems();
		}
		public void PutAwayDecoration(int index, DecorationTypes decorationType) {
			GameSave.IsChanged = true;
			int usageIndex = GetIndexOfDecorationInUse(index, decorationType);
			DecorationUsages usage = GetDecorationUsage(index, decorationType);
			if (usage == DecorationUsages.SecretBase)
				PutAwayDecorationInSecretBaseAt(usageIndex);
			else
				PutAwayDecorationInBedroomAt(usageIndex);
		}
		public bool IsDecorationInUse(int index, DecorationTypes decorationType) {
			byte id = pockets[decorationType][index].ID;
			int idsBeforeIndex = 0;
			for (int i = 0; i < index; i++) {
				if (pockets[decorationType][i].ID == id)
					idsBeforeIndex++;
			}
			int idsInUse = 0;
			for (int i = 0; secretBaseDecorations != null && i < secretBaseDecorations.Count; i++) {
				if (secretBaseDecorations[i].ID == id)
					idsInUse++;
			}
			for (int i = 0; bedroomDecorations != null && i < bedroomDecorations.Count; i++) {
				if (bedroomDecorations[i].ID == id)
					idsInUse++;
			}
			return idsInUse > idsBeforeIndex;
		}
		public DecorationUsages GetDecorationUsage(int index, DecorationTypes decorationType) {
			byte id = pockets[decorationType][index].ID;
			int idsBeforeIndex = 0;
			for (int i = 0; i < index; i++) {
				if (pockets[decorationType][i].ID == id)
					idsBeforeIndex++;
			}
			for (int i = 0; secretBaseDecorations != null && i < secretBaseDecorations.Count; i++) {
				if (secretBaseDecorations[i].ID == id) {
					if (idsBeforeIndex == 0)
						return DecorationUsages.SecretBase;
					idsBeforeIndex--;
				}
			}
			for (int i = 0; bedroomDecorations != null && i < bedroomDecorations.Count; i++) {
				if (bedroomDecorations[i].ID == id) {
					if (idsBeforeIndex == 0)
						return DecorationUsages.Bedroom;
					idsBeforeIndex--;
				}
			}
			return DecorationUsages.Unused;
		}
		public int GetNumDecorationsWithIDInUse(byte id) {
			int idsInUse = 0;
			for (int i = 0; secretBaseDecorations != null && i < secretBaseDecorations.Count; i++) {
				if (secretBaseDecorations[i].ID == id)
					idsInUse++;
			}
			for (int i = 0; bedroomDecorations != null && i < bedroomDecorations.Count; i++) {
				if (bedroomDecorations[i].ID == id)
					idsInUse++;
			}
			return idsInUse;
		}
		public int GetIndexOfDecorationInUse(int index, DecorationTypes decorationType) {
			byte id = pockets[decorationType][index].ID;
			int idsBeforeIndex = 0;
			for (int i = 0; i < index; i++) {
				if (pockets[decorationType][i].ID == id)
					idsBeforeIndex++;
			}
			int idsInUse = 0;
			for (int i = 0; secretBaseDecorations != null && i < secretBaseDecorations.Count; i++) {
				if (secretBaseDecorations[i].ID == id) {
					if (idsInUse == idsBeforeIndex)
						return i;
					idsInUse++;
				}
			}
			for (int i = 0; bedroomDecorations != null && i < bedroomDecorations.Count; i++) {
				if (bedroomDecorations[i].ID == id) {
					if (idsInUse == idsBeforeIndex)
						return i;
					idsInUse++;
				}
			}
			return -1;
		}

		#endregion

		private RoomData RoomData {
			get { return SecretBaseDatabase.GetLocationFromID((GameSave as GBAGameSave).SecretBaseLocation).RoomData; }
		}

		private static readonly Dictionary<SecretBasePlacementTypes, List<SecretBasePlacementTypes>> AllowedPlacements = new Dictionary<SecretBasePlacementTypes, List<SecretBasePlacementTypes>>() {
			{SecretBasePlacementTypes.Solid, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Floor
			}},
			{SecretBasePlacementTypes.Back, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Floor,
				SecretBasePlacementTypes.Wall,
				SecretBasePlacementTypes.Reserved
			}},
			{SecretBasePlacementTypes.BackNoWall, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Floor,
				SecretBasePlacementTypes.Reserved
			}},
			{SecretBasePlacementTypes.Poster, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Wall
			}},
			{SecretBasePlacementTypes.Mat, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Floor
			}},
			{SecretBasePlacementTypes.MatCenter, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Floor
			}},
			{SecretBasePlacementTypes.Doll, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Mat,
				SecretBasePlacementTypes.MatCenter,
				SecretBasePlacementTypes.DollSide,
				SecretBasePlacementTypes.DollBack
			}},
			{SecretBasePlacementTypes.DollSide, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Mat,
				SecretBasePlacementTypes.Doll,
				SecretBasePlacementTypes.DollSide,
				SecretBasePlacementTypes.DollBack
			}},
			{SecretBasePlacementTypes.Board, new List<SecretBasePlacementTypes>() {
				SecretBasePlacementTypes.Hole
			}},
		};

		private SecretBasePlacementTypes[,] FinalPlacementGrid {
			get {
				SecretBasePlacementTypes[,] finalGrid = (SecretBasePlacementTypes[,])RoomData.PlacementGrid.Clone();

				List<DecorationDataTypes> dollTypes = new List<DecorationDataTypes>() {
					DecorationDataTypes.SmallDoll,
					DecorationDataTypes.LargeDoll
				};

				// Everything else
				foreach (PlacedDecoration decoration in secretBaseDecorations) {
					if (dollTypes.Contains(decoration.DecorationData.DataType))
						continue;
					for (int x = 0; x < decoration.DecorationData.Width; x++) {
						for (int y = 0; y < decoration.DecorationData.Height; y++) {
							int finalX = decoration.X - decoration.DecorationData.OriginX + x;
							int finalY = decoration.Y - decoration.DecorationData.OriginY + y;
							if (finalX < 0 || finalY < 0 || finalX >= RoomData.Width || finalY >= RoomData.Height)
								continue;
							SecretBasePlacementTypes place = decoration.DecorationData.PlacementGrid[x, y];
							if (CanPlaceTypeOn(finalGrid[finalX, finalY], place)) {
								if (place != SecretBasePlacementTypes.Empty && place != SecretBasePlacementTypes.DollSide && place != SecretBasePlacementTypes.DollBack) {
									finalGrid[finalX, finalY] = place;
								}
							}
						}
					}
				}

				// Dolls
				foreach (PlacedDecoration decoration in secretBaseDecorations) {
					if (!dollTypes.Contains(decoration.DecorationData.DataType))
						continue;

					for (int x = 0; x < decoration.DecorationData.Width; x++) {
						for (int y = 0; y < decoration.DecorationData.Height; y++) {
							int finalX = decoration.X - decoration.DecorationData.OriginX + x;
							int finalY = decoration.Y - decoration.DecorationData.OriginY + y;
							if (finalX < 0 || finalY < 0 || finalX >= RoomData.Width || finalY >= RoomData.Height)
								continue;
							SecretBasePlacementTypes place = decoration.DecorationData.PlacementGrid[x, y];
							if (CanPlaceTypeOn(finalGrid[finalX, finalY], place)) {
								if (place != SecretBasePlacementTypes.Empty && place != SecretBasePlacementTypes.DollSide && place != SecretBasePlacementTypes.DollBack) {
									finalGrid[finalX, finalY] = place;
								}
							}
						}
					}
				}

				return finalGrid;
			}
		}

		private bool CanPlaceTypeOn(SecretBasePlacementTypes roomType, SecretBasePlacementTypes decorationType) {
			if (decorationType == SecretBasePlacementTypes.Empty)
				return true;
			else if (decorationType == SecretBasePlacementTypes.DollBack)
				return roomType != SecretBasePlacementTypes.BedroomFloor;
			else
				return AllowedPlacements[decorationType].Contains(roomType);
		}

		private void RemoveInvalidDecorations() {
			SecretBasePlacementTypes[,] finalGrid = (SecretBasePlacementTypes[,])RoomData.PlacementGrid.Clone();

			List<DecorationDataTypes> dollTypes = new List<DecorationDataTypes>() {
				DecorationDataTypes.SmallDoll,
				DecorationDataTypes.LargeDoll
			};

			// Everything else
			foreach (PlacedDecoration decoration in secretBaseDecorations) {
				if (dollTypes.Contains(decoration.DecorationData.DataType))
					continue;
				for (int x = 0; x < decoration.DecorationData.Width; x++) {
					for (int y = 0; y < decoration.DecorationData.Height; y++) {
						int finalX = decoration.X - decoration.DecorationData.OriginX + x;
						int finalY = decoration.Y - decoration.DecorationData.OriginY + y;
						if (finalX < 0 || finalY < 0 || finalX >= RoomData.Width || finalY >= RoomData.Height)
							continue;
						SecretBasePlacementTypes place = decoration.DecorationData.PlacementGrid[x, y];
						if (CanPlaceTypeOn(finalGrid[finalX, finalY], place)) {
							if (place != SecretBasePlacementTypes.Empty && place != SecretBasePlacementTypes.DollSide && place != SecretBasePlacementTypes.DollBack) {
								finalGrid[finalX, finalY] = place;
							}
						}
					}
				}
			}
			List<PlacedDecoration> toRemove = new List<PlacedDecoration>();
			// Dolls
			foreach (PlacedDecoration decoration in secretBaseDecorations) {
				if (!dollTypes.Contains(decoration.DecorationData.DataType))
					continue;

				for (int x = 0; x < decoration.DecorationData.Width; x++) {
					for (int y = 0; y < decoration.DecorationData.Height; y++) {
						int finalX = decoration.X - decoration.DecorationData.OriginX + x;
						int finalY = decoration.Y - decoration.DecorationData.OriginY + y;
						if (finalX < 0 || finalY < 0 || finalX >= RoomData.Width || finalY >= RoomData.Height) {
							if (!toRemove.Contains(decoration))
								toRemove.Add(decoration);
							continue;
						}
						SecretBasePlacementTypes place = decoration.DecorationData.PlacementGrid[x, y];
						if (CanPlaceTypeOn(finalGrid[finalX, finalY], place)) {
							if (place != SecretBasePlacementTypes.Empty && place != SecretBasePlacementTypes.DollSide && place != SecretBasePlacementTypes.DollBack) {
								finalGrid[finalX, finalY] = place;
							}
						}
						else if (!toRemove.Contains(decoration)) {
							toRemove.Add(decoration);
						}
					}
				}
			}
			foreach (PlacedDecoration decoration in toRemove) {
				secretBaseDecorations.Remove(decoration);
			}
		}
	}
}
