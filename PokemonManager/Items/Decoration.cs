using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class Decoration {

		#region Members

		private DecorationPocket pocket;
		private byte id;
		private uint count;

		#endregion

		public Decoration(byte id, uint count, DecorationPocket container) {
			this.pocket	= container;
			this.id			= id;
			this.count		= count;
		}

		#region Properties

		public DecorationData DecorationData {
			get { return ItemDatabase.GetDecorationFromID(id); }
		}
		public DecorationPocket Container {
			get { return pocket; }
		}
		public byte ID {
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
			return "Decoration: " + DecorationData.Name + "  x" + Count.ToString();
		}

		#endregion
	}
}
