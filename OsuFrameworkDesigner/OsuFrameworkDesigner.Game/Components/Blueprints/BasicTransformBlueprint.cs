using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class BasicTransformBlueprint<T> : Blueprint<IComponent> where T : IComponent {
	SelectionBox box;
	OriginHandle origin;
	new public T Value => (T)base.Value;
	public TransformProps TransformProps;

	public Vector2 ToTargetSpace ( Vector2 screenSpacePosition )
		=> Value.AsDrawable().ToLocalSpace( screenSpacePosition );

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

	public BasicTransformBlueprint ( T value, TransformProps props ) : base( value ) {
		AddInternal( box = new SelectionBox().Fill() );
		AddInternal( origin = new OriginHandle { CursorStyle = Cursor.CursorStyle.ResizeOrthogonal } );
		TransformProps = props;

		box.TopLeft.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		box.TopRight.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		box.BottomLeft.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		box.BottomRight.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		box.Bottom.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		box.Top.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		box.Left.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
		};
		box.Right.Dragged += e => {
			var (x, y) = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
		};
		Vector2 boxDragHandle = Vector2.Zero;
		box.DragStarted += e => boxDragHandle = new( TransformProps.X.Value, TransformProps.Y.Value );
		box.Dragged += e => {
			e.Target = Value.AsDrawable();
			var delta = boxDragHandle + ( e.MousePosition - e.MouseDownPosition );
			TransformProps.X.Value = e.AltPressed ? delta.X : delta.X.Round();
			TransformProps.Y.Value = e.AltPressed ? delta.Y : delta.Y.Round();
		};
		origin.Dragged += e => {
			var pos = Vector2.Divide( Value.AsDrawable().ToLocalSpace( e.ScreenSpaceMousePosition ), Value.AsDrawable().DrawSize );
			TransformProps.SetOrigin( e.AltPressed ? pos : pos.Round( 0.5f ).Clamp( Vector2.Zero, Vector2.One ) );
		};

		box.FarBottomLeft.DragStarted += onRotationDragStarted;
		box.FarBottomRight.DragStarted += onRotationDragStarted;
		box.FarTopLeft.DragStarted += onRotationDragStarted;
		box.FarTopRight.DragStarted += onRotationDragStarted;
		box.FarBottomLeft.Dragged += onRotationDrag;
		box.FarBottomRight.Dragged += onRotationDrag;
		box.FarTopLeft.Dragged += onRotationDrag;
		box.FarTopRight.Dragged += onRotationDrag;

		box.TopLeft.DragEnded += dragEnded;
		box.TopRight.DragEnded += dragEnded;
		box.BottomLeft.DragEnded += dragEnded;
		box.BottomRight.DragEnded += dragEnded;
		box.Left.DragEnded += dragEnded;
		box.Right.DragEnded += dragEnded;
		box.Top.DragEnded += dragEnded;
		box.Bottom.DragEnded += dragEnded;
	}

	protected override void Update () {
		base.Update();
		updateOrigin();
	}

	void updateOrigin () {
		origin.Position = DrawSize * TransformProps.RelativeOrigin;
	}

	private void dragEnded ( DragEndEvent e ) {
		TransformProps.Normalize();
	}

	float startAngle;
	private void onRotationDragStarted ( DragStartEvent e ) {
		startAngle = TransformProps.Rotation.Value;
	}

	private void onRotationDrag ( DragEvent e ) {
		var center = Value.AsDrawable().ToScreenSpace( TransformProps.RelativeOrigin * Value.AsDrawable().DrawSize );
		var startDiff = e.ScreenSpaceMouseDownPosition - center;
		var diff = e.ScreenSpaceMousePosition - center;

		var startAngle = MathF.Atan2( startDiff.Y, startDiff.X ) / MathF.PI * 180;
		var angle = MathF.Atan2( diff.Y, diff.X ) / MathF.PI * 180;
		angle = this.startAngle + angle - startAngle;
		TransformProps.Rotation.Value = e.AltPressed ? angle : angle.Round();
	}

	protected override void PositionSelf () {
		var topLeft = Parent.ToLocalSpace( Value.AsDrawable().ScreenSpaceDrawQuad.TopLeft );
		Position = topLeft;
		var a = ToLocalSpace( Composer.ContentToScreenSpace( Vector2.Zero ) );
		var b = ToLocalSpace( Composer.ContentToScreenSpace( new( 1, 0 ) ) );
		var scale = ( a - b ).Length;
		Size = Value.AsDrawable().DrawSize * Value.AsDrawable().Scale * scale;
		//Shear = Value.AsDrawable().Shear;
		Rotation = TransformProps.Rotation.Value;

		box.TopRight.CursorRotation = Rotation;
		box.TopLeft.CursorRotation = Rotation;
		box.BottomRight.CursorRotation = Rotation;
		box.BottomLeft.CursorRotation = Rotation;
		box.Left.CursorRotation = Rotation;
		box.Right.CursorRotation = Rotation;
		box.Top.CursorRotation = Rotation;
		box.Bottom.CursorRotation = Rotation;

		box.FarTopLeft.CursorRotation = Rotation;
		box.FarTopRight.CursorRotation = Rotation + 90;
		box.FarBottomRight.CursorRotation = Rotation + 180;
		box.FarBottomLeft.CursorRotation = Rotation + 270;

		if ( Width < 0 ) {
			X += MathF.Cos( Rotation / 180 * MathF.PI ) * Width;
			Y += MathF.Sin( Rotation / 180 * MathF.PI ) * Width;
			Width = -Width;
		}

		if ( Height < 0 ) {
			Y += MathF.Sin( ( Rotation + 90 ) / 180 * MathF.PI ) * Height;
			X += MathF.Cos( ( Rotation + 90 ) / 180 * MathF.PI ) * Height;
			Height = -Height;
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