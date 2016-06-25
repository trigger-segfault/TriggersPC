using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Util {
	public static class LittleEndian {

		#region Converting

		public static bool ToBool(byte[] data, int index) {
			return BitConverter.ToBoolean(data, index);
		}
		public static short ToSInt16(byte[] data, int index) {
			return BitConverter.ToInt16(SubByteArray(index, data, 2), 0);
		}
		public static int ToSInt32(byte[] data, int index) {
			return BitConverter.ToInt32(SubByteArray(index, data, 4), 0);
		}
		public static long ToSInt64(byte[] data, int index) {
			return BitConverter.ToInt64(SubByteArray(index, data, 8), 0);
		}
		public static ushort ToUInt16(byte[] data, int index) {
			return BitConverter.ToUInt16(SubByteArray(index, data, 2), 0);
		}
		public static uint ToUInt32(byte[] data, int index) {
			return BitConverter.ToUInt32(SubByteArray(index, data, 4), 0);
		}
		public static ulong ToUInt64(byte[] data, int index) {
			return BitConverter.ToUInt64(SubByteArray(index, data, 8), 0);
		}

		#endregion

		#region Writing

		public static void WriteBool(bool val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}
		public static void WriteSInt16(short val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}
		public static void WriteSInt32(int val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}
		public static void WriteSInt64(long val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}
		public static void WriteUInt16(ushort val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}
		public static void WriteUInt32(uint val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}
		public static void WriteUInt64(ulong val, byte[] data, int index) {
			ReplaceBytes(data, index, BitConverter.GetBytes(val));
		}

		#endregion

		private static byte[] SubByteArray(int start, byte[] data, int size) {
			byte[] outData = new byte[size];
			for (int i = 0; i < size; i++)
				outData[i] = data[start + i];
			return outData;
		}

		private static void ReplaceBytes(byte[] data, int index, byte[] dataToReplace) {
			for (int i = 0; i < dataToReplace.Length; i++)
				data[index + i] = dataToReplace[i];
		}
	}
}
