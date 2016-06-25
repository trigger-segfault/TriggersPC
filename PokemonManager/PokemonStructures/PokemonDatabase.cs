using PokemonManager.Game;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Int32Point = System.Drawing.Point;
using Color = System.Windows.Media.Color;
using System.Windows.Media;
using PokemonManager.Items;
using System.Globalization;
using System.Threading;

namespace PokemonManager.PokemonStructures {
	public class Gen3PokemonImageTypes {
		public BitmapImage Image;
		public BitmapImage ShinyImage;
		public BitmapImage FRLGImage;
		public BitmapImage FRLGShinyImage;
		public BitmapImage BoxImage;
		public BitmapImage NewBoxImage;
		public BitmapImage NewBoxShinyImage;
		public BitmapSource CustomImage;
		public BitmapSource CustomShinyImage;
	}
	public class XDShadowPokemonData {
		public ushort DexID;
		public uint HeartGauge;
		public ushort[] ShadowMovesIDs;
	}
	public static class PokemonDatabase {

		private static Dictionary<byte, NatureData> natureMap;
		private static List<NatureData> natureList;

		private static Dictionary<byte, AbilityData> abilityMap;
		private static List<AbilityData> abilityList;

		private static Dictionary<ushort, MoveData> gen3MoveMap;
		private static List<MoveData> gen3MoveList;

		private static Dictionary<ushort, PokemonData> gen3PokemonMap;
		private static Dictionary<ushort, PokemonData> gen3PokemonDexMap;
		private static List<PokemonData> gen3PokemonDexList;

		private static Dictionary<ushort, Gen3PokemonImageTypes> gen3PokemonImages;
		private static Dictionary<DexFormID, Gen3PokemonImageTypes> gen3PokemonFormImages;

		private static Dictionary<ExperienceGroups, uint[]> experienceTable;

		private static Dictionary<byte, BitmapImage> ballCaughtImages;

		private static Dictionary<ushort, string> metLocationMap;
		private static List<string> metLocationList;

		private static Dictionary<string, BitmapImage> ribbonImages;
		private static Dictionary<string, RibbonData> ribbonMap;

		private static Dictionary<ushort, uint> colosseumHeartGauges;
		private static Dictionary<ushort, XDShadowPokemonData> xdShadowData;

		private static FormatConvertedBitmap writeableSpindaNormal;
		private static FormatConvertedBitmap writeableSpindaShiny;

		private static Dictionary<string, BitmapSource> customWallpapersMap;
		private static List<string> customWallpaperNamesList;

		private static Dictionary<ushort, ushort> machineMoveMap;
		private static Dictionary<ushort, ushort> machineItemMap;

		public static void Initialize() {

			PokemonDatabase.natureMap = new Dictionary<byte, NatureData>();
			PokemonDatabase.natureList = new List<NatureData>();
			PokemonDatabase.abilityMap = new Dictionary<byte, AbilityData>();
			PokemonDatabase.abilityList = new List<AbilityData>();
			PokemonDatabase.gen3MoveMap = new Dictionary<ushort, MoveData>();
			PokemonDatabase.gen3MoveList = new List<MoveData>();
			PokemonDatabase.gen3PokemonMap = new Dictionary<ushort, PokemonData>();
			PokemonDatabase.gen3PokemonDexMap = new Dictionary<ushort, PokemonData>();
			PokemonDatabase.gen3PokemonDexList = new List<PokemonData>();

			PokemonDatabase.gen3PokemonImages = new Dictionary<ushort, Gen3PokemonImageTypes>();
			PokemonDatabase.gen3PokemonFormImages = new Dictionary<DexFormID, Gen3PokemonImageTypes>();

			PokemonDatabase.ballCaughtImages = new Dictionary<byte, BitmapImage>();

			PokemonDatabase.experienceTable = new Dictionary<ExperienceGroups, uint[]>();
			PokemonDatabase.experienceTable.Add(ExperienceGroups.Fast, new uint[100]);
			PokemonDatabase.experienceTable.Add(ExperienceGroups.MediumFast, new uint[100]);
			PokemonDatabase.experienceTable.Add(ExperienceGroups.MediumSlow, new uint[100]);
			PokemonDatabase.experienceTable.Add(ExperienceGroups.Slow, new uint[100]);
			PokemonDatabase.experienceTable.Add(ExperienceGroups.Fluctuating, new uint[100]);
			PokemonDatabase.experienceTable.Add(ExperienceGroups.Erratic, new uint[100]);

			PokemonDatabase.metLocationMap = new Dictionary<ushort, string>();
			PokemonDatabase.metLocationList = new List<string>();

			PokemonDatabase.ribbonImages = new Dictionary<string, BitmapImage>();
			PokemonDatabase.ribbonMap = new Dictionary<string, RibbonData>();

			PokemonDatabase.colosseumHeartGauges = new Dictionary<ushort, uint>();
			PokemonDatabase.xdShadowData = new Dictionary<ushort, XDShadowPokemonData>();

			PokemonDatabase.customWallpapersMap = new Dictionary<string, BitmapSource>();
			PokemonDatabase.customWallpaperNamesList = new List<string>();

			PokemonDatabase.machineMoveMap = new Dictionary<ushort, ushort>();
			PokemonDatabase.machineItemMap = new Dictionary<ushort, ushort>();

			SQLiteConnection connection = new SQLiteConnection("Data Source=PokemonDatabase.db");
			connection.Open();

			LoadNatures(connection);
			LoadAbilities(connection);
			LoadMoves(connection);
			LoadPokemon(connection);
			LoadEvolutions(connection);
			LoadForms(connection);
			LoadExperienceTable(connection);
			LoadRibbons(connection);
			LoadShadowPokemon(connection);
			LoadLearnsets(connection);
			LoadImages(connection);

			connection.Close();

			LoadMachines();
			
			ReloadCustomPokemonSprites();
			ReloadCustomWallpapers();
		}

