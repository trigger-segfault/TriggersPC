using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Game.FileStructure.Gen3.PB;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.PokemonStructures.Events;
using PokemonManager.Util;
using PokemonManager.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace PokemonManager {

	public class PokemonLocation {
		public IPokemon Pokemon { get; set; }
		public IPokeContainer Container { get; set; }
		public int Index { get; set; }
		public PokemonLocation(IPokemon pokemon) {
			this.Pokemon = pokemon;
			this.Container = pokemon.PokeContainer;
			this.Index = pokemon.ContainerIndex;
		}
	}
	public enum FrontSpriteSelectionTypes {
		RSE = 0,
		FRLG = 1,
		Custom = 2,
		Selection = 3
	}

	public class PokeManagerSettings {
		private string managerNickname;
		private bool useFRLGSprites;
		private bool useNewBoxSprites;
		private bool makeBackups;
		private double volume;
		private bool muted;

		private bool closeConfirmation;
		private bool tossConfirmation;
		private bool releaseConfirmation;

		private bool disableChangesWhileLoading;

		private FrontSpriteSelectionTypes frontSprites;
		private FrontSpriteSelectionTypes[,] frontSpriteSelections;
		private bool useDifferentShinyFrontSprites;

		private int numBoxRows;

		private bool allowDoubleBoxRows;

		private bool mysteryEggs;
		private bool forceSaveAll;

		private bool startupMirageIsland;
		private bool startupShinyEggs;
		private bool startupPokerus;

		private bool revealEggs;

		private bool autoSortItems;
		
		private int defaultStartupTab;
		private Size defaultStartupSize;
		private int defaultGame;
		private int defaultBoxRow1;
		private int defaultBoxGame2;
		private int defaultBoxRow2;
		private int defaultBoxGame3;
		private int defaultBoxRow3;

		private bool keepMissingFiles;

		private bool forceAprilFools;
		private bool debugMode;

		private bool aprilFoolsMode;

		public PokeManagerSettings() {
			this.managerNickname = "Your PC";
			this.useFRLGSprites = false;
			this.useNewBoxSprites = false;
			this.makeBackups = true;
			this.volume = 0.7;
			this.muted = true;
			this.closeConfirmation = true;
			this.tossConfirmation = true;
			this.releaseConfirmation = true;
			this.frontSprites = FrontSpriteSelectionTypes.RSE;
			this.frontSpriteSelections = new FrontSpriteSelectionTypes[2, 387]; //+1 to include egg
			this.useDifferentShinyFrontSprites = false;
			this.numBoxRows = 2;
			this.allowDoubleBoxRows = false;
			this.mysteryEggs = false;
			this.forceSaveAll = false;
			this.startupMirageIsland = false;
			this.startupShinyEggs = false;
			this.startupPokerus = false;
			this.revealEggs = false;
			this.autoSortItems = false;

			this.defaultStartupTab = 0;
			this.defaultStartupSize = new Size();
			this.defaultGame = -1;
			this.defaultBoxRow1 = 0;
			this.defaultBoxGame2 = -1;
			this.defaultBoxRow2 = 0;
			this.defaultBoxGame3 = -1;
			this.defaultBoxRow3 = 0;

			this.keepMissingFiles = false;

			this.forceAprilFools = false;
			this.debugMode = false;

			this.aprilFoolsMode = true;
		}

		public bool AprilFoolsEnabled {
			get { return aprilFoolsMode; }
			set {
				aprilFoolsMode = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}

		public bool ForceAprilFools {
			get { return forceAprilFools; }
			set { forceAprilFools = value; }
		}
		public bool DebugMode {
			get { return debugMode; }
			set { debugMode = value; }
		}

		public bool KeepMissingFiles {
			get { return keepMissingFiles; }
			set {
				keepMissingFiles = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		public bool IsValidDefaultGame {
			get { return defaultGame < PokeManager.NumGameSaves; }
		}
		public bool IsValidDefaultBox1 {
			get {
				if (defaultGame == -1)
					return defaultBoxRow1 < PokeManager.ManagerGameSave.NumPokePCRows;
				else
					return defaultGame < PokeManager.NumGameSaves;
			}
		}
		public bool IsValidDefaultBox2 {
			get {
				if (defaultBoxGame2 == -1)
					return defaultBoxRow2 < PokeManager.ManagerGameSave.NumPokePCRows;
				else
					return defaultBoxGame2 < PokeManager.NumGameSaves;
			}
		}
		public bool IsValidDefaultBox3 {
			get {
				if (defaultBoxGame3 == -1)
					return defaultBoxRow3 < PokeManager.ManagerGameSave.NumPokePCRows;
				else
					return defaultBoxGame3 < PokeManager.NumGameSaves;
			}
		}

		public int DefaultBoxRow1 {
			get { return defaultBoxRow1; }
			set {
				defaultBoxRow1 = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public int DefaultBoxGame2 {
			get { return defaultBoxGame2; }
			set {
				defaultBoxGame2 = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public int DefaultBoxRow2 {
			get { return defaultBoxRow2; }
			set {
				defaultBoxRow2 = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public int DefaultBoxGame3 {
			get { return defaultBoxGame3; }
			set {
				defaultBoxGame3 = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public int DefaultBoxRow3 {
			get { return defaultBoxRow3; }
			set {
				defaultBoxRow3 = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public int DefaultGame {
			get { return defaultGame; }
			set {
				defaultGame = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public int DefaultStartupTab {
			get { return defaultStartupTab; }
			set {
				defaultStartupTab = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}
		public Size DefaultStartupSize {
			get { return defaultStartupSize; }
			set {
				defaultStartupSize = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		public bool RevealEggs {
			get { return revealEggs; }
			set {
				revealEggs = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public bool MysteryEggs {
			get { return mysteryEggs; }
			set {
				mysteryEggs = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}

		public bool AutoSortItems {
			get { return autoSortItems; }
			set {
				autoSortItems = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.ManagerGameSave.Inventory.Items[ItemTypes.InBattle].IsOrdered = value;
					PokeManager.ManagerGameSave.Inventory.Items[ItemTypes.Valuables].IsOrdered = value;
					PokeManager.ManagerGameSave.Inventory.Items[ItemTypes.Hold].IsOrdered = value;
					PokeManager.ManagerGameSave.Inventory.Items[ItemTypes.Misc].IsOrdered = value;
					PokeManager.ManagerGameSave.Inventory.Items[ItemTypes.PokeBalls].IsOrdered = value;
				}
			}
		}

		public bool AlwaysSaveAllSaves {
			get { return forceSaveAll; }
			set {
				forceSaveAll = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		public bool StartupMirageIslandCheck {
			get { return startupMirageIsland; }
			set {
				startupMirageIsland = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		public bool StartupShinyEggsCheck {
			get { return startupShinyEggs; }
			set {
				startupShinyEggs = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		public bool StartupPokerusCheck {
			get { return startupPokerus; }
			set {
				startupPokerus = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		public string ManagerNickname {
			get { return managerNickname; }
			set {
				managerNickname = (value != "" ? value : "Your PC");
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public bool AllowDoubleBoxRows {
			get { return allowDoubleBoxRows; }
			set {
				allowDoubleBoxRows = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public FrontSpriteSelectionTypes UsedFrontSpritesType {
			get { return frontSprites; }
			set {
				frontSprites = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public FrontSpriteSelectionTypes[,] FrontSpriteSelections {
			get { return frontSpriteSelections; }
		}
		public bool UseDifferentShinyFrontSprites {
			get { return useDifferentShinyFrontSprites; }
			set { useDifferentShinyFrontSprites = value; }
		}
		public int NumBoxRows {
			get { return numBoxRows; }
			set {
				numBoxRows = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public bool UseFRLGSprites {
			get { return useFRLGSprites; }
			set {
				useFRLGSprites = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public bool UseNewBoxSprites {
			get { return useNewBoxSprites; }
			set {
				useNewBoxSprites = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
					PokeManager.RefreshUI();
				}
			}
		}
		public bool MakeBackups {
			get { return makeBackups; }
			set {
				makeBackups = value;
				if (!disableChangesWhileLoading)
					PokeManager.SaveSettings();
			}
		}
		public double Volume {
			get { return volume; }
			set {
				volume = Math.Max(0, Math.Min(1, value));
				if (!disableChangesWhileLoading)
					PokeManager.SaveSettings();
			}
		}
		public double MutedVolume {
			get { return muted ? 0 : volume; }
		}
		public bool IsMuted {
			get { return muted; }
			set {
				muted = value;
				if (!disableChangesWhileLoading)
					PokeManager.SaveSettings();
			}
		}
		public bool CloseConfirmation {
			get { return closeConfirmation; }
			set {
				closeConfirmation = value;
				if (!disableChangesWhileLoading) {
					PokeManager.SaveSettings();
				}
			}
		}

		// Settings that reset
		public bool TossConfirmation {
			get { return tossConfirmation; }
			set { tossConfirmation = value; }
		}
		public bool ReleaseConfirmation {
			get { return releaseConfirmation; }
			set { releaseConfirmation = value; }
		}
		
		// Used to prevent the extra effects of changing a property
		public bool DisableChangesWhileLoading {
			get { return disableChangesWhileLoading; }
			set { disableChangesWhileLoading = value; }
		}
	}

	public enum UIModifyFlags : uint {
		PokemonViewer = 0x1,
		PokemonBoxes = 0x2,
		Inventory = 0x4,
		Pokedex = 0x8,
		GameSaves = 0x10,
		Rows = 0x20,
		All = 0xFFFFFFFF
	}

	public class GameSaveFileInfo {
		public string FilePath { get; set; }
		public GameTypes GameType { get; set; }
		public string Nickname { get; set; }
		public IGameSave GameSave { get; set; }
		public FileInfo FileInfo { get; set; }
		public bool IsJapanese { get; set; }
		public bool IsLivingDex { get; set; }

		public GameSaveFileInfo(string filePath, GameTypes gameType = GameTypes.Any, string nickname = "", bool japanese = false, bool livingDex = false) {
			this.FilePath = filePath;
			this.GameType = gameType;
			this.Nickname = nickname;
			this.IsJapanese = japanese;
			this.IsLivingDex = livingDex;
			this.GameSave = null;
			this.FileInfo = new FileInfo(filePath);
		}

		public bool IsFileModified {
			get {
				if (File.Exists(FilePath)) {
					FileInfo newFileInfo = new FileInfo(FilePath);
					return this.FileInfo.Length != newFileInfo.Length || this.FileInfo.LastWriteTimeUtc != newFileInfo.LastWriteTimeUtc;
				}
				return false;
			}
		}
	}

	public static class PokeManager {

		private static PokeManagerWindow managerWindow;
		private static PokeManagerSettings settings;

		// Gen 3 Saves
		private static List<GameSaveFileInfo> gameSaveFiles;
		private static List<GameSaveFileInfo> missingGameSaveFiles;
		private static ManagerGameSave managerGameSave;

		// Events
		private static List<EventDistribution> events;
		private static Dictionary<string, EventDistribution> eventMap;
		private static Dictionary<string, List<uint>> completedEvents;

		// Pokemon Moving
		private static PokemonLocation holdPokemon;
		private static List<PokemonLocation> selectedPokemon;
		private static PokeBoxControl holdAdorner;

		private static IGameSave lastGameInDialog;
		private static BitmapSource customTrainerImage;

		private static List<SharedSecretBase> secretBases;

		private static List<PokerusStrain> pokerusStrains;

		public static bool IsReloading { get; set; }

		private static bool firstTimeStartingUp;

		private static bool loaded;

		public static bool IsFirstTimeStartingUp {
			get { return firstTimeStartingUp; }
		}

		public static bool IsAprilFoolsDay {
			get {
#if DEBUG
				return true;
#else
				return settings.ForceAprilFools || (DateTime.Now.Month == 4 && DateTime.Now.Day == 1);
#endif
			}
		}

		public static bool IsAprilFoolsMode {
			get {
				return IsAprilFoolsDay && settings.AprilFoolsEnabled;
			}
		}

		public static bool DebugMode {
			get { return settings.DebugMode; }
		}

		public static void Initialize(PokeManagerWindow managerWindow) {
			PokeManager.managerWindow = managerWindow;

			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
			ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

			selectedPokemon = new List<PokemonLocation>();
			secretBases = new List<SharedSecretBase>();
			pokerusStrains = new List<PokerusStrain>();
			firstTimeStartingUp = true;

			#region Stuff

			/*SQLiteConnection connection = new SQLiteConnection("Data Source=\"C:/Users/Jrob/My Projects/C#/PokemonManager/PokemonManager/SecretBaseDatabase.db\"");
			connection.Open();

			SQLiteCommand command;
			SQLiteDataReader reader;
			DataTable table;
			string[] files;

			{
				SQLiteCommand cmd;
				string path = Path.Combine(@"C:\Users\Jrob\Desktop\Trigger's PC\Secret Bases", "Route120ID204.png");
				cmd = new SQLiteCommand("UPDATE Locations SET Image = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
				param0.Value = File.ReadAllBytes(path);
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = 204;
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();
			}
			{
				SQLiteCommand cmd;
				string path = Path.Combine(@"C:\Users\Jrob\Desktop\Trigger's PC\Secret Bases", "Route120ID232.png");
				cmd = new SQLiteCommand("UPDATE Locations SET Image = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
				param0.Value = File.ReadAllBytes(path);
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = 232;
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();
			}

			connection.Close();

			Sfiles = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\DecorationsFullSize");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE DecorationImages SET ImageFullSize = (@1) WHERE ID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}
			connection.Close();

			command = new SQLiteCommand("SELECT * FROM Rooms", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Rooms");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				SQLiteCommand cmd;
				string path = Path.Combine(@"C:\Users\Jrob\Desktop\New folder (2)\SecretBases\Layouts", (row["Type"] as string).Replace(" ", "") + (row["Layout"] as string) + ".png");
				cmd = new SQLiteCommand("UPDATE Rooms SET Image = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
				param0.Value = File.ReadAllBytes(path);
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();

				path = Path.Combine(@"C:\Users\Jrob\Desktop\New folder (2)\SecretBases\Layouts", (row["Type"] as string).Replace(" ", "") + "Background.png");
				cmd = new SQLiteCommand("UPDATE Rooms SET BackgroundImage = (@0) WHERE ID = (@1);", connection);
				param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
				param0.Value = File.ReadAllBytes(path);
				param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();
			}

			command = new SQLiteCommand("SELECT * FROM Locations", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Locations");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				SQLiteCommand cmd;
				string path = Path.Combine(@"C:\Users\Jrob\Desktop\Trigger's PC\Secret Bases", "Route" + ((long)row["Route"]) + "ID" + ((long)row["ID"]) + ".png");
				cmd = new SQLiteCommand("UPDATE Locations SET Image = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
				param0.Value = File.ReadAllBytes(path);
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();
			}

			command = new SQLiteCommand("SELECT * FROM Routes", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Locations");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				SQLiteCommand cmd;
				string path = Path.Combine(@"C:\Users\Jrob\Desktop\New folder (2)\SecretBases\Routes", "Route" + ((long)row["ID"]) + ".png");
				cmd = new SQLiteCommand("UPDATE Routes SET Image = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
				param0.Value = File.ReadAllBytes(path);
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();
			}

			connection.Close();*/

			/*string file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Front\Forms\201-26.png";
			SQLiteCommand cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET Image = (@1) WHERE ID = (@0);";
			SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 26;
			SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Front\Forms\201-27.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET Image = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 27;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\FrontShiny\Forms\201-26.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET ShinyImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 26;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\FrontShiny\Forms\201-27.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET ShinyImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 27;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Box Sprites\Forms\201-26.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET BoxImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 26;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Box Sprites\Forms\201-27.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET BoxImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 27;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Box Sprites\Forms\201-26.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET NewBoxImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 26;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Box Sprites\Forms\201-27.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET NewBoxImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 27;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Shiny Box Sprites\Forms\201-26.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET NewBoxShinyImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 26;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Shiny Box Sprites\Forms\201-27.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE UnownFormImages SET NewBoxShinyImage = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 27;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();


			connection.Close();

			connection = new SQLiteConnection("Data Source=\"C:/Users/Jrob/My Projects/C#/PokemonManager/PokemonManager/ItemDatabase.db\"");
			connection.Open();

			file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Items\371.png";
			cmd = connection.CreateCommand();
			cmd.CommandText = "UPDATE ItemImages SET Image = (@1) WHERE ID = (@0);";
			param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
			param1.Value = 371;
			param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
			param2.Value = File.ReadAllBytes(file);
			cmd.Parameters.Add(param1);
			cmd.Parameters.Add(param2);
			cmd.ExecuteNonQuery();

			connection.Close();*/

			/*files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Box Sprites");
			foreach (string file in files) {
				string file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Box Sprites\000.png";
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE PokemonImages SET NewBoxImage = (@1) WHERE DexID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}

			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Shiny Box Sprites");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE PokemonImages SET NewBoxShinyImage = (@1) WHERE DexID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}

			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Box Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					if (Path.GetFileName(file).Length >= 10) {
						int dexID = int.Parse(Path.GetFileName(file).Substring(0, 3));
						if (dexID == 201) {
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "UPDATE UnownFormImages SET NewBoxImage = (@1) WHERE ID = (@0);";
							SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
							param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4, 2));
							SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
							param2.Value = File.ReadAllBytes(file);
							cmd.Parameters.Add(param1);
							cmd.Parameters.Add(param2);
							cmd.ExecuteNonQuery();
						}
						else if (dexID == 386) {
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "UPDATE DeoxysFormImages SET NewBoxImage = (@1) WHERE ID = (@0);";
							SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
							param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4, 2));
							SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
							param2.Value = File.ReadAllBytes(file);
							cmd.Parameters.Add(param1);
							cmd.Parameters.Add(param2);
							cmd.ExecuteNonQuery();
						}
					}
				}
			}

			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Shiny Box Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					if (Path.GetFileName(file).Length >= 10) {
						int dexID = int.Parse(Path.GetFileName(file).Substring(0, 3));
						if (dexID == 201) {
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "UPDATE UnownFormImages SET NewBoxShinyImage = (@1) WHERE ID = (@0);";
							SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
							param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4, 2));
							SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
							param2.Value = File.ReadAllBytes(file);
							cmd.Parameters.Add(param1);
							cmd.Parameters.Add(param2);
							cmd.ExecuteNonQuery();
						}
						else if (dexID == 386) {
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "UPDATE DeoxysFormImages SET NewBoxShinyImage = (@1) WHERE ID = (@0);";
							SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
							param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4, 2));
							SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
							param2.Value = File.ReadAllBytes(file);
							cmd.Parameters.Add(param1);
							cmd.Parameters.Add(param2);
							cmd.ExecuteNonQuery();
						}
					}
				}
			}

			connection.Close();*/

			/*string text = File.ReadAllText(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\EasyChatCodes1.txt");
			string word = "";
			int codeGroup = 1;
			int codeGroupIndex = -1;
			while (GetNextCode(ref text, out word, ref codeGroup, ref codeGroupIndex)) {
				SQLiteCommand cmd = connection.CreateCommand();
				cmd.CommandText = "INSERT INTO EasyChat (ID, Word) VALUES (@0, @1);";
				SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
				param1.Value = (codeGroup << 9) | codeGroupIndex;
				SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.String);
				param2.Value = ChangeStringCasing(word);
				cmd.Parameters.Add(param1);
				cmd.Parameters.Add(param2);
				cmd.ExecuteNonQuery();
			}*/

			/*files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\DecorationsSmall");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE DecorationImages SET Image = (@1) WHERE ID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}*/

			/*files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Ribbons");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "INSERT INTO RibbonImages (ID, Image) VALUES (@0, @1);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.String);
					param1.Value = Path.GetFileNameWithoutExtension(file).ToUpper();
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}
			
			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Box Sprites\000.png");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE PokemonImages SET BoxImage = (@1) WHERE DexID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}*/

			/*{
				string file = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Box Sprites\000.png";
				SQLiteCommand cmd = connection.CreateCommand();
				cmd.CommandText = "UPDATE PokemonImages SET BoxImage = (@1) WHERE DexID = (@0);";
				SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
				param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
				SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
				param2.Value = File.ReadAllBytes(file);
				cmd.Parameters.Add(param1);
				cmd.Parameters.Add(param2);
				cmd.ExecuteNonQuery();
			}*/
			/*{
				string file1 = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\egg2.png";
				string file2 = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\egg.png";
				SQLiteCommand cmd = connection.CreateCommand();
				cmd.CommandText = "UPDATE PokemonImages SET Image = (@1), ShinyImage = (@1), BoxImage = (@2) WHERE DexID = (@0);";
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Int32);
				param0.Value = 387;
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Binary);
				param1.Value = File.ReadAllBytes(file1);
				SQLiteParameter param2 = new SQLiteParameter("@2", System.Data.DbType.Binary);
				param2.Value = File.ReadAllBytes(file2);
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.Parameters.Add(param2);
				cmd.ExecuteNonQuery();
			}*/

			/*files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Front Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png" && Path.GetFileName(file).StartsWith("201")) {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "INSERT INTO UnownFormImages (ID, Image) VALUES (@0, @1);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}
			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Front Shiny Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png" && Path.GetFileName(file).StartsWith("201")) {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE UnownFormImages SET ShinyImage = (@1) WHERE ID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}
			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Box Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png" && Path.GetFileName(file).StartsWith("201")) {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE UnownFormImages SET BoxImage = (@1) WHERE ID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}*/

			/*files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Front Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png" && Path.GetFileName(file).StartsWith("386")) {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "INSERT INTO DeoxysFormImages (ID, Image) VALUES (@0, @1);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}
			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Front Shiny Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png" && Path.GetFileName(file).StartsWith("386")) {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE DeoxysFormImages SET ShinyImage = (@1) WHERE ID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}
			files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Box Sprites\Forms");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png" && Path.GetFileName(file).StartsWith("386")) {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "UPDATE DeoxysFormImages SET BoxImage = (@1) WHERE ID = (@0);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file).Substring(4));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}*/

			/*files = Directory.GetFiles(@"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\BallCaught");
			foreach (string file in files) {
				if (Path.GetExtension(file) == ".png") {
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = "INSERT INTO BallImages (ID, Image) VALUES (@0, @1);";
					SQLiteParameter param1 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param1.Value = Int32.Parse(Path.GetFileNameWithoutExtension(file));
					SQLiteParameter param2 = new SQLiteParameter("@1", System.Data.DbType.Binary);
					param2.Value = File.ReadAllBytes(file);
					cmd.Parameters.Add(param1);
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}
			}*/

			/*

			command = new SQLiteCommand("SELECT * FROM Moves", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Moves");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				SQLiteCommand cmd;

				string name = ChangeStringCasing(row["Name"] as string);
				cmd = new SQLiteCommand("UPDATE Moves SET Name = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.String);
				param0.Value = name;
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();

				string conditionType = row["ConditionType"] as string;
				if (conditionType == "BEAUTIFUL" || conditionType == "CLEVER") {
					if (conditionType == "BEAUTIFUL")
						conditionType = "BEAUTY";
					else
						conditionType = "SMART";
					cmd = new SQLiteCommand("UPDATE Moves SET ConditionType = (@0) WHERE ID = (@1);", connection);
					SQLiteParameter param2 = new SQLiteParameter("@0", System.Data.DbType.String);
					param2.Value = conditionType;
					SQLiteParameter param3 = new SQLiteParameter("@1", System.Data.DbType.Int32);
					param3.Value = row["ID"];
					cmd.Parameters.Add(param2);
					cmd.Parameters.Add(param3);
					cmd.ExecuteNonQuery();
				}
			}

			command = new SQLiteCommand("SELECT * FROM Pokemon", connection);
			reader = command.ExecuteReader();
			table = new DataTable("Pokemon");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				SQLiteCommand cmd;

				string name = ChangeStringCasing(row["Name"] as string);
				cmd = new SQLiteCommand("UPDATE Pokemon SET Name = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.String);
				param0.Value = name;
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();

				string type2 = row["Type2"] as string;
				if (type2 == "") {
					cmd = new SQLiteCommand("UPDATE Pokemon SET Type2 = NULL WHERE ID = (@0);", connection);
					SQLiteParameter param2 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param2.Value = row["ID"];
					cmd.Parameters.Add(param2);
					cmd.ExecuteNonQuery();
				}

				string ability2 = row["Ability2"] as string;
				if (ability2 == "") {
					cmd = new SQLiteCommand("UPDATE Pokemon SET Ability2 = NULL WHERE ID = (@0);", connection);
					SQLiteParameter param3 = new SQLiteParameter("@0", System.Data.DbType.Int32);
					param3.Value = row["ID"];
					cmd.Parameters.Add(param3);
					cmd.ExecuteNonQuery();
				}
			}*/

			/*command = new SQLiteCommand("SELECT * FROM MetLocations", connection);
			reader = command.ExecuteReader();
			table = new DataTable("MetLocations");
			table.Load(reader);
			foreach (DataRow row in table.Rows) {
				string name = ChangeStringCasing(row["Name"] as string);
				SQLiteCommand cmd = new SQLiteCommand("UPDATE MetLocations SET Name = (@0) WHERE ID = (@1);", connection);
				SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.String);
				param0.Value = name;
				SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Int32);
				param1.Value = row["ID"];
				cmd.Parameters.Add(param0);
				cmd.Parameters.Add(param1);
				cmd.ExecuteNonQuery();
			}

			connection.Close();*/

			#endregion

			CreateDirectories();

			CharacterEncoding.Initialize();
			ResourceDatabase.Initialize();
			ItemDatabase.Initialize();
			PokemonDatabase.Initialize();
			ItemDatabase.InitializeEasyChat();
			SecretBaseDatabase.Initialize();

			try {
				LoadSettings();
			}
			catch (Exception ex) {
				MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Settings. Would you like to view the error?", "Load Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex);
			}
			CreateEvents();

			LoadPokeManager();
#if DEBUG
			/*GBAGameSave boy = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Male.sav");
			GBAGameSave girl = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Female.sav");
			GBAGameSave boy2 = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Male2.sav");
			GBAGameSave girl2 = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Female2.sav");
			GBAGameSave boy3 = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Male3.sav");
			GBAGameSave girl3 = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Female3.sav");

			var blocksBoy = boy.MostRecentSave.BlockDataCollection;
			var blocksGirl = girl.MostRecentSave.BlockDataCollection;
			var blocksBoy2 = boy2.MostRecentSave.BlockDataCollection;
			var blocksGirl2 = girl2.MostRecentSave.BlockDataCollection;
			var blocksBoy3 = boy3.MostRecentSave.BlockDataCollection;
			var blocksGirl3 = girl3.MostRecentSave.BlockDataCollection;

			Dictionary<SectionTypes, List<DifferenceData>> differenceData = new Dictionary<SectionTypes, List<DifferenceData>>();

			foreach (SectionTypes sectionType in Enum.GetValues(typeof(SectionTypes))) {
				if (sectionType >= SectionTypes.PCBufferA && sectionType <= SectionTypes.PCBufferI)
					continue;
				byte[] rawBoy = blocksBoy.GetBlockData(sectionType).Raw;
				byte[] rawGirl = blocksGirl.GetBlockData(sectionType).Raw;
				byte[] rawBoy2 = blocksBoy2.GetBlockData(sectionType).Raw;
				byte[] rawGirl2 = blocksGirl2.GetBlockData(sectionType).Raw;
				byte[] rawBoy3 = blocksBoy3.GetBlockData(sectionType).Raw;
				byte[] rawGirl3 = blocksGirl3.GetBlockData(sectionType).Raw;
				differenceData.Add(sectionType, new List<DifferenceData>());

				DifferenceData difference = null;
				for (int i = 0; i < SectionIDTable.GetContents(sectionType); i++) {
					if (rawBoy[i] != rawGirl[i] && rawBoy[i] == rawBoy2[i] && rawGirl[i] == rawGirl2[i]) {
						if (difference == null)
							difference = new DifferenceData(i);
						else
							difference.Length++;
					}
					else if (difference != null) {
						difference.Data1 = new DataString(rawBoy, difference.StartIndex, difference.Length);
						difference.Data2 = new DataString(rawGirl, difference.StartIndex, difference.Length);
						difference.Data3 = new DataString(rawBoy3, difference.StartIndex, difference.Length);
						difference.Data4 = new DataString(rawGirl3, difference.StartIndex, difference.Length);
						differenceData[sectionType].Add(difference);
						difference = null;
					}
				}
			}
			Console.WriteLine(differenceData.ToString());
			boy3.TrainerGender = Genders.Female;
			
			boy3.Save(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Gender Saves\Emerald Test.sav");
			Environment.Exit(0);*/
#endif

			#region Stuff 2 - Electric Boogaloo

			/*GBAGameSave emSave = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Real Saves\Emerald - EMERALD.sav");

			NationalPokedexBAndCBlockData data = emSave.MostRecentSave.BlockDataCollection.NationalPokedexBAndC;
			data.Raw[1176] = 3;
			data.Raw[1177] = 4;
			Console.WriteLine(data.Raw[1112]);

			emSave.Save(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Real Saves\Emerald Altering Cave Test.sav");

			GBAGameSave frSave = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Real Saves\FireRed Altering Cave Houndour.sav");


			Dictionary<SectionTypes, List<DifferenceData>> difData = FindDifferenceData(frSave.MostRecentSave.BlockDataCollection, frSave.LeastRecentSave.BlockDataCollection);


			Console.WriteLine(difData);*/

			//GBAGameSave frSave = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\FireRed - RED.sav");
			//GBAGameSave emSave = new GBAGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Emerald - EMERALD.sav");
			/*Mail mail = new Mail();
			mail.SetEasyChatCodeAt(0, ItemDatabase.GetIDFromEasyChat("I"));
			mail.SetEasyChatCodeAt(1, ItemDatabase.GetIDFromEasyChat("Want"));
			mail.SetEasyChatCodeAt(2, ItemDatabase.GetIDFromEasyChat("To"));
			mail.SetEasyChatCodeAt(3, ItemDatabase.GetIDFromEasyChat("See"));
			mail.SetEasyChatCodeAt(4, ItemDatabase.GetIDFromEasyChat("A"));
			mail.SetEasyChatCodeAt(5, ItemDatabase.GetIDFromEasyChat("Petal Dance"));
			mail.SetEasyChatCodeAt(6, ItemDatabase.GetIDFromEasyChat("It's"));
			mail.SetEasyChatCodeAt(7, ItemDatabase.GetIDFromEasyChat("So"));
			mail.SetEasyChatCodeAt(8, ItemDatabase.GetIDFromEasyChat("Pretty"));*/
			//var result = SearchFor(rSave.MostRecentSave.BlockDataCollection, ByteHelper.SubByteArray(0, mail.Raw, 18));
			//var result = SearchFor(frSave.MostRecentSave.BlockDataCollection, );
			//byte[] raw = frSave.MostRecentSave.BlockDataCollection.GetBlockData(SectionTypes.Unknown2).Raw;
			//var result = SearchFor(ByteHelper.SubByteArray(3554 - 18 - 300 + 225 + 324, raw, raw.Length - (3554 - 18 - 300 + 225 + 324)), BitConverter.GetBytes((byte)131));
			//byte[] data = ByteHelper.SubByteArray(3554 - 30, frSave.MostRecentSave.BlockDataCollection.GetBlockData(SectionTypes.Unknown2).Raw, 100);
			//Mailbox mailbox = new Mailbox(frSave, 10);
			//byte[] data3 = ByteHelper.SubByteArray(3536 - 36 * 4, raw, 3968 - (3536 - 36 * 4));

			//mailbox.LoadPart1(ByteHelper.SubByteArray(3536, raw, 36 * 12));
			//raw = frSave.MostRecentSave.BlockDataCollection.GetBlockData(SectionTypes.RivalInfo).Raw;
			//mailbox.LoadPart2(ByteHelper.SubByteArray(0, raw, 36 * 4));
			//3382 - 18 - 216 (234) //3148
			//3554 - 234 //3320
			//3460
			//3461 + 323
			//3784


			//byte[] data2 = ByteHelper.SubByteArray(3784 - 30, raw, 100);
			//frSave.MostRecentSave.BlockDataCollection.GetBlockData(SectionTypes.Unknown2).Raw[3784] = 121;

			/*Mail mail1 = emSave.Mailbox[0];
			emSave.Mailbox.Clear();
			for (int i = 0; i < 10; i++) {
				Mail mail2 = new Mail(ByteHelper.SubByteArray(0, mail1.Raw, 36));
				mail2.MailItemID = (ushort)(123 + i);
				emSave.Mailbox.AddMail(mail2);
			}

			emSave.Save(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\Emerald - EMERALD3.sav");
			Console.WriteLine(result);
			Console.WriteLine(data2);
			Console.WriteLine(data3);
			Console.WriteLine(mailbox);*/

			//GCGameSave xdSave = new GCGameSave(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\01-GXXE-PokemonXD Erik2.gci");
			//byte[] d1 = xdSave.MostRecentSave.ShadowPokemonData.GetInfoFromDexID(57).Raw;
			//byte[] d2 = xdSave.LeastRecentSave.ShadowPokemonData.GetInfoFromDexID(57).Raw;

			//DifferenceData[] difData = FindDifferenceData(d1, d2);

			//Console.WriteLine(difData);

			//xdSave.Save(@"C:\Users\Jrob\My Projects\C#\PokemonManager\Saves\01-GXXE-PokemonXD Erik TEST.gci");
			//Environment.Exit(0);

			/*BitmapImage boxLayout = new BitmapImage(new Uri(@"C:\Users\Jrob\My Projects\C#\PokemonManager\PokemonManager\Pokecons.png"));

			Dictionary<int, int> skipIndexes = new Dictionary<int, int>() {
				{3, 1}, {6, 2}, {9, 1}, {65, 1}, {94, 1}, {115, 1}, {127, 1}, {130, 1}, {142, 1}, {150, 2},
				{181, 1}, {201, 27}, {212, 1}, {214, 1}, {229, 1}, {248, 1},
				{257, 1}, {282, 1}, {303, 1}, {306, 1}, {308, 1}, {310, 1}, {351, 3}, {354, 1}, {359, 1}, {380, 1}, {381, 1}
			};
			string path = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Box Sprites";
			string pathShiny = @"C:\Users\Jrob\Desktop\New folder (2)\Pokemon Sprites\Gen III\Gen 6 Shiny Box Sprites";
			Console.WriteLine(boxLayout);
			int skipCount = 0;
			for (int i = 0; i < 386; i++) {
				if (i + 1 == 201 || i + 1 == 386)
					Console.WriteLine(i.ToString() + " Skips: " + skipCount);
				int normalIndex = (i + skipCount) * 2;
				int shinyIndex = (i + skipCount) * 2 + 1;
				GetAndSaveIndex(boxLayout, normalIndex, Path.Combine(path, (i + 1).ToString("000") + ".png"));
				GetAndSaveIndex(boxLayout, shinyIndex, Path.Combine(pathShiny, (i + 1).ToString("000") + ".png"));
				if (skipIndexes.ContainsKey(i + 1))
					skipCount += skipIndexes[i + 1];
			}
			skipCount = 13 + 201 - 1;
			for (int i = 0; i < 28; i++) {
				int normalIndex = (i + skipCount) * 2;
				int shinyIndex = (i + skipCount) * 2 + 1;
				GetAndSaveIndex(boxLayout, normalIndex, Path.Combine(path, "Forms", "201-" + i.ToString("00") + ".png"));
				GetAndSaveIndex(boxLayout, shinyIndex, Path.Combine(pathShiny, "Forms", "201-" + i.ToString("00") + ".png"));
			}
			skipCount = 57 + 386 - 1;
			for (int i = 0; i < 4; i++) {
				int normalIndex = (i + skipCount) * 2;
				int shinyIndex = (i + skipCount) * 2 + 1;
				GetAndSaveIndex(boxLayout, normalIndex, Path.Combine(path, "Forms", "386-" + i.ToString("00") + ".png"));
				GetAndSaveIndex(boxLayout, shinyIndex, Path.Combine(pathShiny, "Forms", "386-" + i.ToString("00") + ".png"));
			}
			Console.WriteLine("Finished");*/

			#endregion

			loaded = true;
		}

		private static void CreateEvents() {
			PokeManager.events = new List<EventDistribution>();
			PokeManager.eventMap = new Dictionary<string, EventDistribution>();
			PokeManager.completedEvents = new Dictionary<string,List<uint>>();

			BikeEventDistribution bike;
			TicketEventDistribution ticket;
			PokemonEventDistribution pokemon;
			BuyPokemonEventDistribution buyPokemon;
			RegiDollEventDistribution regi;

			bike = new BikeEventDistribution("ACRO MACH BIKE [RSE]");
			bike.SmallSprite = ItemDatabase.GetItemImageFromID(272);
			bike.BigSprite = bike.SmallSprite;
			bike.Title = "Acro Bike & Mach Bike";
			bike.Description = "Own both the Acro Bike and Mach Bike in order to overcome any obstacle.";
			bike.Requirements = "You must beat the Elite Four in order to receive this bike.";
			bike.AllowedGames = GameTypeFlags.Ruby | GameTypeFlags.Sapphire | GameTypeFlags.Emerald;
			RegisterEvent(bike);

			ticket = new TicketEventDistribution("EON TICKET [R]");
			ticket.TicketType = TicketTypes.EonTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(275);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Eon Ticket";
			ticket.Description = "Travel on the S.S. Tidal in Lilycove to Southern Island where you will find Latias.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.Ruby;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("EON TICKET [S]");
			ticket.TicketType = TicketTypes.EonTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(275);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Eon Ticket";
			ticket.Description = "Travel on the S.S. Tidal in Lilycove to Southern Island where you will find Latios.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.Sapphire;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("EON TICKET [E]");
			ticket.TicketType = TicketTypes.EonTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(275);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Eon Ticket";
			ticket.Description = "Travel on the S.S. Tidal in Lilycove to Southern Island where you will find either Latios or Latias.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.Emerald;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("MYSTIC TICKET [FRLG]");
			ticket.TicketType = TicketTypes.MysticTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(370);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Mystic Ticket";
			ticket.Description = "Travel on the Seagallop Ferry in Vermilion to Navel Rock where you will find Lugia and Ho-Oh.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.FireRed | GameTypeFlags.LeafGreen;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("MYSTIC TICKET [E]");
			ticket.TicketType = TicketTypes.MysticTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(370);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Mystic Ticket";
			ticket.Description = "Travel on the S.S. Tidal in Lilycove to Navel Rock where you will find Lugia and Ho-Oh.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.Emerald;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("AURORA TICKET [FRLG]");
			ticket.TicketType = TicketTypes.AuroraTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(371);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Aurora Ticket";
			ticket.Description = "Travel on the Seagallop Ferry in Vermilion to Birth Island where you will find Deoxys.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.FireRed | GameTypeFlags.LeafGreen;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("AURORA TICKET [E]");
			ticket.TicketType = TicketTypes.AuroraTicket;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(371);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Aurora Ticket";
			ticket.Description = "Travel on the S.S. Tidal in Lilycove to Birth Island where you will find Deoxys.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.Emerald;
			RegisterEvent(ticket);

			ticket = new TicketEventDistribution("OLD SEA MAP [E]");
			ticket.TicketType = TicketTypes.OldSeaMap;
			ticket.SmallSprite = ItemDatabase.GetItemImageFromID(376);
			ticket.BigSprite = ticket.SmallSprite;
			ticket.Title = "Old Sea Map";
			ticket.Description = "Travel on the S.S. Tidal in Lilycove to Faraway Island where you will find Mew.";
			ticket.Requirements = "You must beat the Elite Four in order to receive this ticket.";
			ticket.AllowedGames = GameTypeFlags.Emerald;
			RegisterEvent(ticket);

			pokemon = new PokemonEventDistribution("JIRACHI [RSE]");
			pokemon.DexID = 385;
			pokemon.Level = 5;
			pokemon.HeldItem = new ushort[] { 169, 170 };
			pokemon.TrainerName = "WISHMKR";
			pokemon.TrainerGender = Genders.Male;
			pokemon.TrainerID = 20043;
			pokemon.SecretID = 0;
			pokemon.SmallSprite = PokemonDatabase.GetPokemonImageTypes(pokemon.DexID).NewBoxImage;
			pokemon.BigSprite = PokemonDatabase.GetPokemonImageTypes(pokemon.DexID).Image;
			pokemon.Title = "Colosseum Bonus Disc";
			pokemon.Description = "A Jirachi obtainable from the English Pokémon Colosseum Bonus Disc.";
			pokemon.Requirements = "You must beat the Elite Four in order to receive this Pokémon.";
			pokemon.AllowedGames = GameTypeFlags.Ruby | GameTypeFlags.Sapphire | GameTypeFlags.Emerald;
			pokemon.CheckRequirements = new CheckEventRequirementsDelegate(delegate(IGameSave gameSave){
				GBAGameSave gbaSave = gameSave as GBAGameSave;
				GameTypes gameType = gameSave.GameType;
				if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
					return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBeatenEliteFour);
				else
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBeatenEliteFour);
			});
			RegisterEvent(pokemon);

			pokemon = new PokemonEventDistribution("CELEBI [FRLG]");
			pokemon.DexID = 251;
			pokemon.Level = 5;
			pokemon.HeldItem = new ushort[] { 141 };
			pokemon.TrainerName = "TIMEFLT";
			pokemon.TrainerGender = Genders.Female;
			pokemon.TrainerID = 20042;
			pokemon.SecretID = 0;
			pokemon.SmallSprite = PokemonDatabase.GetPokemonImageTypes(pokemon.DexID).NewBoxImage;
			pokemon.BigSprite = PokemonDatabase.GetPokemonImageTypes(pokemon.DexID).Image;
			pokemon.Title = "Time Flute";
			pokemon.Description = "An obtainable Celebi for those who look to complete their Pokédex in its entirety.";
			pokemon.Requirements = "You must beat the Elite Four and link up the Network Machine between Kanto and Hoenn in order to receive this Pokémon.";
			pokemon.AllowedGames = GameTypeFlags.FireRed | GameTypeFlags.LeafGreen;
			pokemon.CheckRequirements = new CheckEventRequirementsDelegate(delegate(IGameSave gameSave) {
				GBAGameSave gbaSave = gameSave as GBAGameSave;
				return gbaSave.GetGameFlag((int)FireRedLeafGreenGameFlags.HasBeatenEliteFour) && gbaSave.GetGameFlag((int)FireRedLeafGreenGameFlags.RubySapphireSubplotFinished);
			});
			RegisterEvent(pokemon);

			regi = new RegiDollEventDistribution("REGIROCK DOLL [RSE]");
			regi.DollID = 118;
			regi.SmallSprite = ItemDatabase.GetDecorationImageFromID(regi.DollID);
			regi.BigSprite = ItemDatabase.GetDecorationFullSizeImageFromID(regi.DollID);
			regi.Title = "e-Reader Regirock Doll";
			regi.Description = "A large doll representing Regirock obtained from an e-Reader card handed out at promotional events in Japan.";
			//regi.Requirements = "You must obtain the Heat Badge from Flannery in order to receive this Doll.";
			regi.AllowedGames = GameTypeFlags.Ruby | GameTypeFlags.Sapphire | GameTypeFlags.Emerald;
			RegisterEvent(regi);

			regi = new RegiDollEventDistribution("REGICE DOLL [RSE]");
			regi.DollID = 119;
			regi.SmallSprite = ItemDatabase.GetDecorationImageFromID(regi.DollID);
			regi.BigSprite = ItemDatabase.GetDecorationFullSizeImageFromID(regi.DollID);
			regi.Title = "e-Reader Regice Doll";
			regi.Description = "A large doll representing Regice obtained from an e-Reader card handed out at promotional events in Japan.";
			//regi.Requirements = "You must obtain the Heat Badge from Flannery in order to receive this Doll.";
			regi.AllowedGames = GameTypeFlags.Ruby | GameTypeFlags.Sapphire | GameTypeFlags.Emerald;
			RegisterEvent(regi);

			regi = new RegiDollEventDistribution("REGISTEEL DOLL [RSE]");
			regi.DollID = 120;
			regi.SmallSprite = ItemDatabase.GetDecorationImageFromID(regi.DollID);
			regi.BigSprite = ItemDatabase.GetDecorationFullSizeImageFromID(regi.DollID);
			regi.Title = "e-Reader Registeel Doll";
			regi.Description = "A large doll representing Registeel obtained from an e-Reader card handed out at promotional events in Japan.";
			//regi.Requirements = "You must obtain the Heat Badge from Flannery in order to receive this Doll.";
			regi.AllowedGames = GameTypeFlags.Ruby | GameTypeFlags.Sapphire | GameTypeFlags.Emerald;
			RegisterEvent(regi);

			if (IsAprilFoolsDay) {
				buyPokemon = new BuyPokemonEventDistribution("APRIL FOOLS ZUBAT [RS]");
				buyPokemon.Cost = 500;
				buyPokemon.DexID = 41;
				buyPokemon.Level = 5;
				buyPokemon.Move1ID = 18;
				buyPokemon.Move2ID = 48;
				buyPokemon.Move3ID = 0;
				buyPokemon.Move4ID = 0;
				buyPokemon.SmallSprite = PokemonDatabase.GetPokemonImageTypes(buyPokemon.DexID).NewBoxImage;
				buyPokemon.BigSprite = PokemonDatabase.GetPokemonImageTypes(buyPokemon.DexID).Image;
				buyPokemon.Title = "Zubat for Sale";
				buyPokemon.Description = "Confuse your enemies as you whirl them away!";
				buyPokemon.Requirements = "You must have $500 to buy this Pokémon.";
				buyPokemon.AllowedGames = GameTypeFlags.Ruby | GameTypeFlags.Sapphire;
				buyPokemon.CheckRequirements = new CheckEventRequirementsDelegate(delegate(IGameSave gameSave) {
					return gameSave.Money >= 500;
				});
				RegisterEvent(buyPokemon);

				buyPokemon = new BuyPokemonEventDistribution("APRIL FOOLS MAGIKARP [E]");
				buyPokemon.Cost = 500;
				buyPokemon.DexID = 129;
				buyPokemon.Level = 5;
				buyPokemon.Move1ID = 150;
				buyPokemon.Move2ID = 0;
				buyPokemon.Move3ID = 0;
				buyPokemon.Move4ID = 0;
				buyPokemon.SmallSprite = PokemonDatabase.GetPokemonImageTypes(buyPokemon.DexID).NewBoxImage;
				buyPokemon.BigSprite = PokemonDatabase.GetPokemonImageTypes(buyPokemon.DexID).Image;
				buyPokemon.Title = "Magikarp for Sale";
				buyPokemon.Description = "A wise investment.";
				buyPokemon.Requirements = "You must have $500 to buy this Pokémon.";
				buyPokemon.AllowedGames = GameTypeFlags.Emerald;
				buyPokemon.CheckRequirements = new CheckEventRequirementsDelegate(delegate(IGameSave gameSave) {
					return gameSave.Money >= 500;
				});
				RegisterEvent(buyPokemon);

				buyPokemon = new BuyPokemonEventDistribution("APRIL FOOLS RATTATA [FRLG]");
				buyPokemon.Cost = 500;
				buyPokemon.DexID = 19;
				buyPokemon.Level = 5;
				buyPokemon.Move1ID = 103;
				buyPokemon.Move2ID = 207;
				buyPokemon.Move3ID = 0;
				buyPokemon.Move4ID = 0;
				buyPokemon.SmallSprite = PokemonDatabase.GetPokemonImageTypes(buyPokemon.DexID).NewBoxImage;
				buyPokemon.BigSprite = PokemonDatabase.GetPokemonImageTypes(buyPokemon.DexID).Image;
				buyPokemon.Title = "Rattata for Sale";
				buyPokemon.Description = "It's only in the top percentage of Rattata!";
				buyPokemon.Requirements = "You must have $500 to buy this Pokémon.";
				buyPokemon.AllowedGames = GameTypeFlags.FireRed | GameTypeFlags.LeafGreen;
				buyPokemon.CheckRequirements = new CheckEventRequirementsDelegate(delegate(IGameSave gameSave) {
					return gameSave.Money >= 500;
				});
				RegisterEvent(buyPokemon);
			}
		}

		public static int NumEvents {
			get { return events.Count; }
		}

		public static List<PokerusStrain> PokerusStrains {
			get { return pokerusStrains; }
		}

		public static List<SharedSecretBase> SecretBases {
			get { return secretBases; }
		}
		public static void SortSecretBases() {
			secretBases.Sort((base1, base2) => (base1.LocationData.Order - base2.LocationData.Order));
		}
		public static SharedSecretBase AddSecretBase(SecretBase secretBase) {
			SharedSecretBase newSecretBase;
			if (secretBase.IsPlayerSecretBase)
				newSecretBase = new SharedSecretBase((PlayerSecretBase)secretBase, null);
			else
				newSecretBase = new SharedSecretBase((SharedSecretBase)secretBase, null);
			secretBases.Add(newSecretBase);
			SortSecretBases();
			managerGameSave.IsChanged = true;
			return newSecretBase;
		}

		public static EventDistribution GetEventAt(int index) {
			return events[index];
		}
		public static bool IsEventSoftCompletedBy(string eventID, IGameSave gameSave) {
			uint fullID = (uint)gameSave.TrainerID << 16 | (uint)gameSave.SecretID;

			foreach (uint completer in completedEvents[eventID]) {
				if (completer == fullID)
					return true;
			}
			return false;
		}

		public static bool IsEventCompletedBy(string eventID, IGameSave gameSave) {
			if (eventMap[eventID].IsCompleted(gameSave))
				return true;

			// Fix for incorrect flag with Old Sea Map event
			if (!(eventMap[eventID] is TicketEventDistribution)) {
				uint fullID = (uint)gameSave.TrainerID << 16 | (uint)gameSave.SecretID;

				foreach (uint completer in completedEvents[eventID]) {
					if (completer == fullID)
						return true;
				}
			}
			return false;
		}

		public static void RegisterEvent(EventDistribution eventDist) {
			events.Add(eventDist);
			eventMap.Add(eventDist.ID, eventDist);
			completedEvents.Add(eventDist.ID, new List<uint>());
		}
		public static bool HasEvent(string eventID) {
			return eventMap.ContainsKey(eventID);
		}
		public static void CompleteEventBy(string eventID, uint fullID) {
			if (!completedEvents[eventID].Contains(fullID))
				completedEvents[eventID].Add(fullID);
		}

		public static void CompleteEventBy(string eventID, IGameSave gameSave) {
			uint fullID = ((uint)gameSave.TrainerID << 16) | (uint)gameSave.SecretID;

			completedEvents[eventID].Add(fullID);
		}

		public static List<uint> GetCompletedEventsList(string eventID) {
			return completedEvents[eventID];
		}

		public static void GetAndSaveIndex(BitmapImage source, int index, string filePath) {
			int x = (index % 32) * 40 + 4;
			int y = (index / 32) * 30;
			Int32Rect sourceRect = new Int32Rect(x, y, 32, 30);
			SaveImage(GetImage(source, sourceRect), filePath);
		}

		public static WriteableBitmap GetImage(BitmapImage source, Int32Rect sourceRect) {
			int stride = (sourceRect.Width * source.Format.BitsPerPixel + 7) / 8;
			byte[] pixels = new byte[stride * sourceRect.Height];
			source.CopyPixels(sourceRect, pixels, stride, 0);
			WriteableBitmap bitmap = new WriteableBitmap(32, 32, source.DpiX, source.DpiY, source.Format, source.Palette);
			bitmap.WritePixels(new Int32Rect(0, 2, 32, 30), pixels, stride, 0);
			return bitmap;
		}
		public static void SaveImage(WriteableBitmap bitmap, string filePath) {
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate)) {
				encoder.Save(stream);
			}
		}

		public static bool GetNextCode(ref string text, out string word, ref int codeGroup, ref int codeGroupIndex) {
			StringBuilder builder = new StringBuilder();
			bool lastWasNewline = false;
			bool started = false;
			int index = 0;
			word = "";
			for (index = 0; index < text.Length; index++) {
				if (text[index] == '\n' || text[index] == '\r') {
					if (started)
						Console.WriteLine("UH OH");
					if (!lastWasNewline) {
						lastWasNewline = true;
						codeGroup++;
						codeGroupIndex = -1;
						Console.WriteLine("Code Group: " + codeGroup);
					}
				}
				else if (text[index] == '█') {
					if (!started)
						Console.WriteLine("UH OH");
					word = builder.ToString();
					text = text.Substring(index + 1);
					codeGroupIndex++;
					return true;
				}
				else if (text[index] != ' ' || started) {
					if (!started)
						started = true;
					builder.Append(text[index]);
				}
			}
			return false;
		}

		public static DifferenceData[] FindDifferenceData(byte[] data1, byte[] data2, int start = 0, int length = -1) {
			if (length == -1)
				length = data1.Length - start;
			List<DifferenceData> differenceData = new List<DifferenceData>();
			DifferenceData difference = null;
			for (int i = 0; i < length; i++) {
				if (data1[start + i] != data2[start + i]) {
					if (difference == null)
						difference = new DifferenceData(start + i);
					else
						difference.Length++;
				}
				else if (difference != null) {
					difference.Data1 = new DataString(data1, difference.StartIndex, difference.Length);
					difference.Data2 = new DataString(data2, difference.StartIndex, difference.Length);
					differenceData.Add(difference);
					difference = null;
				}
			}
			return differenceData.ToArray();
		}

		public static int SearchFor(byte[] dataToSearch, byte[] data) {
			for (int i = 0; i < dataToSearch.Length - data.Length; i++) {
				int j = 0;
				for (j = 0; j < data.Length; j++) {
					if (dataToSearch[i + j] != data[j])
						break;
				}
				if (j == data.Length)
					return i;
			}
			return -1;
		}
		public static KeyValuePair<SectionTypes, int> SearchFor(BlockDataCollection blocks, byte[] data) {
			foreach (IBlockData block in blocks) {
				if (block.SectionID >= SectionTypes.PCBufferA && block.SectionID <= SectionTypes.PCBufferI)
					continue;

				for (int i = 0; i < block.Raw.Length - data.Length; i++) {
					int j = 0;
					for (j = 0; j < data.Length; j++) {
						if (block.Raw[i + j] != data[j])
							break;
					}
					if (j == data.Length)
						return new KeyValuePair<SectionTypes, int>(block.SectionID, i);
				}
			}
			return new KeyValuePair<SectionTypes, int>(SectionTypes.PCBufferI, -1);
		}

		public static Dictionary<SectionTypes, List<DifferenceData>> FindDifferenceData(BlockDataCollection blocks1, BlockDataCollection blocks2) {
			Dictionary<SectionTypes, List<DifferenceData>> differenceData = new Dictionary<SectionTypes, List<DifferenceData>>();

			foreach (SectionTypes sectionType in Enum.GetValues(typeof(SectionTypes))) {
				if (sectionType >= SectionTypes.PCBufferA && sectionType <= SectionTypes.PCBufferI)
					continue;
				IBlockData least = blocks1.GetBlockData(sectionType);
				IBlockData most = blocks2.GetBlockData(sectionType);
				differenceData.Add(sectionType, new List<DifferenceData>());

				DifferenceData difference = null;
				for (int i = 0; i < SectionIDTable.GetContents(sectionType); i++) {
					if (least.Raw[i] != most.Raw[i]) {
						if (difference == null)
							difference = new DifferenceData(i);
						else
							difference.Length++;
					}
					else if (difference != null) {
						difference.Data1 = new DataString(least.Raw, difference.StartIndex, difference.Length);
						difference.Data2 = new DataString(most.Raw, difference.StartIndex, difference.Length);
						differenceData[sectionType].Add(difference);
						difference = null;
					}
				}
			}
			return differenceData;
		}

		#region Interface

		public static void CreateDirectories() {
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Trainer"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Pokemon"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Pokemon", "Cries"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Pokemon", "Front"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Pokemon", "Front", "Forms"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Pokemon", "FrontShiny"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Pokemon", "FrontShiny", "Forms"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Resources", "Wallpapers"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Gen 3"));
			TryCreateDirectory(Path.Combine(ApplicationDirectory, "Gen 3", "Backups"));
		}

		public static void TryCreateDirectory(string path) {
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		public static BitmapSource LoadImage(string filePath, bool handleError = false) {
			if (!File.Exists(filePath))
				return null;
			try {
				BitmapImage bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
				bitmap.UriSource = new Uri(filePath);
				bitmap.EndInit();

				double dpi = PokemonDatabase.GetPokemonImageTypes(1).Image.DpiX;
				int stride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
				byte[] pixelData = new byte[stride * bitmap.PixelHeight];
				bitmap.CopyPixels(pixelData, stride, 0);
				BitmapSource bitmapSource = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, dpi, dpi, bitmap.Format, bitmap.Palette, pixelData, stride);

				return bitmapSource;
			}
			catch (Exception ex) {
				if (handleError)
					return null;
				else
					throw;
			}
		}

		public static void SaveImage(BitmapSource bitmap, string filePath) {
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate)) {
				encoder.Save(stream);
			}
		}
		public static PokeManagerWindow ManagerWindow {
			get { return managerWindow; }
		}

		public static BitmapSource TrainerImage {
			get {
				if (customTrainerImage != null)
					return customTrainerImage;
				else
					return ResourceDatabase.GetImageFromName("Manager" + managerGameSave.TrainerGender.ToString());
			}
		}
		public static BitmapSource CustomTrainerImage {
			get { return customTrainerImage; }
			set { customTrainerImage = value; }
		}

		public static IGameSave LastGameInDialog {
			get { return lastGameInDialog; }
			set { lastGameInDialog = value; }
		}
		public static int LastGameInDialogIndex {
			get { return GetIndexOfGame(lastGameInDialog); }
			set { lastGameInDialog = GetGameSaveAt(value); }
		}
		public static bool IsChanged {
			get {
				if (managerGameSave.IsChanged)
					return true;
				foreach (GameSaveFileInfo gameSaveFile in gameSaveFiles) {
					if (gameSaveFile.GameSave.IsChanged)
						return true;
				}
				return false;
			}
		}
		public static bool IsNight {
			get { return DateTime.Now.Hour < 12; }
		}

		public static void ReloadRowComboBoxes() {
			managerWindow.ReloadRowComboBoxes();
		}

		/*public static void RefreshUI(UIModifyFlags flags) {
			managerWindow.RefreshUI(flags);
		}
		public static void Reload(UIModifyFlags flags) {
			managerWindow.Reload(flags);
		}*/

		public static void RefreshUI() {
			managerWindow.RefreshUI();
		}
		public static void Reload() {
			IsReloading = true;
			DropAll();
			ClearSelectedPokemon();
			managerWindow.FinishActions();
			LoadPokeManager();
			managerWindow.Reload();
			IsReloading = false;
		}
		public static void FinishActions() {
			DropAll();
			ClearSelectedPokemon();
			managerWindow.FinishActions();
			RefreshUI();
		}
		public static void SaveEverything() {
			try {
				FinishActions();

				List<string> missingsDirs = new List<string>();
				List<string> failedBackups = new List<string>();

				string backupDirectory = Path.Combine(ApplicationDirectory, "Gen 3", "Backups");
				if (!Directory.Exists(Path.Combine(ApplicationDirectory, "Gen 3")))
					Directory.CreateDirectory(Path.Combine(ApplicationDirectory, "Gen 3"));
				if (!Directory.Exists(Path.Combine(ApplicationDirectory, "Gen 3", "Backups")))
					Directory.CreateDirectory(Path.Combine(ApplicationDirectory, "Gen 3", "Backups"));

				// Remove the previous backups
				//if (settings.MakeBackups) {
					foreach (string file in Directory.GetFiles(backupDirectory))
						File.Delete(file);
				//}

				if (File.Exists(Path.Combine(ApplicationDirectory, "Gen 3", "YourPC.trigspc")))
					File.Copy(Path.Combine(ApplicationDirectory, "Gen 3", "YourPC.trigspc"), Path.Combine(ApplicationDirectory, "Gen 3", "Backups", "YourPC.trigspc"));

				bool backupFailedContinueAnyway = false;
				bool saveFailedContinueAnyway = false;

				foreach (GameSaveFileInfo gameSaveFile in gameSaveFiles) {
					IGameSave gameSave = gameSaveFile.GameSave;

					// Make a backup
					try {
						string name = Path.Combine(backupDirectory, Path.GetFileNameWithoutExtension(gameSaveFile.FilePath));
						string extension = Path.GetExtension(gameSaveFile.FilePath);
						int index = 1;
						while (File.Exists(name + (index != 1 ? " (" + index + ")" : "") + extension)) {
							index++;
						}
						string backupFilePath = name + (index != 1 ? " (" + index + ")" : "") + extension;
						File.Copy(gameSaveFile.FilePath, backupFilePath);
					}
					catch (Exception ex) {
						if (ex is DirectoryNotFoundException || ex is FileNotFoundException) {
							missingsDirs.Add(Path.GetDirectoryName(gameSaveFile.FilePath));
						}
						else {
							if (backupFailedContinueAnyway)
								continue;
							string nickname = (gameSaveFile.Nickname != "" ? gameSaveFile.Nickname : gameSave.GameType.ToString());
							MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to backup" + nickname + " [" + gameSave.TrainerName + "]. Would you like to see the error?", "Backup Error", MessageBoxButton.YesNo);
							if (result == MessageBoxResult.Yes)
								ErrorMessageBox.Show(ex);
							result = TriggerMessageBox.Show(managerWindow, "Would you like to continue saving?", "Continue Saving", MessageBoxButton.YesNo);
							if (result == MessageBoxResult.Yes)
								backupFailedContinueAnyway = true;
							else
								return;
						}
					}
				}
				if (missingsDirs.Count > 0) {
					string list = "";
					foreach (string dir in missingsDirs)
						list += "\n" + dir;
					MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "The following files for some games were missing and thus they cannot be backed up. Would you like to continue saving?\n" + list, "Missing Files", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.No)
						return;
					missingsDirs.Clear();
				}

				foreach (GameSaveFileInfo gameSaveFile in gameSaveFiles) {
					IGameSave gameSave = gameSaveFile.GameSave;

					// Don't waste time saving if we don't need to
					if (gameSave.IsChanged || settings.AlwaysSaveAllSaves) {
						if (!Directory.Exists(Path.GetDirectoryName(gameSaveFile.FilePath))) {
							missingsDirs.Add(Path.GetDirectoryName(gameSaveFile.FilePath));
						}
					}
				}
				if (missingsDirs.Count > 0) {
					string list = "";
					foreach (string dir in missingsDirs)
						list += "\n" + dir;
					MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "The following directories for some games were missing and thus they cannot be saved. Would you like to continue saving?\n" + list, "Missing Directories", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.No)
						return;
				}

				if (!SavePokeManager()) {
					MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Would you like to continue saving?", "Continue Saving", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.No)
						return;
				}
					
				foreach (GameSaveFileInfo gameSaveFile in gameSaveFiles) {
					IGameSave gameSave = gameSaveFile.GameSave;

					// Don't waste time saving if we don't need to
					if (gameSave.IsChanged || settings.AlwaysSaveAllSaves) {
						try {
							if (Directory.Exists(Path.GetDirectoryName(gameSaveFile.FilePath))) {
								gameSave.Save(gameSaveFile.FilePath);
								gameSaveFile.FileInfo = new FileInfo(gameSaveFile.FilePath);
								gameSave.IsChanged = false;
							}
						}
						catch (Exception ex) {
							if (saveFailedContinueAnyway)
								continue;
							string nickname = (gameSaveFile.Nickname != "" ? gameSaveFile.Nickname : gameSave.GameType.ToString());
							MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to save " + nickname + " [" + gameSave.TrainerName + "]. Would you like to see the error?", "Save Error", MessageBoxButton.YesNo);
							if (result == MessageBoxResult.Yes)
								ErrorMessageBox.Show(ex);
							result = TriggerMessageBox.Show(managerWindow, "Would you like to continue saving?", "Continue Saving", MessageBoxButton.YesNo);
							if (result == MessageBoxResult.Yes)
								saveFailedContinueAnyway = true;
							else
								return;
						}
					}
				}
			}
			catch (Exception ex) {
				MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while preparing to save changes. Would you like to see the error?", "Save Preparation Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex);
			}
		}
		public static void ReloadEverything() {
			IsReloading = true;
			DropAll();
			ClearSelectedPokemon();
			LoadPokeManager();
			managerWindow.Reload();
			IsReloading = false;
		}

		#endregion

		#region Basic

		public static PokeManagerSettings Settings {
			get { return settings; }
		}
		public static string ApplicationDirectory {
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		#endregion

		#region Pokemon Moving

		public static bool IsPokemonSelected(IPokemon pokemon) {
			for (int i = 0; i < selectedPokemon.Count; i++) {
				if (selectedPokemon[i].Pokemon == pokemon)
					return true;
			}
			return false;
		}

		public static void SelectPokemon(IPokemon pokemon) {
			if (!IsHoldingPokemon && !IsPokemonSelected(pokemon)) {
				selectedPokemon.Add(new PokemonLocation(pokemon));
			}
		}
		public static void UnselectPokemon(IPokemon pokemon) {
			if (!IsHoldingPokemon) {
				for (int i = 0; i < selectedPokemon.Count; i++) {
					if (selectedPokemon[i].Pokemon == pokemon) {
						selectedPokemon.RemoveAt(i);
						break;
					}
				}
			}
		}
		public static bool TogglePokemonSelection(IPokemon pokemon) {
			if (!IsHoldingPokemon) {
				for (int i = 0; i < selectedPokemon.Count; i++) {
					if (selectedPokemon[i].Pokemon == pokemon) {
						selectedPokemon.RemoveAt(i);
						return false;
					}
				}
				selectedPokemon.Add(new PokemonLocation(pokemon));
			}
			return true;
		}
		public static PokemonLocation GetSelectedPokemonAt(int index) {
			return selectedPokemon[index];
		}
		public static bool HasSelection {
			get { return selectedPokemon.Count > 0; }
		}
		public static int NumSelectedPokemon {
			get { return selectedPokemon.Count; }
		}
		public static void ClearSelectedPokemon() {
			if (!IsHoldingSelection)
				selectedPokemon.Clear();
			else
				throw new Exception("This shouldn't happen");
		}
		public static void MoveAllSelectedPokemonTo(IPokePC pokePC, int box, int index) {
			if (!IsHoldingPokemon && pokePC.HasRoomForPokemon(selectedPokemon.Count)) {
				foreach (IPokemon pokemon in selectedPokemon) {
					pokemon.PokeContainer.Remove(pokemon);
					pokePC.PlacePokemonInNextAvailableSlot(box, index, pokemon);
				}
				selectedPokemon.Clear();
			}
		}

		public static bool CanSafelyPlaceHeldUnknownItem(IPokeContainer container) {
			if (IsHoldingSingle) {
				return holdPokemon.Pokemon.HeldItemID == 0 || holdPokemon.Pokemon.HeldItemData.ID != 0 || holdPokemon.Pokemon.GameSave == container.GameSave;
			}
			else if (IsHoldingSelection) {
				foreach (PokemonLocation pokemon in selectedPokemon) {
					if (pokemon.Pokemon.HeldItemID != 0 && pokemon.Pokemon.HeldItemData.ID == 0 && pokemon.Pokemon.GameSave != container.GameSave)
						return false;
				}
			}
			return true;
		}
		public static bool IsHoldingPokemon {
			get { return holdPokemon != null; }
		}
		public static bool IsHoldingSelection {
			get { return IsHoldingPokemon && HasSelection; }
		}
		public static bool IsHoldingSingle {
			get { return IsHoldingPokemon && !HasSelection; }
		}
		public static PokemonLocation HoldPokemon {
			get { return holdPokemon; }
		}
		public static void PickupSelection(PokeBoxControl holdAdorner) {
			if (!IsHoldingPokemon && HasSelection) {
				// Sort selection
				selectedPokemon.Sort((pkm1, pkm2) => (
					((pkm1.Container is IPokeBox ? (int)((IPokeBox)pkm1.Container).BoxNumber : (int)pkm1.Container.PokePC.NumBoxes) * 30 + pkm1.Index) -
					((pkm2.Container is IPokeBox ? (int)((IPokeBox)pkm2.Container).BoxNumber : (int)pkm2.Container.PokePC.NumBoxes) * 30 + pkm2.Index)
				));
				holdPokemon = selectedPokemon[0];
				foreach (PokemonLocation pokemon in selectedPokemon) {
					pokemon.Pokemon.PokeContainer.Remove(pokemon.Pokemon);
					pokemon.Pokemon.IsMoving = true;
				}
				PokeManager.holdAdorner = holdAdorner;
				if (holdAdorner != null)
					holdAdorner.EnableAdorner();
			}
		}
		public static void PlaceSelection(IPokeContainer container, int index) {
			if (IsHoldingSelection && container.PokePC.HasRoomForPokemon(selectedPokemon.Count)) {
				foreach (PokemonLocation pokemon in selectedPokemon) {
					container.PokePC.PlacePokemonInNextAvailableSlot(container is IPokeBox ? (int)((IPokeBox)container).BoxNumber : -1, index, pokemon.Pokemon);
					pokemon.Pokemon.IsMoving = false;
				}
				for (int i = 0; i < selectedPokemon.Count; i++) {
					selectedPokemon[i] = new PokemonLocation(selectedPokemon[i].Pokemon.PokemonFinder.Pokemon);
				}
				holdPokemon = null;
				if (holdAdorner != null)
					holdAdorner.DisableAdorner();
			}
		}
		public static void DropSelection() {
			if (IsHoldingSelection) {
				foreach (PokemonLocation pokemon in selectedPokemon) {
					pokemon.Container[pokemon.Index] = pokemon.Pokemon;
					pokemon.Pokemon.IsMoving = false;
				}
				selectedPokemon.Clear();
				holdPokemon = null;
				if (holdAdorner != null)
					holdAdorner.DisableAdorner();
			}
		}
		public static bool IsPartyHoldingMail(IPokeContainer container) {
			if (container.Type == ContainerTypes.Party) {
				foreach (IPokemon partyPokemon in container) {
					if (partyPokemon.IsHoldingMail)
						return true;
				}
			}
			return false;
		}
		public static bool CanPickupPokemon(IPokemon pokemon) {
			if (pokemon.PokeContainer.Type == ContainerTypes.Party) {
				bool canPickup = false;
				foreach (IPokemon partyPokemon in pokemon.PokeContainer) {
					if (partyPokemon.IsHoldingMail)
						return false;
					if (!partyPokemon.IsEgg && partyPokemon != pokemon)
						canPickup = true;
				}
				return canPickup;
			}
			return true;
		}
		public static bool CanPlaceEgg(IPokeContainer container) {
			if (IsHoldingSelection) {
				if (container.GameType == GameTypes.Colosseum || container.GameType == GameTypes.XD) {
					foreach (PokemonLocation pokemon in selectedPokemon) {
						if (pokemon.Pokemon.IsEgg)
							return false;
					}
				}
				return true;
			}
			else if (IsHoldingSingle) {
				if (holdPokemon.Pokemon.IsEgg && (container.GameType == GameTypes.Colosseum || container.GameType == GameTypes.XD))
					return false;
				return true;
			}
			return false;
		}
		public static bool CanPlaceShadowPokemon(IPokeContainer container) {
			if (IsHoldingSelection) {
				foreach (PokemonLocation pokemon in selectedPokemon) {
					if (pokemon.Pokemon.IsShadowPokemon) {
						if (pokemon.Pokemon.GameSave != container.GameSave)
							return false;
					}
				}
				return true;
			}
			else if (IsHoldingSingle) {
				if (holdPokemon.Pokemon.IsShadowPokemon && holdPokemon.Pokemon.GameSave != container.GameSave)
					return false;
				return true;
			}
			return false;
		}
		public static bool CanSwitchPokemon(IPokemon pokemon) {
			if (IsHoldingPokemon) {
				if (pokemon.PokeContainer.Type == ContainerTypes.Party && holdPokemon.Pokemon.IsEgg) {
					bool canSwitch = false;
					foreach (IPokemon partyPokemon in pokemon.PokeContainer) {
						if (partyPokemon.IsHoldingMail)
							return false;
						if (!partyPokemon.IsEgg && partyPokemon != pokemon)
							canSwitch = true;
					}
					return canSwitch;
				}
				return true;
			}
			return false;
		}
		public static bool CanSwitchShadowPokemon(IPokemon pokemon) {
			if (IsHoldingPokemon) {
				bool needsNewLocation = pokemon.IsShadowPokemon && pokemon.GameSave != holdPokemon.Container.GameSave;
				for (int i = 0; i < pokemon.PokePC.NumBoxes && needsNewLocation; i++) {
					for (int j = 0; j < 30 && needsNewLocation; j++) {
						if (pokemon.PokePC[i][j] == null) {
							needsNewLocation = false;
							break;
						}
					}
				}
				return !needsNewLocation;
			}
			return false;
		}
		public static bool CanSwitchEgg(IPokemon pokemon) {
			if (IsHoldingPokemon) {
				bool needsNewLocation = pokemon.IsEgg && (holdPokemon.Container.GameType == GameTypes.Colosseum || holdPokemon.Container.GameType == GameTypes.XD);
				for (int i = 0; i < pokemon.PokePC.NumBoxes && needsNewLocation; i++) {
					for (int j = 0; j < 30 && needsNewLocation; j++) {
						if (pokemon.PokePC[i][j] == null) {
							needsNewLocation = false;
							break;
						}
					}
				}
				return !needsNewLocation;
			}
			return false;
		}
		public static void PickupPokemon(IPokemon pokemon, PokeBoxControl holdAdorner) {
			if (!IsHoldingPokemon && CanPickupPokemon(pokemon)) {
				selectedPokemon.Clear();
				holdPokemon = new PokemonLocation(pokemon);
				pokemon.PokeContainer.Remove(pokemon);
				pokemon.IsMoving = true;
				PokeManager.holdAdorner = holdAdorner;
				if (holdAdorner != null)
					holdAdorner.EnableAdorner();
			}
		}
		public static void SwitchPokemon(IPokemon pokemon) {
			if (IsHoldingSingle && CanSwitchPokemon(pokemon)) {
				PokemonLocation originalHoldPokemon = holdPokemon;
				pokemon.PokeContainer[pokemon.ContainerIndex] = holdPokemon.Pokemon;
				holdPokemon.Pokemon.IsMoving = false;
				pokemon.IsMoving = true;
				holdPokemon = new PokemonLocation(pokemon);
				holdPokemon.Container = originalHoldPokemon.Container;
				holdPokemon.Index = originalHoldPokemon.Index;
				// Try to make the pokemon drop in the current game. It's only required for shadow Pokemon though.
				bool needsNewLocation = (pokemon.IsShadowPokemon && pokemon.GameSave != holdPokemon.Container.GameSave) ||
					(pokemon.IsEgg && (holdPokemon.Container.GameType == GameTypes.Colosseum || holdPokemon.Container.GameType == GameTypes.XD));
				for (int i = 0; i < pokemon.PokePC.NumBoxes && needsNewLocation; i++) {
					for (int j = 0; j < 30 && needsNewLocation; j++) {
						if (pokemon.PokePC[i][j] == null) {
							holdPokemon.Container = pokemon.PokePC[i];
							holdPokemon.Index = j;
							needsNewLocation = false;
							break;
						}
					}
				}
				if (holdAdorner != null) {
					holdAdorner.DisableAdorner();
					holdAdorner.EnableAdorner();
				}
			}
		}
		public static void PlacePokemon(IPokeContainer container, int index) {
			if (IsHoldingSingle) {
				if (container[index] != null) {
					throw new Exception("Error cannot place Pokemon where one already exists");
				}
				else {
					container[index] = holdPokemon.Pokemon;
					holdPokemon.Pokemon.IsMoving = false;
					holdPokemon = null;
					if (holdAdorner != null)
						holdAdorner.DisableAdorner();
				}
			}
		}
		public static void DropPokemon() {
			if (IsHoldingSingle) {
				holdPokemon.Pokemon.IsMoving = false;
				if (holdPokemon.Container.Type == ContainerTypes.Party)
					((IPokeParty)holdPokemon.Container).AddPokemon(holdPokemon.Pokemon);
				else if (holdPokemon.Container.Type == ContainerTypes.Purifier && holdPokemon.Index > 0)
					((XDPurificationChamber)holdPokemon.Container).AddPokemon(holdPokemon.Pokemon);
				else
					holdPokemon.Container[holdPokemon.Index] = holdPokemon.Pokemon;
				holdPokemon = null;
				if (holdAdorner != null)
					holdAdorner.DisableAdorner();
			}
		}
		public static void DropAll() {
			if (IsHoldingSingle)
				DropPokemon();
			else if (IsHoldingSelection)
				DropSelection();
		}
		public static bool SelectionHasShadowPokemon {
			get {
				foreach (PokemonLocation pokemon in selectedPokemon) {
					if (pokemon.Pokemon.IsShadowPokemon)
						return true;
				}
				return false;
			}
		}
		public static bool SelectionHasEgg {
			get {
				foreach (PokemonLocation pokemon in selectedPokemon) {
					if (pokemon.Pokemon.IsEgg)
						return true;
				}
				return false;
			}
		}

		#endregion

		#region Manager Saving and Loading

		public static void LoadPokeManager() {
			managerGameSave = new ManagerGameSave();

			// Clear the completed events so that they can be repopulated by loading Trigger's PC
			foreach (KeyValuePair<string, List<uint>> pair in completedEvents) {
				pair.Value.Clear();
			}

			// Make sure the Gen 3 Directory exists
			if (!Directory.Exists(Path.Combine(ApplicationDirectory, "Gen 3")))
				Directory.CreateDirectory(Path.Combine(ApplicationDirectory, "Gen 3"));

			if (File.Exists(Path.Combine(ApplicationDirectory, "Gen 3", "YourPC.trigspc"))) {
				try {
					PokeManager.firstTimeStartingUp = false;
					if (loaded)
						LoadSettings();
					customTrainerImage = LoadImage(Path.Combine(ApplicationDirectory, "Resources", "Trainer", "Trainer.png"), true);
					try {
						byte[] data = File.ReadAllBytes(Path.Combine(ApplicationDirectory, "Gen 3", "YourPC.trigspc"));
						managerGameSave = new ManagerGameSave();
						managerGameSave.Load(data);
						try {
							LoadSecretBases();
						}
						catch (Exception ex) {
							MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Secret Bases. Would you like to view the error?", "Load Error", MessageBoxButton.YesNo);
							if (result == MessageBoxResult.Yes)
								ErrorMessageBox.Show(ex);
						}
					}
					catch (Exception ex) {
						MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Save File. Would you like to view the error?", "Load Error", MessageBoxButton.YesNo);
						if (result == MessageBoxResult.Yes)
							ErrorMessageBox.Show(ex);
					}
				}
				catch (Exception ex) {
					MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Settings. Would you like to view the error?", "Load Error", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes)
						ErrorMessageBox.Show(ex);
				}
			}
			else {
				try {
					LoadManager();
					try {
						LoadPokemon();
						try {
							LoadMailbox();
							try {
								if (loaded)
									LoadSettings();
							}
							catch (Exception ex) {
								TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Settings\n\nException: " + ex.Message, "Load Error");
							}
						}
						catch (Exception ex) {
							TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Mailbox\n\nException: " + ex.Message, "Load Error");
						}
					}
					catch (Exception ex) {
						TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Pokemon\n\nException: " + ex.Message, "Load Error");
					}
				}
				catch (Exception ex) {
					TriggerMessageBox.Show(managerWindow, "Error while trying to load Trigger's PC Info\n\nException: " + ex.Message, "Load Error");
				}
			}
			
			managerGameSave.IsLoaded = true;
		}
		public static bool SavePokeManager() {
			FinishActions();

			// Make sure the Gen 3 Directory exists
			if (!Directory.Exists(Path.Combine(ApplicationDirectory, "Gen 3")))
				Directory.CreateDirectory(Path.Combine(ApplicationDirectory, "Gen 3"));

			try {
				managerGameSave.Save(Path.Combine(ApplicationDirectory, "Gen 3", "YourPC.trigspc"));
				try {
					SaveSecretBases();
					if (SaveSettings())
						managerGameSave.IsChanged = false;

				}
				catch (Exception ex) {
					MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to save Trigger's PC Secret Bases. Would you like to view the error?", "Save Error", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes)
						ErrorMessageBox.Show(ex);
					return false;
				}
			}
			catch (Exception ex) {
				MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to save Trigger's PC Save File. Would you like to view the error?", "Save Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex);
				return false;
			}

			#region Old Saving

			/*try {
				SaveManager();
				try {
					SavePokemon();
					try {
						SaveMailbox();
						if (SaveSettings())
							managerGameSave.IsChanged = false;
					}
					catch (Exception ex) {
						TriggerMessageBox.Show(managerWindow, "Error while trying to save Trigger's PC Mailbox\n\nException: " + ex.Message, "Save Error");
					}
				}
				catch (Exception ex) {
					TriggerMessageBox.Show(managerWindow, "Error while trying to save Trigger's PC Pokemon\n\nException: " + ex.Message, "Save Error");
				}
			} catch (Exception ex) {
				TriggerMessageBox.Show(managerWindow, "Error while trying to save Trigger's PC Info\n\nException: " + ex.Message, "Save Error");
			}*/

			#endregion

			return true;
		}

		#region Secret Bases

		public static void LoadSecretBases() {
			secretBases = new List<SharedSecretBase>();
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "SecretBases.scrtbs");

			if (File.Exists(path)) {
				byte[] data = File.ReadAllBytes(path);

				uint version = LittleEndian.ToUInt32(data, 0);
				if (version >= 1) {
					int count = LittleEndian.ToSInt32(data, 4);
					for (int i = 0; i < count; i++) {
						secretBases.Add(new SharedSecretBase(ByteHelper.SubByteArray(8 + i * 160, data, 160), null));
					}
				}
			}
		}
		public static void SaveSecretBases() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "SecretBases.scrtbs");

			uint version = 1;
			List<byte> data = new List<byte>();
			data.AddRange(BitConverter.GetBytes(version));
			data.AddRange(BitConverter.GetBytes(secretBases.Count));
			foreach (SharedSecretBase secretBase in secretBases) {
				data.AddRange(secretBase.GetFinalData());
			}
			File.WriteAllBytes(path, data.ToArray());
		}

		#endregion

		#region Manager

		public static void LoadManager() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Manager.xml");

			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				#region Trainer
				XmlNodeList element = doc.GetElementsByTagName("TrainerName");
				if (element.Count != 0) managerGameSave.TrainerName = element[0].InnerText;

				element = doc.GetElementsByTagName("TrainerGender");
				if (element.Count != 0) managerGameSave.TrainerGender = (Genders)Enum.Parse(typeof(Genders), element[0].InnerText);

				element = doc.GetElementsByTagName("TrainerID");
				if (element.Count != 0) managerGameSave.TrainerID = ushort.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("SecretID");
				if (element.Count != 0) managerGameSave.SecretID = ushort.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("PlayTime");
				if (element.Count != 0) managerGameSave.PlayTime = TimeSpan.Parse(element[0].InnerText);

				managerGameSave.TimeOfLastSave = DateTime.Now;

				element = doc.GetElementsByTagName("PokedexSeen");
				bool makeOwned = doc.GetElementsByTagName("PokedexOwned").Count == 0;
				if (element.Count != 0) {
					string dexString = element[0].InnerText;
					for (int i = 0; i < dexString.Length; i++) {
						if (dexString[i] == '1') {
							managerGameSave.PokedexSeen[i] = true;
							// Support for an issue where saving wouldn't save owned stats
							if (makeOwned)
								managerGameSave.PokedexOwned[i] = true;
						}
					}
				}
				element = doc.GetElementsByTagName("PokedexOwned");
				if (element.Count != 0) {
					string dexString = element[0].InnerText;
					for (int i = 0; i < dexString.Length; i++) {
						if (dexString[i] == '1')
							managerGameSave.PokedexOwned[i] = true;
					}
				}

				element = doc.GetElementsByTagName("Money");
				if (element.Count != 0) managerGameSave.Money = uint.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("Coins");
				if (element.Count != 0) managerGameSave.Coins = uint.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("BattlePoints");
				if (element.Count != 0) managerGameSave.BattlePoints = uint.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("PokeCoupons");
				if (element.Count != 0) managerGameSave.PokeCoupons = uint.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("VolcanicAsh");
				if (element.Count != 0) managerGameSave.VolcanicAsh = uint.Parse(element[0].InnerText);
				#endregion

				managerGameSave.Inventory.Clear();

				#region Items
				XmlNodeList pocket = doc.SelectNodes("/PokeManager/ItemPockets/Items/Item");
				ItemPocket itemPocket = managerGameSave.Inventory.Items[ItemTypes.Items];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				pocket = doc.SelectNodes("/PokeManager/ItemPockets/PokeBalls/Item");
				itemPocket = managerGameSave.Inventory.Items[ItemTypes.PokeBalls];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				pocket = doc.SelectNodes("/PokeManager/ItemPockets/Berries/Item");
				itemPocket = managerGameSave.Inventory.Items[ItemTypes.Berries];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				pocket = doc.SelectNodes("/PokeManager/ItemPockets/TMs/Item");
				itemPocket = managerGameSave.Inventory.Items[ItemTypes.TMCase];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				pocket = doc.SelectNodes("/PokeManager/ItemPockets/KeyItems/Item");
				itemPocket = managerGameSave.Inventory.Items[ItemTypes.KeyItems];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				pocket = doc.SelectNodes("/PokeManager/ItemPockets/CologneCase/Item");
				itemPocket = managerGameSave.Inventory.Items[ItemTypes.CologneCase];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				pocket = doc.SelectNodes("/PokeManager/ItemPockets/DiscCase/Item");
				itemPocket = managerGameSave.Inventory.Items[ItemTypes.DiscCase];
				for (int i = 0; i < pocket.Count; i++) {
					itemPocket.AddItem(UInt16.Parse(pocket[i].Attributes["ID"].Value), UInt32.Parse(pocket[i].Attributes["Count"].Value));
				}
				#endregion

				#region Decorations
				XmlNodeList container = doc.SelectNodes("/PokeManager/Decorations/Desks/Decoration");
				DecorationPocket decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Desk];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Chairs/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Chair];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Plants/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Plant];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Ornaments/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Ornament];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Mats/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Mat];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Posters/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Poster];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Dolls/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Doll];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				container = doc.SelectNodes("/PokeManager/Decorations/Cushions/Decoration");
				decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Cushion];
				for (int i = 0; i < container.Count; i++) {
					decorationContainer.AddDecoration(Byte.Parse(container[i].Attributes["ID"].Value), UInt32.Parse(container[i].Attributes["Count"].Value));
				}
				#endregion

				#region Pokeblocks
				XmlNodeList pokeblocks = doc.SelectNodes("/PokeManager/Pokeblocks/Pokeblock");
				PokeblockCase blockCase = managerGameSave.Inventory.Pokeblocks;
				for (int i = 0; i < pokeblocks.Count; i++) {
					blockCase.AddPokeblock(
						(PokeblockColors)Enum.Parse(typeof(PokeblockColors), pokeblocks[i].Attributes["Color"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Spicy"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Dry"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Sweet"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Bitter"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Sour"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Feel"].Value),
						Byte.Parse(pokeblocks[i].Attributes["Unknown"].Value)
					);
				}
				#endregion
			}
		}

		/*public static void SaveManager() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Manager.xml");

			XmlDocument doc = new XmlDocument();
			doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

			XmlElement pokeManager = doc.CreateElement("PokeManager");
			doc.AppendChild(pokeManager);

			XmlElement version = doc.CreateElement("Version");
			version.AppendChild(doc.CreateTextNode("1"));
			pokeManager.AppendChild(version);

			#region Trainer
			XmlElement element = doc.CreateElement("TrainerName");
			element.AppendChild(doc.CreateTextNode(managerGameSave.TrainerName));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("TrainerGender");
			element.AppendChild(doc.CreateTextNode(managerGameSave.TrainerGender.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("TrainerID");
			element.AppendChild(doc.CreateTextNode(managerGameSave.TrainerID.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("SecretID");
			element.AppendChild(doc.CreateTextNode(managerGameSave.SecretID.ToString()));
			pokeManager.AppendChild(element);

			// Update the play time
			managerGameSave.PlayTime += new TimeSpan((DateTime.Now - managerGameSave.TimeOfLastSave).Ticks);
			managerGameSave.TimeOfLastSave = DateTime.Now;

			element = doc.CreateElement("PlayTime");
			element.AppendChild(doc.CreateTextNode(managerGameSave.PlayTime.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("PokedexSeen");
			StringBuilder dexString = new StringBuilder();
			for (int i = 0; i < 386; i++)
				dexString.Append(managerGameSave.PokedexSeen[i] ? "1" : "0");
			element.AppendChild(doc.CreateTextNode(dexString.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("PokedexOwned");
			dexString = new StringBuilder();
			for (int i = 0; i < 386; i++)
				dexString.Append(managerGameSave.PokedexOwned[i] ? "1" : "0");
			element.AppendChild(doc.CreateTextNode(dexString.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("Money");
			element.AppendChild(doc.CreateTextNode(managerGameSave.Money.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("Coins");
			element.AppendChild(doc.CreateTextNode(managerGameSave.Coins.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("BattlePoints");
			element.AppendChild(doc.CreateTextNode(managerGameSave.BattlePoints.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("PokeCoupons");
			element.AppendChild(doc.CreateTextNode(managerGameSave.PokeCoupons.ToString()));
			pokeManager.AppendChild(element);

			element = doc.CreateElement("VolcanicAsh");
			element.AppendChild(doc.CreateTextNode(managerGameSave.VolcanicAsh.ToString()));
			pokeManager.AppendChild(element);
			#endregion

			#region Items
			XmlElement items = doc.CreateElement("ItemPockets");
			pokeManager.AppendChild(items);

			XmlElement pocket = doc.CreateElement("Items");
			items.AppendChild(pocket);
			ItemPocket itemPocket = managerGameSave.Inventory.Items[ItemTypes.Items];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			pocket = doc.CreateElement("PokeBalls");
			items.AppendChild(pocket);
			itemPocket = managerGameSave.Inventory.Items[ItemTypes.PokeBalls];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			pocket = doc.CreateElement("Berries");
			items.AppendChild(pocket);
			itemPocket = managerGameSave.Inventory.Items[ItemTypes.Berries];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			pocket = doc.CreateElement("TMs");
			items.AppendChild(pocket);
			itemPocket = managerGameSave.Inventory.Items[ItemTypes.TMCase];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			pocket = doc.CreateElement("KeyItems");
			items.AppendChild(pocket);
			itemPocket = managerGameSave.Inventory.Items[ItemTypes.KeyItems];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			pocket = doc.CreateElement("CologneCase");
			items.AppendChild(pocket);
			itemPocket = managerGameSave.Inventory.Items[ItemTypes.CologneCase];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			pocket = doc.CreateElement("DiscCase");
			items.AppendChild(pocket);
			itemPocket = managerGameSave.Inventory.Items[ItemTypes.DiscCase];
			for (int i = 0; i < itemPocket.SlotsUsed; i++) {
				Item item = itemPocket[i];
				XmlElement listItem = doc.CreateElement("Item");
				listItem.SetAttribute("ID", item.ID.ToString());
				listItem.SetAttribute("Count", item.Count.ToString());
				pocket.AppendChild(listItem);
			}
			#endregion

			#region Decorations
			XmlElement decorations = doc.CreateElement("Decorations");
			pokeManager.AppendChild(decorations);

			XmlElement container = doc.CreateElement("Desks");
			decorations.AppendChild(container);
			DecorationPocket decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Desk];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Chairs");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Chair];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Plants");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Plant];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Ornaments");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Ornament];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Mats");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Mat];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Posters");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Poster];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Dolls");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Doll];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}

			container = doc.CreateElement("Cushions");
			decorations.AppendChild(container);
			decorationContainer = managerGameSave.Inventory.Decorations[DecorationTypes.Cushion];
			for (int i = 0; i < decorationContainer.SlotsUsed; i++) {
				Decoration decoration = decorationContainer[i];
				XmlElement listDecoration = doc.CreateElement("Decoration");
				listDecoration.SetAttribute("ID", decoration.ID.ToString());
				listDecoration.SetAttribute("Count", decoration.Count.ToString());
				container.AppendChild(listDecoration);
			}
			#endregion

			#region Pokeblocks
			XmlElement pokeblocks = doc.CreateElement("Pokeblocks");
			pokeManager.AppendChild(pokeblocks);
			for (int i = 0; i < managerGameSave.Inventory.Pokeblocks.SlotsUsed; i++) {
				Pokeblock pokeblock = managerGameSave.Inventory.Pokeblocks[i];
				XmlElement listPokeblock = doc.CreateElement("Pokeblock");
				listPokeblock.SetAttribute("Color", pokeblock.Color.ToString());
				listPokeblock.SetAttribute("Spicy", pokeblock.Spicyness.ToString());
				listPokeblock.SetAttribute("Dry", pokeblock.Dryness.ToString());
				listPokeblock.SetAttribute("Sweet", pokeblock.Sweetness.ToString());
				listPokeblock.SetAttribute("Bitter", pokeblock.Bitterness.ToString());
				listPokeblock.SetAttribute("Sour", pokeblock.Sourness.ToString());
				listPokeblock.SetAttribute("Feel", pokeblock.Feel.ToString());
				listPokeblock.SetAttribute("Unknown", pokeblock.Unknown.ToString());
				pokeblocks.AppendChild(listPokeblock);
			}
			#endregion

			doc.Save(path);
		}*/

		#endregion

		#region Pokemon

		public static void LoadPokemon() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Pokemon.boxes");

			if (File.Exists(path)) {
				byte[] data = File.ReadAllBytes(path);
				((ManagerPokePC)managerGameSave.PokePC).Load(data);
			}
			else {
				((ManagerPokePC)managerGameSave.PokePC).Clear();
			}
		}
		public static void SavePokemon() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Pokemon.boxes");

			File.WriteAllBytes(path, ((ManagerPokePC)managerGameSave.PokePC).GetFinalData());
		}

		#endregion

		#region Mailbox

		public static void LoadMailbox() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Mail.box");

			if (File.Exists(path)) {
				byte[] data = File.ReadAllBytes(path);
				managerGameSave.Mailbox.Load(data);
			}
			else {
				managerGameSave.Mailbox.Reset();
			}
		}
		public static void SaveMailbox() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Mail.box");

			File.WriteAllBytes(path, managerGameSave.Mailbox.GetFinalData());
		}

		#endregion

		#region Settings

		public static void LoadSettings() {
			string path = Path.Combine(ApplicationDirectory, "Gen 3", "Settings.xml");

			PokeManager.settings = new PokeManagerSettings();
			PokeManager.gameSaveFiles = new List<GameSaveFileInfo>();
			PokeManager.missingGameSaveFiles = new List<GameSaveFileInfo>();
			if (File.Exists(path)) {
				PokeManager.firstTimeStartingUp = false;

				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				#region Settings
				settings = new PokeManagerSettings();
				settings.DisableChangesWhileLoading = true;

				XmlNodeList element = doc.GetElementsByTagName("ManagerNickname");
				if (element.Count != 0) settings.ManagerNickname = element[0].InnerText;

				element = doc.GetElementsByTagName("UsedFrontSpritesType");
				if (element.Count != 0) settings.UsedFrontSpritesType = (FrontSpriteSelectionTypes)Enum.Parse(typeof(FrontSpriteSelectionTypes), element[0].InnerText);

				element = doc.GetElementsByTagName("UseNewBoxSprites");
				if (element.Count != 0) settings.UseNewBoxSprites = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("MakeBackups");
				if (element.Count != 0) settings.MakeBackups = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("CloseConfirmation");
				if (element.Count != 0) settings.CloseConfirmation = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("Volume");
				if (element.Count != 0) settings.Volume = double.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("Muted");
				if (element.Count != 0) settings.IsMuted = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("FrontSpriteRegularSelections");
				if (element.Count != 0) {
					string spriteString = element[0].InnerText;
					for (int i = 0; i < spriteString.Length; i++) {
						if (spriteString[i] == '1')
							PokeManager.Settings.FrontSpriteSelections[0, i] = FrontSpriteSelectionTypes.FRLG;
						else if (spriteString[i] == '2')
							PokeManager.Settings.FrontSpriteSelections[0, i] = FrontSpriteSelectionTypes.Custom;
					}
				}
				element = doc.GetElementsByTagName("FrontSpriteShinySelections");
				if (element.Count != 0) {
					string spriteString = element[0].InnerText;
					for (int i = 0; i < spriteString.Length; i++) {
						if (spriteString[i] == '1')
							PokeManager.Settings.FrontSpriteSelections[1, i] = FrontSpriteSelectionTypes.FRLG;
						else if (spriteString[i] == '2')
							PokeManager.Settings.FrontSpriteSelections[1, i] = FrontSpriteSelectionTypes.Custom;
					}
				}

				element = doc.GetElementsByTagName("UseDifferentShinyFrontSprites");
				if (element.Count != 0) settings.UseDifferentShinyFrontSprites = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("NumBoxRows");
				if (element.Count != 0) settings.NumBoxRows = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("AllowDoubleBoxRows");
				if (element.Count != 0) settings.AllowDoubleBoxRows = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("MysteryEggs");
				if (element.Count != 0) settings.MysteryEggs = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("AlwaysSaveAllSaves");
				if (element.Count != 0) settings.AlwaysSaveAllSaves = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("StartupMirageIslandCheck");
				if (element.Count != 0) settings.StartupMirageIslandCheck = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("StartupShinyEggsCheck");
				if (element.Count != 0) settings.StartupShinyEggsCheck = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("StartupPokerusCheck");
				if (element.Count != 0) settings.StartupPokerusCheck = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("ShowEggSpeciesInBox");
				if (element.Count != 0) settings.RevealEggs = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("AutoSortYourPCItems");
				if (element.Count != 0) settings.AutoSortItems = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultStartupSize");
				if (element.Count != 0) settings.DefaultStartupSize = Size.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultStartupTab");
				if (element.Count != 0) settings.DefaultStartupTab = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultGame");
				if (element.Count != 0) settings.DefaultGame = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultBoxRow1");
				if (element.Count != 0) settings.DefaultBoxRow1 = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultBoxGame2");
				if (element.Count != 0) settings.DefaultBoxGame2 = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultBoxRow2");
				if (element.Count != 0) settings.DefaultBoxRow2 = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultBoxGame3");
				if (element.Count != 0) settings.DefaultBoxGame3 = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DefaultBoxRow3");
				if (element.Count != 0) settings.DefaultBoxRow3 = int.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("KeepMissingFiles");
				if (element.Count != 0) settings.KeepMissingFiles = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("AprilFoolsEnabled");
				if (element.Count != 0) settings.AprilFoolsEnabled = bool.Parse(element[0].InnerText);

				// Hidden Settings
				element = doc.GetElementsByTagName("ForceAprilFools");
				if (element.Count != 0) settings.ForceAprilFools = bool.Parse(element[0].InnerText);

				element = doc.GetElementsByTagName("DebugMode");
				if (element.Count != 0) settings.DebugMode = bool.Parse(element[0].InnerText);

				settings.DisableChangesWhileLoading = false;
				#endregion

				#region Save Files
				XmlNodeList saveList = doc.SelectNodes("/PokeManager/SaveFiles/SaveFile");
				for (int i = 0; i < saveList.Count; i++) {
					string filePath = (saveList[i].Attributes["FilePath"] != null ? saveList[i].Attributes["FilePath"].Value : "");
					string nickname = (saveList[i].Attributes["Nickname"] != null ? saveList[i].Attributes["Nickname"].Value : "");
					GameTypes gameType = GameTypes.Any;
					if (saveList[i].Attributes["GameType"] != null)
						Enum.TryParse<GameTypes>(saveList[i].Attributes["GameType"].Value, out gameType);
					bool japanese = false;
					if (saveList[i].Attributes["Japanese"] != null)
						bool.TryParse(saveList[i].Attributes["Japanese"].Value, out japanese);
					bool livingDex = false;
					if (saveList[i].Attributes["LivingDex"] != null)
						bool.TryParse(saveList[i].Attributes["LivingDex"].Value, out livingDex);
					if (filePath != "")
						gameSaveFiles.Add(new GameSaveFileInfo(filePath, gameType, nickname, japanese, livingDex));
				}

				List<string> missingFiles = new List<string>();
				List<string> errorFiles = new List<string>();
				List<GameSaveFileInfo> gameSavesToRemove = new List<GameSaveFileInfo>();

				// Load em' up
				foreach (GameSaveFileInfo gameSaveFile in gameSaveFiles) {
					if (File.Exists(gameSaveFile.FilePath)) {
						try {
							gameSaveFile.GameSave = LoadGameSave(gameSaveFile.FilePath, gameSaveFile.GameType, gameSaveFile.IsJapanese);
						}
						catch (Exception) {
							errorFiles.Add(gameSaveFile.FilePath);
							gameSavesToRemove.Add(gameSaveFile);
						}
					}
					else {
						missingGameSaveFiles.Add(gameSaveFile);
						missingFiles.Add(gameSaveFile.FilePath);
						gameSavesToRemove.Add(gameSaveFile);
					}
				}
				if (missingFiles.Count > 0 && !settings.KeepMissingFiles) {
					string list = "";
					foreach (string file in missingFiles)
						list += "\n" + file;
					TriggerMessageBox.Show(managerWindow, "The following game files no longer exist and have been removed from your list\n" + list, "Missing Files");
				}
				if (errorFiles.Count > 0) {
					string list = "";
					foreach (string file in errorFiles)
						list += "\n" + file;
					TriggerMessageBox.Show(managerWindow, "The following game files no had errors when loading and have been removed from your list\n" + list, "Error Loading");
				}
				foreach (GameSaveFileInfo gameSaveFile in gameSavesToRemove)
					gameSaveFiles.Remove(gameSaveFile);
				#endregion
			}
		}

		public static bool SaveSettings() {
			try {
				string path = Path.Combine(ApplicationDirectory, "Gen 3", "Settings.xml");

				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

				XmlElement pokeManager = doc.CreateElement("PokeManager");
				doc.AppendChild(pokeManager);

				XmlElement version = doc.CreateElement("Version");
				version.AppendChild(doc.CreateTextNode("1"));
				pokeManager.AppendChild(version);

				#region Settings
				XmlElement setting = doc.CreateElement("Settings");
				pokeManager.AppendChild(setting);

				XmlElement element = doc.CreateElement("ManagerNickname");
				element.AppendChild(doc.CreateTextNode(settings.ManagerNickname));
				setting.AppendChild(element);

				element = doc.CreateElement("UsedFrontSpritesType");
				element.AppendChild(doc.CreateTextNode(settings.UsedFrontSpritesType.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("UseNewBoxSprites");
				element.AppendChild(doc.CreateTextNode(settings.UseNewBoxSprites.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("MakeBackups");
				element.AppendChild(doc.CreateTextNode(settings.MakeBackups.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("CloseConfirmation");
				element.AppendChild(doc.CreateTextNode(settings.CloseConfirmation.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("Volume");
				element.AppendChild(doc.CreateTextNode(settings.Volume.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("Muted");
				element.AppendChild(doc.CreateTextNode(settings.IsMuted.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("FrontSpriteRegularSelections");
				StringBuilder spriteString = new StringBuilder();
				for (int i = 0; i < 386; i++)
					spriteString.Append(((int)PokeManager.Settings.FrontSpriteSelections[0, i]).ToString());
				element.AppendChild(doc.CreateTextNode(spriteString.ToString()));
				setting.AppendChild(element);
				element = doc.CreateElement("FrontSpriteShinySelections");
				spriteString = new StringBuilder();
				for (int i = 0; i < 386; i++)
					spriteString.Append(((int)PokeManager.Settings.FrontSpriteSelections[1, i]).ToString());
				element.AppendChild(doc.CreateTextNode(spriteString.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("UseDifferentShinyFrontSprites");
				element.AppendChild(doc.CreateTextNode(settings.UseDifferentShinyFrontSprites.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("NumBoxRows");
				element.AppendChild(doc.CreateTextNode(settings.NumBoxRows.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("AllowDoubleBoxRows");
				element.AppendChild(doc.CreateTextNode(settings.AllowDoubleBoxRows.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("MysteryEggs");
				element.AppendChild(doc.CreateTextNode(settings.MysteryEggs.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("AlwaysSaveAllSaves");
				element.AppendChild(doc.CreateTextNode(settings.AlwaysSaveAllSaves.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("StartupMirageIslandCheck");
				element.AppendChild(doc.CreateTextNode(settings.StartupMirageIslandCheck.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("StartupShinyEggsCheck");
				element.AppendChild(doc.CreateTextNode(settings.StartupShinyEggsCheck.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("StartupPokerusCheck");
				element.AppendChild(doc.CreateTextNode(settings.StartupPokerusCheck.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("ShowEggSpeciesInBox");
				element.AppendChild(doc.CreateTextNode(settings.RevealEggs.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("AutoSortYourPCItems");
				element.AppendChild(doc.CreateTextNode(settings.AutoSortItems.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultStartupSize");
				element.AppendChild(doc.CreateTextNode(settings.DefaultStartupSize.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultStartupTab");
				element.AppendChild(doc.CreateTextNode(settings.DefaultStartupTab.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultGame");
				element.AppendChild(doc.CreateTextNode(settings.DefaultGame.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultBoxRow1");
				element.AppendChild(doc.CreateTextNode(settings.DefaultBoxRow1.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultBoxGame2");
				element.AppendChild(doc.CreateTextNode(settings.DefaultBoxGame2.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultBoxRow2");
				element.AppendChild(doc.CreateTextNode(settings.DefaultBoxRow2.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultBoxGame3");
				element.AppendChild(doc.CreateTextNode(settings.DefaultBoxGame3.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("DefaultBoxRow3");
				element.AppendChild(doc.CreateTextNode(settings.DefaultBoxRow3.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("KeepMissingFiles");
				element.AppendChild(doc.CreateTextNode(settings.KeepMissingFiles.ToString()));
				setting.AppendChild(element);

				element = doc.CreateElement("AprilFoolsEnabled");
				element.AppendChild(doc.CreateTextNode(settings.AprilFoolsEnabled.ToString()));
				setting.AppendChild(element);

				// Hidden Settings
				if (settings.ForceAprilFools) {
					element = doc.CreateElement("ForceAprilFools");
					element.AppendChild(doc.CreateTextNode(settings.ForceAprilFools.ToString()));
					setting.AppendChild(element);
				}
				if (settings.DebugMode) {
					element = doc.CreateElement("DebugMode");
					element.AppendChild(doc.CreateTextNode(settings.DebugMode.ToString()));
					setting.AppendChild(element);
				}
				#endregion

				#region SaveFiles
				XmlElement saveFiles = doc.CreateElement("SaveFiles");
				pokeManager.AppendChild(saveFiles);

				foreach (GameSaveFileInfo gameSaveFile in gameSaveFiles) {
					XmlElement listSave = doc.CreateElement("SaveFile");
					listSave.SetAttribute("GameType", gameSaveFile.GameType.ToString());
					listSave.SetAttribute("Nickname", gameSaveFile.Nickname);
					listSave.SetAttribute("Japanese", gameSaveFile.IsJapanese.ToString());
					listSave.SetAttribute("LivingDex", gameSaveFile.IsLivingDex.ToString());
					listSave.SetAttribute("FilePath", gameSaveFile.FilePath);
					saveFiles.AppendChild(listSave);
				}
				if (settings.KeepMissingFiles) {
					foreach (GameSaveFileInfo gameSaveFile in missingGameSaveFiles) {
						XmlElement listSave = doc.CreateElement("SaveFile");
						listSave.SetAttribute("GameType", gameSaveFile.GameType.ToString());
						listSave.SetAttribute("Nickname", gameSaveFile.Nickname);
						listSave.SetAttribute("Japanese", gameSaveFile.IsJapanese.ToString());
						listSave.SetAttribute("LivingDex", gameSaveFile.IsLivingDex.ToString());
						listSave.SetAttribute("FilePath", gameSaveFile.FilePath);
						saveFiles.AppendChild(listSave);
					}
				}
				#endregion

				doc.Save(path);
				return true;
			}
			catch (Exception ex) {
				MessageBoxResult result = TriggerMessageBox.Show(managerWindow, "Error while trying to save Trigger's PC Settings. Would you like to see the error?", "Save Error", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex);
			}
			return false;
		}

		#endregion

		#endregion

		#region Game Saves

		public static ManagerGameSave ManagerGameSave {
			get { return managerGameSave; }
		}
		public static int NumGameSaves {
			get {
				if (gameSaveFiles == null)
					return 0;
				return gameSaveFiles.Count;
			}
		}
		public static GameSaveFileInfo LastGameSaveFileInfo {
			get { return gameSaveFiles[gameSaveFiles.Count - 1]; }
		}
		public static IGameSave LastGameSave {
			get { return gameSaveFiles[gameSaveFiles.Count - 1].GameSave; }
		}
		public static GameSaveFileInfo GetGameSaveFileInfoAt(int gameIndex) {
			if (gameIndex >= 0 && gameIndex < gameSaveFiles.Count)
				return gameSaveFiles[gameIndex];
			return null;
		}
		public static IGameSave GetGameSaveAt(int gameIndex) {
			if (gameIndex == -1)
				return managerGameSave;
			else if (gameIndex >= 0 && gameIndex < gameSaveFiles.Count)
				return gameSaveFiles[gameIndex].GameSave;
			return null;
		}
		public static int GetIndexOfGame(IGameSave gameSave) {
			if (managerGameSave == gameSave)
				return -1;
			for (int i = 0; i < gameSaveFiles.Count; i++) {
				if (gameSaveFiles[i].GameSave == gameSave)
					return i;
			}
			return -2;
		}

		public static GameSaveFileInfo MakeNewGameSaveFileInfo(string filePath, GameTypes gameType, bool japanese) {
			IGameSave gameSave = LoadGameSave(filePath, gameType, japanese);
			GameSaveFileInfo gameSaveFile = new GameSaveFileInfo(filePath, gameSave.GameType, "", japanese);
			gameSaveFile.GameSave = gameSave;
			return gameSaveFile;
		}
		private static IGameSave LoadGameSave(string filePath, GameTypes gameType = GameTypes.Any, bool japanese = false) {
			if (File.Exists(filePath)) {
				long length = new FileInfo(filePath).Length;
				if (length == 483328 || length == 483392)
					return new PokemonBoxGameSave(filePath, japanese);
				else if (length == 352256 || length == 352320 || length == 393216 || length == 393280)
					return new GCGameSave(filePath, japanese);
				else if (length == 131072 || length == 65536 || length == 139264)
					return new GBAGameSave(filePath, gameType, japanese);
				else
					throw new Exception("File size does not match any known Pokemon game");
			}
			return null;
		}

		public static void SetGameTypeOfGameSaveAt(int gameIndex, GameTypes gameType) {
			if (gameSaveFiles[gameIndex].GameSave is GBAGameSave) {
				gameSaveFiles[gameIndex].GameType = gameType;
				((GBAGameSave)gameSaveFiles[gameIndex].GameSave).GameType = gameType;
				RefreshUI();
			}
		}

		public static void SetGameSaveFileInfoNickname(int gameIndex, string nickname) {
			if (gameIndex == -1)
				settings.ManagerNickname = nickname;
			else
				gameSaveFiles[gameIndex].Nickname = nickname;
		}
		public static string GetGameSaveFileInfoNickname(int gameIndex) {
			if (gameIndex == -1)
				return settings.ManagerNickname;
			else
				return gameSaveFiles[gameIndex].Nickname;
		}
		public static void SetGameSaveFileInfoList(GameSaveFileInfo[] newGameSaveFiles) {
			gameSaveFiles.Clear();
			gameSaveFiles.AddRange(newGameSaveFiles);
			SaveSettings();
			managerWindow.ReloadGames();
		}

		public static void ReloadGameSave(GameSaveFileInfo gameSaveFile) {
			gameSaveFile.GameSave = LoadGameSave(gameSaveFile.FilePath, gameSaveFile.GameType);
			gameSaveFile.FileInfo = new FileInfo(gameSaveFile.FilePath);
		}
		public static void ReloadGameSaveAt(int index) {
			FinishActions();
			GameSaveFileInfo gameSaveFile = gameSaveFiles[index];
			gameSaveFile.GameSave = LoadGameSave(gameSaveFile.FilePath, gameSaveFile.GameType);
			gameSaveFile.FileInfo = new FileInfo(gameSaveFile.FilePath);
		}

		#endregion

		#region Junk

		public static string ChangeStringCasing(string text) {
			StringBuilder builder = new StringBuilder(text);
			bool capitalize = true;
			for (int i = 0; i < text.Length; i++) {
				if (char.IsWhiteSpace(text[i]) || text[i] == '-' || text[i] == '/') {
					capitalize = true;
					continue;
				}
				if (capitalize && char.IsLetterOrDigit(text[i])) {
					builder[i] = char.ToUpper(text[i]);
					capitalize = false;
				}
				else {
					builder[i] = char.ToLower(text[i]);
				}
			}
			return builder.ToString();
		}

		#endregion
	}
}
