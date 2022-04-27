using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class CircleTool : ShapeTool<CircleComponent> {
	protected override CircleComponent CreateShape ()
		=> new();

	protected override void UpdateShape ( CircleComponent shape, Vector2 start, Vector2 end ) {
		shape.TransformProps.X.Value = start.X;
		shape.TransformProps.Y.Value = start.Y;

		var r = ( end - start ).Length * 2;
		shape.TransformProps.Width.Value = r;
		shape.TransformProps.Height.Value = r;

		shape.TransformProps.CopyProps( shape );
	}
}
