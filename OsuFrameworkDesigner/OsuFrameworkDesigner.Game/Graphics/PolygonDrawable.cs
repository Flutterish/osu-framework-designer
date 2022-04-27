using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;

namespace OsuFrameworkDesigner.Game.Graphics;

public class PolygonDrawable : Drawable {
	int sideCount = 3;
	public int SideCount {
		get => sideCount;
		set {
			if ( sideCount == value )
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

	public float CornerRadiusAtDistance ( float distance ) {
		var q = distance / MathF.Max( DrawWidth, DrawHeight );
		if ( q > 0.5f ) {
			return 0;
		}

		var vertex = new Vector2( 0, -0.5f );
		var theta = MathF.Tau / sideCount;
		var vertexDelta = vertex.Rotate( theta ) - vertex;
		var m1 = vertexDelta.X / -vertexDelta.Y;
		var m2 = -m1;
		var b1 = -vertex.Y + vertex.X / m2;
		var d = m2 + 1 / m2;

		var x = (b1 - q) / d;
		var y = m2 * x;
		return MathF.Sqrt( x * x + y * y ) * MathF.Max( DrawWidth, DrawHeight );
	}

	public float CornerCentreDistance {
		get {
			var vertex = new Vector2( 0, -0.5f );
			var theta = MathF.Tau / sideCount;

			var vertexDelta = vertex.Rotate( theta ) - vertex;
			var m1 = vertexDelta.X / -vertexDelta.Y;
			var m2 = -m1;
			var b1 = -vertex.Y + vertex.X / m2;
			var d = m2 + 1 / m2;
			var r = cornerRadius / MathF.Max( DrawWidth, DrawHeight );

			var m = 1 + m2 * m2;
			var n = -2 * b1 - 2 * m2 * m2 * b1;
			var o = b1 * b1 + m2 * m2 * b1 * b1 - d * d * r * r;
			var root = MathF.Sqrt( n * n - 4 * m * o );
			var b2 = MathF.Min( ( root - n ) / 2 / m, ( -root - n ) / 2 / m );

			if ( b2 < 0 ) {
				return 0;
			}

			return b2 * MathF.Max( DrawWidth, DrawHeight ) * 2;
		}
	}

	public float MaxCornerRadius => CornerRadiusAtDistance( 0 );

	IShader shader = null!;

	[BackgroundDependencyLoader]
	private void load ( ShaderManager shaders ) {
		shader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
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
			cornerRadius = Source.cornerRadius / MathF.Max( Source.DrawWidth, Source.DrawHeight );
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
			var m1 = vertexDelta.X / -vertexDelta.Y;
			var m2 = -m1;
			var b1 = -vertex.Y + vertex.X / m2;
			var d = m2 + 1 / m2;
			var r = cornerRadius;

			var m = 1 + m2 * m2;
			var n = -2 * b1 - 2 * m2 * m2 * b1;
			var o = b1 * b1 + m2 * m2 * b1 * b1 - d * d * r * r;
			var root = MathF.Sqrt( n * n - 4 * m * o );
			var b2 = MathF.Min( ( root - n ) / 2 / m, ( -root - n ) / 2 / m );

			var cornerAngle = 2 * ( MathF.PI / 2 - MathF.Atan2( m2, 1 ) );
			var cappedCornerRadius = cornerRadius;
			if ( b2 < 0 ) {
				b2 = 0;
				var x = b1 / d;
				var y = m2 * x;
				cappedCornerRadius = MathF.Sqrt( x * x + y * y );
			}

			loopEnd = null;
			for ( int i = 0; i < sideCount; i++ ) {
				if ( cappedCornerRadius > 0 ) {
					var cornerCentre = centre + b2 * vertex.Normalized();
					var up = vertex.Normalized() * cappedCornerRadius;

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