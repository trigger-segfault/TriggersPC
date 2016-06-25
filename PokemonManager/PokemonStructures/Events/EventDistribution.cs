using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures.Events {
	public enum EventRewardTypes {
		Pokemon,
		Item,
		Decoration,
		Currency
	}

	public delegate bool CheckEventRequirementsDelegate(IGameSave gameSave);

	public abstract class EventDistribution {

		private string id;

		public EventDistribution(string id) {
			this.id = id;
		}

		public abstract EventRewardTypes RewardType { get; }
		public abstract void GenerateReward(IGameSave gameSave);
		// Can only be used after generate reward.
		public abstract BitmapSource RewardSprite { get; }
		public abstract void GiveReward(IGameSave gameSave);

		public BitmapSource SmallSprite { get; set; }
		public BitmapSource BigSprite { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Requirements { get; set; }
		public GameTypeFlags AllowedGames { get; set; }

		public virtual string GetTitle(IGameSave gameSave) {
			return Title;
		}
		public virtual string GetDescription(IGameSave gameSave) {
			return Description;
		}
		public virtual string GetRequirements(IGameSave gameSave) {
			return Requirements;
		}

		public string ID {
			get { return id; }
		}

		public abstract bool IsCompleted(IGameSave gameSave);
		public abstract bool IsRequirementsFulfilled(IGameSave gameSave);
		public abstract bool HasRoomForReward(IGameSave gameSave);
	}
}
