using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class PCData : GCData {

		public PCData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {
			if (parent.Inventory.Items == null)
				parent.Inventory.AddItemInventory();

			if (gameSave.GameType == GameTypes.Colosseum) {
				((ColosseumPokePC)parent.PokePC).Load(ByteHelper.SubByteArray(0, data, 9380 * 3));

				AddPCPocket(28140, 235, 999, true, false);
			}
			else {
				((XDPokePC)parent.PokePC).Load(ByteHelper.SubByteArray(0, data, 5900 * 8));

				AddPCPocket(47200, 235, 999, true, false);
			}
		}

		private void AddPCPocket(int index, uint pocketSize, uint maxStackSize, bool allowDuplicateStacks, bool ordered) {
			if (parent.Inventory.Items == null)
				parent.Inventory.AddItemInventory();
			parent.Inventory.Items.AddPocket(ItemTypes.PC, pocketSize, maxStackSize, allowDuplicateStacks, ordered);
			ItemPocket pocket = parent.Inventory.Items[ItemTypes.PC];
			for (int i = 0; i < (int)pocketSize; i++) {
				ushort id =  BigEndian.ToUInt16(raw, index + i * 4);
				// Because XD and Colosseum share the same item IDs with different items, XD items are 1000 higher. (I chose 1000 for easy reading)
				// When we save the items we'll decrement their id again.
				if (id >= 500 && gameSave.GameType == GameTypes.XD)
					id += 1000;
				ushort count = BigEndian.ToUInt16(raw, index + i * 4 + 2);
				if (id != 0)
					pocket.AddItem(id, count);
			}
		}

		private void SavePCPocket(int index) {
			ItemPocket pocket = parent.Inventory.Items[ItemTypes.PC];
			uint pocketSize = pocket.TotalSlots;
			for (int i = 0; i < (int)pocketSize; i++) {
				if (i < (int)pocket.SlotsUsed) {
					ushort id = pocket[i].ID;
					// Because XD and Colosseum share the same item IDs with different items, XD items are 1000 higher. (I chose 1000 for easy reading)
					if (id >= 1500 && gameSave.GameType == GameTypes.XD)
						id -= 1000;
					BigEndian.WriteUInt16(id, raw, index + i * 4);
					BigEndian.WriteUInt16((ushort)pocket[i].Count, raw, index + i * 4 + 2);
				}
				else {
					BigEndian.WriteUInt32(0, raw, index + i * 4);
				}
			}
		}

		public override byte[] GetFinalData() {
			if (gameSave.GameType == GameTypes.Colosseum) {
				ByteHelper.ReplaceBytes(raw, 0, ((ColosseumPokePC)parent.PokePC).GetFinalData());

				SavePCPocket(28140);
			}
			else {
				ByteHelper.ReplaceBytes(raw, 0, ((XDPokePC)parent.PokePC).GetFinalData());

				SavePCPocket(47200);
			}

			return raw;
		}
	}
}
