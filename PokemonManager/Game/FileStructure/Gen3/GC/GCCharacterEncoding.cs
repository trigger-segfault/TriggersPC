using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GC {
	public class GCCharacterEncoding {

		public static string GetString(byte[] data, Languages language) {
			string str = "";

			for (int i = 0; i < data.Length; i += 2) {
				char c = (char)BigEndian.ToUInt16(data, i);
				if (c == 0)
					break;
				str += new string(c, 1);
			}
			return str;
		}
		public static byte[] GetBytes(string data, int limit, Languages language) {
			data = CharacterEncoding.ConvertToLanguage(data, language);
			byte[] finalData = new byte[limit * 2];
			for (int i = 0; i < data.Length; i++) {
				BigEndian.WriteUInt16(data[i], finalData, i * 2);
			}
			return finalData;
		}
	}
}
