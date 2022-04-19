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
		if ( blueprint != null ) {
			var bounds = ToLocalSpace( blueprint.Value.AsDrawable().ScreenSpaceDrawQuad ).AABBFloat;
			blueprint.Position = bounds.Location;
			blueprint.Size = bounds.Size;
		}

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

public class SelectionBox : CompositeDrawable {
	Container border;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionDefault );

	public readonly Handle TopLeft;
	public readonly Handle TopRight;
	public readonly Handle BottomLeft;
	public readonly Handle BottomRight;

	public SelectionBox () {
		AddInternal( border = new Container {
			Masking = true,
			BorderThickness = 4,
			Child = new Box { Alpha = 0, AlwaysPresent = true }.Fill()
		}.Fill() );

		AddInternal( TopLeft = new Handle { Anchor = Anchor.TopLeft, CursorStyle = CursorStyle.ResizeNW } );
		AddInternal( TopRight = new Handle { Anchor = Anchor.TopRight, CursorStyle = CursorStyle.ResizeSW } );
		AddInternal( BottomLeft = new Handle { Anchor = Anchor.BottomLeft, CursorStyle = CursorStyle.ResizeSW } );
		AddInternal( BottomRight = new Handle { Anchor = Anchor.BottomRight, CursorStyle = CursorStyle.ResizeNW } );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.Selection );
		backgroundColor.BindValueChanged( v => border.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}

	new public class Handle : CompositeDrawable, IUsesCursorStyle {
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

		protected override bool OnDragStart ( DragStartEvent e ) {
			return true;
		}

		protected override void OnDrag ( DragEvent e ) {
			Dragged?.Invoke( e );
		}

		protected override void OnDragEnd ( DragEndEvent e ) {
			DragEnded?.Invoke( e );
		}

		public event Action<DragEvent>? Dragged;
		public event Action<DragEndEvent>? DragEnded;

		public CursorStyle CursorStyle { get; set; }
	}
}