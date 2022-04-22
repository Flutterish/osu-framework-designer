using System.Collections;

namespace OsuFrameworkDesigner.Game.Components;

public class TransformProps : IEnumerable<IProp> {
	public TransformProps ( Drawable drawable ) {
		CopyProps( drawable );

		X.ValueChanged += v => drawable.X = v.NewValue;
		Y.ValueChanged += v => drawable.Y = v.NewValue;
		Width.ValueChanged += v => drawable.Width = v.NewValue;
		Height.ValueChanged += v => drawable.Height = v.NewValue;
		Rotation.ValueChanged += v => drawable.Rotation = v.NewValue;
		(ScaleX, ScaleY).BindValueChanged( ( sx, sy ) => drawable.Scale = new( sx, sy ) );
		(ShearX, ShearY).BindValueChanged( ( sx, sy ) => drawable.Shear = new( sx, sy ) );

		(OriginX, OriginY).BindValueChanged( ( ox, oy ) => {
			var xAnchor = ox switch { 0 => Anchor.x0, 0.5f => Anchor.x1, 1 => Anchor.x2, _ => Anchor.Custom };
			var yAnchor = oy switch { 0 => Anchor.y0, 0.5f => Anchor.y1, 1 => Anchor.y2, _ => Anchor.Custom };
			if ( xAnchor != Anchor.Custom && yAnchor != Anchor.Custom ) {
				drawable.Origin = xAnchor | yAnchor;
			}
			else {
				drawable.Origin = Anchor.Custom;
			}
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

	public void CopyProps ( Drawable drawable ) {
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

	public readonly Prop<float> X = new() { Category = "Basic" };
	public readonly Prop<float> Y = new() { Category = "Basic" };
	public readonly Prop<float> Width = new() { Category = "Basic" };
	public readonly Prop<float> Height = new() { Category = "Basic" };
	public readonly Prop<float> Rotation = new() { Category = "Basic" };
	public readonly Prop<float> ScaleX = new( "X" ) { Category = "Scale" };
	public readonly Prop<float> ScaleY = new( "Y" ) { Category = "Scale" };
	public readonly Prop<float> ShearX = new( "X" ) { Category = "Shear" };
	public readonly Prop<float> ShearY = new( "Y" ) { Category = "Shear" };
	public readonly Prop<float> OriginX = new( "X" ) { Category = "Origin" };
	public readonly Prop<float> OriginY = new( "Y" ) { Category = "Origin" };

	public Vector2 RelativeOrigin => new( OriginX.Value, OriginY.Value );

	public float EffectiveWidth => Width.Value * ScaleX.Value;
	public float EffectiveHeight => Height.Value * ScaleY.Value;

	public Vector2 Size => new( Width.Value, Height.Value );
	public Vector2 Scale => new( ScaleX.Value, ScaleY.Value );
	public Vector2 Position => new( X.Value, Y.Value );
	public Vector2 Shear => new( ShearX.Value, ShearY.Value );

	public void Normalize () {
		if ( Height.Value < 0 ) {
			var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
			X.Value += ( cos - sin * ShearX.Value ) * ( 1 - OriginY.Value * 2 ) * Height.Value;
			Y.Value += ( sin + cos * ShearX.Value ) * ( 1 - OriginY.Value * 2 ) * Height.Value;

			Height.Value = -Height.Value;
		}

		if ( Width.Value < 0 ) {
			var cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
			var sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
			X.Value += ( cos + sin * ShearY.Value ) * ( 1 - OriginX.Value * 2 ) * Width.Value;
			Y.Value += ( sin - cos * ShearY.Value ) * ( 1 - OriginX.Value * 2 ) * Width.Value;
			// shear is non-commutative
			var total = Shear.X * Shear.Y;
			X.Value += cos * total * Width.Value * ( 1 - OriginX.Value * 2 );
			Y.Value += sin * total * Width.Value * ( 1 - OriginX.Value * 2 );

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
		OriginY.Value = value.Y;
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		X.Value += ( cos - sin * ShearX.Value ) * deltaY * EffectiveHeight;
		Y.Value += ( sin + cos * ShearX.Value ) * deltaY * EffectiveHeight;

		var deltaX = value.X - OriginX.Value;
		OriginX.Value = value.X;
		cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
		sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );
		X.Value += ( cos + sin * ShearY.Value ) * deltaX * EffectiveWidth;
		Y.Value += ( sin - cos * ShearY.Value ) * deltaX * EffectiveWidth;
		// shear is non-commutative
		var total = Shear.X * Shear.Y;
		X.Value += cos * total * deltaX * EffectiveWidth;
		Y.Value += sin * total * deltaX * EffectiveWidth;
	}

	public void ShearTop ( float deltaX ) {
		var cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
		var sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );

		X.Value += ( cos + sin * ShearY.Value ) * deltaX * ( 1 - OriginY.Value );
		Y.Value += ( sin - cos * ShearY.Value ) * deltaX * ( 1 - OriginY.Value );

		var deltaXShear = -deltaX / EffectiveHeight;

		// shear is non-commutative
		var dy = deltaXShear * ShearY.Value * ShearY.Value / ( deltaXShear * ShearX.Value + 1 );
		var dw = deltaXShear * EffectiveWidth * ShearY.Value; // TODO this is scuffed (maybe becase of invalid input?)
		ShearX.Value -= deltaXShear;
		if ( float.IsNormal( dy ) ) {
			var orig = RelativeOrigin;
			SetOrigin( Vector2.Zero );
			Height.Value += deltaX * ShearY.Value;
			ShearY.Value += dy;
			Width.Value += dw;
			SetOrigin( orig );
		}
	}

	public void ShearBottom ( float deltaX ) {
		var cos = MathF.Cos( Rotation.Value / 180 * MathF.PI );
		var sin = MathF.Sin( Rotation.Value / 180 * MathF.PI );

		X.Value += ( cos - sin * ShearY.Value ) * deltaX * OriginY.Value;
		Y.Value += ( sin + cos * ShearY.Value ) * deltaX * OriginY.Value;

		var deltaXShear = deltaX / EffectiveHeight;

		// shear is non-commutative
		var dy = -deltaXShear * ShearY.Value * ShearY.Value / ( deltaXShear * ShearY.Value + 1 );
		var dw = deltaXShear * EffectiveWidth * ShearY.Value; // TODO this is scuffed (maybe becase of invalid input?)
		ShearX.Value -= deltaXShear;
		if ( float.IsNormal( dy ) ) {
			var orig = RelativeOrigin;
			SetOrigin( Vector2.Zero );
			Height.Value -= deltaX * ShearY.Value;
			ShearY.Value += dy;
			Width.Value += dw;
			SetOrigin( orig );
		}
	}

	public void ShearLeft ( float deltaY ) {
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );

		Y.Value += ( sin + cos * ShearX.Value ) * deltaY * ( 1 - OriginX.Value );
		X.Value += ( cos - sin * ShearX.Value ) * deltaY * ( 1 - OriginX.Value );
		ShearY.Value += deltaY / EffectiveWidth;
	}

	public void ShearRight ( float deltaY ) {
		var cos = MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI );
		var sin = MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI );

		Y.Value += ( sin + cos * ShearX.Value ) * deltaY * OriginX.Value;
		X.Value += ( cos - sin * ShearX.Value ) * deltaY * OriginX.Value;
		ShearY.Value -= deltaY / EffectiveWidth;
	}

	public IEnumerator<IProp> GetEnumerator () {
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
}
