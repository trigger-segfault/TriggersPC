using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class GBADaycare : IDaycare {

		#region Members

		private GameCodes gameCode;
		private IPokePC pokePC;
		private byte[] raw;
		private GBAPokemon[] originalPokemon;
		private GBAPokemon[] finalPokemon;

		#endregion

		public GBADaycare(IPokePC pokePC, byte[] data, GameCodes gameCode) {
			this.gameCode = gameCode;
			this.pokePC = pokePC;
			this.raw = data;

			if (gameCode == GameCodes.FireRedLeafGreen) {
				originalPokemon = new GBAPokemon[3];
				finalPokemon = new GBAPokemon[3];
				GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(0, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[0] = pkm;
					else
						originalPokemon[0] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(0);
				}
				pkm = new GBAPokemon(ByteHelper.SubByteArray(140, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[1] = pkm;
					else
						originalPokemon[1] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(1);
				}

				pkm = new GBAPokemon(ByteHelper.SubByteArray(3352, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[2] = pkm;
					else
						originalPokemon[2] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(2);
				}
			}
			else if (gameCode == GameCodes.Emerald) {
				originalPokemon = new GBAPokemon[2];
				finalPokemon = new GBAPokemon[2];
				GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(0, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[0] = pkm;
					else
						originalPokemon[0] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(0);
				}
				pkm = new GBAPokemon(ByteHelper.SubByteArray(140, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[1] = pkm;
					else
						originalPokemon[1] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(1);
				}
			}
			else if (gameCode == GameCodes.RubySapphire) {
				originalPokemon = new GBAPokemon[2];
				finalPokemon = new GBAPokemon[2];
				GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(0, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[0] = pkm;
					else
						originalPokemon[0] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(0);
				}
				pkm = new GBAPokemon(ByteHelper.SubByteArray(80, raw, 80));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						originalPokemon[1] = pkm;
					else
						originalPokemon[1] = GBAPokemon.CreateInvalidPokemon(pkm);
					SetFinalPokemon(1);
				}
			}
		}

		#region Containment Properties

		public IPokePC PokePC {
			get { return pokePC; }
		}
		public IGameSave GameSave {
			get { return pokePC.GameSave; }
		}
		public GameTypes GameType {
			get { return pokePC.GameSave.GameType; }
		}
		public int GameIndex {
			get { return PokeManager.GetIndexOfGame(pokePC.GameSave); }
		}
		public ContainerTypes Type {
			get { return ContainerTypes.Daycare; }
		}

		#endregion

		#region Pokemon Accessors

		public void CancelLearnedMoves(int index) {
			for (int i = 0; i < 4; i++) {
				finalPokemon[index].SetMoveAt(i, originalPokemon[index].GetMoveAt(i));
			}
		}
		public bool HasLearnedNewMoves(int index) {
			for (int i = 0; i < 4; i++) {
				if (finalPokemon[index].GetMoveIDAt(i) != originalPokemon[index].GetMoveIDAt(i))
					return true;
			}
			return false;
		}
		public IPokemon this[int index] {
			get { return finalPokemon[index]; }
			set {
				pokePC.GameSave.IsChanged = true;
				IPokemon pkm = (value != null ? (value is GBAPokemon ? value : value.CreateGBAPokemon(GameType)): null);
				if (pkm != null) {
					pkm.PokeContainer = this;
					if (pokePC.GameSave != null)
						pokePC.GameSave.OwnPokemon(pkm);
				}
				finalPokemon[index] = pkm as GBAPokemon;
				originalPokemon[index] = pkm as GBAPokemon;
				SetGainedExperience(index, 0);

				if (index == 0 || index == 1) {
					// Prevent the user from breeding Groudons with Ditto or freezing the game.
					((GBAGameSave)PokePC.GameSave).ClearDaycareEgg();
				}

				if ((index == 0 || index == 1) && originalPokemon[0] == null && originalPokemon[1] != null) {
					finalPokemon[0] = finalPokemon[1];
					originalPokemon[0] = originalPokemon[1];

					SetGainedExperience(0, GetGainedExperience(1));
					SetGainedExperience(1, 0);
					finalPokemon[1] = null;
					originalPokemon[1] = null;
				}
			}
		}
		public int IndexOf(IPokemon pokemon) {
			for (int i = 0; i < finalPokemon.Length; i++) {
				if (finalPokemon[i] == pokemon)
					return i;
			}
			return -1;
		}
		public void Remove(IPokemon pokemon) {
			int index = IndexOf(pokemon);
			if (index == 0 || index == 1) {
				// Prevent the user from breeding Groudons with Ditto or freezing the game.
				((GBAGameSave)PokePC.GameSave).ClearDaycareEgg();
			}
			if (index != -1) {
				this[index] = null;
			}
		}
		public IEnumerator<IPokemon> GetEnumerator() {
			return new PokeContainerEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public uint GetWithdrawCost(int index) {
			if (originalPokemon[index] != null && finalPokemon[index].Experience - originalPokemon[index].Experience != 0) {
				int dif = finalPokemon[index].Level - originalPokemon[index].Level;
				return (uint)((dif + 1) * 100);
			}
			return 0;
		}

		#endregion

		#region Daycare Properties

		public uint NumPokemon {
			get {
				uint count = 0;
				for (int i = 0; i < originalPokemon.Length; i++) {
					if (originalPokemon[i] != null)
						count++;
				}
				return count;
			}
		}
		public uint NumSlots {
			get { return (uint)finalPokemon.Length; }
		}
		public bool IsEmpty {
			get { return finalPokemon == null; }
		}

		#endregion

		#region Saving/Loading

		public byte[] GetFinalData() {
			if (gameCode == GameCodes.Emerald) {
				if (originalPokemon[0] != null)
					ByteHelper.ReplaceBytes(raw, 0, originalPokemon[0].GetFinalData().Take<byte>(80).ToArray<byte>());
				else
					ByteHelper.ReplaceBytes(raw, 0, new byte[80]);
				if (originalPokemon[1] != null)
					ByteHelper.ReplaceBytes(raw, 140, originalPokemon[1].GetFinalData().Take<byte>(80).ToArray<byte>());
				else
					ByteHelper.ReplaceBytes(raw, 140, new byte[80]);
			}
			else if (gameCode == GameCodes.RubySapphire) {
				if (originalPokemon[0] != null)
					ByteHelper.ReplaceBytes(raw, 0, originalPokemon[0].GetFinalData().Take<byte>(80).ToArray<byte>());
				else
					ByteHelper.ReplaceBytes(raw, 0, new byte[80]);
				if (originalPokemon[1] != null)
					ByteHelper.ReplaceBytes(raw, 80, originalPokemon[1].GetFinalData().Take<byte>(80).ToArray<byte>());
				else
					ByteHelper.ReplaceBytes(raw, 80, new byte[80]);
			}
			return raw;
		}

		public byte[] GetFinalDataKanto() {
			byte[] data = ByteHelper.SubByteArray(3352, raw, 140);
			if (originalPokemon[2] != null)
				ByteHelper.ReplaceBytes(data, 0, originalPokemon[2].GetFinalData().Take<byte>(80).ToArray<byte>());
			else
				ByteHelper.ReplaceBytes(data, 0, new byte[80]);
			return data;
		}

		public byte[] GetFinalDataSevii() {
			byte[] data = ByteHelper.SubByteArray(0, raw, 280);
			if (originalPokemon[0] != null)
				ByteHelper.ReplaceBytes(data, 0, originalPokemon[0].GetFinalData().Take<byte>(80).ToArray<byte>());
			else
				ByteHelper.ReplaceBytes(data, 0, new byte[80]);
			if (originalPokemon[1] != null)
				ByteHelper.ReplaceBytes(data, 140, originalPokemon[1].GetFinalData().Take<byte>(80).ToArray<byte>());
			else
				ByteHelper.ReplaceBytes(data, 140, new byte[80]);
			return data;
		}

		#endregion

		#region Private Helpers

		private uint GetGainedExperience(int index) {
			if (gameCode == GameCodes.Emerald) {
				return LittleEndian.ToUInt32(raw, 136 + 140 * index);
			}
			else if (gameCode == GameCodes.RubySapphire) {
				return LittleEndian.ToUInt32(raw, 272 + 4 * index);
			}
			else if (gameCode == GameCodes.FireRedLeafGreen) {
				if (index == 2)
					return LittleEndian.ToUInt32(raw, 3488);
				else if (index == 0 || index == 1)
					return LittleEndian.ToUInt32(raw, 136 + 140 * index);
				else
					throw new ArgumentOutOfRangeException("Daycare index out of range", new Exception());
			}
			return 0;
		}
		private void SetGainedExperience(int index, uint exp) {
			if (gameCode == GameCodes.Emerald) {
				LittleEndian.WriteUInt32(exp, raw, 136 + 140 * index);
			}
			else if (gameCode == GameCodes.RubySapphire) {
				LittleEndian.WriteUInt32(exp, raw, 272 + 4 * index);
			}
			else if (gameCode == GameCodes.FireRedLeafGreen) {
				if (index == 2)
					LittleEndian.WriteUInt32(exp, raw, 3488);
				else if (index == 0 || index == 1)
					LittleEndian.WriteUInt32(exp, raw, 136 + 140 * index);
				else
					throw new ArgumentOutOfRangeException("Daycare index out of range", new Exception());
			}
		}

		private void SetFinalPokemon(int index) {
			uint gainedExp = GetGainedExperience(index);
			GBAPokemon original = originalPokemon[index];
			GBAPokemon output = (GBAPokemon)original.Clone();
			output.PokeContainer = this;

			output.Experience += gainedExp;
			output.RecalculateStats();

			ushort[] moves = PokemonDatabase.GetMovesLearnedAtLevelRange(original, (byte)(original.Level + 1), output.Level);

			// Teach the Pokemon all the new moves
			// Push the moves up just like the game does it to make room for the new move. Even HMs can be removed.
			foreach (ushort moveID in moves) {
				if (!PokemonDatabase.PokemonHasMove(output, moveID)) {
					if (output.NumMoves < 4) {
						output.SetMoveAt(output.NumMoves, new Move(moveID));
					}
					else {
						for (int i = 0; i < 3; i++) {
							output.SetMoveAt(i, output.GetMoveAt(i + 1));
						}
						output.SetMoveAt(3, new Move(moveID));
					}
				}
			}
			finalPokemon[index] = output;
		}

		#endregion
	}
}
