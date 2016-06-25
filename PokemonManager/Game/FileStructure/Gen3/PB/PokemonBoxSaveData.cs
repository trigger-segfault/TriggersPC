using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.PB {
	public class PokemonBoxSaveData {

		#region Members
		
		private PokemonBoxGameSave gameSave;
		private byte[] raw;
		private List<PokemonBoxBlockData> sortedBlocks;
		private List<PokemonBoxBlockData> blocks;
		private byte[] actualData;
		private BoxPokePC pokePC;

		#endregion

		public PokemonBoxSaveData(PokemonBoxGameSave gameSave, byte[] data) {
			this.raw = data;
			this.gameSave = gameSave;
			this.pokePC = new BoxPokePC(gameSave);
			this.blocks = new List<PokemonBoxBlockData>();
			this.sortedBlocks = new List<PokemonBoxBlockData>();

			LoadActualData();

			this.pokePC.Load(ByteHelper.SubByteArray(4, this.actualData, 4 + 1500 * 84 + 250));
		}

		#region Basic

		public byte[] Raw {
			get { return raw; }
		}
		public byte[] ActualData {
			get { return actualData; }
		}
		public int SaveCount {
			get { return blocks[0].SaveCount; }
		}
		public IPokePC PokePC {
			get { return pokePC; }
		}

		#endregion

		#region Loading/Saving

		public void LoadActualData() {
			// Load then sort the blocks
			for (int i = 0; i < 23; i++)
				blocks.Add(new PokemonBoxBlockData(ByteHelper.SubByteArray(i * PokemonBoxBlockData.BlockSize, raw, PokemonBoxBlockData.BlockSize)));
			sortedBlocks.AddRange(blocks);
			sortedBlocks.Sort((b1, b2) => ((int)b1.BlockID - (int)b2.BlockID));

			// Create the actual data from the blocks
			List<byte> newData = new List<byte>();
			foreach (PokemonBoxBlockData block in sortedBlocks) {
				newData.AddRange(block.ActualData);
			}
			actualData = newData.ToArray();
		}
		public void SaveActualData() {
			for (int i = 0; i < 23; i++) {
				sortedBlocks[i].ActualData = ByteHelper.SubByteArray(i * PokemonBoxBlockData.ActualDataSize, actualData, PokemonBoxBlockData.ActualDataSize);
			}

			for (int i = 0; i < 23; i++) {
				ByteHelper.ReplaceBytes(raw, i * PokemonBoxBlockData.BlockSize, blocks[i].GetFinalData());
			}
		}

		public byte[] GetFinalData() {
			// Save the Poke PC
			ByteHelper.ReplaceBytes(actualData, 4, pokePC.GetFinalData());

			SaveActualData();

			return raw;
		}

		#endregion
	}
}
