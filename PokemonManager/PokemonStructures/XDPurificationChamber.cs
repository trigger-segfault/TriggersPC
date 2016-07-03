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
	public class XDPurificationChamber : IPokeContainer {

		#region Members

		private IPokePC pokePC;
		private List<XDPokemon> normalPokemon;
		private XDPokemon shadowPokemon;
		private byte[] raw;
		private byte chamberNumber;

		#endregion

		public XDPurificationChamber(IPokePC pokePC, byte chamberNumber, byte[] data) {
			this.pokePC = pokePC;
			this.raw = data;
			this.chamberNumber = chamberNumber;

			this.normalPokemon = new List<XDPokemon>();
			for (int i = 0; i < 4; i++) {
				XDPokemon nPkm = new XDPokemon(ByteHelper.SubByteArray(i * 196, data, 196));
				if (nPkm.DexID != 0 && nPkm.Experience != 0) {
					if (nPkm.IsInvalid)
						nPkm = XDPokemon.CreateInvalidPokemon(nPkm);
					normalPokemon.Add(nPkm);
					nPkm.PokeContainer = this;
				}
				else
					break;
			}
			XDPokemon sPkm = new XDPokemon(ByteHelper.SubByteArray(4 * 196, data, 196));
			if (sPkm.DexID != 0 && sPkm.Experience != 0) {
				if (sPkm.IsInvalid)
					sPkm = XDPokemon.CreateInvalidPokemon(sPkm);
				shadowPokemon = sPkm;
				sPkm.PokeContainer = this;
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
			get { return ContainerTypes.Purifier; }
		}
		public byte ChamberNumber {
			get { return chamberNumber; }
		}

		#endregion

		#region Pokemon Accessors

		public IPokemon this[int index] {
			get {
				if (index == 0)
					return shadowPokemon;
				else if (index - 1 >= normalPokemon.Count && index - 1 < 4)
					return null;
				else
					return normalPokemon[index - 1];
			}
			set {
				pokePC.GameSave.IsChanged = true;
				IPokemon pkm = (value != null ? (value is XDPokemon ? value : value.CreateXDPokemon(((GCGameSave)GameSave).CurrentRegion)): null);
				if (pkm != null) {
					pkm.PokeContainer = this;
					if (pokePC.GameSave != null)
						((GCGameSave)pokePC.GameSave).RegisterPokemon(pkm);
				}
				if (index == 0)
					shadowPokemon = (XDPokemon)pkm;
				else if (index - 1 >= normalPokemon.Count && pkm != null)
					normalPokemon.Add((XDPokemon)pkm);
				else if (pkm != null)
					normalPokemon[index - 1] = (XDPokemon)pkm;
				else if (index - 1 < normalPokemon.Count)
					normalPokemon.RemoveAt(index - 1);
			}
		}
		public void AddPokemon(IPokemon pokemon) {
			pokePC.GameSave.IsChanged = true;
			IPokemon pkm = (pokemon != null ? (pokemon is XDPokemon ? pokemon : pokemon.CreateXDPokemon(((GCGameSave)GameSave).CurrentRegion)): null);
			if (pkm != null) {
				pkm.PokeContainer = this;
				if (pokePC.GameSave != null)
					pokePC.GameSave.OwnPokemon(pkm);
				normalPokemon.Add((XDPokemon)pkm);
			}
		}
		public IEnumerator<IPokemon> GetEnumerator() {
			return new PokeContainerEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public int IndexOf(IPokemon pokemon) {
			if (pokemon == shadowPokemon)
				return 0;
			for (int i = 0; i < normalPokemon.Count; i++) {
				if (pokemon == normalPokemon[i])
					return i + 1;
			}
			return -1;
		}
		public void Remove(IPokemon pokemon) {
			if (pokemon == shadowPokemon) {
				pokePC.GameSave.IsChanged = true;
				shadowPokemon = null;
			}
			if (normalPokemon.Contains(pokemon)) {
				pokePC.GameSave.IsChanged = true;
				normalPokemon.Remove((XDPokemon)pokemon);
			}
		}

		#endregion

		#region Chamber Properties

		public uint NumPokemon {
			get { return (uint)(normalPokemon.Count + (shadowPokemon != null ? 1 : 0)); }
		}
		public uint NumSlots {
			get { return 5; }
		}
		public bool IsEmpty {
			get { return normalPokemon.Count == 0 && shadowPokemon == null; }
		}

		#endregion

		#region Saving/Loading

		public byte[] GetFinalData() {
			for (int i = 0; i < 4; i++) {
				if (i < normalPokemon.Count)
					ByteHelper.ReplaceBytes(raw, i * 196, normalPokemon[i].GetFinalData());
				else
					ByteHelper.ReplaceBytes(raw, i * 196, new byte[196]);
			}
			ByteHelper.ReplaceBytes(raw, 5 * 196, new byte[4]);
			return raw;
		}

		#endregion
	}
}
