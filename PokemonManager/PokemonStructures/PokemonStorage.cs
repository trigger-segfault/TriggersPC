using PokemonManager.Game;
using PokemonManager.Game.FileStructure.Gen3.GC;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.PokemonStructures {
	public class PokemonStorage : List<IPokemon> {

		private uint size;
		private PokemonFormatTypes formatType;

		public PokemonStorage(byte[] data, PokemonFormatTypes formatType, IPokeContainer container) {
			int formatSize = 0;
			this.formatType = formatType;
			if (formatType == PokemonFormatTypes.Gen3GBA) {
				formatSize = 80;
				if (data.Length % formatSize != 0)
					throw new Exception("Pokemon Storage data size for GBA games should be divisible by 80");
				this.size = (uint)(data.Length / formatSize);

				for (int i = 0; i < size; i++) {
					GBAPokemon pkm = new GBAPokemon(ByteHelper.SubByteArray(i * formatSize, data, formatSize));
					if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
						if (pkm.IsValid)
							Add(pkm);
						else
							Add(GBAPokemon.CreateInvalidPokemon(pkm));
						this[Count - 1].PokeContainer = container;
					}
					else {
						Add(null);
					}
				}
			}
			else if (formatType == PokemonFormatTypes.Gen3PokemonBox) {
				formatSize = 84;
				if (data.Length % formatSize != 0)
					throw new Exception("Pokemon Storage data size for Pokemon Box games should be divisible by 84");
				this.size = (uint)(data.Length / formatSize);

				for (int i = 0; i < size; i++) {
					BoxPokemon pkm = new BoxPokemon(ByteHelper.SubByteArray(i * formatSize, data, formatSize));
					if (pkm.DexID != 0 && pkm.Checksum != 0 && pkm.Experience != 0) {
						if (pkm.IsValid)
							Add(pkm);
						else
							Add(BoxPokemon.CreateInvalidPokemon(pkm));
						this[Count - 1].PokeContainer = container;
					}
					else {
						Add(null);
					}
				}
			}
			else if (formatType == PokemonFormatTypes.Gen3Colosseum) {
				formatSize = 312;
				if (data.Length % formatSize != 0)
					throw new Exception("Pokemon Storage data size for Colosseum should be divisible by 312");
				this.size = (uint)(data.Length / formatSize);

				for (int i = 0; i < size; i++) {
					ColosseumPokemon colopkm = new ColosseumPokemon(ByteHelper.SubByteArray(i * formatSize, data, formatSize));
					if (colopkm.DexID != 0 && colopkm.Experience != 0) {
						if (colopkm.IsValid)
							Add(colopkm);
						else
							Add(ColosseumPokemon.CreateInvalidPokemon(colopkm));
						this[Count - 1].PokeContainer = container;
					}
					else {
						Add(null);
					}
				}
			}
			else if (formatType == PokemonFormatTypes.Gen3XD) {
				formatSize = 196;
				if (data.Length % formatSize != 0)
					throw new Exception("Pokemon Storage data size for XD should be divisible by 196");
				this.size = (uint)(data.Length / formatSize);

				for (int i = 0; i < size; i++) {
					XDPokemon xdpkm = new XDPokemon(ByteHelper.SubByteArray(i * formatSize, data, formatSize));
					if (xdpkm.DexID != 0 && xdpkm.Experience != 0) {
						if (xdpkm.IsValid)
							Add(xdpkm);
						else
							Add(XDPokemon.CreateInvalidPokemon(xdpkm));
						this[Count - 1].PokeContainer = container;
					}
					else {
						Add(null);
					}
				}
			}
		}

		public uint NumPokemon {
			get {
				uint count = 0;
				for (int i = 0; i < Count; i++) {
					if (this[i] != null)
						count++;
				}
				return count;
			}
		}
		public uint StorageSize {
			get { return size; }
		}

		public byte[] GetFinalData() {
			int formatSize = 0;
			if (formatType == PokemonFormatTypes.Gen3GBA)
				formatSize = 80;
			else if (formatType == PokemonFormatTypes.Gen3Colosseum)
				formatSize = 312;
			else if (formatType == PokemonFormatTypes.Gen3XD)
				formatSize = 196;

			List<byte> data = new List<byte>((int)size * formatSize);
			foreach (IPokemon pokemon in this) {
				if (pokemon != null)
					data.AddRange(pokemon.GetFinalData().Take<byte>(formatSize));
				else
					data.AddRange(new byte[formatSize]);
			}
			return data.ToArray();
		}
	}
}
