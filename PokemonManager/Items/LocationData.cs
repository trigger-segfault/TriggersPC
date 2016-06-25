using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.Items {
	public class LocationData {

		private byte id;
		private int order;
		private byte route;
		private byte mapX;
		private byte mapY;
		private Sides side;
		private SecretBaseRoomTypes type;
		private SecretBaseRoomLayouts layout;
		private string requirements;
		private BitmapSource image;

		public LocationData(DataRow row) {
			this.id				= (byte)(long)row["ID"];
			this.order			= (int)(long)row["Order"];
			this.route			= (byte)(long)row["Route"];
			this.mapX			= (byte)((long)row["MapX"] + 1); // At first I made the map position 1 off to center on the location but I'm thinking I'll just highlight the grid location
			this.mapY			= (byte)((long)row["MapY"] + 1);
			this.side			= GetSideFromString(row["Side"] as string);
			this.type			= GetTypeFromString(row["Type"] as string);
			this.layout			= GetLayoutFromString(row["Layout"] as string);
			this.requirements	= row["Requirements"] as string;

			this.image			= LoadImage((byte[])row["Image"]);
		}

		public byte ID {
			get { return id; }
		}
		public int Order {
			get { return order; }
		}
		public byte Route {
			get { return route; }
		}
		public byte MapX {
			get { return mapX; }
		}
		public byte MapY {
			get { return mapY; }
		}
		public Sides Side {
			get { return side; }
		}
		public SecretBaseRoomTypes Type {
			get { return type; }
		}
		public SecretBaseRoomLayouts Layout {
			get { return layout; }
		}
		public string Requirements {
			get { return requirements; }
		}
		public BitmapSource Image {
			get { return image; }
		}
		public RoomData RoomData {
			get { return SecretBaseDatabase.GetRoomFromID(type, layout); }
		}
		public RouteData RouteData {
			get { return SecretBaseDatabase.GetRouteFromID(route); }
		}


		private static Sides GetSideFromString(string text) {
			if (text == null) return Sides.None;
			if (text == "LEFT") return Sides.Left;
			if (text == "RIGHT") return Sides.Right;
			throw new Exception("Invalid Secret Base Location Side");
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
	}
}
