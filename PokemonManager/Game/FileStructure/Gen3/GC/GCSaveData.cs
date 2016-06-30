using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {


	public class GCSaveData {

		private static sbyte[] SubstructureOrder = new sbyte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, 15, 14 };
		private static ushort[] SubstructureOffsetIncrements = new ushort[16] {
			0xa0, 0x990, 0xbc60, 0x60, 0xe0, 0x1790,
			0x380, 0x2410, 0x6d0, 0x20, 0x410, 0x810,
			0x1010, 0x2010, 0x22b0, 0xc810
		};
		private static ushort[] SubstructureMaxSizes = new ushort[16] {
			0x88, 0x978, 0xbc50, 0x4a, 0xcc, 0x1774,
			0x364, 0x2400, 0x6b4, 0xb, 0x400, 0x800, 0x1000, 0x2000,
			0x2298, 0xc800
		};

		private byte[] raw;

		private GCGameSave gameSave;
		private GameConfigData gameConfigData;
		private PlayerData playerData;
		private PCData pcData;
		private ShadowPokemonData shadowData;
		private StrategyMemoData memoData;
		private DaycareData daycareData;

		private Inventory inventory;
		private IPokePC pokePC;

		private byte[] randomBytes;

		// XD
		private uint[] checksum;
		private ushort[] substructureSizes;
		private uint[] substructureOffsets;
		private ushort[] flagDataSubSizes;
		private bool otherCorruptionFlags;
		private byte[][] unhandledSubstructures;
		private ushort[] encryptionKeys;

		public GCSaveData(GCGameSave gameSave, byte[] data) {
			this.raw = data;
			this.gameSave = gameSave;
			this.inventory = new Inventory(gameSave);

			if (gameSave.GameType == GameTypes.Colosseum) {
				this.pokePC = new ColosseumPokePC(gameSave);

				DecryptColosseum(this.raw, ByteHelper.SubByteArray(0x1DFEC, raw, 20));

				LoadColosseum();
			}
			else {
				this.pokePC = new XDPokePC(gameSave);

				this.encryptionKeys = new ushort[4];
				BigEndian.LoadArray(encryptionKeys, data, 8);
				DecryptXD(this.raw, encryptionKeys);

				LoadXD();
			}
		}
		private void AdvanceKeys(ushort[] keys) {
			ushort a = (ushort)(keys[0] + 0x43);
			ushort b = (ushort)(keys[1] + 0x29);
			ushort c = (ushort)(keys[2] + 0x17);
			ushort d = (ushort)(keys[3] + 0x13);

			keys[0] = (ushort)((a & 0xf) | ((b << 4) & 0xf0) | ((c << 8) & 0xf00) | ((d << 12) & 0xf000));
			keys[1] = (ushort)(((a >> 4) & 0xf) | (b & 0xf0) | ((c << 4) & 0xf00) | ((d << 8) & 0xf000));
			keys[2] = (ushort)((c & 0xf00) | ((b & 0xf00) >> 4) | ((a & 0xf00) >> 8) | ((d << 4) & 0xf000));
			keys[3] = (ushort)(((a >> 12) & 0xf) | ((b >> 8) & 0xf0) | ((c >> 4) & 0xf00) | (d & 0xf000));
		}

		public void DecryptXD(byte[] data, ushort[] encryptionKeys) {
			uint end = 0x27FD8;
			ushort tmp = 0;
			ushort[] keys = new ushort[4];
			for (int i = 0; i < 4; i++)
				keys[i] = encryptionKeys[i];

			for (int i = 16; i < end; i += 8) {
				for (int j = 0; j < 4; j++) {
					tmp = BigEndian.ToUInt16(data, i + j * 2);
					tmp -= keys[j];
					BigEndian.WriteUInt16(tmp, data, i + j * 2);
				}
				AdvanceKeys(keys);
			}
		}

		public void EncryptXD(byte[] data, ushort[] encryptionKeys, uint end = 0x27FD8) {
			ushort tmp = 0;
			ushort[] keys = new ushort[4];
			for (int i = 0; i < 4; i++)
				keys[i] = encryptionKeys[i];
			for (int i = 16; i < end; i += 8) {
				for (int j = 0; j < 4; j++) {
					tmp = BigEndian.ToUInt16(data, i + j * 2);
					tmp += keys[j];
					BigEndian.WriteUInt16(tmp, data, i + j * 2);
				}
				AdvanceKeys(keys);
			}
		}

		public byte[] LoadSubstructure(byte[] data, uint index, uint size, uint maxSize) {
			if (index + 0xA8 >= 0x28000) {
				return new byte[maxSize];
			}

			if (size < maxSize) {
				byte[] outData = new byte[maxSize];
				ByteHelper.ReplaceBytes(outData, 0, ByteHelper.SubByteArray((int)index + 0xA8, data, (int)size));
				return outData;
			}
			else if (size == maxSize) {
				return ByteHelper.SubByteArray((int)index + 0xA8, data, (int)maxSize);
			}
			else {
				return new byte[maxSize];
			}
		}

		public int SnaggedPokemon {
			get {
				if (gameSave.GameType == GameTypes.Colosseum) {
					int count = 0;
					for (int i = 0; i < 88; i++) {
						if (ByteHelper.GetBit(raw, 114812 + i / 8, i % 8))
							count++;
					}
					return count++;
				}
				else {
					return shadowData.SnaggedPokemon;
				}
			}
		}
		public int PurifiedPokemon {
			get {
				if (gameSave.GameType == GameTypes.Colosseum) {
					int count = 0;
					for (int i = 0; i < 88; i++) {
						if (ByteHelper.GetBit(raw, 114416 + i / 8, i % 8))
							count++;
					}
					return count++;
				}
				else {
					return shadowData.PurifiedPokemon;
				}
			}
		}

		public void LoadXD() {
			this.checksum = new uint[4];
			this.substructureSizes = new ushort[16];
			this.substructureOffsets = new uint[16];
			this.flagDataSubSizes = new ushort[5];
			this.unhandledSubstructures = new byte[16][];
			this.randomBytes = new byte[40];


			ushort[] checksum_tmp = new ushort[8];
			for (int i = 0; i < 8; i++)
				checksum_tmp[8 - i - 1] = BigEndian.ToUInt16(raw, 0x10 + i * 2);
			for (int i = 0; i < 4; i++)
				checksum[i] = ((uint)checksum_tmp[i * 2] << 16) | (uint)checksum_tmp[i * 2 + 1];

			ushort[] substructureOffsetsTmp = new ushort[32];
			BigEndian.LoadArray(substructureSizes, raw, 0x20);
			BigEndian.LoadArray(substructureOffsetsTmp, raw, 0x40);
			BigEndian.LoadArray(flagDataSubSizes, raw, 0x80);
			flagDataSubSizes[4] = raw[0x8A];

			//int start = 8 + 0xA0;
			for (int i = 0; i < 16; i++)
				substructureOffsets[i] = (uint)(((uint)substructureOffsetsTmp[2 * i + 1] << 16) | (uint)substructureOffsetsTmp[2 * i]);

			otherCorruptionFlags = substructureSizes[0] != 0x88;
			for (int i = 0; i < 16; i++) {
				//if (i == 1 || i == 2)
				//	unhandledSubstructures[i] = null;
				//else
					unhandledSubstructures[i] = LoadSubstructure(raw, substructureOffsets[i], substructureSizes[i], SubstructureMaxSizes[i]);
			}

			int id = 0;
			this.gameConfigData = new GameConfigData(gameSave, LoadSubstructure(raw, substructureOffsets[id], substructureSizes[id], SubstructureMaxSizes[id]), this);
			id = 1;
			this.playerData = new PlayerData(gameSave, LoadSubstructure(raw, substructureOffsets[id], substructureSizes[id], SubstructureMaxSizes[id]), this);
			id = 2;
			this.pcData = new PCData(gameSave, LoadSubstructure(raw, substructureOffsets[id], substructureSizes[id], SubstructureMaxSizes[id]), this);
			id = 4;
			this.daycareData = new DaycareData(gameSave, LoadSubstructure(raw, substructureOffsets[id], substructureSizes[id], SubstructureMaxSizes[id]), this);
			id = 5;
			this.memoData = new StrategyMemoData(gameSave, LoadSubstructure(raw, substructureOffsets[id], substructureSizes[id], SubstructureMaxSizes[id]), this);
			id = 7;
			this.shadowData = new ShadowPokemonData(gameSave, LoadSubstructure(raw, substructureOffsets[id], substructureSizes[id], SubstructureMaxSizes[id]), this);
			{ // Flags
				ushort size = 0;
				for (int i = 0; i < 5; i++)
					size += flagDataSubSizes[i];
				if (size <= SubstructureMaxSizes[8])
					ByteHelper.ReplaceBytes(unhandledSubstructures[8], size, new byte[SubstructureMaxSizes[8] - size]);
			}
			for (int i = 0; i < 16; i++)
				substructureSizes[i] = SubstructureMaxSizes[i];

			ByteHelper.ReplaceBytes(randomBytes, 0, ByteHelper.SubByteArray(raw.Length - 40, raw, 40));
		}

		public void LoadColosseum() {
			this.gameConfigData = new GameConfigData(gameSave, ByteHelper.SubByteArray(8, raw, 112), this);
			this.playerData = new PlayerData(gameSave, ByteHelper.SubByteArray(120, raw, 2840), this);
			this.pcData = new PCData(gameSave, ByteHelper.SubByteArray(2960, raw, 29080), this);
			this.daycareData = new DaycareData(gameSave, ByteHelper.SubByteArray(33136, raw, 0x140), this);
			this.memoData = new StrategyMemoData(gameSave, ByteHelper.SubByteArray(33456, raw, 6004), this);
			//Mailbox 32040 + 1096 = 33136
			//BattleMode 58344
			//RibbonDescriptions 115804
			this.randomBytes = new byte[20];
			ByteHelper.ReplaceBytes(randomBytes, 0, ByteHelper.SubByteArray(0x1dfd8, raw, 20));
		}

		#region Game

		public SaveMagic SaveMagic {
			get {
				uint magic = BigEndian.ToUInt32(raw, 0);
				if ((SaveMagic)((uint)magic & 0xFFFF0000) == SaveMagic.Colosseum)
					return SaveMagic.Colosseum;
				else if ((SaveMagic)((uint)magic & 0xFFFFFF00) == SaveMagic.XD)
					return SaveMagic.XD;
				return SaveMagic.Unknown;
			}
		}
		public uint SaveCount {
			get { return BigEndian.ToUInt32(raw, 4); }
		}
		public GameConfigData GameConfigData {
			get { return gameConfigData; }
		}
		public PlayerData PlayerData {
			get { return playerData; }
		}
		public PCData PCData {
			get { return pcData; }
		}
		public ShadowPokemonData ShadowPokemonData {
			get { return shadowData; }
		}
		public StrategyMemoData StrategyMemoData {
			get { return memoData; }
		}
		public Inventory Inventory {
			get { return inventory; }
		}
		public IPokePC PokePC {
			get { return pokePC; }
		}
		public byte[] Raw {
			get { return raw; }
		}
		public uint this[int index, uint val] {
			get { return BigEndian.ToUInt32(raw, index); }
		}
		public ushort this[int index, ushort val] {
			get { return BigEndian.ToUInt16(raw, index); }
		}
		public byte this[int index, byte val] {
			get { return raw[index]; }
		}
		public TimeSpan PlayTime {
			get { return TimeSpan.FromSeconds((double)(BigEndian.ToUInt32(raw, 40) - 0x47000000) / 128); }
			set { BigEndian.WriteUInt32((uint)(value.TotalSeconds * 128) + 0x47000000, raw, 40); }
		}
		public ushort HoursPlayed {
			get { return (ushort)PlayTime.Hours; }
			set {
				TimeSpan playTime = PlayTime;
				PlayTime = playTime - TimeSpan.FromHours(playTime.Hours) + TimeSpan.FromHours(value);
			}
		}
		public byte MinutesPlayed {
			get { return (byte)PlayTime.Minutes; }
			set {
				TimeSpan playTime = PlayTime;
				PlayTime = playTime - TimeSpan.FromMinutes(playTime.Minutes) + TimeSpan.FromMinutes(value);
			}
		}
		public byte SecondsPlayed {
			get { return (byte)PlayTime.Seconds; }
			set {
				TimeSpan playTime = PlayTime;
				PlayTime = playTime - TimeSpan.FromSeconds(playTime.Seconds) + TimeSpan.FromSeconds(value);
			}
		}

		#endregion

		public void DecryptColosseum(byte[] data, byte[] digest) {
			byte[] key = new byte[20];
			byte[] k = new byte[20];
			byte[] d = new byte[20];

			ByteHelper.ReplaceBytes(key, 0, digest);
			ByteHelper.ReplaceBytes(k, 0, key);

			for (int i = 0; i < 20; i++)
				k[i] = (byte)~k[i];

			ByteHelper.ReplaceBytes(key, 0, k);

			SHA1 sha1 = SHA1Cng.Create();
			for (int i = 0x18; i < 0x1DFD8; i += 20) {
				key = sha1.ComputeHash(data, i, 20);
				ByteHelper.ReplaceBytes(d, 0, ByteHelper.SubByteArray(i, data, 20));

				for (int j = 0; j < 20; j++)
					d[j] ^= k[j];

				ByteHelper.ReplaceBytes(data, i, d);
				ByteHelper.ReplaceBytes(k, 0, key);
			}
		}

		public void EncryptColosseum(byte[] data, byte[] digest) {
			byte[] key = new byte[20];
			byte[] k = new byte[20];
			byte[] d = new byte[20];
			byte[] od = new byte[20];

			ByteHelper.ReplaceBytes(key, 0, digest);
			ByteHelper.ReplaceBytes(k, 0, key);

			for (int i = 0; i < 20; i++)
				k[i] = (byte)~k[i];

			ByteHelper.ReplaceBytes(key, 0, k);
			
			SHA1 sha1 = SHA1.Create();
			for (int i = 0x18; i < 0x1DFD8; i += 20) {
				ByteHelper.ReplaceBytes(d, 0, ByteHelper.SubByteArray(i, data, 20));

				for (int j = 0; j < 20; j++)
					od[j] = (byte)(d[j] ^ k[j]);

				ByteHelper.ReplaceBytes(data, i, od);
				key = sha1.ComputeHash(data, i, 20);
				ByteHelper.ReplaceBytes(k, 0, key);
			}
		}

		private void SaveColosseum(byte[] data) {
			ByteHelper.ReplaceBytes(data, 120, playerData.GetFinalData());
			ByteHelper.ReplaceBytes(data, 2960, pcData.GetFinalData());
			ByteHelper.ReplaceBytes(data, 33136, daycareData.GetFinalData());
			ByteHelper.ReplaceBytes(data, 33456, memoData.GetFinalData());

			byte[] previousChecksum = ByteHelper.SubByteArray(0x1dfec, data, 20);
			int previousHeaderChecksum = BigEndian.ToSInt32(data, 12);

			BigEndian.WriteUInt32(0, data, 12);
			SHA1 sha1 = SHA1.Create();
			byte[] checksum = new byte[20];
			checksum = sha1.ComputeHash(ByteHelper.SubByteArray(0, data, 0x1dfd8));

			byte[] D = new byte[8];
			byte[] H = new byte[8];
			byte[] tmpBuf = new byte[8];

			ByteHelper.ReplaceBytes(H, 0, ByteHelper.SubByteArray(0, checksum, 8));
			ByteHelper.ReplaceBytes(D, 0, ByteHelper.SubByteArray(0x18, data, 8));

			for (int i = 0; i < 8; i++) {
				D[i] ^= (byte)~H[i];
			}

			ByteHelper.ReplaceBytes(tmpBuf, 0, D);

			int newHC = 0;
			for (int i = 0; i < 0x18; i += 4)
				newHC -= BigEndian.ToSInt32(data, i);

			newHC -= BigEndian.ToSInt32(tmpBuf, 0);
			newHC -= BigEndian.ToSInt32(tmpBuf, 4);

			BigEndian.WriteSInt32(newHC, data, 12);

			ByteHelper.ReplaceBytes(data, 0x1dfd8, randomBytes);
			ByteHelper.ReplaceBytes(data, 0x1dfec, checksum);
		}

		private void SaveXD(byte[] data) {
			int start = 8 + 0xA0;

			BigEndian.WriteUInt32((uint)SaveMagic.XD, data, 0);
			BigEndian.WriteUInt32(SaveCount, data, 4);
			BigEndian.WriteArray(encryptionKeys, data, 8);
			ByteHelper.ReplaceBytes(data, data.Length - 40, randomBytes);

			{ // Flags
				ushort size = 0;
				for (int i = 0; i < 5; i++) size += flagDataSubSizes[i];
				if (size <= SubstructureMaxSizes[8])
					ByteHelper.ReplaceBytes(unhandledSubstructures[8], size, new byte[SubstructureMaxSizes[8] - size]);
			}

			for (int i = 0; i < 16; i++) {
				if (unhandledSubstructures[i] != null) {
					if (SubstructureOrder[i] == -1)
						ByteHelper.ReplaceBytes(unhandledSubstructures[i], 0, new byte[SubstructureMaxSizes[i]]);
					ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[i], unhandledSubstructures[i]);
					ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[i] + (int)substructureSizes[i], new byte[SubstructureMaxSizes[i] - substructureSizes[i]]);
				}
			}

			int id = 1;
			ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[id], playerData.GetFinalData());
			id = 2;
			ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[id], pcData.GetFinalData());
			id = 4;
			ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[id], daycareData.GetFinalData());
			id = 5;
			ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[id], memoData.GetFinalData());
			id = 7;
			ByteHelper.ReplaceBytes(data, (int)start + (int)substructureOffsets[id], shadowData.GetFinalData());

			ushort[] substructureOffsetsTmp = new ushort[32];
			for (int i = 0; i < 16; i++)
				substructureSizes[i] = SubstructureMaxSizes[i];
			BigEndian.WriteArray(substructureSizes, data, 0x20);
			otherCorruptionFlags = false;

			for (int i = 0; i < 16; i++) {
				substructureOffsetsTmp[i * 2] = (ushort)substructureOffsets[i];
				substructureOffsetsTmp[i * 2 + 1] = (ushort)(substructureOffsets[i] >> 16);
			}

			BigEndian.WriteArray(substructureOffsetsTmp, data, 0x40);
			for (int i = 0; i < 4; i++)
				BigEndian.WriteUInt16(flagDataSubSizes[i], data, 0x80 + i * 2);
			BigEndian.WriteUInt16(flagDataSubSizes[4], data, 0x8A);

			// DO checksums
			int oldHC = BigEndian.ToSInt32(data, (int)start + (int)substructureOffsets[0] + 0x38);
			int newHC = 0;
			for (int i = 0; i < 8; i++)
				newHC += (int)data[i];
			if (oldHC != newHC)
				Console.WriteLine();
			BigEndian.WriteSInt32(newHC, data, (int)start + (int)substructureOffsets[0] + 0x38);

			uint[] newChecksum = new uint[4];
			byte[] tmpbuf = new byte[16];
			ByteHelper.ReplaceBytes(tmpbuf, 0, ByteHelper.SubByteArray(0x10, data, 16));
			ByteHelper.ReplaceBytes(data, 0x10, new byte[16]);

			int start2 = 0x08;
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 0x9ff4; j += 2) {
					newChecksum[i] += BigEndian.ToUInt16(data, start2);
					start2 += 2;
				}
			}

			for (int i = 0; i < 4; i++) {
				if (checksum[i] != newChecksum[i])
					Console.WriteLine();
				checksum[i] = newChecksum[i];
			}

			ByteHelper.ReplaceBytes(data, 0x10, tmpbuf);

			ushort[] checksum_tmp = new ushort[8];

			for (int i = 0; i < 4; i++) {
				checksum_tmp[i * 2] = (ushort)(checksum[i] >> 16);
				checksum_tmp[i * 2 + 1] = (ushort)checksum[i];
			}
			for (int i = 0; i < 4; i++) {
				ushort swap = checksum_tmp[i];
				checksum_tmp[i] = checksum_tmp[8 - i - 1];
				checksum_tmp[8 - i - 1] = swap;
			}
			BigEndian.WriteArray(checksum_tmp, data, 0x10);
		}

		public byte[] GetFinalData() {
			byte[] data = new byte[raw.Length];
			ByteHelper.ReplaceBytes(data, 0, raw);
			if (gameSave.GameType == GameTypes.Colosseum) {
				SaveColosseum(data);
				EncryptColosseum(data, ByteHelper.SubByteArray(0x1DFEC, data, 20));
			}
			else {
				SaveXD(data);
				EncryptXD(data, encryptionKeys);
			}

			return data;
		}
	}
}
