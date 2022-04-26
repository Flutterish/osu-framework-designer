using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using OsuFrameworkDesigner.Game.Cursor;

namespace OsuFrameworkDesigner.Game.Tools;

public class Handle : CompositeDrawable, IUsesCursorStyle, IUsesCursorRotation, IHasTooltip {
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

	public void ClearEvents () {
		DragStarted = null;
		Dragged = null;
		DragEnded = null;
	}

	public event Action<DragStartEvent>? DragStarted;
	public event Action<DragEvent>? Dragged;
	public event Action<DragEndEvent>? DragEnded;

	public CursorStyle CursorStyle { get; set; }
	public float CursorRotation { get; set; }
	public LocalisableString TooltipText { get; set; }
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
		Size = new( 28 );
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

public class PointHandle : Handle {
	Circle background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionHandleDefault );
	Bindable<Colour4> selectionColor = new( ColourConfiguration.SelectionDefault );

	public PointHandle () {
		Origin = Anchor.Centre;
		AddInternal( new Container {
			Size = new( 16 ),
			Child = background = new Circle { BorderThickness = 4 }.Fill(),
		}.Center() );
		Size = new( 24 );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.SelectionHandle );
		selectionColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		selectionColor.BindValueChanged( v => background.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}