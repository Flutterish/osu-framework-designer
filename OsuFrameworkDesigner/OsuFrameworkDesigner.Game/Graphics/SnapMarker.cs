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

public class SnapLine : Box {
	public readonly Bindable<Colour4> FillColour = new( ColourConfiguration.SnapMarkerDefault );

	public SnapLine () {
		Origin = Anchor.CentreLeft;
		Size = new( 0, 1 );
	}

	public void Connect ( Vector2 guideA, Vector2 guideB, Vector2 point ) {
		if ( ( guideA - point ).Length > ( guideB - point ).Length )
			Connect( guideA, point );
		else
			Connect( guideB, point );
	}

	public void Connect ( Vector2 pointA, Vector2 pointB ) {
		Position = pointA;
		var delta = pointB - pointA;
		Width = delta.Length;
		Rotation = MathF.Atan2( delta.Y, delta.X ) / MathF.PI * 180;
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		FillColour.BindTo( colours.SnapMarker );
		this.FadeColour( FillColour );

		FinishTransforms( true );
	}
}