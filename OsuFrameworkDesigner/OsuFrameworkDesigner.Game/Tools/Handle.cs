using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Cursor;

namespace OsuFrameworkDesigner.Game.Tools;

public class Handle : CompositeDrawable, IUsesCursorStyle {
	protected override bool OnDragStart ( DragStartEvent e ) {
		DragStarted?.Invoke( e );
		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		Dragged?.Invoke( e );
	}

	protected override void OnDragEnd ( DragEndEvent e ) {
		DragEnded?.Invoke( e );
	}

	public event Action<DragStartEvent>? DragStarted;
	public event Action<DragEvent>? Dragged;
	public event Action<DragEndEvent>? DragEnded;

	public CursorStyle CursorStyle { get; set; }
}

public class CornerHandle : Handle {
	Container border;
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionHandleDefault );
	Bindable<Colour4> selectionColor = new( ColourConfiguration.SelectionDefault );

	public CornerHandle () {
		Origin = Anchor.Centre;
		AddInternal( border = new Container {
			Size = new( 12 ),
			Child = background = new Box().Fill(),
			Masking = true,
			BorderThickness = 4
		}.Center() );
		Size = new( 36 );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.SelectionHandle );
		selectionColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		selectionColor.BindValueChanged( v => border.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}