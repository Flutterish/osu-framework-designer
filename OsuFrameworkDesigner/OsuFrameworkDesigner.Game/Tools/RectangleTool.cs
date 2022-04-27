using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class RectangleTool : ShapeTool<RectangleComponent> {
	protected override RectangleComponent CreateShape ()
		=> new();

	protected override void UpdateShape ( RectangleComponent shape, Vector2 start, Vector2 end ) {
		shape.TransformProps.X.Value = start.X;
		shape.TransformProps.Y.Value = start.Y;
		shape.TransformProps.Width.Value = end.X - start.X;
		shape.TransformProps.Height.Value = end.Y - start.Y;

		shape.TransformProps.CopyProps( shape );
	}
}
