using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.PB {
	public class PokemonBoxGameSave : IGameSave {

		#region Members

		private bool hasGCIData;
		private PokemonBoxSaveData[] saveData;
		private byte[] raw;
		private bool changed;
		private bool loaded;
		private bool japanese;

		#endregion

		public PokemonBoxGameSave(string filePath, bool japanese = false) {
			byte[] data		= File.ReadAllBytes(filePath);
			this.loaded		= false;
			this.hasGCIData	= false;
			this.japanese	= japanese;

			if (data.Length == 483328 || data.Length == 483392) {
				if (data.Length == 483392)
					this.hasGCIData = true;
			}
			else {
				throw new Exception("Pokemon Box saves must be either 483,328 or 483,392 bytes in length");
			}

			int start = 0x2000 + (hasGCIData ? 64 : 0);
			this.saveData = new PokemonBoxSaveData[2];
			List<PokemonBoxBlockData> blocks = new List<PokemonBoxBlockData>();
			for (int i = 0; start + 0x2000 * (i + 1) <= data.Length; i++) {
				//blocks.Add(new PokemonBoxBlockData(ByteHelper.SubByteArray(start + 0x2000 * i, data, 0x2E000)));
			}
			if (BigEndian.ToSInt32(data, start + 0x8) != 0)
				this.saveData[0] = new PokemonBoxSaveData(this, ByteHelper.SubByteArray(start, data, 0x2E000));
			if (BigEndian.ToSInt32(data, start + 0x2E000 + 0x8) != 0)
				this.saveData[1] = new PokemonBoxSaveData(this, ByteHelper.SubByteArray(start + 0x2E000, data, 0x2E000));
			PokePC.ApplyGameType(GameTypes.PokemonBox);

			this.raw	= data;
			this.loaded	= true;
		}

		#region Save Data

		public uint SaveCount {
			get { return (uint)MostRecentSave.SaveCount; }
		}
		public PokemonBoxSaveData SaveData1 {
			get { return saveData[0]; }
		}
		public PokemonBoxSaveData SaveData2 {
			get { return saveData[1]; }
		}
		public PokemonBoxSaveData MostRecentSave {
			get {
				if (saveData[0] == null)
					return saveData[1];
				else if (saveData[1] == null)
					return saveData[0];
				else
					return (saveData[0].SaveCount > saveData[1].SaveCount ? saveData[0] : saveData[1]);
			}
		}
		public PokemonBoxSaveData LeastRecentSave {
			get { return (saveData[0].SaveCount <= saveData[1].SaveCount ? saveData[0] : saveData[1]); }
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
			get { return GameTypes.PokemonBox; }
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
			get { return japanese; }
			set {
				IsChanged = true;
				japanese = value;
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
			get { return "PKMNBOX"; }
			set { }
		}
		public Genders TrainerGender {
			get { return Genders.Genderless; }
			set { }
		}
		public ushort TrainerID {
			get { return 0; }
			set { }
		}
		public ushort SecretID {
			get { return 0; }
			set { }
		}

		#endregion

		#region Play Time

		public TimeSpan PlayTime {
			get { return new TimeSpan(); }
			set { }
		}
		public ushort HoursPlayed {
			get { return 0; }
			set { }
		}
		public byte MinutesPlayed {
			get { return 0; }
			set { }
		}
		public byte SecondsPlayed {
			get { return 0; }
			set { }
		}
		public byte FramesPlayed {
			get { return 0; }
			set {  }
		}

		#endregion

		#region Containers

		public IPokePC PokePC {
			get { return MostRecentSave.PokePC; }
		}
		public Inventory Inventory {
			get { return null; }
		}
		public Mailbox Mailbox {
			get { return null; }
		}

		#endregion

		#region Currencies

		public uint Money {
			get { return 0; }
			set { }
		}
		public uint PokeCoupons {
			get { return 0; }
			set { }
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

		#region Pokedex

		public bool[] PokedexOwned {
			get { return null; }
			set { }
		}
		public bool[] PokedexSeen {
			get { return null; }
			set { }
		}
		public ushort PokemonSeen {
			get { return 0; }
		}
		public ushort PokemonOwned {
			get { return 0; }
		}
		public bool IsPokemonSeen(ushort dexID) {
			return false;
		}
		public bool IsPokemonOwned(ushort dexID) {
			return false;
		}
		public void SetPokemonSeen(ushort dexID, bool seen) {
			
		}
		public void SetPokemonOwned(ushort dexID, bool owned) {
			
		}
		public bool HasNationalPokedex {
			get { return false; }
			set { }
		}
		public bool IsPokedexPokemonShiny(ushort dexID) {
			return false;
		}
		public uint GetPokedexPokemonPersonality(ushort dexID) {
			return 0;
		}
		public void OwnPokemon(IPokemon pokemon) {
			
		}

		#endregion

		#region Misc

		public string RivalName {
			get { return "TRIGSPC"; }
			set { }
		}

		#endregion

		#region Loading/Saving

		public void Save(string filePath) {
			int start = 0x2000 + (hasGCIData ? 64 : 0);
			if (saveData[0] != null)
				ByteHelper.ReplaceBytes(raw, start, saveData[0].GetFinalData());
			if (saveData[1] != null)
				ByteHelper.ReplaceBytes(raw, start + 0x2E000, saveData[1].GetFinalData());
			
			File.WriteAllBytes(filePath, raw);
		}

		#endregion
	}
}
