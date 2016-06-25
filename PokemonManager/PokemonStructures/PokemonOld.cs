using PokemonManager.Game;
using PokemonManager.Game.FileStructure.Gen3;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class PokemonOld {

		// Used for Deoxys since his form is game specific
		private byte deoxysFormID;
		// Used to figure out Deoxys main form
		private GameTypes gameType;

		private byte growthSubDataOffset;
		private byte attacksSubDataOffset;
		private byte evConditionSubDataOffset;
		private byte miscSubDataOffset;

		private byte[] raw;

		public byte[] Raw { get { return raw; } }

		public PokemonOld() {
			this.raw = new byte[100];
		}

		public PokemonOld(GameTypes gameType, byte[] data, bool decrypted = false) {
			this.gameType		= gameType;
			this.deoxysFormID	= (byte)(gameType == GameTypes.FireRed ? 1 : (gameType == GameTypes.LeafGreen ? 2 : (gameType == GameTypes.Emerald ? 3 : 0)));
			
			if (decrypted)
				OpenDecryptedData(data);
			else
				OpenEncryptedData(data);
		}

		#region Reading


		public void OpenEncryptedData(byte[] data) {
			if (data.Length != 80 && data.Length != 100)
				throw new Exception("Pokemon data must contain 80 or 100 bytes");
			raw = data;
			byte[] subData = ByteHelper.SubByteArray(32, data, 48);
			SubDataParseWithOrder(subData, SubDataOrder);
			if (data.Length != 80)
				return;
			RecalculateStats();
		}

		public void OpenDecryptedData(byte[] data) {
			growthSubDataOffset = (byte)(32 + 0);
			attacksSubDataOffset = (byte)(32 + 12);
			evConditionSubDataOffset = (byte)(32 + 24);
			miscSubDataOffset = (byte)(32 + 36);

			if (data.Length != 80 && data.Length != 100)
				throw new Exception("Pokemon data must contain 80 or 100 bytes");
			raw = data;
			if (data.Length != 80)
				return;
			RecalculateStats();
		}

		#endregion

		#region Stats

		public void RecalculateStats() {
			if (raw.Length == 80) {
				List<byte> list = new List<byte>();
				list.AddRange(raw);
				list.AddRange(new byte[20]);
				raw = list.ToArray();
			}
			NatureData nature = PokemonDatabase.GetNatureFromID(NatureID);
			Level = PokemonDatabase.GetLevelFromExperience(PokemonData.ExperienceGroup, Experience);
			PokerusRemaining = byte.MaxValue;
			HP = (DexID != 292 ? Convert.ToUInt16(Math.Floor(((double)PokemonData.HP * 2 + (double)HPIV + (double)HPEV / 4) * (double)Level / 100 + 10 + (double)Level)) : (ushort)1);
			CurrentHP = HP;
			Attack = Convert.ToUInt16(Math.Floor(Math.Floor(((double)PokemonData.Attack * 2 + (double)AttackIV + (double)AttackEV / 4) * (double)Level / 100 + 5) * nature.AttackModifier));
			Defense = Convert.ToUInt16(Math.Floor(Math.Floor(((double)PokemonData.Defense * 2 + (double)DefenseIV + (double)DefenseEV / 4) * (double)Level / 100 + 5) * nature.DefenseModifier));
			SpAttack = Convert.ToUInt16(Math.Floor(Math.Floor(((double)PokemonData.SpAttack * 2 + (double)SpAttackIV + (double)SpAttackEV / 4) * (double)Level / 100 + 5) * nature.SpAttackModifier));
			SpDefense = Convert.ToUInt16(Math.Floor(Math.Floor(((double)PokemonData.SpDefense * 2 + (double)SpDefenseIV + (double)SpDefenseEV / 4) * (double)Level / 100 + 5) * nature.SpDefenseModifier));
			Speed = Convert.ToUInt16(Math.Floor(Math.Floor(((double)PokemonData.Speed * 2 + (double)SpeedIV + (double)SpeedEV / 4) * (double)Level / 100 + 5) * nature.SpeedModifier));
		}

		#endregion

		#region Unencrypted Data

		public uint Personality {
			get { return BitConverter.ToUInt32(ByteHelper.SubByteArray(0, raw, 4), 0); }
			set { ByteHelper.ReplaceBytes(raw, 0, BitConverter.GetBytes(value)); }
		}
		public uint FullTrainerID {
			get { return BitConverter.ToUInt32(ByteHelper.SubByteArray(4, raw, 4), 0); }
			set { ByteHelper.ReplaceBytes(raw, 4, BitConverter.GetBytes(value)); }
		}
		public ushort TrainerID {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(4, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 4, BitConverter.GetBytes(value)); }
		}
		public ushort SecretID {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(6, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 6, BitConverter.GetBytes(value)); }
		}
		public string Nickname {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(8, raw, 10)); }
			set { ByteHelper.ReplaceBytes(raw, 8, GBACharacterEncoding.GetBytes(value, 10)); }
		}
		public Languages Language {
			get { return (Languages)BitConverter.ToUInt16(ByteHelper.SubByteArray(18, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 6, BitConverter.GetBytes((ushort)value)); }
		}
		public string TrainerName {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(20, raw, 7)); }
			set { ByteHelper.ReplaceBytes(raw, 20, GBACharacterEncoding.GetBytes(value, 7)); }
		}
		public MarkingFlags Markings {
			get { return (MarkingFlags)raw[27]; }
			set { raw[27] = (byte)value; }
		}
		public ushort Checksum {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(28, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 28, BitConverter.GetBytes(value)); }
		}
		public ushort Unknown {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(30, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 30, BitConverter.GetBytes(value)); }
		}
		public StatusConditionFlags StatusCondition {
			get { return (StatusConditionFlags)BitConverter.ToUInt32(ByteHelper.SubByteArray(80, raw, 4), 0); }
			set { ByteHelper.ReplaceBytes(raw, 80, BitConverter.GetBytes((uint)value)); }
		}
		public byte Level {
			get { return raw[84]; }
			set { raw[84] = value; }
		}
		public byte PokerusRemaining {
			get { return raw[85]; }
			set { raw[85] = value; }
		}
		public ushort CurrentHP {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(86, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 86, BitConverter.GetBytes(value)); }
		}
		public ushort HP {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(88, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 88, BitConverter.GetBytes(value)); }
		}
		public ushort Attack {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(90, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 90, BitConverter.GetBytes(value)); }
		}
		public ushort Defense {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(92, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 92, BitConverter.GetBytes(value)); }
		}
		public ushort Speed {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(94, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 94, BitConverter.GetBytes(value)); }
		}
		public ushort SpAttack {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(96, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 96, BitConverter.GetBytes(value)); }
		}
		public ushort SpDefense {
			get { return BitConverter.ToUInt16(ByteHelper.SubByteArray(98, raw, 2), 0); }
			set { ByteHelper.ReplaceBytes(raw, 98, BitConverter.GetBytes(value)); }
		}

		#endregion

		#region Encrypted Data

		public ushort SpeciesID {
			get { return BitConverter.ToUInt16(GrowthSubData, 0); }
			set { ByteHelper.ReplaceBytes(raw, growthSubDataOffset, BitConverter.GetBytes(value)); }
		}
		public ushort HelpItemID {
			get { return BitConverter.ToUInt16(GrowthSubData, 2); }
			set { ByteHelper.ReplaceBytes(raw, growthSubDataOffset + 2, BitConverter.GetBytes(value)); }
		}
		public uint Experience {
			get { return BitConverter.ToUInt32(GrowthSubData, 4); }
			set { ByteHelper.ReplaceBytes(raw, growthSubDataOffset + 4, BitConverter.GetBytes(value)); }
		}
		public byte Move1PPUpsUsed {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(new byte[1] { GrowthSubData[8] }), 0, 2)); }
			set { raw[growthSubDataOffset + 8] = ByteHelper.InserBit(raw[growthSubDataOffset + 8], ByteHelper.SubBit(new BitArray(new byte[1] { value }), 0, 2), 0); }
		}
		public byte Move2PPUpsUsed {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(new byte[1] { GrowthSubData[8] }), 2, 2)); }
			set { raw[growthSubDataOffset + 8] = ByteHelper.InserBit(raw[growthSubDataOffset + 8], ByteHelper.SubBit(new BitArray(new byte[1] { value }), 0, 2), 2); }
		}
		public byte Move3PPUpsUsed {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(new byte[1] { GrowthSubData[8] }), 4, 2)); }
			set { raw[growthSubDataOffset + 8] = ByteHelper.InserBit(raw[growthSubDataOffset + 8], ByteHelper.SubBit(new BitArray(new byte[1] { value }), 0, 2), 4); }
		}
		public byte Move4PPUpsUsed {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(new byte[1] { GrowthSubData[8] }), 6, 2)); }
			set { raw[growthSubDataOffset + 8] = ByteHelper.InserBit(raw[growthSubDataOffset + 8], ByteHelper.SubBit(new BitArray(new byte[1] { value }), 0, 2), 6); }
		}
		public byte Friendship {
			get { return GrowthSubData[9]; }
			set { raw[growthSubDataOffset + 9] = value; }
		}

		public ushort Move1ID {
			get { return BitConverter.ToUInt16(AttacksSubData, 0); }
			set { ByteHelper.ReplaceBytes(raw, attacksSubDataOffset, BitConverter.GetBytes(value)); }
		}
		public ushort Move2ID {
			get { return BitConverter.ToUInt16(AttacksSubData, 2); }
			set { ByteHelper.ReplaceBytes(raw, attacksSubDataOffset + 2, BitConverter.GetBytes(value)); }
		}
		public ushort Move3ID {
			get { return BitConverter.ToUInt16(AttacksSubData, 4); }
			set { ByteHelper.ReplaceBytes(raw, attacksSubDataOffset + 4, BitConverter.GetBytes(value)); }
		}
		public ushort Move4ID {
			get { return BitConverter.ToUInt16(AttacksSubData, 6); }
			set { ByteHelper.ReplaceBytes(raw, attacksSubDataOffset + 6, BitConverter.GetBytes(value)); }
		}
		public byte Move1PP {
			get { return AttacksSubData[8]; }
			set { raw[attacksSubDataOffset + 8] = value; }
		}
		public byte Move2PP {
			get { return AttacksSubData[9]; }
			set { raw[attacksSubDataOffset + 9] = value; }
		}
		public byte Move3PP {
			get { return AttacksSubData[10]; }
			set { raw[attacksSubDataOffset + 10] = value; }
		}
		public byte Move4PP {
			get { return AttacksSubData[11]; }
			set { raw[attacksSubDataOffset + 11] = value; }
		}

		public byte HPEV {
			get { return EVConditionSubData[0]; }
			set { raw[evConditionSubDataOffset] = value; }
		}
		public byte AttackEV {
			get { return EVConditionSubData[1]; }
			set { raw[evConditionSubDataOffset + 1] = value; }
		}
		public byte DefenseEV {
			get { return EVConditionSubData[2]; }
			set { raw[evConditionSubDataOffset + 2] = value; }
		}
		public byte SpeedEV {
			get { return EVConditionSubData[3]; }
			set { raw[evConditionSubDataOffset + 3] = value; }
		}
		public byte SpAttackEV {
			get { return EVConditionSubData[4]; }
			set { raw[evConditionSubDataOffset + 4] = value; }
		}
		public byte SpDefenseEV {
			get { return EVConditionSubData[5]; }
			set { raw[evConditionSubDataOffset + 5] = value; }
		}
		public byte Coolness {
			get { return EVConditionSubData[6]; }
			set { raw[evConditionSubDataOffset + 6] = value; }
		}
		public byte Beauty {
			get { return EVConditionSubData[7]; }
			set { raw[evConditionSubDataOffset + 7] = value; }
		}
		public byte Cuteness {
			get { return EVConditionSubData[8]; }
			set { raw[evConditionSubDataOffset + 8] = value; }
		}
		public byte Smartness {
			get { return EVConditionSubData[9]; }
			set { raw[evConditionSubDataOffset + 9] = value; }
		}
		public byte Toughness {
			get { return EVConditionSubData[10]; }
			set { raw[evConditionSubDataOffset + 10] = value; }
		}
		public byte Feel {
			get { return EVConditionSubData[11]; }
			set { raw[evConditionSubDataOffset + 11] = value; }
		}

		public PokerusStatuses PokerusStatus {
			get {
				BitArray bits = new BitArray(new byte[1]{ MiscSubData[0] });
				byte num1 = ByteHelper.BitToByte(ByteHelper.SubBit(bits, 0, 4));
				byte num2 = ByteHelper.BitToByte(ByteHelper.SubBit(bits, 4, 4));
				if ((int)num1 == 0 && (int)num2 > 0)
					return PokerusStatuses.Cured;
				return (int)num1 > 0 ? PokerusStatuses.Infected : PokerusStatuses.None;
			}
			set {
				if (value == PokerusStatuses.None)
					raw[miscSubDataOffset] = (byte)0;
				else if (value == PokerusStatuses.Infected) {
					BitArray bits1 = new BitArray(4);
					bits1.SetAll(true);
					BitArray bits2 = new BitArray(4);
					bits2[0] = bits2[1] = true;
					int num1 = (int)ByteHelper.InserBit(raw[miscSubDataOffset], bits1, 0);
					int num2 = (int)ByteHelper.InserBit(raw[miscSubDataOffset], bits2, 4);
				}
				else if (value == PokerusStatuses.Cured) {
					BitArray bits1 = new BitArray(4);
					bits1.SetAll(false);
					BitArray bits2 = new BitArray(4);
					bits2[0] = bits2[1] = true;
					int num1 = (int)ByteHelper.InserBit(raw[miscSubDataOffset], bits1, 0);
					int num2 = (int)ByteHelper.InserBit(raw[miscSubDataOffset], bits2, 4);
				}
			}
		}
		public byte MetLocationID {
			get { return MiscSubData[1]; }
			set { raw[miscSubDataOffset + 1] = value; }
		}
		public byte LevelMet {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(2, MiscSubData, 2)), 0, 7)); }
			set { raw[miscSubDataOffset + 2] = ByteHelper.InserBit(raw[miscSubDataOffset + 2], ByteHelper.SubBit(new BitArray(new byte[1]{ value }), 0, 7), 0); }
		}
		public GameOrigins Origin {
			get { return (GameOrigins)ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(2, MiscSubData, 2)), 7, 4)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 2, ByteHelper.InserBit(ByteHelper.SubByteArray(miscSubDataOffset + 2, raw, 2), ByteHelper.SubBit(new BitArray(new byte[1]{ (byte)value }), 0, 4), 7)); }
		}
		public byte BallCaughtInID {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(2, MiscSubData, 2)), 11, 4)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 2, ByteHelper.InserBit(ByteHelper.SubByteArray(miscSubDataOffset + 2, raw, 2), ByteHelper.SubBit(new BitArray(new byte[1] { (byte)value }), 0, 4), 11)); }
		}
		public Genders TrainerGender {
			get { return (Genders)ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(3, MiscSubData, 1)), 7, 1)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 2, ByteHelper.InserBit(ByteHelper.SubByteArray(miscSubDataOffset + 2, raw, 2), ByteHelper.SubBit(new BitArray(new byte[1]{ (byte)value }), 0, 1), 15)); }
		}
		public byte HPIV {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 0, 5)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), ByteHelper.SubBit(new BitArray(new byte[1]{ Math.Min(value, (byte)31) }), 0, 5), 0)); }
		}
		public byte AttackIV {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 5, 5)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), ByteHelper.SubBit(new BitArray(new byte[1]{ Math.Min(value, (byte)31) }), 0, 5), 5)); }
		}
		public byte DefenseIV {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 10, 5)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), ByteHelper.SubBit(new BitArray(new byte[1]{ Math.Min(value, (byte)31) }), 0, 5), 10)); }
		}
		public byte SpeedIV {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 15, 5)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), ByteHelper.SubBit(new BitArray(new byte[1]{ Math.Min(value, (byte)31) }), 0, 5), 15)); }
		}
		public byte SpAttackIV {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 20, 5)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), ByteHelper.SubBit(new BitArray(new byte[1]{ Math.Min(value, (byte)31) }), 0, 5), 20)); }
		}
		public byte SpDefenseIV {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 25, 5)); }
			set { ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), ByteHelper.SubBit(new BitArray(new byte[1]{ Math.Min(value, (byte)31) }), 0, 5), 25)); }
		}
		public bool IsEgg {
			get { return ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 30, 1)[0]; }
			set {
				BitArray bits = new BitArray(1);
				bits[0] = value;
				ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), bits, 30));
			}
		}
		public bool IsFirstAbility {
			get { return ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(4, MiscSubData, 4)), 31, 1)[0]; }
			set {
				BitArray bits = new BitArray(1);
				bits[0] = value;
				ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 4, ByteHelper.InserBit(ByteHelper.SubByteArray(4, MiscSubData, 4), bits, 31));
			}
		}

		public bool FatefulEncounter {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 31 & 1U); }
			set {
				BitArray bits = new BitArray(1);
				bits[0] = value;
				ByteHelper.ReplaceBytes(raw, miscSubDataOffset + 8, ByteHelper.InserBit(ByteHelper.SubByteArray(8, this.MiscSubData, 4), bits, 31));
			}
		}

		public byte CoolRibbonCount {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(8, MiscSubData, 4)), 0, 3)); }
		}
		public byte BeautyRibbonCount {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(8, MiscSubData, 4)), 3, 3)); }
		}
		public byte CuteRibbonCount {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(8, MiscSubData, 4)), 6, 3)); }
		}
		public byte SmartRibbonCount {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(8, MiscSubData, 4)), 9, 3)); }
		}
		public byte ToughRibbonCount {
			get { return ByteHelper.BitToByte(ByteHelper.SubBit(new BitArray(ByteHelper.SubByteArray(8, MiscSubData, 4)), 12, 3)); }
		}
		public bool HasChampionRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 15 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 15); }
		}
		public bool HasWinningRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 16 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 16); }
		}
		public bool HasVictoryRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 17 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 17); }
		}
		public bool HasArtistRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 18 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 18); }
		}
		public bool HasEffortRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 19 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 19); }
		}
		public bool HasMarineRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 20 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 20); }
		}
		public bool HasLandRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 21 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 21); }
		}
		public bool HasSkyRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 22 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 22); }
		}
		public bool HasCountryRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 23 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 23); }
		}
		public bool HasNationalRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 24 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 24); }
		}
		public bool HasEarthRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 25 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 25); }
		}
		public bool HasWorldRibbon {
			get { return Convert.ToBoolean(BitConverter.ToUInt32(this.MiscSubData, 8) >> 26 & 1U); }
			set { ByteHelper.SetBit(raw, miscSubDataOffset + 8, true, 26); }
		}

		#endregion

		#region Encryption

		private byte[] GrowthSubData {
			get { return ByteHelper.SubByteArray(growthSubDataOffset, raw, 12); }
		}
		private byte[] AttacksSubData {
			get { return ByteHelper.SubByteArray(attacksSubDataOffset, raw, 12); }
		}
		private byte[] EVConditionSubData {
			get { return ByteHelper.SubByteArray(evConditionSubDataOffset, raw, 12); }
		}
		private byte[] MiscSubData {
			get { return ByteHelper.SubByteArray(miscSubDataOffset, raw, 12); }
		}
		private uint SecurityKey {
			get { return FullTrainerID ^ Personality; }
		}
		public SubDataOrder SubDataOrder {
			get { return (SubDataOrder)(Personality % 24U); }
		}

		private void SubDataParseWithOrder(byte[] subData, SubDataOrder order) {
			switch (order) {
			case SubDataOrder.GAEM:
				this.SubDataParse(subData, 0, 12, 24, 36);
				break;
			case SubDataOrder.GAME:
				this.SubDataParse(subData, 0, 12, 36, 24);
				break;
			case SubDataOrder.GEAM:
				this.SubDataParse(subData, 0, 24, 12, 36);
				break;
			case SubDataOrder.GEMA:
				this.SubDataParse(subData, 0, 36, 12, 24);
				break;
			case SubDataOrder.GMAE:
				this.SubDataParse(subData, 0, 24, 36, 12);
				break;
			case SubDataOrder.GMEA:
				this.SubDataParse(subData, 0, 36, 24, 12);
				break;
			case SubDataOrder.AGEM:
				this.SubDataParse(subData, 12, 0, 24, 36);
				break;
			case SubDataOrder.AGME:
				this.SubDataParse(subData, 12, 0, 36, 24);
				break;
			case SubDataOrder.AEGM:
				this.SubDataParse(subData, 24, 0, 12, 36);
				break;
			case SubDataOrder.AEMG:
				this.SubDataParse(subData, 36, 0, 12, 24);
				break;
			case SubDataOrder.AMGE:
				this.SubDataParse(subData, 24, 0, 36, 12);
				break;
			case SubDataOrder.AMEG:
				this.SubDataParse(subData, 36, 0, 24, 12);
				break;
			case SubDataOrder.EGAM:
				this.SubDataParse(subData, 12, 24, 0, 36);
				break;
			case SubDataOrder.EGMA:
				this.SubDataParse(subData, 12, 36, 0, 24);
				break;
			case SubDataOrder.EAGM:
				this.SubDataParse(subData, 24, 12, 0, 36);
				break;
			case SubDataOrder.EAMG:
				this.SubDataParse(subData, 36, 12, 0, 24);
				break;
			case SubDataOrder.EMGA:
				this.SubDataParse(subData, 24, 36, 0, 12);
				break;
			case SubDataOrder.EMAG:
				this.SubDataParse(subData, 36, 24, 0, 12);
				break;
			case SubDataOrder.MGAE:
				this.SubDataParse(subData, 12, 24, 36, 0);
				break;
			case SubDataOrder.MGEA:
				this.SubDataParse(subData, 12, 36, 24, 0);
				break;
			case SubDataOrder.MAGE:
				this.SubDataParse(subData, 24, 12, 36, 0);
				break;
			case SubDataOrder.MAEG:
				this.SubDataParse(subData, 36, 12, 24, 0);
				break;
			case SubDataOrder.MEGA:
				this.SubDataParse(subData, 24, 36, 12, 0);
				break;
			case SubDataOrder.MEAG:
				this.SubDataParse(subData, 36, 24, 12, 0);
				break;
			}
		}

		private byte[] SubDataMergeWithOrder(byte[] g, byte[] a, byte[] m, byte[] e, SubDataOrder order) {
			switch (order) {
			case SubDataOrder.GAEM:
				return this.SubDataMerge(g, a, e, m);
			case SubDataOrder.GAME:
				return this.SubDataMerge(g, a, m, e);
			case SubDataOrder.GEAM:
				return this.SubDataMerge(g, e, a, m);
			case SubDataOrder.GEMA:
				return this.SubDataMerge(g, e, m, a);
			case SubDataOrder.GMAE:
				return this.SubDataMerge(g, m, a, e);
			case SubDataOrder.GMEA:
				return this.SubDataMerge(g, m, e, a);
			case SubDataOrder.AGEM:
				return this.SubDataMerge(a, g, e, m);
			case SubDataOrder.AGME:
				return this.SubDataMerge(a, g, m, e);
			case SubDataOrder.AEGM:
				return this.SubDataMerge(a, e, g, m);
			case SubDataOrder.AEMG:
				return this.SubDataMerge(a, e, m, g);
			case SubDataOrder.AMGE:
				return this.SubDataMerge(a, m, g, e);
			case SubDataOrder.AMEG:
				return this.SubDataMerge(a, m, e, g);
			case SubDataOrder.EGAM:
				return this.SubDataMerge(e, g, a, m);
			case SubDataOrder.EGMA:
				return this.SubDataMerge(e, g, m, a);
			case SubDataOrder.EAGM:
				return this.SubDataMerge(e, a, g, m);
			case SubDataOrder.EAMG:
				return this.SubDataMerge(e, a, m, g);
			case SubDataOrder.EMGA:
				return this.SubDataMerge(e, m, g, a);
			case SubDataOrder.EMAG:
				return this.SubDataMerge(e, m, a, g);
			case SubDataOrder.MGAE:
				return this.SubDataMerge(m, g, a, e);
			case SubDataOrder.MGEA:
				return this.SubDataMerge(m, g, e, a);
			case SubDataOrder.MAGE:
				return this.SubDataMerge(m, a, g, e);
			case SubDataOrder.MAEG:
				return this.SubDataMerge(m, a, e, g);
			case SubDataOrder.MEGA:
				return this.SubDataMerge(m, e, g, a);
			case SubDataOrder.MEAG:
				return this.SubDataMerge(m, e, a, g);
			default:
				return new byte[48];
			}
		}

		private void SubDataParse(byte[] subData, int growthPos, int attackPos, int evPos, int miscPos) {
			growthSubDataOffset = (byte)(32 + growthPos);
			attacksSubDataOffset = (byte)(32 + attackPos);
			evConditionSubDataOffset = (byte)(32 + evPos);
			miscSubDataOffset = (byte)(32 + miscPos);
			byte[] subDataSection1 = ByteHelper.SubByteArray(growthPos, subData, 12);
			byte[] subDataSection2 = ByteHelper.SubByteArray(attackPos, subData, 12);
			byte[] subDataSection3 = ByteHelper.SubByteArray(evPos, subData, 12);
			byte[] subDataSection4 = ByteHelper.SubByteArray(miscPos, subData, 12);
			ByteHelper.ReplaceBytes(raw, growthSubDataOffset, Decrypt(subDataSection1));
			ByteHelper.ReplaceBytes(raw, attacksSubDataOffset, Decrypt(subDataSection2));
			ByteHelper.ReplaceBytes(raw, evConditionSubDataOffset, Decrypt(subDataSection3));
			ByteHelper.ReplaceBytes(raw, miscSubDataOffset, Decrypt(subDataSection4));
		}

		private byte[] SubDataMerge(byte[] pos1, byte[] pos2, byte[] pos3, byte[] pos4) {
			List<byte> list = new List<byte>(48);
			list.AddRange(pos1);
			list.AddRange(pos2);
			list.AddRange(pos3);
			list.AddRange(pos4);
			return list.ToArray();
		}

		private byte[] Decrypt(byte[] subDataSection) {
			uint securityKey = SecurityKey;
			byte[] bytes1 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 0) ^ securityKey);
			byte[] bytes2 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 4) ^ securityKey);
			byte[] bytes3 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 8) ^ securityKey);
			List<byte> list = new List<byte>(12);
			list.AddRange((IEnumerable<byte>)bytes1);
			list.AddRange((IEnumerable<byte>)bytes2);
			list.AddRange((IEnumerable<byte>)bytes3);
			return list.ToArray();
		}

		private byte[] Encrypt(byte[] subDataSection) {
			uint securityKey = SecurityKey;
			byte[] bytes1 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 0) ^ securityKey);
			byte[] bytes2 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 4) ^ securityKey);
			byte[] bytes3 = BitConverter.GetBytes(BitConverter.ToUInt32(subDataSection, 8) ^ securityKey);
			List<byte> list = new List<byte>(12);
			list.AddRange((IEnumerable<byte>)bytes1);
			list.AddRange((IEnumerable<byte>)bytes2);
			list.AddRange((IEnumerable<byte>)bytes3);
			return list.ToArray();
		}

		#endregion

		#region Personality Related Data

		public byte NatureID {
			get { return (byte)(Personality % 25U); }
		}
		public bool IsShiny {
			get {
				byte[] bytes = BitConverter.GetBytes(Personality);
				return ((int)TrainerID ^ (int)SecretID ^ (int)BitConverter.ToUInt16(bytes, 0) ^ (int)BitConverter.ToUInt16(bytes, 2)) < 8;
			}
		}
		public bool IsSecondAbility {
			get { return (Personality & 0x1) == 0; }
		}
		public byte GenderRatio {
			get { return (byte)(Personality & (uint)byte.MaxValue); }
		}

		#endregion

		#region Useful

		public PokemonData PokemonData {
			get { return PokemonDatabase.GetPokemonFromID(SpeciesID); }
		}

		public ushort DexID {
			get { return PokemonDatabase.GetPokemonFromID(SpeciesID).DexID; }
		}

		private BitArray GetHiddenPowerTypeBits() {
			BitArray typeBits = new BitArray(6);
			typeBits[0] = ByteHelper.GetBit(HPIV, 0);
			typeBits[1] = ByteHelper.GetBit(AttackIV, 0);
			typeBits[2] = ByteHelper.GetBit(DefenseIV, 0);
			typeBits[3] = ByteHelper.GetBit(SpeedIV, 0);
			typeBits[4] = ByteHelper.GetBit(SpAttackIV, 0);
			typeBits[5] = ByteHelper.GetBit(SpDefenseIV, 0);
			return typeBits;
		}
		private void SetHiddenPowerTypeBits(BitArray typeBits) {
			HPIV		= ByteHelper.SetBit(HPIV, typeBits[0], 0);
			AttackIV	= ByteHelper.SetBit(AttackIV, typeBits[1], 0);
			DefenseIV	= ByteHelper.SetBit(DefenseIV, typeBits[2], 0);
			SpeedIV		= ByteHelper.SetBit(SpeedIV, typeBits[3], 0);
			SpAttackIV	= ByteHelper.SetBit(SpAttackIV, typeBits[4], 0);
			SpDefenseIV	= ByteHelper.SetBit(SpDefenseIV, typeBits[5], 0);
		}
		private BitArray GetHiddenPowerDamageBits() {
			BitArray damageBits = new BitArray(6);
			damageBits[0] = ByteHelper.GetBit(HPIV, 1);
			damageBits[1] = ByteHelper.GetBit(AttackIV, 1);
			damageBits[2] = ByteHelper.GetBit(DefenseIV, 1);
			damageBits[3] = ByteHelper.GetBit(SpeedIV, 1);
			damageBits[4] = ByteHelper.GetBit(SpAttackIV, 1);
			damageBits[5] = ByteHelper.GetBit(SpDefenseIV, 1);
			return damageBits;
		}
		private void SetHiddenPowerDamageBits(BitArray damageBits) {
			HPIV		= ByteHelper.SetBit(HPIV, damageBits[0], 1);
			AttackIV	= ByteHelper.SetBit(AttackIV, damageBits[1], 1);
			DefenseIV	= ByteHelper.SetBit(DefenseIV, damageBits[2], 1);
			SpeedIV		= ByteHelper.SetBit(SpeedIV, damageBits[3], 1);
			SpAttackIV	= ByteHelper.SetBit(SpAttackIV, damageBits[4], 1);
			SpDefenseIV	= ByteHelper.SetBit(SpDefenseIV, damageBits[5], 1);
		}

		public PokemonTypes HiddenPowerType {
			get {
				int a = ((int)HPIV        % 2);
				int b = ((int)AttackIV    % 2) * 2;
				int c = ((int)DefenseIV   % 2) * 4;
				int d = ((int)SpeedIV     % 2) * 8;
				int e = ((int)SpAttackIV  % 2) * 16;
				int f = ((int)SpDefenseIV % 2) * 32;
				return (PokemonTypes)((a + b + c + d + e + f) * 15 / 63 + 2); // The plus two is because the types enum is offset by two
			}
			set {
				if (value == PokemonTypes.None || value == PokemonTypes.Normal)
					return; // Invalid Hidden Power type
				// The minus two is because the types enum is offset by two due to including the Unknown and Normal types
				byte typeBits = (byte)(((int)value - 2 - 30) * 63 / 50); // The minimum value for this move type

				// Now we could say that we're done since we have the info to change the IVs.
				// But let's make sure the IVs are as close to the originals as possible.
				BitArray originalTypeBits = GetHiddenPowerTypeBits();
				int minDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(typeBits), originalTypeBits);
				byte minDeviationTypeBits = typeBits;
				while (typeBits + 1 <= 0x3F && GetHiddenPowerType((byte)(typeBits + 1)) == value) {
					typeBits++;
					int newDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(typeBits), originalTypeBits);
					if (newDeviation < minDeviation) {
						minDeviation = newDeviation;
						minDeviationTypeBits = typeBits;
					}
				}

				SetHiddenPowerTypeBits(new BitArray(minDeviationTypeBits));
			}
		}
		public byte HiddenPowerDamage {
			get {
				int u = (((int)HPIV        / 2) % 2);
				int v = (((int)AttackIV    / 2) % 2) * 2;
				int w = (((int)DefenseIV   / 2) % 2) * 4;
				int x = (((int)SpeedIV     / 2) % 2) * 8;
				int y = (((int)SpAttackIV  / 2) % 2) * 16;
				int z = (((int)SpDefenseIV / 2) % 2) * 32;
				return (byte)((u + v + w + x + y + z) * 40 / 63 + 30);
			}
			set {
				if (value < 30 || value > 70)
					return; // Outside of damage range
				byte damageBits = (byte)(((int)value - 30) * 63 / 40); // The minimum value for this damage

				// Now we could say that we're done since we have the info to change the IVs.
				// But let's make sure the IVs are as close to the originals as possible.
				BitArray originalDamageBits = GetHiddenPowerDamageBits();
				int minDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(damageBits), originalDamageBits);
				byte minDeviationDamageBits = damageBits;
				while (damageBits + 1 <= 0x3F && GetHiddenPowerDamage((byte)(damageBits + 1)) == value) {
					damageBits++;
					int newDeviation = ByteHelper.GetBitDeviation(ByteHelper.GetBits(damageBits), originalDamageBits);
					if (newDeviation < minDeviation) {
						minDeviation = newDeviation;
						minDeviationDamageBits = damageBits;
					}
				}

				SetHiddenPowerDamageBits(new BitArray(minDeviationDamageBits));
			}
		}
		private PokemonTypes GetHiddenPowerType(byte typesBits) {
			// The plus two is because the types enum is offset by two due to including Unknown and Normal types
			return (PokemonTypes)((int)typesBits * 15 / 63 + 2);
		}

		private byte GetHiddenPowerDamage(byte damageBits) {
			return (byte)((int)damageBits * 40 / 63 + 30);
		}

		public bool IsHoldingItem {
			get { return HelpItemID != 0; }
		}
		public bool HasNickname {
			get { return false; }
		}
		public void RemoveNickname() {
			Nickname = PokemonData.Name.ToUpper();
		}
		public bool IsCircleMarked {
			get { return Markings.HasFlag(MarkingFlags.Circle); }
			set {
				if (value) Markings |= MarkingFlags.Circle;
				else Markings &= ~MarkingFlags.Circle;
			}
		}
		public bool IsSquareMarked {
			get { return Markings.HasFlag(MarkingFlags.Square); }
			set {
				if (value) Markings |= MarkingFlags.Square;
				else Markings &= ~MarkingFlags.Square;
			}
		}
		public bool IsTriangleMarked {
			get { return Markings.HasFlag(MarkingFlags.Triangle); }
			set {
				if (value) Markings |= MarkingFlags.Triangle;
				else Markings &= ~MarkingFlags.Triangle;
			}
		}
		public bool IsHeartMarked {
			get { return Markings.HasFlag(MarkingFlags.Heart); }
			set {
				if (value) Markings |= MarkingFlags.Heart;
				else Markings &= ~MarkingFlags.Heart;
			}
		}
		public void WipeEVs() {
			HPEV = 0;
			AttackEV = 0;
			DefenseEV = 0;
			SpeedEV = 0;
			SpAttackEV = 0;
			SpDefenseEV = 0;
			RecalculateStats();
		}

		public Genders Gender {
			get {
				if (PokemonData.GenderRatio == 255)
					return Genders.Genderless;
				byte val = (byte)(Personality % 256);
				if (PokemonData.GenderRatio == 254 || val < PokemonData.GenderRatio)
					return Genders.Female;
				else
					return Genders.Male;
			}
		}

		public byte FormID {
			get {
				if (DexID == 201) {// Unown
					BitArray bits = new BitArray(8);
					bits[0] = ByteHelper.GetBit(Personality, 0);
					bits[1] = ByteHelper.GetBit(Personality, 1);
					bits[2] = ByteHelper.GetBit(Personality, 8);
					bits[3] = ByteHelper.GetBit(Personality, 9);
					bits[4] = ByteHelper.GetBit(Personality, 16);
					bits[5] = ByteHelper.GetBit(Personality, 17);
					bits[6] = ByteHelper.GetBit(Personality, 24);
					bits[7] = ByteHelper.GetBit(Personality, 25);
					byte val = ByteHelper.BitsToByte(bits);
					return (byte)(val % 28);
				}
				if (DexID == 386) //Deoxys
					return deoxysFormID;
				return 0;
			}
			set {
				if (DexID == 386)
					deoxysFormID = Math.Max((byte)0, Math.Min((byte)3, value));
			}
		}


		public PokemonFormData PokemonFormData {
			get {
				if (DexID == 201 || DexID == 286)
					return PokemonData.GetForm(FormID);
				return null;
			}
		}


		public Move GetMoveAt(int index) {
			switch (index) {
			case 0: return new Move(Move1ID, Move1PP, Move1PPUpsUsed);
			case 1: return new Move(Move2ID, Move2PP, Move2PPUpsUsed);
			case 2: return new Move(Move3ID, Move3PP, Move3PPUpsUsed);
			case 3: return new Move(Move4ID, Move4PP, Move4PPUpsUsed);
			}
			return new Move();
		}

		public void SetMoveAt(int index, Move move) {
			switch (index) {
			case 0: Move1ID = move.ID; Move1PPUpsUsed = move.PPUpsUsed; Move1PP = move.PP; break;
			case 1: Move2ID = move.ID; Move2PPUpsUsed = move.PPUpsUsed; Move2PP = move.PP; break;
			case 2: Move3ID = move.ID; Move3PPUpsUsed = move.PPUpsUsed; Move3PP = move.PP; break;
			case 3: Move4ID = move.ID; Move4PPUpsUsed = move.PPUpsUsed; Move4PP = move.PP; break;
			}
		}
		public void SetMoveAt(int index, ushort id) {
			switch (index) {
			case 0: Move1ID = move.ID; Move1PPUpsUsed = 0; Move1PP = Move1; break;
			case 1: Move2ID = move.ID; Move2PPUpsUsed = 0; Move2PP = move.PP; break;
			case 2: Move3ID = move.ID; Move3PPUpsUsed = 0; Move3PP = move.PP; break;
			case 3: Move4ID = move.ID; Move4PPUpsUsed = 0; Move4PP = move.PP; break;
			}
		}

		public byte GetMovePPAt(int index) {
			switch (index) {
			case 0: return Move1PP;
			case 1: return Move2PP;
			case 2: return Move3PP;
			case 3: return Move4PP;
			}
			return 0;
		}
		public void SetMovePPAt(int index, byte pp) {
			switch (index) {
			case 0: Move1PP = pp; break;
			case 1: Move2PP = pp; break;
			case 2: Move3PP = pp; break;
			case 3: Move4PP = pp; break;
			}
		}
		public byte GetMoveTotalPPAt(int index) {
			switch (index) {
			case 0: return Move1TotalPP;
			case 1: return Move2TotalPP;
			case 2: return Move3TotalPP;
			case 3: return Move4TotalPP;
			}
			return 0;
		}
		public byte GetMovePPUpsUsedAt(int index) {
			switch (index) {
			case 0: return Move1PPUpsUsed;
			case 1: return Move2PPUpsUsed;
			case 2: return Move3PPUpsUsed;
			case 3: return Move4PPUpsUsed;
			}
			return 0;
		}
		public void SetMovePPUpsUsedAt(int index, byte ppUpsUsed) {
			switch (index) {
			case 0: Move1PPUpsUsed = ppUpsUsed; break;
			case 1: Move2PPUpsUsed = ppUpsUsed; break;
			case 2: Move3PPUpsUsed = ppUpsUsed; break;
			case 3: Move4PPUpsUsed = ppUpsUsed; break;
			}
		}
		public ushort GetMoveIDAt(int index) {
			switch (index) {
			case 0: return Move1ID;
			case 1: return Move2ID;
			case 2: return Move3ID;
			case 3: return Move4ID;
			}
			return 0;
		}
		public void SetMoveIDAt(int index, ushort id) {
			switch (index) {
			case 0: Move1ID = id; break;
			case 1: Move2ID = id; break;
			case 2: Move3ID = id; break;
			case 3: Move4ID = id; break;
			}
		}

		public void MoveMove(int oldIndex, int newIndex) {
			List<Move> moveList = new List<Move>();
			for (int i = 0; i < NumMoves; i++)
				moveList.Add(GetMoveAt(i));
			Move move = moveList[oldIndex];
			moveList.RemoveAt(oldIndex);
			moveList.Insert(newIndex, move);
			for (int i = 0; i < 4; i++) {
				if (i < NumMoves)
					SetMoveAt(i, moveList[i]);
				else
					SetMoveAt(i, new Move());
			}
		}

		public void SwitchMove(int move1Index, int move2Index) {
			List<Move> moveList = new List<Move>();
			for (int i = 0; i < NumMoves; i++)
				moveList.Add(GetMoveAt(i));
			Move move1 = moveList[move1Index];
			moveList[move1Index] = moveList[move2Index];
			moveList[move2Index] = move1;
			for (int i = 0; i < 4; i++) {
				if (i < NumMoves)
					SetMoveAt(i, moveList[i]);
				else
					SetMoveAt(i, new Move());
			}
		}

		public void RemoveMoveAt(int index) {
			List<Move> moveList = new List<Move>();
			for (int i = 0; i < NumMoves; i++)
				moveList.Add(GetMoveAt(i));
			moveList.RemoveAt(index);
			for (int i = 0; i < 4; i++) {
				if (i < NumMoves)
					SetMoveAt(i, moveList[i]);
				else
					SetMoveAt(i, new Move());
			}
		}

		public void AddMove(Move move) {
			if (NumMoves < 4) {
				List<Move> moveList = new List<Move>();
				for (int i = 0; i < NumMoves; i++)
					moveList.Add(GetMoveAt(i));
				moveList.Add(move);
				for (int i = 0; i < 4; i++) {
					if (i < NumMoves)
						SetMoveAt(i, moveList[i]);
					else
						SetMoveAt(i, new Move());
				}
			}
		}
		public void AddMove(ushort id) {
			if (NumMoves < 4) {
				List<Move> moveList = new List<Move>();
				for (int i = 0; i < NumMoves; i++)
					moveList.Add(GetMoveAt(i));
				moveList.Add(move);
				for (int i = 0; i < 4; i++) {
					if (i < NumMoves)
						SetMoveAt(i, moveList[i]);
					else
						SetMoveAt(i, new Move());
				}
			}
		}
		public void InsertMove(int index, Move move) {
			if (NumMoves < 4) {
				List<Move> moveList = new List<Move>();
				for (int i = 0; i < NumMoves; i++)
					moveList.Add(GetMoveAt(i));
				moveList.Insert(index, move);
				for (int i = 0; i < 4; i++) {
					if (i < NumMoves)
						SetMoveAt(i, moveList[i]);
					else
						SetMoveAt(i, new Move());
				}
			}
		}

		#endregion

		#region Saving

		public ushort CalculateChecksum() {
			ushort checksum = 0;
			byte[] numArray = SubDataMerge(GrowthSubData, AttacksSubData, MiscSubData, EVConditionSubData);
			int startIndex = 0;
			while (startIndex < 48) {
				checksum += BitConverter.ToUInt16(numArray, startIndex);
				startIndex += 2;
			}
			return checksum;
		}

		public byte[] GetFinalEncryptedData() {
			byte[] rawData = raw.Clone() as byte[];
			byte[] g = Encrypt(GrowthSubData);
			byte[] a = Encrypt(AttacksSubData);
			byte[] e = Encrypt(EVConditionSubData);
			byte[] m = Encrypt(MiscSubData);
			byte[] dataToReplace = SubDataMergeWithOrder(g, a, m, e, SubDataOrder);
			ByteHelper.ReplaceBytes(rawData, 32, dataToReplace);
			byte[] bytes = BitConverter.GetBytes(CalculateChecksum());
			ByteHelper.ReplaceBytes(rawData, 28, bytes);
			return rawData;
		}

		public byte[] GetFinalDecryptedData() {
			ByteHelper.ReplaceBytes(raw, 28, BitConverter.GetBytes(this.CalculateChecksum()));
			return raw;
		}

		public void Save(string filePath, bool decrypted = true) {
			if (decrypted)
				File.WriteAllBytes(filePath, GetFinalDecryptedData());
			else
				File.WriteAllBytes(filePath, GetFinalEncryptedData());
		}

		#endregion
	}
}
