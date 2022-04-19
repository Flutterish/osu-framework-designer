using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public abstract class Blueprint<T> : CompositeDrawable {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	public T Value { get; private set; }

	protected Blueprint ( T value ) {
		Value = value;
	}
}