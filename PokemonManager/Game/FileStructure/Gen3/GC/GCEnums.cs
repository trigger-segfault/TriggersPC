using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {

	public enum SaveMagic : uint {
		Unknown = 0x00000000,
		Colosseum = 0x01010000,
		XD = 0x01010100
	}

	public enum GCGameOrigins : byte {
		ColosseumBonusDisc = 0,
		FireRed = 1,
		LeafGreen = 2,
		Sapphire = 8,
		Ruby = 9,
		Emerald = 10,
		ColosseumXD = 11
	}
	public enum GCLanguages : byte {
		NoLanguage = 0,
		Japanese = 1,
		English = 2,
		German = 3,
		French = 4,
		Italian = 5,
		Spanish = 6
	}
	public enum GCRegions : byte {
		NoRegion,
		NTSC_J,
		NTSC_U,
		PAL
	}
	public enum GCStatusConditions : ushort {
		None = 0,
		Poisoned = 3,
		BadlyPoisoned = 4,
		Paralyzed = 5,
		Burned = 6,
		Frozen = 7,
		Asleep = 8
	}
	public enum GCEncounterTypes {
		None = 0,
		FatefulEncounter = 1,
		Gift = 0x50,
		Normal = 0xb8
	}
}
