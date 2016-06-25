using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class NatureData {

		private byte id;
		private string name;
		private StatTypes raisedStat;
		private StatTypes loweredStat;

		public NatureData(DataRow row) {
			this.id				= (byte)(long)row["ID"];
			this.name			= row["Name"] as string;
			this.raisedStat		= GetStatTypeFromString(row["RaisedStat"] as string);
			this.loweredStat	= GetStatTypeFromString(row["LoweredStat"] as string);
		}

		public byte ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public StatTypes RaisedStat {
			get { return raisedStat; }
		}
		public StatTypes LoweredStat {
			get { return loweredStat; }
		}
		public ConditionTypes RaisedCondition {
			get { return (ConditionTypes)raisedStat; }
		}
		public ConditionTypes LoweredCondition {
			get { return (ConditionTypes)loweredStat; }
		}

		public double AttackModifier {
			get {
				if (raisedStat == StatTypes.Attack) return 1.1;
				else if (loweredStat == StatTypes.Attack) return 0.9;
				return 1.0;
			}
		}
		public double DefenseModifier {
			get {
				if (raisedStat == StatTypes.Defense) return 1.1;
				else if (loweredStat == StatTypes.Defense) return 0.9;
				return 1.0;
			}
		}
		public double SpAttackModifier {
			get {
				if (raisedStat == StatTypes.SpAttack) return 1.1;
				else if (loweredStat == StatTypes.SpAttack) return 0.9;
				return 1.0;
			}
		}
		public double SpDefenseModifier {
			get {
				if (raisedStat == StatTypes.SpDefense) return 1.1;
				else if (loweredStat == StatTypes.SpDefense) return 0.9;
				return 1.0;
			}
		}
		public double SpeedModifier {
			get {
				if (raisedStat == StatTypes.Speed) return 1.1;
				else if (loweredStat == StatTypes.Speed) return 0.9;
				return 1.0;
			}
		}

		public StatTypes GetStatTypeFromString(string stat) {
			if (stat != null) {
				if (stat == "ATTACK") return StatTypes.Attack;
				if (stat == "DEFENSE") return StatTypes.Defense;
				if (stat == "SPEED") return StatTypes.Speed;
				if (stat == "SP ATTACK") return StatTypes.SpAttack;
				if (stat == "SP DEFENSE") return StatTypes.SpDefense;
			}

			return StatTypes.None;
		}
	}
}
