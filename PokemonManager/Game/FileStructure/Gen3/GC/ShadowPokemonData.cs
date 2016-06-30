using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class XDShadowPokemonInfo {
		private byte[] raw;

		public XDShadowPokemonInfo() {
			this.raw = new byte[72]; ;
		}
		public XDShadowPokemonInfo(byte[] data) {
			this.raw = data;
		}

		public bool IsPurified {
			get { return ByteHelper.GetBit(raw, 0, 7); }
		}
		public bool IsSnagged {
			get { return ByteHelper.GetBit(raw, 0, 6) || ByteHelper.GetBit(raw, 0, 7); }
		}

		public byte[] Raw {
			get { return raw; }
		}

		public ushort DexID {
			get { return BigEndian.ToUInt16(raw, 26); }
			set { BigEndian.WriteUInt16(value, raw, 26); }
		}
		public uint Personality {
			get { return BigEndian.ToUInt32(raw, 28); }
			set { BigEndian.WriteUInt32(value, raw, 28); }
		}

		public int Purification {
			get { return BigEndian.ToSInt32(raw, 36); }
			set { BigEndian.WriteSInt32(value, raw, 36); }
		}

		public uint ExperienceStored {
			get { return BigEndian.ToUInt32(raw, 4) >> 12; }
			set { BigEndian.WriteUInt32((BigEndian.ToUInt32(raw, 4) & 0xFFF) | (value << 12), raw, 32); }
		}
	}

	public class ColosseumShadowPokemonInfo {
		private byte[] raw;

		public ColosseumShadowPokemonInfo() {
			this.raw = new byte[12];
		}
		public ColosseumShadowPokemonInfo(byte[] data) {
			this.raw = data;
		}
		public byte[] Raw {
			get { return raw; }
		}
		public uint Personality {
			get { return BigEndian.ToUInt32(raw, 0); }
			set { BigEndian.WriteUInt32(value, raw, 0); }
		}
		public ushort MetLocationID {
			get { return BigEndian.ToUInt16(raw, 6); }
			set { BigEndian.WriteUInt16(value, raw, 6); }
		}
		public uint Unknown0x8 {
			get { return BigEndian.ToUInt32(raw, 8); }
			set { BigEndian.WriteUInt32(value, raw, 8); }
		}
	}

	public class ShadowPokemonData : GCData {

		private Dictionary<uint, XDShadowPokemonInfo> shadowInfoMap;
		private XDShadowPokemonInfo[] shadowInfo;

		public ShadowPokemonData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {
			this.raw = data;
			this.shadowInfoMap = new Dictionary<uint, XDShadowPokemonInfo>();
			this.shadowInfo = new XDShadowPokemonInfo[data.Length / 72];
			for (int i = 0; i < shadowInfo.Length; i++) {
				shadowInfo[i] = new XDShadowPokemonInfo(ByteHelper.SubByteArray(i * 72, data, 72));
				if (!this.shadowInfoMap.ContainsKey(shadowInfo[i].Personality))
					this.shadowInfoMap.Add(shadowInfo[i].Personality, shadowInfo[i]);
			}
		}

		public XDShadowPokemonInfo this[uint personality] {
			get {
				if (shadowInfoMap.ContainsKey(personality))
					return shadowInfoMap[personality];
				return new XDShadowPokemonInfo();
			}
		}

		public bool Contains(uint personality) {
			return shadowInfoMap.ContainsKey(personality);
		}

		public XDShadowPokemonInfo GetAt(int index) {
			return shadowInfo[index];
		}

		public override byte[] GetFinalData() {
			for (int i = 0; i < shadowInfo.Length; i++) {
				ByteHelper.ReplaceBytes(raw, i * 72, shadowInfo[i].Raw);
			}

			return raw;
		}

		public int SnaggedPokemon {
			get {
				int count = 0;
				foreach (KeyValuePair<uint, XDShadowPokemonInfo> pair in shadowInfoMap) {
					if (pair.Value.IsSnagged)
						count++;
				}
				return count;
			}
		}
		public int PurifiedPokemon {
			get {
				int count = 0;
				foreach (KeyValuePair<uint, XDShadowPokemonInfo> pair in shadowInfoMap) {
					if (pair.Value.IsPurified)
						count++;
				}
				return count;
			}
		}

	}
}
