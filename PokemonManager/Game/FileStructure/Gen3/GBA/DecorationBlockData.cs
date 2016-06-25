using PokemonManager.Items;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class DecorationBlockData : BlockData {

		private List<SharedSecretBase> sharedSecretBases;

		public DecorationBlockData(IGameSave gameSave, byte[] data, BlockDataCollection parent)
			: base(gameSave, data, parent) {

			if (parent.GameCode == GameCodes.RubySapphire) {
				if (parent.Inventory.Decorations == null)
					parent.Inventory.AddDecorationInventory();
				AddDecorationContainer(DecorationTypes.Desk, 1952, 10);
				AddDecorationContainer(DecorationTypes.Chair, 1962, 10);
				AddDecorationContainer(DecorationTypes.Plant, 1972, 10);
				AddDecorationContainer(DecorationTypes.Ornament, 1982, 30);
				AddDecorationContainer(DecorationTypes.Mat, 2012, 30);
				AddDecorationContainer(DecorationTypes.Poster, 2042, 10);
				AddDecorationContainer(DecorationTypes.Doll, 2052, 40);
				AddDecorationContainer(DecorationTypes.Cushion, 2092, 10);
				// TODO: Find where the XY values actually are stored.
				for (int i = 0; i < 12; i++) {
					byte id = raw[1928 + i];
					if (id != 0) {
						byte x = ByteHelper.BitsToByte(raw, 1928 + 12 + i, 4, 4);
						byte y = ByteHelper.BitsToByte(raw, 1928 + 12 + i, 0, 4);
						parent.Inventory.Decorations.BedroomDecorations.Add(new PlacedDecoration(id, x, y));
					}
				}

				parent.Mailbox.Load(ByteHelper.SubByteArray(3148, raw, 16 * 36));
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				if (parent.Inventory.Decorations == null)
					parent.Inventory.AddDecorationInventory();
				AddDecorationContainer(DecorationTypes.Desk, 2100, 10);
				AddDecorationContainer(DecorationTypes.Chair, 2110, 10);
				AddDecorationContainer(DecorationTypes.Plant, 2120, 10);
				AddDecorationContainer(DecorationTypes.Ornament, 2130, 30);
				AddDecorationContainer(DecorationTypes.Mat, 2160, 30);
				AddDecorationContainer(DecorationTypes.Poster, 2190, 10);
				AddDecorationContainer(DecorationTypes.Doll, 2200, 40);
				AddDecorationContainer(DecorationTypes.Cushion, 2240, 10);
				// TODO: Find where the XY values actually are stored.
				for (int i = 0; i < 12; i++) {
					byte id = raw[2076 + i];
					if (id != 0) {
						byte x = ByteHelper.BitsToByte(raw, 2076 + 12 + i, 4, 4);
						byte y = ByteHelper.BitsToByte(raw, 2076 + 12 + i, 0, 4);
						parent.Inventory.Decorations.BedroomDecorations.Add(new PlacedDecoration(id, x, y));
					}
				}

				parent.Mailbox.Load(ByteHelper.SubByteArray(3296, raw, 16 * 36));

				/*sharedSecretBases = new List<SharedSecretBase>();
				for (int i = 0; i < 3; i++) {
					byte locationID = raw[1596 + i * 160];
					if (SecretBaseDatabase.GetLocationFromID(locationID) != null)
						sharedSecretBases.Add(new SharedSecretBase(ByteHelper.SubByteArray(1596 + i * 160, data, 160)));
				}*/
			}
			else if (parent.GameCode == GameCodes.FireRedLeafGreen) {
				parent.Mailbox.LoadPart1(ByteHelper.SubByteArray(3536, raw, 12 * 36));
			}
		}

		public List<SharedSecretBase> SharedSecretBases {
			get { return sharedSecretBases; }
		}

		private void AddDecorationContainer(DecorationTypes pocketType, int index, uint containerSize) {
			parent.Inventory.Decorations.AddPocket(pocketType, containerSize, 1);
			for (int i = 0; i < (int)parent.Inventory.Decorations[pocketType].TotalSlots; i++) {
				byte id = raw[index + i];
				if (id != 0)
					parent.Inventory.Decorations[pocketType].AddDecoration(id, 1);
			}
		}

		private void SaveDecorationContainer(DecorationTypes pocketType, int index) {
			for (int i = 0; i < (int)parent.Inventory.Decorations[pocketType].TotalSlots; i++) {
				if (i < (int)parent.Inventory.Decorations[pocketType].SlotsUsed) {
					Decoration decoration = parent.Inventory.Decorations[pocketType][i];
					raw[index + i] = decoration.ID;
				}
				else {
					raw[index + i] = 0;
				}
			}
		}

		public override byte[] GetFinalData() {
			if (parent.GameCode == GameCodes.RubySapphire) {
				SaveDecorationContainer(DecorationTypes.Desk, 1952);
				SaveDecorationContainer(DecorationTypes.Chair, 1962);
				SaveDecorationContainer(DecorationTypes.Plant, 1972);
				SaveDecorationContainer(DecorationTypes.Ornament, 1982);
				SaveDecorationContainer(DecorationTypes.Mat, 2012);
				SaveDecorationContainer(DecorationTypes.Poster, 2042);
				SaveDecorationContainer(DecorationTypes.Doll, 2052);
				SaveDecorationContainer(DecorationTypes.Cushion, 2092);
				for (int i = 0; i < 12; i++) {
					if (i < parent.Inventory.Decorations.BedroomDecorations.Count) {
						raw[1928 + i] = parent.Inventory.Decorations.BedroomDecorations[i].ID;
						byte xy = 0;
						xy = ByteHelper.SetBits(xy, 4, ByteHelper.GetBits(parent.Inventory.Decorations.BedroomDecorations[i].X, 0, 4));
						xy = ByteHelper.SetBits(xy, 0, ByteHelper.GetBits(parent.Inventory.Decorations.BedroomDecorations[i].Y, 0, 4));
						raw[1928 + 12 + i] = xy;
					}
					else {
						raw[1928 + i] = 0;
						raw[1928 + 12 + i] = 0;
					}
				}
				ByteHelper.ReplaceBytes(raw, 3148, parent.Mailbox.GetFinalData());
			}
			else if (parent.GameCode == GameCodes.Emerald) {
				SaveDecorationContainer(DecorationTypes.Desk, 2100);
				SaveDecorationContainer(DecorationTypes.Chair, 2110);
				SaveDecorationContainer(DecorationTypes.Plant, 2120);
				SaveDecorationContainer(DecorationTypes.Ornament, 2130);
				SaveDecorationContainer(DecorationTypes.Mat, 2160);
				SaveDecorationContainer(DecorationTypes.Poster, 2190);
				SaveDecorationContainer(DecorationTypes.Doll, 2200);
				SaveDecorationContainer(DecorationTypes.Cushion, 2240);
				for (int i = 0; i < 12; i++) {
					if (i < parent.Inventory.Decorations.BedroomDecorations.Count) {
						raw[2076 + i] = parent.Inventory.Decorations.BedroomDecorations[i].ID;
						byte xy = 0;
						xy = ByteHelper.SetBits(xy, 4, ByteHelper.GetBits(parent.Inventory.Decorations.BedroomDecorations[i].X, 0, 4));
						xy = ByteHelper.SetBits(xy, 0, ByteHelper.GetBits(parent.Inventory.Decorations.BedroomDecorations[i].Y, 0, 4));
						raw[2076 + 12 + i] = xy;
					}
					else {
						raw[2076 + i] = 0;
						raw[2076 + 12 + i] = 0;
					}
				}
				ByteHelper.ReplaceBytes(raw, 3296, parent.Mailbox.GetFinalData());
			}
			else if (parent.GameCode == GameCodes.FireRedLeafGreen) {
				ByteHelper.ReplaceBytes(raw, 3536, parent.Mailbox.GetFinalDataPart1());
			}
			Checksum = CalculateChecksum();
			return raw;
		}
	}
}
