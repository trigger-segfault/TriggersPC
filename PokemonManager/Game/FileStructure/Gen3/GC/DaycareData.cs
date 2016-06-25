using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class DaycareData : GCData {

		public DaycareData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {
			if (parent.Inventory.Items == null)
				parent.Inventory.AddItemInventory();

			if (gameSave.GameType == GameTypes.Colosseum) {
				((ColosseumPokePC)parent.PokePC).AddDaycare(ByteHelper.SubByteArray(0, data, 0x140));
			}
			else {
				((XDPokePC)parent.PokePC).AddDaycare(ByteHelper.SubByteArray(0, data, 0xCC));
			}
		}

		public override byte[] GetFinalData() {
			if (gameSave.GameType == GameTypes.Colosseum) {
				ByteHelper.ReplaceBytes(raw, 0, ((ColosseumDaycare)parent.PokePC.Daycare).GetFinalData());
			}
			else {
				ByteHelper.ReplaceBytes(raw, 0, ((XDDaycare)parent.PokePC.Daycare).GetFinalData());
			}

			return raw;
		}
	}
}
