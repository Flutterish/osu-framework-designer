namespace OsuFrameworkDesigner.Game.Extensions;

public static class FluentUIExtensions {
	public static T Center<T> ( this T self ) where T : Drawable {
		self.Anchor = Anchor.Centre;
		self.Origin = Anchor.Centre;
		return self;
	}

	public static T FillX<T> ( this T self ) where T : Container<Drawable> {
		self.RelativeSizeAxes = Axes.X;
		self.AutoSizeAxes = Axes.Y;
		return self;
	}

	public static T FillY<T> ( this T self ) where T : Container<Drawable> {
		self.RelativeSizeAxes = Axes.Y;
		self.AutoSizeAxes = Axes.X;
		return self;
	}

	public static T Fill<T> ( this T self ) where T : Drawable {
		self.RelativeSizeAxes = Axes.Both;
		return self;
	}

	public static T Fit<T> ( this T self, float aspectRatio = 1 ) where T : Drawable {
		self.RelativeSizeAxes = Axes.Both;
		self.FillMode = FillMode.Fit;
		self.FillAspectRatio = aspectRatio;
		return self;
	}

	public static T Vertical<T> ( this T self ) where T : FillFlowContainer<Drawable> {
		self.RelativeSizeAxes = Axes.X;
		self.AutoSizeAxes = Axes.Y;
		self.Direction = FillDirection.Vertical;
		return self;
	}

	public static T Horizontal<T> ( this T self ) where T : FillFlowContainer<Drawable> {
		self.RelativeSizeAxes = Axes.Y;
		self.AutoSizeAxes = Axes.X;
		self.Direction = FillDirection.Horizontal;
		return self;
	}

	public static T FilledVertical<T> ( this T self ) where T : FillFlowContainer<Drawable> {
		self.RelativeSizeAxes = Axes.Both;
		self.Direction = FillDirection.Vertical;
		return self;
	}

	public static T FilledHorizontal<T> ( this T self ) where T : FillFlowContainer<Drawable> {
		self.RelativeSizeAxes = Axes.Both;
		self.Direction = FillDirection.Horizontal;
		return self;
	}

	public static TContainer WithEachChild<TContainer, TChild> ( this TContainer container, Action<TChild, IEnumerable<TChild>> action )
			where TContainer : IEnumerable<Drawable>
			where TChild : Drawable {
		var children = container.OfType<TChild>();
		foreach ( var i in children )
			action( i, children );

		return container;
	}

	public static TContainer WithChildren<TContainer, TChild> ( this TContainer container, params TChild[] children )
			where TContainer : IContainerCollection<TChild>
			where TChild : Drawable {
		container.Children = children;

		return container;
	}
}
