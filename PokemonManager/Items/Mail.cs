using PokemonManager.Game;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {

	public class Mail {

		private byte[] raw;
		private Mailbox mailbox;

		public Mail() {
			this.raw = new byte[36];
			this.OriginalHolderSpeciesID = 1; // Always seems to be set to one on null mail.
			for (int i = 0; i < 9; i++)
				SetEasyChatCodeAt(i, 0xFFFF);
		}

		public Mail(byte[] data) {
			this.raw = data;
		}

		public Mailbox Mailbox {
			get { return mailbox; }
			set { mailbox = value; }
		}

		public byte[] Raw {
			get { return raw; }
		}
		public string TrainerName {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(18, raw, 7), (mailbox != null && mailbox.GameSave.IsJapanese) ? Languages.Japanese : Languages.English); }
			set { ByteHelper.ReplaceBytes(raw, 18, GBACharacterEncoding.GetBytes(value, 7, (mailbox != null && mailbox.GameSave.IsJapanese) ? Languages.Japanese : Languages.English)); }
		}

		// I have NO idea why the game decided it was time to SOMETIMES use big endian for encoding the trainer ID.
		public ushort TrainerID {
			get {
				if ((LittleEndian.ToUInt16(raw, 26) == 0 || LittleEndian.ToUInt16(raw, 26) == 256) && LittleEndian.ToUInt16(raw, 28) != 0)
					return BigEndian.ToUInt16(raw, 28);
				return LittleEndian.ToUInt16(raw, 26);
			}
		}
		public ushort SecretID {
			get {
				if ((LittleEndian.ToUInt16(raw, 26) == 0 || LittleEndian.ToUInt16(raw, 26) == 256) && LittleEndian.ToUInt16(raw, 28) != 0)
					return BigEndian.ToUInt16(raw, 26);
				return LittleEndian.ToUInt16(raw, 28);
			}
		}
		public void SetTrainerIDs(ushort trainerID, ushort secretID) {
			if (secretID == 0 || secretID == 1) {
				BigEndian.WriteUInt16(secretID, raw, 26);
				BigEndian.WriteUInt16(trainerID, raw, 28);
			}
			else {
				LittleEndian.WriteUInt16(trainerID, raw, 26);
				LittleEndian.WriteUInt16(secretID, raw, 28);
			}
		}

		public ushort GetEasyChatCodeAt(int index) {
			if (index < 9)
				return LittleEndian.ToUInt16(raw, index * 2);
			return 0;
		}
		public void SetEasyChatCodeAt(int index, ushort code) {
			if (index < 9)
				LittleEndian.WriteUInt16(code, raw, index * 2);
		}
		public ushort this[int index] {
			get {
				if (index >= 0 && index < 9)
					return LittleEndian.ToUInt16(raw, index * 2);
				return 0;
			}
			set {
				if (index >= 0 && index < 9)
					LittleEndian.WriteUInt16(value, raw, index * 2);
			}
		}
		public ushort OriginalHolderSpeciesID {
			get { return LittleEndian.ToUInt16(raw, 30); }
			set { LittleEndian.WriteUInt16(value, raw, 30); }
		}
		public ushort OriginalHolderDexID {
			get {
				if (OriginalHolderPokemonData != null)
					return PokemonDatabase.GetPokemonFromID(OriginalHolderSpeciesID).DexID;
				return 0;
			}
			set {
				if (OriginalHolderPokemonData != null)
					LittleEndian.WriteUInt16(PokemonDatabase.GetPokemonFromDexID(value).ID, raw, 30);
			}
		}
		public PokemonData OriginalHolderPokemonData {
			get { return PokemonDatabase.GetPokemonFromID(OriginalHolderSpeciesID); }
		}
		public ushort MailItemID {
			get { return LittleEndian.ToUInt16(raw, 32); }
			set { LittleEndian.WriteUInt16(value, raw, 32); }
		}
		public ItemData MailItemData {
			get { return ItemDatabase.GetItemFromID(MailItemID); }
		}
		public byte Unknown1 {
			get { return raw[34]; }
			set { raw[34] = value; }
		}
		public byte Unknown2 {
			get { return raw[35]; }
			set { raw[35] = value; }
		}

		public string Message {
			get {
				StringBuilder builder = new StringBuilder();
				for (int row = 0; row < 5; row++) {
					string line = "";
					for (int column = 0; column < (row == 4 ? 1 : 2); column++) {
						ushort code = GetEasyChatCodeAt(row * 2 + column);
						if (code != 0xFFFF) {
							string word = ItemDatabase.GetEasyChatFromID(code);
							if (line.Length != 0)
								line += " ";
							line += (word ?? "???");
						}
					}
					if (line.Length > 0) {
						if (builder.ToString().Length > 0 && !builder.ToString().EndsWith("\n"))
							builder.Append("\n");
						builder.Append(line);
					}
				}
				return builder.ToString();
			}
		}
		public string[] Lines {
			get {
				List<string> lines = new List<string>();
				for (int row = 0; row < 5; row++) {
					string line = "";
					for (int column = 0; column < (row == 4 ? 1 : 2); column++) {
						ushort code = GetEasyChatCodeAt(row * 2 + column);
						if (code != 0xFFFF) {
							string word = ItemDatabase.GetEasyChatFromID(code);
							if (line.Length != 0)
								line += " ";
							line += (word ?? "???");
						}
					}
					if (line.Length > 0)
						lines.Add(line);
				}
				return lines.ToArray();
			}
		}
		public string[] Words {
			get {
				List<string> words = new List<string>();
				for (int i = 0; i < 9; i++) {
					ushort code = GetEasyChatCodeAt(i);
					if (code != 0xFFFF) {
						string word = ItemDatabase.GetEasyChatFromID(code);
						words.Add(word ?? "???");
					}
				}
				return words.ToArray();
			}
		}
	}
}
