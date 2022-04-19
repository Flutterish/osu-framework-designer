using osu.Framework.IO.Stores;
using OsuFrameworkDesigner.Resources;

namespace OsuFrameworkDesigner.Game;

public class OsuFrameworkDesignerGameBase : osu.Framework.Game {
	[Cached]
	public ColourConfiguration ColourConfiguration { get; } = new();

	protected override Container<Drawable> Content { get; }
	protected OsuFrameworkDesignerGameBase () {
		base.Content.Add( Content = new DrawSizePreservingFillContainer {
			TargetDrawSize = new Vector2( 1920, 1080 )
		} );
	}

	[BackgroundDependencyLoader]
	private void load () {
		Resources.AddStore( new DllResourceStore( typeof( OsuFrameworkDesignerResources ).Assembly ) );
	}
}