		private static void LoadNatures(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Gen3 Nature Data
			command = new SQLiteCommand("SELECT * FROM Natures", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Natures");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				NatureData nature = new NatureData(row);
				natureMap.Add(nature.ID, nature);
				natureList.Add(nature);
			}
		}
		private static void LoadAbilities(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Gen3 Ability Data
			command = new SQLiteCommand("SELECT * FROM Abilities", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Abilities");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				AbilityData ability = new AbilityData(row);
				abilityMap.Add(ability.ID, ability);
				abilityList.Add(ability);
			}
		}
		private static void LoadMoves(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Gen3 Move Data
			command = new SQLiteCommand("SELECT * FROM Moves", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Moves");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				MoveData move = new MoveData(row);
				gen3MoveMap.Add(move.ID, move);
				gen3MoveList.Add(move);
			}
		}
		private static void LoadPokemon(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Gen3 Pokemon Data
			command = new SQLiteCommand("SELECT * FROM Pokemon", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Pokemon");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				if ((int)(long)row["ID"] != 412) {
					PokemonData pokemon = new PokemonData(row);
					gen3PokemonMap.Add(pokemon.ID, pokemon);
					gen3PokemonDexMap.Add(pokemon.DexID, pokemon);
					gen3PokemonDexList.Add(pokemon);
				}
			}
			// Sort the Dex ID list
			gen3PokemonDexList = gen3PokemonDexList.OrderBy(o => o.DexID).ToList();
		}
		private static void LoadEvolutions(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Pokemon Evolutions, we need to read it again since we didn't have all the pokemon names before
			command = new SQLiteCommand("SELECT * FROM Pokemon", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Pokemon");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ushort id = (ushort)(long)row["ID"];
				if (id != 412) {
					PokemonData pokemon = GetPokemonFromID(id);
					string evoScript = row["Evolutions"] as string;
					if (evoScript != null) {
						string[] evoStrings = evoScript.Split(new string[] { ", ", "," }, StringSplitOptions.None);
						foreach (string evoString in evoStrings) {
							EvolutionData evolution = new EvolutionData(evoString);
							pokemon.AddEvolution(evolution);
						}
					}
				}
			}
		}
		private static void LoadForms(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Unown Forms
			command = new SQLiteCommand("SELECT * FROM UnownForms", connection);
			reader = command.ExecuteReader();
			table = new DataTable("UnownForms");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				PokemonFormData pokemonForm = new PokemonFormData(row);
				gen3PokemonDexMap[201].AddForm(pokemonForm);
			}

