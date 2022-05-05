using System.Diagnostics.CodeAnalysis;

namespace OsuFrameworkDesigner.Game.Persistence;

public class History {
	// TODO limit the amount of changes stored (probably a circle buffer)
	List<IChange> changes = new();
	int currentIndex = -1;

	public bool IsLocked;
	public IReadOnlyCollection<IChange> Changes => changes;
	public void Push ( IChange change ) {
		if ( IsLocked )
			return;

		removeAfter( currentIndex );
		changes.Add( change );
		currentIndex++;
		ChangePushed?.Invoke( change );
	}

	public bool Back ( [NotNullWhen( true )] out IChange? change ) {
		if ( IsLocked ) {
			change = null;
			return false;
		}

		if ( currentIndex != -1 ) {
			change = changes[currentIndex];
			currentIndex--;
			ChangePopped?.Invoke( change );
			return true;
		}

		change = null;
		return false;
	}

	public bool Forward ( [NotNullWhen( true )] out IChange? change ) {
		if ( IsLocked ) {
			change = null;
			return false;
		}

		if ( currentIndex != changes.Count - 1 ) {
			currentIndex++;
			change = changes[currentIndex];
			ChangePushed?.Invoke( change );
			return true;
		}

		change = null;
		return false;
	}

	void removeAfter ( int index ) {
		while ( changes.Count > index + 1 ) {
			if ( changes[^1] is IDisposable d )
				d.Dispose();

			changes.RemoveAt( changes.Count - 1 );
		}
	}

	public event Action<IChange>? ChangePushed;
	public event Action<IChange>? ChangePopped;
}
