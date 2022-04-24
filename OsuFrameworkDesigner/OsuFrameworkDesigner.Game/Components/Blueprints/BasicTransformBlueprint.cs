using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class BasicTransformBlueprint<T> : Blueprint<IComponent> where T : IComponent {
	public readonly SelectionBox SelectionBox;
	OriginHandle origin;
	new public T Value => (T)base.Value;
	public TransformProps TransformProps;

	public Vector2 ToTargetSpace ( Vector2 screenSpacePosition )
		=> TransformProps.ToLocalSpace( Composer.ToContentSpace( screenSpacePosition ) );

	/// <summary>
	/// Transforms screen space coordinates to target local space where (TransformProps.X, Y) is the origin of the drawable
	/// </summary>
	public Vector2 ToTargetOriginSpace ( Vector2 screenSpacePosition )
		=> TransformProps.Position + ToTargetSpace( screenSpacePosition );

	/// <summary>
	/// Transforms screen space coordinates to target local space where (TransformProps.X, Y) is the top left of the drawable
	/// </summary>
	public Vector2 ToTargetTopLeftSpace ( Vector2 screenSpacePosition )
		=> TransformProps.Position - TransformProps.RelativeOrigin * TransformProps.Size + ToTargetSpace( screenSpacePosition );

	public Vector2 Unshear ( Vector2 localPosition ) {
		var lx = localPosition.X;
		var ly = localPosition.Y;
		var sx = TransformProps.ShearX.Value;
		var sy = TransformProps.ShearY.Value;

		var x = lx - ly * sx;
		var y = ly + x * sy;

		return new( x, y );
	}

	public BasicTransformBlueprint ( T value, TransformProps props ) : base( value ) {
		AddInternal( SelectionBox = new SelectionBox().Fill() );
		AddInternal( origin = new OriginHandle { CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );
		TransformProps = props;

		SelectionBox.TopLeft.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		SelectionBox.TopRight.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		SelectionBox.BottomLeft.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		SelectionBox.BottomRight.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		SelectionBox.Bottom.Dragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ToTargetSpace( e.ScreenSpaceMousePosition ) );
				var (lx, ly) = Unshear( ToTargetSpace( e.ScreenSpaceLastMousePosition ) );
				TransformProps.ShearBottom( x - lx );
			}
			else {
				var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
				TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
			}
		};
		SelectionBox.Top.Dragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ToTargetSpace( e.ScreenSpaceMousePosition ) );
				var (lx, ly) = Unshear( ToTargetSpace( e.ScreenSpaceLastMousePosition ) );
				TransformProps.ShearTop( x - lx );
			}
			else {
				var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
				TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
			}
		};
		SelectionBox.Left.Dragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ToTargetSpace( e.ScreenSpaceMousePosition ) );
				var (lx, ly) = Unshear( ToTargetSpace( e.ScreenSpaceLastMousePosition ) );
				TransformProps.ShearLeft( y - ly );
			}
			else {
				var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
				TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			}
		};
		SelectionBox.Right.Dragged += e => {
			if ( isShearing ) {
				var (x, y) = Unshear( ToTargetSpace( e.ScreenSpaceMousePosition ) );
				var (lx, ly) = Unshear( ToTargetSpace( e.ScreenSpaceLastMousePosition ) );
				TransformProps.ShearRight( y - ly );
			}
			else {
				var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
				TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			}
		};
		Vector2 boxDragHandle = Vector2.Zero;
		SelectionBox.DragStarted += e => boxDragHandle = new( TransformProps.X.Value, TransformProps.Y.Value );
		SelectionBox.Dragged += e => {
			var delta = boxDragHandle + Composer.ToContentSpace( e.ScreenSpaceMousePosition ) - Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition );
			TransformProps.X.Value = e.AltPressed ? delta.X : delta.X.Round();
			TransformProps.Y.Value = e.AltPressed ? delta.Y : delta.Y.Round();
		};
		origin.Dragged += e => {
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

	protected override void Update () {
		base.Update();
		updateOrigin();
	}

	void updateOrigin () {
		origin.Position = DrawSize * TransformProps.RelativeOrigin;
	}

	private void dragStarted ( DragStartEvent e ) {
		isDragging = true;
	}
	private void dragEnded ( DragEndEvent e ) {
		isDragging = false;
		TransformProps.Normalize();
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

		if ( !isDragging ) {
			isShearing = InputManager.CurrentState.Keyboard.ControlPressed;
		}

		SelectionBox.UpdateCursorStyles( Rotation, isShearing, Shear );

		if ( Height < 0 ) {
			var cos = MathF.Cos( ( Rotation + 90 ) / 180 * MathF.PI );
			var sin = MathF.Sin( ( Rotation + 90 ) / 180 * MathF.PI );
			X += ( cos - sin * Shear.X ) * Height;
			Y += ( sin + cos * Shear.X ) * Height;

			Height = -Height;
		}

		if ( Width < 0 ) {
			var cos = MathF.Cos( Rotation / 180 * MathF.PI );
			var sin = MathF.Sin( Rotation / 180 * MathF.PI );
			X += ( cos + sin * Shear.Y ) * Width;
			Y += ( sin - cos * Shear.Y ) * Width;
			// shear is non-commutative
			var total = Shear.X * Shear.Y;
			X += cos * total * Width;
			Y += sin * total * Width;

			Width = -Width;
		}
	}

	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds ) {
		return !origin.ScreenSpaceDrawQuad.AABBFloat.IntersectsWith( maskingBounds ) && base.ComputeIsMaskedAway( maskingBounds );
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