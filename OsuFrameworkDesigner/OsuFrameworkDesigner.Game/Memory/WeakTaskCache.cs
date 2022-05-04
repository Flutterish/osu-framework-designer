using System.Threading.Tasks;

namespace OsuFrameworkDesigner.Game.Memory;

/// <summary>
/// A dictionary cache where each entry is a weak reference to the result of the task
/// </summary>
public abstract class WeakTaskCache<Tkey, T> where Tkey : notnull where T : class {
	Dictionary<Tkey, Task<T>> runningTasks = new();
	Dictionary<Tkey, WeakReference<T>> cache = new();

	protected abstract Task<T> PerformAsync ( Tkey key );
	public async Task<T> GetAsync ( Tkey key ) {
		T? value;
		if ( cache.TryGetValue( key, out var @ref ) ) {
			if ( @ref.TryGetTarget( out value ) )
				return value;
			else
				cache.Remove( key );
		}

		Task<T>? task;
		if ( runningTasks.TryGetValue( key, out task ) )
			return await task;

		task = PerformAsync( key );
		runningTasks.Add( key, task );
		value = await task;
		cache.Add( key, new WeakReference<T>( value ) );
		runningTasks.Remove( key );
		return value;
	}

	protected virtual T PerformSync ( Tkey key )
		=> PerformAsync( key ).Result;
	public T Get ( Tkey key ) {
		T? value;
		if ( cache.TryGetValue( key, out var @ref ) ) {
			if ( @ref.TryGetTarget( out value ) )
				return value;
			else
				cache.Remove( key );
		}

		Task<T>? task;
		if ( runningTasks.TryGetValue( key, out task ) )
			return task.Result;

		value = PerformSync( key );
		cache.Add( key, new WeakReference<T>( value ) );
		return value;
	}
}
