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
using osu.Framework.Graphics.Primitives;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;

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

	public static Drawable AsDrawable ( this IComponent component )
		=> (Drawable)component;

	public static RectangleF GetBoundingBox<T> ( this IEnumerable<T> self, Func<T, RectangleF> func ) {
		var rect = func( self.First() );
		var minX = rect.Left;
		var maxX = rect.Right;
		var minY = rect.Top;
		var maxY = rect.Bottom;

		foreach ( var i in self.Skip( 1 ).Select( func ) ) {
			if ( i.Left < minX )
				minX = i.Left;
			if ( i.Top < minY )
				minY = i.Top;
			if ( i.Right > maxX )
				maxX = i.Right;
			if ( i.Bottom > maxY )
				maxY = i.Bottom;
		}

		return new RectangleF( minX, minY, maxX - minX, maxY - minY );
	}

	public static IEnumerable<IProp> GetNestedProperties ( this IComponent component )
		=> component is IEnumerable<IComponent> container
			? component.Properties.Concat( container.SelectMany( GetNestedProperties ) )
			: component.Properties;

	public static Vector2 Round ( this Vector2 self )
		=> new( MathF.Round( self.X ), MathF.Round( self.Y ) );

	public static float Round ( this float self )
		=> MathF.Round( self );

	public static float Abs ( this float self )
		=> MathF.Abs( self );

	public static Vector2 ClosestPointToLine ( Vector2 startPoint, Vector2 direction, Vector2 point ) {
		Vector2 w = point - startPoint;

		float b = Vector2.Dot( w, direction ) / direction.LengthSquared;
		Vector2 pB = startPoint + b * direction;

		return pB;
	}

	public static float SignedDistance ( Vector2 startPoint, Vector2 direction, Vector2 point ) {
		direction.Normalize();
		Vector2 w = point - startPoint;

		return Vector2.Dot( w, direction );
	}

	public static void Deconstruct ( this Vector2 self, out float x, out float y ) {
		x = self.X;
		y = self.Y;
	}
}
