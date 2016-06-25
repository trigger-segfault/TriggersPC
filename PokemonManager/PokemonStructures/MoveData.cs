using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class MoveData {

		private ushort id;
		private string name;
		private string description;
		private PokemonTypes type;
		private byte power;
		private byte accuracy;
		private byte pp;
		private MoveCategories category;

		private ConditionTypes conditionType;
		private string contestDescription;
		private byte appeal;
		private byte jam;

		public MoveData(DataRow row) {
			this.id					= (ushort)(long)row["ID"];
			this.name				= row["Name"] as string;
			this.description		= row["Description"] as string;
			this.type				= GetPokemonTypeFromString(row["Type"] as string);
			this.power				= (byte)(long)row["Power"];
			this.accuracy			= (byte)(long)row["Accuracy"];
			this.pp					= (byte)(long)row["PP"];
			this.category			= GetMoveCategoryFromString(row["Category"] as string);

			this.conditionType		= GetConditionTypeFromString(row["ConditionType"] as string);
			this.contestDescription	= row["ContestDescription"] as string;
			this.appeal				= (byte)(long)row["Appeal"];
			this.jam				= (byte)(long)row["Jam"];
		}

		public ushort ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public string Description {
			get { return description; }
		}
		public PokemonTypes Type {
			get { return type; }
		}
		public byte Power {
			get { return power; }
		}
		public byte Accuracy {
			get { return accuracy; }
		}
		public byte PP {
			get { return pp; }
		}
		public MoveCategories Category {
			get { return category; }
		}

		public ConditionTypes ConditionType {
			get { return conditionType; }
		}
		public string ContestDescription {
			get { return contestDescription; }
		}
		public byte Appeal {
			get { return appeal; }
		}
		public byte Jam {
			get { return jam; }
		}

		private MoveCategories GetMoveCategoryFromString(string category) {
			if (category == "PHYSICAL") return MoveCategories.Physical;
			if (category == "SPECIAL") return MoveCategories.Special;
			if (category == "STATUS") return MoveCategories.Status;

			return (MoveCategories)byte.MaxValue;
		}

		private PokemonTypes GetPokemonTypeFromString(string type) {
			if (type == "NORMAL") return PokemonTypes.Normal;
			if (type == "FIGHTING") return PokemonTypes.Fighting;
			if (type == "FLYING") return PokemonTypes.Flying;
			if (type == "POISON") return PokemonTypes.Poison;
			if (type == "GROUND") return PokemonTypes.Ground;
			if (type == "ROCK") return PokemonTypes.Rock;
			if (type == "BUG") return PokemonTypes.Bug;
			if (type == "GHOST") return PokemonTypes.Ghost;
			if (type == "STEEL") return PokemonTypes.Steel;
			if (type == "FIRE") return PokemonTypes.Fire;
			if (type == "WATER") return PokemonTypes.Water;
			if (type == "GRASS") return PokemonTypes.Grass;
			if (type == "ELECTRIC") return PokemonTypes.Electric;
			if (type == "PSYCHIC") return PokemonTypes.Psychic;
			if (type == "ICE") return PokemonTypes.Ice;
			if (type == "DRAGON") return PokemonTypes.Dragon;
			if (type == "DARK") return PokemonTypes.Dark;
			if (type == "SHADOW") return PokemonTypes.Shadow;

			return PokemonTypes.None;
		}

		private ConditionTypes GetConditionTypeFromString(string type) {
			if (type == "COOL") return ConditionTypes.Cool;
			if (type == "BEAUTY") return ConditionTypes.Beauty;
			if (type == "CUTE") return ConditionTypes.Cute;
			if (type == "SMART") return ConditionTypes.Smart;
			if (type == "TOUGH") return ConditionTypes.Tough;

			return ConditionTypes.None;
		}
	}
}
