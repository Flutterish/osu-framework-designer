using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Memory;

namespace OsuFrameworkDesigner.Game.Persistence;

public interface IPropChange : IChange {
	IProp Target { get; }
}

public record PropChange<T> : Change<IProp<T>>, IPropChange {
	IProp IPropChange.Target => Target;
	public T PreviousValue { get; init; } = default!;
	public T NextValue { get; init; } = default!;

	public override void Undo ()
		=> Target.Value = PreviousValue;

	public override void Redo ()
		=> Target.Value = NextValue;
}

public record PropChange : Change<IProp>, IPropChange {
	public object? PreviousValue { get; init; }
	public object? NextValue { get; init; }

	public override void Undo ()
		=> Target.Value = PreviousValue;

	public override void Redo ()
		=> Target.Value = NextValue;
}

public record PropsChange : Change<RentedArray<IPropChange>>, IDisposable {
	public override void Undo () {
		foreach ( var change in Target ) {
			change.Undo();
		}
	}

	public override void Redo () {
		foreach ( var change in Target ) {
			change.Redo();
		}
	}

	public void Dispose () {
		Target.Dispose();
	}
}
