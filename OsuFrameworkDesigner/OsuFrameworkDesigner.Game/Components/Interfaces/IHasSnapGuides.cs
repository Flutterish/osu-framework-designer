using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasSnapGuides {
	IEnumerable<PointGuide> PointGuides { get; }
	IEnumerable<LineGuide> LineGuides { get; }

	public static IEnumerable<PointGuide> PointGuidesFrom ( IComponent component, Composer composer ) {
		if ( component is IHasSnapGuides s ) {
			foreach ( var i in s.PointGuides )
				yield return i;
		}

		if ( component is Drawable d ) {
			var quad = d.ScreenSpaceDrawQuad;
			yield return composer.ToContentSpace( quad.Centre );
			yield return composer.ToContentSpace( quad.TopLeft );
			yield return composer.ToContentSpace( quad.BottomLeft );
			yield return composer.ToContentSpace( quad.TopRight );
			yield return composer.ToContentSpace( quad.BottomRight );

			yield return composer.ToContentSpace( ( quad.TopLeft + quad.TopRight ) / 2 );
			yield return composer.ToContentSpace( ( quad.TopLeft + quad.BottomLeft ) / 2 );
			yield return composer.ToContentSpace( ( quad.BottomRight + quad.TopRight ) / 2 );
			yield return composer.ToContentSpace( ( quad.BottomRight + quad.BottomLeft ) / 2 );
		}
	}
}

public struct LineGuide {
	public Vector2 StartPoint;
	public Vector2 EndPoint;
	public Vector2 Direction => ( EndPoint - StartPoint ).Normalized();
}

public struct PointGuide {
	public Vector2 Point;

	public bool IsInRange ( Vector2 p )
		=> ( Point - p ).LengthSquared <= 100;

	public static implicit operator PointGuide ( Vector2 p )
		=> new() { Point = p };
}