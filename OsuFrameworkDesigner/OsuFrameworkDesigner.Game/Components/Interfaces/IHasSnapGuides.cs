namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasSnapGuides {
	IEnumerable<PointGuide> PointGuides { get; }
	IEnumerable<LineGuide> LineGuides { get; }
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