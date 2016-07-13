using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class XDDaycare : IDaycare {

		#region Members

		private IPokePC pokePC;
		private byte[] raw;
		private XDPokemon depositedPokemon;

		#endregion

		public XDDaycare(IPokePC pokePC, byte[] data) {
			this.pokePC = pokePC;
			this.raw = data;

			if (DaycareStatus > 0) {
				depositedPokemon = new XDPokemon(ByteHelper.SubByteArray(0x8, data, 196));
				depositedPokemon.PokeContainer = this;
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

		public IPokemon this[int index] {
			get {
				if (index == 0)
					return depositedPokemon;
				else
					throw new ArgumentOutOfRangeException("Index outside of bounds for Daycare", new Exception());
			}
			set {
				if (index == 0) {
					pokePC.GameSave.IsChanged = true;
					IPokemon pkm = (value != null ? (value is XDPokemon ? value : value.CreateXDPokemon(((GCGameSave)GameSave).CurrentRegion)): null);
					if (pkm != null) {
						pkm.PokeContainer = this;
						if (pokePC.GameSave != null)
							pokePC.GameSave.OwnPokemon(pkm);
					}
					depositedPokemon = pkm as XDPokemon;
					if (depositedPokemon == null) {
						DaycareStatus = 0;
						InitialLevel = 0;
						InitialPurification = 0;
					}
					else {
						DaycareStatus = 1;
						InitialLevel = pkm.Level;
						InitialPurification = pkm.Purification;
					}

				}
				else
					throw new ArgumentOutOfRangeException("Index outside of bounds for Daycare", new Exception());
			}
		}
		public void AddPokemon(IPokemon pokemon) {
			pokePC.GameSave.IsChanged = true;
			IPokemon pkm = (pokemon != null ? (pokemon is XDPokemon ? pokemon : pokemon.CreateXDPokemon(((GCGameSave)GameSave).CurrentRegion)): null);
			pkm.GameType = GameType;
			pkm.PokeContainer = this;
			if (pokePC.GameSave != null)
				pokePC.GameSave.OwnPokemon(pkm);
			depositedPokemon = pkm as XDPokemon;
			depositedPokemon = pkm as XDPokemon;
			DaycareStatus = 1;
			InitialLevel = 0;
			InitialPurification = 0;
		}
		public int IndexOf(IPokemon pokemon) {
			if (pokemon == depositedPokemon)
				return 0;
			return -1;
		}
		public void Remove(IPokemon pokemon) {
			if (pokemon == depositedPokemon) {
				pokePC.GameSave.IsChanged = true;
				depositedPokemon = null;
				DaycareStatus = 0;
				InitialLevel = 0;
				InitialPurification = 0;
			}
		}
		public IEnumerator<IPokemon> GetEnumerator() {
			return new PokeContainerEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public byte GetInitialPokemonLevel(int index) {
			if (index == 0)
				return InitialLevel;
			else
				throw new ArgumentOutOfRangeException("Index outside of bounds for Daycare", new Exception());
		}
		public int GetInitialPokemonPurification(int index) {
			if (index == 0)
				return InitialPurification;
			else
				throw new ArgumentOutOfRangeException("Index outside of bounds for Daycare", new Exception());
		}
		public uint GetWithdrawCost(int index) {
			if (index == 0) {
				if (depositedPokemon != null) {
					if (depositedPokemon.IsShadowPokemon) {
						int dif = Math.Max(0, InitialPurification) - Math.Max(0, depositedPokemon.Purification);
						if (dif == 0)
							return 0;
						else if (dif <= 800)
							return 800;
						else
							return (uint)((((dif - 800) / 1200) + 1) * 1200 + 800);
					}
					else {
						int dif = depositedPokemon.Level - InitialLevel;
						if (dif == 0)
							return 0;
						else
							return (uint)((dif + 1) * 100);
					}
				}
				return 0;
			}
			else {
				throw new ArgumentOutOfRangeException("Index outside of bounds for Daycare", new Exception());
			}
		}

		#endregion

		#region Daycare Properties

		public uint NumPokemon {
			get { return (uint)(depositedPokemon != null ? 1 : 0); }
		}
		public uint NumSlots {
			get { return 1; }
		}
		public bool IsEmpty {
			get { return depositedPokemon == null; }
		}

		#endregion

		#region Saving/Loading

		public byte[] GetFinalData() {
			if (depositedPokemon != null)
				ByteHelper.ReplaceBytes(raw, 8, depositedPokemon.GetFinalData());
			else
				ByteHelper.ReplaceBytes(raw, 8, new byte[196]);
			return raw;
		}

		#endregion

		#region Private Helpers

		private sbyte DaycareStatus {
			get { return (sbyte)raw[0x0]; }
			set { raw[0x0] = (byte)value; }
		}
		private byte InitialLevel {
			get { return raw[0x1]; }
			set { raw[0x1] = Math.Min((byte)100, Math.Max((byte)1, value)); }
		}
		private int InitialPurification {
			get { return BigEndian.ToSInt32(raw, 0x4); }
			set { BigEndian.WriteSInt32(value, raw, 0x4); }
		}

		#endregion
	}
}
