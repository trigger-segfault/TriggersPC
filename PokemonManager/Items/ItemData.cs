using PokemonManager.Game;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class ItemData {

		#region Members

		private ushort id;
		private string name;
		private string description;
		private ItemTypes pocketType;
		private ItemTypes subPocketType;
		private ushort transferUpID;
		private int order;

		private uint price;
		private uint sell;
		private uint coinsPrice;
		private uint bpPrice;
		private uint pcPrice;
		private uint sootPrice;
		private bool obtainable;
		private bool important;
		private GameTypeFlags exclusives;

		#endregion

		public ItemData(DataRow row, Generations gen) {
			this.id				= (ushort)(long)row["ID"];
			this.name			= row["Name"] as string;
			this.description	= row["Description"] as string;
			this.pocketType		= ItemData.GetPocketTypeFromString(row["Pocket"] as string);
			this.subPocketType	= (row["SubPocket"] is DBNull ? this.pocketType : ItemData.GetPocketTypeFromString(row["SubPocket"] as string));
			this.transferUpID	= 0; //(ushort)(long)row["TransferUpID"];
			this.order			= (row["Order"] is DBNull ? 0 : (int)(long)row["Order"]);
			this.price			= (uint)(long)row["Price"];
			this.sell			= (uint)(long)row["Sell"];
			this.coinsPrice		= (uint)(long)row["Coins"];
			this.bpPrice		= (uint)(long)row["BP"];
			this.pcPrice		= (uint)(long)row["PC"];
			this.sootPrice		= (uint)(long)row["Soot"];
			this.obtainable		= (bool)row["Obtainable"];
			this.important		= (bool)row["Important"];
			this.exclusives		= ItemData.GetExclusivesFromString(row["Exclusive"] as string, gen);
		}

		#region Properties

		public ushort ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public string Description {
			get { return description; }
		}
		public ItemTypes PocketType {
			get { return pocketType; }
		}
		public ItemTypes SubPocketType {
			get { return subPocketType; }
		}
		public bool HasSubPocket {
			get { return pocketType != subPocketType; }
		}
		public ushort TransferUpID {
			get { return transferUpID; }
		}
		public int Order {
			get { return order; }
		}
		public bool CanTransferUp {
			get { return transferUpID != 0; }
		}
		public uint Price {
			get { return price; }
		}
		public uint SellPrice {
			get { return sell; }
		}
		public uint CoinsPrice {
			get { return coinsPrice; }
		}
		public uint BattlePointsPrice {
			get { return bpPrice; }
		}
		public uint PokeCouponsPrice {
			get { return pcPrice; }
		}
		public uint VolcanicAshPrice {
			get { return sootPrice; }
		}
		public bool IsObtainable {
			get { return obtainable; }
		}
		public bool IsImportant {
			get { return important; }
		}
		public GameTypeFlags Exclusives {
			get { return exclusives; }
		}

		#endregion

		#region Private Helpers

		private static ItemTypes GetPocketTypeFromString(string pocketString) {
			if (pocketString == "UNKNOWN") return ItemTypes.Unknown;
			if (pocketString == "ITEMS") return ItemTypes.Items;
			if (pocketString == "KEY ITEMS") return ItemTypes.KeyItems;
			if (pocketString == "POKE BALLS") return ItemTypes.PokeBalls;
			if (pocketString == "TM CASE") return ItemTypes.TMCase;
			if (pocketString == "BERRIES") return ItemTypes.Berries;
			if (pocketString == "COLOGNE CASE") return ItemTypes.CologneCase;
			if (pocketString == "DISC CASE") return ItemTypes.DiscCase;
			if (pocketString == "IN BATTLE") return ItemTypes.InBattle;
			if (pocketString == "VALUABLES") return ItemTypes.Valuables;
			if (pocketString == "HOLD") return ItemTypes.Hold;
			if (pocketString == "MISC") return ItemTypes.Misc;
			return ItemTypes.Unknown;
		}

		private static GameTypeFlags GetExclusivesFromString(string exclusives, Generations gen) {
			GameTypeFlags gameTypeFlags = GameTypeFlags.None;
			if (gen == Generations.Gen3) {
				if (exclusives == null || exclusives == "")
					return GameTypeFlags.AllGen3;
				if (exclusives.Contains('R')) gameTypeFlags |= GameTypeFlags.Ruby;
				if (exclusives.Contains('S')) gameTypeFlags |= GameTypeFlags.Sapphire;
				if (exclusives.Contains('E')) gameTypeFlags |= GameTypeFlags.Emerald;
				if (exclusives.Contains('F')) gameTypeFlags |= GameTypeFlags.FireRed;
				if (exclusives.Contains('L')) gameTypeFlags |= GameTypeFlags.LeafGreen;
				if (exclusives.Contains('C')) gameTypeFlags |= GameTypeFlags.Colosseum;
				if (exclusives.Contains('X')) gameTypeFlags |= GameTypeFlags.XD;
			}
			else if (gen == Generations.Gen2) {
				if (exclusives == null || exclusives == "")
					return GameTypeFlags.AllGen2;
				if (exclusives.Contains('G')) gameTypeFlags |= GameTypeFlags.Gold;
				if (exclusives.Contains('S')) gameTypeFlags |= GameTypeFlags.Silver;
				if (exclusives.Contains('C')) gameTypeFlags |= GameTypeFlags.Crystal;
			}
			else if (gen == Generations.Gen1) {
				if (exclusives == null || exclusives == "")
					return GameTypeFlags.AllGen1;
				if (exclusives.Contains('R')) gameTypeFlags |= GameTypeFlags.Red;
				if (exclusives.Contains('B')) gameTypeFlags |= GameTypeFlags.Blue;
				if (exclusives.Contains('Y')) gameTypeFlags |= GameTypeFlags.Yellow;
			}
			return gameTypeFlags;
		}

		#endregion
	}
}
