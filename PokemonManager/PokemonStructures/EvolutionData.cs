using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class EvolutionData {

		private ushort dexID;

		private EvolutionTypes type;
		private int[] parameters;

		public EvolutionData(string script) {
			string[] sides = script.Split(':');
			string pokemon = sides[0];
			string[] methods = sides[1].Split(new string[]{ "(" }, StringSplitOptions.RemoveEmptyEntries);
			string methodType = methods[0];
			string[] methodParameters = new string[0];
			if (methods.Length == 2)
				methodParameters = methods[1].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			PokemonData pokemonData = PokemonDatabase.GetPokemonFromName(pokemon);
			if (pokemonData == null) {
				Console.WriteLine("");
			}
			else {
				// Get dex ID
				this.dexID = pokemonData.DexID;

				// Get Evolution Type
				EvolutionTypes[] types = (EvolutionTypes[])Enum.GetValues(typeof(EvolutionTypes));
				foreach (EvolutionTypes t in types) {
					if (t.ToString().ToUpper() == methodType.ToUpper()) {
						this.type = t;
						break;
					}
				}

				// Get Parameters
				this.parameters = new int[methodParameters.Length];
				for (int i = 0; i < methodParameters.Length; i++) {
					methodParameters[i] = methodParameters[i].Replace(")", "");
					this.parameters[i] = -1;
					EvolutionParameters[] evoParamTypes = (EvolutionParameters[])Enum.GetValues(typeof(EvolutionParameters));
					foreach (EvolutionParameters p in evoParamTypes) {
						if (p.ToString().ToUpper() == methodParameters[i].ToUpper()) {
							this.parameters[i] = (int)p;
							break;
						}
					}
					if (this.parameters[i] == -1) {
						ItemData itemData = ItemDatabase.GetItemFromName(methodParameters[i]);
						if (itemData != null)
							this.parameters[i] = itemData.ID;
					}
					if (methodParameters[i] == "BEAUTY") {
						// Ignore
						this.parameters[i] = 0;
					}
					if (this.parameters[i] == -1) {
						if (!int.TryParse(methodParameters[i], out this.parameters[i])) {
							Console.WriteLine("Error reading evolution parameter " + methodParameters[i]);
						}
					}
				}
			}
		}

		public ushort DexID {
			get { return dexID; }
		}
		public EvolutionTypes EvolutionType {
			get { return type; }
		}
		public int[] Parameters {
			get { return parameters; }
		}
		public int NumParameters {
			get { return parameters.Length; }
		}
		public bool HasParameters {
			get { return parameters.Length > 0; }
		}
	}
}
