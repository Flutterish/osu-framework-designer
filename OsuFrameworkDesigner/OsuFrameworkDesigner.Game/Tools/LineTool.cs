using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class LineTool : ShapeTool<LineComponent> {
	protected override LineComponent CreateShape ()
		=> new();

	protected override void UpdateShape ( LineComponent shape, Vector2 start, Vector2 end ) {
		shape.Start.Value = start;
		shape.End.Value = end;

		shape.FillColour.Value = shape.Colour;
	}
}
