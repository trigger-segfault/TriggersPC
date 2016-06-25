using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class PokeBoxBlockData : BlockData {

		public PokeBoxBlockData(GBAGameSave gameSave, byte[] data, BlockDataCollection parent)
			: base(gameSave, data, parent) {
			
		}

		public byte[] GetBoxData() {
			if (SectionID == SectionTypes.PCBufferI)
				return ByteHelper.SubByteArray(0, raw, 2000);
			return ByteHelper.SubByteArray(0, raw, 3968);
		}

		public void OverrideBoxData(byte[] data) {
			ByteHelper.ReplaceBytes(raw, 0, data);
			Checksum = CalculateChecksum();
		}
	}
}
