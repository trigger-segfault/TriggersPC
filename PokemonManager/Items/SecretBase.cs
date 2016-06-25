using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public abstract class SecretBase {

		#region Trainer

		public abstract byte[] TrainerNameRaw { get; set; }
		public abstract string TrainerName { get; set; }
		public abstract Genders TrainerGender { get; set; }
		public abstract ushort TrainerID { get; set; }
		public abstract ushort SecretID { get; set; }

		#endregion

		#region Location

		public abstract IGameSave GameSave { get; }
		public abstract byte LocationID { get; protected set; }
		public abstract bool IsPlayerSecretBase { get; }

		public LocationData LocationData {
			get { return SecretBaseDatabase.GetLocationFromID(LocationID); }
		}
		public RoomData RoomData {
			get { return LocationData.RoomData; }
		}
		public RouteData RouteData {
			get { return LocationData.RouteData; }
		}

		public void SetNewLocation(byte newLocationID) {
			if (newLocationID != LocationID) {
				LocationData newLocationData = SecretBaseDatabase.GetLocationFromID(newLocationID);
				RoomData newRoomData = newLocationData.RoomData;

				if (newRoomData.Type != RoomData.Type || newRoomData.Layout != RoomData.Layout) {
					while (SlotsUsed > 0) {
						PutAwayDecorationAt(0);
					}
				}
				LocationID = newLocationID;
			}
		}

		#endregion

		#region Placed Decorations

		public abstract List<PlacedDecoration> PlacedDecorations { get; }

		public int TotalSlots {
			get { return 16; }
		}
		public int SlotsUsed {
			get { return PlacedDecorations.Count; }
		}
		public int SlotsLeft {
			get { return (16 - PlacedDecorations.Count); }
		}

		public abstract void PlaceDecoration(byte id, byte x, byte y);
		public abstract void PutAwayDecoration(PlacedDecoration decoration);
		public abstract void PutAwayDecorationAt(int index);

		public abstract List<GBAPokemon> PokemonTeam { get; }
		public abstract bool HasTeam { get; }
		public abstract Languages Language { get; set; }

		#endregion

		protected static readonly Dictionary<SecretBasePlacementTypes, List<SecretBasePlacementTypes>> AllowedPlacements = new Dictionary<SecretBasePlacementTypes, List<SecretBasePlacementTypes>>() {
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

		protected SecretBasePlacementTypes[,] FinalPlacementGrid {
			get {
				SecretBasePlacementTypes[,] finalGrid = (SecretBasePlacementTypes[,])RoomData.PlacementGrid.Clone();

				List<DecorationDataTypes> dollTypes = new List<DecorationDataTypes>() {
					DecorationDataTypes.SmallDoll,
					DecorationDataTypes.LargeDoll
				};

				// Everything else
				foreach (PlacedDecoration decoration in PlacedDecorations) {
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
				foreach (PlacedDecoration decoration in PlacedDecorations) {
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

		protected bool CanPlaceTypeOn(SecretBasePlacementTypes roomType, SecretBasePlacementTypes decorationType) {
			if (decorationType == SecretBasePlacementTypes.Empty)
				return true;
			else if (decorationType == SecretBasePlacementTypes.DollBack)
				return roomType != SecretBasePlacementTypes.BedroomFloor;
			else
				return AllowedPlacements[decorationType].Contains(roomType);
		}

		protected void RemoveInvalidDecorations() {
			SecretBasePlacementTypes[,] finalGrid = (SecretBasePlacementTypes[,])RoomData.PlacementGrid.Clone();

			List<DecorationDataTypes> dollTypes = new List<DecorationDataTypes>() {
				DecorationDataTypes.SmallDoll,
				DecorationDataTypes.LargeDoll
			};

			// Everything else
			foreach (PlacedDecoration decoration in PlacedDecorations) {
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
			foreach (PlacedDecoration decoration in PlacedDecorations) {
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
				PlacedDecorations.Remove(decoration);
			}
		}

		public bool CanPlaceDecoration(byte id, byte x, byte y) {
			if (PlacedDecorations.Count >= 16)
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
	}
}
