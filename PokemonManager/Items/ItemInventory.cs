using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class ItemInventory {

		#region Members

		private Inventory inventory;
		private Dictionary<ItemTypes, ItemPocket> pockets;

		#endregion

		public ItemInventory(Inventory inventory) {
			this.inventory	= inventory;
			this.pockets	= new Dictionary<ItemTypes, ItemPocket>();
		}

		#region Containment Properties

		public Inventory Inventory {
			get { return inventory; }
		}
		public IGameSave GameSave {
			get { return inventory.GameSave; }
		}
		public GameTypes GameType {
			get { return inventory.GameSave.GameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(inventory.GameSave); }
		}
		public Generations Generation {
			get { return inventory.GameSave.Generation; }
		}
		public Platforms Platform {
			get { return inventory.GameSave.Platform; }
		}

		#endregion

		#region Item Pockets

		public ItemPocket this[ItemTypes pocketType] {
			get {
				if (pockets.ContainsKey(pocketType))
					return pockets[pocketType];
				return null;
			}
		}
		public void AddPocket(ItemTypes pocketType, uint pocketSize, uint maxStackSize, bool allowDuplicateStacks, bool ordered) {
			pockets.Add(pocketType, new ItemPocket(this, pocketType, pocketSize, maxStackSize, allowDuplicateStacks, ordered));
		}
		public bool ContainsPocket(ItemTypes pocketType) {
			return pockets.ContainsKey(pocketType);
		}
		public void Clear() {
			foreach (KeyValuePair<ItemTypes, ItemPocket> pocket in pockets) {
				pocket.Value.Clear();
			}
		}

		#endregion
	}
}
