using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using OsuFrameworkDesigner.Game.Containers;
using OsuFrameworkDesigner.Game.Memory;
using OsuFrameworkDesigner.Resources;

namespace OsuFrameworkDesigner.Game;

[Cached]
public class OsuFrameworkDesignerGameBase : osu.Framework.Game {
	[Cached]
	public Theme Theme { get; } = new();

	[Cached]
	public FileTextureCache FileTextures { get; } = new();

	protected override Container<Drawable> Content { get; }
	protected OsuFrameworkDesignerGameBase () {
		base.Content.Add( new DesignerInputManager {
			Child = Content = new DrawSizePreservingFillContainer {
				TargetDrawSize = new Vector2( 1920, 1080 )
			}
		}.Fill() );
	}

	[BackgroundDependencyLoader]
	private void load () {
		Resources.AddStore( new DllResourceStore( typeof( OsuFrameworkDesignerResources ).Assembly ) );
	}

	public override void SetHost ( GameHost host ) {
		unbindFileDropEvents();
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
		unbindFileDropEvents();
	}

	protected override bool OnExiting () {
		unbindFileDropEvents();

		return base.OnExiting();
	}

	void unbindFileDropEvents () {
		if ( Host is null )
			return;

		if ( Host.Window is OsuTKWindow tkWindow )
			tkWindow.FileDrop -= onTkWindowFileDrop;
		else if ( Host.Window is SDL2DesktopWindow sdlWindow )
			sdlWindow.DragDrop -= onSdlWindowDragDrop;
	}

	public event Action<string[]>? FileDrop;
}