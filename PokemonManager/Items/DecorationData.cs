using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class DecorationData {

		#region Members

		private byte id;
		private string name;
		private string description;
		private DecorationTypes decorationType;
		private uint price;
		private uint coinsPrice;
		private uint bpPrice;
		private uint sootPrice;
		private bool sale;

		private byte originX;
		private byte originY;
		private SecretBasePlacementTypes[,] placementGrid;
		private DecorationDataTypes dataType;

		#endregion

		public DecorationData(DataRow row) {
			this.id				= (byte)(long)row["ID"];
			this.name			= row["Name"] as string;
			this.description	= row["Description"] as string;
			this.decorationType	= DecorationData.GetDecorationTypeFromString(row["Type"] as string);
			this.price			= (uint)(long)row["Price"];
			this.coinsPrice		= (uint)(long)row["Coins"];
			this.bpPrice		= (uint)(long)row["BP"];
			this.sootPrice		= (uint)(long)row["Soot"];
			this.sale			= (bool)row["Sale"];

			CompileTypeData(row["DataType"] as string);
		}

		#region Properties

		public byte ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public string Description {
			get { return description; }
		}
		public DecorationTypes DecorationType {
			get { return decorationType; }
		}
		public uint Price {
			get { return price; }
		}
		public uint CoinsPrice {
			get { return coinsPrice; }
		}
		public uint BattlePointsPrice {
			get { return bpPrice; }
		}
		public uint VolcanicAshPrice {
			get { return sootPrice; }
		}
		public bool IsOnlyPurchasableDuringSale {
			get { return sale; }
		}
		public SecretBasePlacementTypes[,] PlacementGrid {
			get { return placementGrid; }
		}
		public byte OriginX {
			get { return originX; }
		}
		public byte OriginY {
			get { return originY; }
		}
		public byte Width {
			get { return (byte)placementGrid.GetLength(0); }
		}
		public byte Height {
			get { return (byte)placementGrid.GetLength(1); }
		}
		public DecorationDataTypes DataType {
			get { return dataType; }
		}

		#endregion

		#region Private Helpers

		private static DecorationTypes GetDecorationTypeFromString(string pocketString) {
			if (pocketString == "DESK") return DecorationTypes.Desk;
			if (pocketString == "CHAIR") return DecorationTypes.Chair;
			if (pocketString == "PLANT") return DecorationTypes.Plant;
			if (pocketString == "ORNAMENT") return DecorationTypes.Ornament;
			if (pocketString == "MAT") return DecorationTypes.Mat;
			if (pocketString == "POSTER") return DecorationTypes.Poster;
			if (pocketString == "DOLL") return DecorationTypes.Doll;
			if (pocketString == "CUSHION") return DecorationTypes.Cushion;
			return DecorationTypes.Unknown;
		}

		private void CompileTypeData(string type) {
			if (type == "SMALL SOLID") {
				dataType = DecorationDataTypes.SmallSolid;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[1, 1] { { SecretBasePlacementTypes.Solid } };
			}
			else if (type == "WIDE DESK") {
				dataType = DecorationDataTypes.WideDesk;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 3] {
					{ SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.MatCenter, SecretBasePlacementTypes.Mat },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "LARGE DESK") {
				dataType = DecorationDataTypes.LargeDesk;
				originX = 0;
				originY = 2;
				placementGrid = new SecretBasePlacementTypes[3, 3] {
					{ SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.MatCenter, SecretBasePlacementTypes.Mat },
					{ SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.MatCenter, SecretBasePlacementTypes.Mat },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "SMALL STEPABLE") {
				dataType = DecorationDataTypes.SmallStepable;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[1, 1] { { SecretBasePlacementTypes.Solid } };
			}
			else if (type == "SMALL STATUE") {
				dataType = DecorationDataTypes.SmallStatue;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 1] {
					{ SecretBasePlacementTypes.Back },
					{ SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "LARGE STATUE") {
				dataType = DecorationDataTypes.LargeStatue;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 2] {
					{ SecretBasePlacementTypes.Back, SecretBasePlacementTypes.Back },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "BRICK") {
				dataType = DecorationDataTypes.Brick;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 1] {
					{ SecretBasePlacementTypes.Mat },
					{ SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "TENT") {
				dataType = DecorationDataTypes.Tent;
				originX = 0;
				originY = 2;
				placementGrid = new SecretBasePlacementTypes[3, 3] {
					{ SecretBasePlacementTypes.BackNoWall, SecretBasePlacementTypes.BackNoWall, SecretBasePlacementTypes.BackNoWall },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "BOARD") {
				dataType = DecorationDataTypes.Board;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 1] {
					{ SecretBasePlacementTypes.Board },
					{ SecretBasePlacementTypes.Board }
				};
			}
			else if (type == "SLIDE") {
				dataType = DecorationDataTypes.Slide;
				originX = 0;
				originY = 3;
				placementGrid = new SecretBasePlacementTypes[4, 2] {
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "TIRE") {
				dataType = DecorationDataTypes.Tire;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 2] {
					{ SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.Mat },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "STAND") {
				dataType = DecorationDataTypes.Stand;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 4] {
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "DOOR") {
				dataType = DecorationDataTypes.Door;
				originX = 0;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 1] {
					{ SecretBasePlacementTypes.Back },
					{ SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "NOTE MAT") {
				dataType = DecorationDataTypes.NoteMat;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[1, 1] { { SecretBasePlacementTypes.Solid } };
			}
			else if (type == "LARGE MAT") {
				dataType = DecorationDataTypes.LargeMat;
				originX = 0;
				originY = 2;
				placementGrid = new SecretBasePlacementTypes[3, 3] {
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.Solid },
					{ SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.MatCenter, SecretBasePlacementTypes.Mat },
					{ SecretBasePlacementTypes.Solid, SecretBasePlacementTypes.Mat, SecretBasePlacementTypes.Solid }
				};
			}
			else if (type == "SMALL POSTER") {
				dataType = DecorationDataTypes.SmallPoster;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[1, 1] { { SecretBasePlacementTypes.Poster } };
			}
			else if (type == "LARGE POSTER") {
				dataType = DecorationDataTypes.LargePoster;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[1, 2] {
					{ SecretBasePlacementTypes.Poster, SecretBasePlacementTypes.Poster }
				};
			}
			else if (type == "SMALL DOLL") {
				dataType = DecorationDataTypes.SmallDoll;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[1, 1] { { SecretBasePlacementTypes.Doll } };
			}
			else if (type == "LARGE DOLL") {
				dataType = DecorationDataTypes.LargeDoll;
				originX = 1;
				originY = 1;
				placementGrid = new SecretBasePlacementTypes[2, 3] {
					{ SecretBasePlacementTypes.Empty, SecretBasePlacementTypes.DollBack, SecretBasePlacementTypes.Empty },
					{ SecretBasePlacementTypes.DollSide, SecretBasePlacementTypes.Doll, SecretBasePlacementTypes.DollSide }
				};
			}
			else if (type == "UNKNOWN") {
				dataType = DecorationDataTypes.Unknown;
				originX = 0;
				originY = 0;
				placementGrid = new SecretBasePlacementTypes[0, 0];
			}

			SecretBasePlacementTypes[,] finalPlacementGrid = new SecretBasePlacementTypes[placementGrid.GetLength(1), placementGrid.GetLength(0)];
			for (int x = 0; x < placementGrid.GetLength(1); x++) {
				for (int y = 0; y < placementGrid.GetLength(0); y++) {
					finalPlacementGrid[x, y] = placementGrid[y, x];
				}
			}
			placementGrid = finalPlacementGrid;
		}

		#endregion
	}
}
