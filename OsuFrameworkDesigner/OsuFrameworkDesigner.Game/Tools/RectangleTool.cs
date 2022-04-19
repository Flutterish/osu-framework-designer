using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class RectangleTool : Tool {
	RectangleComponent? rect;
	Vector2 dragStartPosition;
	protected override bool OnDragStart ( DragStartEvent e ) {
		dragStartPosition = Composer.ToLocalSpace( e.ScreenSpaceMouseDownPosition );
		Composer.Add( rect = new RectangleComponent {
			Position = e.AltPressed ? dragStartPosition : dragStartPosition.Round(),
			Colour = Colour4.Green
		} );

		rect.TransformProps.CopyProps( rect );
		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		var size = Composer.ToLocalSpace( e.ScreenSpaceMousePosition ) - dragStartPosition;
		if ( e.AltPressed ) {
			rect!.Position = dragStartPosition;
			rect.Size = size;
		}
		else {
			rect!.Position = dragStartPosition.Round();
			rect.Size = size.Round();
		}

		rect!.TransformProps.CopyProps( rect );
	}
}
