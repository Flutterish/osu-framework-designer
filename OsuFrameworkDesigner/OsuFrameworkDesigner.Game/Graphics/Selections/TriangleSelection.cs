using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Graphics.Selections;

public class TriangleSelection : DrawableSelection {
	new public TriangleComponent Selection => (TriangleComponent)base.Selection;

	DrawableTriangle background;
	Bindable<Colour4> backgroundColor = new( Theme.SelectionDefault );

	public TriangleSelection () {
		AddInternal( background = new DrawableTriangle { BorderThickness = 6, Colour = Color4.Transparent }.Fill() );
	}

	protected override void Update () {
		base.Update();

		var a = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Selection.PointA ) );
		var b = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Selection.PointB ) );
		var c = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Selection.PointC ) );

		Position = a;
		background.PointB = b - a;
		background.PointC = c - a;
	}

	public override Quad ScreenSpaceDrawQuad
		=> Selection.ScreenSpaceDrawQuad;

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.Selection );
		backgroundColor.BindValueChanged( v => background.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}
