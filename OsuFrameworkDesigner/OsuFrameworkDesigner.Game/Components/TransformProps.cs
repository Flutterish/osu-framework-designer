using osu.Framework.Caching;
using System.Collections;

namespace OsuFrameworkDesigner.Game.Components;

public class TransformProps : IEnumerable<IProp> {
	public TransformProps ( Drawable drawable ) {
		CopyProps( drawable );

		X.ValueChanged += v => {
			drawable.X = v.NewValue;
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		};
		Y.ValueChanged += v => {
			drawable.Y = v.NewValue;
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		};
		Width.ValueChanged += v => {
			drawable.Width = v.NewValue;
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		};
		Height.ValueChanged += v => {
			drawable.Height = v.NewValue;
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		};
		Rotation.ValueChanged += v => {
			drawable.Rotation = v.NewValue;
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		};
		(ScaleX, ScaleY).BindValueChanged( ( sx, sy ) => {
			drawable.Scale = new( sx, sy );
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		} );
		(ShearX, ShearY).BindValueChanged( ( sx, sy ) => {
			drawable.Shear = new( sx, sy );
			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		} );

		(OriginX, OriginY).BindValueChanged( ( ox, oy ) => {
			var xAnchor = ox switch { 0 => Anchor.x0, 0.5f => Anchor.x1, 1 => Anchor.x2, _ => Anchor.Custom };
			var yAnchor = oy switch { 0 => Anchor.y0, 0.5f => Anchor.y1, 1 => Anchor.y2, _ => Anchor.Custom };
			if ( xAnchor != Anchor.Custom && yAnchor != Anchor.Custom ) {
				drawable.Origin = xAnchor | yAnchor;
			}
			else {
				drawable.Origin = Anchor.Custom;
			}

			drawInfo.Invalidate();
			MatrixChanged?.Invoke();
		} );

		(OriginX, Width).BindValueChanged( ( ox, w ) => {
			if ( drawable.Origin is Anchor.Custom )
				drawable.OriginPosition = drawable.OriginPosition with { X = ox * w };
		} );
		(OriginY, Height).BindValueChanged( ( oy, h ) => {
			if ( drawable.Origin is Anchor.Custom )
				drawable.OriginPosition = drawable.OriginPosition with { Y = oy * h };
		} );
	}

	public virtual void CopyProps ( Drawable drawable ) {
		X.Value = drawable.X;
		Y.Value = drawable.Y;
		Width.Value = drawable.Width;
		Height.Value = drawable.Height;
		Rotation.Value = drawable.Rotation;
		ScaleX.Value = drawable.Scale.X;
		ScaleY.Value = drawable.Scale.Y;
		ShearX.Value = drawable.Shear.X;
		ShearY.Value = drawable.Shear.Y;

		if ( drawable.Origin is Anchor.Custom ) {
			if ( Width.Value != 0 ) OriginX.Value = drawable.OriginPosition.X / Width.Value;
			if ( Height.Value != 0 ) OriginY.Value = drawable.OriginPosition.Y / Height.Value;
		}
		else {
			OriginX.Value = drawable.RelativeOriginPosition.X;
			OriginY.Value = drawable.RelativeOriginPosition.Y;
		}
	}

	public readonly Prop<float> X = new( PropDescriptions.X );
	public readonly Prop<float> Y = new( PropDescriptions.Y );
	public readonly Prop<float> Width = new( PropDescriptions.Width );
	public readonly Prop<float> Height = new( PropDescriptions.Height );
	public readonly Prop<float> Rotation = new( PropDescriptions.Rotation );
	public readonly Prop<float> ScaleX = new( PropDescriptions.ScaleX );
	public readonly Prop<float> ScaleY = new( PropDescriptions.ScaleY );
	public readonly Prop<float> ShearX = new( PropDescriptions.ShearX );
	public readonly Prop<float> ShearY = new( PropDescriptions.ShearY );
	public readonly Prop<float> OriginX = new( PropDescriptions.OriginX );
	public readonly Prop<float> OriginY = new( PropDescriptions.OriginY );

	public Vector2 RelativeOrigin => new( OriginX.Value, OriginY.Value );

	public float EffectiveWidth => Width.Value * ScaleX.Value;
	public float EffectiveHeight => Height.Value * ScaleY.Value;

