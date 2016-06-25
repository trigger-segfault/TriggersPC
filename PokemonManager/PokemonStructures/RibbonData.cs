using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class RibbonData {

		private string id;
		private string name;
		private string description;

		public RibbonData(DataRow row) {
			this.id				= row["ID"] as string;
			this.name			= row["Name"] as string;
			this.description	= row["Description"] as string;
		}

		public string ID {
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
