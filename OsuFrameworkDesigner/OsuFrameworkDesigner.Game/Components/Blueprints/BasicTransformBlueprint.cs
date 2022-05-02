using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

// TODO translating snaps the origin, but not any other point
public abstract class BasicTransformBlueprint<T> : Blueprint<IComponent> where T : IComponent {
	public readonly SelectionBox SelectionBox;
	public readonly OriginHandle OriginHandle;
	new public T Value => (T)base.Value;
	public abstract TransformProps TransformProps { get; }
	public bool ResizingScales = false;
	public bool CanShear = true;

	public Vector2 ToTargetSpace ( Vector2 screenSpacePosition )
		=> TransformProps.ToLocalSpace( Composer.ToContentSpace( screenSpacePosition ) );

	public Vector2 ContentToTargetSpace ( Vector2 contentSpacePosition )
		=> TransformProps.ToLocalSpace( contentSpacePosition );

	/// <summary>
	/// Transforms screen space coordinates to target local space where (TransformProps.X, Y) is the origin of the drawable
	/// </summary>
	public Vector2 ToTargetOriginSpace ( Vector2 screenSpacePosition )
		=> TransformProps.Position - TransformProps.Size * TransformProps.RelativeOrigin + ToTargetSpace( screenSpacePosition );

	/// <summary>
	/// Transforms screen space coordinates to target local space where (TransformProps.X, Y) is the top left of the drawable
	/// </summary>
	public Vector2 ToTargetTopLeftSpace ( Vector2 screenSpacePosition )
		=> TransformProps.Position + ToTargetSpace( screenSpacePosition );

	/// <summary>
	/// Transforms content space coordinates to target local space where (TransformProps.X, Y) is the origin of the drawable
	/// </summary>
	public Vector2 ContentToTargetOriginSpace ( Vector2 contentSpacePosition )
		=> TransformProps.Position - TransformProps.Size * TransformProps.RelativeOrigin + ContentToTargetSpace( contentSpacePosition );

	/// <summary>
	/// Transforms content space coordinates to target local space where (TransformProps.X, Y) is the top left of the drawable
	/// </summary>
	public Vector2 ContentToTargetTopLeftSpace ( Vector2 contentSpacePosition )
		=> TransformProps.Position + ContentToTargetSpace( contentSpacePosition );

	public Vector2 ContentToLocalSpace ( Vector2 contentSpace )
		=> ToLocalSpace( Composer.ContentToScreenSpace( contentSpace ) );

	public Vector2 TargetToLocalSpace ( Vector2 local )
		=> ContentToLocalSpace( TransformProps.ToContentSpace( local ) );

	public Vector2 Unshear ( Vector2 localPosition ) {
		var lx = localPosition.X;
		var ly = localPosition.Y;
		var sx = TransformProps.ShearX.Value;
		var sy = TransformProps.ShearY.Value;

		var x = lx - ly * sx;
		var y = ly + x * sy;

		return new( x, y );
	}

