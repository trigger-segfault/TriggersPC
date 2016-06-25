using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class StrategyMemoEntry {

		private byte[] raw;
		private bool xd;
		// Colosseum:
		// * Not Seen: SpeciesID == 0
		// * Not Owned: Flags == 0x2
		// * Fully Registered (Owned): Flags == 0
		// XD:
		// * Not Seen: SpeciesID == 0
		// * Fully Registered (Seen): Flags == 0x2
		
		public StrategyMemoEntry(bool xd) {
			this.raw = new byte[12];
			this.xd = xd;
		}
		public StrategyMemoEntry(byte[] data, bool xd) {
			this.raw = data;
			this.xd = xd;
		}

		public byte[] Raw {
			get { return raw; }
		}
		public ushort Flags {
			get { return ByteHelper.BitsToByte(raw, 0, 6, 2); }
			set { BigEndian.WriteUInt16((ushort)(SpeciesID | (value << 14)), raw, 0); }
		}
		public bool IsSeen {
			get {
				if (xd)
					return (Flags & 0x2) != 0;
				else
					return SpeciesID != 0;
			}
		}
		public bool IsOwned {
			get {
				if (xd)
					return false;
				else
					return Flags == 0;
			}
		}
		public ushort SpeciesID {
			get { return (ushort)(BigEndian.ToUInt16(raw, 0) & 0x1FF); }
			set { BigEndian.WriteUInt16((ushort)(value | (Flags << 14)), raw, 0); }
		}
		public ushort DexID {
			get {
				PokemonData pokemonData = PokemonDatabase.GetPokemonFromID(SpeciesID);
				if (pokemonData != null)
					return pokemonData.DexID;
				return 0;
			}
			set {
				PokemonData pokemonData = PokemonDatabase.GetPokemonFromDexID(value);
				if (pokemonData != null)
					SpeciesID = pokemonData.ID;
			}
		}
		public ushort FirstTrainerID {
			get { return BigEndian.ToUInt16(raw, 6); }
			set { BigEndian.WriteUInt16(value, raw, 6); }
		}
		public ushort FirstSecretID {
			get { return BigEndian.ToUInt16(raw, 4); }
			set { BigEndian.WriteUInt16(value, raw, 4); }
		}
		public uint FirstPersonality {
			get { return BigEndian.ToUInt32(raw, 8); }
			set { BigEndian.WriteUInt32(value, raw, 8); }
		}
		public bool IsShiny {
			get {
				byte[] bytes = BitConverter.GetBytes(FirstPersonality);
				return ((uint)FirstTrainerID ^ (uint)FirstSecretID ^ (uint)BitConverter.ToUInt16(bytes, 0) ^ (uint)BitConverter.ToUInt16(bytes, 2)) < 8;
			}
		}
	}
	public class StrategyMemoData : GCData {

		List<StrategyMemoEntry> entries;

		public StrategyMemoData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {

			this.entries = new List<StrategyMemoEntry>();
			ushort numEntries = Math.Min((ushort)500, BigEndian.ToUInt16(data, 0));
			//numEntries = 500;
			for (int i = 0; i < numEntries; i++) {
				entries.Add(new StrategyMemoEntry(ByteHelper.SubByteArray(4 + i * 12, data, 12), gameSave.GameType == GameTypes.XD));

				// Remove invalid entries caused by Trigger's PC corruption.
				if (entries[entries.Count - 1].SpeciesID == 0)
					entries.RemoveAt(entries.Count - 1);
			}
		}

		public StrategyMemoEntry this[ushort dexID] {
			get {
				ushort speciesID = PokemonDatabase.GetPokemonFromDexID(dexID).ID;
				foreach (StrategyMemoEntry entry in entries) {
					if (entry.SpeciesID == speciesID)
						return entry;
				}
				return null;
			}
		}

		public bool[] PokedexOwned {
			get {
				bool[] owned = new bool[386];
				foreach (StrategyMemoEntry entry in entries) {
					if (entry.SpeciesID != 0 && ((gameSave.GameType == GameTypes.Colosseum && entry.Flags == 0) ||
												(gameSave.GameType == GameTypes.XD && entry.Flags == 0x2)))
						owned[entry.DexID - 1] = true;
				}
				return owned;
			}
		}
		public bool[] PokedexSeen {
			get {
				bool[] seen = new bool[386];
				foreach (StrategyMemoEntry entry in entries) {
					if (entry.SpeciesID != 0)
						seen[entry.DexID - 1] = true;
				}
				return seen;
			}
		}
		public ushort PokemonSeen {
			get {
				ushort seenCount = 0;
				foreach (StrategyMemoEntry entry in entries) {
					if (entry.SpeciesID != 0)
						seenCount++;
				}
				return seenCount;
			}
		}
		public ushort PokemonOwned {
			get {
				ushort ownedCount = 0;
				foreach (StrategyMemoEntry entry in entries) {
					if (entry.SpeciesID != 0 && ((gameSave.GameType == GameTypes.Colosseum && entry.Flags == 0) ||
												(gameSave.GameType == GameTypes.XD && entry.Flags == 0x2)))
						ownedCount++;
				}
				return ownedCount;
			}
		}
		public bool IsPokemonSeen(ushort dexID) {
			foreach (StrategyMemoEntry entry in entries) {
				if (entry.DexID == dexID)
					return true;
			}
			return false;
		}
		public bool IsPokemonOwned(ushort dexID) {
			foreach (StrategyMemoEntry entry in entries) {
				if (entry.DexID == dexID &&
					((gameSave.GameType == GameTypes.Colosseum && entry.Flags == 0) ||
					(gameSave.GameType == GameTypes.XD && entry.Flags == 0x2)))
					return true;
			}
			return false;
		}

		public void RegisterPokemon(IPokemon pokemon) {
			ushort speciesID = pokemon.SpeciesID;
			StrategyMemoEntry unusedEntry = null;
			for (int i = 0; i < entries.Count; i++) {
				if (entries[i].SpeciesID == speciesID) {
					return;
				}
				else if (entries[i].SpeciesID == 0 && unusedEntry == null) {
					unusedEntry = entries[i];
				}
			}
			if (unusedEntry == null) {
				entries.Add(new StrategyMemoEntry(gameSave.GameType == GameTypes.XD));
				unusedEntry = entries[entries.Count - 1];
			}
			unusedEntry.SpeciesID = pokemon.SpeciesID;
			unusedEntry.FirstTrainerID = pokemon.TrainerID;
			unusedEntry.FirstSecretID = pokemon.SecretID;
			unusedEntry.FirstPersonality = pokemon.Personality;
			if (gameSave.GameType == GameTypes.XD)
				unusedEntry.Flags = 0x2;
			else
				unusedEntry.Flags = 0x0;
		}
		public void UnregisterPokemon(ushort dexID) {
			foreach (StrategyMemoEntry entry in entries) {
				if (entry.DexID == dexID) {
					entry.SpeciesID = 0;
				}
			}
		}
		public void UnsetPokemonOwned(ushort dexID) {
			if (gameSave.GameType != GameTypes.XD) {
				foreach (StrategyMemoEntry entry in entries) {
					if (entry.DexID == dexID) {
						entry.Flags = 0x2;
					}
				}
			}
		}

		public override byte[] GetFinalData() {
			BigEndian.WriteUInt16((ushort)entries.Count, raw, 0);
			for (int i = 0; i < 500; i++) {
				if (i < entries.Count)
					ByteHelper.ReplaceBytes(raw, 4 + i * 12, entries[i].Raw);
				else
					ByteHelper.ReplaceBytes(raw, 4 + i * 12, new byte[10]);
			}

			return raw;
		}
	}
}
