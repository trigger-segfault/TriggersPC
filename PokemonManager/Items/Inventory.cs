using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PokemonManager.Items {
	public class Inventory {

		#region Memebers

		private IGameSave gameSave;
		private ItemInventory itemInventory;
		private DecorationInventory decorationInventory;
		private PokeblockCase pokeblockCase;

		#endregion

		public Inventory(IGameSave gameSave) {
			this.gameSave				= gameSave;
			this.itemInventory			= null;
			this.decorationInventory	= null;
			this.pokeblockCase			= null;
		}

		#region Containment Properties

		public IGameSave GameSave {
			get { return gameSave; }
		}
		public GameTypes GameType {
			get { return gameSave.GameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(gameSave); }
		}
		public Generations Generation {
			get { return gameSave.Generation; }
		}
		public Platforms Platform {
			get { return gameSave.Platform; }
		}

		#endregion

		#region Inventories Accessors

		public ItemInventory Items {
			get { return itemInventory; }
		}
		public DecorationInventory Decorations {
			get { return decorationInventory; }
		}
		public PokeblockCase Pokeblocks {
			get { return pokeblockCase; }
		}

		#endregion

		#region Inventory Management

		public void AddItemInventory() {
			if (itemInventory != null)
				throw new Exception("Cannot add Item Inventory to inventory, one already exists");
			else
				itemInventory = new ItemInventory(this);
		}
		public void AddDecorationInventory() {
			if (decorationInventory != null)
				throw new Exception("Cannot add Decoration Inventory to inventory, one already exists");
			else
				decorationInventory = new DecorationInventory(this);
		}
		public void AddPokeblockCase(uint caseSize) {
			if (pokeblockCase != null)
				throw new Exception("Cannot add Pokeblock Case to inventory, one already exists");
			else
				pokeblockCase = new PokeblockCase(this, caseSize);
		}
		public void Clear() {
			if (itemInventory != null)
				itemInventory.Clear();
			if (decorationInventory != null)
				decorationInventory.Clear();
			if (pokeblockCase != null)
				pokeblockCase.Clear();
		}

		#endregion
	}
}
