using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	/** <summary>The section type codes for each of the 14 block data types.</summary> */
	public enum SectionTypes : ushort {
		TrainerInfo = 0,
		TeamAndItems = 1,
		Unknown1 = 2,
		Unknown2 = 3,
		RivalInfo = 4,
		PCBufferA = 5,
		PCBufferB = 6,
		PCBufferC = 7,
		PCBufferD = 8,
		PCBufferE = 9,
		PCBufferF = 10,
		PCBufferG = 11,
		PCBufferH = 12,
		PCBufferI = 13
	}
	/** <summary>The game codes used to determine the game type of the save file.</summary> */
	public enum GameCodes : uint {
		RubySapphire = 0,
		FireRedLeafGreen = 1,
		Emerald = 2
	}

	public enum AlteringCavePokemon : ushort {
		Zubat = 0,
		Mareep = 1,
		Pineco = 2,
		Houndour = 3,
		Teddiursa = 4,
		Aipom = 5,
		Shuckle = 6,
		Stantler = 7,
		Smeargle = 8
	}
}
