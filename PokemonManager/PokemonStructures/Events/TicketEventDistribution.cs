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
	public enum TicketTypes {
		EonTicket,
		MysticTicket,
		AuroraTicket,
		OldSeaMap
	}

	public class TicketEventDistribution : EventDistribution {

		public TicketTypes TicketType { get; set; }

		public TicketEventDistribution(string id) : base(id) { }

		private ushort TicketItemID {
			get {
				if (TicketType == TicketTypes.EonTicket)
					return 275;
				else if (TicketType == TicketTypes.MysticTicket)
					return 370;
				else if (TicketType == TicketTypes.AuroraTicket)
					return 371;
				else if (TicketType == TicketTypes.OldSeaMap)
					return 376;
				return 0;
			}
		}
		public override EventRewardTypes RewardType {
			get { return EventRewardTypes.Item; }
		}
		public override void GenerateReward(IGameSave gameSave) { }
		// Can only be used after generate reward.
		public override BitmapSource RewardSprite {
			get { return ItemDatabase.GetItemImageFromID(TicketItemID); }
		}
		public override void GiveReward(IGameSave gameSave) {
			GBAGameSave gbaSave = gameSave as GBAGameSave;
			GameTypes gameType = gameSave.GameType;
			if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire) {
				if (TicketType == TicketTypes.EonTicket)
					gbaSave.SetGameFlag((int)RubySapphireGameFlags.EonTicketActivated, true);
			}
			else if (gameType == GameTypes.FireRed || gameType == GameTypes.LeafGreen) {
				if (TicketType == TicketTypes.MysticTicket)
					gbaSave.SetGameFlag((int)FireRedLeafGreenGameFlags.MysticTicketActivated, true);
				else if (TicketType == TicketTypes.AuroraTicket)
					gbaSave.SetGameFlag((int)FireRedLeafGreenGameFlags.AuroraTicketActivated, true);
			}
			else if (gameType == GameTypes.Emerald) {
				if (TicketType == TicketTypes.EonTicket)
					gbaSave.SetGameFlag((int)EmeraldGameFlags.EonTicketActivated, true);
				else if (TicketType == TicketTypes.MysticTicket)
					gbaSave.SetGameFlag((int)EmeraldGameFlags.MysticTicketActivated, true);
				else if (TicketType == TicketTypes.AuroraTicket)
					gbaSave.SetGameFlag((int)EmeraldGameFlags.AuroraTicketActivated, true);
				else if (TicketType == TicketTypes.OldSeaMap)
					gbaSave.SetGameFlag((int)EmeraldGameFlags.OldSeaMapActivated, true);
			}
			if (gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(TicketItemID) == 0)
				gameSave.Inventory.Items[ItemTypes.KeyItems].AddItem(TicketItemID, 1);
			PokeManager.ManagerWindow.GotoItem(gameSave.GameIndex, ItemTypes.KeyItems, TicketItemID);
		}

		public override bool IsRequirementsFulfilled(IGameSave gameSave) {
			GBAGameSave gbaSave = gameSave as GBAGameSave;
			GameTypes gameType = gameSave.GameType;
			if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire)
				return gbaSave.GetGameFlag((int)RubySapphireGameFlags.HasBeatenEliteFour);
			else if (gameType == GameTypes.FireRed || gameType == GameTypes.LeafGreen)
				return gbaSave.GetGameFlag((int)FireRedLeafGreenGameFlags.HasBeatenEliteFour);
			else if (gameType == GameTypes.Emerald)
				return gbaSave.GetGameFlag((int)EmeraldGameFlags.HasBeatenEliteFour);
			return false;
		}
		public override bool IsCompleted(IGameSave gameSave) {
			if (gameSave.Inventory.Items[ItemTypes.KeyItems].GetCountOfID(TicketItemID) == 0)
				return false;
			GBAGameSave gbaSave = gameSave as GBAGameSave;
			GameTypes gameType = gameSave.GameType;
			if (gameType == GameTypes.Ruby || gameType == GameTypes.Sapphire) {
				if (TicketType == TicketTypes.EonTicket)
					return gbaSave.GetGameFlag((int)RubySapphireGameFlags.EonTicketActivated);
			}
			else if (gameType == GameTypes.FireRed || gameType == GameTypes.LeafGreen) {
				if (TicketType == TicketTypes.MysticTicket)
					return gbaSave.GetGameFlag((int)FireRedLeafGreenGameFlags.MysticTicketActivated);
				else if (TicketType == TicketTypes.AuroraTicket)
					return gbaSave.GetGameFlag((int)FireRedLeafGreenGameFlags.AuroraTicketActivated);
			}
			else if (gameType == GameTypes.Emerald) {
				if (TicketType == TicketTypes.EonTicket)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.EonTicketActivated);
				else if (TicketType == TicketTypes.MysticTicket)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.MysticTicketActivated);
				else if (TicketType == TicketTypes.AuroraTicket)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.AuroraTicketActivated);
				else if (TicketType == TicketTypes.OldSeaMap)
					return gbaSave.GetGameFlag((int)EmeraldGameFlags.OldSeaMapActivated);
			}
			return false;
		}
		public override bool HasRoomForReward(IGameSave gameSave) {
			return gameSave.Inventory.Items[ItemTypes.KeyItems].HasRoomForItem(TicketItemID, 1);
		}

	}
}
