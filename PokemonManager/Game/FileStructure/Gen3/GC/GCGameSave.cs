using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class GCGameSave : IGameSave {

		#region Members

		private bool hasGCIData;
		private GCSaveData[] saveData;
		private byte[] raw;
		private bool changed;
		private bool loaded;
		private bool japanese;
		private GameTypes gameType;

		#endregion

		public GCGameSave(string filePath, bool japanese = false) {
			byte[] data		= File.ReadAllBytes(filePath);
			this.hasGCIData	= false;
			this.gameType	= GameTypes.Any;
			this.japanese	= japanese;

			// GCI Data is 64 bytes in length
			if (data.Length == 393216 || data.Length == 393280) {
				if (data.Length == 393280)
					hasGCIData = true;
				gameType = GameTypes.Colosseum;
			}
			else if (data.Length == 352256 || data.Length == 352320) {
				if (data.Length == 352320)
					hasGCIData = true;
				gameType = GameTypes.XD;
			}
			else {
				throw new Exception("GameCube save data must be 393,216 or 393,280 bytes in length for Colosseum, and 352,256 or 352,320 bytes in length for XD");
			}
			// First 24,576 bytes are for the gamecube memory card manager, such as name and icon
			int start = 24576 + (hasGCIData ? 64 : 0);
			if (gameType == GameTypes.Colosseum) {
				this.saveData = new GCSaveData[3];
				this.saveData[0] = new GCSaveData(this, ByteHelper.SubByteArray(start, data, 122880));
				this.saveData[1] = new GCSaveData(this, ByteHelper.SubByteArray(start + 122880, data, 122880));
				this.saveData[2] = new GCSaveData(this, ByteHelper.SubByteArray(start + 122880 * 2, data, 122880));
				PokePC.ApplyGameType(gameType);
			}
			else {
				this.saveData = new GCSaveData[2];
				this.saveData[0] = new GCSaveData(this, ByteHelper.SubByteArray(start, data, 163840));
				this.saveData[1] = new GCSaveData(this, ByteHelper.SubByteArray(start + 163840, data, 163840));
				PokePC.ApplyGameType(gameType);
			}
			this.raw = data;
			loaded = true;
		}

		#region Save Data

		public uint SaveCount {
			get { return MostRecentSave.SaveCount; }
		}
		public GCSaveData SaveData1 {
			get { return saveData[0]; }
		}
		public GCSaveData SaveData2 {
			get { return saveData[1]; }
		}
		public GCSaveData SaveData3 {
			get {
				if (gameType == GameTypes.Colosseum)
					return saveData[2];
				return null;
			}
		}
		public GCSaveData MostRecentSave {
			get {
				if (gameType == GameTypes.Colosseum) {
					int maxIndex = 0;
					if (saveData[1].SaveCount > saveData[0].SaveCount)
						maxIndex = 1;
					if (saveData[2].SaveCount > saveData[maxIndex].SaveCount)
						maxIndex = 2;
					return saveData[maxIndex];
				}
				else {
					return saveData[0].SaveCount > saveData[1].SaveCount ? saveData[0] : saveData[1];
				}
			}
		}
		public GCSaveData MiddleRecentSave {
			get {
				if (gameType == GameTypes.Colosseum) {
					for (int i = 0; i < 3; i++) {
						if ((saveData[i].SaveCount < saveData[(i + 1) % 3].SaveCount && saveData[i].SaveCount > saveData[(i + 2) % 3].SaveCount) ||
							(saveData[i].SaveCount > saveData[(i + 1) % 3].SaveCount && saveData[i].SaveCount < saveData[(i + 2) % 3].SaveCount)) {
							return saveData[i];
						}
					}
				}
				return null;
			}
		}
		public GCSaveData LeastRecentSave {
			get {
				if (gameType == GameTypes.Colosseum) {
					int minIndex = 0;
					if (saveData[1].SaveCount < saveData[0].SaveCount)
						minIndex = 1;
					if (saveData[2].SaveCount < saveData[minIndex].SaveCount)
						minIndex = 2;
					return saveData[minIndex];
				}
				else {
					return saveData[0].SaveCount < saveData[1].SaveCount ? saveData[0] : saveData[1];
				}
			}
		}
		public GCRegions CurrentRegion {
			get { return MostRecentSave.GameConfigData.CurrentRegion; }
			set { MostRecentSave.GameConfigData.CurrentRegion = value; }
		}

		#endregion

		#region Basic

		public Generations Generation {
			get { return Generations.Gen3; }
		}
		public Platforms Platform {
			get { return Platforms.GameCube; }
		}
		public GameTypes GameType {
			get { return gameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(this); }
		}
		public bool IsChanged {
			get { return changed; }
			set {
				if (loaded) {
					changed = value;
				}
			}
		}
		public bool IsJapanese {
			get {
				return MostRecentSave.GameConfigData.Language == Languages.Japanese;
				//return japanese;
			}
			set {
				//changed = true;
				//japanese = value;
			}
		}
		public byte[] Raw {
			get { return raw; }
		}
		public bool HasGCIData {
			get { return hasGCIData; }
		}

		#endregion

		#region Trainer

		public string TrainerName {
			get { return MostRecentSave.PlayerData.TrainerName; }
			set {
				IsChanged = true;
				MostRecentSave.PlayerData.TrainerName = value;
			}
		}
		public Genders TrainerGender {
			get { return MostRecentSave.PlayerData.TrainerGender; }
			set { MostRecentSave.PlayerData.TrainerGender = value; }
		}
		public ushort TrainerID {
			get { return MostRecentSave.PlayerData.TrainerID; }
			set { MostRecentSave.PlayerData.TrainerID = value; }
		}
		public ushort SecretID {
			get { return MostRecentSave.PlayerData.SecretID; }
			set { MostRecentSave.PlayerData.SecretID = value; }
		}

		#endregion

		#region Play Time

		public TimeSpan PlayTime {
			get { return MostRecentSave.PlayTime; }
			set { MostRecentSave.PlayTime = value; }
		}
		public ushort HoursPlayed {
			get { return MostRecentSave.HoursPlayed; }
			set { MostRecentSave.HoursPlayed = value; }
		}
		public byte MinutesPlayed {
			get { return MostRecentSave.MinutesPlayed; }
			set { MostRecentSave.MinutesPlayed = value; }
		}
		public byte SecondsPlayed {
			get { return MostRecentSave.SecondsPlayed; }
			set { MostRecentSave.SecondsPlayed = value; }
		}
		public byte FramesPlayed {
			get { return 0; }
			set {  }
		}

		#endregion

		#region Containers

		public Inventory Inventory {
			get { return MostRecentSave.Inventory; }
		}
		public IPokePC PokePC {
			get { return MostRecentSave.PokePC; }
		}
		public Mailbox Mailbox {
			get { return null; }
		}
		public XDShadowPokemonInfo GetShadowInfo(uint personality) {
			return MostRecentSave.ShadowPokemonData[personality];
		}
		public bool HasShadowInfo(uint personality) {
			return MostRecentSave.ShadowPokemonData.Contains(personality);
		}

		#endregion

		#region Currencies

		public uint Money {
			get { return MostRecentSave.PlayerData.Money; }
			set {
				IsChanged = true;
				MostRecentSave.PlayerData.Money = value;
			}
		}
		public uint PokeCoupons {
			get { return MostRecentSave.PlayerData.PokeCoupons; }
			set {
				IsChanged = true;
				MostRecentSave.PlayerData.PokeCoupons = value;
			}
		}
		public uint Coins {
			get { return 0; }
			set { }
		}
		public uint BattlePoints {
			get { return 0; }
			set { }
		}
		public uint VolcanicAsh {
			get { return 0; }
			set { }
		}

		#endregion

		#region Misc

		public string RivalName {
			get { return MostRecentSave.PlayerData.RuisName; }
			set { }
		}

		#endregion

		#region Pokedex

		public bool[] PokedexOwned {
			get { return MostRecentSave.StrategyMemoData.PokedexOwned; }
			set {  }
		}
		public bool[] PokedexSeen {
			get { return MostRecentSave.StrategyMemoData.PokedexSeen; }
			set {  }
		}
		public ushort PokemonSeen {
			get { return MostRecentSave.StrategyMemoData.PokemonSeen; }
		}
		public ushort PokemonOwned {
			get { return MostRecentSave.StrategyMemoData.PokemonOwned; }
		}
		public bool IsPokemonSeen(ushort dexID) {
			return MostRecentSave.StrategyMemoData.IsPokemonSeen(dexID);
		}
		public bool IsPokemonOwned(ushort dexID) {
			return MostRecentSave.StrategyMemoData.IsPokemonOwned(dexID);
		}
		public void SetPokemonSeen(ushort dexID, bool seen) {
			// Use Register and Unregister
		}
		public void SetPokemonOwned(ushort dexID, bool owned) {
			// Use Register and Unregister
		}
		public bool HasNationalPokedex {
			get { return true; }
			set { }
		}
		public void RegisterPokemon(IPokemon pokemon) {
			MostRecentSave.StrategyMemoData.RegisterPokemon(pokemon);
		}
		public void UnregisterPokemon(ushort dexID) {
			MostRecentSave.StrategyMemoData.UnregisterPokemon(dexID);
		}
		public void UnsetPokemonOwned(ushort dexID) {
			MostRecentSave.StrategyMemoData.UnsetPokemonOwned(dexID);
		}
		public bool IsPokedexPokemonShiny(ushort dexID) {
			StrategyMemoEntry entry = MostRecentSave.StrategyMemoData[dexID];
			if (entry != null)
				return entry.IsShiny;
			return false;
		}
		public uint GetPokedexPokemonPersonality(ushort dexID) {
			StrategyMemoEntry entry = MostRecentSave.StrategyMemoData[dexID];
			if (entry != null)
				return entry.FirstPersonality;
			return 0;
		}
		public void OwnPokemon(IPokemon pokemon) {
			if (!pokemon.IsEgg)
				RegisterPokemon(pokemon);
		}


		#endregion

		#region Loading/Saving

		public void Save(string filePath) {
			int start = 24576 + (hasGCIData ? 64 : 0);
			if (gameType == GameTypes.Colosseum) {
				ByteHelper.ReplaceBytes(raw, start, saveData[0].GetFinalData());
				ByteHelper.ReplaceBytes(raw, start + 122880, saveData[1].GetFinalData());
				ByteHelper.ReplaceBytes(raw, start + 122880 * 2, saveData[2].GetFinalData());
			}
			else {
				ByteHelper.ReplaceBytes(raw, start, saveData[0].GetFinalData());
				ByteHelper.ReplaceBytes(raw, start + 163840, saveData[1].GetFinalData());
			}
			File.WriteAllBytes(filePath, raw);
		}

		#endregion
	}
}
