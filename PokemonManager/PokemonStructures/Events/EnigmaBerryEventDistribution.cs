using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures.Events {
	public class EnigmaBerryEventDistribution : EventDistribution {

		public const ushort BerryItemID = 175;
			

		public EnigmaBerryEventDistribution(string id) : base(id) { }

		public override EventRewardTypes RewardType {
			get { return EventRewardTypes.Item; }
		}
		public override void GenerateReward(IGameSave gameSave) { }
		// Can only be used after generate reward.
		public override BitmapSource RewardSprite {
			get { return ItemDatabase.GetItemImageFromID(BerryItemID); }
		}
		public override void GiveReward(IGameSave gameSave) {
			gameSave.Inventory.Items[ItemTypes.Berries].AddItem(BerryItemID, 1);
			PokeManager.ManagerWindow.GotoItem(gameSave.GameIndex, ItemTypes.Berries, BerryItemID);
		}

		public override bool IsRequirementsFulfilled(IGameSave gameSave) {
			GBAGameSave gbaSave = gameSave as GBAGameSave;
			GameTypes gameType = gameSave.GameType;
			for (int i = 0; i < 40; i++) {
				if (gameSave.Inventory.Items[ItemTypes.Berries].GetCountOfID((ushort)(i + 133)) == 0 && gameSave.Inventory.Items[ItemTypes.PC].GetCountOfID((ushort)(i + 133)) == 0)
					return false;
			}
			if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
				return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBeatenEliteFour);
			else if (gameType == GameTypes.FireRed || gameType == GameTypes.LeafGreen)
				return gbaSave.GetGameFlag((int)FireRedLeafGreenGameFlags.HasBeatenEliteFour);
			else if (gameType == GameTypes.Emerald)
				return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBeatenEliteFour);
			return false;
		}
		public override bool IsCompleted(IGameSave gameSave) {
			return false;
		}
		public override bool HasRoomForReward(IGameSave gameSave) {
			return gameSave.Inventory.Items[ItemTypes.Berries].HasRoomForItem(BerryItemID, 1);
		}

	}
}
