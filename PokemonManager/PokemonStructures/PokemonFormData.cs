using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class PokemonFormData {

		private byte id;
		private string name;
		private byte hp;
		private byte attack;
		private byte defense;
		private byte spAttack;
		private byte spDefense;
		private byte speed;
		private List<LearnableMove> learnableMoves;

		public PokemonFormData(DataRow row) {
			this.id				= (byte)(long)row["ID"];
			this.name			= row["Name"] as string;

			this.hp				= (byte)(long)row["HP"];
			this.attack			= (byte)(long)row["Attack"];
			this.defense		= (byte)(long)row["Defense"];
			this.spAttack		= (byte)(long)row["SpAttack"];
			this.spDefense		= (byte)(long)row["SpDefense"];
			this.speed			= (byte)(long)row["Speed"];

			this.learnableMoves	= new List<LearnableMove>();
		}

		public byte ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public byte HP {
			get { return hp; }
		}
		public byte Attack {
			get { return attack; }
		}
		public byte Defense {
			get { return defense; }
		}
		public byte SpAttack {
			get { return spAttack; }
		}
		public byte SpDefense {
			get { return spDefense; }
		}
		public byte Speed {
			get { return speed; }
		}

		public void AddLearnableMove(LearnableMove move) {
			learnableMoves.Add(move);
		}
		public int NumLearnableMoves {
			get { return learnableMoves.Count; }
		}
		public LearnableMove GetLearnableMove(int index) {
			return learnableMoves[index];
		}
	}
}
