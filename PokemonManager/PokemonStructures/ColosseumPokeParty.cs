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
using System.Windows;

namespace PokemonManager.PokemonStructures {
	public class ColosseumPokeParty : IPokeParty {

		#region Members

		private IPokePC pokePC;
		private byte[] raw;
		private List<IPokemon> party;

		#endregion

		public ColosseumPokeParty(IPokePC pokePC, byte[] data) {
			this.pokePC = pokePC;
			this.raw = data;
			this.party = new List<IPokemon>();
			for (int i = 0; i < 6; i++) {
				ColosseumPokemon colopkm = new ColosseumPokemon(ByteHelper.SubByteArray(312 * i, data, 312));
				if (colopkm.DexID != 0 && colopkm.Experience != 0) {
					if (colopkm.IsValid)
						party.Add(colopkm);
					else
						party.Add(ColosseumPokemon.CreateInvalidPokemon(colopkm));
					party[party.Count - 1].PokeContainer = this;
				}
				else {
					break;
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
			get { return ContainerTypes.Party; }
		}

		#endregion

		#region Pokemon Accessors

		public IPokemon this[int index] {
			get {
				if (index >= party.Count && index < 6)
					return null;
				return party[index];
			}
			set {
				pokePC.GameSave.IsChanged = true;
				IPokemon pkm = (value != null ? (value is ColosseumPokemon ? value : value.CreateColosseumPokemon(((GCGameSave)GameSave).CurrentRegion)): null);
				if (pkm != null) {
					pkm.PokeContainer = this;
					if (pokePC.GameSave != null)
						pokePC.GameSave.OwnPokemon(pkm);
				}
				if (index >= party.Count && pkm != null)
					party.Add(pkm);
				else if (pkm != null)
					party[index] = pkm;
				else if (index < party.Count)
					party.RemoveAt(index);
			}
		}
		public void AddPokemon(IPokemon pokemon) {
			pokePC.GameSave.IsChanged = true;
			IPokemon pkm = (pokemon != null ? (pokemon is ColosseumPokemon ? pokemon : pokemon.CreateColosseumPokemon(((GCGameSave)GameSave).CurrentRegion)): null);
			if (pkm != null) {
				pkm.PokeContainer = this;
				if (pokePC.GameSave != null)
					((GCGameSave)pokePC.GameSave).RegisterPokemon(pkm);
				party.Add(pkm);
			}
		}
		public IEnumerator<IPokemon> GetEnumerator() {
			return new PokeContainerEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public int IndexOf(IPokemon pokemon) {
			for (int i = 0; i < party.Count; i++) {
				if (party[i] == pokemon)
					return i;
			}
			return -1;
		}
		public void Remove(IPokemon pokemon) {
			if (party.Contains(pokemon)) {
				pokePC.GameSave.IsChanged = true;
				party.Remove(pokemon);
			}
		}

		#endregion

		#region Party Properties

		public uint NumPokemon {
			get { return (uint)party.Count; }
		}
		public uint NumSlots {
			get { return 6; }
		}
		public bool IsEmpty {
			get { return party.Count == 0; }
		}

		#endregion

		#region Saving/Loading

		public byte[] GetFinalData() {
			for (int i = 0; i < 6; i++) {
				if (i < party.Count)
					ByteHelper.ReplaceBytes(raw, 312 * i, party[i].GetFinalData());
				else
					ByteHelper.ReplaceBytes(raw, 312 * i, new byte[312]);
			}
			return raw;
		}

		#endregion
	}
}
