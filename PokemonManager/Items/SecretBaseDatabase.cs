using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {

	public struct SecretBaseRoomID {
		public SecretBaseRoomTypes Type { get; set; }
		public SecretBaseRoomLayouts Layout { get; set; }
	}

	public static class SecretBaseDatabase {

		private static Dictionary<byte, LocationData> locationMap;
		private static List<LocationData> locationList;

		private static Dictionary<SecretBaseRoomID, RoomData> roomMap;
		private static List<RoomData> roomList;

		private static Dictionary<byte, RouteData> routeMap;
		private static List<RouteData> routeList;

		public static void Initialize() {

			SecretBaseDatabase.locationMap = new Dictionary<byte, LocationData>();
			SecretBaseDatabase.locationList = new List<LocationData>();
			SecretBaseDatabase.roomMap = new Dictionary<SecretBaseRoomID, RoomData>();
			SecretBaseDatabase.roomList = new List<RoomData>();
			SecretBaseDatabase.routeMap = new Dictionary<byte, RouteData>();
			SecretBaseDatabase.routeList = new List<RouteData>();

			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			SQLiteConnection connection = new SQLiteConnection("Data Source=SecretBaseDatabase.db");
			connection.Open();

			// Load Locations
			command = new SQLiteCommand("SELECT * FROM Locations", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Locations");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				LocationData location = new LocationData(row);
				locationMap.Add(location.ID, location);
				locationList.Add(location);
			}

			// Load Rooms
			command = new SQLiteCommand("SELECT * FROM Rooms", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Rooms");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				RoomData room = new RoomData(row);
				roomMap.Add(new SecretBaseRoomID { Type = room.Type, Layout = room.Layout }, room);
				roomList.Add(room);
			}

			// Load Routes
			command = new SQLiteCommand("SELECT * FROM Routes", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Routes");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				RouteData route = new RouteData(row);
				routeMap.Add(route.ID, route);
				routeList.Add(route);
			}

			connection.Close();
		}

		public static int NumLocations {
			get { return locationMap.Count; }
		}
		public static int NumRooms {
			get { return roomMap.Count; }
		}
		public static int NumRoutes {
			get { return routeMap.Count; }
		}

		public static LocationData GetLocationFromID(byte id) {
			if (locationMap.ContainsKey(id))
				return locationMap[id];
			return null;
		}
		public static LocationData GetLocationAt(int index) {
			return locationList[index];
		}
		public static RoomData GetRoomFromID(SecretBaseRoomTypes type, SecretBaseRoomLayouts layout) {
			SecretBaseRoomID id = new SecretBaseRoomID { Type = type, Layout = layout };
			if (roomMap.ContainsKey(id))
				return roomMap[id];
			return null;
		}
		public static RoomData GetRoomAt(int index) {
			return roomList[index];
		}
		public static RouteData GetRouteFromID(byte id) {
			if (routeMap.ContainsKey(id))
				return routeMap[id];
			return null;
		}
		public static RouteData GetRouteAt(int index) {
			return routeList[index];
		}
	}
}
