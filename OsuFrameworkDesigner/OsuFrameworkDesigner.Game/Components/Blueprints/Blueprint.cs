namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public abstract class Blueprint<T> : CompositeDrawable {
	public T Value { get; private set; }

	protected Blueprint ( T value ) {
		Value = value;
	}
}