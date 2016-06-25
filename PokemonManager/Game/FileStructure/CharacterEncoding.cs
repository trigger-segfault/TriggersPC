using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure {
	public static class CharacterEncoding {

		private static Dictionary<string, string> fullWidthToNormalWidthTable = new Dictionary<string, string>();
		private static Dictionary<string, string> normalWidthToFullWidthTable = new Dictionary<string, string>() {
			{"!", "！"},
			{"-", "－"},
			{"", "？"},
			{"A", "Ａ"},
			{"B", "Ｂ"},
			{"C", "Ｃ"},
			{"D", "Ｄ"},
			{"E", "Ｅ"},
			{"F", "Ｆ"},
			{"G", "Ｇ"},
			{"H", "Ｈ"},
			{"I", "Ｉ"},
			{"J", "Ｊ"},
			{"K", "Ｋ"},
			{"L", "Ｌ"},
			{"M", "Ｍ"},
			{"N", "Ｎ"},
			{"O", "Ｏ"},
			{"P", "Ｐ"},
			{"Q", "Ｑ"},
			{"R", "Ｒ"},
			{"S", "Ｓ"},
			{"T", "Ｔ"},
			{"U", "Ｕ"},
			{"V", "Ｖ"},
			{"W", "Ｗ"},
			{"X", "Ｘ"},
			{"Y", "Ｙ"},
			{"Z", "Ｚ"},
			{"a", "ａ"},
			{"b", "ｂ"},
			{"c", "ｃ"},
			{"d", "ｄ"},
			{"e", "ｅ"},
			{"f", "ｆ"},
			{"g", "ｇ"},
			{"h", "ｈ"},
			{"i", "ｉ"},
			{"j", "ｊ"},
			{"k", "ｋ"},
			{"l", "ｌ"},
			{"m", "ｍ"},
			{"n", "ｎ"},
			{"o", "ｏ"},
			{"p", "ｐ"},
			{"q", "ｑ"},
			{"r", "ｒ"},
			{"s", "ｓ"},
			{"t", "ｔ"},
			{"u", "ｕ"},
			{"v", "ｖ"},
			{"w", "ｗ"},
			{"x", "ｘ"},
			{"y", "ｙ"},
			{"z", "ｚ"}/*,
			{"\"", "＂"},
			{"#", "＃"},
			{"$", "＄"},
			{"%", "％"},
			{"&", "＆"},
			{"'", "＇"},
			{"(", "（"},
			{")", "）"},
			{"*", "＊"},
			{"+", "＋"},
			{",", "，"},
			{".", "．"},
			{"/", "／"},
			{"0", "０"},
			{"1", "１"},
			{"2", "２"},
			{"3", "３"},
			{"4", "４"},
			{"5", "５"},
			{"6", "６"},
			{"7", "７"},
			{"8", "８"},
			{"9", "９"},
			{"", "："},
			{"", "；"},
			{"", "＜"},
			{"", "＝"},
			{"", "＞"},
			{"", "＠"},
			{"", "［"},
			{"", "＼"},
			{"", "］"},
			{"", "＾"},
			{"", "＿"},
			{"", "｀"},
			{"{", "｛"},
			{"|", "｜"},
			{"}", "｝"},
			{"~", "～"}*/
		};

		public static void Initialize() {
			foreach (KeyValuePair<string, string> pair in normalWidthToFullWidthTable) {
				fullWidthToNormalWidthTable.Add(pair.Value, pair.Key);
			}
		}

		public static string ConvertToLanguage(string text, Languages language) {
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < text.Length; i++) {
				string s = new string(text[i], 1);
				if (language == Languages.Japanese && normalWidthToFullWidthTable.ContainsKey(s))
					s = normalWidthToFullWidthTable[s];
				else if (language != Languages.Japanese && fullWidthToNormalWidthTable.ContainsKey(s))
					s = fullWidthToNormalWidthTable[s];
				builder.Append(s);
			}
			return builder.ToString();
		}
	}
}
