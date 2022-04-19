using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class BasicTransformBlueprint<T> : Blueprint<IComponent> where T : IComponent {
	SelectionBox box;
	new public T Value => (T)base.Value;
	public TransformProps TransformProps;

	public BasicTransformBlueprint ( T value, TransformProps props ) : base( value ) {
		AddInternal( box = new SelectionBox().Fill() );
		TransformProps = props;

		box.TopLeft.Dragged += e => {
			e.Target = Value.AsDrawable();
			TransformProps.SetLeftEdge( e.MousePosition.X );
			TransformProps.SetTopEdge( e.MousePosition.Y );
		};
		box.TopRight.Dragged += e => {
			e.Target = Value.AsDrawable();
			TransformProps.SetRightEdge( e.MousePosition.X );
			TransformProps.SetTopEdge( e.MousePosition.Y );
		};
		box.BottomLeft.Dragged += e => {
			e.Target = Value.AsDrawable();
			TransformProps.SetLeftEdge( e.MousePosition.X );
			TransformProps.SetBottomEdge( e.MousePosition.Y );
		};
		box.BottomRight.Dragged += e => {
			e.Target = Value.AsDrawable();
			TransformProps.SetRightEdge( e.MousePosition.X );
			TransformProps.SetBottomEdge( e.MousePosition.Y );
		};

		box.TopLeft.DragEnded += dragEnded;
		box.TopRight.DragEnded += dragEnded;
		box.TopRight.DragEnded += dragEnded;
		box.BottomRight.DragEnded += dragEnded;
	}

	private void dragEnded ( DragEndEvent obj ) {
		TransformProps.Normalize();
	}
}
