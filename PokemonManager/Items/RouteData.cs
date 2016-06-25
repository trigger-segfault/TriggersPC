using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.Items {
	public class RouteData {

		private byte id;
		private byte width;
		private byte height;
		private BitmapSource image;

		public RouteData(DataRow row) {
			this.id					= (byte)(long)row["ID"];
			this.width				= (byte)(long)row["Width"];
			this.height				= (byte)(long)row["Height"];
			this.image				= LoadImage((byte[])row["Image"]);
		}

		public byte ID {
			get { return id; }
		}
		public byte Width {
			get { return width; }
		}
		public byte Height {
			get { return height; }
		}
		public BitmapSource Image {
			get { return image; }
		}

		private static BitmapImage LoadImage(byte[] imageData) {
			if (imageData == null || imageData.Length == 0) return null;
			var image = new BitmapImage();
			using (var mem = new MemoryStream(imageData)) {
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();
			return image;
		}
	}
}
