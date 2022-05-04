using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace OsuFrameworkDesigner.Game.Memory;

public class FileTextureCache : WeakTaskCache<string, Texture> {
	protected override async Task<Texture> PerformAsync ( string file ) {
		var image = await Image.LoadAsync<Rgba32>( file );
		var txt = new Texture( image.Width, image.Height, filteringMode: All.Nearest );
		var upload = new TextureUpload( image );
		txt.SetData( upload );

		return txt;
	}

	protected override Texture PerformSync ( string file ) {
		var image = Image.Load<Rgba32>( file );
		var txt = new Texture( image.Width, image.Height, filteringMode: All.Nearest );
		var upload = new TextureUpload( image );
		txt.SetData( upload );

		return txt;
	}
}
