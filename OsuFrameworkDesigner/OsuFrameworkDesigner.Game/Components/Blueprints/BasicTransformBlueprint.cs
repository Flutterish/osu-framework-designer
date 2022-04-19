using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class BasicTransformBlueprint<T> : Blueprint<IComponent> where T : IComponent {
	SelectionBox box;
	new public T Value => (T)base.Value;
	public TransformProps TransformProps;

	public Vector2 ToTargetSpace ( Vector2 screenSpacePosition )
		=> new Vector2( TransformProps.X.Value, TransformProps.Y.Value ) + Value.AsDrawable().ToLocalSpace( screenSpacePosition );

	public BasicTransformBlueprint ( T value, TransformProps props ) : base( value ) {
		AddInternal( box = new SelectionBox().Fill() );
		TransformProps = props;

		box.TopLeft.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		box.TopRight.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		box.BottomLeft.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		box.BottomRight.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetRightEdge( e.AltPressed ? x : x.Round() );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		box.Bottom.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetBottomEdge( e.AltPressed ? y : y.Round() );
		};
		box.Top.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetTopEdge( e.AltPressed ? y : y.Round() );
		};
		box.Left.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
			TransformProps.SetLeftEdge( e.AltPressed ? x : x.Round() );
		};
		box.Right.Dragged += e => {
			var (x, y) = ToTargetSpace( e.ScreenSpaceMousePosition );
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

	private void dragEnded ( DragEndEvent e ) {
		TransformProps.Normalize();
	}

	float startAngle;
	private void onRotationDragStarted ( DragStartEvent e ) {
		startAngle = TransformProps.Rotation.Value;
	}

	private void onRotationDrag ( DragEvent e ) {
		var center = Value.AsDrawable().ScreenSpaceDrawQuad.TopLeft;
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
		Size = Value.AsDrawable().DrawSize * scale;
		Rotation = TransformProps.Rotation.Value;
	}
}
