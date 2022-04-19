using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace OsuFrameworkDesigner.Game.Tests;

public class OsuFrameworkDesignerTestBrowser : OsuFrameworkDesignerGameBase {
	protected override void LoadComplete () {
		base.LoadComplete();

		AddRange( new Drawable[]
		{
			new TestBrowser("OsuFrameworkDesigner"),
			new CursorContainer()
		} );
	}

	public override void SetHost ( GameHost host ) {
		base.SetHost( host );
		host.Window.CursorState |= CursorState.Hidden;
	}
}