using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Containers;
using OsuFrameworkDesigner.Game.Memory;

namespace OsuFrameworkDesigner.Game.Persistence;

public record ComponentChange : Change<IComponent> {
	public Composer Composer { get; init; } = null!;
	public ChangeType Type { get; init; }

	public override void Undo () {
		if ( Type is ChangeType.Added )
			Composer.Remove( Target );
		else
			Composer.Add( Target );
	}

	public override void Redo () {
		if ( Type is ChangeType.Removed )
			Composer.Remove( Target );
		else
			Composer.Add( Target );
	}
}

public record ComponentsChange : Change<RentedArray<IComponent>>, IDisposable {
	public Composer Composer { get; init; } = null!;
	public ChangeType Type { get; init; }

	public override void Undo () {
		if ( Type is ChangeType.Added )
			Composer.RemoveRange( Target );
		else
			Composer.AddRange( Target );
	}

	public override void Redo () {
		if ( Type is ChangeType.Removed )
			Composer.RemoveRange( Target );
		else
			Composer.AddRange( Target );
	}

	public void Dispose () {
		Target.Dispose();
	}
}

public enum ChangeType {
	Added,
	Removed
}