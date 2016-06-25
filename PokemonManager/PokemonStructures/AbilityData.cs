using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class AbilityData {

		private byte id;
		private string name;
		private string description;

		public AbilityData(DataRow row) {
			this.id				= (byte)(long)row["ID"];
			this.name			= row["Name"] as string;
			this.description	= row["Description"] as string;
		}

		public byte ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public string Description {
			get { return description; }
		}
	}
}
