using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class PlayerData : GCData {

		public PlayerData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {
			if (parent.Inventory.Items == null)
				parent.Inventory.AddItemInventory();

			if (gameSave.GameType == GameTypes.Colosseum) {
				AddPocket(ItemTypes.Items, 1920, 20, 999, true, false);
				AddPocket(ItemTypes.KeyItems, 2000, 43, 999, true, false);
				AddPocket(ItemTypes.PokeBalls, 2172, 16, 999, true, false);
				AddPocket(ItemTypes.TMCase, 2236, 64, 999, false, true);
				AddPocket(ItemTypes.Berries, 2492, 46, 999, false, true);
				AddPocket(ItemTypes.CologneCase, 2676, 3, 999, false, true);
				((ColosseumPokePC)parent.PokePC).AddParty(ByteHelper.SubByteArray(48, data, 1872));
			}
			else {
				AddPocket(ItemTypes.Items, 1224, 30, 999, true, false);
				AddPocket(ItemTypes.KeyItems, 1344, 43, 999, true, false);
				AddPocket(ItemTypes.PokeBalls, 1516, 16, 999, true, false);
				AddPocket(ItemTypes.TMCase, 1580, 64, 999, false, true);
				AddPocket(ItemTypes.Berries, 1836, 46, 999, false, true);
				AddPocket(ItemTypes.CologneCase, 2020, 3, 999, false, true);
				AddPocket(ItemTypes.DiscCase, 2032, 60, 1, false, true);
				((XDPokePC)parent.PokePC).AddParty(ByteHelper.SubByteArray(48, data, 1176));
			}
		}

		private void AddPocket(ItemTypes pocketType, int index, uint pocketSize, uint maxStackSize, bool allowDuplicateStacks, bool ordered) {
			parent.Inventory.Items.AddPocket(pocketType, pocketSize, maxStackSize, allowDuplicateStacks, ordered);
			ItemPocket pocket = parent.Inventory.Items[pocketType];
			for (int i = 0; i < (int)pocketSize; i++) {
				ushort id =  BigEndian.ToUInt16(raw, index + i * 4);
				if (gameSave.GameType == GameTypes.XD) {
					// Because XD and Colosseum share the same item IDs with different items, XD items are 1000 higher. (I chose 1000 for easy reading)
					// When we save the items we'll decrement their id again.
					if (id >= 500)
						id += 1000;
					// Share Colognes amd cologne case between games so make sure we use one of the game's ID's, We're using Colosseum's IDs
					if (id >= 1512 && id <= 1515)
						id -= 970;
					// Two Key items that are shared between Colosseum and XD
					if (id == 1501)
						id -= 1000;
					if (id == 1505)
						id -= 975;
				}
				ushort count = BigEndian.ToUInt16(raw, index + i * 4 + 2);
				if (id != 0)
					pocket.AddItem(id, count);
			}
		}
		private void SavePocket(ItemTypes pocketType, int index) {
			ItemPocket pocket = parent.Inventory.Items[pocketType];
			uint pocketSize = pocket.TotalSlots;
			for (int i = 0; i < (int)pocketSize; i++) {
				if (i < (int)pocket.SlotsUsed) {
					ushort id = pocket[i].ID;
					if (gameSave.GameType == GameTypes.XD) {
						// Share Colognes and cologne case between games so make sure we use one of the game's ID's, We're using Colosseum's IDs
						if (id >= 542 && id <= 545)
							id += 970;
						// Two Key items that are shared between Colosseum and XD
						if (id == 501)
							id += 1000;
						if (id == 530)
							id += 975;
						// Because XD and Colosseum share the same item IDs with different items, XD items are 1000 higher. (I chose 1000 for easy reading)
						if (id >= 1500)
							id -= 1000;
					}
					BigEndian.WriteUInt16(id, raw, index + i * 4);
					BigEndian.WriteUInt16((ushort)pocket[i].Count, raw, index + i * 4 + 2);
				}
				else {
					BigEndian.WriteUInt32(0, raw, index + i * 4);
				}
			}
		}
		
		public string TrainerName {
			get { return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(0, raw, 20), gameSave.IsJapanese ? Languages.Japanese : Languages.English); }
			set { ByteHelper.ReplaceBytes(raw, 0, GCCharacterEncoding.GetBytes(value, 10, gameSave.IsJapanese ? Languages.Japanese : Languages.English)); }
		}
		public Genders TrainerGender {//2,272
			get {
				if (gameSave.GameType == GameTypes.Colosseum)
					return (raw[2688] != 1 ? Genders.Male : Genders.Female);
				else
					return (raw[2272] != 1 ? Genders.Male : Genders.Female);
			}
			set {
				if (gameSave.GameType == GameTypes.Colosseum)
					raw[2688] = (byte)(value == Genders.Male ? 0 : 1);
				else
					raw[2272] = (byte)(value == Genders.Male ? 0 : 1);
			}
		}
		public ushort TrainerID {
			get { return BigEndian.ToUInt16(raw, 46); }
			set { BigEndian.WriteUInt16(value, raw, 46); }
		}
		public ushort SecretID {
			get { return BigEndian.ToUInt16(raw, 44); }
			set { BigEndian.WriteUInt16(value, raw, 44); }
		}
		public string RuisName {
			get {
				if (gameSave.GameType == GameTypes.Colosseum)
					return GCCharacterEncoding.GetString(ByteHelper.SubByteArray(2754, raw, 20), gameSave.IsJapanese ? Languages.Japanese : Languages.English);
				return "";
			}
			set {
				if (gameSave.GameType == GameTypes.Colosseum)
					ByteHelper.ReplaceBytes(raw, 2754, GCCharacterEncoding.GetBytes(value, 10, gameSave.IsJapanese ? Languages.Japanese : Languages.English));
			}
		}
		public uint Money {
			get {
				if (gameSave.GameType == GameTypes.Colosseum)
					return BigEndian.ToUInt32(raw, 2692);
				else
					return BigEndian.ToUInt32(raw, 2276);
			}
			set {
				if (gameSave.GameType == GameTypes.Colosseum)
					BigEndian.WriteUInt32(value, raw, 2692);
				else
					BigEndian.WriteUInt32(value, raw, 2276);
			}
		}
		public uint PokeCoupons {
			get {
				if (gameSave.GameType == GameTypes.Colosseum)
					return BigEndian.ToUInt32(raw, 2696);
				else
					return BigEndian.ToUInt32(raw, 2280);
			}
			set {
				if (gameSave.GameType == GameTypes.Colosseum)
					BigEndian.WriteUInt32(value, raw, 2696);
				else
					BigEndian.WriteUInt32(value, raw, 2280);
			}
		}

		public override byte[] GetFinalData() {
			if (gameSave.GameType == GameTypes.Colosseum) {
				SavePocket(ItemTypes.Items, 1920);
				SavePocket(ItemTypes.KeyItems, 2000);
				SavePocket(ItemTypes.PokeBalls, 2172);
				SavePocket(ItemTypes.TMCase, 2236);
				SavePocket(ItemTypes.Berries, 2492);
				SavePocket(ItemTypes.CologneCase, 2676);
				ByteHelper.ReplaceBytes(raw, 48, ((ColosseumPokeParty)parent.PokePC.Party).GetFinalData());
			}
			else {
				SavePocket(ItemTypes.Items, 1224);
				SavePocket(ItemTypes.KeyItems, 1344);
				SavePocket(ItemTypes.PokeBalls, 1516);
				SavePocket(ItemTypes.TMCase, 1580);
				SavePocket(ItemTypes.Berries, 1836);
				SavePocket(ItemTypes.CologneCase, 2020);
				SavePocket(ItemTypes.DiscCase, 2032);
				ByteHelper.ReplaceBytes(raw, 48, ((XDPokeParty)parent.PokePC.Party).GetFinalData());
			}
			return raw;
		}
	}
}
