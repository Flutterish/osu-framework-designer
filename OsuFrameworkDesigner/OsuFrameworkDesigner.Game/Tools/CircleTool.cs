using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class CircleTool : ShapeTool<CircleComponent> {
	protected override CircleComponent CreateShape ()
		=> new();
	protected override void UpdateShape ( CircleComponent shape ) {
		shape.TransformProps.CopyProps( shape );
	}

	protected override Vector2 SizeAt ( Vector2 cursorDelta ) {
		return new( cursorDelta.Length * 2 );
	}
}
