using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class LineBlueprint : Blueprint<IComponent> {
	new LineComponent Value => (LineComponent)base.Value;

	PointHandle start;
	PointHandle end;
	public LineBlueprint ( LineComponent value ) : base( value ) {
		AddInternal( start = new PointHandle { Anchor = Anchor.CentreLeft, CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );
		AddInternal( end = new PointHandle { Anchor = Anchor.CentreRight, CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );

		start.Dragged += e => {
			var pos = Composer.ToContentSpace( e.ScreenSpaceMousePosition );
			(Value.StartX.Value, Value.StartY.Value) = e.AltPressed ? pos : pos.Round();
		};

		end.Dragged += e => {
			var pos = Composer.ToContentSpace( e.ScreenSpaceMousePosition );
			(Value.EndX.Value, Value.EndY.Value) = e.AltPressed ? pos : pos.Round();
		};
	}
}
