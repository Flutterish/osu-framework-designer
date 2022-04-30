using System.Buffers;
using System.Collections;

namespace OsuFrameworkDesigner.Game.Memory;

public sealed class MemoryPool<T> {
	ArrayPool<T> backing;

	public MemoryPool ( ArrayPool<T> backing ) {
		this.backing = backing;
	}

	public RentedArray<T> Rent ( int length )
		=> new( backing, length );

	public RentedArray<T> Rent ( ICollection<T> source ) {
		var arr = new RentedArray<T>( backing, source.Count );
		source.CopyTo( arr.Array, arr.Length );
		return arr;
	}

	public RentedArray<T> Rent ( IEnumerable<T> source ) {
		var arr = source.TryGetNonEnumeratedCount( out int count )
			? Rent( count )
			: Rent( source.Count() );

		int i = 0;
		foreach ( var k in source ) {
			arr[i++] = k;
		}

		return arr;
	}

	public static MemoryPool<T> Shared { get; } = new( ArrayPool<T>.Shared );
}

public struct RentedArray<T> : IDisposable, IEnumerable<T> {
	ArrayPool<T> backing;
	public readonly T[] Array;
	public readonly int Length;

	public RentedArray ( ArrayPool<T> backing, int length ) {
		this.backing = backing;
		Length = length;
		Array = backing.Rent( length );
	}

	public Span<T> AsSpan () => Array.AsSpan( 0, Length );
	public Span<T> AsSpan ( int start, int length ) => AsSpan().Slice( start, length );
	public Span<T>.Enumerator GetEnumerator () => AsSpan().GetEnumerator();

	public ref T this[int i] => ref Array[i];
	public ref T this[Index i] => ref Array[i];

	public void Dispose () {
		backing.Return( Array );
	}

	public void TryDispose () {
		if ( backing != null )
			Dispose();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator () {
		for ( int i = 0; i < Length; i++ ) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator () {
		for ( int i = 0; i < Length; i++ ) {
			yield return this[i];
		}
	}

	public static implicit operator Span<T> ( RentedArray<T> arr )
		=> arr.AsSpan();

	public static implicit operator ReadOnlySpan<T> ( RentedArray<T> arr )
		=> arr.AsSpan();
}