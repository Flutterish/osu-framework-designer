using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Cursor;
using osuTK.Input;

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

		var selected = Composer.Components.Where( x => x.AsDrawable().ScreenSpaceDrawQuad.Intersects( selectionQuad ) );
		if ( !selected.SequenceEqual( selection ) ) {
			selection.Clear();
			selection.AddRange( selected );
		}
	}

	protected override void OnDragEnd ( DragEndEvent e ) {
		hoverSelection.FadeOut( 500 ).Expire();
		hoverSelection = null;
	}

	protected override bool OnClick ( ClickEvent e ) {
		var item = Composer.ComponentsReverse.FirstOrDefault( x => x.AsDrawable().ScreenSpaceDrawQuad.Contains( e.ScreenSpaceMouseDownPosition ) );

		if ( item != null ) {
			if ( selection.Count != 1 || item != selection.Single() ) {
				selection.Clear();
				Composer.Selection.Add( item );
			}
		}
		else {
			selection.Clear();
		}

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

	Blueprint<IComponent>? blueprint;
	protected override void LoadComplete () {
		base.LoadComplete();

		selection.BindTo( Composer.Selection );
		selection.BindCollectionChanged( ( _, _ ) => {
			if ( blueprint != null ) {
				RemoveInternal( blueprint );
				blueprint = null;
			}
			selectionBox.Alpha = selection.Skip( 1 ).Any() ? 1 : 0;
			if ( selectionBox.Alpha == 0 && selection.SingleOrDefault() is IComponent comp ) {
				AddInternal( blueprint = comp.CreateBlueprint() );
			}
		} );
	}

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( e.Key is Key.Delete && selection.Any() ) {
			Composer.RemoveRange( selection );
			selection.Clear();
			return true;
		}

		return base.OnKeyDown( e );
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

public class SelectionBox : Handle {
	Container border;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public readonly CornerHandle TopLeft;
	public readonly CornerHandle TopRight;
	public readonly CornerHandle BottomLeft;
	public readonly CornerHandle BottomRight;

	public readonly Handle Left;
	public readonly Handle Right;
	public readonly Handle Top;
	public readonly Handle Bottom;

	public readonly Handle FarTopLeft;
	public readonly Handle FarTopRight;
	public readonly Handle FarBottomLeft;
	public readonly Handle FarBottomRight;

	public SelectionBox () {
		AddInternal( border = new Container {
			Masking = true,
			BorderThickness = 4,
			Child = new Box { Alpha = 0, AlwaysPresent = true }.Fill()
		}.Fill() );

		AddInternal( FarTopLeft = new Handle { Anchor = Anchor.TopLeft, Origin = Anchor.BottomRight, Size = new( 28 ), CursorStyle = CursorStyle.Rotate } );
		AddInternal( FarTopRight = new Handle { Anchor = Anchor.TopRight, Origin = Anchor.BottomLeft, Size = new( 28 ), CursorStyle = CursorStyle.Rotate } );
		AddInternal( FarBottomLeft = new Handle { Anchor = Anchor.BottomLeft, Origin = Anchor.TopRight, Size = new( 28 ), CursorStyle = CursorStyle.Rotate } );
		AddInternal( FarBottomRight = new Handle { Anchor = Anchor.BottomRight, Origin = Anchor.TopLeft, Size = new( 28 ), CursorStyle = CursorStyle.Rotate } );

		AddInternal( Top = new Handle { Origin = Anchor.Centre, Anchor = Anchor.TopCentre, Height = 24, RelativeSizeAxes = Axes.X, CursorStyle = CursorStyle.ResizeVertical } );
		AddInternal( Bottom = new Handle { Origin = Anchor.Centre, Anchor = Anchor.BottomCentre, Height = 24, RelativeSizeAxes = Axes.X, CursorStyle = CursorStyle.ResizeVertical } );
		AddInternal( Left = new Handle { Origin = Anchor.Centre, Anchor = Anchor.CentreLeft, Width = 24, RelativeSizeAxes = Axes.Y, CursorStyle = CursorStyle.ResizeHorizontal } );
		AddInternal( Right = new Handle { Origin = Anchor.Centre, Anchor = Anchor.CentreRight, Width = 24, RelativeSizeAxes = Axes.Y, CursorStyle = CursorStyle.ResizeHorizontal } );

		AddInternal( TopLeft = new CornerHandle { Anchor = Anchor.TopLeft, CursorStyle = CursorStyle.ResizeNW } );
		AddInternal( TopRight = new CornerHandle { Anchor = Anchor.TopRight, CursorStyle = CursorStyle.ResizeSW } );
		AddInternal( BottomLeft = new CornerHandle { Anchor = Anchor.BottomLeft, CursorStyle = CursorStyle.ResizeSW } );
		AddInternal( BottomRight = new CornerHandle { Anchor = Anchor.BottomRight, CursorStyle = CursorStyle.ResizeNW } );

	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Selection );
		backgroundColor.BindValueChanged( v => border.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}