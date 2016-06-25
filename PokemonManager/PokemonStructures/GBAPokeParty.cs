using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PokemonManager.PokemonStructures {
	public class GBAPokeParty : IPokeParty {

		#region Members

		private IPokePC pokePC;
		private byte[] raw;
		private List<IPokemon> party;

		#endregion

		public GBAPokeParty(IPokePC pokePC, byte[] data) {
			this.pokePC = pokePC;
			this.raw = data;
			this.party = new List<IPokemon>();
			uint teamSize = LittleEndian.ToUInt32(data, 0);
			for (int i = 0; i < teamSize && i < 6; i++) {
				GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(4 + 100 * i, data, 100));
				if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
					if (pkm.IsValid)
						party.Add(pkm);
					else
						party.Add(GBAPokemon.CreateInvalidPokemon(pkm));
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
				IPokemon pkm = (value != null ? (value is GBAPokemon ? value : value.CreateGBAPokemon(GameType)): null);
				if (pkm != null) {
					pkm.GameType = GameType;
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
			IPokemon pkm = (pokemon != null ? (pokemon is GBAPokemon ? pokemon : pokemon.CreateGBAPokemon(GameType)): null);
			if (pkm != null) {
				pkm.GameType = GameType;
				pkm.PokeContainer = this;
				if (pokePC.GameSave != null)
					pokePC.GameSave.SetPokemonOwned(pkm.DexID, true);
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

		#region Loading/Saving

		public byte[] GetFinalData() {
			LittleEndian.WriteUInt32((uint)party.Count, raw, 0);
			for (int i = 0; i < 6; i++) {
				if (i < party.Count)
					ByteHelper.ReplaceBytes(raw, 4 + 100 * i, party[i].GetFinalData());
				else
					ByteHelper.ReplaceBytes(raw, 4 + 100 * i, new byte[100]);
			}
			return raw;
		}

		#endregion
	}
}
