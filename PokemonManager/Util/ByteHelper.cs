using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Util {
	public class ByteHelper {

		#region Basic Bytes

		public static byte[] SubByteArray(int start, byte[] data, int size) {
			byte[] outData = new byte[size];
			for (int i = 0; i < size; i++)
				outData[i] = data[start + i];
			return outData;
		}
		public static void ReplaceBytes(byte[] data, int index, byte[] dataToReplace) {
			for (int i = 0; i < dataToReplace.Length; i++)
				data[index + i] = dataToReplace[i];
		}

		#endregion

		public static string ReadTerminatingString(byte[] data, int index, char terminator = (char)0) {
			StringBuilder builder = new StringBuilder();
			for (int i = index; i < data.Length; i++) {
				char c = (char)BitConverter.ToChar(data, i);
				if (c == terminator)
					break;
				builder.Append(c);
			}
			return builder.ToString();
		}
		public static byte[] GetTerminatingStringBytes(string text, char terminator = (char)0) {
			byte[] data = new byte[(text.Length + 1) * 2];
			for (int i = 0; i < text.Length; i++) {
				LittleEndian.WriteUInt16((ushort)text[i], data, i * 2);
			}
			LittleEndian.WriteUInt16(0, data, text.Length * 2);
			return data;
		}

		public static string ReadString(byte[] data, int index, int length) {
			StringBuilder text = new StringBuilder();
			for (int i = 0; i < length; i++) {
				char c = (char)LittleEndian.ToUInt16(data, index + i * 2);
				if (c == 0)
					break;
				text.Append(c);
			}
			return text.ToString();
		}
		public static void WriteString(byte[] data, int index, string text, int length) {
			for (int i = 0; i < length; i++) {
				if (i < text.Length)
					LittleEndian.WriteUInt16((ushort)text[i], data, index + i * 2);
				else
					LittleEndian.WriteUInt16((ushort)0, data, index + i * 2);
			}
		}
		public static byte[] GetStringBytes(string text, int length) {
			byte[] data = new byte[length * 2];
			for (int i = 0; i < length; i++) {
				if (i < text.Length)
					LittleEndian.WriteUInt16((ushort)text[i], data, i * 2);
				else
					LittleEndian.WriteUInt16((ushort)0, data, i * 2);
			}
			return data;
		}

		public static bool CompareBytes(byte val, byte[] data) {
			for (int i = 0; i < data.Length; i++) {
				if (data[i] != val)
					return false;
			}
			return true;
		}
		public static bool CompareBytes(byte val, byte[] data, int index, int length) {
			for (int i = 0; i < length; i++) {
				if (data[index + i] != val)
					return false;
			}
			return true;
		}

		#region Basic Bits

		public static BitArray SubBitArray(int start, BitArray bits, int size) {
			BitArray bitArray = new BitArray(size);
			for (int i = 0; i < size; i++) {
				bitArray[i] = bits[start + i];
			}
			return bitArray;
		}
		public static void ReplaceBits(byte[] bits, int index, byte[] bitsToReplace) {
			for (int i = 0; i < bitsToReplace.Length; i++)
				bits[index + i] = bitsToReplace[i];
		}
		public static void ReverseBits(BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				bool bit = bits[i];
				bits[i] = bits[bits.Length - i - 1];
				bits[bits.Length - i - 1] = bit;
			}
		}
		public static void FlipBits(BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				bits[i] = !bits[i];
			}
		}
		public static int GetBitDeviation(BitArray bitArray1, BitArray bitArray2) {
			int deviation = 0;
			for (int i = 0; i < Math.Max(bitArray1.Length, bitArray2.Length); i++) {
				if (i < bitArray1.Length) {
					if (i < bitArray2.Length) {
						if (bitArray1[i] != bitArray2[i])
							deviation++;
					}
					else if (bitArray1[i]) {
						deviation++;
					}
				}
				else if (i < bitArray2.Length && bitArray2[i]) {
					deviation++;
				}
			}
			return deviation;
		}

		#endregion

		#region Get Bits

		public static bool GetBit(byte[] data, int index, int bitIndex) {
			return ((data[index + bitIndex / 8] >> (bitIndex % 8)) & 0x1) == 1;
		}
		public static bool GetBit(byte data, int bitIndex) {
			return ((data >> bitIndex) & 0x1) == 1;
		}
		public static bool GetBit(ushort data, int bitIndex) {
			return ((data >> bitIndex) & 0x1) == 1;
		}
		public static bool GetBit(uint data, int bitIndex) {
			return ((data >> bitIndex) & 0x1) == 1;
		}
		public static bool GetBit(ulong data, int bitIndex) {
			return ((data >> bitIndex) & 0x1) == 1;
		}

		public static BitArray GetBits(byte[] data, int index, int bitIndex, int length) {
			BitArray bitArray = new BitArray(length);
			for (int i = 0; i < length; i++)
				bitArray[i] = ((data[index + (bitIndex + i) / 8] >> ((bitIndex + i) % 8)) & 0x1) == 1;
			return bitArray;
		}
		public static BitArray GetBits(byte data, int bitIndex = 0, int length = 8) {
			BitArray bitArray = new BitArray(length);
			for (int i = 0; i < length; i++)
				bitArray[i] = ((data >> (bitIndex + i)) & 0x1) == 1;
			return bitArray;
		}
		public static BitArray GetBits(ushort data, int bitIndex = 0, int length = 16) {
			BitArray bitArray = new BitArray(length);
			for (int i = 0; i < length; i++)
				bitArray[i] = ((data >> (bitIndex + i)) & 0x1) == 1;
			return bitArray;
		}
		public static BitArray GetBits(uint data, int bitIndex = 0, int length = 32) {
			BitArray bitArray = new BitArray(length);
			for (int i = 0; i < length; i++)
				bitArray[i] = ((data >> (bitIndex + i)) & 0x1) == 1;
			return bitArray;
		}
		public static BitArray GetBits(ulong data, int bitIndex = 0, int length = 64) {
			BitArray bitArray = new BitArray(length);
			for (int i = 0; i < length; i++)
				bitArray[i] = ((data >> (bitIndex + i)) & 0x1) == 1;
			return bitArray;
		}

		#endregion

		#region Set Bits

		public static byte[] SetBit(byte[] data, int index, int bitIndex, bool bit) {
			if (bit) data[index + bitIndex / 8] |= (byte)(1 << (bitIndex % 8));
			else     data[index + bitIndex / 8] &= (byte)~(1 << (bitIndex % 8));
			return data;
		}
		public static byte SetBit(byte data, int bitIndex, bool bit) {
			if (bit) data |= (byte)(1 << bitIndex);
			else     data &= (byte)~(1 << bitIndex);
			return data;
		}
		public static ushort SetBit(ushort data, int bitIndex, bool bit) {
			if (bit) data |= (ushort)(1 << bitIndex);
			else     data &= (ushort)~(1 << bitIndex);
			return data;
		}
		public static uint SetBit(uint data, int bitIndex, bool bit) {
			if (bit) data |= (uint)(1U << bitIndex);
			else     data &= (uint)~(1U << bitIndex);
			return data;
		}
		public static ulong SetBit(ulong data, int bitIndex, bool bit) {
			if (bit) data |= (ulong)(1UL << bitIndex);
			else     data &= (ulong)~(1UL << bitIndex);
			return data;
		}

		public static byte[] SetBits(byte[] data, int index, int bitIndex, int length, byte val, int valBitIndex = 0) {
			return SetBits(data, index, bitIndex, GetBits(val, valBitIndex, length));
		}
		public static byte[] SetBits(byte[] data, int index, int bitIndex, int length, ushort val, int valBitIndex = 0) {
			return SetBits(data, index, bitIndex, GetBits(val, valBitIndex, length));
		}
		public static byte[] SetBits(byte[] data, int index, int bitIndex, int length, uint val, int valBitIndex = 0) {
			return SetBits(data, index, bitIndex, GetBits(val, valBitIndex, length));
		}
		public static byte[] SetBits(byte[] data, int index, int bitIndex, int length, ulong val, int valBitIndex = 0) {
			return SetBits(data, index, bitIndex, GetBits(val, valBitIndex, length));
		}
		public static byte[] SetBits(byte[] data, int index, int bitIndex, BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) data[index + (bitIndex + i) / 8] |= (byte)(1 << ((bitIndex + i) % 8));
				else data[index + (bitIndex + i) / 8] &= (byte)~(1 << ((bitIndex + i) % 8));
			}
			return data;
		}
		public static byte SetBits(byte data, int bitIndex, BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) data |= (byte)(1 << (bitIndex + i));
				else         data &= (byte)~(1 << (bitIndex + i));
			}
			return data;
		}
		public static ushort SetBits(ushort data, int bitIndex, BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) data |= (ushort)(1 << (bitIndex + i));
				else         data &= (ushort)~(1 << (bitIndex + i));
			}
			return data;
		}
		public static uint SetBits(uint data, int bitIndex, BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) data |= (uint)(1U << (bitIndex + i));
				else         data &= (uint)~(1U << (bitIndex + i));
			}
			return data;
		}
		public static ulong SetBits(ulong data, int bitIndex, BitArray bits) {
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) data |= (ulong)(1UL << (bitIndex + i));
				else         data &= (ulong)~(1UL << (bitIndex + i));
			}
			return data;
		}

		#endregion

		#region Convert Bits

		public static byte[] BitsToByteArray(BitArray bits, int bitIndex = 0) {
			byte[] data = new byte[(bits.Length + bitIndex) / 8];
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) data[(bitIndex + i) / 8] |= (byte)(1 << ((bitIndex + i) % 8));
				else         data[(bitIndex + i) / 8] &= (byte)~(1 << ((bitIndex + i) % 8));
			}
			return data;
		}

		public static byte BitsToByte(byte[] data, int index, int bitIndex, int length, int outBitIndex = 0) {
			return BitsToByte(GetBits(data, index, bitIndex, length), outBitIndex);
		}
		public static ushort BitsToUInt16(byte[] data, int index, int bitIndex, int length, int outBitIndex = 0) {
			return BitsToUInt16(GetBits(data, index, bitIndex, length), outBitIndex);
		}
		public static uint BitsToUInt32(byte[] data, int index, int bitIndex, int length, int outBitIndex = 0) {
			return BitsToUInt32(GetBits(data, index, bitIndex, length), outBitIndex);
		}
		public static ulong BitsToUInt64(byte[] data, int index, int bitIndex, int length, int outBitIndex = 0) {
			return BitsToUInt64(GetBits(data, index, bitIndex, length), outBitIndex);
		}
		public static byte BitsToByte(BitArray bits, int bitIndex = 0) {
			byte val = 0;
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) val |= (byte)(1 << (bitIndex + i));
				else         val &= (byte)~(1 << (bitIndex + i));
			}
			return val;
		}
		public static ushort BitsToUInt16(BitArray bits, int bitIndex = 0) {
			ushort val = 0;
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) val |= (ushort)(1 << (bitIndex + i));
				else         val &= (ushort)~(1 << (bitIndex + i));
			}
			return val;
		}
		public static uint BitsToUInt32(BitArray bits, int bitIndex = 0) {
			uint val = 0;
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) val |= (uint)(1U << (bitIndex + i));
				else         val &= (uint)~(1U << (bitIndex + i));
			}
			return val;
		}
		public static ulong BitsToUInt64(BitArray bits, int bitIndex = 0) {
			ulong val = 0;
			for (int i = 0; i < bits.Length; i++) {
				if (bits[i]) val |= (ulong)(1UL << (bitIndex + i));
				else         val &= (ulong)~(1UL << (bitIndex + i));
			}
			return val;
		}


		#endregion
	}
}