	public Vector2 Size => new( Width.Value, Height.Value );
	public Vector2 Scale => new( ScaleX.Value, ScaleY.Value );
	public Vector2 Position => new( X.Value, Y.Value );
	// Notes about shear:
	//	given the origin is at (x,y), the point at (x + dx, y) is translated so that:
	//		X += dx * shearX * shearY
	//		Y -= dx * shearY
	//	given the origin is at (x,y), the point at (x, y + dy) is translated so that:
	//		X -= dy * shearX
	//		Y stays constant
	//	given the origin is at (x,y), the point at (x + dx, y + dy) is translated so that:
	//		X += dx * shearX * shearY - dy * shearX
	//		Y -= dx * shearY
	// if you ever encounter shear being fucky in the X axis when the Y axis has a non-zero value, this is why
	public Vector2 Shear => new( ShearX.Value, ShearY.Value );

	public Vector2 PositionAtRelative ( Vector2 relativePosition ) {
		var x = X.Value;
		var y = Y.Value;

		var deltaY = relativePosition.Y - OriginY.Value;

		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		x += ( cos - sin * ShearX.Value ) * deltaY * EffectiveHeight;
		y += ( sin + cos * ShearX.Value ) * deltaY * EffectiveHeight;

		var deltaX = relativePosition.X - OriginX.Value;

		cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
		sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
		x += ( cos + sin * ShearY.Value ) * deltaX * EffectiveWidth;
		y += ( sin - cos * ShearY.Value ) * deltaX * EffectiveWidth;
		// shear is non-commutative
		var total = Shear.X * Shear.Y;
		x += cos * total * deltaX * EffectiveWidth;
		y += sin * total * deltaX * EffectiveWidth;

		return new( x, y );
	}

	public Vector2 TopLeft {
		get {
			var x = X.Value;
			var y = Y.Value;

			var deltaY = -OriginY.Value;
			var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			x += ( cos - sin * ShearX.Value ) * deltaY * EffectiveHeight;
			y += ( sin + cos * ShearX.Value ) * deltaY * EffectiveHeight;

			var deltaX = -OriginX.Value;
			cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
			sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
			x += ( cos + sin * ShearY.Value ) * deltaX * EffectiveWidth;
			y += ( sin - cos * ShearY.Value ) * deltaX * EffectiveWidth;
			// shear is non-commutative
			var total = Shear.X * Shear.Y;
			x += cos * total * deltaX * EffectiveWidth;
			y += sin * total * deltaX * EffectiveWidth;

			return new( x, y );
		}
	}

	public Vector2 Centre {
		get {
			var x = X.Value;
			var y = Y.Value;

			var deltaY = 0.5f - OriginY.Value;

			var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			x += ( cos - sin * ShearX.Value ) * deltaY * EffectiveHeight;
			y += ( sin + cos * ShearX.Value ) * deltaY * EffectiveHeight;

			var deltaX = 0.5f - OriginX.Value;

			cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
			sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
			x += ( cos + sin * ShearY.Value ) * deltaX * EffectiveWidth;
			y += ( sin - cos * ShearY.Value ) * deltaX * EffectiveWidth;
			// shear is non-commutative
			var total = Shear.X * Shear.Y;
			x += cos * total * deltaX * EffectiveWidth;
			y += sin * total * deltaX * EffectiveWidth;

			return new( x, y );
		}
	}

	public void Normalize () {
		if ( ScaleX.Value < 0 ) {
			ScaleX.Value = -ScaleX.Value;
			Width.Value = -Width.Value;
		}
		if ( ScaleY.Value < 0 ) {
			ScaleY.Value = -ScaleY.Value;
			Height.Value = -Height.Value;
		}

		if ( Height.Value < 0 ) {
			var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			X.Value += ( cos - sin * ShearX.Value ) * ( 1 - OriginY.Value * 2 ) * EffectiveHeight;
			Y.Value += ( sin + cos * ShearX.Value ) * ( 1 - OriginY.Value * 2 ) * EffectiveHeight;

			Height.Value = -Height.Value;
		}

		if ( Width.Value < 0 ) {
			var cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
			var sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
			X.Value += ( cos + sin * ShearY.Value ) * ( 1 - OriginX.Value * 2 ) * EffectiveWidth;
			Y.Value += ( sin - cos * ShearY.Value ) * ( 1 - OriginX.Value * 2 ) * EffectiveWidth;
			// shear is non-commutative
			var total = Shear.X * Shear.Y;
			X.Value += cos * total * EffectiveWidth * ( 1 - OriginX.Value * 2 );
			Y.Value += sin * total * EffectiveWidth * ( 1 - OriginX.Value * 2 );

			Width.Value = -Width.Value;
		}
	}

