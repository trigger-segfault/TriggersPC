using PokemonManager.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {

	public enum LearnableMoveTypes {
		None,
		Level,
		Egg,
		Tutor,
		Machine,
		Purification,
		Event
	}

	public class LearnableMove {
		private ushort moveID;
		private LearnableMoveTypes learnType;
		private byte level;
		private string description;
		private GameTypes gameType;

		private LearnableMove(ushort moveID, LearnableMoveTypes learnType) {
			this.moveID = moveID;
			this.learnType = learnType;
		}

		public static LearnableMove CreateLevelUpMove(ushort moveID, byte level) {
			LearnableMove move = new LearnableMove(moveID, LearnableMoveTypes.Level);
			move.level = level;
			return move;
		}
		public static LearnableMove CreateLearnableMove(ushort moveID, LearnableMoveTypes learnType) {
			return new LearnableMove(moveID, learnType);
		}
		public static LearnableMove CreateTutorEgg(ushort moveID) {
			return new LearnableMove(moveID, LearnableMoveTypes.Egg);
		}
		public static LearnableMove CreateTutorMove(ushort moveID) {
			return new LearnableMove(moveID, LearnableMoveTypes.Tutor);
		}
		public static LearnableMove CreateMachineMove(ushort moveID) {
			return new LearnableMove(moveID, LearnableMoveTypes.Machine);
		}
		public static LearnableMove CreatePurificationMove(ushort moveID, byte level, GameTypes gameType) {
			LearnableMove move = new LearnableMove(moveID, LearnableMoveTypes.Purification);
			move.level = level;
			move.gameType = gameType;
			return move;
		}
		public static LearnableMove CreateEventMove(ushort moveID, string description) {
			LearnableMove move = new LearnableMove(moveID, LearnableMoveTypes.Event);
			move.description = description;
			return move;
		}

		public ushort MoveID {
			get { return moveID; }
		}
		public MoveData MoveData {
			get { return PokemonDatabase.GetMoveFromID(moveID); }
		}
		public LearnableMoveTypes LearnType {
			get { return learnType; }
		}
		public byte Level {
			get { return level; }
		}
		public GameTypes GameType {
			get { return gameType; }
		}
		public string Description {
			get { return description; }
		}
	}
}
