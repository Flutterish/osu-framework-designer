using osu.Framework.IO.Stores;
using OsuFrameworkDesigner.Resources;
using osuTK;

namespace OsuFrameworkDesigner.Game;

public class OsuFrameworkDesignerGameBase : osu.Framework.Game {
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