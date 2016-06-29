using PokemonManager.Game;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokemonManager.Items {

	public static class ItemDatabase {

		private static Dictionary<ushort, BitmapSource> gen3ImageMap;
		private static Dictionary<ushort, ItemData> gen3ItemMap;
		private static List<ItemData> gen3ItemList;

		private static Dictionary<PokeblockColors, BitmapSource> pokeblockLargeImageMap;
		private static Dictionary<PokeblockColors, BitmapSource> pokeblockSmallImageMap;

		private static Dictionary<byte, BitmapSource> decorationImageMap;
		private static Dictionary<byte, BitmapSource> decorationFullSizeImageMap;
		private static Dictionary<byte, DecorationData> decorationMap;
		private static List<DecorationData> decorationList;

		private static Dictionary<ItemTypes, string> pocketNamesMap;
		private static Dictionary<DecorationTypes, string> decorationContainerNamesMap;

		private static Dictionary<ushort, string> easyChatMap;
		private static Dictionary<string, ushort> easyChatReverseMap;
		private static List<string> easyChatList;

		public static BitmapSource secretBaseUsedIcon;
		public static BitmapSource bedroomUsedIcon;

		public static void Initialize() {
			ItemDatabase.gen3ImageMap	= new Dictionary<ushort, BitmapSource>();
			ItemDatabase.gen3ItemMap	= new Dictionary<ushort, ItemData>();
			ItemDatabase.gen3ItemList	= new List<ItemData>();

			ItemDatabase.pokeblockLargeImageMap	= new Dictionary<PokeblockColors, BitmapSource>();
			ItemDatabase.pokeblockSmallImageMap	= new Dictionary<PokeblockColors, BitmapSource>();

			ItemDatabase.decorationImageMap			= new Dictionary<byte, BitmapSource>();
			ItemDatabase.decorationFullSizeImageMap	= new Dictionary<byte, BitmapSource>();
			ItemDatabase.decorationMap				= new Dictionary<byte, DecorationData>();
			ItemDatabase.decorationList				= new List<DecorationData>();

			ItemDatabase.easyChatMap		= new Dictionary<ushort, string>();
			ItemDatabase.easyChatReverseMap	= new Dictionary<string, ushort>();
			ItemDatabase.easyChatList		= new List<string>();

			ItemDatabase.pocketNamesMap	= new Dictionary<ItemTypes, string>() {
				{ItemTypes.PC, "PC"},
				{ItemTypes.Items, "Items"},
				{ItemTypes.KeyItems, "Key Items"},
				{ItemTypes.PokeBalls, "Poké Balls"},
				{ItemTypes.TMCase, "TMs & HMs"},
				{ItemTypes.Berries, "Berries"},
				{ItemTypes.CologneCase, "Cologne Case"},
				{ItemTypes.DiscCase, "Disc Case"},
				{ItemTypes.InBattle, "In-Battle Items"},
				{ItemTypes.Valuables, "Valuable Items"},
				{ItemTypes.Hold, "Hold Items"},
				{ItemTypes.Misc, "Other Items"},
				{ItemTypes.Any, "Any"},
				{ItemTypes.TheVoid, "The Void"}
			};
			ItemDatabase.decorationContainerNamesMap	= new Dictionary<DecorationTypes, string>() {
				{DecorationTypes.Desk, "Desks"},
				{DecorationTypes.Chair, "Chairs"},
				{DecorationTypes.Plant, "Plants"},
				{DecorationTypes.Ornament, "Ornaments"},
				{DecorationTypes.Mat, "Mats"},
				{DecorationTypes.Poster, "Posters"},
				{DecorationTypes.Doll, "Dolls"},
				{DecorationTypes.Cushion, "Cushions"}
			};

			secretBaseUsedIcon = ResourceDatabase.GetImageFromName("DecorationSecretBase");
			bedroomUsedIcon = ResourceDatabase.GetImageFromName("DecorationBedroom");

			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			SQLiteConnection connection = new SQLiteConnection("Data Source=ItemDatabase.db");
			connection.Open();

			// Load Gen3 Item Data
			command = new SQLiteCommand("SELECT * FROM Items", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Items");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ItemData item = new ItemData(row, Generations.Gen3);
				gen3ItemMap.Add(item.ID, item);
				gen3ItemList.Add(item);
			}

			// Load Gen3 Item Images
			command = new SQLiteCommand("SELECT * FROM ItemImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Images");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ushort id = (ushort)(long)row["ID"];
				BitmapSource image = LoadImage((byte[])row["Image"]);
				gen3ImageMap.Add(id, image);
			}

			// Load Gen3 Decoration Data
			command = new SQLiteCommand("SELECT * FROM Decorations", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Decorations");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				DecorationData decoration = new DecorationData(row);
				decorationMap.Add(decoration.ID, decoration);
				decorationList.Add(decoration);
			}

			// Load Gen3 Decoration Images
			command = new SQLiteCommand("SELECT * FROM DecorationImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("DecorationImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				byte id = (byte)(long)row["ID"];
				decorationImageMap.Add(id, LoadImage((byte[])row["Image"]));
				decorationFullSizeImageMap.Add(id, LoadImage((byte[])row["ImageFullSize"]));
			}

			// Load Gen3 Pokeblock Small Images
			command = new SQLiteCommand("SELECT * FROM PokeblockImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("PokeblockImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				PokeblockColors id = (PokeblockColors)(long)row["ID"];
				pokeblockSmallImageMap.Add(id, LoadImage((byte[])row["ImageSmall"]));
				pokeblockLargeImageMap.Add(id, LoadImage((byte[])row["ImageLarge"]));
			}
			// Load Gen3 Pokeblock Large Images
			/*command = new SQLiteCommand("SELECT * FROM PokeblockLargeImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("PokeblockLargeImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				PokeblockColors id = (PokeblockColors)(long)row["ID"];
				BitmapImage image = LoadImage((byte[])row["Data"]);
				pokeblockLargeImageMap.Add(id, image);
			}*/


			connection.Close();
		}

		public static void InitializeEasyChat() {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			SQLiteConnection connection = new SQLiteConnection("Data Source=ItemDatabase.db");
			connection.Open();

			// Load Gen3 Item Data
			command = new SQLiteCommand("SELECT * FROM EasyChat", connection);
			reader = command.ExecuteReader();
			table = new DataTable("EasyChat");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ushort id = (ushort)(long)row["ID"];
				string word = row["Word"] as string;
				easyChatMap.Add(id, word);
				easyChatReverseMap.Add(word, id);
				easyChatList.Add(word);
			}

			// Add Pokemon
			for (ushort i = 1; i <= 386; i++) {
				string name = PokemonDatabase.GetPokemonFromDexID(i).Name;
				ushort speciesID = PokemonDatabase.GetPokemonFromDexID(i).ID;
				easyChatMap.Add(speciesID, name);
				easyChatReverseMap.Add(name, speciesID);
				easyChatMap.Add((ushort)(0x2A00 | speciesID), name);
				easyChatList.Add(name);
			}

			// Add Moves
			for (ushort i = 1; i <= 354; i++) {
				string name = PokemonDatabase.GetMoveFromID(i).Name;
				easyChatMap.Add((ushort)(0x2600 | i), name);
				// There one duplicate word, Psychic the type and Psychic the move. Ignore them
				if (!easyChatReverseMap.ContainsKey(name))
					easyChatReverseMap.Add(name, (ushort)(0x2600 | i));
				easyChatList.Add(name);
			}

			easyChatList.Sort((word1, word2) => string.Compare(word1, word2, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase));

			connection.Close();
		}

		private static BitmapSource LoadImage(byte[] imageData) {
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

			FormatConvertedBitmap formatImage = new FormatConvertedBitmap();
			formatImage.BeginInit();
			formatImage.Source = image;
			formatImage.DestinationFormat = PixelFormats.Bgra32;
			formatImage.EndInit();
			return formatImage;
		}

		public static string GetEasyChatFromID(ushort id) {
			if (easyChatMap.ContainsKey(id))
				return easyChatMap[id];
			return null;
		}
		public static string GetEasyChatFromGroup(EasyChatGroups group, ushort code) {
			return GetEasyChatFromID((ushort)(((ushort)group << 9) | code));
		}

		public static string[] GetEasyChatSearchResults(string search) {
			List<string> results = new List<string>();
			foreach (string word in easyChatList) {
				if (search.Length <= word.Length) {
					if (string.Compare(search, word.Substring(0, search.Length), CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0)
						results.Add(word);
				}
			}
			return results.ToArray();
		}

		public static ushort GetIDFromEasyChat(string word) {
			if (easyChatReverseMap.ContainsKey(word))
				return easyChatReverseMap[word];
			return 0xFFFF;
		}

		public static BitmapSource GetPokeblockImageFromColor(PokeblockColors color, bool large) {
			if (large) {
				if (pokeblockLargeImageMap.ContainsKey(color))
					return pokeblockLargeImageMap[color];
			}
			else {
				if (pokeblockSmallImageMap.ContainsKey(color))
					return pokeblockSmallImageMap[color];
			}
			return null;
		}

		public static BitmapSource GetDecorationImageFromID(byte id) {
			if (decorationImageMap.ContainsKey(id))
				return decorationImageMap[id];
			return null;
		}
		public static BitmapSource GetDecorationFullSizeImageFromID(byte id) {
			if (decorationFullSizeImageMap.ContainsKey(id))
				return decorationFullSizeImageMap[id];
			return null;
		}

		public static BitmapSource GetItemImageFromID(ushort id) {
			if (gen3ImageMap.ContainsKey(id))
				return gen3ImageMap[id];
			return null;
		}

		public static DecorationData GetDecorationFromID(byte id) {
			if (decorationMap.ContainsKey(id))
				return decorationMap[id];
			return null;
		}
		public static bool DecorationIDExists(byte id) {
			return decorationMap.ContainsKey(id);
		}
		public static DecorationData GetDecorationAt(int index) {
			if (index >= 0 && index < decorationList.Count)
				return decorationList[index];
			return null;
		}
		public static DecorationData GetDecorationTypeAt(int index, DecorationTypes decorationType) {
			for (int i = 1; i < gen3ItemList.Count; i++) {
				if (decorationList[i].DecorationType == decorationType) {
					if (index == 0)
						return decorationList[i];
					index--;
				}
			}
			return decorationList[0];
		}

		public static ItemData GetItemFromID(ushort id) {
			if (gen3ItemMap.ContainsKey(id))
				return gen3ItemMap[id];
			return null;
		}

		public static bool ContainsItemWithID(ushort id) {
			return gen3ItemMap.ContainsKey(id);
		}

		public static ItemData GetItemAt(int index) {
			if (index >= 0 && index < gen3ItemList.Count)
				return gen3ItemList[index];
			return null;
		}

		public static ItemData GetItemTypeAt(int index, GameTypes game, ItemTypes pocket, bool mustBeObtainable) {
			for (int i = 1; i < gen3ItemList.Count; i++) {
				if ((game == GameTypes.Any || ((int)gen3ItemList[i].Exclusives & (1 << ((int)game - 1))) != 0) &&
				(gen3ItemList[i].PocketType == pocket || pocket == ItemTypes.PC) && (gen3ItemList[i].IsObtainable || !mustBeObtainable)) {
					if (index == 0)
						return gen3ItemList[i];
					index--;
				}
			}
			return null;
		}

		public static ItemData GetItemFromName(string name) {
			name = name.ToLower();
			for (int i = 1; i < gen3ItemList.Count; i++) {
				if (gen3ItemList[i].Name.ToLower() == name)
					return gen3ItemList[i];
			}
			return null;
		}

		public static string GetPocketName(ItemTypes pocketType) {
			if (pocketNamesMap.ContainsKey(pocketType))
				return pocketNamesMap[pocketType];
			return "Unknown";
		}

		public static string GetDecorationContainerName(DecorationTypes decorationType) {
			if (decorationContainerNamesMap.ContainsKey(decorationType))
				return decorationContainerNamesMap[decorationType];
			return "Unknown";
		}
	}
}
