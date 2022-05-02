using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using OsuFrameworkDesigner.Game.Memory;
using osuTK.Graphics.ES30;

namespace OsuFrameworkDesigner.Game.Graphics;

public class PolygonDrawable : Drawable, IConvexPolygon {
	int sideCount = 3;
	public int SideCount {
		get => sideCount;
		set {
			if ( sideCount == value || value < 3 )
				return;

			sideCount = value;
			Invalidate( Invalidation.DrawNode | Invalidation.MiscGeometry );
		}
	}

	float cornerRadius = 0;
	public float CornerRadius {
		get => cornerRadius;
		set {
			if ( cornerRadius == value )
				return;

			cornerRadius = value;
			Invalidate( Invalidation.DrawNode | Invalidation.MiscGeometry );
		}
	}

	public float MaxSize => MathF.Max( DrawWidth.Abs(), DrawHeight.Abs() );
	public float CornerRadiusAtDistance ( float distance ) {
		var vertex = new Vector2( 0, -0.5f );
		var theta = MathF.Tau / sideCount;

		var vertexDelta = vertex.Rotate( theta ) - vertex;

		return MathExtensions.RoundedTriangleRadius( -vertex.Y, -vertexDelta.Y / vertexDelta.X, distance / MaxSize ) * MaxSize;
	}

	public float CornerCentreDistance {
		get {
			var vertex = new Vector2( 0, -0.5f );
			var theta = MathF.Tau / sideCount;

			var vertexDelta = vertex.Rotate( theta ) - vertex;

			var (h, r, a) = MathExtensions.RoundTriangle( -vertex.Y, -vertexDelta.Y / vertexDelta.X, cornerRadius / MaxSize );
			return h * MaxSize * 2;
		}
	}

	public float MaxCornerRadius => CornerRadiusAtDistance( 0 );

	IShader shader = null!;

	[BackgroundDependencyLoader]
	private void load ( ShaderManager shaders ) {
		shader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
	}

	public ReadOnlySpan<Vector2> GetAxisVertices ()
		=> GetVertices();

	RentedArray<Vector2> verticesArray;
	public ReadOnlySpan<Vector2> GetVertices () {
		if ( verticesArray.Length != sideCount ) {
			verticesArray.TryDispose();
			verticesArray = MemoryPool<Vector2>.Shared.Rent( sideCount );
		}

		var matrix = ScreenSpaceDrawQuad.AsMatrix();
		var centre = new Vector2( 0.5f );
		var vertex = new Vector2( 0, -0.5f );
		var theta = MathF.Tau / sideCount;
		for ( int i = 0; i < sideCount; i++ ) {
			var p = centre + vertex;
			Vector2Extensions.Transform( ref p, ref matrix, out verticesArray[i] );
			vertex = vertex.Rotate( theta );
		}

		return verticesArray;
	}

	public override bool Contains ( Vector2 screenSpacePos )
		=> this.Intersects( new Quad( screenSpacePos, screenSpacePos, screenSpacePos, screenSpacePos ) );

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		verticesArray.TryDispose();
		verticesArray = default;
	}

	protected override DrawNode CreateDrawNode ()
		=> new PolygonDrawNode( this );

	protected class PolygonDrawNode : DrawNode {
		new protected PolygonDrawable Source => (PolygonDrawable)base.Source;

		public PolygonDrawNode ( PolygonDrawable source ) : base( source ) { }

		IShader shader = null!;
		int sideCount;
		float cornerRadius;
		Matrix3 matrix;
		const int MAX_SIDES = 100;
		const int SMOOTHING_PER_VERTEX = 50;

		public override void ApplyState () {
			base.ApplyState();

			shader = Source.shader;
			sideCount = Math.Clamp( Source.sideCount, 3, MAX_SIDES );
			cornerRadius = Source.cornerRadius / Source.MaxSize;
			matrix = Source.ScreenSpaceDrawQuad.AsMatrix();
		}

		LinearBatch<TexturedVertex2D> vertexBatch = new( MAX_SIDES * SMOOTHING_PER_VERTEX + 1, 1, PrimitiveType.TriangleFan );
		// https://www.desmos.com/calculator/txty6davsf
		public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
			base.Draw( vertexAction );

			var centre = new Vector2( 0.5f );
			var vertex = new Vector2( 0, -0.5f );
			var theta = MathF.Tau / sideCount;

			shader.Bind();
			Texture.WhitePixel.TextureGL.Bind();

			TexturedVertex2D? loopEnd = null;
			vertexAction = ( TexturedVertex2D v ) => {
				if ( !loopEnd.HasValue )
					loopEnd = v;

				vertexBatch.AddAction( v );
			};
			vertexAction( new TexturedVertex2D {
				Colour = Color4Extensions.ToLinear( DrawColourInfo.Colour ),
				Position = Vector2Extensions.Transform( centre, matrix )
			} );

			var vertexDelta = vertex.Rotate( theta ) - vertex;
			var (height, cornerRadius, cornerAngle) = MathExtensions.RoundTriangle( -vertex.Y, -vertexDelta.Y / vertexDelta.X, this.cornerRadius );

			loopEnd = null;
			for ( int i = 0; i < sideCount; i++ ) {
				if ( cornerRadius > 0 ) {
					var cornerCentre = centre + height * vertex.Normalized();
					var up = vertex.Normalized() * cornerRadius;

					for ( int k = 0; k < SMOOTHING_PER_VERTEX; k++ ) {
						var angle = ( cornerAngle / ( SMOOTHING_PER_VERTEX - 1 ) * k ) - cornerAngle / 2;
						vertexAction( new TexturedVertex2D {
							Colour = Color4Extensions.ToLinear( DrawColourInfo.Colour ),
							Position = Vector2Extensions.Transform( cornerCentre + up.Rotate( angle ), matrix )
						} );
					}
				}
				else {
					vertexAction( new TexturedVertex2D {
						Colour = Color4Extensions.ToLinear( DrawColourInfo.Colour ),
						Position = Vector2Extensions.Transform( centre + vertex, matrix )
					} );
				}

				vertex = vertex.Rotate( theta );
			}

			vertexAction( loopEnd!.Value );
			shader.Unbind();
		}
	}
}