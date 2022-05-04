using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Graphics.Selections;

namespace OsuFrameworkDesigner.Game.Components;

public class TriangleComponent : CompositeDrawable, IComponent, IHasMatrix, IHasSnapGuides, IHasCustomSelection {
	public static readonly PropDescription X1Proto = PropDescriptions.FloatProp with { Name = "X", Category = "Vertice 1" };
	public static readonly PropDescription Y1Proto = PropDescriptions.FloatProp with { Name = "Y", Category = "Vertice 1" };
	public static readonly PropDescription X2Proto = PropDescriptions.FloatProp with { Name = "X", Category = "Vertice 2" };
	public static readonly PropDescription Y2Proto = PropDescriptions.FloatProp with { Name = "Y", Category = "Vertice 2" };
	public static readonly PropDescription X3Proto = PropDescriptions.FloatProp with { Name = "X", Category = "Vertice 3" };
	public static readonly PropDescription Y3Proto = PropDescriptions.FloatProp with { Name = "Y", Category = "Vertice 3" };

	DrawableTriangle triangle;

	public readonly Prop<float> X1 = new( X1Proto );
	public readonly Prop<float> Y1 = new( Y1Proto );
	public readonly Prop<float> X2 = new( X2Proto );
	public readonly Prop<float> Y2 = new( Y2Proto );
	public readonly Prop<float> X3 = new( X3Proto );
	public readonly Prop<float> Y3 = new( Y3Proto );
	public readonly Prop<Colour4> FillColour = new( Colour4.Green, PropDescriptions.FillColour );

	public Vector2 PointA => new( X1, Y1 );
	public Vector2 PointB => new( X2, Y2 );
	public Vector2 PointC => new( X3, Y3 );

	public TriangleComponent () {
		AddInternal( triangle = new DrawableTriangle().Fill() );

		FillColour.ValueChanged += v => Colour = v.NewValue;

		(X1, Y1).BindValueChanged( ( x, y ) => {
			Position = new( x, y );
			triangle.PointB = new Vector2( X2, Y2 ) - Position;
			triangle.PointC = new Vector2( X3, Y3 ) - Position;
		} );
		(X2, Y2).BindValueChanged( ( x, y ) => {
			triangle.PointB = new Vector2( x, y ) - Position;
		} );
		(X3, Y3).BindValueChanged( ( x, y ) => {
			triangle.PointC = new Vector2( x, y ) - Position;
		} );
	}

	public override Quad ScreenSpaceDrawQuad
		=> triangle.ScreenSpaceDrawQuad;

	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> !maskingBounds.IntersectsWith( ScreenSpaceDrawQuad.AABBFloat );

	public override bool Contains ( Vector2 screenSpacePos )
		=> triangle.Contains( screenSpacePos );

	public Blueprint<IComponent> CreateBlueprint ()
		=> new TriangleBlueprint();

	string IComponent.Name { get => Name; set => Name = value; }
	public IEnumerable<IProp> Properties {
		get {
			yield return X1;
			yield return Y1;
			yield return X2;
			yield return Y2;
			yield return X3;
			yield return Y3;
			yield return FillColour;
		}
	}

	public IEnumerable<PointGuide> PointGuides {
		get {
			yield return ( PointA + PointB + PointC ) / 3;

			yield return PointA;
			yield return PointB;
			yield return PointC;
		}
	}
	public IEnumerable<LineGuide> LineGuides {
		get {
			yield return new() { StartPoint = PointA, EndPoint = PointB };
			yield return new() { StartPoint = PointB, EndPoint = PointC };
			yield return new() { StartPoint = PointC, EndPoint = PointA };
		}
	}

	public Matrix3 Matrix {
		get {
			var minX = MathF.Min( MathF.Min( X1, X2 ), X3 );
			var maxX = MathF.Max( MathF.Max( X1, X2 ), X3 );
			var minY = MathF.Min( MathF.Min( Y1, Y2 ), Y3 );
			var maxY = MathF.Max( MathF.Max( Y1, Y2 ), Y3 );

			return new Quad( minX, minY, maxX - minX, maxY - minY ).AsMatrix();
		}
		set {
			var minX = MathF.Min( MathF.Min( X1, X2 ), X3 );
			var maxX = MathF.Max( MathF.Max( X1, X2 ), X3 );
			var minY = MathF.Min( MathF.Min( Y1, Y2 ), Y3 );
			var maxY = MathF.Max( MathF.Max( Y1, Y2 ), Y3 );

			(X1.Value, Y1.Value) = Vector2Extensions.Transform( new Vector2( ( X1 - minX ) / ( maxX - minX ), ( Y1 - minY ) / ( maxY - minY ) ), value );
			(X2.Value, Y2.Value) = Vector2Extensions.Transform( new Vector2( ( X2 - minX ) / ( maxX - minX ), ( Y2 - minY ) / ( maxY - minY ) ), value );
			(X3.Value, Y3.Value) = Vector2Extensions.Transform( new Vector2( ( X3 - minX ) / ( maxX - minX ), ( Y3 - minY ) / ( maxY - minY ) ), value );
		}
	}

	public DrawableSelection CreateSelection ()
		=> new TriangleSelection();
}
