using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class LineBlueprint : Blueprint<IComponent> {
	new LineComponent Value => (LineComponent)base.Value;

	PointHandle start;
	PointHandle end;
	Handle move;
	public LineBlueprint () {
		AddInternal( move = new Handle().Fill() );
		AddInternal( start = new PointHandle { Anchor = Anchor.CentreLeft, CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );
		AddInternal( end = new PointHandle { Anchor = Anchor.CentreRight, CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );

		start.SnapDragged += e => {
			Value.Start.Value = e.Position;
		};

		end.SnapDragged += e => {
			Value.End.Value = e.Position;
		};

		Vector2 dragDeltaHandle = Vector2.Zero;
		move.HandleSnappedTranslate( ( lines, points ) => {
			var from = Value.Start.Value;
			var to = Value.End.Value;
			points.Add( new() { Point = from, SnapToLines = true } );
			points.Add( new() { Point = to, SnapToLines = true } );
			lines.Add( new() { StartPoint = from, EndPoint = to } );

			dragDeltaHandle = to - from;
			return from;
		}, position => {
			Value.Start.Value = position;
			Value.End.Value = position + dragDeltaHandle;
		} );
	}

	protected override void Update () {
		base.Update();

		start.TooltipText = $"{Value.Start.Value.X:0}, {Value.Start.Value.Y:0}";
		end.TooltipText = $"{Value.End.Value.X:0}, {Value.End.Value.Y:0}";
	}
}
