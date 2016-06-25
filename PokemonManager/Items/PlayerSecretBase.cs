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
	public class PlayerSecretBase : SecretBase {

		#region Members

		private GBAGameSave gameSave;

		#endregion

		public PlayerSecretBase(IGameSave gameSave) {
			this.gameSave = (GBAGameSave)gameSave;
		}

		#region Trainer

		public override byte[] TrainerNameRaw {
			get { return GBACharacterEncoding.GetBytes(gameSave.TrainerName, 7, Language); }
			set {  }
		}
		public override string TrainerName {
			get { return gameSave.TrainerName; }
			set { /* No setting this */ }
		}
		public override Genders TrainerGender {
			get { return gameSave.TrainerGender; }
			set { /* No setting this */ }
		}
		public override ushort TrainerID {
			get { return gameSave.TrainerID; }
			set { /* No setting this */ }
		}
		public override ushort SecretID {
			get { return gameSave.SecretID; }
			set { /* No setting this */ }
		}

		#endregion

		#region Location

		public override Languages Language {
			get { return gameSave.SecretBaseLanguage; }
			set { }
		}
		public override List<GBAPokemon> PokemonTeam {
			get {
				List<GBAPokemon> team = new List<GBAPokemon>();
				foreach (IPokemon pokemon in gameSave.PokePC.Party) {
					if (!pokemon.IsEgg)
						team.Add((GBAPokemon)pokemon.Clone());
				}
				return team;
			}
		}
		public override bool HasTeam {
			get { return true; }
		}
		public override IGameSave GameSave {
			get { return gameSave; }
		}
		public override byte LocationID {
			get { return gameSave.SecretBaseLocation; }
			protected set { gameSave.SecretBaseLocation = value; }
		}
		public override bool IsPlayerSecretBase {
			get { return true; }
		}

		public override List<PlacedDecoration> PlacedDecorations {
			get { return gameSave.Inventory.Decorations.SecretBaseDecorations; }
		}

		public override void PlaceDecoration(byte id, byte x, byte y) {
			if (gameSave.Inventory.Decorations.SecretBaseDecorations.Count < 16)
				gameSave.Inventory.Decorations.PlaceDecorationInSecretBase(id, x, y);
			else
				throw new Exception("Cannot place decoration when 16 already exist");
		}
		public override void PutAwayDecoration(PlacedDecoration decoration) {
			gameSave.Inventory.Decorations.PutAwayDecorationInSecretBaseAt(gameSave.Inventory.Decorations.SecretBaseDecorations.IndexOf(decoration));
		}
		public override void PutAwayDecorationAt(int index) {
			gameSave.Inventory.Decorations.PutAwayDecorationInSecretBaseAt(index);
		}

		#endregion

	}
}
