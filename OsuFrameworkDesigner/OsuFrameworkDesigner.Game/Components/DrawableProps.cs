namespace OsuFrameworkDesigner.Game.Components;

public class DrawableProps : TransformProps {
	public DrawableProps ( Drawable drawable ) : base( drawable ) {
		FillColour.ValueChanged += v => drawable.Colour = v.NewValue;
	}

	public override void CopyProps ( Drawable drawable ) {
		base.CopyProps( drawable );
		FillColour.Value = drawable.Colour;
	}

	public readonly Prop<Colour4> FillColour = new( Colour4.Green, "Colour" ) { Category = "Fill", Groupable = true };

	public override IEnumerator<IProp> GetEnumerator () {
		var b = base.GetEnumerator();
		while ( b.MoveNext() )
			yield return b.Current;

		yield return FillColour;
	}
}
