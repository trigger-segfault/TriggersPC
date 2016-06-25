using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class RivalInfoBlockData : BlockData {

		public RivalInfoBlockData(GBAGameSave gameSave, byte[] data, BlockDataCollection parent)
			: base(gameSave, data, parent) {

			if (parent.GameCode == GameCodes.FireRedLeafGreen) {
				parent.Mailbox.LoadPart2(ByteHelper.SubByteArray(0, raw, 4 * 36));
				((GBAPokePC)parent.PokePC).AddDaycare(ByteHelper.SubByteArray(256, raw, 3492), parent.GameCode);
				
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				((GBAPokePC)parent.PokePC).AddDaycare(ByteHelper.SubByteArray(432, raw, 280), parent.GameCode);
			}
			else if (parent.GameCode == GameCodes.RubySapphire) {
				/*for (int i = 0; i < SectionIDTable.GetContents(SectionID) - 4; i++) {
					if (LittleEndian.ToUInt32(raw, i) == 2664098125)
						Console.WriteLine(i);
					if (LittleEndian.ToUInt32(raw, i) == 183926766)
						Console.WriteLine(i);
					if (LittleEndian.ToUInt32(raw, i) == 22)
						Console.WriteLine(i);
				}*/
				((GBAPokePC)parent.PokePC).AddDaycare(ByteHelper.SubByteArray(284, raw, 280), parent.GameCode);
			}
		}

		public string RivalName {
			get {
				if (parent.GameCode == GameCodes.FireRedLeafGreen)
					return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(3020, raw, 7), gameSave.IsJapanese ? Languages.Japanese : Languages.English);
				else
					return "";
			}
			set {
				if (parent.GameCode == GameCodes.FireRedLeafGreen)
					ByteHelper.ReplaceBytes(raw, 3020, GBACharacterEncoding.GetBytes(value, 7, gameSave.IsJapanese ? Languages.Japanese : Languages.English));
			}
		}

		public bool IsPokemonSeenC(ushort dexID) {
			int index = (parent.GameCode == GameCodes.RubySapphire ? 3084 : (parent.GameCode == GameCodes.Emerald ? 3236 : 2968));
			return ByteHelper.GetBit(raw, index, dexID - 1);
		}
		public void SetPokemonSeenC(ushort dexID, bool seen) {
			int index = (parent.GameCode == GameCodes.RubySapphire ? 3084 : (parent.GameCode == GameCodes.Emerald ? 3236 : 2968));
			ByteHelper.SetBit(raw, index, dexID - 1, seen);
		}
		public bool[] PokedexSeenC {
			get {
				int index = (parent.GameCode == GameCodes.RubySapphire ? 3084 : (parent.GameCode == GameCodes.Emerald ? 3236 : 2968));
				BitArray bitArray = ByteHelper.GetBits(raw, index, 0, 386);
				bool[] flags = new bool[386];
				for (int i = 0; i < 386; i++)
					flags[i] = bitArray[i];
				return flags;
			}
			set {
				int index = (parent.GameCode == GameCodes.RubySapphire ? 3084 : (parent.GameCode == GameCodes.Emerald ? 3236 : 2968));
				ByteHelper.SetBits(raw, index, 0, new BitArray(value));
			}
		}

		public override byte[] GetFinalData() {
			if (parent.GameCode == GameCodes.FireRedLeafGreen) {
				ByteHelper.ReplaceBytes(raw, 0, parent.Mailbox.GetFinalDataPart2());
				ByteHelper.ReplaceBytes(raw, 256, ((GBADaycare)parent.PokePC.Daycare).GetFinalDataSevii());
				ByteHelper.ReplaceBytes(raw, 3608, ((GBADaycare)parent.PokePC.Daycare).GetFinalDataKanto());
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				ByteHelper.ReplaceBytes(raw, 432, ((GBADaycare)parent.PokePC.Daycare).GetFinalData());
			}
			else if (parent.GameCode == GameCodes.RubySapphire) {
				ByteHelper.ReplaceBytes(raw, 284, ((GBADaycare)parent.PokePC.Daycare).GetFinalData());
			}
			Checksum = CalculateChecksum();
			return raw;
		}
	}
}
