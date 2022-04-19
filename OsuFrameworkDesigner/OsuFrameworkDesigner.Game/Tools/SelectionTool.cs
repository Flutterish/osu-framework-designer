using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Tools;

public class SelectionTool : Tool {
	public SelectionTool () {
		AddInternal( selectionBox = new() { Alpha = 0 } );
	}

	HoverSelectionBox? hoverSelection;
	protected override bool OnDragStart ( DragStartEvent e ) {
		AddInternal( hoverSelection = new() );
		hoverSelection.Position = e.MousePosition;

		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		hoverSelection!.Size = e.MousePosition - hoverSelection.Position;

		var topLeft = ToScreenSpace( hoverSelection.Position );
		var bottomRight = ToScreenSpace( hoverSelection.Position + hoverSelection.Size );
		var selectionQuad = new Quad( topLeft, new( bottomRight.X, topLeft.Y ), new( topLeft.X, bottomRight.Y ), bottomRight );

		Composer.Selection.Clear();
		Composer.Selection.AddRange( Composer.Components.Where( x => x.AsDrawable().ScreenSpaceDrawQuad.Intersects( selectionQuad ) ) );
	}

	protected override void OnDragEnd ( DragEndEvent e ) {
		hoverSelection.FadeOut( 500 ).Expire();
		hoverSelection = null;
	}

	protected override bool OnClick ( ClickEvent e ) {
		Composer.Selection.Clear();
		var item = Composer.ComponentsReverse.FirstOrDefault( x => x.AsDrawable().ScreenSpaceDrawQuad.Contains( e.ScreenSpaceMouseDownPosition ) );
		if ( item != null )
			Composer.Selection.Add( item );

		return true;
	}

	BindableList<IComponent> selection = new();
	SelectionBox selectionBox;
	protected override void Update () {
		base.Update();
		if ( selectionBox.Alpha == 0 )
			return;

		var box = ToLocalSpace( selection.Select( x => x.AsDrawable() ).GetBoundingBox( x => x.ScreenSpaceDrawQuad.AABBFloat ) ).AABBFloat;
		//var localBox = selection.Select( x => x.AsDrawable() ).GetBoundingBox( x => x.DrawRectangle );
		selectionBox.Position = box.Location;
		selectionBox.Size = box.Size;
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		selection.BindTo( Composer.Selection );
		Composer.Selection.BindCollectionChanged( ( _, _ ) => {
			selectionBox.Alpha = Composer.Selection.Skip( 1 ).Any() ? 1 : 0;
		} );
	}
}

public class HoverSelectionBox : CompositeDrawable { // TODO the borders should be around, not inside
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public HoverSelectionBox () {
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

public class SelectionBox : CompositeDrawable {
	Container border;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	Handle topLeft;
	Handle topRight;
	Handle bottomLeft;
	Handle bottomRight;

	public SelectionBox () {
		AddInternal( border = new Container {
			Masking = true,
			BorderThickness = 4,
			Child = new Box { Alpha = 0, AlwaysPresent = true }.Fill()
		}.Fill() );

		AddInternal( topLeft = new Handle { Anchor = Anchor.TopLeft } );
		AddInternal( topRight = new Handle { Anchor = Anchor.TopRight } );
		AddInternal( bottomLeft = new Handle { Anchor = Anchor.BottomLeft } );
		AddInternal( bottomRight = new Handle { Anchor = Anchor.BottomRight } );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Selection );
		backgroundColor.BindValueChanged( v => border.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}

	new private class Handle : CompositeDrawable {
		Box background;
		Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionHandleDefault );
		Bindable<Colour4> selectionColor = new( ColourConfiguration.SelectionDefault );

		public Handle () {
			Origin = Anchor.Centre;
			Size = new( 12 );
			AddInternal( background = new Box().Fill() );
			Masking = true;
			BorderThickness = 4;
		}

		[BackgroundDependencyLoader]
		private void load ( ColourConfiguration colours ) {
			backgroundColor.BindTo( colours.SelectionHandle );
			selectionColor.BindTo( colours.Selection );
			background.FadeColour( backgroundColor );
			selectionColor.BindValueChanged( v => BorderColour = v.NewValue, true );
			FinishTransforms( true );
		}
	}
}