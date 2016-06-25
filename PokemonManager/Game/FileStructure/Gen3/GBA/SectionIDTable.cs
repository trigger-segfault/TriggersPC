using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public static class SectionIDTable {
		private static Dictionary<SectionTypes, int> table = new Dictionary<SectionTypes, int>() {
			{SectionTypes.TrainerInfo, 3884},
			{SectionTypes.TeamAndItems, 3968},
			{SectionTypes.Unknown1, 3968},
			{SectionTypes.Unknown2, 3968},
			{SectionTypes.RivalInfo, 3848},
			{SectionTypes.PCBufferA, 3968},
			{SectionTypes.PCBufferB, 3968},
			{SectionTypes.PCBufferC, 3968},
			{SectionTypes.PCBufferD, 3968},
			{SectionTypes.PCBufferE, 3968},
			{SectionTypes.PCBufferF, 3968},
			{SectionTypes.PCBufferG, 3968},
			{SectionTypes.PCBufferH, 3968},
			{SectionTypes.PCBufferI, 2000}
		};

		public static int GetContents(SectionTypes sectionType) {
			return SectionIDTable.table[sectionType];
		}
	}
}
