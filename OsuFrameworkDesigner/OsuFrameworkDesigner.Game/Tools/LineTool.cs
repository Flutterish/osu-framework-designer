using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class LineTool : ShapeTool<LineComponent> {
	protected override LineComponent CreateShape ()
		=> new();

	protected override void UpdateShape ( LineComponent shape, Vector2 start, Vector2 end ) {
		(shape.StartX.Value, shape.StartY.Value) = start;
		(shape.EndX.Value, shape.EndY.Value) = end;
	}
}
