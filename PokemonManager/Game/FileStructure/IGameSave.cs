using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure {
	public interface IGameSave {

		// Basic
		Generations Generation { get; }
		Platforms Platform { get; }
		GameTypes GameType { get; }
		int GameIndex { get; }
		bool IsChanged { get; set; }
		bool IsJapanese { get; set; }

		// Trainer
		string TrainerName { get; set; }
		Genders TrainerGender { get; set; }
		ushort TrainerID { get; set; }
		ushort SecretID { get; set; }

		// Play Time
		TimeSpan PlayTime { get; set; }
		ushort HoursPlayed { get; set; }
		byte MinutesPlayed { get; set; }
		byte SecondsPlayed { get; set; }
		byte FramesPlayed { get; set; }

		// Currencies
		uint Money { get; set; }
		uint Coins { get; set; }
		uint BattlePoints { get; set; }
		uint PokeCoupons { get; set; }
		uint VolcanicAsh { get; set; }

		// Containers
		Inventory Inventory { get; }
		IPokePC PokePC { get; }
		Mailbox Mailbox { get; }

		// Pokedex
		bool[] PokedexOwned { get; set; }
		bool[] PokedexSeen { get; set; }
		ushort PokemonSeen { get; }
		ushort PokemonOwned { get; }
		bool IsPokemonSeen(ushort dexID);
		bool IsPokemonOwned(ushort dexID);
		void SetPokemonSeen(ushort dexID, bool seen);
		void SetPokemonOwned(ushort dexID, bool owned);
		bool HasNationalPokedex { get; set; }
		bool IsPokedexPokemonShiny(ushort dexID);
		uint GetPokedexPokemonPersonality(ushort dexID);
		void OwnPokemon(IPokemon pokemon);

		// Loading/Saving
		void Save(string filePath);
	}
}
