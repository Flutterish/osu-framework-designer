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
		//(ShearX, ShearY).BindValueChanged( (sx, sy) => drawable.Shear = new( sx, sy ) ); 

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
		//ShearX.Value = drawable.Shear.X;
		//ShearY.Value = drawable.Shear.Y;

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
	//public readonly Prop<float> ShearX = new( "X" ) { Category = "Shear" };
	//public readonly Prop<float> ShearY = new( "Y" ) { Category = "Shear" };
	public readonly Prop<float> OriginX = new( "X" ) { Category = "Origin" };
	public readonly Prop<float> OriginY = new( "Y" ) { Category = "Origin" };

	public Vector2 RelativeOrigin => new( OriginX.Value, OriginY.Value );

	public float EffectiveWidth => Width.Value * ScaleX.Value;
	public float EffectiveHeight => Height.Value * ScaleY.Value;

	public void Normalize () {
		var w = Width.Value;
		if ( w < 0 ) {
			X.Value += MathF.Cos( Rotation.Value / 180 * MathF.PI ) * ( 1 - OriginX.Value * 2 ) * w;
			Y.Value += MathF.Sin( Rotation.Value / 180 * MathF.PI ) * ( 1 - OriginX.Value * 2 ) * w;
			Width.Value = -w;
		}

		var h = Height.Value;
		if ( h < 0 ) {
			X.Value += MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * ( 1 - OriginY.Value * 2 ) * h;
			Y.Value += MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * ( 1 - OriginY.Value * 2 ) * h;
			Height.Value = -h;
		}
	}

	public void SetRightEdge ( float x ) {
		var right = X.Value + Width.Value * ( 1 - OriginX.Value );
		var delta = x - right;

		Width.Value += delta;

		X.Value += OriginX.Value * MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;
		Y.Value += OriginX.Value * MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;
	}

	public void SetBottomEdge ( float y ) {
		var bottom = Y.Value + Height.Value * ( 1 - OriginY.Value );
		var delta = y - bottom;

		Height.Value += delta;

		X.Value += OriginY.Value * MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;
		Y.Value += OriginY.Value * MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;
	}

	public void SetLeftEdge ( float x ) {
		var left = X.Value - Width.Value * OriginX.Value;
		var delta = x - left;

		Width.Value -= delta;

		X.Value += ( 1 - OriginX.Value ) * MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;
		Y.Value += ( 1 - OriginX.Value ) * MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta * ScaleX.Value;
	}

	public void SetTopEdge ( float y ) {
		var top = Y.Value - Height.Value * OriginY.Value;
		var delta = y - top;

		Height.Value -= delta;

		X.Value += ( 1 - OriginY.Value ) * MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;
		Y.Value += ( 1 - OriginY.Value ) * MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta * ScaleY.Value;
	}

	public void SetOrigin ( Vector2 value ) {
		var deltaX = value.X - OriginX.Value;
		OriginX.Value = value.X;
		X.Value += MathF.Cos( Rotation.Value / 180 * MathF.PI ) * deltaX * EffectiveWidth;
		Y.Value += MathF.Sin( Rotation.Value / 180 * MathF.PI ) * deltaX * EffectiveWidth;

		var deltaY = value.Y - OriginY.Value;
		OriginY.Value = value.Y;
		X.Value += MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * deltaY * EffectiveHeight;
		Y.Value += MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * deltaY * EffectiveHeight;
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
		//yield return ShearX;
		//yield return ShearY;
	}

	IEnumerator IEnumerable.GetEnumerator ()
		=> GetEnumerator();
}
