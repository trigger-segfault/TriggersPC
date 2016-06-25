using PokemonManager.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager {
	public static class ResourceDatabase {

		private static Dictionary<string, BitmapImage> imageNameMap;
		private static Dictionary<string, string> textNameMap;

		public static void Initialize() {
			ResourceDatabase.imageNameMap = new Dictionary<string, BitmapImage>();
			ResourceDatabase.textNameMap = new Dictionary<string, string>();

			// Load all Images
			foreach (string r in GetResourcesWithExtension(".png")) {
				BitmapImage bitmap = LoadImage(r);
				imageNameMap.Add(Path.GetFileNameWithoutExtension(r).ToLower(), bitmap);
			}
			foreach (string r in GetResourcesWithExtension(".ico")) {
				BitmapImage bitmap = LoadImage(r);
				imageNameMap.Add(Path.GetFileNameWithoutExtension(r).ToLower(), bitmap);
			}
			textNameMap.Add("learnablemovesdatabase",
				Resources.LearnableMoves001_050 +
				Resources.LearnableMoves051_100 +
				Resources.LearnableMoves101_151 +
				Resources.LearnableMoves152_200 +
				Resources.LearnableMoves201_251 +
				Resources.LearnableMoves252_300 +
				Resources.LearnableMoves301_350 +
				Resources.LearnableMoves351_386
			);
			textNameMap.Add("learnablemovesdeoxys", Resources.LearnableMovesDeoxys);
		}

		// Case insensitive since the resource compiler always lowers all case
		public static BitmapImage GetImageFromName(string name, string defaultName = null) {
			name = name.ToLower();
			if (imageNameMap.ContainsKey(name))
				return imageNameMap[name];
			if (defaultName != null) {
				defaultName = defaultName.ToLower();
				if (imageNameMap.ContainsKey(defaultName))
					return imageNameMap[defaultName];
			}
			return null;
		}
		// Case insensitive since the resource compiler always lowers all case
		public static string GetTextFromName(string name, string defaultName = null) {
			name = name.ToLower();
			if (textNameMap.ContainsKey(name))
				return textNameMap[name];
			if (defaultName != null) {
				defaultName = defaultName.ToLower();
				if (textNameMap.ContainsKey(defaultName))
					return textNameMap[defaultName];
			}
			return null;
		}
		public static bool ContainsImageWithName(string name) {
			name = name.ToLower();
			return imageNameMap.ContainsKey(name);
		}

		private static string[] GetResourcesWithExtension(string extension) {
			string folder = "resources/";

			var assembly       = Assembly.GetCallingAssembly();
			var resourcesName  = assembly.GetName().Name + ".g.resources";
			var stream         = assembly.GetManifestResourceStream(resourcesName);
			var resourceReader = new ResourceReader(stream);
			
			var resources =
				from p in resourceReader.OfType<DictionaryEntry>()
				let theme = (string)p.Key
				where theme.StartsWith(folder) && theme.EndsWith(extension)
				select theme.Substring(folder.Length);

			return resources.ToArray();
		}
		private static BitmapImage LoadImage(string resourcePath) {
			resourcePath = "pack://application:,,,/resources/" + resourcePath;
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(resourcePath);
			bitmap.EndInit();
			return bitmap;
		}
	}
}
