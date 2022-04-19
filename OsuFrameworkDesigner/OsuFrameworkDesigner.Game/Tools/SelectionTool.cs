using osu.Framework.Input.Events;

namespace OsuFrameworkDesigner.Game.Tools;

public class SelectionTool : Tool {
	SelectionBox? selection;
	protected override bool OnMouseDown ( MouseDownEvent e ) {
		selection?.FadeOut( 500 ).Expire();

		AddInternal( selection = new() );
		selection.Position = e.MousePosition;

		return true;
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		if ( selection != null ) {
			selection.Size = e.MousePosition - selection.Position;
		}

		return true;
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		selection?.FadeOut( 500 ).Expire();
		selection = null;
	}
}

public class SelectionBox : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public SelectionBox () {
		AddInternal( background = new Box { Alpha = 0.2f }.Fill() );
		Masking = true;
		BorderThickness = 4;
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		backgroundColor.BindValueChanged( v => BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}