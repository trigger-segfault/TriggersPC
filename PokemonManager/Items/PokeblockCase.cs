using PokemonManager.Game.FileStructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PokemonManager.Items {

	public class PokeblockCaseEventArgs : EventArgs {
		public int Index { get; set; }
		public int OldIndex { get; set; }
		public int NewIndex { get; set; }
		public Pokeblock Pokeblock { get; set; }
	}
	public delegate void PokeblockCaseEventHandler(object sender, PokeblockCaseEventArgs e);

	public class PokeblockCase {

		#region Members

		private Inventory inventory;
		private List<Pokeblock> pokeblocks;
		private uint caseSize;

		// ListView Management
		private ObservableCollection<ListViewItem> listViewItems;
		public event PokeblockCaseEventHandler AddListViewItem;
		public event PokeblockCaseEventHandler RemoveListViewItem;
		public event PokeblockCaseEventHandler MoveListViewItem;

		#endregion

		public PokeblockCase(Inventory inventory, uint caseSize) {
			this.inventory	= inventory;
			this.caseSize	= caseSize;
			this.pokeblocks	= new List<Pokeblock>();
			this.listViewItems = new ObservableCollection<ListViewItem>();
		}

		#region Basic Properties

		public Inventory Inventory {
			get { return inventory; }
		}
		public IGameSave GameSave {
			get { return inventory.GameSave; }
		}

		#endregion

		#region Pokeblock Properties

		public ObservableCollection<ListViewItem> ListViewItems {
			get { return listViewItems; }
		}
		public uint TotalSlots {
			get { return caseSize; }
		}
		public uint SlotsUsed {
			get { return (uint)pokeblocks.Count; }
		}
		public uint SlotsLeft {
			get { return caseSize - (uint)pokeblocks.Count; }
		}

		#endregion

		public bool HasRoomForPokeblock {
			get { return caseSize == 0 || pokeblocks.Count < (int)caseSize; }
		}

		public void Clear() {
			pokeblocks.Clear();
			listViewItems.Clear();
		}

		public Pokeblock this[int index] {
			get {
				if (index == -1)
					return null;
				return pokeblocks[index];
			}
		}

		public Pokeblock GetPokeblockAt(int index) {
			if (index == -1)
				return null;
			return pokeblocks[index];
		}
		public int IndexOf(Pokeblock pokeblock) {
			return pokeblocks.IndexOf(pokeblock);
		}

		public void TossPokeblockAt(int index) {
			inventory.GameSave.IsChanged = true;
			pokeblocks.RemoveAt(index);

			PokeblockCaseEventArgs args = new PokeblockCaseEventArgs();
			args.Index = index;
			OnRemoveListViewItem(args);
		}

		public void AddPokeblock(PokeblockColors color, byte spicy, byte dry, byte sweet, byte bitter, byte sour, byte feel, byte unknown) {
			if (caseSize == 0 || pokeblocks.Count < caseSize) {
				inventory.GameSave.IsChanged = true;
				pokeblocks.Add(new Pokeblock(this, color, spicy, dry, sweet, bitter, sour, feel, unknown));

				PokeblockCaseEventArgs args = new PokeblockCaseEventArgs();
				args.Index = pokeblocks.Count - 1;
				args.Pokeblock = pokeblocks[pokeblocks.Count - 1];
				OnAddListViewItem(args);
			}
		}

		public void AddPokeblock(Pokeblock pokeblock) {
			if (caseSize == 0 || pokeblocks.Count < caseSize) {
				inventory.GameSave.IsChanged = true;
				pokeblocks.Add(new Pokeblock(this, pokeblock.Color, pokeblock.Spicyness, pokeblock.Dryness, pokeblock.Sweetness, pokeblock.Bitterness, pokeblock.Sourness, pokeblock.Feel, pokeblock.Unknown));

				PokeblockCaseEventArgs args = new PokeblockCaseEventArgs();
				args.Index = pokeblocks.Count - 1;
				args.Pokeblock = pokeblocks[pokeblocks.Count - 1];
				OnAddListViewItem(args);
			}
		}

		public void MovePokeblock(int oldIndex, int newIndex) {
			inventory.GameSave.IsChanged = true;
			Pokeblock pokeblock = pokeblocks[oldIndex];
			pokeblocks.RemoveAt(oldIndex);
			pokeblocks.Insert(newIndex, pokeblock);

			PokeblockCaseEventArgs args = new PokeblockCaseEventArgs();
			args.OldIndex = oldIndex;
			args.NewIndex = newIndex;
			args.Pokeblock = pokeblock;
			OnMoveListViewItem(args);
		}


		private void OnAddListViewItem(PokeblockCaseEventArgs e) {
			if (AddListViewItem != null) {
				AddListViewItem(this, e);
			}
		}
		private void OnRemoveListViewItem(PokeblockCaseEventArgs e) {
			if (RemoveListViewItem != null) {
				RemoveListViewItem(this, e);
			}
		}
		private void OnMoveListViewItem(PokeblockCaseEventArgs e) {
			if (MoveListViewItem != null) {
				MoveListViewItem(this, e);
			}
		}

		public void RepopulateListView() {
			listViewItems.Clear();
			for (int i = 0; i < pokeblocks.Count; i++) {
				PokeblockCaseEventArgs args = new PokeblockCaseEventArgs();
				args.Index = i;
				args.Pokeblock = pokeblocks[i];
				OnAddListViewItem(args);
			}
		}
	}
}
