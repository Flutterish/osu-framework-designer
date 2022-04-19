﻿global using osu.Framework.Allocation;
global using osu.Framework.Bindables;
global using osu.Framework.Graphics;
global using osu.Framework.Graphics.Containers;
global using osu.Framework.Graphics.Shapes;
global using OsuFrameworkDesigner.Game.Dependencies;
global using osuTK;
global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace OsuFrameworkDesigner.Game;

public static class Extensions {
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

	public static T Center<T> ( this T self ) where T : Drawable {
		self.Anchor = Anchor.Centre;
		self.Origin = Anchor.Centre;
		return self;
	}

	public static T FadeColour<T> ( this T drawable, IBindable<Colour4> newColour, double duration = 100, Easing easing = Easing.Out ) where T : Drawable {
		newColour.BindValueChanged( v => {
			drawable.FadeColour( v.NewValue, duration, easing );
		}, true );
		return drawable;
	}
}
