namespace OsuFrameworkDesigner.Game.Containers;

public class TransformContainer : TransformContainer<Drawable> { }
public class TransformContainer<T> : UnmaskableContainer<T> where T : Drawable {
	UnmaskableContainer<T> moveContainer;
	protected override Container<T> Content => moveContainer;

	public TransformContainer () {
		AddInternal( moveContainer = new UnmaskableContainer<T>().Center() );
	}

	new public Vector2 Position {
		get => -moveContainer.Position;
		set => moveContainer.Position = -value;
	}
}

public class UnmaskableContainer : UnmaskableContainer<Drawable> { }
public class UnmaskableContainer<T> : Container<T> where T : Drawable {
	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds )
		=> false;
}