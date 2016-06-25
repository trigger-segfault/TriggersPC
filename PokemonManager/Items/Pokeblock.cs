using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class Pokeblock {

		#region Members

		private PokeblockCase pokeblockCase;
		private PokeblockColors color;
		private byte spicy;
		private byte dry;
		private byte sweet;
		private byte bitter;
		private byte sour;
		private byte feel;
		private byte unknown;

		#endregion

		public Pokeblock(PokeblockCase pokeblockCase, PokeblockColors color, byte spicy, byte dry, byte sweet, byte bitter, byte sour, byte feel, byte unknown) {
			this.pokeblockCase	= pokeblockCase;
			this.color			= color;
			this.spicy			= spicy;
			this.dry			= dry;
			this.sweet			= sweet;
			this.bitter			= bitter;
			this.sour			= sour;
			this.feel			= feel;
			this.unknown		= unknown;
		}

		#region Properties

		public PokeblockCase PokeblockCase {
			get { return pokeblockCase; }
		}
		public PokeblockColors Color {
			get { return color; }
		}
		public byte Level {
			get { return Math.Max(spicy, Math.Max(dry, Math.Max(sweet, Math.Max(bitter, sour)))); }
		}
		public byte Spicyness {
			get { return spicy; }
		}
		public byte Dryness {
			get { return dry; }
		}
		public byte Sweetness {
			get { return sweet; }
		}
		public byte Bitterness {
			get { return bitter; }
		}
		public byte Sourness {
			get { return sour; }
		}
		public byte Feel {
			get { return feel; }
		}
		// Usually 3
		public byte Unknown {
			get { return unknown; }
		}

		#endregion
	}
}
