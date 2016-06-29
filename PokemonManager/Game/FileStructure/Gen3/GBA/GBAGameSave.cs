using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class GBAGameSave : IGameSave {

		#region Members

		private GBASaveData[] saveData;
		private bool is128KbSave;
		private byte[] raw;
		private GameTypes gameType;
		private bool changed;
		private bool loaded;
		private bool japanese;

		#endregion

		public GBAGameSave(string filePath, GameTypes gameType = GameTypes.Any, bool japanese = false) {
			this.gameType	= gameType;
			this.japanese	= japanese;
			//try {
				this.raw = File.ReadAllBytes(filePath);
				this.saveData = new GBASaveData[2];
				if (raw.Length == 131072 || raw.Length == 139264) {
					if (!ByteHelper.CompareBytes(0, ByteHelper.SubByteArray(4092, raw, 4)))
						saveData[0] = new GBASaveData(this, ByteHelper.SubByteArray(0, raw, 57344));
					if (!ByteHelper.CompareBytes(0, ByteHelper.SubByteArray(57344 + 4092, raw, 4)))
						saveData[1] = new GBASaveData(this, ByteHelper.SubByteArray(57344, raw, 57344));
					is128KbSave = true;
				}
				else if (raw.Length == 65536) {
					saveData[0] = new GBASaveData(this, ByteHelper.SubByteArray(0, raw, 57344));
					is128KbSave = false;
				}
				if (this.gameType == GameTypes.Any) {
					// The User will pick the proper game type later
					if (MostRecentSave.BlockDataCollection.GameCode == GameCodes.RubySapphire)
						this.gameType = GameTypes.Ruby;
					else if (MostRecentSave.BlockDataCollection.GameCode == GameCodes.Emerald)
						this.gameType = GameTypes.Emerald;
					else if (MostRecentSave.BlockDataCollection.GameCode == GameCodes.FireRedLeafGreen)
						this.gameType = GameTypes.FireRed;
				}
				PokePC.ApplyGameType(this.gameType);
				loaded = true;
			//}
			//catch (Exception) {
				//throw new Exception("An error occurred with the save file. It seems that the program cannot read the supplied save file. Please load a 128KB save file for Generation 3 pokemon games.");
			//}
		}

		#region Save Data Properties

		public byte[] Raw {
			get { return raw; }
		}
		public GBASaveData SaveData1 {
			get { return saveData[0]; }
		}
		public GBASaveData SaveData2 {
			get { return saveData[1]; }
		}
		public GBASaveData MostRecentSave {
			get {
				if (saveData[0] == null)
					return saveData[1];
				else if (saveData[1] == null)
					return saveData[0];
				else
					return saveData[0].BlockDataCollection[0].SaveIndex > saveData[1].BlockDataCollection[0].SaveIndex ? saveData[0] : saveData[1];
			}
		}
		public GBASaveData LeastRecentSave {
			get {
				if (is128KbSave && saveData[0].BlockDataCollection[0].SaveIndex > saveData[1].BlockDataCollection[0].SaveIndex)
					return saveData[1];
				else
					return saveData[0];
			}
		}
		public bool Is128KbSave {
			get { return is128KbSave; }
		}

		#endregion

		#region Basic Properties

		public Generations Generation {
			get { return Generations.Gen3; }
		}
		public Platforms Platform {
			get { return Platforms.GameBoyAdvance; }
		}
		public GameTypes GameType {
			get { return gameType; }
			set {
				gameType = value;
				PokePC.ApplyGameType(value);
			}
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(this); }
		}
		public bool IsChanged {
			get { return changed; }
			set {
				if (loaded) {
					//Console.WriteLine("Changed: " + GameType + " [" + TrainerName + "]");
					changed = value;
				}
			}
		}
		public bool IsJapanese {
			get { return japanese; }
			set {
				changed = true;
				japanese = value;
			}
		}

		public bool GetGameFlag(int index) {
			return MostRecentSave.BlockDataCollection.GetGameFlag(index);
		}
		public void SetGameFlag(int index, bool flag) {
			MostRecentSave.BlockDataCollection.SetGameFlag(index, flag);
		}

		#endregion

		#region Trainer

		public string TrainerName {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.TrainerName; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.TrainerInfo.TrainerName = value;
			}
		}
		public Genders TrainerGender {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.TrainerGender; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.TrainerInfo.TrainerGender = value;
			}
		}
		public ushort TrainerID {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.TrainerID; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.TrainerID = value; }
		}
		public ushort SecretID {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.SecretID; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.SecretID = value; }
		}

		#endregion

		#region Play Time

		public TimeSpan PlayTime {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.PlayTime; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.PlayTime = value; }
		}
		public ushort HoursPlayed {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.HoursPlayed; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.HoursPlayed = value; }
		}
		public byte MinutesPlayed {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.MinutesPlayed; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.MinutesPlayed = value; }
		}
		public byte SecondsPlayed {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.SecondsPlayed; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.SecondsPlayed = value; }
		}
		public byte FramesPlayed {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.FramesPlayed; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.FramesPlayed = value; }
		}

		#endregion

		#region Currencies

		public uint Money {
			get { return MostRecentSave.BlockDataCollection.TeamAndItems.Money; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.TeamAndItems.Money = value;
			}
		}
		public uint Coins {
			get { return MostRecentSave.BlockDataCollection.TeamAndItems.Coins; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.TeamAndItems.Coins = (ushort)value;
			}
		}
		public uint BattlePoints {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.BattlePoints; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.TrainerInfo.BattlePoints = (ushort)value;
			}
		}
		public uint BattlePointsWon {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.BattlePointsWon; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.TrainerInfo.BattlePointsWon = (ushort)value;
			}
		}
		public uint VolcanicAsh {
			get { return MostRecentSave.BlockDataCollection.NationalPokedexBAndC.VolcanicAsh; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.NationalPokedexBAndC.VolcanicAsh = (ushort)value;
			}
		}
		public uint PokeCoupons {
			get { return 0; }
			set { }
		}

		#endregion

		#region Containers

		public Inventory Inventory {
			get { return MostRecentSave.BlockDataCollection.Inventory; }
		}
		public IPokePC PokePC {
			get { return MostRecentSave.BlockDataCollection.PokePC; }
		}
		public Mailbox Mailbox {
			get { return MostRecentSave.BlockDataCollection.Mailbox; }
		}

		#endregion

		#region Misc

		public string RivalName {
			get { return MostRecentSave.BlockDataCollection.RivalInfo.RivalName; }
			set {
				IsChanged = true;
				MostRecentSave.BlockDataCollection.RivalInfo.RivalName = value;
			}
		}

		public ushort MirageIslandValue {
			get { return MostRecentSave.BlockDataCollection.NationalPokedexBAndC.MirageIslandValue; }
			set { MostRecentSave.BlockDataCollection.NationalPokedexBAndC.MirageIslandValue = value; }
		}

		public TimeSpan SaveTimestamp {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.SaveTimestamp; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.SaveTimestamp = value; }
		}
		public TimeSpan RealTimeClock {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.RealTimeClock; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.RealTimeClock = value; }
		}
		public DateTime RealTimeClockDate {
			get { return new DateTime(2000, 1, 1, 0, 0, 0) + RealTimeClock; }
		}

		public AlteringCavePokemon AlteringCavePokemon {
			get { return MostRecentSave.BlockDataCollection.NationalPokedexBAndC.AlteringCavePokemon; }
			set { MostRecentSave.BlockDataCollection.NationalPokedexBAndC.AlteringCavePokemon = value; }
		}

		public bool[] Badges {
			get { return MostRecentSave.BlockDataCollection.Badges; }
		}
		public bool HasBattledStevenEmerald {
			get { return MostRecentSave.BlockDataCollection.HasBattledStevenEmerald; }
			set { MostRecentSave.BlockDataCollection.HasBattledStevenEmerald = value; }
		}
		public byte SecretBaseLocation {
			get { return MostRecentSave.BlockDataCollection.NationalPokedexBAndC.SecretBaseLocation; }
			set { MostRecentSave.BlockDataCollection.NationalPokedexBAndC.SecretBaseLocation = value; }
		}
		public SecretBaseManager SecretBaseManager {
			get { return MostRecentSave.BlockDataCollection.SecretBaseManager; }
		}
		public Languages SecretBaseLanguage {
			get { return MostRecentSave.BlockDataCollection.NationalPokedexBAndC.SecretBaseLanguage; }
			set { MostRecentSave.BlockDataCollection.NationalPokedexBAndC.SecretBaseLanguage = value; }
		}

		#endregion

		#region Pokedex

		public bool[] PokedexOwned {
			get { return MostRecentSave.BlockDataCollection.TrainerInfo.PokedexOwned; }
			set { MostRecentSave.BlockDataCollection.TrainerInfo.PokedexOwned = value; }
		}
		public bool[] PokedexSeen {
			get {
				return MostRecentSave.BlockDataCollection.TrainerInfo.PokedexSeenA;
			}
			set {
				MostRecentSave.BlockDataCollection.TrainerInfo.PokedexSeenA = value;
				MostRecentSave.BlockDataCollection.TeamAndItems.PokedexSeenB = value;
				MostRecentSave.BlockDataCollection.RivalInfo.PokedexSeenC = value;
				for (int i = 0; i < value.Length; i++) {
					if (!value[i])
						SetPokemonOwned((ushort)(i + 1), false);
				}
			}
		}
		public ushort PokemonSeen {
			get {
				bool[] seenList = PokedexSeen;
				ushort seenCount = 0;
				foreach (bool b in seenList) {
					if (b)
						seenCount++;
				}
				return seenCount;
			}
		}
		public ushort PokemonOwned {
			get {
				bool[] ownedList = PokedexOwned;
				ushort ownedCount = 0;
				foreach (bool b in ownedList) {
					if (b)
						ownedCount++;
				}
				return ownedCount;
			}
		}
		public bool IsPokemonSeen(ushort dexID) {
			return	MostRecentSave.BlockDataCollection.TrainerInfo.IsPokemonSeenA(dexID) &&
					MostRecentSave.BlockDataCollection.TeamAndItems.IsPokemonSeenB(dexID) &&
					MostRecentSave.BlockDataCollection.RivalInfo.IsPokemonSeenC(dexID);
		}
		public bool IsPokemonOwned(ushort dexID) {
			return MostRecentSave.BlockDataCollection.TrainerInfo.IsPokemonOwned(dexID);
		}
		public void SetPokemonSeen(ushort dexID, bool seen) {
			MostRecentSave.BlockDataCollection.TrainerInfo.SetPokemonSeenA(dexID, seen);
			MostRecentSave.BlockDataCollection.TeamAndItems.SetPokemonSeenB(dexID, seen);
			MostRecentSave.BlockDataCollection.RivalInfo.SetPokemonSeenC(dexID, seen);
			if (!seen)
				SetPokemonOwned(dexID, false);
		}
		public void SetPokemonOwned(ushort dexID, bool owned) {
			MostRecentSave.BlockDataCollection.TrainerInfo.SetPokemonOwned(dexID, owned);
			if (owned) {
				MostRecentSave.BlockDataCollection.TrainerInfo.SetPokemonSeenA(dexID, true);
				MostRecentSave.BlockDataCollection.TeamAndItems.SetPokemonSeenB(dexID, true);
				MostRecentSave.BlockDataCollection.RivalInfo.SetPokemonSeenC(dexID, true);
			}
		}
		public bool HasNationalPokedex {
			get {
				return MostRecentSave.BlockDataCollection.TrainerInfo.NationalPokedexAUnlocked &&
					MostRecentSave.BlockDataCollection.NationalPokedexBAndC.NationalPokedexBUnlocked &&
					MostRecentSave.BlockDataCollection.NationalPokedexBAndC.NationalPokedexCUnlocked;
			}
			set {
				MostRecentSave.BlockDataCollection.TrainerInfo.NationalPokedexAUnlocked = value;
				MostRecentSave.BlockDataCollection.NationalPokedexBAndC.NationalPokedexBUnlocked = value;
				MostRecentSave.BlockDataCollection.NationalPokedexBAndC.NationalPokedexCUnlocked = value;
			}
		}
		public bool IsPokedexPokemonShiny(ushort dexID) {
			return false;
		}
		public uint GetPokedexPokemonPersonality(ushort dexID) {
			if (dexID == 201)
				return MostRecentSave.BlockDataCollection.TrainerInfo.UnownPokedexPersonality;
			if (dexID == 327)
				return MostRecentSave.BlockDataCollection.TrainerInfo.SpindaPokedexPersonality;
			return 0;
		}
		public void OwnPokemon(IPokemon pokemon) {
			if (!pokemon.IsEgg) {
				ushort dexID = pokemon.DexID;
				if (dexID == 201 && MostRecentSave.BlockDataCollection.TrainerInfo.UnownPokedexPersonality == 0)
					MostRecentSave.BlockDataCollection.TrainerInfo.UnownPokedexPersonality = pokemon.Personality;
				else if (dexID == 327 && MostRecentSave.BlockDataCollection.TrainerInfo.SpindaPokedexPersonality == 0)
					MostRecentSave.BlockDataCollection.TrainerInfo.SpindaPokedexPersonality = pokemon.Personality;
				MostRecentSave.BlockDataCollection.TrainerInfo.SetPokemonOwned(dexID, true);
				MostRecentSave.BlockDataCollection.TrainerInfo.SetPokemonSeenA(dexID, true);
				MostRecentSave.BlockDataCollection.TeamAndItems.SetPokemonSeenB(dexID, true);
				MostRecentSave.BlockDataCollection.RivalInfo.SetPokemonSeenC(dexID, true);
			}
		}

		#endregion

		#region Loading/Saving

		private byte[] GetFinalData() {
			if (saveData[0] != null)
				ByteHelper.ReplaceBytes(raw, 0, this.SaveData1.GetFinalData());
			if (saveData[1] != null)
				ByteHelper.ReplaceBytes(raw, 57344, this.SaveData2.GetFinalData());
			return raw;
		}
		public void Save(string filePath) {
			File.WriteAllBytes(filePath, GetFinalData());
		}

		#endregion
	}

	public class DifferenceData {
		public int StartIndex;
		public int Length;

		public DifferenceData(int startIndex) {
			this.StartIndex = startIndex;
			this.Length = 1;
		}

		public override string ToString() {
			return StartIndex.ToString() + " (0x" + StartIndex.ToString("X") + ") - " + Length.ToString();
		}
	}
}
