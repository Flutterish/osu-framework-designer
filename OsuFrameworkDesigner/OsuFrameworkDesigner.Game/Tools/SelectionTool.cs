using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Cursor;
using OsuFrameworkDesigner.Game.Graphics;
using osuTK.Input;

namespace OsuFrameworkDesigner.Game.Tools;

public class SelectionTool : Tool {
	public SelectionTool () {
		AddInternal( selections = new Container<DrawableSelection>().Fill() );
		AddInternal( selectionBox = new() { Alpha = 0 } );

		Dictionary<IComponent, (IHasPosition pos, Vector2 handle)> dragHandleByComponent = new();
		selectionBox.DragStarted += e => {
			perform( IHasPosition.From, ( pos, comp ) => {
				dragHandleByComponent.Add( comp, (pos, new( pos.X.Value, pos.Y.Value )) );
			} );
		};
		selectionBox.Dragged += e => {
			var delta = Composer.ToContentSpace( e.ScreenSpaceMousePosition ) - Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition );
			delta = e.AltPressed ? delta : delta.Round();

			foreach ( var (pos, handle) in dragHandleByComponent.Values ) {
				pos.X.Value = handle.X + delta.X;
				pos.Y.Value = handle.Y + delta.Y;
			}
		};
		selectionBox.DragEnded += e => dragHandleByComponent.Clear();

		selectionBox.Top.Hide();
		selectionBox.Bottom.Hide();
		selectionBox.Left.Hide();
		selectionBox.Right.Hide();

		selectionBox.TopLeft.Hide();
		selectionBox.TopRight.Hide();
		selectionBox.BottomLeft.Hide();
		selectionBox.BottomRight.Hide();

		selectionBox.FarTopLeft.Hide();
		selectionBox.FarTopRight.Hide();
		selectionBox.FarBottomLeft.Hide();
		selectionBox.FarBottomRight.Hide();
	}

	void perform<T> ( Func<IComponent, T?> transformer, Action<T, IComponent> action ) {
		var transformed = Selection.Select( x => (transformer( x ), x) );
		if ( transformed.All( x => x.Item1 != null ) ) {
			foreach ( var i in transformed ) {
				action( i.Item1!, i.Item2 );
			}
		}
	}
	void perform<T> ( Func<IComponent, T?> transformer, Action<T> action ) {
		var transformed = Selection.Select( transformer );
		if ( transformed.All( x => x != null ) ) {
			foreach ( var i in transformed ) {
				action( i! );
			}
		}
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
		if ( !selected.SequenceEqual( Selection ) ) {
			Selection.Clear();
			Selection.AddRange( selected );
		}
	}

	protected override void OnDragEnd ( DragEndEvent e ) {
		hoverSelection.FadeOut( 500 ).Expire();
		hoverSelection = null;
	}

	protected override bool OnClick ( ClickEvent e ) {
		var item = Composer.ComponentsReverse.FirstOrDefault( x => x.AsDrawable().ScreenSpaceDrawQuad.Contains( e.ScreenSpaceMouseDownPosition ) );

		if ( item != null ) {
			if ( Selection.Count != 1 || item != Selection.Single() ) {
				Selection.Clear();
				Selection.Add( item );
			}
		}
		else {
			Selection.Clear();
		}

		return true;
	}

	public readonly BindableList<IComponent> Selection = new();
	SelectionBox selectionBox;
	protected override void Update () {
		base.Update();

		if ( selectionBox.Alpha == 0 )
			return;

		var box = ToLocalSpace( Selection.Select( x => x.AsDrawable() ).GetBoundingBox( x => x.ScreenSpaceDrawQuad.AABBFloat ) ).AABBFloat;
		//var localBox = selection.Select( x => x.AsDrawable() ).GetBoundingBox( x => x.DrawRectangle );
		selectionBox.Position = box.Location;
		selectionBox.Size = box.Size;
	}

	Container<DrawableSelection> selections;
	Blueprint<IComponent>? blueprint;
	Stack<DrawableSelection> selectionPool = new();
	Dictionary<IComponent, DrawableSelection> visibleSelections = new();
	protected override void LoadComplete () {
		base.LoadComplete();

		Selection.BindCollectionChanged( ( _, _ ) => {
			if ( blueprint != null ) {
				RemoveInternal( blueprint );
				blueprint = null;
			}
			selectionBox.Alpha = Selection.Count > 1 ? 1 : 0;
			if ( selectionBox.Alpha == 0 && Selection.SingleOrDefault() is IComponent comp ) {
				AddInternal( blueprint = comp.CreateBlueprint() );
			}
		} );

		Selection.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( IComponent i in e.OldItems ) {
					visibleSelections.Remove( i, out var selection );
					selection!.Free();
					selectionPool.Push( selection );
					selections.Remove( selection );
				}
			}
			if ( e.NewItems != null ) {
				foreach ( IComponent i in e.NewItems ) {
					if ( !selectionPool.TryPop( out var selection ) ) {
						selection = new();
					}
					visibleSelections.Add( i, selection );
					selections.Add( selection );
					selection.Apply( i.AsDrawable() );
				}
			}

			if ( Selection.Count < 2 )
				selections.Hide();
			else
				selections.Show();
		} );
	}

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( e.Key is Key.Delete && Selection.Any() ) {
			Composer.RemoveRange( Selection );
			Selection.Clear();
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