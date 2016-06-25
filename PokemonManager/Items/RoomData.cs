using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.Items {
	public class RoomData {
		
		private byte id;
		private SecretBaseRoomTypes type;
		private SecretBaseRoomLayouts layout;
		private byte width;
		private byte height;
		private byte trainerX;
		private byte trainerY;
		private BitmapSource image;
		private BitmapSource backgroundImage;

		private SecretBasePlacementTypes[,] placementGrid;

		public RoomData(DataRow row) {
			this.id					= (byte)(long)row["ID"];
			this.type				= GetTypeFromString(row["Type"] as string);
			this.layout				= GetLayoutFromString(row["Layout"] as string);
			this.width				= (byte)(long)row["Width"];
			this.height				= (byte)(long)row["Height"];
			this.trainerX			= (byte)(long)row["TrainerX"];
			this.trainerY			= (byte)(long)row["TrainerY"];
			this.image				= LoadImage(row["Image"] as byte[]);
			this.backgroundImage	= LoadImage(row["BackgroundImage"] as byte[]);

			CompilePlacementGrid(row["PlacementGrid"] as string);
		}

		public byte ID {
			get { return id; }
		}
		public SecretBaseRoomTypes Type {
			get { return type; }
		}
		public SecretBaseRoomLayouts Layout {
			get { return layout; }
		}
		public byte Width {
			get { return width; }
		}
		public byte Height {
			get { return height; }
		}
		public byte TrainerX {
			get { return trainerX; }
		}
		public byte TrainerY {
			get { return trainerY; }
		}
		public BitmapSource Image {
			get { return image; }
		}
		public BitmapSource BackgroundImage {
			get { return backgroundImage; }
		}
		public SecretBasePlacementTypes[,] PlacementGrid {
			get { return placementGrid; }
		}

		private static SecretBaseRoomTypes GetTypeFromString(string text) {
			if (text == "RED CAVE") return SecretBaseRoomTypes.RedCave;
			if (text == "BROWN CAVE") return SecretBaseRoomTypes.BrownCave;
			if (text == "BLUE CAVE") return SecretBaseRoomTypes.BlueCave;
			if (text == "YELLOW CAVE") return SecretBaseRoomTypes.YellowCave;
			if (text == "TREE") return SecretBaseRoomTypes.Tree;
			if (text == "SHRUB") return SecretBaseRoomTypes.Shrub;
			throw new Exception("Invalid Secret Base Location Type");
		}
		private static SecretBaseRoomLayouts GetLayoutFromString(string text) {
			return (SecretBaseRoomLayouts)Enum.Parse(typeof(SecretBaseRoomLayouts), text);
		}
		private static BitmapImage LoadImage(byte[] imageData) {
			if (imageData == null || imageData.Length == 0) return null;
			var image = new BitmapImage();
			using (var mem = new MemoryStream(imageData)) {
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();
			return image;
		}

		private void CompilePlacementGrid(string grid) {
			placementGrid = new SecretBasePlacementTypes[width, height];

			grid = grid.Replace("\n", "").Replace("\r", "");

			if (grid.Length != width * height)
				throw new Exception("Secret Base Room placement grid incorrect length");

			for (int i = 0; i < grid.Length; i++) {
				SecretBasePlacementTypes type = SecretBasePlacementTypes.Blocked;
				if (grid[i] == 'B') type = SecretBasePlacementTypes.Blocked;
				else if (grid[i] == 'F') type = SecretBasePlacementTypes.Floor;
				else if (grid[i] == 'W') type = SecretBasePlacementTypes.Wall;
				else if (grid[i] == 'R') type = SecretBasePlacementTypes.Rock;
				else if (grid[i] == 'H') type = SecretBasePlacementTypes.Hole;
				else if (grid[i] == 'S') type = SecretBasePlacementTypes.Reserved;
				else throw new Exception("Invalid placement grid letter");

				placementGrid[i % width, i / width] = type;
			}
		}
	}
}
