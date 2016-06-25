using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.PB {
	public class PokemonBoxBlockData {

		#region Constants

		public const int BlockSize = 0x2000;
		public const int ActualDataSize = 0x1FF0;

		#endregion

		#region Members

		public byte[] Raw { get; set; }

		#endregion

		public PokemonBoxBlockData(byte[] data) {
			this.Raw = data;
			GetFinalData();
		}

		public override string ToString() {
			return "BlockData: ID=" + BlockID + " Saves=" + SaveCount;
		}

		#region Block Information

		public ushort ChecksumA {
			get { return BigEndian.ToUInt16(Raw, 0x0); }
			set { BigEndian.WriteUInt16(value, Raw, 0x0); }
		}
		public ushort ChecksumB {
			get { return BigEndian.ToUInt16(Raw, 0x2); }
			set { BigEndian.WriteUInt16(value, Raw, 0x2); }
		}
		public int BlockID {
			get { return BigEndian.ToSInt32(Raw, 0x4); }
			set { BigEndian.WriteSInt32(value, Raw, 0x4); }
		}
		public int SaveCount {
			get { return BigEndian.ToSInt32(Raw, 0x8); }
			set { BigEndian.WriteSInt32(value, Raw, 0x8); }
		}
		public uint Footer {
			get { return BigEndian.ToUInt32(Raw, 0x1FFC); }
		}

		#endregion

		#region Loading/Saving

		public byte[] ActualData {
			get { return ByteHelper.SubByteArray(0xC, Raw, PokemonBoxBlockData.ActualDataSize); }
			set { ByteHelper.ReplaceBytes(Raw, 0xC, value); }
		}
		public ushort CalculateChecksum() {
			//Thanks from:
			//https://github.com/VitHuang/PokemonBoxSaveEditor
			uint checksum = (ushort)((ushort)BlockID + (ushort)(BlockID >> 16) + (ushort)SaveCount + (ushort)(SaveCount >> 16));
			for (int i = 0xC; i < 0x1FFC; i += 2) {
				checksum += BigEndian.ToUInt16(Raw, i);
			}
			return (ushort)checksum;
		}
		public byte[] GetFinalData() {
			ushort newChecksum = CalculateChecksum();
			if (newChecksum != ChecksumA || (ushort)(0xF004 - newChecksum) != ChecksumB)
				Console.WriteLine("Different Checksum");
			ChecksumA = newChecksum;
			ChecksumB = (ushort)((ushort)0xF004 - (ushort)newChecksum);
			return Raw;
		}

		#endregion
	}
}
