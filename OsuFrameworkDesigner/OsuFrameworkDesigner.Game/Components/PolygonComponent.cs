using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Components;

public class PolygonComponent : CompositeDrawable, IComponent, IHasSnapGuides {
	public static readonly PropDescription CornerCountProto = PropDescriptions.IntProp with { Name = "Count", Category = "Corners" };
	public readonly PolygonDrawable Polygon;

	public readonly DrawableProps TransformProps;
	new public readonly Prop<float> CornerRadius = new( PropDescriptions.CornerRadius );
	public readonly ClampedProp<int> CornerCount = new( 3, CornerCountProto ) { MinValue = 3, MaxValue = 60 };

	public PolygonComponent () {
		TransformProps = new( this );
		AddInternal( Polygon = new PolygonDrawable().Fill() );

		CornerRadius.BindValueChanged( v => Polygon.CornerRadius = v.NewValue );
		CornerCount.BindValueChanged( v => Polygon.SideCount = v.NewValue );
	}

	public override bool Contains ( Vector2 screenSpacePos )
		=> InternalChild.Contains( screenSpacePos );

	public Blueprint<IComponent> CreateBlueprint ()
		=> new PolygonBlueprint();
	string IComponent.Name { get => Name; set => Name = value; }
	public IEnumerable<IProp> Properties => TransformProps.Append( CornerRadius ).Append( CornerCount );

	public IEnumerable<PointGuide> PointGuides {
		get {
			var quad = TransformProps.ContentSpaceQuad;
			var matrix = quad.AsMatrix();

			yield return quad.Centre;

			var centre = new Vector2( 0.5f );
			var vertex = new Vector2( 0, -0.5f );
			var theta = MathF.Tau / CornerCount;
			for ( int i = 0; i < CornerCount; i++ ) {
				yield return Vector2Extensions.Transform( centre + vertex, matrix );
				vertex = vertex.Rotate( theta );
			}
		}
	}
	public IEnumerable<LineGuide> LineGuides {
		get {
			var quad = TransformProps.ContentSpaceQuad;
			var matrix = quad.AsMatrix();

			var centre = new Vector2( 0.5f );
			var vertex = new Vector2( 0, -0.5f );
			var theta = MathF.Tau / CornerCount;
			var last = Vector2Extensions.Transform( centre + vertex, matrix );
			for ( int i = 0; i < CornerCount; i++ ) {
				vertex = vertex.Rotate( theta );
				var next = Vector2Extensions.Transform( centre + vertex, matrix );
				yield return new() { StartPoint = last, EndPoint = next };
				last = next;
			}
		}
	}
}
