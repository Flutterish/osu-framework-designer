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
	}

	public void CopyProps ( Drawable drawable ) {
		X.Value = drawable.X;
		Y.Value = drawable.Y;
		Width.Value = drawable.Width;
		Height.Value = drawable.Height;
		Rotation.Value = drawable.Rotation;
	}

	public readonly Prop<float> X = new() { Category = "Basic" };
	public readonly Prop<float> Y = new() { Category = "Basic" };
	public readonly Prop<float> Width = new() { Category = "Basic" };
	public readonly Prop<float> Height = new() { Category = "Basic" };
	public readonly Prop<float> Rotation = new() { Category = "Basic" };

	public void Normalize () {
		var w = Width.Value;
		if ( w < 0 ) {
			X.Value += w;
			Width.Value = -w;
		}

		var h = Height.Value;
		if ( h < 0 ) {
			Y.Value += h;
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
		X.Value -= delta;
		Width.Value += delta;
	}

	public void SetTopEdge ( float y ) {
		var delta = Y.Value - y;
		Y.Value -= delta;
		Height.Value += delta;
	}

	public IEnumerator<IProp> GetEnumerator () {
		yield return X;
		yield return Y;
		yield return Width;
		yield return Height;
		yield return Rotation;
	}

	IEnumerator IEnumerable.GetEnumerator ()
		=> GetEnumerator();
}