	public void SetRightEdge ( float x ) {
		var right = X.Value + Width.Value * ( 1 - OriginX.Value );
		var delta = x - right;

		Width.Value += delta;
		var cos = MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;
		var sin = MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;

		X.Value += OriginX.Value * ( cos + sin * ShearY.Value );
		Y.Value += OriginX.Value * ( sin - cos * ShearY.Value );
		// shear is non-commutative
		var total = Shear.X * Shear.Y;
		X.Value += OriginX.Value * cos * total;
		Y.Value += OriginX.Value * sin * total;
	}

	public void SetBottomEdge ( float y ) {
		var bottom = Y.Value + Height.Value * ( 1 - OriginY.Value );
		var delta = y - bottom;

		Height.Value += delta;
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;

		X.Value += OriginY.Value * ( cos - sin * ShearX.Value );
		Y.Value += OriginY.Value * ( sin + cos * ShearX.Value );
	}

	public void SetLeftEdge ( float x ) {
		var left = X.Value - Width.Value * OriginX.Value;
		var delta = x - left;

		Width.Value -= delta;
		var cos = MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;
		var sin = MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;

		X.Value += ( 1 - OriginX.Value ) * ( cos + sin * ShearY.Value );
		Y.Value += ( 1 - OriginX.Value ) * ( sin - cos * ShearY.Value );
		// shear is non-commutative
		var total = Shear.X * Shear.Y;
		X.Value += ( 1 - OriginX.Value ) * cos * total;
		Y.Value += ( 1 - OriginX.Value ) * sin * total;
	}

	public void SetTopEdge ( float y ) {
		var top = Y.Value - Height.Value * OriginY.Value;
		var delta = y - top;

		Height.Value -= delta;
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;

		X.Value += ( 1 - OriginY.Value ) * ( cos - sin * ShearX.Value );
		Y.Value += ( 1 - OriginY.Value ) * ( sin + cos * ShearX.Value );
	}

	public void SetOrigin ( Vector2 value ) {
		var deltaY = value.Y - OriginY.Value;

		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		X.Value += ( cos - sin * ShearX.Value ) * deltaY * EffectiveHeight;
		Y.Value += ( sin + cos * ShearX.Value ) * deltaY * EffectiveHeight;

		var deltaX = value.X - OriginX.Value;

		cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
		sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
		X.Value += ( cos + sin * ShearY.Value ) * deltaX * EffectiveWidth;
		Y.Value += ( sin - cos * ShearY.Value ) * deltaX * EffectiveWidth;
		// shear is non-commutative
		var total = Shear.X * Shear.Y;
		X.Value += cos * total * deltaX * EffectiveWidth;
		Y.Value += sin * total * deltaX * EffectiveWidth;

		OriginY.Value = value.Y;
		OriginX.Value = value.X;
	}

	// TODO this and shear bottom is still incorrect because of shearY, but normalizing makes it never come up
	public void ShearTop ( float deltaX ) {
		var orig = RelativeOrigin;
		SetOrigin( Vector2.UnitY );

		deltaX *= ScaleX.Value;
		var deltaXShear = -deltaX / EffectiveHeight;

		// shear is non-commutative
		var dy = -deltaXShear * ShearY.Value * ShearY.Value / ( deltaXShear * ShearY.Value + 1 );
		var dw = deltaXShear * EffectiveWidth * ShearY.Value;
		ShearX.Value -= deltaXShear;
		if ( float.IsNormal( dy ) ) {
			Height.Value += deltaX * ShearY.Value;
			ShearY.Value -= dy;
			Width.Value += dw;
			normalizeShear();
		}

		SetOrigin( orig );
	}

	public void ShearBottom ( float deltaX ) {
		var orig = RelativeOrigin;
		SetOrigin( Vector2.Zero );

		deltaX *= ScaleX.Value;
		var deltaXShear = deltaX / EffectiveHeight;

		// shear is non-commutative
		var dy = -deltaXShear * ShearY.Value * ShearY.Value / ( deltaXShear * ShearY.Value + 1 );
		var dw = deltaXShear * EffectiveWidth * ShearY.Value;
		ShearX.Value -= deltaXShear;
		if ( float.IsNormal( dy ) ) {
			Height.Value -= deltaX * ShearY.Value;
			ShearY.Value += dy;
			Width.Value += dw;
			normalizeShear();
		}

		SetOrigin( orig );
	}

