using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class RectangleTool : Tool {
	RectangleComponent? rect;
	protected override bool OnDragStart ( DragStartEvent e ) {
		Composer.Add( rect = new RectangleComponent { Position = Composer.ToLocalSpace( e.ScreenSpaceMouseDownPosition ), Colour = Colour4.Green } );
		rect.TransformProps.CopyProps( rect );
		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		rect!.Size = Composer.ToLocalSpace( e.ScreenSpaceMousePosition ) - rect.Position;
		rect.TransformProps.CopyProps( rect );
	}
}
