namespace OsuFrameworkDesigner.Game.Graphics;

public class SnapMarker : CompositeDrawable {
	public readonly Bindable<Colour4> FillColour = new( ColourConfiguration.SnapMarkerDefault );

	public SnapMarker () {
		Origin = Anchor.Centre;
		AddInternal( new Box {
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			Rotation = 45,
			Size = new( 8, 2 )
		} );
		AddInternal( new Box {
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			Rotation = -45,
			Size = new( 8, 2 )
		} );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		FillColour.BindTo( colours.SnapMarker );
		this.FadeColour( FillColour );

		FinishTransforms( true );
	}
}
