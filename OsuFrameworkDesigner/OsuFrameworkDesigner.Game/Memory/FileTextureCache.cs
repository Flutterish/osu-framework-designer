using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace OsuFrameworkDesigner.Game.Memory;

public class FileTextureCache {
	Dictionary<string, Task<WeakReference<Texture>>> cache = new();

	public Texture GetTexture ( string file ) {
		Texture? txt;
		if ( cache.TryGetValue( file, out var task ) ) {
			if ( task.Result.TryGetTarget( out txt ) )
				return txt;
			else
				cache.Remove( file );
		}

		var image = Image.Load<Rgba32>( file );
		txt = new Texture( image.Width, image.Height, filteringMode: All.Nearest );
		var upload = new TextureUpload( image );
		txt.SetData( upload );

		cache.Add( file, Task.FromResult( new WeakReference<Texture>( txt ) ) );
		return txt;
	}

	public async Task<Texture> GetTextureAsync ( string file ) {
		Texture? txt;
		if ( cache.TryGetValue( file, out var task ) ) {
			if ( ( await task ).TryGetTarget( out txt ) )
				return txt;
			else
				cache.Remove( file );
		}

		task = getTextureAsync( file );
		cache.Add( file, task );
		( await task ).TryGetTarget( out txt );
		return txt!;
	}

	async Task<WeakReference<Texture>> getTextureAsync ( string file ) {
		var image = await Image.LoadAsync<Rgba32>( file );
		var txt = new Texture( image.Width, image.Height, filteringMode: All.Nearest );
		var upload = new TextureUpload( image );
		txt.SetData( upload );

		return new WeakReference<Texture>( txt );
	}
}