			// Load Deoxys Forms
			command = new SQLiteCommand("SELECT * FROM DeoxysForms", connection);
			reader = command.ExecuteReader();
			table = new DataTable("DeoxysForms");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				PokemonFormData pokemonForm = new PokemonFormData(row);
				gen3PokemonDexMap[386].AddForm(pokemonForm);
			}
		}
		private static void LoadExperienceTable(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Experience Table
			command = new SQLiteCommand("SELECT * FROM ExperienceTable", connection);
			reader = command.ExecuteReader();
			table = new DataTable("ExperienceTable");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ushort id = (ushort)(long)row["ID"];
				ExperienceGroups group = GetExperienceGroupFromString(row["ExperienceGroup"] as string);
				byte level = (byte)(long)row["Level"];
				uint totalExperience = (uint)(long)row["TotalExperience"];
				experienceTable[group][level - 1] = totalExperience;
			}
		}
		private static void LoadRibbons(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Ribbon Data
			command = new SQLiteCommand("SELECT * FROM Ribbons", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Ribbons");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				RibbonData ribbon = new RibbonData(row);
				ribbonMap.Add(ribbon.ID, ribbon);
			}
		}
		private static void LoadShadowPokemon(SQLiteConnection connection) {
			LoadColosseumShadowPokemon(connection);
			LoadXDShadowPokemon(connection);
		}
		private static void LoadColosseumShadowPokemon(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Colosseum Heart Guage Data
			command = new SQLiteCommand("SELECT * FROM ColosseumShadowPokemon", connection);
			reader = command.ExecuteReader();
			table = new DataTable("ColosseumShadowPokemon");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				byte id = (byte)(long)row["ID"];
				uint heartGauge = (uint)(long)row["HeartGauge"];
				colosseumHeartGauges.Add(id, heartGauge);
			}
		}
		private static void LoadXDShadowPokemon(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load XD Shadow Pokemon Data
			command = new SQLiteCommand("SELECT * FROM XDShadowPokemon", connection);
			reader = command.ExecuteReader();
			table = new DataTable("XDShadowPokemon");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				byte id = (byte)(long)row["ID"];
				XDShadowPokemonData shadowData = new XDShadowPokemonData();
				//shadowData.DexID = (ushort)(long)row["DexID"];
				shadowData.HeartGauge = (uint)(long)row["HeartGauge"];
				string pokemonName = row["PokemonName"] as string;
				PokemonData pokemonData = GetPokemonFromName(pokemonName);
				if (pokemonData == null)
					throw new Exception("Invalid Pokemon Name while loading shadow Pokemon. Invalid Text: " + pokemonName);
				shadowData.DexID = pokemonData.DexID;
				List<ushort> moves = new List<ushort>();
				for (int i = 0; i < 4; i++) {
					string moveName = row["ShadowMove" + (i + 1)] as string;
					if (moveName != null) {
						if (moveName == "RUSH") {
							moves.Add(357);
						}
						else {
							MoveData moveData = GetMoveFromName("SHADOW " + moveName);
							if (moveData == null)
								throw new Exception("Invalid Shadow Move Name while loading shadow Pokemon. Invalid Text: " + moveName);
							moves.Add(moveData.ID);
						}
					}
				}
				shadowData.ShadowMovesIDs = moves.ToArray();
				xdShadowData.Add(id, shadowData);
			}
		}
		private static void LoadLearnsets(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			string[] moveLines = ResourceDatabase.GetTextFromName("LearnableMovesDatabase").Split('\n', '\r');
			bool deoxysForms = false;
			PokemonData pokemonData = null;
			PokemonFormData pokemonFormData = null;
			LearnableMoveTypes learnType = LearnableMoveTypes.None;
			int lineIndex = -1;
			SkipToNextValidLine(ref lineIndex, moveLines);
			while (lineIndex < moveLines.Length) {
				string line = moveLines[lineIndex];
				if (line.StartsWith("POKEMON: ")) {
					if (deoxysForms) {
						string name = line.Substring(9).ToUpper();
						if (name == "DEOXYS") pokemonFormData = pokemonData.GetForm(0);
						else if (name == "ATTACK DEOXYS") pokemonFormData = pokemonData.GetForm(1);
						else if (name == "DEFENSE DEOXYS") pokemonFormData = pokemonData.GetForm(2);
						else if (name == "SPEED DEOXYS") pokemonFormData = pokemonData.GetForm(3);
						else throw new Exception("PokemonFormData in Move Parsing equals null. Invalid Text: " + line.Substring(9));
					}
					else {
						pokemonData = GetPokemonFromName(line.Substring(9));
						if (pokemonData == null)
							throw new Exception("PokemonData in Move Parsing equals null. Invalid Text: " + line.Substring(9));
					}
				}
				else if (line.StartsWith("TYPE: ")) {
					learnType = (LearnableMoveTypes)Enum.Parse(typeof(LearnableMoveTypes), line.Substring(6), true);
				}
				else if (learnType != LearnableMoveTypes.None) {
					string[] parameters = line.Split(new string[] { ", " }, StringSplitOptions.None);
					if (GetMoveFromName(parameters[0]) != null) {
						ushort moveID = GetMoveFromName(parameters[0]).ID;
						if (learnType == LearnableMoveTypes.Level) {
							byte level = byte.Parse(parameters[1]);
							if (deoxysForms)
								pokemonFormData.AddLearnableMove(LearnableMove.CreateLevelUpMove(moveID, level));
							else
								pokemonData.AddLearnableMove(LearnableMove.CreateLevelUpMove(moveID, level));
						}
						else if (learnType == LearnableMoveTypes.Purification) {
							GameTypes gameType = (GameTypes)Enum.Parse(typeof(GameTypes), parameters[1], true);
							byte level = byte.Parse(parameters[2]);
							if (deoxysForms)
								pokemonFormData.AddLearnableMove(LearnableMove.CreatePurificationMove(moveID, level, gameType));
							else
								pokemonData.AddLearnableMove(LearnableMove.CreatePurificationMove(moveID, level, gameType));
						}
						else if (learnType == LearnableMoveTypes.Event) {
							string description = line.Substring(line.IndexOf(", "));
							if (deoxysForms)
								pokemonFormData.AddLearnableMove(LearnableMove.CreateEventMove(moveID, description));
							else
								pokemonData.AddLearnableMove(LearnableMove.CreateEventMove(moveID, description));
						}
						else {
							if (deoxysForms)
								pokemonFormData.AddLearnableMove(LearnableMove.CreateLearnableMove(moveID, learnType));
							else
								pokemonData.AddLearnableMove(LearnableMove.CreateLearnableMove(moveID, learnType));
						}
					}
					else {
						throw new Exception("MoveData in Move Parsing equals null. Invalid Text: " + parameters[0]);
					}
				}
				else {
					throw new Exception("No Learn type selected in Move Parsing");
				}

				SkipToNextValidLine(ref lineIndex, moveLines);
				if (lineIndex >= moveLines.Length && !deoxysForms) {
					lineIndex = -1;
					deoxysForms = true;
					moveLines = ResourceDatabase.GetTextFromName("LearnableMovesDeoxys").Split('\n', '\r');
					SkipToNextValidLine(ref lineIndex, moveLines);
				}
			}
		}
		private static void LoadImages(SQLiteConnection connection) {
			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;

			// Load Ball Caught Images
			command = new SQLiteCommand("SELECT * FROM BallImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("BallImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				byte id = (byte)(long)row["ID"];
				BitmapImage image = LoadImage((byte[])row["Image"]);
				ballCaughtImages.Add(id, image);
			}

			// Load Pokemon Images
			command = new SQLiteCommand("SELECT * FROM PokemonImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("PokemonImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ushort id = (ushort)(long)row["DexID"];
				Gen3PokemonImageTypes pokemonImageTypes = new Gen3PokemonImageTypes();
				pokemonImageTypes.Image = LoadImage(row["Image"] as byte[]);
				pokemonImageTypes.ShinyImage = LoadImage(row["ShinyImage"] as byte[]);
				pokemonImageTypes.FRLGImage = LoadImage(row["FRLGImage"] as byte[]);
				pokemonImageTypes.FRLGShinyImage = LoadImage(row["FRLGShinyImage"] as byte[]);
				pokemonImageTypes.BoxImage = LoadImage(row["BoxImage"] as byte[]);
				pokemonImageTypes.NewBoxImage = LoadImage(row["NewBoxImage"] as byte[]);
				pokemonImageTypes.NewBoxShinyImage = LoadImage(row["NewBoxShinyImage"] as byte[]);
				gen3PokemonImages.Add(id, pokemonImageTypes);

				if (id == 327) {
					writeableSpindaNormal = new FormatConvertedBitmap();
					writeableSpindaNormal.BeginInit();
					writeableSpindaNormal.Source = pokemonImageTypes.Image;
					writeableSpindaNormal.DestinationFormat = PixelFormats.Bgra32;
					writeableSpindaNormal.EndInit();

					writeableSpindaShiny = new FormatConvertedBitmap();
					writeableSpindaShiny.BeginInit();
					writeableSpindaShiny.Source = pokemonImageTypes.ShinyImage;
					writeableSpindaShiny.DestinationFormat = PixelFormats.Bgra32;
					writeableSpindaShiny.EndInit();
				}
			}

			// Load Unown Form Images
			command = new SQLiteCommand("SELECT * FROM UnownFormImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("UnownFormImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				byte id = (byte)(long)row["ID"];
				DexFormID dexFormID = new DexFormID { DexID = 201, FormID = id };
				Gen3PokemonImageTypes pokemonImageTypes = new Gen3PokemonImageTypes();
				pokemonImageTypes.Image = LoadImage(row["Image"] as byte[]);
				pokemonImageTypes.ShinyImage = LoadImage(row["ShinyImage"] as byte[]);
				pokemonImageTypes.BoxImage = LoadImage(row["BoxImage"] as byte[]);
				pokemonImageTypes.NewBoxImage = LoadImage(row["NewBoxImage"] as byte[]);
				pokemonImageTypes.NewBoxShinyImage = LoadImage(row["NewBoxShinyImage"] as byte[]);
				gen3PokemonFormImages.Add(dexFormID, pokemonImageTypes);
			}

			// Load Deoxys Form Images
			command = new SQLiteCommand("SELECT * FROM DeoxysFormImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("DeoxysFormImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				byte id = (byte)(long)row["ID"];
				DexFormID dexFormID = new DexFormID { DexID = 386, FormID = id };
				Gen3PokemonImageTypes pokemonImageTypes = new Gen3PokemonImageTypes();
				pokemonImageTypes.Image = LoadImage(row["Image"] as byte[]);
				pokemonImageTypes.ShinyImage = LoadImage(row["ShinyImage"] as byte[]);
				pokemonImageTypes.BoxImage = LoadImage(row["BoxImage"] as byte[]);
				pokemonImageTypes.NewBoxImage = LoadImage(row["NewBoxImage"] as byte[]);
				pokemonImageTypes.NewBoxShinyImage = LoadImage(row["NewBoxShinyImage"] as byte[]);
				gen3PokemonFormImages.Add(dexFormID, pokemonImageTypes);
			}

			// Load Ribbon Images
			command = new SQLiteCommand("SELECT * FROM RibbonImages", connection);
			reader = command.ExecuteReader();
			table = new DataTable("RibbonImages");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				string id = row["ID"] as string;
				BitmapImage image = LoadImage((byte[])row["Image"]);
				ribbonImages.Add(id, image);
			}

			// Load Met Locations
			command = new SQLiteCommand("SELECT * FROM MetLocations", connection);
			reader = command.ExecuteReader();
			table = new DataTable("MetLocations");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				ushort id = (ushort)(long)row["ID"];
				string name = row["Name"] as string;
				metLocationMap.Add(id, name);
				metLocationList.Add(name);
			}
		}
		private static void LoadMachines() {
			for (int i = 289; i <= 346; i++) {
				ItemData itemData = ItemDatabase.GetItemFromID((ushort)i);
				ushort moveID = GetMoveFromName(itemData.Name.Substring(5)).ID;
				machineMoveMap.Add((ushort)i, moveID);
				machineItemMap.Add(moveID, (ushort)i);
			}
		}

		public static void SkipToNextValidLine(ref int lineIndex, string[] moveLines) {
			lineIndex++;
			while (lineIndex < moveLines.Length) {
				if (moveLines[lineIndex].Length > 0) {
					char start = moveLines[lineIndex][0];
					if (start != ' ' && start != '\r' && start != '\n' && start != '\t' && start != '-' && start != '=' && start != '(')
						return;
				}
				lineIndex++;
			}
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
		enum SpindaSpots {
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight
		}
		enum SpindaBaseColors : uint {
			Light = 0xFFFFE7AD,
			Normal = 0xFFE7CE9C,
			Shaded = 0xFFCEB57B
		}
		enum SpindaRegularSpotColors : uint {
			Light = 0xFFE0824B,
			Normal = 0xFFE06735,
			Shaded = 0xFFB1501E
		}
		enum SpindaShinySpotColors : uint {
			Light = 0xFFB0C058,
			Normal = 0xFF90A038,
			Shaded = 0xFF708018
		}
		static Color GetSpindaColor(SpindaBaseColors spindaColor) {
			if (spindaColor == SpindaBaseColors.Light)
				return Color.FromRgb(255, 231, 173);
			if (spindaColor == SpindaBaseColors.Normal)
				return Color.FromRgb(231, 206, 156);
			return Color.FromRgb(206, 181, 123);
		}
		static Color GetSpindaColor(SpindaRegularSpotColors spindaColor) {
			if (spindaColor == SpindaRegularSpotColors.Light)
				return Color.FromRgb(224, 130, 75);
			if (spindaColor == SpindaRegularSpotColors.Normal)
				return Color.FromRgb(224, 103, 53);
			return Color.FromRgb(177, 80, 30);
		}
		static Color GetSpindaColor(SpindaShinySpotColors spindaColor) {
			if (spindaColor == SpindaShinySpotColors.Light)
				return Color.FromRgb(176, 192, 88);
			if (spindaColor == SpindaShinySpotColors.Normal)
				return Color.FromRgb(144, 160, 56);
			return Color.FromRgb(112, 128, 24);
		}
		static Color GetSinglePixel(BitmapImage source, int x, int y) {
			int stride = (source.Format.BitsPerPixel + 7) / 8;
			byte[] pixels = new byte[stride];
			source.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, stride, 0);
			return Color.FromArgb(pixels[3], pixels[0], pixels[1], pixels[2]);
		}
		public static unsafe BitmapSource GetSpindaSprite(uint PID, bool IsShiny = false) {
			Int32Point TopLeftOrigin		= new Int32Point(8, 6);
			Int32Point TopRightOrigin		= new Int32Point(32, 7);
			Int32Point BottomLeftOrigin		= new Int32Point(14, 24);
			Int32Point BottomRightOrigin	= new Int32Point(26, 25);
			Int32Point[] SpotOrigins		= { TopLeftOrigin, TopRightOrigin, BottomLeftOrigin, BottomRightOrigin };

			Int32Point TopLeftOffsets		= new Int32Point((int)(PID & 0xf),       (int)(PID >>  4 & 0xf));
			Int32Point TopRightOffsets		= new Int32Point((int)(PID >>  8 & 0xf), (int)(PID >> 12 & 0xf));
			Int32Point BottomLeftOffsets	= new Int32Point((int)(PID >> 16 & 0xf), (int)(PID >> 20 & 0xf));
			Int32Point BottomRightOffsets	= new Int32Point((int)(PID >> 24 & 0xf), (int)(PID >> 28 & 0xf));
			Int32Point[] SpotOffsets		= { TopLeftOffsets, TopRightOffsets, BottomLeftOffsets, BottomRightOffsets };

			FormatConvertedBitmap BaseSprite= (IsShiny ? writeableSpindaShiny : writeableSpindaNormal);
			BitmapImage TopLeft				= ResourceDatabase.GetImageFromName("SpindaSpotTopLeft");
			BitmapImage TopRight			= ResourceDatabase.GetImageFromName("SpindaSpotTopRight");
			BitmapImage BottomLeft			= ResourceDatabase.GetImageFromName("SpindaSpotBottomLeft");
			BitmapImage BottomRight			= ResourceDatabase.GetImageFromName("SpindaSpotBottomRight");
			BitmapImage[] Spots				= { TopLeft, TopRight, BottomLeft, BottomRight };

			WriteableBitmap bitmap = new WriteableBitmap(BaseSprite);//64, 64, BaseSprite.DpiX, BaseSprite.DpiY, PixelFormats.Bgra32, null);
			bitmap.Lock();
			int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
			byte* scan0 = (byte*)bitmap.BackBuffer.ToPointer();
			uint color;
			int startx;
			int starty;
			for (int i = 0; i < 4; i++) {
				startx = SpotOrigins[i].X + SpotOffsets[i].X;
				starty = SpotOrigins[i].Y + SpotOffsets[i].Y;
				for (int x = 16; x < 53; x++) {
					for (int y = 14; y < 63; y++) {
						color = 0;
						if (((x >= startx) && (y >= starty) && (x < startx + Spots[i].PixelWidth) && (y < starty + Spots[i].PixelHeight)) && (GetSinglePixel(Spots[i], x - startx, y - starty).A != 0)) {
							byte* data = scan0 + y * stride + x * 4;
							if (data[0] != 0) {
								byte[] datab = { data[0], data[1], data[2], data[3] };
								uint SpriteColor = BitConverter.ToUInt32(datab, 0);
								//7D644B
								if (SpriteColor == 0xFFF0E0A8) {
									if (IsShiny)
										color = (uint)(SpindaShinySpotColors.Light);
									else
										color = (uint)(SpindaRegularSpotColors.Light);
								}
								else if (SpriteColor == 0xFFE0D0A0) {
									if (IsShiny)
										color = (uint)(SpindaShinySpotColors.Normal);
									else
										color = (uint)(SpindaRegularSpotColors.Normal);
								}
								else if (SpriteColor == 0xFFC0B080) {
									if (IsShiny)
										color = (uint)(SpindaShinySpotColors.Shaded);
									else
										color = (uint)(SpindaRegularSpotColors.Shaded);
								}
								if (color != 0) {
									byte[] colordata = BitConverter.GetBytes(color);
									data[0] = colordata[0];
									data[1] = colordata[1];
									data[2] = colordata[2];
									data[3] = 0xFF;
								}
							}
						}
					}
				}
			}
			bitmap.Unlock();
			return bitmap;
		}
		public static ushort[] GetMovesLearnedAtLevelRange(IPokemon pokemon, byte startLevel, byte endLevel) {
			List<ushort> moves = new List<ushort>();
			if (pokemon.DexID == 386) {
				for (int i = 0; i < pokemon.PokemonData.GetForm(pokemon.FormID).NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetForm(pokemon.FormID).GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Level && move.Level >= startLevel && move.Level <= endLevel && !PokemonHasMove(pokemon, move.MoveID)) {
						if (!moves.Contains(move.MoveID))
							moves.Add(move.MoveID);
					}
				}
			}
			else {
				for (int i = 0; i < pokemon.PokemonData.NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Level && move.Level >= startLevel && move.Level <= endLevel && !PokemonHasMove(pokemon, move.MoveID)) {
						if (!moves.Contains(move.MoveID))
							moves.Add(move.MoveID);
					}
				}
			}
			return moves.ToArray();
		}

		public static ushort[] GetMovesLearnedAtLevel(IPokemon pokemon) {
			List<ushort> moves = new List<ushort>();
			if (pokemon.DexID == 386) {
				for (int i = 0; i < pokemon.PokemonData.GetForm(pokemon.FormID).NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetForm(pokemon.FormID).GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Level && move.Level == pokemon.Level && !PokemonHasMove(pokemon, move.MoveID)) {
						if (!moves.Contains(move.MoveID))
							moves.Add(move.MoveID);
					}
				}
			}
			else {
				for (int i = 0; i < pokemon.PokemonData.NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Level && move.Level == pokemon.Level && !PokemonHasMove(pokemon, move.MoveID)) {
						if (!moves.Contains(move.MoveID))
							moves.Add(move.MoveID);
					}
				}
			}
			return moves.ToArray();
		}

		public static ushort[] GetRelearnableMoves(IPokemon pokemon) {
			List<ushort> moves = new List<ushort>();
			if (pokemon.DexID == 386) {
				for (int i = 0; i < pokemon.PokemonData.GetForm(pokemon.FormID).NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetForm(pokemon.FormID).GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Level && move.Level <= pokemon.Level && !PokemonHasMove(pokemon, move.MoveID)) {
						if (!moves.Contains(move.MoveID))
							moves.Add(move.MoveID);
					}
				}
			}
			else {
				for (int i = 0; i < pokemon.PokemonData.NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Level && move.Level <= pokemon.Level && !PokemonHasMove(pokemon, move.MoveID)) {
						if (!moves.Contains(move.MoveID))
							moves.Add(move.MoveID);
					}
				}
			}
			return moves.ToArray();
		}

		public static ushort[] GetTeachableMachineMoves(IPokemon pokemon) {
			List<ushort> moves = new List<ushort>();
			if (pokemon.DexID == 386) {
				for (int i = 0; i < pokemon.PokemonData.GetForm(pokemon.FormID).NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetForm(pokemon.FormID).GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Machine && !PokemonHasMove(pokemon, move.MoveID)) {
						moves.Add(move.MoveID);
					}
				}
			}
			else {
				for (int i = 0; i < pokemon.PokemonData.NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Machine && !PokemonHasMove(pokemon, move.MoveID)) {
						moves.Add(move.MoveID);
					}
				}
			}
			return moves.ToArray();
		}

		public static ushort[] GetTeachableMachineMoveItems(IPokemon pokemon) {
			List<ushort> items = new List<ushort>();
			if (pokemon.DexID == 386) {
				for (int i = 0; i < pokemon.PokemonData.GetForm(pokemon.FormID).NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetForm(pokemon.FormID).GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Machine && !PokemonHasMove(pokemon, move.MoveID)) {
						items.Add(machineItemMap[move.MoveID]);
					}
				}
			}
			else {
				for (int i = 0; i < pokemon.PokemonData.NumLearnableMoves; i++) {
					LearnableMove move = pokemon.PokemonData.GetLearnableMove(i);
					if (move.LearnType == LearnableMoveTypes.Machine && !PokemonHasMove(pokemon, move.MoveID)) {
						items.Add(machineItemMap[move.MoveID]);
					}
				}
			}
			return items.ToArray();
		}

		public static bool IsPokemonWithRoamingIVGlitch(IPokemon pokemon) {
			if (pokemon.DexID == 243 || pokemon.DexID == 244 || pokemon.DexID == 245 || pokemon.DexID == 380 || pokemon.DexID == 381) {
				return (pokemon.AttackIV <= 7 && pokemon.DefenseIV == 0 && pokemon.SpAttackIV == 0 && pokemon.SpDefenseIV == 0 && pokemon.SpeedIV == 0) ;
			}
			return false;
		}

		public static string GetEggGroupName(EggGroups group) {
			switch (group) {
			case EggGroups.Water1: return "Water 1";
			case EggGroups.Water2: return "Water 2";
			case EggGroups.Water3: return "Water 3";
			case EggGroups.Humanoid: return "Human-Like";
			case EggGroups.DittoOnly: return "Ditto Only";
			default: return group.ToString();
			}
		}

		public static ushort GetMoveFromMachine(ushort itemID) {
			return machineMoveMap[itemID];
		}

		public static bool PokemonHasMove(IPokemon pokemon, ushort moveID) {
			for (int i = 0; i < pokemon.NumMoves; i++) {
				if (pokemon.GetMoveIDAt(i) == moveID)
					return true;
			}
			return false;
		}

		public static EvolutionData GetEvolutionOnLevelUp(IPokemon pokemon) {
			PokemonData pokemonData = pokemon.PokemonData;
			if (pokemonData.HasEvolutions) {
				for (int i = 0; i < pokemonData.NumEvolutions; i++) {
					EvolutionData evolution = pokemonData.GetEvolution(i);
					if (evolution.EvolutionType == EvolutionTypes.Level && pokemon.Level >= evolution.Parameters[0]) {
						return evolution;
					}
					if (evolution.EvolutionType == EvolutionTypes.Friendship && pokemon.Friendship >= 220) {
						if (evolution.HasParameters) {
							if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.Day && !PokeManager.IsNight)
								return evolution;
							else if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.Night && PokeManager.IsNight)
								return evolution;
						}
						else
							return evolution;
					}
					if (evolution.EvolutionType == EvolutionTypes.Stat && pokemon.Level >= evolution.Parameters[1]) {
						if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.AttackOverDefense && pokemon.Attack > pokemon.Defense)
							return evolution;
						else if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.DefenseOverAttack && pokemon.Defense > pokemon.Attack)
							return evolution;
						else if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.AttackAndDefense && pokemon.Attack == pokemon.Defense)
							return evolution;
					}
					if (evolution.EvolutionType == EvolutionTypes.Condition && pokemon.Beauty >= 170) {
						return evolution;
					}
					if (evolution.EvolutionType == EvolutionTypes.Personality) {
						if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.LessFive && !pokemon.WurpleIsCascoon)
							return evolution;
						else if ((EvolutionParameters)evolution.Parameters[0] == EvolutionParameters.GreaterEqualFive && pokemon.WurpleIsCascoon)
							return evolution;
					}
					if (evolution.EvolutionType == EvolutionTypes.NinjaskShedinja && pokemon.Level >= evolution.Parameters[0]) {
						return evolution;
					}
				}
			}
			return null;
		}

		public static EvolutionData GetEvolutionNow(IPokemon pokemon) {
			PokemonData pokemonData = pokemon.PokemonData;
			if (pokemonData.HasEvolutions) {
				List<EvolutionData> returnEvolutions = new List<EvolutionData>();
				for (int i = 0; i < pokemonData.NumEvolutions; i++) {
					EvolutionData evolution = pokemonData.GetEvolution(i);
					if (evolution.EvolutionType == EvolutionTypes.Trade && !evolution.HasParameters) {
						return evolution;
					}
				}
			}
			return null;
		}

		public static EvolutionData[] GetEvolutionsOnItem(IPokemon pokemon) {
			PokemonData pokemonData = pokemon.PokemonData;
			if (pokemonData.HasEvolutions) {
				List<EvolutionData> returnEvolutions = new List<EvolutionData>();
				for (int i = 0; i < pokemonData.NumEvolutions; i++) {
					EvolutionData evolution = pokemonData.GetEvolution(i);
					if (evolution.EvolutionType == EvolutionTypes.Item) {
						returnEvolutions.Add(evolution);
					}
					if (evolution.EvolutionType == EvolutionTypes.Trade && evolution.HasParameters) {
						returnEvolutions.Add(evolution);
					}
				}
				return returnEvolutions.ToArray();
			}
			return new EvolutionData[0];
		}

		public static ushort[] GetEvolutionItemIDs(IPokemon pokemon) {
			PokemonData pokemonData = pokemon.PokemonData;
			if (pokemonData.HasEvolutions) {
				List<ushort> returnItems = new List<ushort>();
				for (int i = 0; i < pokemonData.NumEvolutions; i++) {
					EvolutionData evolution = pokemonData.GetEvolution(i);
					if (evolution.EvolutionType == EvolutionTypes.Item) {
						returnItems.Add((ushort)evolution.Parameters[0]);
					}
					if (evolution.EvolutionType == EvolutionTypes.Trade && evolution.HasParameters) {
						returnItems.Add((ushort)evolution.Parameters[0]);
					}
				}
				return returnItems.ToArray();
			}
			return new ushort[0];
		}

		public static EvolutionData GetEvolutionFromItemID(IPokemon pokemon, ushort itemID) {
			PokemonData pokemonData = pokemon.PokemonData;
			if (pokemonData.HasEvolutions) {
				List<ushort> returnItems = new List<ushort>();
				for (int i = 0; i < pokemonData.NumEvolutions; i++) {
					EvolutionData evolution = pokemonData.GetEvolution(i);
					if (evolution.EvolutionType == EvolutionTypes.Item && (ushort)evolution.Parameters[0] == itemID) {
						return evolution;
					}
					if (evolution.EvolutionType == EvolutionTypes.Trade && evolution.HasParameters && (ushort)evolution.Parameters[0] == itemID) {
						return evolution;
					}
				}
			}
			return null;
		}

		public static string FindCryFile(ushort dexID) {
			string[] possibleCryFiles = new string[]{
				System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Pokemon", "Cries", dexID.ToString() + ".wav"),
				System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Pokemon", "Cries", dexID.ToString() + ".mp3"),
				System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Pokemon", "Cries", dexID.ToString("000") + ".wav"),
				System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Pokemon", "Cries", dexID.ToString("000") + ".mp3")
			};
			foreach (string cryFile in possibleCryFiles) {
				if (File.Exists(cryFile)) {
					return cryFile;
				}
			}
			return null;
		}

		private static ExperienceGroups GetExperienceGroupFromString(string group) {
			if (group == "FAST") return ExperienceGroups.Fast;
			if (group == "MEDIUM FAST") return ExperienceGroups.MediumFast;
			if (group == "MEDIUM SLOW") return ExperienceGroups.MediumSlow;
			if (group == "SLOW") return ExperienceGroups.Slow;
			if (group == "FLUCTUATING") return ExperienceGroups.Fluctuating;
			if (group == "ERRATIC") return ExperienceGroups.Erratic;

			return (ExperienceGroups)byte.MaxValue;
		}

		public static byte GetAbilityIDFromString(string ability) {
			foreach (AbilityData abilityData in abilityList) {
				if (string.Equals(abilityData.Name, ability, StringComparison.CurrentCultureIgnoreCase)) {
					return abilityData.ID;
				}
			}
			return 0;
		}

		public static Gen3PokemonImageTypes GetPokemonImageTypes(ushort dexID, byte formID = byte.MaxValue) {
			if (formID != byte.MaxValue)
				return gen3PokemonFormImages[new DexFormID { DexID = dexID, FormID = formID }];
			return gen3PokemonImages[dexID];
		}
		public static void ReloadCustomWallpapers() {
			customWallpapersMap.Clear();
			customWallpaperNamesList.Clear();
			string path = Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Wallpapers");
			if (Directory.Exists(path)) {
				string[] files = Directory.GetFiles(path);
				foreach (string file in files) {
					if (Path.GetExtension(file) == ".png") {
						string name = Path.GetFileNameWithoutExtension(file);
						if (name.Length < 30) {
							BitmapSource bitmap = PokeManager.LoadImage(file, true);
							if (bitmap != null) {
								customWallpapersMap.Add(name.ToLower(), bitmap);
								customWallpaperNamesList.Add(name);
							}
						}
					}
				}
			}
		}

		public static BitmapSource GetCustomWallpaper(string name) {
			name = name.ToLower();
			if (customWallpapersMap.ContainsKey(name))
				return customWallpapersMap[name];
			return null;
		}
		public static BitmapSource GetCustomWallpaperAt(int index) {
			string name = GetCustomWallpaperNameAt(index);
			if (customWallpapersMap.ContainsKey(name))
				return customWallpapersMap[name];
			return null;
		}
		public static string GetCustomWallpaperNameAt(int index) {
			if (index >= 0 && index < customWallpaperNamesList.Count)
				return customWallpaperNamesList[index];
			return "";
		}

		public static int NumCustomWallpapers {
			get { return customWallpapersMap.Count; }
		}

		public static void ReloadCustomPokemonSprites() {
			foreach (KeyValuePair<ushort, Gen3PokemonImageTypes> pair in gen3PokemonImages) {
				pair.Value.CustomImage = LoadPokemonImageFromFile(pair.Key, false);
				pair.Value.CustomShinyImage = LoadPokemonImageFromFile(pair.Key, true);
			}
			foreach (KeyValuePair<DexFormID, Gen3PokemonImageTypes> pair in gen3PokemonFormImages) {
				pair.Value.CustomImage = LoadPokemonImageFromFile(pair.Key.DexID, false, pair.Key.FormID);
				pair.Value.CustomShinyImage = LoadPokemonImageFromFile(pair.Key.DexID, true, pair.Key.FormID);
			}
		}
		public static bool HasPokemonImageType(ushort dexID, FrontSpriteSelectionTypes type, bool shiny, byte formID = byte.MaxValue) {
			switch (type) {
			case FrontSpriteSelectionTypes.RSE: return true;
			case FrontSpriteSelectionTypes.FRLG:
				if (formID != byte.MaxValue)
					return false;
				else
					return gen3PokemonImages[dexID].FRLGImage != null;
			case FrontSpriteSelectionTypes.Custom:
				if (formID != byte.MaxValue) {
					if (shiny)
						return gen3PokemonFormImages[new DexFormID { DexID = dexID, FormID = formID }].CustomShinyImage != null;
					else
						return gen3PokemonFormImages[new DexFormID { DexID = dexID, FormID = formID }].CustomImage != null;
				}
				else {
					if (shiny)
						return gen3PokemonImages[dexID].CustomShinyImage != null;
					else
						return gen3PokemonImages[dexID].CustomImage != null;
				}
			}
			return false;
		}
		public static BitmapSource LoadPokemonImageFromFile(ushort dexID, bool isShiny, byte formID = byte.MaxValue) {
			string path = Path.Combine(PokeManager.ApplicationDirectory, "Resources/Pokemon/" + (isShiny ? "FrontShiny" : "Front"));
			string file = null;
			if (formID != byte.MaxValue) {
				if (File.Exists(Path.Combine(path, "Forms", dexID.ToString() + "-" + formID.ToString() + ".png")))
					file = Path.Combine(path, "Forms", dexID.ToString() + "-" + formID.ToString() + ".png");
				else if (File.Exists(Path.Combine(path, "Forms", dexID.ToString("000") + "-" + formID.ToString("00") + ".png")))
					file = Path.Combine(path, "Forms", dexID.ToString("000") + "-" + formID.ToString("00") + ".png");
				else
					return null;
			}
			else if (dexID == 387 && File.Exists(Path.Combine(path, "Egg.png")))
				file = Path.Combine(path, "Egg.png");
			else if (File.Exists(Path.Combine(path, dexID.ToString() + ".png")))
				file = Path.Combine(path, dexID.ToString() + ".png");
			else if (File.Exists(Path.Combine(path, dexID.ToString("000") + ".png")))
				file = Path.Combine(path, dexID.ToString("000") + ".png");
			else
				return null;
			
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			bitmap.UriSource = new Uri(file);
			bitmap.EndInit();

			double dpi = gen3PokemonImages[1].Image.DpiX;
			int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
			byte[] pixelData = new byte[stride * bitmap.PixelHeight];
			bitmap.CopyPixels(pixelData, stride, 0);
			BitmapSource bitmapSource = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, dpi, dpi, bitmap.Format, bitmap.Palette, pixelData, stride);

			return bitmapSource;
		}

		public static NatureData GetNatureFromID(byte id) {
			return natureMap[id];
		}
		public static NatureData GetNatureAt(int index) {
			return natureList[index];
		}

		public static AbilityData GetAbilityFromID(byte id) {
			return abilityMap[id];
		}
		public static AbilityData GetAbilityAt(int index) {
			return abilityList[index];
		}

		public static MoveData GetMoveFromID(ushort id) {
			if (gen3MoveMap.ContainsKey(id))
				return gen3MoveMap[id];
			return null;
		}
		public static MoveData GetMoveAt(int index) {
			return gen3MoveList[index];
		}

		public static PokemonData GetPokemonFromID(ushort id) {
			if (gen3PokemonMap.ContainsKey(id))
				return gen3PokemonMap[id];
			return null;
		}
		public static PokemonData GetPokemonFromDexID(ushort dexID) {
			return gen3PokemonDexMap[dexID];
		}
		public static PokemonData GetPokemonAtDexID(int index) {
			return gen3PokemonDexList[index];
		}

		public static BitmapSource GetPokemonImageFromDexID(ushort dexID, bool shiny, byte formID = byte.MaxValue) {
			FrontSpriteSelectionTypes selectionType = PokeManager.Settings.UsedFrontSpritesType;
			if (dexID == 0 || dexID == 327) { // Spinda and Missing No. keep their default sprites
				selectionType = FrontSpriteSelectionTypes.RSE;
			}
			else if (selectionType == FrontSpriteSelectionTypes.Selection) {
				int shinyID = (shiny && PokeManager.Settings.UseDifferentShinyFrontSprites ? 1 : 0);
				selectionType = PokeManager.Settings.FrontSpriteSelections[shinyID, dexID - 1];
			}

			if (selectionType == FrontSpriteSelectionTypes.Custom) {
				BitmapSource source = LoadPokemonImageFromFile(dexID, shiny);
				if (source != null)
					return source;
			}

			if (formID != byte.MaxValue) {
				Gen3PokemonImageTypes pokemonFormImages = gen3PokemonFormImages[new DexFormID { DexID = dexID, FormID = formID }];
				if (shiny)
					return pokemonFormImages.ShinyImage;
				else
					return pokemonFormImages.Image;
			}
			Gen3PokemonImageTypes pokemonImages = gen3PokemonImages[dexID];
			if (shiny) {
				if (selectionType == FrontSpriteSelectionTypes.FRLG && pokemonImages.FRLGShinyImage != null)
					return pokemonImages.FRLGShinyImage;
				else
					return pokemonImages.ShinyImage;
			}
			else {
				if (selectionType == FrontSpriteSelectionTypes.FRLG && pokemonImages.FRLGImage != null)
					return pokemonImages.FRLGImage;
				else
					return pokemonImages.Image;
			}
		}
		public static BitmapImage GetPokemonBoxImageFromDexID(ushort dexID, bool isShiny, byte formID = byte.MaxValue) {
			Gen3PokemonImageTypes pokemonImages;
			if (formID != byte.MaxValue)
				pokemonImages = gen3PokemonFormImages[new DexFormID { DexID = dexID, FormID = formID }];
			else
				pokemonImages = gen3PokemonImages[dexID];

			if (PokeManager.Settings.UseNewBoxSprites) {
				if (isShiny)
					return pokemonImages.NewBoxShinyImage;
				else
					return pokemonImages.NewBoxImage;
			}
			else {
				return pokemonImages.BoxImage;
			}
		}

		public static BitmapImage GetBallCaughtImageFromID(byte id) {
			if (ballCaughtImages.ContainsKey(id))
				return ballCaughtImages[id];
			return null;
		}

		public static byte GetLevelFromExperience(ExperienceGroups group, uint experience) {
			for (int i = 0; i < 100; i++) {
				if (experienceTable[group][i] > experience)
					return (byte)i;
			}
			return 100;
		}

		public static BitmapImage GetRibbonImageFromID(string id) {
			return ribbonImages[id];
		}

		public static uint GetExperienceFromLevel(ExperienceGroups group, byte level) {
			if (level == 0)
				return 1;
			if (level > 100)
				level = 100;
			return experienceTable[group][level - 1];
		}

		public static string GetMetLocationFromID(ushort id) {
			if (metLocationMap.ContainsKey(id))
				return metLocationMap[id];
			return "Met at a distant land.";
		}

		public static string GetMetLocationAt(int index) {
			return metLocationList[index];
		}

		public static RibbonData GetRibbonFromID(string id) {
			return ribbonMap[id];
		}

		public static uint GetHeartGaugeFromID(ushort shadowID, GameTypes gameType) {
			if (gameType == GameTypes.Colosseum) {
				if (colosseumHeartGauges.ContainsKey(shadowID))
					return colosseumHeartGauges[shadowID];
				return 0;
			}
			return 0;
		}

		public static XDShadowPokemonData GetShadowPokemonDataFromID(ushort shadowID) {
			if (xdShadowData.ContainsKey(shadowID))
				return xdShadowData[shadowID];
			return null;
		}

		public static PokemonData GetPokemonFromName(string name) {
			foreach (KeyValuePair<ushort, PokemonData> pair in gen3PokemonDexMap) {
				if (string.Compare(pair.Value.Name, name, true) == 0)
					return pair.Value;
			}
			return null;
		}
		public static MoveData GetMoveFromName(string name) {
			foreach (KeyValuePair<ushort, MoveData> pair in gen3MoveMap) {
				if (string.Compare(pair.Value.Name, name, true) == 0)
					return pair.Value;
			}
			return null;
		}
	}

	public struct DexFormID {
		public ushort DexID { get; set; }
		public byte FormID { get; set; }
	}
}
