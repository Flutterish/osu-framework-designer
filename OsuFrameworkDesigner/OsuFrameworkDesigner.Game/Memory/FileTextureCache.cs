using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace OsuFrameworkDesigner.Game.Memory;

public class FileTextureCache {
	Dictionary<string, Task<Texture>> cache = new();

	public Texture GetTexture ( string file ) {
		if ( cache.TryGetValue( file, out var tx ) ) {
			tx.Wait();
			return tx.Result;
		}

		var image = Image.Load<Rgba32>( file );
		var txt = new Texture( image.Width, image.Height, filteringMode: All.Nearest );
		var upload = new TextureUpload( image );
		txt.SetData( upload );

		cache.Add( file, Task.FromResult( txt ) );
		return txt;
	}

	public Task<Texture> GetTextureAsync ( string file ) {
		if ( cache.TryGetValue( file, out var tx ) )
			return tx;

		var task = getTextureAsync( file );
		cache.Add( file, task );
		return task;
	}

	async Task<Texture> getTextureAsync ( string file ) {
		var image = await Image.LoadAsync<Rgba32>( file );
		var txt = new Texture( image.Width, image.Height, filteringMode: All.Nearest );
		var upload = new TextureUpload( image );
		txt.SetData( upload );

		return txt;
	}
}
