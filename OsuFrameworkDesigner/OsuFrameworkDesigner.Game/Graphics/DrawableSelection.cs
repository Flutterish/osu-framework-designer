using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Graphics;

public class DrawableSelection : CompositeDrawable {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public DrawableSelection () {
		AddInternal( background = new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		Masking = true;
		BorderThickness = 6;
	}

	Drawable? selection;
	protected override void Update () {
		base.Update();

		if ( selection is null )
			return;

		var topLeft = Parent.ToLocalSpace( selection.ScreenSpaceDrawQuad.TopLeft );
		Position = topLeft;
		var a = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Vector2.Zero ) );
		var b = Parent.ToLocalSpace( Composer.ContentToScreenSpace( new( 1, 0 ) ) );
		var scale = ( a - b ).Length;
		Size = selection.DrawSize * selection.Scale * scale;
		Shear = selection.Shear;
		Rotation = selection.Rotation;
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
		backgroundColor.BindValueChanged( v => BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}
