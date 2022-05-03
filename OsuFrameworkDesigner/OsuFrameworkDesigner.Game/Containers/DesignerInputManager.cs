using osu.Framework.Input;
using osu.Framework.Logging;

namespace OsuFrameworkDesigner.Game.Containers;

[Cached]
public class DesignerInputManager : PassThroughInputManager {
	[Resolved]
	OsuFrameworkDesignerGameBase game { get; set; } = null!;

	[Resolved( canBeNull: true )]
	DesignerInputManager? fileDropParent { get; set; }

	[BackgroundDependencyLoader]
	private void load () {
		if ( fileDropParent is null )
			game.FileDrop += onFileDrop;
	}

	private void onFileDrop ( string[] e ) {
		Schedule( () => {
			foreach ( var file in e ) {
				var handled = PositionalInputQueue.OfType<IFileDropHandler>().FirstOrDefault( d => tryHandleFileDrop( d, file ) );

				if ( handled != null )
					Logger.Log( $"File drop ({file}) handled by {handled}.", LoggingTarget.Runtime, LogLevel.Debug );
				else
					Logger.Log( $"File drop ({file}) was not handled by anything.", LoggingTarget.Runtime, LogLevel.Debug );
			}
		} );
	}

	private bool tryHandleFileDrop ( IFileDropHandler drawable, string file ) {
		return drawable.OnFileDrop( file );
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		game.FileDrop -= onFileDrop;
	}
}

/// <summary>
/// Allows this drawable to handle file drop events. You need <see cref="Drawable.HandlePositionalInput"/> for this to work.
/// </summary>
public interface IFileDropHandler {
	bool OnFileDrop ( string files );
}