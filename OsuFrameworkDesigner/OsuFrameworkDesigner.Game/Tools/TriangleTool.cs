using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class TriangleTool : ShapeTool<TriangleComponent> {
	protected override TriangleComponent CreateShape ()
		=> new();

	protected override void UpdateShape ( TriangleComponent shape, Vector2 start, Vector2 end ) {
		(shape.X1.Value, shape.Y1.Value) = start;
		var delta = end - start;
		var x = delta.Length / MathF.Sqrt( 3 );
		delta = delta.Normalized();
		(shape.X2.Value, shape.Y2.Value) = end + delta.PerpendicularLeft * x;
		(shape.X3.Value, shape.Y3.Value) = end + delta.PerpendicularRight * x;

		shape.FillColour.Value = shape.Colour;
	}
}
