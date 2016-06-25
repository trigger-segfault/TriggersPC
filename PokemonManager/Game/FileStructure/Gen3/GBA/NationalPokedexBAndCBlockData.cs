using PokemonManager.Items;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class NationalPokedexBAndCBlockData : BlockData {

		public NationalPokedexBAndCBlockData(IGameSave gameSave, byte[] data, BlockDataCollection parent)
			: base(gameSave, data, parent) {
			if (parent.GameCode == GameCodes.RubySapphire) {
				if (parent.Inventory.Decorations == null)
					parent.Inventory.AddDecorationInventory();
				for (int i = 0; i < 16; i++) {
					byte id = raw[2714 + i];
					if (id != 0) {
						byte x = ByteHelper.BitsToByte(raw, 2714 + 16 + i, 4, 4);
						byte y = ByteHelper.BitsToByte(raw, 2714 + 16 + i, 0, 4);
						parent.Inventory.Decorations.SecretBaseDecorations.Add(new PlacedDecoration(id, x, y));
					}
				}
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				if (parent.Inventory.Decorations == null)
					parent.Inventory.AddDecorationInventory();
				for (int i = 0; i < 16; i++) {
					byte id = raw[2862 + i];
					if (id != 0) {
						byte x = ByteHelper.BitsToByte(raw, 2862 + 16 + i, 4, 4);
						byte y = ByteHelper.BitsToByte(raw, 2862 + 16 + i, 0, 4);
						parent.Inventory.Decorations.SecretBaseDecorations.Add(new PlacedDecoration(id, x, y));
					}
				}
			}
		}

		public ushort VolcanicAsh {
			get {
				if (parent.GameCode == GameCodes.Emerald)
					return LittleEndian.ToUInt16(raw, 1196);
				else if (parent.GameCode == GameCodes.RubySapphire)
					return LittleEndian.ToUInt16(raw, 1104);
				return 0;
			}
			set {
				if (parent.GameCode == GameCodes.Emerald)
					LittleEndian.WriteUInt16(value, raw, 1196);
				else if (parent.GameCode == GameCodes.RubySapphire)
					LittleEndian.WriteUInt16(value, raw, 1104);
			}
		}

		public ushort MirageIslandValue {
			get {
				if (parent.GameCode == GameCodes.RubySapphire)
					return LittleEndian.ToUInt16(raw, 1032);
				else if (parent.GameCode == GameCodes.Emerald)
					return LittleEndian.ToUInt16(raw, 1124);
				return 0;
			}
			set {
				if (parent.GameCode == GameCodes.RubySapphire)
					LittleEndian.WriteUInt16(value, raw, 1032);
				else if (parent.GameCode == GameCodes.Emerald)
					LittleEndian.WriteUInt16(value, raw, 1124);
			}
		}

		public bool NationalPokedexBUnlocked {
			get {
				if (parent.GameCode == GameCodes.RubySapphire)
					return raw[934] == 64;
				else if (parent.GameCode == GameCodes.Emerald)
					return raw[1026] == 64;
				else
					return raw[104] == 1;
			}
			set {
				if (parent.GameCode == GameCodes.RubySapphire)
					raw[934] = (byte)(value ? 64 : 0);
				else if (parent.GameCode == GameCodes.Emerald)
					raw[1026] = (byte)(value ? 64 : 0);
				else
					raw[104] = (byte)(value ? 1 : 0);
			}
		}
		public bool NationalPokedexCUnlocked {
			get {
				if (parent.GameCode == GameCodes.RubySapphire)
					return raw[1100] == 2 && raw[1101] == 3;
				else if (parent.GameCode == GameCodes.Emerald)
					return raw[1192] == 2 && raw[1193] == 3;
				else
					return raw[284] == 88 && raw[285] == 98;
			}
			set {
				if (parent.GameCode == GameCodes.RubySapphire) {
					raw[1100] = (byte)(value ? 2 : 0);
					raw[1101] = (byte)(value ? 3 : 0);
				}
				else if (parent.GameCode == GameCodes.Emerald) {
					raw[1192] = (byte)(value ? 2 : 0);
					raw[1193] = (byte)(value ? 3 : 0);
				}
				else {
					raw[284] = (byte)(value ? 88 : 0);
					raw[285] = (byte)(value ? 98 : 0);
				}
			}
		}

		public List<bool> GameFlagsPart2 {
			get {
				BitArray bitArray = null;
				if (parent.GameCode == GameCodes.RubySapphire)
					bitArray = ByteHelper.GetBits(raw, 672, 0, 0xFFF);
				else if (parent.GameCode == GameCodes.Emerald)
					bitArray = ByteHelper.GetBits(raw, 752, 0, 0xFFF);
				else // FRLG Second half of flags: (starts with flag 0x500)
					bitArray = ByteHelper.GetBits(raw, 0, 0, 0xAFF);
				List<bool> list = new List<bool>();
				foreach (bool b in bitArray)
					list.Add(b);
				return list;
			}
		}
		public bool GetGameFlag(int index) {
			if (parent.GameCode == GameCodes.RubySapphire)
				return ByteHelper.GetBit(raw, 672, index);
			else if (parent.GameCode == GameCodes.Emerald)
				return ByteHelper.GetBit(raw, 752, index);
			else if (parent.GameCode == GameCodes.FireRedLeafGreen && index >= 0x500)
				return ByteHelper.GetBit(raw, 0, index - 0x500);
			return false;
		}
		public void SetGameFlag(int index, bool flag) {
			if (parent.GameCode == GameCodes.RubySapphire)
				ByteHelper.SetBit(raw, 672, index, flag);
			else if (parent.GameCode == GameCodes.Emerald)
				ByteHelper.SetBit(raw, 752, index, flag);
			else if (parent.GameCode == GameCodes.FireRedLeafGreen && index >= 0x500)
				ByteHelper.SetBit(raw, 0, index - 0x500, flag);
		}

		public AlteringCavePokemon AlteringCavePokemon {
			get {
				ushort pokemon = 0;
				if (parent.GameCode == GameCodes.Emerald)
					pokemon = LittleEndian.ToUInt16(raw, 1176);
				else if (parent.GameCode == GameCodes.FireRedLeafGreen)
					pokemon = LittleEndian.ToUInt16(raw, 200);
				if (pokemon > 8)
					pokemon = 0;
				return (AlteringCavePokemon)pokemon;
			}
			set {
				gameSave.IsChanged = true;
				if (parent.GameCode == GameCodes.Emerald)
					LittleEndian.WriteUInt16((ushort)value, raw, 1176);
				else if (parent.GameCode == GameCodes.FireRedLeafGreen)
					LittleEndian.WriteUInt16((ushort)value, raw, 200);
			}
		}
		public byte SecretBaseLocation {
			get {
				if (parent.GameCode == GameCodes.Emerald)
					return raw[2844];
				else if (parent.GameCode == GameCodes.RubySapphire)
					return raw[2696];
				return 0;
			}
			set {
				gameSave.IsChanged = true;
				if (parent.GameCode == GameCodes.Emerald)
					raw[2844] = value;
				else if (parent.GameCode == GameCodes.RubySapphire)
					raw[2696] = value;
			}
		}
		public string SecretBaseTrainerName {
			get {
				if (parent.GameCode == GameCodes.Emerald)
					return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(2844 + 0x2, raw, 7), SecretBaseLanguage);
				else if (parent.GameCode == GameCodes.RubySapphire)
					return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(2696 + 0x2, raw, 7), SecretBaseLanguage);
				return "";
			}
			set {
				gameSave.IsChanged = true;
				if (parent.GameCode == GameCodes.Emerald)
					ByteHelper.ReplaceBytes(raw, 2844 + 0x2, GBACharacterEncoding.GetBytes(value, 7, SecretBaseLanguage));
				else if (parent.GameCode == GameCodes.RubySapphire)
					ByteHelper.ReplaceBytes(raw, 2696 + 0x2, GBACharacterEncoding.GetBytes(value, 7, SecretBaseLanguage));
			}
		}
		public Genders SecretBaseTrainerGender {
			get {
				if (parent.GameCode == GameCodes.Emerald)
					return (ByteHelper.GetBit(raw, 2844 + 0x1, 4) ? Genders.Female : Genders.Male);
				else if (parent.GameCode == GameCodes.RubySapphire)
					return (ByteHelper.GetBit(raw, 2696 + 0x1, 4) ? Genders.Female : Genders.Male);
				return Genders.Genderless;
			}
			set {
				gameSave.IsChanged = true;
				if (parent.GameCode == GameCodes.Emerald)
					ByteHelper.SetBit(raw, 2844 + 0x1, 4, value == Genders.Female);
				else if (parent.GameCode == GameCodes.RubySapphire)
					ByteHelper.SetBit(raw, 2696 + 0x1, 4, value == Genders.Female);
			}
		}
		public Languages SecretBaseLanguage {
			get {
				Languages language = Languages.NoLanguage;
				if (parent.GameCode == GameCodes.Emerald)
					language = (Languages)(raw[2844 + 0xD] + 0x200);
				else if (parent.GameCode == GameCodes.RubySapphire)
					language = (Languages)(raw[2696 + 0xD] + 0x200);
				if (language == Languages.NoLanguage)
					return gameSave.IsJapanese ? Languages.Japanese : Languages.English;
				return language;
			}
			set {
				gameSave.IsChanged = true;
				if (parent.GameCode == GameCodes.Emerald)
					raw[2844] = (byte)value;
				else if (parent.GameCode == GameCodes.RubySapphire)
					raw[2696] = (byte)value;
			}
		}
				

		public override byte[] GetFinalData() {
			if (parent.GameCode == GameCodes.RubySapphire) {
				for (int i = 0; i < 16; i++) {
					if (i < parent.Inventory.Decorations.SecretBaseDecorations.Count) {
						raw[2714 + i] = parent.Inventory.Decorations.SecretBaseDecorations[i].ID;
						byte xy = 0;
						xy = ByteHelper.SetBits(xy, 4, ByteHelper.GetBits(parent.Inventory.Decorations.SecretBaseDecorations[i].X, 0, 4));
						xy = ByteHelper.SetBits(xy, 0, ByteHelper.GetBits(parent.Inventory.Decorations.SecretBaseDecorations[i].Y, 0, 4));
						raw[2714 + 16 + i] = xy;
					}
					else {
						raw[2714 + i] = 0;
						raw[2714 + 16 + i] = 0;
					}
				}
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				for (int i = 0; i < 16; i++) {
					if (i < parent.Inventory.Decorations.SecretBaseDecorations.Count) {
						raw[2862 + i] = parent.Inventory.Decorations.SecretBaseDecorations[i].ID;
						byte xy = 0;
						xy = ByteHelper.SetBits(xy, 4, ByteHelper.GetBits(parent.Inventory.Decorations.SecretBaseDecorations[i].X, 0, 4));
						xy = ByteHelper.SetBits(xy, 0, ByteHelper.GetBits(parent.Inventory.Decorations.SecretBaseDecorations[i].Y, 0, 4));
						raw[2862 + 16 + i] = xy;
					}
					else {
						raw[2862 + i] = 0;
						raw[2862 + 16 + i] = 0;
					}
				}
			}
			Checksum = CalculateChecksum();
			return raw;
		}
	}
}
