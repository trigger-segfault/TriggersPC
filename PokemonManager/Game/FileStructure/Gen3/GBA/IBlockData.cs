using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public interface IBlockData {

		byte[] Raw { get; }

		uint SaveIndex { get; set; }
		SectionTypes SectionID { get; set; }
		ushort Checksum { get; set; }
		BlockDataCollection Parent { get; set; }
		byte[] GetFinalData();

		ushort CalculateChecksum();
	}
}
