namespace OsuFrameworkDesigner.Game.Persistence;

public interface IChange {
	void Undo ();
	void Redo ();
}

public abstract record Change<T> : IChange where T : notnull {
	public T Target { get; init; } = default!;

	public abstract void Undo ();
	public abstract void Redo ();
}
