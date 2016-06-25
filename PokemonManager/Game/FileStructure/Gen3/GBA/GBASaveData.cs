using PokemonManager.Game.FileStructure.Gen3.GBA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class GBASaveData {

		private byte[] raw;
		private BlockDataCollection blockDataCollection;

		public GBASaveData(GBAGameSave gameSave, byte[] data) {
			if (data.Length != 57344)
				throw new Exception("Gen 3 GBA Save data must contain 57344 bytes");
			this.raw = data;
			this.blockDataCollection = new BlockDataCollection(gameSave, data);
		}

		#region Containers

		public BlockDataCollection BlockDataCollection {
			get { return blockDataCollection; }
			set { blockDataCollection = value; }
		}
		public byte[] Raw {
			get { return raw; }
		}

		#endregion

		#region Saving/Loading

		public void IncrementSaveIndex() {
			foreach (IBlockData blockData in blockDataCollection) {
				blockData.SaveIndex++;
			}
		}
		public byte[] GetFinalData() {
			return blockDataCollection.GetFinalData();
		}

		#endregion
	}
}
