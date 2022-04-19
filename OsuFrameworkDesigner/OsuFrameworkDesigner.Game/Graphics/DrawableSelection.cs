namespace OsuFrameworkDesigner.Game.Graphics;

public class DrawableSelection : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public DrawableSelection () {
		AddInternal( background = new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		Masking = true;
		BorderThickness = 3;
	}

	Drawable? selection;
	protected override void Update () {
		base.Update();

		if ( selection is null )
			return;

		Rotation = selection.Rotation;
		Width = selection.DrawWidth;
		Height = selection.DrawHeight;
		X = selection.X;
		Y = selection.Y;
		Origin = selection.Origin;
		Anchor = selection.Anchor;
	}

	public void Apply ( Drawable drawable ) {
		selection = drawable;
	}

	public void Free () {
		selection = null;
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		backgroundColor.BindValueChanged( v => BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}
