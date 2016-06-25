using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Windows.Controls {
	public interface IPokemonViewer {

		void LoadPokemon(IPokemon pokemon);
		void UnloadPokemon();
		void RefreshUI();
		IPokemon ViewedPokemon { get; }
	}
}
