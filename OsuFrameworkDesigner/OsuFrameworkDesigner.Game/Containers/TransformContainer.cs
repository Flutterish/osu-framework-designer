using osu.Framework.Graphics.Primitives;

namespace OsuFrameworkDesigner.Game.Containers;

public class TransformContainer : TransformContainer<Drawable> { }
public class TransformContainer<T> : Container<T> where T : Drawable {
	MoveContainer moveContainer;
	protected override Container<T> Content => moveContainer;

	public TransformContainer () {
		AddInternal( moveContainer = new MoveContainer().Center() );
	}

	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> false;

	new public Vector2 Position {
		get => -moveContainer.Position;
		set => moveContainer.Position = -value;
	}

	private class MoveContainer : Container<T> {
		protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
			=> false;
	}
}
