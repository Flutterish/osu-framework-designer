using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasSnapGuides {
	IEnumerable<PointGuide> PointGuides { get; }
	IEnumerable<LineGuide> LineGuides { get; }

	public static IEnumerable<PointGuide> PointGuidesFrom ( IComponent component, Composer composer ) {
		if ( component is IHasSnapGuides s )
			return s.PointGuides;

		if ( component is Drawable d )
			return PointGuidesFrom( composer.ToContentSpace( d.ScreenSpaceDrawQuad ) );

		return Array.Empty<PointGuide>();
	}

	public static IEnumerable<PointGuide> PointGuidesFrom ( Quad quad ) {
		yield return quad.Centre;
		yield return quad.TopLeft;
		yield return quad.BottomLeft;
		yield return quad.TopRight;
		yield return quad.BottomRight;

		yield return ( quad.TopLeft + quad.TopRight ) / 2;
		yield return ( quad.TopLeft + quad.BottomLeft ) / 2;
		yield return ( quad.BottomRight + quad.TopRight ) / 2;
		yield return ( quad.BottomRight + quad.BottomLeft ) / 2;
	}

	public static IEnumerable<LineGuide> LineGuidesFrom ( IComponent component, Composer composer ) {
		if ( component is IHasSnapGuides s )
			return s.LineGuides;

		if ( component is Drawable d )
			return LineGuidesFrom( composer.ToContentSpace( d.ScreenSpaceDrawQuad ) );

		return Array.Empty<LineGuide>();
	}

	public static IEnumerable<LineGuide> LineGuidesFrom ( Quad quad ) {
		yield return new() {
			StartPoint = ( quad.TopLeft + quad.BottomLeft ) / 2,
			EndPoint = ( quad.TopRight + quad.BottomRight ) / 2
		};
		yield return new() {
			StartPoint = ( quad.TopLeft + quad.TopRight ) / 2,
			EndPoint = ( quad.BottomLeft + quad.BottomRight ) / 2
		};

		yield return new() {
			StartPoint = quad.TopLeft,
			EndPoint = quad.TopRight
		};
		yield return new() {
			StartPoint = quad.BottomLeft,
			EndPoint = quad.BottomRight
		};
		yield return new() {
			StartPoint = quad.TopLeft,
			EndPoint = quad.BottomLeft
		};
		yield return new() {
			StartPoint = quad.TopRight,
			EndPoint = quad.BottomRight
		};
	}
}

public struct LineGuide {
	public Vector2 StartPoint;
	public Vector2 EndPoint;
	public Vector2 Direction => EndPoint - StartPoint;

	public float SnapRatingFor ( Vector2 point, out Vector2 snapped ) {
		snapped = MathExtensions.ClosestPointToLine( StartPoint, EndPoint - StartPoint, point );
		return ( snapped - point ).LengthSquared;
	}

	public override string ToString ()
		=> $"{StartPoint} -> {EndPoint}";
}

public struct PointGuide {
	public Vector2 Point;

	public float SnapRatingFor ( Vector2 p )
		=> ( Point - p ).LengthSquared;

	public override string ToString ()
		=> Point.ToString();

	public static implicit operator PointGuide ( Vector2 p )
		=> new() { Point = p };
}