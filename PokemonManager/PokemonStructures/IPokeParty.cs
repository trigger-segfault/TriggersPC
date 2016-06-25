using PokemonManager.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PokemonManager.PokemonStructures {
	public interface IPokeParty : IPokeContainer {
		void AddPokemon(IPokemon pokemon);
	}
}
