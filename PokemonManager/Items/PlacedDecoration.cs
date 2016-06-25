using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class PlacedDecoration {

		public byte ID { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }

		public PlacedDecoration(byte id, byte x, byte y) {
			this.ID		= id;
			this.X		= x;
			this.Y		= y;
		}
		public PlacedDecoration(PlacedDecoration copy) {
			this.ID		= copy.ID;
			this.X		= copy.X;
			this.Y		= copy.Y;
		}

		public DecorationData DecorationData {
			get { return ItemDatabase.GetDecorationFromID(ID); }
		}

		public override string ToString() {
			return DecorationData.Name + " (" + X + "," + Y + ")";
		}
	}
}
