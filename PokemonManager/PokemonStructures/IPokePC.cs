using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public interface IPokePC : IEnumerable<IPokemon> {

		IGameSave GameSave { get; }
		GameTypes GameType { get; }
		int GameIndex { get; }

		IPokeParty Party { get; }
		IDaycare Daycare { get; }
		IPokeBox this[int index] { get; }
		int CurrentBox { get; set; }
		uint NumBoxes { get; }
		bool HasRoomForPokemon(int amount);
		void PlacePokemonInNextAvailableSlot(int boxStart, int indexStart, IPokemon pokemon);

		void ApplyGameType(GameTypes gameType);
	}
}
