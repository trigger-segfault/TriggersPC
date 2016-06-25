using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class TeamAndItemsBlockData : BlockData {

		public TeamAndItemsBlockData(GBAGameSave gameSave, byte[] data, BlockDataCollection parent)
			: base(gameSave, data, parent) {

			if (parent.Inventory.Items == null)
				parent.Inventory.AddItemInventory();
			if (parent.GameCode == GameCodes.FireRedLeafGreen) {
				AddPocket(ItemTypes.PC, 664, 30, 999, false, false);
				AddPocket(ItemTypes.Items, 784, 42, 999, false, false);
				AddPocket(ItemTypes.KeyItems, 952, 30, 999, false, false);
				AddPocket(ItemTypes.PokeBalls, 1072, 13, 999, false, false);
				AddPocket(ItemTypes.TMCase, 1124, 58, 999, false, true);
				AddPocket(ItemTypes.Berries, 1356, 43, 999, false, true);
				parent.PokePC.AddParty(ByteHelper.SubByteArray(52, raw, 604));
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				AddPocket(ItemTypes.PC, 1176, 50, 999, true, false);
				AddPocket(ItemTypes.Items, 1376, 30, 99, true, false);
				AddPocket(ItemTypes.KeyItems, 1496, 30, 99, true, false);
				AddPocket(ItemTypes.PokeBalls, 1616, 16, 99, true, false);
				AddPocket(ItemTypes.TMCase, 1680, 64, 99, false, true);
				AddPocket(ItemTypes.Berries, 1936, 46, 999, false, true);
				AddPokeblockCase(2120, 40);
				parent.PokePC.AddParty(ByteHelper.SubByteArray(564, raw, 604));
			}
			else {
				AddPocket(ItemTypes.PC, 1176, 50, 999, true, false);
				AddPocket(ItemTypes.Items, 1376, 20, 99, true, false);
				AddPocket(ItemTypes.KeyItems, 1456, 20, 99, true, false);
				AddPocket(ItemTypes.PokeBalls, 1536, 16, 99, true, false);
				AddPocket(ItemTypes.TMCase, 1600, 64, 99, false, true);
				AddPocket(ItemTypes.Berries, 1856, 46, 999, false, true);
				AddPokeblockCase(2040, 40);
				parent.PokePC.AddParty(ByteHelper.SubByteArray(564, raw, 604));
			}

		}

		private void AddPokeblockCase(int index, uint caseSize) {
			parent.Inventory.AddPokeblockCase(caseSize);
			for (int i = 0; i < parent.Inventory.Pokeblocks.TotalSlots; i++) {
				PokeblockColors color = (PokeblockColors)raw[index + i * 8];
				byte spicy = raw[index + i * 8 + 1];
				byte dry = raw[index + i * 8 + 2];
				byte sweet = raw[index + i * 8 + 3];
				byte bitter = raw[index + i * 8 + 4];
				byte sour = raw[index + i * 8 + 5];
				byte feel = raw[index + i * 8 + 6];
				byte unknown = raw[index + i * 8 + 7];
				if (color != PokeblockColors.None)
					parent.Inventory.Pokeblocks.AddPokeblock(color, spicy, dry, sweet, bitter, sour, feel, unknown);
			}
		}

		private void SavePokeblockCase(int index) {
			for (int i = 0; i < (int)parent.Inventory.Pokeblocks.TotalSlots; i++) {
				if (i < (int)parent.Inventory.Pokeblocks.SlotsUsed) {
					Pokeblock pokeblock = parent.Inventory.Pokeblocks.GetPokeblockAt(i);
					raw[index + i * 8] = (byte)pokeblock.Color;
					raw[index + i * 8 + 1] = pokeblock.Spicyness;
					raw[index + i * 8 + 2] = pokeblock.Dryness;
					raw[index + i * 8 + 3] = pokeblock.Sweetness;
					raw[index + i * 8 + 4] = pokeblock.Bitterness;
					raw[index + i * 8 + 5] = pokeblock.Sourness;
					raw[index + i * 8 + 6] = pokeblock.Feel;
					raw[index + i * 8 + 7] = pokeblock.Unknown;
				}
				else {
					LittleEndian.WriteUInt64(0, raw, index + i * 8);
				}
			}
		}

		private void AddPocket(ItemTypes pocketType, int index, uint pocketSize, uint maxStackSize, bool allowDuplicateStacks, bool ordered) {
			parent.Inventory.Items.AddPocket(pocketType, pocketSize, maxStackSize, allowDuplicateStacks, ordered);
			ItemPocket pocket = parent.Inventory.Items[pocketType];
			ushort securityKey = (ushort)(pocketType == ItemTypes.PC ? 0 : parent.UInt16SecurityKey);
			for (int i = 0; i < (int)pocketSize; i++) {
				ushort id = LittleEndian.ToUInt16(raw, index + i * 4);
				ushort count = (ushort)((uint)LittleEndian.ToUInt16(raw, index + i * 4 + 2) ^ securityKey);
				if (id != 0)
					pocket.AddItem(id, count);
			}
		}

		private void SavePocket(ItemTypes pocketType, int index) {
			ItemPocket pocket = parent.Inventory.Items[pocketType];
			uint pocketSize = pocket.TotalSlots;
			ushort securityKey = (ushort)(pocketType == ItemTypes.PC ? 0 : parent.UInt16SecurityKey);
			for (int i = 0; i < (int)pocketSize; i++) {
				if (i < (int)pocket.SlotsUsed) {
					LittleEndian.WriteUInt16(pocket[i].ID, raw, index + i * 4);
					LittleEndian.WriteUInt16((ushort)(pocket[i].Count ^ securityKey), raw, index + i * 4 + 2);
				}
				else {
					LittleEndian.WriteUInt16(0, raw, index + i * 4);
					LittleEndian.WriteUInt16(securityKey, raw, index + i * 4 + 2);
				}
			}
		}

		public uint Money {
			get {
				if (parent.GameCode == GameCodes.FireRedLeafGreen)
					return LittleEndian.ToUInt32(raw, 656) ^ parent.SecurityKey;
				else
					return LittleEndian.ToUInt32(raw, 1168) ^ parent.SecurityKey;
			}
			set {
				if (parent.GameCode == GameCodes.FireRedLeafGreen)
					LittleEndian.WriteUInt32(value ^ parent.SecurityKey, raw, 656);
				else
					LittleEndian.WriteUInt32(value ^ parent.SecurityKey, raw, 1168);
			}
		}
		public ushort Coins {
			get {
				if (parent.GameCode == GameCodes.FireRedLeafGreen)
					return (ushort)(LittleEndian.ToUInt16(raw, 660) ^ parent.UInt16SecurityKey);
				else
					return (ushort)(LittleEndian.ToUInt16(raw, 1172) ^ parent.UInt16SecurityKey);
			}
			set {
				if (parent.GameCode == GameCodes.FireRedLeafGreen)
					LittleEndian.WriteUInt16((ushort)(value ^ parent.UInt16SecurityKey), raw, 660);
				else
					LittleEndian.WriteUInt16((ushort)(value ^ parent.UInt16SecurityKey), raw, 1172);
			}
		}

		public bool IsPokemonSeenB(ushort dexID) {
			int index = (parent.GameCode == GameCodes.RubySapphire ? 2360 : (parent.GameCode == GameCodes.Emerald ? 2440 : 1528));
			return ByteHelper.GetBit(raw, index, dexID - 1);
		}
		public void SetPokemonSeenB(ushort dexID, bool seen) {
			int index = (parent.GameCode == GameCodes.RubySapphire ? 2360 : (parent.GameCode == GameCodes.Emerald ? 2440 : 1528));
			ByteHelper.SetBit(raw, index, dexID - 1, seen);
		}

		public bool[] PokedexSeenB {
			get {
				int index = (parent.GameCode == GameCodes.RubySapphire ? 2360 : (parent.GameCode == GameCodes.Emerald ? 2440 : 1528));
				BitArray bitArray = ByteHelper.GetBits(raw, index, 0, 386);
				bool[] flags = new bool[386];
				for (int i = 0; i < 386; i++)
					flags[i] = bitArray[i];
				return flags;
			}
			set {
				int index = (parent.GameCode == GameCodes.RubySapphire ? 2360 : (parent.GameCode == GameCodes.Emerald ? 2440 : 1528));
				ByteHelper.SetBits(raw, index, 0, new BitArray(value));
			}
		}
		public List<bool> GameFlagsPart1 {
			get {
				BitArray bitArray = null;
				if (parent.GameCode == GameCodes.RubySapphire || parent.GameCode == GameCodes.Emerald)
					return new List<bool>();
				else // FRLG first half of flags: (ends with flag 0x4FF)
					bitArray = ByteHelper.GetBits(raw, 0, 0, 0xAFF);
				List<bool> list = new List<bool>();
				foreach (bool b in bitArray)
					list.Add(b);
				return list;
			}
		}
		public bool GetGameFlag(int index) {
			if (parent.GameCode == GameCodes.FireRedLeafGreen && index < 0x500)
				return ByteHelper.GetBit(raw, 3808, index);
			return false;
		}
		public void SetGameFlag(int index, bool flag) {
			if (parent.GameCode == GameCodes.FireRedLeafGreen && index < 0x500)
				ByteHelper.SetBit(raw, 3808, index, flag);
		}

		public override byte[] GetFinalData() {
			if (parent.GameCode == GameCodes.RubySapphire) {
				SavePocket(ItemTypes.PC, 1176);
				SavePocket(ItemTypes.Items, 1376);
				SavePocket(ItemTypes.KeyItems, 1456);
				SavePocket(ItemTypes.PokeBalls, 1536);
				SavePocket(ItemTypes.TMCase, 1600);
				SavePocket(ItemTypes.Berries, 1856);
				SavePokeblockCase(2040);
				ByteHelper.ReplaceBytes(raw, 564, ((GBAPokeParty)parent.PokePC.Party).GetFinalData());
			}
			else if (parent.GameCode == GameCodes.FireRedLeafGreen) {
				SavePocket(ItemTypes.PC, 664);
				SavePocket(ItemTypes.Items, 784);
				SavePocket(ItemTypes.KeyItems, 952);
				SavePocket(ItemTypes.PokeBalls, 1072);
				SavePocket(ItemTypes.TMCase, 1124);
				SavePocket(ItemTypes.Berries, 1356);
				ByteHelper.ReplaceBytes(raw, 52, ((GBAPokeParty)parent.PokePC.Party).GetFinalData());
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				SavePocket(ItemTypes.PC, 1176);
				SavePocket(ItemTypes.Items, 1376);
				SavePocket(ItemTypes.KeyItems, 1496);
				SavePocket(ItemTypes.PokeBalls, 1616);
				SavePocket(ItemTypes.TMCase, 1680);
				SavePocket(ItemTypes.Berries, 1936);
				SavePokeblockCase(2120);
				ByteHelper.ReplaceBytes(raw, 564, ((GBAPokeParty)parent.PokePC.Party).GetFinalData());
			}
			Checksum = CalculateChecksum();
			return raw;
		}
	}
}
