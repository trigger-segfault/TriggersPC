using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class GCData {

		protected GCSaveData parent;
		protected byte[] raw;
		protected GCGameSave gameSave;

		public GCData(GCGameSave gameSave, byte[] data, GCSaveData parent) {
			this.raw = data;
			this.parent = parent;
			this.gameSave = gameSave;
		}

		public byte[] Raw {
			get { return raw; }
		}

		public virtual byte[] GetFinalData() {
			return raw;
		}
	}
}