	public void ShearLeft ( float deltaY ) {
		deltaY *= ScaleY.Value;
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );

		Y.Value += ( sin + cos * ShearX.Value ) * deltaY * ( 1 - OriginX.Value );
		X.Value += ( cos - sin * ShearX.Value ) * deltaY * ( 1 - OriginX.Value );
		ShearY.Value += deltaY / EffectiveWidth;

		normalizeShear();
	}

	public void ShearRight ( float deltaY ) {
		deltaY *= ScaleY.Value;
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );

		Y.Value += ( sin + cos * ShearX.Value ) * deltaY * OriginX.Value;
		X.Value += ( cos - sin * ShearX.Value ) * deltaY * OriginX.Value;
		ShearY.Value -= deltaY / EffectiveWidth;

		normalizeShear();
	}

	void normalizeShear () {
		// dont ask. I dont know. It works, okay?
		// okay, anyway, the way it works is it rotates such that shearY = 0,
		// then it constraints the top right X value,
		// then shears so that the X value of the bottom left is constrained
		// and finally constraints the Y of the bottom left
		// all done under an offset rotation because the calculations assume the X and Y axis
		// the math can be relatively easily derived from the shear notes above and vector rotation
		var w = Width.Value;
		var h = Height.Value;

		var TRX = w;
		var TRY = 0;
		var BRX = w - h * ShearX.Value;
		var BRY = h;
		var offset = MathF.Atan2( TRY - BRY, TRX - BRX ) + Rotation.Value / 180 * MathF.PI + MathF.PI / 2;

		var ncos = MathF.Cos( -Rotation.Value / 180 * MathF.PI + offset );
		var nsin = MathF.Sin( -Rotation.Value / 180 * MathF.PI + offset );
		var currrentX = ncos * w;
		var currrentY = ncos * h + nsin * h * ShearX.Value;

		var px = -h * ShearY.Value;
		var py = h;
		ShearY.Value = 0;

		var theta = ( MathF.Atan2( -py, px ) + MathF.PI / 2 );
		Rotation.Value += theta / MathF.PI * 180;
		ncos = MathF.Cos( -Rotation.Value / 180 * MathF.PI + offset );
		nsin = MathF.Sin( -Rotation.Value / 180 * MathF.PI + offset );
		w = Width.Value;
		h = Height.Value;

		Width.Value += currrentX / ncos - w;
		ShearX.Value += nsin / ncos - ShearX.Value;
		Height.Value += ( currrentY - ncos * h - nsin * ShearX.Value * h ) / ( ncos + nsin * ShearX.Value );
	}

	Cached<DrawInfo> drawInfo = new();
	public DrawInfo DrawInfo {
		get {
			if ( !drawInfo.IsValid ) {
				var info = new DrawInfo( null );
				info.ApplyTransform( Position, Scale, Rotation.Value, Shear, RelativeOrigin * Size );
				drawInfo.Value = info;
			}

			return drawInfo.Value;
		}
	}

	public DrawInfo LocalDrawInfo {
		get {
			var info = new DrawInfo( null );
			info.ApplyTransform( Vector2.Zero, Scale, Rotation.Value, Shear, Vector2.Zero );
			return info;
		}
	}

	public void SetMatrix ( Matrix3 matrix ) {
		float rot;
		((X.Value, Y.Value), (ScaleX.Value, ScaleY.Value), (ShearX.Value, ShearY.Value), rot) = matrix.Decompose();
		Rotation.Value = rot * 180 / MathF.PI;
	}

	public Vector2 ToLocalSpace ( Vector2 contentSpace )
		=> Vector2Extensions.Transform( contentSpace, DrawInfo.MatrixInverse );

	public Vector2 ToContentSpace ( Vector2 localSpace )
		=> Vector2Extensions.Transform( localSpace, DrawInfo.Matrix );

	public virtual IEnumerator<IProp> GetEnumerator () {
		yield return X;
		yield return Y;
		yield return Width;
		yield return Height;
		yield return Rotation;
		yield return OriginX;
		yield return OriginY;
		yield return ScaleX;
		yield return ScaleY;
		yield return ShearX;
		yield return ShearY;
	}

	IEnumerator IEnumerable.GetEnumerator ()
		=> GetEnumerator();

	public event Action? MatrixChanged;
}
