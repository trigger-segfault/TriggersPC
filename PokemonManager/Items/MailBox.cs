using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PokemonManager.Items {
	public class MailboxEventArgs : EventArgs {
		public int Index { get; set; }
		public int OldIndex { get; set; }
		public int NewIndex { get; set; }
		public Mail Mail { get; set; }
	}

	public delegate void MailboxEventHandler(object sender, MailboxEventArgs e);

	public class Mailbox {
		private IGameSave gameSave;
		private byte[] raw;
		private List<Mail> partyMail;
		private List<Mail> boxMail;
		private uint mailboxSize;

		// ListView Management
		private ObservableCollection<ListViewItem> listViewItems;
		public event MailboxEventHandler AddListViewItem;
		public event MailboxEventHandler UpdateListViewItem;
		public event MailboxEventHandler RemoveListViewItem;
		public event MailboxEventHandler MoveListViewItem;

		public Mailbox(IGameSave gameSave, uint mailboxSize) {
			this.gameSave		= gameSave;
			this.raw			= null;
			this.partyMail		= new List<Mail>();
			this.boxMail		= new List<Mail>();
			this.mailboxSize	= mailboxSize;

			this.listViewItems	= new ObservableCollection<ListViewItem>();
		}

		public IGameSave GameSave {
			get { return gameSave; }
		}
		public ObservableCollection<ListViewItem> ListViewItems {
			get { return listViewItems; }
		}

		public int PartyMailCount {
			get { return partyMail.Count; }
		}
		public int MailboxCount {
			get { return boxMail.Count; }
		}
		public uint MailboxSize {
			get { return mailboxSize; }
		}

		public Mail this[int index, bool party = false] {
			get {
				if (party && index >= 0 && index < partyMail.Count)
					return partyMail[index];
				else if (!party && index >= 0 && index < boxMail.Count)
					return boxMail[index];
				return null;
			}
			set {
				if (party && index >= 0 && index < partyMail.Count) {
					gameSave.IsChanged = true;
					partyMail[index] = value;
					if (value != null)
						value.Mailbox = this;
				}
				else if (!party && index >= 0 && index < boxMail.Count) {
					gameSave.IsChanged = true;
					boxMail[index] = value;
					if (value != null)
						value.Mailbox = this;
				}
			}
		}
		public bool HasRoomForMail {
			get { return boxMail.Count < (int)mailboxSize || mailboxSize == 0; }
		}

		public bool ContainsMail(Mail mail, bool party = false) {
			if (party)
				return partyMail.Contains(mail);
			return boxMail.Contains(mail);
		}
		public int IndexOfMail(Mail mail, bool party = false) {
			if (party)
				return partyMail.IndexOf(mail);
			return boxMail.IndexOf(mail);
		}
		public void AddMail(Mail mail, bool party = false) {
			if (party) {
				if (partyMail.Count < 6) {
					gameSave.IsChanged = true;
					partyMail.Add(mail);
					mail.Mailbox = this;
				}
			}
			else if (boxMail.Count < (int)mailboxSize || mailboxSize == 0) {
				gameSave.IsChanged = true;
				boxMail.Add(mail);
				mail.Mailbox = this;
				MailboxEventArgs args = new MailboxEventArgs();
				args.Index = boxMail.Count - 1;
				args.Mail = mail;
				OnAddListViewItem(args);
			}
		}
		public void MoveMail(int oldIndex, int newIndex, bool party = false) {
			gameSave.IsChanged = true;
			Mail mail = boxMail[oldIndex];
			boxMail.RemoveAt(oldIndex);
			boxMail.Insert(newIndex, mail);

			MailboxEventArgs args = new MailboxEventArgs();
			args.OldIndex = oldIndex;
			args.NewIndex = newIndex;
			OnMoveListViewItem(args);
		}
		public void TossMailAt(int index, bool party = false) {
			gameSave.IsChanged = true;
			boxMail.RemoveAt(index);

			MailboxEventArgs args = new MailboxEventArgs();
			args.Index = index;
			OnRemoveListViewItem(args);
		}
		public void Clear() {
			for (int i = 0; i < partyMail.Count; i++)
				partyMail[i] = null;
			//partyMail.Clear();
			boxMail.Clear();
			listViewItems.Clear();
		}
		public void Reset() {
			if (mailboxSize == 0) {
				boxMail.Clear();
				listViewItems.Clear();

				Mail defaultMail = new Mail();
				defaultMail.TrainerName = "Trigger";
				defaultMail.SetTrainerIDs(45465, 28557);
				defaultMail.MailItemID = 130;
				defaultMail.OriginalHolderDexID = 197;

				defaultMail.SetEasyChatCodeAt(0, 0x0806);
				defaultMail.SetEasyChatCodeAt(1, 0x0E15);
				defaultMail.SetEasyChatCodeAt(2, 0x0811);
				defaultMail.SetEasyChatCodeAt(3, 0x102B);
				defaultMail.SetEasyChatCodeAt(4, 0x162C);
				defaultMail.SetEasyChatCodeAt(5, 0x0A28);
				defaultMail.SetEasyChatCodeAt(6, 0x1A25);
				defaultMail.SetEasyChatCodeAt(8, 0x122A);
				
				boxMail.Add(defaultMail);
			}
		}

		#region ListView Methods

		private void OnAddListViewItem(MailboxEventArgs e) {
			if (AddListViewItem != null) {
				AddListViewItem(this, e);
			}
		}
		private void OnUpdateListViewItem(MailboxEventArgs e) {
			if (UpdateListViewItem != null) {
				UpdateListViewItem(this, e);
			}
		}
		private void OnRemoveListViewItem(MailboxEventArgs e) {
			if (RemoveListViewItem != null) {
				RemoveListViewItem(this, e);
			}
		}
		private void OnMoveListViewItem(MailboxEventArgs e) {
			if (MoveListViewItem != null) {
				MoveListViewItem(this, e);
			}
		}
		public void RepopulateListView() {
			listViewItems.Clear();
			for (int i = 0; i < boxMail.Count; i++) {
				MailboxEventArgs args = new MailboxEventArgs();
				args.Index = i;
				args.Mail = boxMail[i];
				OnAddListViewItem(args);
			}
		}

		#endregion

		#region Loading/Saving

		public void Load(byte[] data) {
			this.raw = data;
			if (mailboxSize != 0) {
				for (int i = 0; i < 6; i++) {
					Mail mail = new Mail(ByteHelper.SubByteArray(i * 36, data, 36));
					mail.Mailbox = this;
					if (mail.MailItemID != 0)
						this.partyMail.Add(mail);
					else
						this.partyMail.Add(null);
				}
				for (int i = 0; i < mailboxSize || mailboxSize == 0; i++) {
					Mail mail = new Mail(ByteHelper.SubByteArray((i + 6) * 36, data, 36));
					mail.Mailbox = this;
					if (mail.MailItemID != 0)
						this.boxMail.Add(mail);
				}
			}
			else {
				uint version = LittleEndian.ToUInt32(data, 0);
				uint count = LittleEndian.ToUInt32(data, 4);
				for (int i = 0; i < count; i++) {
					Mail mail = new Mail(ByteHelper.SubByteArray(8 + i * 36, data, 36));
					mail.Mailbox = this;
					this.boxMail.Add(mail);
				}
			}
		}
		public void LoadPart1(byte[] data) {
			if (raw == null)
				raw = new byte[36 * 16];
			ByteHelper.ReplaceBytes(raw, 0, data);
			for (int i = 0; i < 6; i++) {
				Mail mail = new Mail(ByteHelper.SubByteArray(i * 36, data, 36));
				mail.Mailbox = this;
				if (mail.MailItemID != 0)
					this.partyMail.Add(mail);
				else
					this.partyMail.Add(null);
			}
			for (int i = 0; i < 6; i++) {
				Mail mail = new Mail(ByteHelper.SubByteArray((i + 6) * 36, data, 36));
				mail.Mailbox = this;
				if (mail.MailItemID != 0)
					this.boxMail.Insert(i, mail);
			}
		}
		public void LoadPart2(byte[] data) {
			if (raw == null)
				raw = new byte[36 * 16];
			ByteHelper.ReplaceBytes(raw, 36 * 12, data);
			for (int i = 0; i < 4; i++) {
				Mail mail = new Mail(ByteHelper.SubByteArray(i * 36, data, 36));
				mail.Mailbox = this;
				if (mail.MailItemID != 0)
					this.boxMail.Add(mail);
			}
		}
		public byte[] GetFinalDataPart1() {
			return ByteHelper.SubByteArray(0, GetFinalData(), 36 * 12);
		}
		public byte[] GetFinalDataPart2() {
			return ByteHelper.SubByteArray(36 * 12, GetFinalData(), 36 * 4);
		}
		public byte[] GetFinalData() {
			// Your PC doesn't store party mail
			if (mailboxSize != 0) {
				for (int i = 0; i < 6; i++) {
					Mail mail = (partyMail[i] != null ? partyMail[i] : new Mail());
					ByteHelper.ReplaceBytes(raw, i * 36, mail.Raw);
				}
				for (int i = 0; i < (mailboxSize == 0 ? boxMail.Count : (int)mailboxSize); i++) {
					Mail mail = (i < boxMail.Count ? boxMail[i] : new Mail());
					ByteHelper.ReplaceBytes(raw, (i + 6) * 36, mail.Raw);
				}
			}
			else {
				raw = new byte[8 + boxMail.Count * 36];
				LittleEndian.WriteUInt32(1, raw, 0); // version
				LittleEndian.WriteUInt32((uint)boxMail.Count, raw, 4);
				for (int i = 0; i < boxMail.Count; i++) {
					ByteHelper.ReplaceBytes(raw, 8 + i * 36, boxMail[i].Raw);
				}
			}
			return raw;
		}

		#endregion
	}
}
