using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure {
	public class TrainerInfoBlockData : BlockData {

		public TrainerInfoBlockData(IGameSave gameSave, byte[] data, BlockDataCollection parent)
			: base(gameSave, data, parent) {
		}

		public string TrainerName {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(0, raw, 7), gameSave.IsJapanese ? Languages.Japanese : Languages.English); }
			set {
				ByteHelper.ReplaceBytes(raw, 0, GBACharacterEncoding.GetBytes(value, 7, gameSave.IsJapanese ? Languages.Japanese : Languages.English));
				// Make sure the secret base has the correct name too.
				parent.NationalPokedexBAndC.SecretBaseTrainerName = value;
			}
		}
		public Genders TrainerGender {
			get { return (Genders)raw[8]; }
			set {
				raw[8] = (byte)value;
				parent.NationalPokedexBAndC.SecretBaseTrainerGender = value;
			}
		}
		public ushort TrainerID {
			get { return LittleEndian.ToUInt16(raw, 10); }
			set { LittleEndian.WriteUInt16(value, raw, 10); }
		}
		public ushort SecretID {
			get { return LittleEndian.ToUInt16(raw, 12); }
			set { LittleEndian.WriteUInt16(value, raw, 12); }
		}
		public TimeSpan PlayTime {
			get { return new TimeSpan(0, HoursPlayed, MinutesPlayed, SecondsPlayed, FramesPlayed); }
			set {
				HoursPlayed = (ushort)value.Hours;
				MinutesPlayed = (byte)value.Minutes;
				SecondsPlayed = (byte)value.Seconds;
				FramesPlayed = (byte)value.Milliseconds;
			}
		}
		public ushort HoursPlayed {
			get { return LittleEndian.ToUInt16(raw, 14); }
			set { LittleEndian.WriteUInt16(value, raw, 14); }
		}
		public byte MinutesPlayed {
			get { return raw[16]; }
			set { raw[16] = value; }
		}
		public byte SecondsPlayed {
			get { return raw[17]; }
			set { raw[17] = value; }
		}
		public byte FramesPlayed {
			get { return raw[18]; }
			set { raw[18] = value; }
		}
		public ushort BattlePoints {
			get {
				if (GameCode == GameCodes.Emerald)
					return LittleEndian.ToUInt16(raw, 3768);
				return 0;
			}
			set {
				if (GameCode == GameCodes.Emerald)
					LittleEndian.WriteUInt16(value, raw, 3768);
			}
		}
		public ushort BattlePointsWon {
			get {
				if (GameCode == GameCodes.Emerald)
					return LittleEndian.ToUInt16(raw, 3770);
				return 0;
			}
			set {
				if (GameCode == GameCodes.Emerald)
					LittleEndian.WriteUInt16(value, raw, 3770);
			}
		}
		public GameCodes GameCode {
			get {
				if (gameSave.GameType != GameTypes.Any) {
					if (gameSave.GameType == GameTypes.Ruby || gameSave.GameType == GameTypes.Sapphire)
						return GameCodes.RubySapphire;
					else if (gameSave.GameType == GameTypes.FireRed || gameSave.GameType == GameTypes.LeafGreen)
						return GameCodes.FireRedLeafGreen;
					else if (gameSave.GameType == GameTypes.Emerald)
						return GameCodes.Emerald;
				}
				uint code = LittleEndian.ToUInt32(raw, 172);
				switch (code) {
				case 0: return GameCodes.RubySapphire;
				case 1: return GameCodes.FireRedLeafGreen;
				default: return (code != LittleEndian.ToUInt32(raw, 500) ? GameCodes.RubySapphire : GameCodes.Emerald);
				}
			}
		}
		public uint SecurityKey {
			get {
				if (GameCode == GameCodes.Emerald)
					return LittleEndian.ToUInt32(raw, 172);
				else if (GameCode == GameCodes.FireRedLeafGreen)
					return LittleEndian.ToUInt32(raw, 2808); 
				else
					return 0;
			}
		}
		public bool NationalPokedexAUnlocked {
			get {
				if (GameCode == GameCodes.FireRedLeafGreen)
					return raw[27] == 185;
				else
					return raw[25] == 1 && raw[26] == 218;
			}
			set {
				if (GameCode == GameCodes.FireRedLeafGreen) {
					raw[27] = (byte)(value ? 185 : 0);
				}
				else {
					raw[25] = (byte)(value ? 1 : 0);
					raw[26] = (byte)(value ? 218 : 0);
				}
			}
		}
		public bool IsPokemonOwned(ushort dexID) {
			return ByteHelper.GetBit(raw, 40, dexID - 1);
		}
		public void SetPokemonOwned(ushort dexID, bool owned) {
			ByteHelper.SetBit(raw, 40, dexID - 1, owned);
		}
		public bool IsPokemonSeenA(ushort dexID) {
			return ByteHelper.GetBit(raw, 92, dexID - 1);
		}
		public void SetPokemonSeenA(ushort dexID, bool seen) {
			ByteHelper.SetBit(raw, 92, dexID - 1, seen);
		}

		public bool[] PokedexOwned {
			get {
				BitArray bitArray = ByteHelper.GetBits(raw, 40, 0, 386);
				bool[] flags = new bool[386];
				for (int i = 0; i < 386; i++)
					flags[i] = bitArray[i];
				return flags;
			}
			set { ByteHelper.SetBits(raw, 40, 0, new BitArray(value)); }
		}
		public bool[] PokedexSeenA {
			get {
				BitArray bitArray = ByteHelper.GetBits(raw, 92, 0, 386);
				bool[] flags = new bool[386];
				for (int i = 0; i < 386; i++)
					flags[i] = bitArray[i];
				return flags;
			}
			set { ByteHelper.SetBits(raw, 92, 0, new BitArray(value)); }
		}

		public TimeSpan SaveTimestamp {
			get { return new TimeSpan(LittleEndian.ToUInt16(raw, 10), raw[12], raw[13], raw[14]); }
			set {
				LittleEndian.WriteUInt16((ushort)value.TotalDays, raw, 10);
				raw[12] = (byte)value.Hours;
				raw[13] = (byte)value.Minutes;
				raw[14] = (byte)value.Seconds;
			}
		}
		public TimeSpan RealTimeClock {
			get { return new TimeSpan(LittleEndian.ToUInt16(raw, 152), raw[154], raw[155], raw[156]); }
			set {
				LittleEndian.WriteUInt16((ushort)value.TotalDays, raw, 152);
				raw[154] = (byte)value.Hours;
				raw[155] = (byte)value.Minutes;
				raw[156] = (byte)value.Seconds;
			}
		}

		public uint UnownPokedexPersonality {
			get { return LittleEndian.ToUInt32(raw, 28); }
			set { LittleEndian.WriteUInt32(value, raw, 28); }
		}
		public uint SpindaPokedexPersonality {
			get { return LittleEndian.ToUInt32(raw, 32); }
			set { LittleEndian.WriteUInt32(value, raw, 32); }
		}
	}
}
