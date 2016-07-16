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
	public class BattleCDEventDistribution : EventDistribution {

		public ushort CDItemID { get; set; }

		public BattleCDEventDistribution(string id) : base(id) { }

		public override EventRewardTypes RewardType {
			get { return EventRewardTypes.Item; }
		}
		public override void GenerateReward(IGameSave gameSave) { }
		// Can only be used after generate reward.
		public override BitmapSource RewardSprite {
			get { return ItemDatabase.GetItemImageFromID(CDItemID); }
		}
		public override void GiveReward(IGameSave gameSave) {
			if (gameSave.Inventory.Items[ItemTypes.DiscCase].GetCountOfID(CDItemID) == 0) {
				gameSave.Inventory.Items[ItemTypes.DiscCase].AddItem(CDItemID, 1);
				PokeManager.ManagerWindow.GotoItem(gameSave.GameIndex, ItemTypes.DiscCase, CDItemID);
			}
		}

		public override bool IsRequirementsFulfilled(IGameSave gameSave) {
			GCGameSave gcSave = (GCGameSave)gameSave;
			return (gcSave.SnaggedPokemon == 83);
		}
		public override bool IsCompleted(IGameSave gameSave) {
			return (gameSave.Inventory.Items[ItemTypes.DiscCase].GetCountOfID(CDItemID) != 0);
		}
		public override bool HasRoomForReward(IGameSave gameSave) {
			return gameSave.Inventory.Items[ItemTypes.DiscCase].HasRoomForItem(CDItemID, 1);
		}

	}
}
