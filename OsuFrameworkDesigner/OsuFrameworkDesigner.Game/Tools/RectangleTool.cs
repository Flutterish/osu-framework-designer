using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class RectangleTool : ShapeTool<RectangleComponent> {
	protected override RectangleComponent CreateShape ()
		=> new();
	protected override void UpdateShape ( RectangleComponent shape ) {
		shape.TransformProps.CopyProps( shape );
	}
}
