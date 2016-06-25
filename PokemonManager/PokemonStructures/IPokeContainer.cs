using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PokemonManager.PokemonStructures {
	public interface IPokeContainer : IEnumerable<IPokemon> {

		IPokePC PokePC { get; }
		IGameSave GameSave { get; }
		GameTypes GameType { get; }
		int GameIndex { get; }
		ContainerTypes Type { get; }

		IPokemon this[int index] { get; set; }
		int IndexOf(IPokemon pokemon);
		void Remove(IPokemon pokemon);

		uint NumPokemon { get; }
		uint NumSlots { get; }
		bool IsEmpty { get; }
	}
}
