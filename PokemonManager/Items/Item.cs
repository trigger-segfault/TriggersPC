using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class Item {

		#region Members

		private ItemPocket pocket;
		private ushort id;
		private uint count;

		#endregion

		public Item(ushort id, uint count, ItemPocket pocket) {
			this.pocket		= pocket;
			this.id			= id;
			this.count		= count;
		}

		#region Properties

		public ItemData ItemData {
			get { return ItemDatabase.GetItemFromID(id) ?? ItemDatabase.GetItemFromID(0); }
		}
		public ItemPocket Pocket {
			get { return pocket; }
			set { pocket = value; }
		}
		public ushort ID {
			get { return id; }
		}
		public uint Count {
			get { return count; }
			set {
				pocket.Inventory.GameSave.IsChanged = true;
				count = value;
			}
		}

		public override string ToString() {
			return "Item: " + ItemData.Name + "  x" + Count.ToString();
		}

		#endregion
	}
}
