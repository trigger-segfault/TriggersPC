using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Game.FileStructure.Gen3.GBA;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public class SecretBaseManager {

		private GBAGameSave gameSave;
		private byte[] raw;
		private List<SharedSecretBase> secretBases;

		public SecretBaseManager(GBAGameSave gameSave, byte[] data) {
			this.gameSave = gameSave;
			this.raw = data;
			this.secretBases = new List<SharedSecretBase>();

			for (int i = 0; i < 19; i++) {
				byte locationID = raw[i * 160];
				if (SecretBaseDatabase.GetLocationFromID(locationID) != null)
					this.secretBases.Add(new SharedSecretBase(ByteHelper.SubByteArray(i * 160, data, 160), this));
			}
			Sort();
		}

		public GBAGameSave GameSave {
			get { return gameSave; }
		}

		public List<SharedSecretBase> SharedSecretBases {
			get { return secretBases; }
		}

		public SharedSecretBase AddSecretBase(SharedSecretBase secretBase) {
			GameSave.IsChanged = true;
			SharedSecretBase newSecretBase = new SharedSecretBase(secretBase, this);
			secretBases.Add(newSecretBase);
			Sort();
			return newSecretBase;
		}
		public SharedSecretBase AddSecretBase(PlayerSecretBase secretBase) {
			GameSave.IsChanged = true;
			SharedSecretBase newSecretBase = new SharedSecretBase(secretBase, this);
			secretBases.Add(newSecretBase);
			Sort();
			return newSecretBase;
		}

		public void Sort() {
			this.secretBases.Sort((base1, base2) => (base1.LocationData.Order - base2.LocationData.Order));
		}

		public bool IsLocationInUse(byte locationID) {
			foreach (SecretBase secretBase in secretBases) {
				if (secretBase.LocationID == locationID)
					return true;
			}
			return gameSave.SecretBaseLocation == locationID;
		}

		public int RegistrationCount {
			get {
				int count = 0;
				foreach (SharedSecretBase secretBase in secretBases) {
					if (secretBase.IsRegistered)
						count++;
				}
				return count;
			}
		}

		public byte[] GetFinalData() {
			for (int i = 0; i < 19; i++) {
				if (i < secretBases.Count)
					ByteHelper.ReplaceBytes(raw, i * 160, secretBases[i].GetFinalData());
				else
					ByteHelper.ReplaceBytes(raw, i * 160, new byte[160]);
			}
			return raw;
		}
	}
}
