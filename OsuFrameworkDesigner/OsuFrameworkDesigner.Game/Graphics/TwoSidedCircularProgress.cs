﻿using osu.Framework.Extensions.MatrixExtensions;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics.ES30;

namespace OsuFrameworkDesigner.Game.Graphics;

public class TwoSidedCircularProgress : CircularProgress {
	float offset;
	public float Offset {
		get => offset;
		set {
			if ( offset == value )
				return;

			offset = value;
			Invalidate( Invalidation.DrawNode );
		}
	}

	protected override DrawNode CreateDrawNode ()
		=> new TwoSidedCircularProgressDrawNode( this );
}

#nullable disable
public class TwoSidedCircularProgressDrawNode : TexturedShaderDrawNode {
	private const float arc_tolerance = 0.1f;

	private const float two_pi = MathF.PI * 2;

	protected new TwoSidedCircularProgress Source => (TwoSidedCircularProgress)base.Source;

	private LinearBatch<TexturedVertex2D> halfCircleBatch;

	private float offset;
	private float angle;
	private float innerRadius = 1;

	private Vector2 drawSize;
	private Texture texture;

	public TwoSidedCircularProgressDrawNode ( TwoSidedCircularProgress source )
		: base( source ) {
	}

	public override void ApplyState () {
		base.ApplyState();

		texture = Source.Texture;
		drawSize = Source.DrawSize;
		offset = (float)Source.Offset * two_pi;
		angle = (float)Source.Current.Value * two_pi;
		innerRadius = Source.InnerRadius;
	}

	private Vector2 pointOnCircle ( float angle ) => new Vector2( MathF.Sin( angle ), -MathF.Cos( angle ) );
	private float angleToUnitInterval ( float angle ) => angle / two_pi + ( angle >= 0 ? 0 : 1 );

	// Gets colour at the localPos position in the unit square of our Colour gradient box.
	private Color4 colourAt ( Vector2 localPos ) => DrawColourInfo.Colour.HasSingleColour
		? DrawColourInfo.Colour.TopLeft.Linear
		: DrawColourInfo.Colour.Interpolate( localPos ).Linear;

	private static readonly Vector2 origin = new Vector2( 0.5f, 0.5f );

	private void updateVertexBuffer () {
		float start_angle = offset;

		float dir = Math.Sign( angle );
		float radius = Math.Max( drawSize.X, drawSize.Y );

		// The amount of points are selected such that discrete curvature is smaller than the provided tolerance.
		// The exact angle required to meet the tolerance is: 2 * Math.Acos(1 - TOLERANCE / r)
		// The special case is for extremely small circles where the radius is smaller than the tolerance.
		int amountPoints = 2 * radius <= arc_tolerance ? 2 : Math.Max( 2, (int)Math.Ceiling( Math.PI / Math.Acos( 1 - arc_tolerance / radius ) ) );

		if ( halfCircleBatch == null || halfCircleBatch.Size < amountPoints * 2 ) {
			halfCircleBatch?.Dispose();

			// Amount of points is multiplied by 2 to account for each part requiring two vertices.
			halfCircleBatch = new LinearBatch<TexturedVertex2D>( amountPoints * 2, 1, PrimitiveType.TriangleStrip );
		}

		Matrix3 transformationMatrix = DrawInfo.Matrix;
		MatrixExtensions.ScaleFromLeft( ref transformationMatrix, drawSize );

		Vector2 current = origin + pointOnCircle( start_angle ) * 0.5f;
		Color4 currentColour = colourAt( current );
		current = Vector2Extensions.Transform( current, transformationMatrix );

		Vector2 screenOrigin = Vector2Extensions.Transform( origin, transformationMatrix );
		Color4 originColour = colourAt( origin );

		// Offset by 0.5 pixels inwards to ensure we never sample texels outside the bounds
		RectangleF texRect = texture.GetTextureRect( new RectangleF( 0.5f, 0.5f, texture.Width - 1, texture.Height - 1 ) );

		float prevOffset = dir >= 0 ? 0 : 1;

		// First center point
		halfCircleBatch.Add( new TexturedVertex2D {
			Position = Vector2.Lerp( current, screenOrigin, innerRadius ),
			TexturePosition = new Vector2( dir >= 0 ? texRect.Left : texRect.Right, texRect.Top ),
			Colour = originColour
		} );

		// First outer point.
		halfCircleBatch.Add( new TexturedVertex2D {
			Position = new Vector2( current.X, current.Y ),
			TexturePosition = new Vector2( dir >= 0 ? texRect.Left : texRect.Right, texRect.Bottom ),
			Colour = currentColour
		} );

		for ( int i = 1; i < amountPoints; i++ ) {
			float fract = (float)i / ( amountPoints - 1 );

			// Clamps the angle so we don't overshoot.
			// dir is used so negative angles result in negative angularOffset.
			float angularOffset = Math.Min( fract * two_pi, dir * angle );
			float normalisedOffset = angularOffset / two_pi;

			if ( dir < 0 )
				normalisedOffset += 1.0f;

			// Update `current`
			current = origin + pointOnCircle( start_angle + angularOffset ) * 0.5f;
			currentColour = colourAt( current );
			current = Vector2Extensions.Transform( current, transformationMatrix );

			// current center point
			halfCircleBatch.Add( new TexturedVertex2D {
				Position = Vector2.Lerp( current, screenOrigin, innerRadius ),
				TexturePosition = new Vector2( texRect.Left + ( normalisedOffset + prevOffset ) / 2 * texRect.Width, texRect.Top ),
				Colour = originColour
			} );

			// current outer point
			halfCircleBatch.Add( new TexturedVertex2D {
				Position = new Vector2( current.X, current.Y ),
				TexturePosition = new Vector2( texRect.Left + normalisedOffset * texRect.Width, texRect.Bottom ),
				Colour = currentColour
			} );

			prevOffset = normalisedOffset;
		}
	}

	public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
		base.Draw( vertexAction );

		if ( texture?.Available != true )
			return;

		Shader.Bind();

		texture.TextureGL.Bind();

		updateVertexBuffer();

		Shader.Unbind();
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );

		halfCircleBatch?.Dispose();
	}
}
