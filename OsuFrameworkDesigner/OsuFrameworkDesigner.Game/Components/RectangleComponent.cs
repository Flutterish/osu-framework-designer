using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class RectangleComponent : Box, IComponent {
	public readonly TransformProps TransformProps;

	public RectangleComponent () {
		TransformProps = new( this );
	}

	public IEnumerable<IProp> Properties => TransformProps;
}
