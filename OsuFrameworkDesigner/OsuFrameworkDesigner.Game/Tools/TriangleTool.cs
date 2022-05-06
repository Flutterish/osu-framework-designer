using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class TriangleTool : ShapeTool<TriangleComponent> {
	protected override TriangleComponent CreateShape ()
		=> new();

	protected override void UpdateShape ( TriangleComponent shape, Vector2 start, Vector2 end ) {
		shape.PointA.Value = start;
		var delta = end - start;
		var x = delta.Length / MathF.Sqrt( 3 );
		delta = delta.Normalized();
		shape.PointB.Value = end + delta.PerpendicularLeft * x;
		shape.PointC.Value = end + delta.PerpendicularRight * x;

		shape.FillColour.Value = shape.Colour;
	}
}
