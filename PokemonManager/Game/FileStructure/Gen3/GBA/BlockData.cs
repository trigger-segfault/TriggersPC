using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class BlockData : IBlockData {

		protected byte[] raw;
		protected BlockDataCollection parent;
		protected IGameSave gameSave;

		public BlockData(IGameSave gameSave, byte[] data, BlockDataCollection parent) {
			if (data.Length != 4096)
				throw new Exception("Block size not equal to 4096 bytes");
			this.raw = data;
			this.parent = parent;
			this.gameSave = gameSave;
		}

		public byte[] Raw {
			get { return raw; }
		}
		public uint this[int index, uint val] {
			get { return LittleEndian.ToUInt32(raw, index); }
			set { LittleEndian.WriteUInt32(value, raw, index); }
		}
		public ushort this[int index, ushort val] {
			get { return LittleEndian.ToUInt16(raw, index); }
			set { LittleEndian.WriteUInt16(value, raw, index); }
		}
		public byte this[int index, byte val] {
			get { return raw[index]; }
			set { raw[index] = value; }
		}

		public BlockDataCollection Parent {
			get { return parent; }
			set { parent = value; }
		}

		public uint SaveIndex {
			get { return LittleEndian.ToUInt32(raw, 4092); }
			set { LittleEndian.WriteUInt32(value, raw, 4092); }
		}

		public SectionTypes SectionID {
			get { return (SectionTypes)LittleEndian.ToUInt16(raw, 4084); }
			set { LittleEndian.WriteUInt16((ushort)value, raw, 4084); }
		}

		public ushort Checksum {
			get { return LittleEndian.ToUInt16(raw, 4086); }
			set { LittleEndian.WriteUInt16(value, raw, 4086); }
		}

		public virtual ushort CalculateChecksum() {
			uint checksum = 0;
			int contents = SectionIDTable.GetContents(SectionID);
			int startIndex = 0;
			while (startIndex < contents) {
				checksum += BitConverter.ToUInt32(raw, startIndex);
				startIndex += 4;
			}
			byte[] bytes = BitConverter.GetBytes(checksum);
			return (ushort)((uint)BitConverter.ToUInt16(bytes, 0) + (uint)BitConverter.ToUInt16(bytes, 2));
		}

		public virtual byte[] GetFinalData() {
			Checksum = CalculateChecksum();
			return raw;
		}
	}
}
