using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {

	public enum EasyChatGroups : byte {
		Pokemon1 = 0x00,
		Group1Game = 0x02,
		Group2TypesAbilitiesConditions = 0x04,
		Group3Challenges = 0x06,
		Group4Responses = 0x08,
		Group5People = 0x0A,
		Group6Exclamations = 0x0C,
		Group7Listen = 0x0E,
		Group8Will = 0x10,
		Group9Meet = 0x12,
		Group10Hot = 0x14,
		Group11Meets = 0x16,
		Group12Chores = 0x18,
		Group13Idol = 0x1A,
		Group14Fall = 0x1C,
		Group15Highs = 0x1E,
		Group16Wandering = 0x20,
		Group17Appeal = 0x22,
		Moves = 0x26,
		Pokemon2 = 0x2A
	}


	public enum ItemTypes : byte {
		Unknown,
		Items,
		KeyItems,
		PokeBalls,
		TMCase,
		Berries,
		CologneCase,
		DiscCase,
		PC,
		InBattle,
		Hold,
		Vitamins,
		Evolution,
		Valuables,
		Misc,
		Any,
		TheVoid
	}
	public enum DecorationTypes : byte {
		Unknown,
		Desk,
		Chair,
		Plant,
		Ornament,
		Mat,
		Poster,
		Doll,
		Cushion
	}
	public enum PokeblockColors : byte {
		None = 0,
		Red,
		Blue,
		Pink,
		Green,
		Yellow,
		Purple,
		Indigo,
		Brown,
		LiteBlue,
		Olive,
		Gray,
		Black,
		White,
		Gold
	}
	public enum PokeblockFlavorTypes : byte {
		None = 0,
		Spicy,
		Dry,
		Sweet,
		Bitter,
		Sour
	}


	public enum DecorationUsages : byte {
		Unused,
		SecretBase,
		Bedroom
	}
	public enum CurrencyTypes : byte {
		Money,
		Coins,
		BattlePoints,
		PokeCoupons,
		VolcanicAsh
	}
}
