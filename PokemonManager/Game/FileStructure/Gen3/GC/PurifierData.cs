using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	

	public class PurifierData : GCData {


		public PurifierData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {

			((XDPokePC)parent.PokePC).AddPurifier((byte[])data.Clone());
		}

		public override byte[] GetFinalData() {
			ByteHelper.ReplaceBytes(raw, 0, ((XDPokePC)parent.PokePC).GetPurifierFinalData());

			return raw;
		}

	}
}
