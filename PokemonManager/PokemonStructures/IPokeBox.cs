using PokemonManager.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures {
	public interface IPokeBox : IPokeContainer {

		uint BoxNumber { get; set; }
		string Name { get; set; }
		PokeBoxWallpapers Wallpaper { get; set; }
		BitmapSource WallpaperImage { get; }
	}
}
