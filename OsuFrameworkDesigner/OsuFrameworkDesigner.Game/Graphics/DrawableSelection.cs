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

		(Position, Size, Shear, var rot) = Parent.ToLocalSpace( selection.ScreenSpaceDrawQuad ).Decompose();
		Rotation = rot / MathF.PI * 180;
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
