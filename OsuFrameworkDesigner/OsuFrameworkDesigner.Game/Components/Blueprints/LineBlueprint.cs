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

		Vector2 dragDeltaHandle = Vector2.Zero;
		move.HandleSnappedTranslate( ( lines, points ) => {
			var from = new Vector2( value.StartX, value.StartY );
			var to = new Vector2( Value.EndX, Value.EndY );
			points.Add( new() { Point = from, SnapToLines = true } );
			points.Add( new() { Point = to, SnapToLines = true } );
			lines.Add( new() { StartPoint = from, EndPoint = to } );

			dragDeltaHandle = to - from;
			return from;
		}, position => {
			Value.StartX.Value = position.X;
			Value.StartY.Value = position.Y;
			Value.EndX.Value = position.X + dragDeltaHandle.X;
			Value.EndY.Value = position.Y + dragDeltaHandle.Y;
		} );
	}

	protected override void Update () {
		base.Update();

		start.TooltipText = $"{Value.StartX.Value:0}, {Value.StartY.Value:0}";
		end.TooltipText = $"{Value.EndX.Value:0}, {Value.EndY.Value:0}";
	}
}
