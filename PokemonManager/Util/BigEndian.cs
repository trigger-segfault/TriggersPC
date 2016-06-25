using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Util {
	public static class BigEndian {

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

		#region Get Bytes

		public static byte[] GetBytes(bool val) {
			return BitConverter.GetBytes(val);
		}
		public static byte[] GetBytes(byte val) {
			return BitConverter.GetBytes(val);
		}
		public static byte[] GetBytes(short val) {
			return ReverseBytes(BitConverter.GetBytes(val));
		}
		public static byte[] GetBytes(int val) {
			return ReverseBytes(BitConverter.GetBytes(val));
		}
		public static byte[] GetBytes(long val) {
			return ReverseBytes(BitConverter.GetBytes(val));
		}
		public static byte[] GetBytes(ushort val) {
			return ReverseBytes(BitConverter.GetBytes(val));
		}
		public static byte[] GetBytes(uint val) {
			return ReverseBytes(BitConverter.GetBytes(val));
		}
		public static byte[] GetBytes(ulong val) {
			return ReverseBytes(BitConverter.GetBytes(val));
		}

		#endregion

		#region Load Array

		public static void LoadArray(short[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				array[i] = ToSInt16(data, index + i * 2);
		}
		public static void LoadArray(int[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				array[i] = ToSInt32(data, index + i * 4);
		}
		public static void LoadArray(long[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				array[i] = ToSInt64(data, index + i * 8);
		}
		public static void LoadArray(ushort[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				array[i] = ToUInt16(data, index + i * 2);
		}
		public static void LoadArray(uint[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				array[i] = ToUInt32(data, index + i * 4);
		}
		public static void LoadArray(ulong[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				array[i] = ToUInt64(data, index + i * 8);
		}

		#endregion

		#region Write Array

		public static void WriteArray(short[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				WriteSInt16(array[i], data, index + i * 2);
		}
		public static void WriteArray(int[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				WriteSInt32(array[i], data, index + i * 4);
		}
		public static void WriteArray(long[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				WriteSInt64(array[i], data, index + i * 8);
		}
		public static void WriteArray(ushort[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				WriteUInt16(array[i], data, index + i * 2);
		}
		public static void WriteArray(uint[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				WriteUInt32(array[i], data, index + i * 4);
		}
		public static void WriteArray(ulong[] array, byte[] data, int index) {
			for (int i = 0; i < array.Length; i++)
				WriteUInt64(array[i], data, index + i * 8);
		}

		#endregion

		public static float ToFloat(byte[] data, int index) {
			return BitConverter.ToSingle(SubByteArray(index, data, 4), 0);
		}

		public static bool GetBit(byte[] data, int index, int bitIndex) {
			return ((data[index + bitIndex / 8] >> (7 - (bitIndex % 8))) & 0x1) == 1;
		}

		public static byte[] SetBit(byte[] data, int index, int bitIndex, bool bit) {
			if (bit) data[index + bitIndex / 8] |= (byte)(1 << (7 - (bitIndex % 8)));
			else data[index + bitIndex / 8] &= (byte)~(1 << (7 - (bitIndex % 8)));
			return data;
		}

		public static byte[] ReverseBytes(byte[] data) {
			return SubByteArray(0, data, data.Length);
		}

		public static byte[] SubByteArray(int start, byte[] data, int size) {
			byte[] outData = new byte[size];
			for (int i = 0; i < size; i++)
				outData[i] = data[start + size - i - 1];
			return outData;
		}

		private static void ReplaceBytes(byte[] data, int index, byte[] dataToReplace) {
			for (int i = 0; i < dataToReplace.Length; i++)
				data[index + i] = dataToReplace[dataToReplace.Length - i - 1];
		}
	}
}
