using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public struct Move {

		private ushort id;
		private byte pp;
		private byte ppUpsUsed;

		public Move(ushort id) {
			this.id = id;
			this.ppUpsUsed = 0;
			this.pp = PokemonDatabase.GetMoveFromID(id).PP;
		}

		public Move(ushort id, byte pp, byte ppUpsUsed) {
			this.id			= id;
			this.pp			= pp;
			this.ppUpsUsed	= ppUpsUsed;
		}

		public MoveData MoveData {
			get { return PokemonDatabase.GetMoveFromID(id); }
		}

		public ushort ID {
			get { return id; }
			set {
				id = value;
				if (PP > TotalPP)
					PP = TotalPP;
			}
		}
		public byte PP {
			get { return pp; }
			set { pp = Math.Max((byte)0, Math.Min(TotalPP, value)); }
		}
		public byte TotalPP {
			get {
				int movePP = PokemonDatabase.GetMoveFromID(id).PP;
				movePP += movePP / 5 * (int)ppUpsUsed;
				return (byte)movePP;
			}
		}
		public byte PPUpsUsed {
			get { return ppUpsUsed; }
			set { ppUpsUsed = Math.Min((byte)3, value); }
		}
	}
}
