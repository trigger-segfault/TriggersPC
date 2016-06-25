using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class GameConfigData : GCData {

		public GameConfigData(GCGameSave gameSave, byte[] data, GCSaveData parent)
			: base(gameSave, data, parent) {
		}



		public GCGameOrigins GCGameIndex {
			get { return (GCGameOrigins)raw[0]; }
			set { raw[0] = (byte)value; }
		}
		public GCRegions CurrentRegion {
			get { return (GCRegions)raw[1]; }
			set { raw[1] = (byte)value; }
		}
		public GCRegions OriginalRegion {
			get { return (GCRegions)raw[2]; }
			set { raw[2] = (byte)value; }
		}
		public GCLanguages GCLanguage {
			get { return (GCLanguages)raw[3]; }
			set { raw[3] = (byte)value; }
		}

		public Languages Language {
			get {
				GCLanguages gcLanguage = (GCLanguages)raw[3];
				switch (gcLanguage) {
				case GCLanguages.Japanese: return Languages.Japanese;
				case GCLanguages.English: return Languages.English;
				case GCLanguages.German: return Languages.German;
				case GCLanguages.French: return Languages.French;
				case GCLanguages.Italian: return Languages.Italian;
				case GCLanguages.Spanish: return Languages.Spanish;
				}
				return Languages.NoLanguage;
			}
			set {
				gameSave.IsChanged = true;
				GCLanguages gcLanguage = GCLanguages.NoLanguage;
				switch (value) {
				case Languages.Japanese: gcLanguage = GCLanguages.Japanese; break;
				case Languages.English: gcLanguage = GCLanguages.English; break;
				case Languages.German: gcLanguage = GCLanguages.German; break;
				case Languages.French: gcLanguage = GCLanguages.French; break;
				case Languages.Italian: gcLanguage = GCLanguages.Italian; break;
				case Languages.Spanish: gcLanguage = GCLanguages.Spanish; break;
				}
				raw[3] = (byte)gcLanguage;
			}
		}
	}
}
