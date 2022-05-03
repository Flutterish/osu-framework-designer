namespace OsuFrameworkDesigner.Game.Graphics.Selections;

public class DrawableQuadSelection : DrawableSelection {
	Box background;
	Bindable<Colour4> backgroundColor = new( Theme.SelectionDefault );

	public DrawableQuadSelection () {
		AddInternal( background = new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		Masking = true;
		BorderThickness = 6;
	}

	protected override void Update () {
		base.Update();

		(Position, Size, Shear, var rot) = Parent.ToLocalSpace( Selection.ScreenSpaceDrawQuad ).Decompose();
		Rotation = rot / MathF.PI * 180;
	}

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.Selection );
		backgroundColor.BindValueChanged( v => BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}
