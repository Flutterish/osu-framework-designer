using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class LineBlueprint : Blueprint<IComponent> {
	new LineComponent Value => (LineComponent)base.Value;

	PointHandle start;
	PointHandle end;
	Handle move;
	public LineBlueprint ( LineComponent value ) : base( value ) {
		AddInternal( move = new Handle().Fill() );
		AddInternal( start = new PointHandle { Anchor = Anchor.CentreLeft, CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );
		AddInternal( end = new PointHandle { Anchor = Anchor.CentreRight, CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );

		start.SnapDragged += e => {
			(Value.StartX.Value, Value.StartY.Value) = e.Position;
		};

		end.SnapDragged += e => {
			(Value.EndX.Value, Value.EndY.Value) = e.Position;
		};

		Vector2 dragHandle = Vector2.Zero;
		Vector2 dragDeltaHandle = Vector2.Zero;
		move.DragStarted += e => {
			dragHandle = new( Value.StartX.Value, Value.StartY.Value );
			dragDeltaHandle = new Vector2( Value.EndX.Value, Value.EndY.Value ) - dragHandle;
		};
		move.Dragged += e => {
			var pos = dragHandle + Composer.ToContentSpace( e.ScreenSpaceMousePosition ) - Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition );
			if ( !e.AltPressed )
				pos = pos.Round();

			Value.StartX.Value = pos.X;
			Value.StartY.Value = pos.Y;
			Value.EndX.Value = pos.X + dragDeltaHandle.X;
			Value.EndY.Value = pos.Y + dragDeltaHandle.Y;
		};
	}

	protected override void Update () {
		base.Update();

		start.TooltipText = $"{Value.StartX.Value:0}, {Value.StartY.Value:0}";
		end.TooltipText = $"{Value.EndX.Value:0}, {Value.EndY.Value:0}";
	}
}
