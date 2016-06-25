using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures.Events {

	public class RegiDollEventDistribution : EventDistribution {

		public byte DollID { get; set; }

		public RegiDollEventDistribution(string id) : base(id) { }

		private int GetRegiEventsCompleted(IGameSave gameSave) {
			int count = 0;
			if (PokeManager.IsEventSoftCompletedBy("REGIROCK DOLL [RSE]", gameSave)) count++;
			if (PokeManager.IsEventSoftCompletedBy("REGICE DOLL [RSE]", gameSave)) count++;
			if (PokeManager.IsEventSoftCompletedBy("REGISTEEL DOLL [RSE]", gameSave)) count++;
			return count;
		}

		public override EventRewardTypes RewardType {
			get { return EventRewardTypes.Decoration; }
		}
		public override void GenerateReward(IGameSave gameSave) { }
		// Can only be used after generate reward.
		public override BitmapSource RewardSprite {
			get { return ItemDatabase.GetDecorationFullSizeImageFromID(DollID); }
		}
		public override void GiveReward(IGameSave gameSave) {
			gameSave.Inventory.Decorations[DecorationTypes.Doll].AddDecoration(DollID, 1);
		}

		public override string GetRequirements(IGameSave gameSave) {
			int count = GetRegiEventsCompleted(gameSave);
			if (count == 0)
				return "You must obtain the Heat Badge from Flannery in order to receive this Doll.";
			else if (count == 1)
				return "You must obtain the Balance Badge from Norman in order to receive this Doll.";
			else
				return "You must obtain the Feather Badge from Winona in order to receive this Doll.";
		}

		public override bool IsRequirementsFulfilled(IGameSave gameSave) {
			GBAGameSave gbaSave = gameSave as GBAGameSave;
			GameTypes gameType = gameSave.GameType;
			int count = GetRegiEventsCompleted(gameSave);
			if (count == 0) {
				if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
					return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBadge4);
				else if (gameType == GameTypes.Emerald)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBadge4);
			}
			else if (count == 1) {
				if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
					return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBadge5);
				else if (gameType == GameTypes.Emerald)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBadge5);
			}
			else if (count == 2) {
				if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
					return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBadge6);
				else if (gameType == GameTypes.Emerald)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBadge6);
			}
			return false;
		}
		public override bool IsCompleted(IGameSave gameSave) {
			return false;
		}
		public override bool HasRoomForReward(IGameSave gameSave) {
			return gameSave.Inventory.Decorations[DecorationTypes.Doll].HasRoomForDecoration(DollID, 1);
		}

	}
}
