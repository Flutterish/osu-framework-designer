using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Cursor;
using OsuFrameworkDesigner.Game.Graphics;
using osuTK.Graphics;
using osuTK.Input;

namespace OsuFrameworkDesigner.Game.Tools;

public class SelectionTool : Tool {
	public SelectionTool () {
		AddInternal( selections = new Container<DrawableSelection>().Fill() );
		selectionComponent = ( new SelectionComponent().CreateBlueprint() as BasicTransformBlueprint<SelectionComponent> )!;
		AddInternal( selectionComponent );
		selectionComponent.Alpha = 0;
		selectionComponent.ResizingScales = true;

		Selection.BindCollectionChanged( ( _, _ ) => {
			var selectionHasMatrices = !Selection.Any( x => IHasMatrix.From( x ) is null );

			selectionBox.Top.Alpha =
			selectionBox.Bottom.Alpha =
			selectionBox.Left.Alpha =
			selectionBox.Right.Alpha =
			selectionBox.TopLeft.Alpha =
			selectionBox.TopRight.Alpha =
			selectionBox.BottomLeft.Alpha =
			selectionBox.BottomRight.Alpha =
			selectionBox.FarTopLeft.Alpha =
			selectionBox.FarTopRight.Alpha =
			selectionBox.FarBottomLeft.Alpha =
			selectionBox.FarBottomRight.Alpha =
				selectionHasMatrices ? 1 : 0;
		} );

		selectionComponent.OriginHandle.Dragged += _ => lastPosition = selectionComponent.TransformProps.Position;
	}

	bool matrixChanged;
	DrawInfo lastMatrices;
	Vector2 lastPosition;
	void onMatrixChanged () {
		matrixChanged = true;
	}

	protected override void Update () {
		base.Update();

		if ( matrixChanged ) {
			var drawInfo = selectionComponent.TransformProps.LocalDrawInfo;
			var matrix = lastMatrices.MatrixInverse * drawInfo.Matrix;
			var delta = selectionComponent.TransformProps.Position - lastPosition;

			var origin = selectionComponent.TransformProps.Position;
			perform( IHasMatrix.From, ( i, c ) => {
				var pos = IHasPosition.From( c )!;
				pos.X.Value += delta.X - origin.X;
				pos.Y.Value += delta.Y - origin.Y;

				i.Matrix *= matrix;

				pos.X.Value += origin.X;
				pos.Y.Value += origin.Y;
			} );

			lastPosition = selectionComponent.TransformProps.Position;
			lastMatrices = drawInfo;
			matrixChanged = false;
		}
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
	BasicTransformBlueprint<SelectionComponent> selectionComponent;
	SelectionBox selectionBox => selectionComponent.SelectionBox;

	Container<DrawableSelection> selections;
	Blueprint<IComponent>? blueprint;
	Stack<DrawableSelection> selectionPool = new();
	Dictionary<IComponent, DrawableSelection> visibleSelections = new();
	protected override void LoadComplete () {
		base.LoadComplete();

		Selection.BindCollectionChanged( ( _, _ ) => {
			selectionComponent.TransformProps.MatrixChanged -= onMatrixChanged;

			if ( blueprint != null ) {
				RemoveInternal( blueprint );
				blueprint = null;
			}
			selectionComponent.Alpha = Selection.Count > 1 ? 1 : 0;
			if ( Selection.Count < 2 && Selection.SingleOrDefault() is IComponent comp ) {
				AddInternal( blueprint = comp.CreateBlueprint() );
			}

			if ( Selection.Count < 2 )
				return;

			var box = Composer.ToContentSpace( Selection.Select( x => x.AsDrawable() ).GetBoundingBox( x => x.ScreenSpaceDrawQuad.AABBFloat ) ).AABBFloat;
			var props = selectionComponent.TransformProps;
			props.X.Value = box.Location.X;
			props.Y.Value = box.Location.Y;
			props.Width.Value = box.Width;
			props.Height.Value = box.Height;
			props.ScaleX.Value = 1;
			props.ScaleY.Value = 1;
			props.Rotation.Value = 0;
			props.ShearX.Value = 0;
			props.ShearY.Value = 0;
			props.OriginX.Value = 0;
			props.OriginY.Value = 0;

			lastPosition = selectionComponent.TransformProps.Position;
			lastMatrices = props.LocalDrawInfo;
			selectionComponent.TransformProps.MatrixChanged += onMatrixChanged;
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

	private class SelectionComponent : Box, IComponent {
		public readonly TransformProps TransformProps;
		public SelectionComponent () {
			Alpha = 0.1f;
			AlwaysPresent = true;
			Colour = Color4.Red;
			TransformProps = new( this );
		}

		public Blueprint<IComponent> CreateBlueprint ()
			=> new BasicTransformBlueprint<SelectionComponent>( this, TransformProps );
		string IComponent.Name => Name;
		public IEnumerable<IProp> Properties => TransformProps;
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

	public void UpdateCursorStyles ( float rotation, bool isShearing, Vector2 shear = default ) {
		var rightX = 1f + shear.X * shear.Y;
		var rightY = -shear.Y;
		var rightAngle = MathF.Atan2( rightY, rightX ).ToDegrees();

		var bottomX = -shear.X;
		var bottomY = 1f;
		var bottomAngle = MathF.Atan2( bottomY, bottomX ).ToDegrees() + 90;

		var diagX = rightX + bottomX;
		var diagY = rightY + bottomY;
		var diagAngle = MathF.Atan2( diagY, diagX ).ToDegrees() - 45;

		if ( isShearing ) {
			Left.CursorRotation = rotation + bottomAngle;
			Right.CursorRotation = rotation + bottomAngle;
			Top.CursorRotation = rotation + 90 + rightAngle;
			Bottom.CursorRotation = rotation + 90 + rightAngle;

			Left.CursorStyle = CursorStyle.Shear;
			Right.CursorStyle = CursorStyle.Shear;
			Top.CursorStyle = CursorStyle.Shear;
			Bottom.CursorStyle = CursorStyle.Shear;
		}
		else {
			Left.CursorRotation = rotation + rightAngle;
			Right.CursorRotation = rotation + rightAngle;
			Top.CursorRotation = rotation + bottomAngle;
			Bottom.CursorRotation = rotation + bottomAngle;

			Left.CursorStyle = CursorStyle.ResizeHorizontal;
			Right.CursorStyle = CursorStyle.ResizeHorizontal;
			Top.CursorStyle = CursorStyle.ResizeVertical;
			Bottom.CursorStyle = CursorStyle.ResizeVertical;
		}

		TopRight.CursorRotation = rotation + diagAngle;
		TopLeft.CursorRotation = rotation + diagAngle;
		BottomRight.CursorRotation = rotation + diagAngle;
		BottomLeft.CursorRotation = rotation + diagAngle;

		FarTopLeft.CursorRotation = rotation;
		FarTopRight.CursorRotation = rotation + 90;
		FarBottomRight.CursorRotation = rotation + 180;
		FarBottomLeft.CursorRotation = rotation + 270;
	}
}