using System.Collections;

namespace OsuFrameworkDesigner.Game.Components;

public struct TransformProps : IEnumerable<IProp> {
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
			X.Value += MathF.Cos( Rotation.Value / 180 * MathF.PI ) * w;
			Y.Value += MathF.Sin( Rotation.Value / 180 * MathF.PI ) * w;
			Width.Value = -w;
		}

		var h = Height.Value;
		if ( h < 0 ) {
			Y.Value += MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * h;
			X.Value += MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * h;
			Height.Value = -h;
		}
	}

	public void SetRightEdge ( float x ) {
		Width.Value = x - X.Value;
	}

	public void SetBottomEdge ( float y ) {
		Height.Value = y - Y.Value;
	}

	public void SetLeftEdge ( float x ) {
		var delta = X.Value - x;
		X.Value -= MathF.Cos( Rotation.Value / 180 * MathF.PI ) * delta;
		Width.Value += delta;
		Y.Value -= MathF.Sin( Rotation.Value / 180 * MathF.PI ) * delta;
	}

	public void SetTopEdge ( float y ) {
		var delta = Y.Value - y;
		Y.Value -= MathF.Sin( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta;
		Height.Value += delta;
		X.Value -= MathF.Cos( ( Rotation.Value + 90 ) / 180 * MathF.PI ) * delta;
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
