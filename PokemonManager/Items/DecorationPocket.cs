using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PokemonManager.Items {

	public class DecorationPocketEventArgs : EventArgs {
		public int Index { get; set; }
		public int OldIndex { get; set; }
		public int NewIndex { get; set; }
		public Decoration Decoration { get; set; }
		public DecorationTypes PocketType { get; set; }
	}
	public delegate void DecorationPocketEventHandler(object sender, DecorationPocketEventArgs e);

	public class DecorationPocket {

		#region Members

		private DecorationInventory inventory;
		private List<Decoration> decorations;
		private DecorationTypes pocketType;
		private uint pocketSize;
		private uint maxStackSize;

		// ListView Management
		private ObservableCollection<ListViewItem> listViewItems;
		public event DecorationPocketEventHandler AddListViewItem;
		public event DecorationPocketEventHandler UpdateListViewItem;
		public event DecorationPocketEventHandler RemoveListViewItem;

		#endregion

		public DecorationPocket(DecorationInventory inventory, DecorationTypes pocketType, uint pocketSize, uint maxStackSize) {
			this.inventory				= inventory;
			this.pocketType			= pocketType;
			this.pocketSize			= pocketSize;
			this.maxStackSize			= maxStackSize;
			this.decorations			= new List<Decoration>();
			this.listViewItems			= new ObservableCollection<ListViewItem>();
		}

		#region Containment Properties

		public IGameSave GameSave {
			get { return inventory.GameSave; }
		}
		public DecorationInventory Inventory {
			get { return inventory; }
		}

		#endregion

		#region Pocket Properties

		public ObservableCollection<ListViewItem> ListViewItems {
			get { return listViewItems; }
		}
		public DecorationTypes PocketType {
			get { return pocketType; }
		}
		public uint TotalSlots {
			get { return pocketSize; }
		}
		public uint SlotsUsed {
			get { return (uint)decorations.Count; }
		}
		public uint SlotsLeft {
			get { return pocketSize - (uint)decorations.Count; }
		}
		public uint MaxStackSize {
			get { return maxStackSize; }
		}

		#endregion

		#region Decoration Accessors

		public Decoration this[int index] {
			get {
				if (index >= 0 && index < decorations.Count)
					return decorations[index];
				return null;
			}
		}
		public bool ContainsAt(int index) {
			return index >= 0 && index < decorations.Count;
		}
		public uint GetCountOfID(byte id) {
			uint count = 0;
			for (int i = 0; i < decorations.Count; i++) {
				if (decorations[i].ID == id)
					count += decorations[i].Count; // Don't break here, remember the games splits stacks after 99
			}
			return count;
		}
		public int IndexOf(Decoration decoration) {
			return decorations.IndexOf(decoration);
		}

		#endregion

		#region Decoration Management

		public bool HasRoomForDecoration(byte id, uint count) {
			int countLeft = (int)count;
			for (int i = 0; i < decorations.Count && countLeft > 0; i++) {
				if (decorations[i].ID == id) {
					if (decorations[i].Count < maxStackSize) {
						int itemCount = (int)decorations[i].Count;
						if (countLeft - Math.Min(countLeft, (int)maxStackSize - itemCount) <= 0) {
							countLeft -= Math.Min(countLeft, (int)maxStackSize - itemCount);
						}
					}
					else if (maxStackSize == 0) {
						countLeft = 0;
					}
				}
			}
			for (int i = decorations.Count; (i < (int)pocketSize || pocketSize == 0) && countLeft > 0; i++) {
				if (maxStackSize == 0)
					countLeft = 0;
				else
					countLeft -= (int)maxStackSize;
			}
			return countLeft <= 0;
		}
		public bool AddDecoration(byte id, uint count) {
			if (HasRoomForDecoration(id, count)) {
				int countLeft = (int)count;
				for (int i = 0; i < decorations.Count && countLeft > 0; i++) {
					if (decorations[i].ID == id) {
						if (decorations[i].Count < maxStackSize) {
							int itemCount = (int)decorations[i].Count;
							if (countLeft - Math.Min(countLeft, (int)maxStackSize - itemCount) <= 0) {
								inventory.GameSave.IsChanged = true;
								decorations[i].Count += (uint)Math.Min(countLeft, (int)maxStackSize - itemCount);
								countLeft -= Math.Min(countLeft, (int)maxStackSize - itemCount);
								DecorationPocketEventArgs args = new DecorationPocketEventArgs();
								args.Index = i;
								args.Decoration = decorations[i];
								args.PocketType = pocketType;
								OnUpdateListViewItem(args);
							}
						}
						else if (maxStackSize == 0) {
							inventory.GameSave.IsChanged = true;
							decorations[i].Count += (uint)countLeft;
							countLeft = 0;
							DecorationPocketEventArgs args = new DecorationPocketEventArgs();
							args.Index = i;
							args.Decoration = decorations[i];
							args.PocketType = pocketType;
							OnUpdateListViewItem(args);
						}
					}
				}
				for (int i = 0; (i < (int)pocketSize || pocketSize == 0) && countLeft > 0; i++) {
					if (i >= decorations.Count || decorations[i].ID > id) {
						if (maxStackSize == 0) {
							inventory.GameSave.IsChanged = true;
							decorations.Insert(i, new Decoration(id, (uint)countLeft, this));
							DecorationPocketEventArgs args = new DecorationPocketEventArgs();
							args.Index = i;
							args.Decoration = decorations[i];
							args.PocketType = pocketType;
							OnAddListViewItem(args);
							countLeft = 0;
						}
						else {
							inventory.GameSave.IsChanged = true;
							decorations.Insert(i, new Decoration(id, (uint)Math.Min(countLeft, maxStackSize), this));
							DecorationPocketEventArgs args = new DecorationPocketEventArgs();
							args.Index = i;
							args.Decoration = decorations[i];
							args.PocketType = pocketType;
							OnAddListViewItem(args);
							countLeft -= Math.Min(countLeft, (int)maxStackSize);
						}
					}
				}
				return true;
			}
			return false;
		}
		public void TossDecorationAt(int index) {
			inventory.GameSave.IsChanged = true;
			RemoveDecorationAt(index);
		}
		public void TossDecorationAt(int index, uint count) {
			if (count > 0) {
				inventory.GameSave.IsChanged = true;
				if (count < decorations[index].Count) {
					decorations[index].Count -= count;
					DecorationPocketEventArgs args = new DecorationPocketEventArgs();
					args.Index = index;
					args.Decoration = decorations[index];
					args.PocketType = pocketType;
					OnUpdateListViewItem(args);
				}
				else {
					RemoveDecorationAt(index);
				}
			}
		}
		public void Clear() {
			decorations.Clear();
			listViewItems.Clear();
		}

		#endregion

		#region Placed Decoration Management

		public bool AreAllDecorationsOfIDInUse(byte id) {
			return GetCountOfID(id) == inventory.GetNumDecorationsWithIDInUse(id);
		}

		#endregion

		#region ListViewManagement

		private void OnAddListViewItem(DecorationPocketEventArgs e) {
			if (AddListViewItem != null) {
				AddListViewItem(this, e);
			}
		}
		private void OnUpdateListViewItem(DecorationPocketEventArgs e) {
			if (UpdateListViewItem != null) {
				UpdateListViewItem(this, e);
			}
		}
		private void OnRemoveListViewItem(DecorationPocketEventArgs e) {
			if (RemoveListViewItem != null) {
				RemoveListViewItem(this, e);
			}
		}
		public void UpdateListViewItems() {
			for (int i = 0; i < decorations.Count; i++) {
				DecorationPocketEventArgs args = new DecorationPocketEventArgs();
				args.Index = i;
				args.Decoration = decorations[i];
				args.PocketType = pocketType;
				OnUpdateListViewItem(args);
			}
		}
		public void RepopulateListView() {
			listViewItems.Clear();
			for (int i = 0; i < decorations.Count; i++) {
				DecorationPocketEventArgs args = new DecorationPocketEventArgs();
				args.Index = i;
				args.Decoration = decorations[i];
				args.PocketType = pocketType;
				OnAddListViewItem(args);
			}
		}

		#endregion

		#region Private Helpers

		private void RemoveDecorationAt(int index) {
			byte id = decorations[index].ID;
			DecorationUsages usage = inventory.GetDecorationUsage(index, pocketType);
			int usageCount = inventory.GetNumDecorationsWithIDInUse(id);
			uint idCount = GetCountOfID(id);
			if (usageCount > 0) {
				int indexInUse = inventory.GetIndexOfDecorationInUse(index, pocketType);
				decorations.RemoveAt(index);
				DecorationPocketEventArgs args = new DecorationPocketEventArgs();
				args.Index = index;
				args.PocketType = pocketType;
				OnRemoveListViewItem(args);
				if (usage == DecorationUsages.SecretBase && idCount == usageCount)
					inventory.PutAwayDecorationInSecretBaseAt(indexInUse);
				else if (usage == DecorationUsages.Bedroom && idCount == usageCount)
					inventory.PutAwayDecorationInBedroomAt(indexInUse);
				else if (usageCount > 0)
					UpdateListViewItems();
			}
			else {
				decorations.RemoveAt(index);
				DecorationPocketEventArgs args = new DecorationPocketEventArgs();
				args.Index = index;
				args.PocketType = pocketType;
				OnRemoveListViewItem(args);
			}
		}

		#endregion
	}
}
