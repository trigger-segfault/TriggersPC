using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class SharedSecretBase : SecretBase {

		#region Members

		private SecretBaseManager secretBaseManager;
		private byte[] raw;
		private List<PlacedDecoration> placedDecorations;
		private List<GBAPokemon> pokemonTeam;

		#endregion

		public SharedSecretBase(byte locationID, SecretBaseManager secretBaseManager) {
			this.secretBaseManager = secretBaseManager;
			this.raw = new byte[160];
			this.placedDecorations = new List<PlacedDecoration>();
			this.pokemonTeam = new List<GBAPokemon>();

			this.LocationID = locationID;
			this.TrainerName = PokeManager.ManagerGameSave.TrainerName;
			this.TrainerGender = PokeManager.ManagerGameSave.TrainerGender;
			this.TrainerID = PokeManager.ManagerGameSave.TrainerID;
			this.SecretID = PokeManager.ManagerGameSave.SecretID;
			this.Language = Languages.English;
		}
		public SharedSecretBase(SharedSecretBase copy, SecretBaseManager secretBaseManager) :
			this(ByteHelper.SubByteArray(0, copy.GetFinalData(), copy.Raw.Length), secretBaseManager) {

			Genders gender = TrainerGender;
			Flags = 0;
			TrainerGender = gender;
		}
		public SharedSecretBase(PlayerSecretBase copy, SecretBaseManager secretBaseManager)
			: this(copy.LocationID, secretBaseManager) {

			TrainerName = copy.TrainerName;
			TrainerGender = copy.TrainerGender;
			TrainerID = copy.TrainerID;
			SecretID = copy.SecretID;
			Language = copy.Language;
			foreach (GBAPokemon pokemon in copy.PokemonTeam) {
				pokemonTeam.Add((GBAPokemon)pokemon.Clone());
			}

			foreach (PlacedDecoration decoration in copy.PlacedDecorations) {
				placedDecorations.Add(new PlacedDecoration(decoration));
			}
		}

		public SharedSecretBase(byte[] data, SecretBaseManager secretBaseManager) {
			if (data.Length != 160)
				throw new Exception("Shared Secret Base must be 160 bytes in length");
			this.secretBaseManager = secretBaseManager;
			this.raw = data;
			this.placedDecorations = new List<PlacedDecoration>();
			this.pokemonTeam = new List<GBAPokemon>();

			for (int i = 0; i < 16; i++) {
				byte id = raw[0x12 + i];
				if (id != 0) {
					byte x = ByteHelper.BitsToByte(raw, 0x22 + i, 4, 4);
					byte y = ByteHelper.BitsToByte(raw, 0x22 + i, 0, 4);
					placedDecorations.Add(new PlacedDecoration(id, x, y));
				}
			}

			for (int i = 0; i < 6; i++) {
				ushort species = LittleEndian.ToUInt16(raw, 0x7C + i * 2);

				if (species != 0 && PokemonDatabase.GetPokemonFromID(species) != null) {
					uint personality = LittleEndian.ToUInt32(raw, 0x34 + i * 4);
					ushort[] moves = new ushort[4];
					for (int j = 0; j < 4; j++)
						moves[j] = LittleEndian.ToUInt16(raw, 0x4C + j * 2 + i * 8);
					ushort heldItem = LittleEndian.ToUInt16(raw, 0x88 + i * 2);
					byte level = raw[0x94 + i];
					byte unknown = raw[0x9A + i];

					GBAPokemon pokemon = new GBAPokemon();
					pokemon.SpeciesID = species;
					pokemon.Personality = personality;
					pokemon.TrainerID = TrainerID;
					pokemon.SecretID = SecretID;
					pokemon.Nickname = pokemon.PokemonData.Name.ToUpper();
					for (int j = 0; j < 4; j++)
						pokemon.SetMoveAt(j, new Move(moves[j]));
					pokemon.HeldItemID = heldItem;
					pokemon.Language = Languages.English;
					pokemon.Experience = PokemonDatabase.GetExperienceFromLevel(pokemon.PokemonData.ExperienceGroup, level);
					pokemon.Level = level;
					pokemon.RecalculateStats();
					pokemonTeam.Add(pokemon);
				}
			}
		}

		#region Misc

		public SecretBaseManager SecretBaseManager {
			get { return secretBaseManager; }
		}
		public byte[] Raw {
			get { return raw; }
		}
		public byte Flags {
			get { return raw[0x1]; }
			set { raw[0x1] = value; }
		}
		public override Languages Language {
			get { return (Languages)(raw[0xD] + 0x200); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				raw[0xD] = (byte)value;
			}
		}
		public ushort Unknown0x0E {
			get { return LittleEndian.ToUInt16(raw, 0xE); }
			set { LittleEndian.WriteUInt16(value, raw, 0xE); }
		}
		public ushort Unknown0x10 {
			get { return LittleEndian.ToUInt16(raw, 0x10); }
			set { LittleEndian.WriteUInt16(value, raw, 0x10); }
		}
		public byte Unknown0x32 {
			get { return raw[0x32]; }
			set { raw[0x32] = value; }
		}
		public byte Unknown0x33 {
			get { return raw[0x33]; }
			set { raw[0x33] = value; }
		}
		public byte Unknown0x9A {
			get { return raw[0x9A]; }
			set { raw[0x9A] = value; }
		}
		public byte Unknown0x9B {
			get { return raw[0x9B]; }
			set { raw[0x9B] = value; }
		}
		public byte Unknown0x9C {
			get { return raw[0x9C]; }
			set { raw[0x9C] = value; }
		}
		public byte Unknown0x9D {
			get { return raw[0x9D]; }
			set { raw[0x9D] = value; }
		}
		public byte Unknown0x9E {
			get { return raw[0x9E]; }
			set { raw[0x9E] = value; }
		}
		public byte Unknown0x9F {
			get { return raw[0x9F]; }
			set { raw[0x9F] = value; }
		}

		#endregion

		#region Trainer

		public override byte[] TrainerNameRaw {
			get { return ByteHelper.SubByteArray(0x2, raw, 7); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				ByteHelper.ReplaceBytes(raw, 0x2, value);
			}
		}
		public override string TrainerName {
			get { return GBACharacterEncoding.GetString(ByteHelper.SubByteArray(0x2, raw, 7), Language); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				ByteHelper.ReplaceBytes(raw, 0x2, GBACharacterEncoding.GetBytes(value, 7, Language));
			}
		}
		public override Genders TrainerGender {
			get { return (ByteHelper.GetBit(raw, 1, 4) ? Genders.Female : Genders.Male); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				ByteHelper.SetBit(raw, 1, 4, value == Genders.Female);
			}
		}
		public override ushort TrainerID {
			get { return LittleEndian.ToUInt16(raw, 0x9); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, 0x9);
			}
		}
		public override ushort SecretID {
			get { return LittleEndian.ToUInt16(raw, 0xB); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				LittleEndian.WriteUInt16(value, raw, 0xB);
			}
		}
		public override List<GBAPokemon> PokemonTeam {
			get { return pokemonTeam; }
		}
		public override bool HasTeam {
			get { return pokemonTeam.Count > 0; }
		}

		#endregion

		#region Location

		public override IGameSave GameSave {
			get { return PokeManager.ManagerGameSave; }
		}
		public override byte LocationID {
			get { return raw[0x0]; }
			protected set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				raw[0x0] = value;
			}
		}
		public override bool IsPlayerSecretBase {
			get { return false; }
		}
		public bool IsRegistered {
			get { return ByteHelper.GetBit(raw, 1, 6); }
			set {
				if (secretBaseManager != null)
					secretBaseManager.GameSave.IsChanged = true;
				else
					PokeManager.ManagerGameSave.IsChanged = true;
				ByteHelper.SetBit(raw, 1, 6, value);
			}
		}

		public override List<PlacedDecoration> PlacedDecorations {
			get { return placedDecorations; }
		}

		public override void PlaceDecoration(byte id, byte x, byte y) {
			if (secretBaseManager != null)
				secretBaseManager.GameSave.IsChanged = true;
			else
				PokeManager.ManagerGameSave.IsChanged = true;
			if (placedDecorations.Count < 16)
				placedDecorations.Add(new PlacedDecoration(id, x, y));
			else
				throw new Exception("Cannot place decoration when 16 already exist");
		}
		public override void PutAwayDecoration(PlacedDecoration decoration) {
			if (secretBaseManager != null)
				secretBaseManager.GameSave.IsChanged = true;
			else
				PokeManager.ManagerGameSave.IsChanged = true;
			placedDecorations.Remove(decoration);
			RemoveInvalidDecorations();
		}
		public override void PutAwayDecorationAt(int index) {
			if (secretBaseManager != null)
				secretBaseManager.GameSave.IsChanged = true;
			else
				PokeManager.ManagerGameSave.IsChanged = true;
			placedDecorations.RemoveAt(index);
			RemoveInvalidDecorations();
		}

		#endregion

		#region Loading/Saving

		public byte[] GetFinalData() {
			for (int i = 0; i < 16; i++) {
				if (i < placedDecorations.Count) {
					raw[0x12 + i] = placedDecorations[i].ID;
					byte xy = 0;
					xy = ByteHelper.SetBits(xy, 4, ByteHelper.GetBits(placedDecorations[i].X, 0, 4));
					xy = ByteHelper.SetBits(xy, 0, ByteHelper.GetBits(placedDecorations[i].Y, 0, 4));
					raw[0x22 + i] = xy;
				}
				else {
					raw[0x12 + i] = 0;
					raw[0x22 + i] = 0;
				}
			}

			for (int i = 0; i < 6; i++) {
				if (i < pokemonTeam.Count) {
					GBAPokemon pokemon = pokemonTeam[i];
					LittleEndian.WriteUInt32(pokemon.Personality, raw, 0x34 + i * 4);
					LittleEndian.WriteUInt16(pokemon.SpeciesID, raw, 0x7C + i * 2);
					for (int j = 0; j < 4; j++)
						LittleEndian.WriteUInt16(pokemon.GetMoveIDAt(j), raw, 0x4C + j * 2 + i * 8);
					LittleEndian.WriteUInt16(pokemon.HeldItemID, raw, 0x88 + i * 2);
					raw[0x94 + i] = pokemon.Level;
				}
				else {
					LittleEndian.WriteUInt32(0, raw, 0x34 + i * 4);
					LittleEndian.WriteUInt16(0, raw, 0x7C + i * 2);
					for (int j = 0; j < 4; j++)
						LittleEndian.WriteUInt16(0, raw, 0x4C + j * 2 + i * 8);
					LittleEndian.WriteUInt16(0, raw, 0x88 + i * 2);
					raw[0x94 + i] = 0;
				}
			}

			return raw;
		}

		#endregion
	}
}
