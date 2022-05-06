using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace OsuFrameworkDesigner.Game.Memory;

public class RingBuffer<T> : IEnumerable<T> {
	T[] buffer;
	int size = 0;
	int index = 0;

	public RingBuffer ( int size ) {
		buffer = new T[size];
	}
	public RingBuffer ( T[] array ) {
		buffer = array;
	}

	public void Push ( T item ) {
		if ( size == Capacity )
			Overflow?.Invoke( buffer[index] );
		else
			size++;

		buffer[index++] = item;
		if ( index == buffer.Length )
			index = 0;
	}

	public bool TryPop ( [NotNullWhen( true )] out T? item ) {
		if ( size == 0 ) {
			item = default;
			return false;
		}

		size--;
		if ( index == 0 )
			index = Capacity;

		item = buffer[--index]!;
		buffer[index] = default!;
		return true;
	}

	public bool TryPeek ( [NotNullWhen( true )] out T? item ) {
		if ( size == 0 ) {
			item = default;
			return false;
		}

		if ( index == 0 )
			item = buffer[Capacity - 1]!;
		else
			item = buffer[index - 1]!;

		return true;
	}

	public T? this[int index] {
		get {
			if ( index >= size || index < 0 )
				return default;

			index = ( this.index - size + index ).Mod( Capacity );

			return buffer[index];
		}
	}

	public int IndexOf ( T item ) {
		for ( int i = 0; i < size; i++ ) {
			if ( this[i]!.Equals( item ) )
				return i;
		}

		return -1;
	}

	public int Capacity => buffer.Length;
	public int Count => size;

	public event Action<T>? Overflow;

	public IEnumerator<T> GetEnumerator () {
		int index = this.index - size;
		if ( index < 0 )
			index += Capacity;

		for ( int i = 0; i < size; i++ ) {
			yield return buffer[index++];
			if ( index == Capacity )
				index = 0;
		}
	}

	IEnumerator IEnumerable.GetEnumerator ()
		=> ( this as IEnumerable<T> ).GetEnumerator();
}
