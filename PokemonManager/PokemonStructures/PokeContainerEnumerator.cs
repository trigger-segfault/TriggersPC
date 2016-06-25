using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class PokeContainerEnumerator : IEnumerator<IPokemon> {

		private IPokeContainer pokeContainer;
		private int index;

		public PokeContainerEnumerator(IPokeContainer container) {
			this.pokeContainer	= container;
			this.index			= -1;
		}

		public IPokemon Current {
			get {
				if (index >= 0 && index <= pokeContainer.NumSlots)
					return pokeContainer[index];
				return null;
			}
		}
		public bool MoveNext() {
			if (index < pokeContainer.NumSlots)
				index++;
			while (index < pokeContainer.NumSlots) {
				if (pokeContainer[index] != null)
					return true;
				index++;
			}
			return false;
		}
		public void Reset() {
			index = -1;
		}

		object IEnumerator.Current {
			get { return Current; }
		}
		void IDisposable.Dispose() { }
	}
}
