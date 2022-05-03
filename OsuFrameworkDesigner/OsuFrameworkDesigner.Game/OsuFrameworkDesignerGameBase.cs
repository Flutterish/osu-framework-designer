using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using OsuFrameworkDesigner.Resources;

namespace OsuFrameworkDesigner.Game;

public class OsuFrameworkDesignerGameBase : osu.Framework.Game {
	[Cached]
	public Theme Theme { get; } = new();

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

	public override void SetHost ( GameHost host ) {
		base.SetHost( host );

		if ( host.Window is OsuTKWindow tkWindow )
			tkWindow.FileDrop += onTkWindowFileDrop;
		else if ( host.Window is SDL2DesktopWindow sdlWindow )
			sdlWindow.DragDrop += onSdlWindowDragDrop;
	}

	void onSdlWindowDragDrop ( string e ) {
		FileDrop?.Invoke( new string[] { e } );
	}

	void onTkWindowFileDrop ( object? _, osuTK.Input.FileDropEventArgs e ) {
		FileDrop?.Invoke( e.FileNames );
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );

		if ( Host.Window is OsuTKWindow tkWindow )
			tkWindow.FileDrop -= onTkWindowFileDrop;
		else if ( Host.Window is SDL2DesktopWindow sdlWindow )
			sdlWindow.DragDrop -= onSdlWindowDragDrop;
	}

	public event Action<string[]>? FileDrop;
}