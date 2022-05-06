using osu.Framework.Graphics.Textures;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Graphics.Selections;

namespace OsuFrameworkDesigner.Game.Components;

public class TriangleComponent : CompositeDrawable, IComponent, IHasMatrix, IHasSnapGuides, IHasCustomSelection {
	public static readonly PropDescription V1Proto = PropDescriptions.Vector2Prop with { Name = "Vertice 1", Category = "Triangle" };
	public static readonly PropDescription V2Proto = PropDescriptions.Vector2Prop with { Name = "Vertice 2", Category = "Triangle" };
	public static readonly PropDescription V3Proto = PropDescriptions.Vector2Prop with { Name = "Vertice 3", Category = "Triangle" };

	DrawableTriangle triangle;

	public readonly Prop<Vector2> PointA = new( V1Proto );
	public readonly Prop<Vector2> PointB = new( V2Proto );
	public readonly Prop<Vector2> PointC = new( V3Proto );
	public readonly Prop<Colour4> FillColour = new( Colour4.Green, PropDescriptions.FillColour );
	public readonly Prop<Texture> Texture = new( PropDescriptions.Texture );

	public TriangleComponent () {
		AddInternal( triangle = new DrawableTriangle().Fill() );

		FillColour.ValueChanged += v => Colour = v.NewValue;

		PointA.BindValueChanged( v => {
			Position = PointA;
			triangle.PointB = PointB - Position;
			triangle.PointC = PointC - Position;
		} );
		PointB.BindValueChanged( v => {
			triangle.PointB = PointB - Position;
		} );
		PointC.BindValueChanged( v => {
			triangle.PointC = PointC - Position;
		} );

		Texture.BindValueChanged( v => triangle.Texture = v.NewValue );
		Name.BindValueChanged( x => base.Name = x.NewValue, true );
	}

	public override Quad ScreenSpaceDrawQuad
		=> triangle.ScreenSpaceDrawQuad;

	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> !maskingBounds.IntersectsWith( ScreenSpaceDrawQuad.AABBFloat );

	public override bool Contains ( Vector2 screenSpacePos )
		=> triangle.Contains( screenSpacePos );

	public Blueprint<IComponent> CreateBlueprint ()
		=> new TriangleBlueprint();

	new public IProp<string> Name { get; } = new Prop<string>( "Triangle", PropDescriptions.Name );
	public IEnumerable<IProp> Properties {
		get {
			yield return PointA;
			yield return PointB;
			yield return PointC;
			yield return FillColour;
			yield return Texture;
		}
	}

	public IEnumerable<PointGuide> PointGuides {
		get {
			yield return ( PointA.Value + PointB + PointC ) / 3;

			yield return PointA.Value;
			yield return PointB.Value;
			yield return PointC.Value;
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
			return new Quad(
				PointA.Value,
				PointB.Value,
				Vector2.Zero,
				PointC.Value
			).AsMatrix();
		}
		set {
			PointA.Value = Vector2Extensions.Transform( new Vector2( 0, 0 ), value );
			PointB.Value = Vector2Extensions.Transform( new Vector2( 1, 0 ), value );
			PointC.Value = Vector2Extensions.Transform( new Vector2( 1, 1 ), value );
		}
	}

	public DrawableSelection CreateSelection ()
		=> new TriangleSelection();
}
