using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public enum PokemonTypes : byte{
		None = 0,
		Normal = 1,
		Fighting = 2,
		Flying = 3,
		Poison = 4,
		Ground = 5,
		Rock = 6,
		Bug = 7,
		Ghost = 8,
		Steel = 9,
		Fire = 10,
		Water = 11,
		Grass = 12,
		Electric = 13,
		Psychic = 14,
		Ice = 15,
		Dragon = 16,
		Dark = 17,
		Shadow = 18
	}

	public enum ContainerTypes : byte {
		Box,
		Party,
		Daycare,
		Purifier
	}

	public enum PokeBoxWallpapers : byte {
		Forest = 0,
		City = 1,
		Desert = 2,
		Savanna = 3,
		Crag = 4,
		Volcano = 5,
		Snow = 6,
		Cave = 7,
		Beach = 8,
		Seafloor = 9,
		River = 10,
		Sky = 11,
		Polkadot = 12, //RSE (Stars FRLG)
		PokeCenter = 13,
		Machine = 14, //RSE (Tiles FRLG)
		Simple = 15,
		Special //E Only
	}
	public enum PokemonBoxPokeBoxWallpapers : byte {
		Forest = 0,
		City = 1,
		Desert = 2,
		Savanna = 3,
		Crag = 4,
		Volcano = 5,
		Snow = 6,
		Cave = 7,
		Beach = 8,
		Seafloor = 9,
		River = 10,
		Sky = 11,
		Polkadot = 12,
		PokeCenter = 13,
		Machine = 14,
		Simple = 15,
		Flower = 16,
		Tile = 17,
		Carpet = 18,
		Ruin = 19
	}

	public enum ColosseumPokeBoxWallpapers : byte {
		Forest,
		Seafloor,
		Desert
	}
	public enum XDPokeBoxWallpapers : byte {
		Desert,
		Polkadot,
		Tiles,
		Forest,
		City,
		Cave,
		Seafloor,
		Volcano
	}
		

	public enum ManagerPokeBoxWallpapers : byte {
		ForestRS,
		ForestE,
		CityRS,
		CityE,
		DesertRS,
		DesertE,
		SavannaRS,
		SavannaE,
		CragRS,
		CragE,
		VolcanoRS,
		VolcanoE,
		SnowRS,
		SnowE,
		CaveRS,
		CaveE,
		BeachRS,
		BeachE,
		SeafloorRS,
		SeafloorE,
		RiverRS,
		RiverE,
		SkyRS,
		SkyE,
		PolkadotRS,
		PolkadotE,
		StarsFRLG,
		PokeCenterRS,
		PokeCenterE,
		PokeCenterFRLG,
		MachineRS,
		MachineE,
		TilesFRLG,
		SimpleRS,
		SimpleE,
		SimpleFRLG,
		SeafloorColoRS,
		SeafloorColoE,
		FlowerBox,
		TileBox,
		CarpetBox,
		RuinBox
	}


	public enum ConditionTypes : byte {
		None = 0,
		Cool = 1,
		Beauty = 2,
		Cute = 3,
		Smart = 4,
		Tough = 5,
		Feel,
		Any,
		All,
		Total
	}

	public enum EvolutionTypes : byte {
		Level,
		Item,
		Trade,
		Friendship,
		Stat,
		Condition,
		Personality,
		NinjaskShedinja
	}

	public enum EvolutionParameters : byte {
		// Stat Type
		AttackOverDefense,
		DefenseOverAttack,
		AttackAndDefense,

		// Personality Type
		LessFive,
		GreaterEqualFive,

		// Time
		Day,
		Night
	}

	public enum ExperienceGroups : byte {
		Fast,
		MediumFast,
		MediumSlow,
		Slow,
		Fluctuating,
		Erratic
	}

	public enum GameOrigins : byte {
		ColosseumBonusDisc = 0,
		Sapphire = 1,
		Ruby = 2,
		Emerald = 3,
		FireRed = 4,
		LeafGreen = 5,
		ColosseumXD = 15,
		Unknown = 16
	}
	public enum StatTypes : byte {
		None = 0,
		Attack = 1,
		Defense = 2,
		Speed = 3,
		SpAttack = 4,
		SpDefense = 5,
		HP = 6,
		Any,
		All,
		Total
	}
	public enum MoveCategories : byte {
		Physical,
		Special,
		Status
	}

	public enum EggGroups : byte {
		None,
		Field,
		Monster,
		Grass,
		Bug,
		Flying,
		Mineral,
		Water1,
		Water2,
		Water3,
		Fairy,
		Dragon,
		Humanoid,
		Amorphous,
		DittoOnly,
		Undiscovered,
		Ditto
	}

	[Flags]
	public enum MarkingFlags : byte {
		None = 0x0,
		Circle = 0x1,
		Square = 0x2,
		Triangle = 0x4,
		Heart = 0x8,
		All = 0xF
	}

	[Flags]
	public enum StatusConditionFlags : byte {
		None = 0x00,
		Sleep1 = 0x01,
		Sleep2 = 0x02,
		Sleep3 = 0x03,
		Sleep4 = 0x04,
		Sleep5 = 0x05,
		Sleep6 = 0x06,
		Sleep7 = 0x07,
		Poisoned = 0x08,
		Burned = 0x10,
		Frozen = 0x20,
		Paralyzed = 0x40,
		BadlyPoisoned = 0x80
	}
	public enum PokerusStrainTypes : sbyte {
		None = -1,
		StrainA = 0,
		StrainB = 1,
		StrainC = 2,
		StrainD = 3
	}

	public enum PokerusStrainVariations : sbyte {
		None = -1,
		VariationW = 0,
		VariationX = 1,
		VariationY = 2,
		VariationZ = 3
	}

	public struct PokerusStrain {
		public byte Value;
		public PokerusStrainTypes Strain {
			get { return (Value != 0 ? (PokerusStrainTypes)ByteHelper.BitsToByte(BitConverter.GetBytes(Value), 0, 0, 2) : PokerusStrainTypes.None); }
		}
		public PokerusStrainVariations Variation {
			get { return (Value != 0 ? (PokerusStrainVariations)ByteHelper.BitsToByte(BitConverter.GetBytes(Value), 0, 2, 2) : PokerusStrainVariations.None); }
		}
		public int Order {
			get { return (int)Strain * 16 + (int)Variation; }
		}

		public PokerusStrain(byte value) {
			Value = value;
		}

		public override string ToString() {
			if (Value == 0)
				return "No Pokérus";
			string output = "Strain " + new string((char)((int)'A' + (int)Strain), 1);
			output += ", Variation " + new string((char)((int)'W' + (int)Variation), 1);
			//output += " (" + ((int)Strain + 1).ToString() + " Days)";
			return output;
		}

		public override bool Equals(object obj) {
			if (obj is PokerusStrain)
				return ((PokerusStrain)obj).Value == Value;
			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
	}

	public enum SubDataOrder : byte {
		GAEM,
		GAME,
		GEAM,
		GEMA,
		GMAE,
		GMEA,
		AGEM,
		AGME,
		AEGM,
		AEMG,
		AMGE,
		AMEG,
		EGAM,
		EGMA,
		EAGM,
		EAMG,
		EMGA,
		EMAG,
		MGAE,
		MGEA,
		MAGE,
		MAEG,
		MEGA,
		MEAG,
	}

	public enum PokerusStatuses : byte {
		None,
		Infected,
		Cured,
		Invalid
	}

	public enum PokemonFormatTypes : byte {
		Gen1,
		Gen2,
		Gen3GBA,
		Gen3Colosseum,
		Gen3XD,
		Gen3PokemonBox
	}

	public enum MoveSource : byte {
		LevelUp,
		Egg,
		TMHM,
		Tutor,
		Purification,
		Other
	}
}
