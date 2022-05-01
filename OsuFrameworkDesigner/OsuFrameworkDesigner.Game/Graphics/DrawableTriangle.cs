using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;

namespace OsuFrameworkDesigner.Game.Graphics;

public class DrawableTriangle : Drawable, IConvexPolygon {
	Vector2 a;
	Vector2 b;
	Vector2 c;

	public Vector2 PointA {
		get => a;
		set {
			if ( a == value )
				return;

			a = value;
			Invalidate( Invalidation.DrawNode | Invalidation.MiscGeometry );
		}
	}
	public Vector2 PointB {
		get => b;
		set {
			if ( a == value )
				return;

			b = value;
			Invalidate( Invalidation.DrawNode | Invalidation.MiscGeometry );
		}
	}
	public Vector2 PointC {
		get => c;
		set {
			if ( a == value )
				return;

			c = value;
			Invalidate( Invalidation.DrawNode | Invalidation.MiscGeometry );
		}
	}

	public override Quad ScreenSpaceDrawQuad {
		get {
			var minX = MathF.Min( MathF.Min( a.X, b.X ), c.X );
			var maxX = MathF.Max( MathF.Max( a.X, b.X ), c.X );
			var minY = MathF.Min( MathF.Min( a.Y, b.Y ), c.Y );
			var maxY = MathF.Max( MathF.Max( a.Y, b.Y ), c.Y );

			return ToScreenSpace( new RectangleF( minX, minY, maxX - minX, maxY - minY ) );
		}
	}

	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> !maskingBounds.IntersectsWith( ScreenSpaceDrawQuad.AABBFloat );

	IShader shader = null!;
	[BackgroundDependencyLoader]
	private void load ( ShaderManager shaders ) {
		shader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
	}

	Vector2[] vertices = new Vector2[3];
	public ReadOnlySpan<Vector2> GetAxisVertices ()
		=> GetVertices();

	public ReadOnlySpan<Vector2> GetVertices () {
		vertices[0] = ToScreenSpace( a );
		vertices[1] = ToScreenSpace( b );
		vertices[2] = ToScreenSpace( c );

		return vertices;
	}

	public override bool Contains ( Vector2 screenSpacePos )
		=> this.Intersects( new Quad( screenSpacePos, screenSpacePos, screenSpacePos, screenSpacePos ) );

	protected override DrawNode CreateDrawNode ()
		=> new TriangleDrawNode( this );

	protected class TriangleDrawNode : DrawNode {
		new DrawableTriangle Source => (DrawableTriangle)base.Source;

		public TriangleDrawNode ( DrawableTriangle source ) : base( source ) { }

		protected Vector2 A;
		protected Vector2 B;
		protected Vector2 C;
		protected IShader Shader = null!;
		public override void ApplyState () {
			base.ApplyState();

			A = Source.ToScreenSpace( Source.a );
			B = Source.ToScreenSpace( Source.b );
			C = Source.ToScreenSpace( Source.c );
			Shader = Source.shader;
		}

		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			base.Draw( vertexAction );

			Shader.Bind();

			DrawQuad( Texture.WhitePixel, new Quad( A, A, B, C ), DrawColourInfo.Colour );

			Shader.Unbind();
		}
	}
}
