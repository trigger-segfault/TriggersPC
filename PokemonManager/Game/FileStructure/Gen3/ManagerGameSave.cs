using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.PokemonStructures.Events;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3 {
	public class ManagerGameSave : IGameSave {

		#region Members

		private string trainerName;
		private Genders trainerGender;
		private ushort trainerID;
		private ushort secretID;
		private TimeSpan playTime;
		private DateTime lastSaveTime;

		private bool[] pokedexSeen;
		private bool[] pokedexOwned;

		private uint money;
		private uint coins;
		private uint battlePoints;
		private uint pokeCoupons;
		private uint volcanicAsh;

		private Inventory inventory;
		private List<ManagerPokePC> pokePCs;
		private Mailbox mailbox;

		private bool changed;
		private bool loaded;

		#endregion

		public ManagerGameSave() {
			Random random		= new Random((int)DateTime.Now.Ticks);
			this.trainerName	= "TRIGSPC";
			this.trainerGender	= (Genders)random.Next(2);
			this.trainerID		= (ushort)random.Next(ushort.MaxValue);
			this.secretID		= (ushort)random.Next(ushort.MaxValue);
			this.playTime		= new TimeSpan();
			this.lastSaveTime	= DateTime.Now;

			this.pokedexSeen	= new bool[386];
			this.pokedexOwned	= new bool[386];

			this.money			= 0;
			this.coins			= 0;
			this.battlePoints	= 0;
			this.pokeCoupons	= 0;
			this.volcanicAsh	= 0;

			this.pokePCs = new List<ManagerPokePC>();
			this.pokePCs.Add(new ManagerPokePC(this, "Row 1"));
			this.inventory = new Inventory(this);
			this.mailbox = new Mailbox(this, 0);

			this.inventory.AddItemInventory();
			this.inventory.Items.AddPocket(ItemTypes.Items, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.InBattle, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.Valuables, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.Hold, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.Misc, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.PokeBalls, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.TMCase, 0, 0, false, true);
			this.inventory.Items.AddPocket(ItemTypes.Berries, 0, 0, false, true);
			this.inventory.Items.AddPocket(ItemTypes.KeyItems, 0, 0, false, false);
			this.inventory.Items.AddPocket(ItemTypes.CologneCase, 0, 0, false, true);
			this.inventory.Items.AddPocket(ItemTypes.DiscCase, 0, 0, false, true);

			this.inventory.AddDecorationInventory();
			this.inventory.Decorations.AddPocket(DecorationTypes.Desk, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Chair, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Plant, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Ornament, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Mat, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Poster, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Doll, 0, 0);
			this.inventory.Decorations.AddPocket(DecorationTypes.Cushion, 0, 0);

			this.inventory.AddPokeblockCase(0);

			loaded = true;
		}

		#region Basic

		public Generations Generation {
			get { return Generations.Gen3; }
		}
		public Platforms Platform {
			get { return Platforms.PC; }
		}
		public GameTypes GameType {
			get { return GameTypes.Any; }
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
		public bool IsLoaded {
			get { return loaded; }
			set { loaded = value; }
		}
		// No need for the Manager save
		public bool IsJapanese {
			get { return false; }
			set { }
		}

		#endregion

		#region Trainer

		public string TrainerName {
			get { return trainerName; }
			set {
				trainerName = value;
				IsChanged = true;
			}
		}
		public Genders TrainerGender {
			get { return trainerGender; }
			set {
				trainerGender = value;
				IsChanged = true;
			}
		}
		public ushort TrainerID {
			get { return trainerID; }
			set {
				trainerID = value;
				IsChanged = true;
			}
		}
		public ushort SecretID {
			get { return secretID; }
			set {
				secretID = value;
				IsChanged = true;
			}
		}

		#endregion

		#region PlayTime

		public TimeSpan PlayTime {
			get { return playTime; }
			set { playTime = value; }
		}
		public DateTime TimeOfLastSave {
			get { return lastSaveTime; }
			set { lastSaveTime = value; }
		}
		public ushort HoursPlayed {
			get { return (ushort)playTime.TotalHours; }
			set { }
		}
		public byte MinutesPlayed {
			get { return (byte)playTime.Minutes; }
			set { }
		}
		public byte SecondsPlayed {
			get { return (byte)playTime.Seconds; }
			set { }
		}
		public byte FramesPlayed {
			get { return 0; }
			set { }
		}

		#endregion

		#region Currencies

		public uint Money {
			get { return money; }
			set {
				money = value;
				IsChanged = true;
			}
		}
		public uint Coins {
			get { return coins; }
			set {
				coins = value;
				IsChanged = true;
			}
		}
		public uint BattlePoints {
			get { return battlePoints; }
			set {
				battlePoints = value;
				IsChanged = true;
			}
		}
		public uint PokeCoupons {
			get { return pokeCoupons; }
			set {
				pokeCoupons = value;
				IsChanged = true;
			}
		}
		public uint VolcanicAsh {
			get { return volcanicAsh; }
			set {
				volcanicAsh = value;
				IsChanged = true;
			}
		}

		#endregion

		#region Containers

		public Inventory Inventory {
			get { return inventory; }
			set { inventory = value; }
		}
		public IPokePC PokePC {
			get { return pokePCs[0]; }
			set { pokePCs[0] = (ManagerPokePC)value; }
		}
		public ManagerPokePC GetPokePCRow(int row) {
			return pokePCs[row];
		}
		public int NumPokePCRows {
			get { return pokePCs.Count; }
		}
		public Mailbox Mailbox {
			get { return mailbox; }
		}
		public void AddPokePCRow(string name = "", bool livingDex = false) {
			IsChanged = true;
			pokePCs.Add(new ManagerPokePC(this, name == "" ? "Row " + (pokePCs.Count + 1).ToString() : name, livingDex));
		}
		public void RemovePokePCRow(ManagerPokePC pokePC) {
			IsChanged = true;
			pokePCs.Remove(pokePC);
		}
		public void RemovePokePCRow(int index) {
			IsChanged = true;
			pokePCs.RemoveAt(index);
		}
		public int PokePCRowIndexOf(ManagerPokePC pokePC) {
			return pokePCs.IndexOf(pokePC);
		}
		public void MoveRowUp(int index) {
			if (index > 0) {
				ManagerPokePC pokePC = pokePCs[index];
				pokePCs.RemoveAt(index);
				pokePCs.Insert(index - 1, pokePC);
			}
		}
		public void MoveRowDown(int index) {
			if (index + 1 < pokePCs.Count) {
				ManagerPokePC pokePC = pokePCs[index];
				pokePCs.RemoveAt(index);
				pokePCs.Insert(index + 1, pokePC);
			}
		}

		#endregion

		#region Pokedex

		public bool[] PokedexSeen {
			get { return pokedexSeen; }
			set { pokedexSeen = value; }
		}
		public bool[] PokedexOwned {
			get { return pokedexOwned; }
			set { pokedexOwned = value; }
		}
		public ushort PokemonSeen {
			get {
				bool[] seenList = pokedexSeen;
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
				bool[] ownedList = pokedexOwned;
				ushort ownedCount = 0;
				foreach (bool b in ownedList) {
					if (b)
						ownedCount++;
				}
				return ownedCount;
			}
		}
		public bool IsPokemonSeen(ushort dexID) {
			return pokedexSeen[dexID - 1];
		}
		public bool IsPokemonOwned(ushort dexID) {
			return pokedexOwned[dexID - 1];
		}
		public void SetPokemonSeen(ushort dexID, bool seen) {
			pokedexSeen[dexID - 1] = seen;
			if (!seen)
				pokedexOwned[dexID - 1] = false;
		}
		public void SetPokemonOwned(ushort dexID, bool owned) {
			pokedexOwned[dexID - 1] = owned;
			if (owned)
				pokedexSeen[dexID - 1] = true;
		}
		public bool HasNationalPokedex {
			get { return true; }
			set { }
		}
		public bool IsPokedexPokemonShiny(ushort dexID) {
			return false;
		}
		public uint GetPokedexPokemonPersonality(ushort dexID) {
			return 0;
		}
		public void OwnPokemon(IPokemon pokemon) {
			SetPokemonOwned(pokemon.DexID, true);
		}

		#endregion

		#region Loading/Saving

		public void Load(byte[] data) {
			loaded = false;
			int index = 0;

			uint version = LittleEndian.ToUInt32(data, 0);
			index += 4;

			LoadTrainerInfo(version, LoadWithSize(ref index, data));
			LoadInventory(version, LoadWithSize(ref index, data));
			LoadPCRows(version, LoadWithSize(ref index, data));
			mailbox.Load(LoadWithSize(ref index, data));
			if (version >= 2) {
				LoadCompletedEvents(LoadWithSize(ref index, data));
			}
			if (version >= 5) {
				LoadPokerusStrains(LoadWithSize(ref index, data));
			}
			loaded = true;
		}

		private void LoadPCRows(uint version, byte[] data) {
			int index = 0;

			int numRows = LittleEndian.ToSInt32(data, 0);
			index += 4;

			pokePCs.Clear();
			for (int i = 0; i < numRows; i++) {
				string name = ByteHelper.ReadString(data, index, 30);
				index += 60;
				bool livingDex = LittleEndian.ToBool(data, index);
				index += 1;
				bool revealEggs = false;
				if (version >= 3) {
					revealEggs = LittleEndian.ToBool(data, index);
					index += 1;
				}
				pokePCs.Add(new ManagerPokePC(this, name, livingDex, revealEggs));
				pokePCs[i].Load(LoadWithSize(ref index, data));
			}
		}

		private byte[] LoadWithSize(ref int index, byte[] data) {
			int size = LittleEndian.ToSInt32(data, index);
			index += 4 + size;
			return ByteHelper.SubByteArray(index - size, data, size);
		}

		private void LoadTrainerInfo(uint version, byte[] data) {

			if (version >= 1) {
				trainerName = GBACharacterEncoding.GetString(ByteHelper.SubByteArray(0, data, 7));
				trainerGender = (Genders)data[7];
				trainerID = LittleEndian.ToUInt16(data, 8);
				secretID = LittleEndian.ToUInt16(data, 10);
				playTime = TimeSpan.FromTicks(LittleEndian.ToSInt64(data, 12));
				lastSaveTime = DateTime.Now;

				money = LittleEndian.ToUInt32(data, 20);
				coins = LittleEndian.ToUInt32(data, 24);
				battlePoints = LittleEndian.ToUInt32(data, 28);
				pokeCoupons = LittleEndian.ToUInt32(data, 32);
				volcanicAsh = LittleEndian.ToUInt32(data, 36);

				BitArray seen = ByteHelper.GetBits(data, 40, 0, 386);
				BitArray owned = ByteHelper.GetBits(data, 90, 0, 386);
				for (int i = 0; i < 386; i++)
					pokedexSeen[i] = seen[i];
				for (int i = 0; i < 386; i++)
					pokedexOwned[i] = owned[i];
			}
		}

		private void LoadInventory(uint version, byte[] data) {
			inventory.Clear();
			int index = 0;

			if (version >= 1) {
				if (version <= 3) {
					LoadItemPocket(ItemTypes.Items, LoadWithSize(ref index, data));
				}
				else if (version >= 4) {
					LoadItemPocket(ItemTypes.InBattle, LoadWithSize(ref index, data));
					LoadItemPocket(ItemTypes.Valuables, LoadWithSize(ref index, data));
					LoadItemPocket(ItemTypes.Hold, LoadWithSize(ref index, data));
					LoadItemPocket(ItemTypes.Misc, LoadWithSize(ref index, data));
				}
				LoadItemPocket(ItemTypes.PokeBalls, LoadWithSize(ref index, data));
				LoadItemPocket(ItemTypes.Berries, LoadWithSize(ref index, data));
				LoadItemPocket(ItemTypes.TMCase, LoadWithSize(ref index, data));
				LoadItemPocket(ItemTypes.KeyItems, LoadWithSize(ref index, data));
				LoadItemPocket(ItemTypes.CologneCase, LoadWithSize(ref index, data));
				LoadItemPocket(ItemTypes.DiscCase, LoadWithSize(ref index, data));

				LoadDecorationPocket(DecorationTypes.Desk, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Chair, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Plant, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Ornament, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Mat, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Poster, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Doll, LoadWithSize(ref index, data));
				LoadDecorationPocket(DecorationTypes.Cushion, LoadWithSize(ref index, data));

				LoadPokeblockCase(LoadWithSize(ref index, data));

				inventory.Items[ItemTypes.InBattle].IsOrdered = PokeManager.Settings.AutoSortItems;
				inventory.Items[ItemTypes.Valuables].IsOrdered = PokeManager.Settings.AutoSortItems;
				inventory.Items[ItemTypes.Hold].IsOrdered = PokeManager.Settings.AutoSortItems;
				inventory.Items[ItemTypes.Misc].IsOrdered = PokeManager.Settings.AutoSortItems;
				inventory.Items[ItemTypes.PokeBalls].IsOrdered = PokeManager.Settings.AutoSortItems;
			}
		}

		private void LoadItemPocket(ItemTypes pocketType, byte[] data) {
			ItemPocket pocket = inventory.Items[pocketType];

			uint count = LittleEndian.ToUInt32(data, 0);
			for (int i = 0; i < (int)count; i++) {
				pocket.AddItem(
					LittleEndian.ToUInt16(data, 4 + i * 6),
					LittleEndian.ToUInt32(data, 4 + i * 6 + 2)
				);
			}
		}

		private void LoadDecorationPocket(DecorationTypes pocketType, byte[] data) {
			DecorationPocket pocket = inventory.Decorations[pocketType];

			uint count = LittleEndian.ToUInt32(data, 0);
			for (int i = 0; i < (int)count; i++) {
				pocket.AddDecoration(
					(byte)LittleEndian.ToUInt16(data, 4 + i * 6),
					LittleEndian.ToUInt32(data, 4 + i * 6 + 2)
				);
			}
		}

		private void LoadPokeblockCase(byte[] data) {
			PokeblockCase blocks = inventory.Pokeblocks;

			uint count = LittleEndian.ToUInt32(data, 0);
			for (int i = 0; i < (int)count; i++) {
				blocks.AddPokeblock(
					(PokeblockColors)data[4 + i * 8],
					data[4 + i * 8 + 1],
					data[4 + i * 8 + 2],
					data[4 + i * 8 + 3],
					data[4 + i * 8 + 4],
					data[4 + i * 8 + 5],
					data[4 + i * 8 + 6],
					data[4 + i * 8 + 7]
				);
			}
		}

		private void LoadCompletedEvents(byte[] data) {
			int index = 0;
			int count = LittleEndian.ToSInt32(data, 0);
			index += 4;

			for (int i = 0; i < count; i++) {
				string eventID = ByteHelper.ReadString(data, index, 40);
				index += 80;
				int numEntries = LittleEndian.ToSInt32(data, index);
				index += 4;
				for (int j = 0; j < numEntries; j++) {
					PokeManager.CompleteEventBy(eventID, LittleEndian.ToUInt32(data, index));
					index += 4;
				}
			}
		}

		private void LoadPokerusStrains(byte[] data) {
			PokeManager.PokerusStrains.Clear();
			for (int i = 1; i < 16; i++) {
				if (ByteHelper.GetBit(data, 0, i))
					PokeManager.PokerusStrains.Add(new PokerusStrain((byte)i));
			}
			PokeManager.PokerusStrains.Sort((strain1, strain2) => (strain1.Order - strain2.Order));
		}


		public void Save(string filePath) {
			List<byte> data = new List<byte>();

			uint version = 5;
			data.AddRange(BitConverter.GetBytes(version));

			SaveWithSize(data, SaveTrainerInfo(version));
			SaveWithSize(data, SaveInventory(version));
			SaveWithSize(data, SavePCRows(version));
			SaveWithSize(data, mailbox.GetFinalData());
			SaveWithSize(data, SaveCompletedEvents());
			SaveWithSize(data, SavePokerusStrains());

			File.WriteAllBytes(filePath, data.ToArray());
		}

		private byte[] SavePCRows(uint version) {
			List<byte> data = new List<byte>();

			data.AddRange(BitConverter.GetBytes(pokePCs.Count));
			foreach (ManagerPokePC pokePC in pokePCs) {
				data.AddRange(ByteHelper.GetStringBytes(pokePC.Name, 30));
				data.AddRange(BitConverter.GetBytes(pokePC.IsLivingDex));
				if (version >= 3)
					data.AddRange(BitConverter.GetBytes(pokePC.RevealEggs));
				SaveWithSize(data, pokePC.GetFinalData());
			}

			return data.ToArray();
		}

		private byte[] SaveTrainerInfo(uint version) {
			List<byte> data = new List<byte>();

			playTime += new TimeSpan((DateTime.Now - lastSaveTime).Ticks);
			lastSaveTime = DateTime.Now;

			byte[] seenData = new byte[50];
			ByteHelper.SetBits(seenData, 0, 0, new BitArray(pokedexSeen));
			byte[] ownedData = new byte[50];
			ByteHelper.SetBits(ownedData, 0, 0, new BitArray(pokedexOwned));

			/* 0 */ data.AddRange(GBACharacterEncoding.GetBytes(trainerName, 7));
			/* 7 */ data.Add((byte)trainerGender);
			/* 8 */ data.AddRange(BitConverter.GetBytes(trainerID));
			/* 10 */ data.AddRange(BitConverter.GetBytes(secretID));
			/* 12 */ data.AddRange(BitConverter.GetBytes(playTime.Ticks));

			/* 20 */ data.AddRange(BitConverter.GetBytes(money));
			/* 24 */ data.AddRange(BitConverter.GetBytes(coins));
			/* 28 */ data.AddRange(BitConverter.GetBytes(battlePoints));
			/* 32 */ data.AddRange(BitConverter.GetBytes(pokeCoupons));
			/* 36 */ data.AddRange(BitConverter.GetBytes(volcanicAsh));

			/* 40 */ data.AddRange(seenData);
			/* 90 */ data.AddRange(ownedData);

			return data.ToArray();
		}

		private byte[] SaveInventory(uint version) {
			List<byte> data = new List<byte>();

			//SaveWithSize(data, SaveItemPocket(ItemTypes.Items));
			SaveWithSize(data, SaveItemPocket(ItemTypes.InBattle));
			SaveWithSize(data, SaveItemPocket(ItemTypes.Valuables));
			SaveWithSize(data, SaveItemPocket(ItemTypes.Hold));
			SaveWithSize(data, SaveItemPocket(ItemTypes.Misc));
			SaveWithSize(data, SaveItemPocket(ItemTypes.PokeBalls));
			SaveWithSize(data, SaveItemPocket(ItemTypes.Berries));
			SaveWithSize(data, SaveItemPocket(ItemTypes.TMCase));
			SaveWithSize(data, SaveItemPocket(ItemTypes.KeyItems));
			SaveWithSize(data, SaveItemPocket(ItemTypes.CologneCase));
			SaveWithSize(data, SaveItemPocket(ItemTypes.DiscCase));

			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Desk));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Chair));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Plant));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Ornament));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Mat));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Poster));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Doll));
			SaveWithSize(data, SaveDecorationPocket(DecorationTypes.Cushion));

			SaveWithSize(data, SavePokeblockCase());

			return data.ToArray();
		}
		private void SaveWithSize(List<byte> data, byte[] dataToAdd) {
			data.AddRange(BitConverter.GetBytes(dataToAdd.Length));
			data.AddRange(dataToAdd);
		}
		private byte[] SaveItemPocket(ItemTypes pocketType) {
			ItemPocket pocket = inventory.Items[pocketType];

			List<byte> data = new List<byte>();
			data.AddRange(BitConverter.GetBytes(pocket.SlotsUsed));

			for (int i = 0; i < pocket.SlotsUsed; i++) {
				data.AddRange(BitConverter.GetBytes(pocket[i].ID));
				data.AddRange(BitConverter.GetBytes(pocket[i].Count));
			}
			
			return data.ToArray();
		}
		private byte[] SaveDecorationPocket(DecorationTypes pocketType) {
			DecorationPocket pocket = inventory.Decorations[pocketType];

			List<byte> data = new List<byte>();
			data.AddRange(BitConverter.GetBytes(pocket.SlotsUsed));

			for (int i = 0; i < pocket.SlotsUsed; i++) {
				data.AddRange(BitConverter.GetBytes((ushort)pocket[i].ID));
				data.AddRange(BitConverter.GetBytes(pocket[i].Count));
			}

			return data.ToArray();
		}
		private byte[] SavePokeblockCase() {
			PokeblockCase blocks = inventory.Pokeblocks;

			List<byte> data = new List<byte>();
			data.AddRange(BitConverter.GetBytes(blocks.SlotsUsed));

			for (int i = 0; i < blocks.SlotsUsed; i++) {
				data.Add((byte)blocks[i].Color);
				data.Add(blocks[i].Spicyness);
				data.Add(blocks[i].Dryness);
				data.Add(blocks[i].Sweetness);
				data.Add(blocks[i].Bitterness);
				data.Add(blocks[i].Sourness);
				data.Add(blocks[i].Feel);
				data.Add(blocks[i].Unknown);
			}

			return data.ToArray();
		}
		private byte[] SaveCompletedEvents() {
			List<byte> data = new List<byte>();

			data.AddRange(BitConverter.GetBytes(PokeManager.NumEvents));

			for (int i = 0; i < PokeManager.NumEvents; i++) {
				EventDistribution eventDist = PokeManager.GetEventAt(i);
				data.AddRange(ByteHelper.GetStringBytes(eventDist.ID, 40));
				List<uint> completedList = PokeManager.GetCompletedEventsList(eventDist.ID);
				data.AddRange(BitConverter.GetBytes(completedList.Count));
				foreach (uint fullID in completedList) {
					data.AddRange(BitConverter.GetBytes(fullID));
				}
			}

			return data.ToArray();
		}

		private byte[] SavePokerusStrains() {
			byte[] data = new byte[2];
			foreach (PokerusStrain strain in PokeManager.PokerusStrains) {
				ByteHelper.SetBit(data, 0, strain.Value, true);
			}
			return data;
		}

		#endregion
	}
}
