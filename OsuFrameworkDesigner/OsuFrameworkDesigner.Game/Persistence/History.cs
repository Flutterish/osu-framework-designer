using System.Diagnostics.CodeAnalysis;

namespace OsuFrameworkDesigner.Game.Persistence;

public class History {
	// TODO limit the amount of changes stored (probably a circle buffer)
	List<IChange> changes = new();
	int currentIndex = -1;

	public bool IsLocked;
	public IReadOnlyCollection<IChange> Changes => changes;
	public IChange? LatestChange => currentIndex == -1 ? null : changes[currentIndex];
	public void Push ( IChange change ) {
		if ( IsLocked )
			return;

		removeAfter( currentIndex );
		changes.Add( change );
		currentIndex++;
		ChangeAdded?.Invoke( change );
		NavigatedForward?.Invoke( change );
	}

	public bool Back ( [NotNullWhen( true )] out IChange? change ) {
		if ( IsLocked ) {
			change = null;
			return false;
		}

		if ( currentIndex != -1 ) {
			change = changes[currentIndex];
			currentIndex--;
			NavigatedBack?.Invoke( change );
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
			NavigatedForward?.Invoke( change );
			return true;
		}

		change = null;
		return false;
	}

	void removeAfter ( int index ) {
		while ( changes.Count > index + 1 ) {
			var change = changes[^1];
			if ( change is IDisposable d )
				d.Dispose();

			changes.RemoveAt( changes.Count - 1 );
			ChangeRemoved?.Invoke( change );
		}
	}

	public void NavigateTo ( IChange change ) {
		if ( IsLocked )
			return;

		var index = changes.IndexOf( change );
		while ( index > currentIndex ) {
			Forward( out _ );
		}
		while ( index < currentIndex ) {
			Back( out _ );
		}
	}

	public event Action<IChange>? NavigatedForward;
	public event Action<IChange>? NavigatedBack;

	public event Action<IChange>? ChangeAdded;
	public event Action<IChange>? ChangeRemoved;
}
