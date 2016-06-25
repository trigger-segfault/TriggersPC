using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class PokemonFinder {
		public IPokemon Pokemon { get; set; }

		public PokemonFinder(IPokemon pokemon) {
			this.Pokemon = pokemon;
		}
	}
}
