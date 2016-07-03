using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures.Events {
	public class BuyPokemonEventDistribution : PokemonEventDistribution {
		
		public BuyPokemonEventDistribution(string id) : base(id) {

		}

		public uint Cost { get; set; }

		public override void GiveReward(IGameSave gameSave) {
			gameSave.Money -= Cost;
			gameSave.PokePC.PlacePokemonInNextAvailableSlot(0, 0, reward);
			PokeManager.ManagerWindow.GotoPokemon(reward);
			PokeManager.RefreshUI();
		}
	}
}
