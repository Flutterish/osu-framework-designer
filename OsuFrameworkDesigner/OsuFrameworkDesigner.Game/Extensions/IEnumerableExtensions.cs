namespace OsuFrameworkDesigner.Game.Extensions;

public static class IEnumerableExtensions {
	public static RectangleF GetBoundingBox<T> ( this IEnumerable<T> self, Func<T, RectangleF> func ) {
		var rect = func( self.First() );
		var minX = rect.Left;
		var maxX = rect.Right;
		var minY = rect.Top;
		var maxY = rect.Bottom;

		foreach ( var i in self.Skip( 1 ).Select( func ) ) {
			if ( i.Left < minX )
				minX = i.Left;
			if ( i.Top < minY )
				minY = i.Top;
			if ( i.Right > maxX )
				maxX = i.Right;
			if ( i.Bottom > maxY )
				maxY = i.Bottom;
		}

		return new RectangleF( minX, minY, maxX - minX, maxY - minY );
	}
}
