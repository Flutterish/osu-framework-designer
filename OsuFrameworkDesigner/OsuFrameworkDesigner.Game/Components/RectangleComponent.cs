using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class RectangleComponent : Box, IComponent {
	public readonly TransformProps TransformProps;

	public RectangleComponent () {
		TransformProps = new( this );
	}

	public Blueprint<IComponent> CreateBlueprint ()
		=> new BasicTransformBlueprint<RectangleComponent>( this, TransformProps );
	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties => TransformProps;

}