	public BasicTransformBlueprint () {
		AddInternal( SelectionBox = new SelectionBox().Fill() );
		AddInternal( OriginHandle = new OriginHandle { CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );

		void setTop ( float y ) {
			if ( ResizingScales ) {
				var h = TransformProps.Height.Value;
				TransformProps.SetTopEdge( y );
				TransformProps.ScaleY.Value *= TransformProps.Height.Value / h;
				TransformProps.Height.Value = h;
			}
			else TransformProps.SetTopEdge( y );
		}
		void setBottom ( float y ) {
			if ( ResizingScales ) {
				var h = TransformProps.Height.Value;
				TransformProps.SetBottomEdge( y );
				TransformProps.ScaleY.Value *= TransformProps.Height.Value / h;
				TransformProps.Height.Value = h;
			}
			else TransformProps.SetBottomEdge( y );
		}
		void setLeft ( float x ) {
			if ( ResizingScales ) {
				var w = TransformProps.Width.Value;
				TransformProps.SetLeftEdge( x );
				TransformProps.ScaleX.Value *= TransformProps.Width.Value / w;
				TransformProps.Width.Value = w;
			}
			else TransformProps.SetLeftEdge( x );
		}
		void setRight ( float x ) {
			if ( ResizingScales ) {
				var w = TransformProps.Width.Value;
				TransformProps.SetRightEdge( x );
				TransformProps.ScaleX.Value *= TransformProps.Width.Value / w;
				TransformProps.Width.Value = w;
			}
			else TransformProps.SetRightEdge( x );
		}
		void shearLeft ( float dy ) {
			if ( ResizingScales ) {
				var w = TransformProps.Width.Value;
				var h = TransformProps.Height.Value;
				TransformProps.ShearLeft( dy );
				TransformProps.ScaleX.Value *= TransformProps.Width.Value / w;
				TransformProps.ScaleY.Value *= TransformProps.Height.Value / h;
				TransformProps.Width.Value = w;
				TransformProps.Height.Value = h;
			}
			else TransformProps.ShearLeft( dy );
		}
		void shearRight ( float dy ) {
			if ( ResizingScales ) {
				var w = TransformProps.Width.Value;
				var h = TransformProps.Height.Value;
				TransformProps.ShearRight( dy );
				TransformProps.ScaleX.Value *= TransformProps.Width.Value / w;
				TransformProps.ScaleY.Value *= TransformProps.Height.Value / h;
				TransformProps.Width.Value = w;
				TransformProps.Height.Value = h;
			}
			else TransformProps.ShearRight( dy );
		}
		void shearTop ( float dx ) {
			TransformProps.ShearTop( dx );
		}
		void shearBottom ( float dx ) {
			TransformProps.ShearBottom( dx );
		}

		SelectionBox.TopLeft.SnapDragged += e => {
			var (x, y) = ContentToTargetOriginSpace( e.Position );
			setLeft( x );
			setTop( y );
		};
		SelectionBox.TopRight.SnapDragged += e => {
			var (x, y) = ContentToTargetOriginSpace( e.Position );
			setRight( x );
			setTop( y );
		};
		SelectionBox.BottomLeft.SnapDragged += e => {
			var (x, y) = ContentToTargetOriginSpace( e.Position );
			setLeft( x );
			setBottom( y );
		};
		SelectionBox.BottomRight.SnapDragged += e => {
			var (x, y) = ContentToTargetOriginSpace( e.Position );
			setRight( x );
			setBottom( y );
		};
		SelectionBox.Bottom.SnapDragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ContentToTargetSpace( e.Position ) );
				var (lx, ly) = Unshear( ContentToTargetSpace( e.LastPosition ) );
				shearBottom( x - lx );
			}
			else {
				var (x, y) = ContentToTargetOriginSpace( e.Position );
				setBottom( y );
			}
		};
		SelectionBox.Top.SnapDragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ContentToTargetSpace( e.Position ) );
				var (lx, ly) = Unshear( ContentToTargetSpace( e.LastPosition ) );
				shearTop( x - lx );
			}
			else {
				var (x, y) = ContentToTargetOriginSpace( e.Position );
				setTop( y );
			}
		};
		SelectionBox.Left.SnapDragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ContentToTargetSpace( e.Position ) );
				var (lx, ly) = Unshear( ContentToTargetSpace( e.LastPosition ) );
				shearLeft( y - ly );
			}
			else {
				var (x, y) = ContentToTargetOriginSpace( e.Position );
				setLeft( x );
			}
		};
		SelectionBox.Right.SnapDragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ContentToTargetSpace( e.Position ) );
				var (lx, ly) = Unshear( ContentToTargetSpace( e.LastPosition ) );
				shearRight( y - ly );
			}
			else {
				var (x, y) = ContentToTargetOriginSpace( e.Position );
				setRight( x );
			}
		};
		SelectionBox.HandleSnappedTranslate( ( lines, points ) => {
			lines.Add( new() { StartPoint = TransformProps.TopCentre, EndPoint = TransformProps.BottomCentre } );
			lines.Add( new() { StartPoint = TransformProps.LeftCentre, EndPoint = TransformProps.RightCentre } );
			lines.Add( new() { StartPoint = TransformProps.TopLeft, EndPoint = TransformProps.TopRight } );
			lines.Add( new() { StartPoint = TransformProps.BottomLeft, EndPoint = TransformProps.BottomRight } );
			lines.Add( new() { StartPoint = TransformProps.TopLeft, EndPoint = TransformProps.BottomLeft } );
			lines.Add( new() { StartPoint = TransformProps.TopRight, EndPoint = TransformProps.BottomRight } );

			points.Add( TransformProps.TopLeft );
			points.Add( TransformProps.TopRight );
			points.Add( TransformProps.BottomLeft );
			points.Add( TransformProps.BottomRight );

			return TransformProps.Position;
		}, position => {
			TransformProps.Position = position;
		} );
		OriginHandle.Dragged += e => {
			var pos = Vector2.Divide( ToTargetSpace( e.ScreenSpaceMousePosition ), TransformProps.Size );
			TransformProps.SetOrigin( e.AltPressed ? pos : pos.Round( 0.5f ).Clamp( Vector2.Zero, Vector2.One ) );
		};

		SelectionBox.FarBottomLeft.DragStarted += onRotationDragStarted;
		SelectionBox.FarBottomRight.DragStarted += onRotationDragStarted;
		SelectionBox.FarTopLeft.DragStarted += onRotationDragStarted;
		SelectionBox.FarTopRight.DragStarted += onRotationDragStarted;
		SelectionBox.FarBottomLeft.Dragged += onRotationDrag;
		SelectionBox.FarBottomRight.Dragged += onRotationDrag;
		SelectionBox.FarTopLeft.Dragged += onRotationDrag;
		SelectionBox.FarTopRight.Dragged += onRotationDrag;

		SelectionBox.TopLeft.DragStarted += dragStarted;
		SelectionBox.TopRight.DragStarted += dragStarted;
		SelectionBox.BottomLeft.DragStarted += dragStarted;
		SelectionBox.BottomRight.DragStarted += dragStarted;
		SelectionBox.Left.DragStarted += dragStarted;
		SelectionBox.Right.DragStarted += dragStarted;
		SelectionBox.Top.DragStarted += dragStarted;
		SelectionBox.Bottom.DragStarted += dragStarted;
		SelectionBox.FarBottomLeft.DragStarted += dragStarted;
		SelectionBox.FarBottomRight.DragStarted += dragStarted;
		SelectionBox.FarTopLeft.DragStarted += dragStarted;
		SelectionBox.FarTopRight.DragStarted += dragStarted;

		SelectionBox.TopLeft.DragEnded += dragEnded;
		SelectionBox.TopRight.DragEnded += dragEnded;
		SelectionBox.BottomLeft.DragEnded += dragEnded;
		SelectionBox.BottomRight.DragEnded += dragEnded;
		SelectionBox.Left.DragEnded += dragEnded;
		SelectionBox.Right.DragEnded += dragEnded;
		SelectionBox.Top.DragEnded += dragEnded;
		SelectionBox.Bottom.DragEnded += dragEnded;
		SelectionBox.FarBottomLeft.DragEnded += dragEnded;
		SelectionBox.FarBottomRight.DragEnded += dragEnded;
		SelectionBox.FarTopLeft.DragEnded += dragEnded;
		SelectionBox.FarTopRight.DragEnded += dragEnded;
	}

	protected void DisableAllHandles () {
		SelectionBox.Top.Alpha =
		SelectionBox.Bottom.Alpha =
		SelectionBox.Left.Alpha =
		SelectionBox.Right.Alpha =
		SelectionBox.TopLeft.Alpha =
		SelectionBox.TopRight.Alpha =
		SelectionBox.BottomLeft.Alpha =
		SelectionBox.BottomRight.Alpha =
		SelectionBox.FarTopLeft.Alpha =
		SelectionBox.FarTopRight.Alpha =
		SelectionBox.FarBottomLeft.Alpha =
		SelectionBox.FarBottomRight.Alpha =
		OriginHandle.Alpha =
			0;
	}

	protected override void Update () {
		base.Update();
		updateOrigin();
	}

	void updateOrigin () {
		OriginHandle.Position = DrawSize * TransformProps.RelativeOrigin;
	}

	private void dragStarted ( DragStartEvent e ) {
		isDragging = true;
	}
	private void dragEnded ( DragEndEvent e ) {
		isDragging = false;
	}

	float startAngle;
	private void onRotationDragStarted ( DragStartEvent e ) {
		startAngle = TransformProps.Rotation.Value;
	}

	private void onRotationDrag ( DragEvent e ) {
		var center = Composer.ContentToScreenSpace( TransformProps.Position );
		var startDiff = e.ScreenSpaceMouseDownPosition - center;
		var diff = e.ScreenSpaceMousePosition - center;

		var startAngle = MathF.Atan2( startDiff.Y, startDiff.X ) / MathF.PI * 180;
		var angle = MathF.Atan2( diff.Y, diff.X ) / MathF.PI * 180;
		angle = this.startAngle + angle - startAngle;
		TransformProps.Rotation.Value = e.AltPressed ? angle : angle.Round();
	}

	InputManager? inputManager;
	InputManager InputManager => inputManager ??= GetContainingInputManager();
	bool isShearing;
	bool isDragging;
	protected override void PositionSelf () {
		var topLeft = Parent.ToLocalSpace( Composer.ContentToScreenSpace( TransformProps.TopLeft ) );
		Position = topLeft;
		var a = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Vector2.Zero ) );
		var b = Parent.ToLocalSpace( Composer.ContentToScreenSpace( new( 1, 0 ) ) );
		var scale = ( a - b ).Length;
		Size = TransformProps.Size * TransformProps.Scale * scale;
		Shear = TransformProps.Shear;
		Rotation = TransformProps.Rotation.Value;

		if ( !isDragging && CanShear ) {
			isShearing = InputManager.CurrentState.Keyboard.ControlPressed;
		}

		SelectionBox.UpdateCursorStyles( Rotation, isShearing, ( TransformProps.EffectiveHeight < 0 ) != ( TransformProps.EffectiveWidth < 0 ), Shear );

		Scale = Vector2.One;
		if ( Height < 0 ) {
			Height = -Height;
			Scale *= new Vector2( 1, -1 );
		}
		if ( Width < 0 ) {
			Width = -Width;
			Scale *= new Vector2( -1, 1 );
		}
	}

	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds ) {
		return !InternalChildren.Any( x => x.ScreenSpaceDrawQuad.AABBFloat.IntersectsWith( maskingBounds ) ) && base.ComputeIsMaskedAway( maskingBounds );
	}
}

public class OriginHandle : Handle {
	SpriteIcon icon;
	SpriteIcon iconBorder1;
	SpriteIcon iconBorder2;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SelectionHandleDefault );
	Bindable<Colour4> selectionColor = new( ColourConfiguration.SelectionDefault );

	public OriginHandle () {
		Origin = Anchor.Centre;
		AddInternal( new Container {
			Size = new( 18 )
		}.Center().WithChildren(
			iconBorder1 = new SpriteIcon { Icon = FontAwesome.Solid.Crosshairs, Size = new( 1.2f ) }.Fill().Center(),
			iconBorder2 = new SpriteIcon { Icon = FontAwesome.Solid.Crosshairs, Size = new( 0.7f ) }.Fill().Center(),
			icon = new SpriteIcon { Icon = FontAwesome.Solid.Crosshairs }.Fill().Center()
		) );
		Size = new( 24 );
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.SelectionHandle );
		selectionColor.BindTo( colours.Selection );
		icon.FadeColour( backgroundColor );
		iconBorder1.FadeColour( selectionColor );
		iconBorder2.FadeColour( selectionColor );
		FinishTransforms( true );
	}
}