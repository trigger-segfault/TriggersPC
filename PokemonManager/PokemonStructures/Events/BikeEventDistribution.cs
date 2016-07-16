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
	public class BikeEventDistribution : EventDistribution {

		public TicketTypes TicketType { get; set; }

		public BikeEventDistribution(string id) : base(id) { }

		private ushort GetBikeItemID(IGameSave gameSave) {
			return (ushort)(gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(259) != 0 ? 272 : 259);
		}
		public override EventRewardTypes RewardType {
			get { return EventRewardTypes.Item; }
		}
		public override void GenerateReward(IGameSave gameSave) { }
		// Can only be used after generate reward.
		public override BitmapSource RewardSprite {
			get { return ItemDatabase.GetItemImageFromID(272); }
		}
		public override void GiveReward(IGameSave gameSave) {
			ushort bikeID = GetBikeItemID(gameSave);
			if (gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(bikeID) == 0) {
				gameSave.Inventory.Items[ItemTypes.KeyItems].AddItem(bikeID, 1);
				PokeManager.ManagerWindow.GotoItem(gameSave.GameIndex, ItemTypes.KeyItems, bikeID);
			}
		}

		public override bool IsRequirementsFulfilled(IGameSave gameSave) {
			GBAGameSave gbaSave = gameSave as GBAGameSave;
			GameTypes gameType = gameSave.GameType;
			bool hasOneBike = gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(259) != 0 || gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(272) != 0;
			if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
				return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBeatenEliteFour) && hasOneBike;
			else if (gameType == GameTypes.Emerald)
				return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBeatenEliteFour) && hasOneBike;
			return false;
		}
		public override bool IsCompleted(IGameSave gameSave) {
			return gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(259) != 0 && gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(272) != 0;
		}
		public override bool HasRoomForReward(IGameSave gameSave) {
			return gameSave.Inventory.Items[ItemTypes.KeyItems].HasRoomForItem(GetBikeItemID(gameSave), 1);
		}

	}
}
