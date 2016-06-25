using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game {

	/** <summary>The generations supported by this app.</summary> */
	public enum Generations : byte {
		Gen1 = 0,
		Gen2 = 1,
		Gen3 = 2
	}
	/** <summary>The different platforms the supported games can be played on. Mostly needed to differentiate gen 3 GBA and GameCube games.</summary> */
	public enum Platforms : byte {
		GameBoy,
		GameBoyColor,
		GameBoyAdvance,
		GameCube,
		PC
	}
	/** <summary>The different game types available for all 3 generations.</summary> */
	public enum GameTypes : sbyte {
		Unknown = -1,
		Any = 0,

		/*Red = 1,
		Blue,
		Yellow,

		Gold = 1,
		Silver,
		Crystal,*/

		Ruby = 1,
		Sapphire,
		Emerald,
		FireRed,
		LeafGreen,
		Colosseum,
		XD,
		PokemonBox
	}
	[Flags]
	/** <summary>Flags for all the game types. This is mainly used to state which games support a certain object.</summary> */
	public enum GameTypeFlags : byte {
		None		= 0x00,
		Red			= 0x01,
		Blue		= 0x02,
		Yellow		= 0x04,
		AllGen1		= 0x07,

		Gold		= 0x01,
		Silver		= 0x02,
		Crystal		= 0x03,
		AllGen2		= 0x07,

		Ruby		= 0x01,
		Sapphire	= 0x02,
		Emerald		= 0x04,
		FireRed		= 0x08,
		LeafGreen	= 0x10,
		Colosseum	= 0x20,
		XD			= 0x40,
		AllGen3		= 0x7F
	}

	/** <summary>The gender of the trainer.</summary> */
	public enum Genders : byte {
		Male = 0,
		Female = 1,
		Genderless = 2
	}

	public enum Languages : ushort {
		NoLanguage = 0x0,
		Japanese = 0x201,
		English = 0x202,
		French = 0x203,
		Italian = 0x204,
		German = 0x205,
		Korean = 0x206,
		Spanish = 0x207
	}
}
