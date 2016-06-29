
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class PokemonData {

		private ushort id;
		private ushort dexID;
		private string name;
		private string description;
		private PokemonTypes type1;
		private PokemonTypes type2;
		private byte ability1ID;
		private byte ability2ID;
		private EggGroups eggGroup1;
		private EggGroups eggGroup2;
		private ExperienceGroups experienceGroup;
		private byte hp;
		private byte attack;
		private byte defense;
		private byte spAttack;
		private byte spDefense;
		private byte speed;
		private byte genderRatio;
		private string pokedexEntry;
		private ushort familyDexID;

		private Dictionary<byte, PokemonFormData> forms;
		private List<EvolutionData> evolutions;
		private List<LearnableMove> learnableMoves;

		public PokemonData(DataRow row) {
			this.id				= (ushort)(long)row["ID"];
			this.dexID			= (ushort)(long)row["DexID"];
			this.name			= row["Name"] as string;
			this.pokedexEntry	= row["PokedexEntry"] as string ?? "";

			this.type1			= GetPokemonTypeFromString(row["Type1"] as string);
			this.type2			= (row["Type2"] as string == null ? this.type1 : GetPokemonTypeFromString(row["Type2"] as string));

			this.ability1ID		= PokemonDatabase.GetAbilityIDFromString(row["Ability1"] as string);
			this.ability2ID		= (row["Ability2"] as string == null ? this.ability1ID : PokemonDatabase.GetAbilityIDFromString(row["Ability2"] as string));

			this.eggGroup1		= GetEggGroupFromString(row["EggGroup1"] as string);
			this.eggGroup2		= GetEggGroupFromString(row["EggGroup2"] as string);

			this.experienceGroup	= GetExperienceGroupFromString(row["ExperienceGroup"] as string);

			this.hp				= (byte)(long)row["HP"];
			this.attack			= (byte)(long)row["Attack"];
			this.defense		= (byte)(long)row["Defense"];
			this.spAttack		= (byte)(long)row["SpAttack"];
			this.spDefense		= (byte)(long)row["SpDefense"];
			this.speed			= (byte)(long)row["Speed"];
			this.genderRatio	= (byte)(long)row["GenderRatio"];

			this.forms			= null;
			this.evolutions		= null;

			this.learnableMoves	= new List<LearnableMove>();

			this.familyDexID	= this.dexID;
		}

		private ExperienceGroups GetExperienceGroupFromString(string group) {
			if (group == "FAST") return ExperienceGroups.Fast;
			if (group == "MEDIUM FAST") return ExperienceGroups.MediumFast;
			if (group == "MEDIUM SLOW") return ExperienceGroups.MediumSlow;
			if (group == "SLOW") return ExperienceGroups.Slow;
			if (group == "FLUCTUATING") return ExperienceGroups.Fluctuating;
			if (group == "ERRATIC") return ExperienceGroups.Erratic;

			return (ExperienceGroups)byte.MaxValue;
		}

		private EggGroups GetEggGroupFromString(string group) {
			if (group == "FIELD") return EggGroups.Field;
			if (group == "MONSTER") return EggGroups.Monster;
			if (group == "GRASS") return EggGroups.Grass;
			if (group == "BUG") return EggGroups.Bug;
			if (group == "FLYING") return EggGroups.Flying;
			if (group == "MINERAL") return EggGroups.Mineral;
			if (group == "WATER 1") return EggGroups.Water1;
			if (group == "WATER 2") return EggGroups.Water2;
			if (group == "WATER 3") return EggGroups.Water3;
			if (group == "FAIRY") return EggGroups.Fairy;
			if (group == "DRAGON") return EggGroups.Dragon;
			if (group == "HUMANOID") return EggGroups.Humanoid;
			if (group == "AMORPHOUS") return EggGroups.Amorphous;
			if (group == "DITTO ONLY") return EggGroups.DittoOnly;
			if (group == "UNDISCOVERED") return EggGroups.Undiscovered;
			if (group == "DITTO") return EggGroups.Ditto;
			if (group == null) return EggGroups.None;
			throw new Exception("Invalid EggGroup in " + name + " Pokemon Entry");
		}

		public byte GenderRatio {
			get { return genderRatio; }
		}

		public ushort ID {
			get { return id; }
		}
		public ushort DexID {
			get { return dexID; }
		}

		public string Name {
			get { return name; }
		}

		public PokemonTypes Type1 {
			get { return type1; }
		}
		public PokemonTypes Type2 {
			get { return type2; }
		}
		public bool HasTwoTypes {
			get { return type1 != type2; }
		}
		public byte Ability1ID {
			get { return ability1ID; }
		}
		public byte Ability2ID {
			get { return ability2ID; }
		}
		public bool HasTwoAbilities {
			get { return ability1ID != ability2ID; }
		}
		public EggGroups EggGroup1 {
			get { return eggGroup1; }
		}
		public EggGroups EggGroup2 {
			get { return eggGroup2; }
		}
		public bool HasTwoEggGroups {
			get { return eggGroup2 != EggGroups.None; }
		}

		public ExperienceGroups ExperienceGroup {
			get { return experienceGroup; }
		}

		public string PokedexEntry {
			get { return pokedexEntry; }
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

		public ushort FamilyDexID {
			get { return familyDexID; }
			set { familyDexID = value; }
		}
		public PokemonData FamilyPokemonData {
			get { return PokemonDatabase.GetPokemonFromDexID(familyDexID); }
		}

		public bool HasEvolutions {
			get { return evolutions != null; }
		}
		public int NumEvolutions {
			get { return (evolutions != null ? evolutions.Count : 0); }
		}
		public EvolutionData GetEvolution(int index) {
			return evolutions[index];
		}
		public void AddEvolution(EvolutionData evolution) {
			if (evolutions == null)
				evolutions = new List<EvolutionData>();
			evolutions.Add(evolution);
		}

		public bool HasForms {
			get { return forms != null; }
		}
		public int NumForms {
			get { return (forms != null ? forms.Count : 0); }
		}
		public PokemonFormData GetForm(byte id) {
			return forms[id];
		}
		public void AddForm(PokemonFormData form) {
			if (forms == null)
				forms = new Dictionary<byte, PokemonFormData>();
			forms.Add(form.ID, form);
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
	}
}
