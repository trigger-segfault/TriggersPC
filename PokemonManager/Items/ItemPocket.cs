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
	public class ItemPocketEventArgs : EventArgs {
		public int Index { get; set; }
		public int OldIndex { get; set; }
		public int NewIndex { get; set; }
		public Item Item { get; set; }
		public ItemTypes PocketType { get; set; }
	}

	public delegate void ItemPocketEventHandler(object sender, ItemPocketEventArgs e);

	public class ItemPocket {

		private static Dictionary<ItemTypes, Dictionary<int, int>> ItemOrders = new Dictionary<ItemTypes, Dictionary<int, int>>{
			{ItemTypes.PC, new Dictionary<int, int>{{3, 0}, {7, 1}, {2, 2}, {0, 3}, {1, 4}, {9, 5}, {4, 6}, {5, 7}, {6, 8}}},
			{ItemTypes.Items, new Dictionary<int, int>{{0, 0}, {1, 1}, {2, 2}, {3, 3}, {7, 4}, {9, 5}}},
			{ItemTypes.Misc, new Dictionary<int, int>{{7, 0}, {1, 1}, {9, 3}}}
		};

		#region Members

		private ItemInventory inventory;
		private List<Item> items;
		private ItemTypes pocketType;
		private uint pocketSize;
		private uint maxStackSize;
		private bool allowDuplicateStacks;
		private bool ordered;

		// ListView Management
		private ObservableCollection<ListViewItem> listViewItems;
		public event ItemPocketEventHandler AddListViewItem;
		public event ItemPocketEventHandler UpdateListViewItem;
		public event ItemPocketEventHandler RemoveListViewItem;
		public event ItemPocketEventHandler MoveListViewItem;

		#endregion

		public ItemPocket(ItemInventory inventory, ItemTypes pocketType, uint pocketSize, uint maxStackSize, bool allowDuplicateStacks, bool ordered) {
			this.inventory				= inventory;
			this.items					= new List<Item>();
			this.pocketType				= pocketType;
			this.pocketSize				= pocketSize;
			this.maxStackSize			= maxStackSize;
			this.allowDuplicateStacks	= allowDuplicateStacks;
			this.ordered				= ordered;

			this.listViewItems			= new ObservableCollection<ListViewItem>();
		}

		#region Containment Properties

		public ItemInventory Inventory {
			get { return inventory; }
		}
		public IGameSave GameSave {
			get { return inventory.Inventory.GameSave; }
		}

		#endregion

		#region Pocket Properties

		public ObservableCollection<ListViewItem> ListViewItems {
			get { return listViewItems; }
		}
		public ItemTypes PocketType {
			get { return pocketType; }
		}
		public uint TotalSlots {
			get { return pocketSize; }
		}
		public uint SlotsUsed {
			get { return (uint)items.Count; }
		}
		public uint SlotsLeft {
			get { return pocketSize - (uint)items.Count; }
		}
		public uint MaxStackSize {
			get { return maxStackSize; }
		}
		public bool AllowDuplicateStacks {
			get { return allowDuplicateStacks; }
		}
		public bool IsOrdered {
			get { return ordered; }
			set {
				if (!ordered && value) {
					ordered = value;
					Sort();
				}
				else {
					ordered = value;
				}
			}
		}

		#endregion

		#region Item Accessors

		public Item this[int index] {
			get {
				if (index >= 0 && index < items.Count)
					return items[index];
				return null;
			}
		}
		public bool ContainsAt(int index) {
			return index >= 0 && index < items.Count;
		}
		public uint GetCountOfID(ushort id) {
			uint count = 0;
			for (int i = 0; i < items.Count; i++) {
				if (items[i].ID == id)
					count += items[i].Count; // Don't break here, remember the games splits stacks after 99 or 999
			}
			return count;
		}
		public int IndexOf(Item item) {
			return items.IndexOf(item);
		}

		#endregion

		#region Item Manipulation

		private int GetOrder(ItemData itemData) {
			if (ItemOrders.ContainsKey(pocketType)) {
				int index = itemData.Order / 100;
				if (ItemOrders[pocketType].ContainsKey(index)) {
					return (itemData.Order % 100) + (ItemOrders[pocketType][index] * 100);
				}
			}
			return itemData.Order;
		}

		public bool HasRoomForItem(ushort id, uint count) {
			ItemData itemData = ItemDatabase.GetItemFromID(id);
			if (pocketType != ItemTypes.PC && itemData.SubPocketType != pocketType && inventory.ContainsPocket(itemData.SubPocketType)) {
				return inventory[itemData.SubPocketType].HasRoomForItem(id, count);
			}
			int countLeft = (int)count;
			bool hasStackAlready = false;
			for (int i = 0; i < items.Count && countLeft > 0; i++) {
				if (items[i].ID == id) {
					hasStackAlready = true;
					if (items[i].Count < maxStackSize) {
						int itemCount = (int)items[i].Count;
						if (countLeft - Math.Min(countLeft, (int)maxStackSize - itemCount) <= 0 || allowDuplicateStacks) {
							countLeft -= Math.Min(countLeft, (int)maxStackSize - itemCount);
						}
					}
					else if (maxStackSize == 0) {
						countLeft = 0;
					}
				}
			}
			for (int i = items.Count; (i < (int)pocketSize || pocketSize == 0) && countLeft > 0 && (allowDuplicateStacks || !hasStackAlready); i++) {
				hasStackAlready = true;
				if (maxStackSize == 0)
					countLeft = 0;
				else
					countLeft -= (int)maxStackSize;
			}
			return countLeft <= 0;
		}
		public bool AddItem(ushort id, uint count) {
			ItemData itemData = ItemDatabase.GetItemFromID(id);
			if (pocketType != ItemTypes.PC && itemData.SubPocketType != pocketType && inventory.ContainsPocket(itemData.SubPocketType)) {
				return inventory[itemData.SubPocketType].AddItem(id, count);
			}
			if (HasRoomForItem(id, count)) {
				int countLeft = (int)count;
				bool hasStackAlready = false;
				for (int i = 0; i < items.Count && countLeft > 0; i++) {
					if (items[i].ID == id) {
						hasStackAlready = true;
						if (items[i].Count < maxStackSize) {
							int itemCount = (int)items[i].Count;
							if (countLeft - Math.Min(countLeft, (int)maxStackSize - itemCount) <= 0 || allowDuplicateStacks) {
								inventory.GameSave.IsChanged = true;
								items[i].Count += (uint)Math.Min(countLeft, (int)maxStackSize - itemCount);
								countLeft -= Math.Min(countLeft, (int)maxStackSize - itemCount);
								ItemPocketEventArgs args = new ItemPocketEventArgs();
								args.Index = i;
								args.Item = items[i];
								args.PocketType = pocketType;
								OnUpdateListViewItem(args);
							}
						}
						else if (maxStackSize == 0) {
							inventory.GameSave.IsChanged = true;
							items[i].Count += (uint)countLeft;
							countLeft = 0;
							ItemPocketEventArgs args = new ItemPocketEventArgs();
							args.Index = i;
							args.Item = items[i];
							args.PocketType = pocketType;
							OnUpdateListViewItem(args);
						}
					}
				}
				if (ordered) {
					for (int i = 0; (i < (int)pocketSize || pocketSize == 0) && countLeft > 0 && (allowDuplicateStacks || !hasStackAlready); i++) {
						if (i >= items.Count || GetOrder(items[i].ItemData) > GetOrder(itemData)) {
							hasStackAlready = true;
							if (maxStackSize == 0) {
								inventory.GameSave.IsChanged = true;
								items.Insert(i, new Item(id, (uint)countLeft, this));
								ItemPocketEventArgs args = new ItemPocketEventArgs();
								args.Index = i;
								args.Item = items[i];
								args.PocketType = pocketType;
								OnAddListViewItem(args);
								countLeft = 0;
							}
							else {
								inventory.GameSave.IsChanged = true;
								items.Insert(i, new Item(id, (uint)Math.Min(countLeft, maxStackSize), this));
								ItemPocketEventArgs args = new ItemPocketEventArgs();
								args.Index = i;
								args.Item = items[i];
								args.PocketType = pocketType;
								OnAddListViewItem(args);
								countLeft -= Math.Min(countLeft, (int)maxStackSize);
							}
						}
					}
				}
				else {
					for (int i = items.Count; (i < (int)pocketSize || pocketSize == 0) && countLeft > 0 && (allowDuplicateStacks || !hasStackAlready); i++) {
						hasStackAlready = true;
						if (maxStackSize == 0) {
							inventory.GameSave.IsChanged = true;
							items.Add(new Item(id, (uint)countLeft, this));
							ItemPocketEventArgs args = new ItemPocketEventArgs();
							args.Index = i;
							args.Item = items[i];
							args.PocketType = pocketType;
							OnAddListViewItem(args);
							countLeft = 0;
						}
						else {
							inventory.GameSave.IsChanged = true;
							items.Add(new Item(id, (uint)Math.Min(countLeft, (int)maxStackSize), this));
							ItemPocketEventArgs args = new ItemPocketEventArgs();
							args.Index = i;
							args.Item = items[i];
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
		public void MoveItem(int oldIndex, int newIndex) {
			if (!ordered) {
				inventory.GameSave.IsChanged = true;
				Item item = items[oldIndex];
				items.RemoveAt(oldIndex);
				items.Insert(newIndex, item);

				ItemPocketEventArgs args = new ItemPocketEventArgs();
				args.OldIndex = oldIndex;
				args.NewIndex = newIndex;
				args.PocketType = pocketType;
				OnMoveListViewItem(args);
			}
		}
		public void TossItemAt(int index) {
			inventory.GameSave.IsChanged = true;
			items.RemoveAt(index);
			ItemPocketEventArgs args = new ItemPocketEventArgs();
			args.Index = index;
			args.PocketType = pocketType;
			OnRemoveListViewItem(args);
		}
		public void TossItemAt(int index, uint count) {
			if (count > 0) {
				inventory.GameSave.IsChanged = true;
				if (count < items[index].Count) {
					items[index].Count -= count;
					ItemPocketEventArgs args = new ItemPocketEventArgs();
					args.Index = index;
					args.Item = items[index];
					args.PocketType = pocketType;
					OnUpdateListViewItem(args);
				}
				else {
					items.RemoveAt(index);
					ItemPocketEventArgs args = new ItemPocketEventArgs();
					args.Index = index;
					args.PocketType = pocketType;
					OnRemoveListViewItem(args);
				}
			}
		}
		public void Sort() {
			List<Item> newItems = new List<Item>();
			foreach (Item item in items) {
				newItems.Add(item);
			}
			Clear();
			bool oldOrdered = ordered;
			ordered = true;
			foreach (Item item in newItems) {
				AddItem(item.ID, item.Count);
			}
			ordered = oldOrdered;

		}
		public void Clear() {
			items.Clear();
			listViewItems.Clear();
		}

		#endregion

		#region ListView Methods

		private void OnAddListViewItem(ItemPocketEventArgs e) {
			if (AddListViewItem != null) {
				AddListViewItem(this, e);
			}
		}
		private void OnUpdateListViewItem(ItemPocketEventArgs e) {
			if (UpdateListViewItem != null) {
				UpdateListViewItem(this, e);
			}
		}
		private void OnRemoveListViewItem(ItemPocketEventArgs e) {
			if (RemoveListViewItem != null) {
				RemoveListViewItem(this, e);
			}
		}
		private void OnMoveListViewItem(ItemPocketEventArgs e) {
			if (MoveListViewItem != null) {
				MoveListViewItem(this, e);
			}
		}
		public void RepopulateListView() {
			listViewItems.Clear();
			for (int i = 0; i < items.Count; i++) {
				ItemPocketEventArgs args = new ItemPocketEventArgs();
				args.Index = i;
				args.Item = items[i];
				args.PocketType = pocketType;
				OnAddListViewItem(args);
			}
		}

		#endregion
	}
}
