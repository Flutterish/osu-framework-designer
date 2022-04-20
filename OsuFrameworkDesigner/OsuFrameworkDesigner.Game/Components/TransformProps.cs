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
	public readonly Prop<float> OriginX = new( "X" ) { Category = "Origin" };
	public readonly Prop<float> OriginY = new( "Y" ) { Category = "Origin" };

	public Vector2 RelativeOrigin => new( OriginX.Value, OriginY.Value );

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

		X.Value += OriginX.Value * MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta;
		Y.Value += OriginX.Value * MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta;
	}

	public void SetBottomEdge ( float y ) {
		var bottom = Y.Value + Height.Value * ( 1 - OriginY.Value );
		var delta = y - bottom;

		Height.Value += delta;

		X.Value += OriginY.Value * MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta;
		Y.Value += OriginY.Value * MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta;
	}

	public void SetLeftEdge ( float x ) {
		var left = X.Value - Width.Value * OriginX.Value;
		var delta = x - left;

		Width.Value -= delta;

		X.Value += ( 1 - OriginX.Value ) * MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta;
		Y.Value += ( 1 - OriginX.Value ) * MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta;
	}

	public void SetTopEdge ( float y ) {
		var top = Y.Value - Height.Value * OriginY.Value;
		var delta = y - top;

		Height.Value -= delta;

		X.Value += ( 1 - OriginY.Value ) * MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta;
		Y.Value += ( 1 - OriginY.Value ) * MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta;
	}

	public IEnumerator<IProp> GetEnumerator () {
		yield return X;
		yield return Y;
		yield return Width;
		yield return Height;
		yield return Rotation;
		yield return OriginX;
		yield return OriginY;
	}

	IEnumerator IEnumerable.GetEnumerator ()
		=> GetEnumerator();
}
