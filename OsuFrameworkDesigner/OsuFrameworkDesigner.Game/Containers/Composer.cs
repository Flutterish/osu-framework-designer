﻿using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Tools;
using osuTK.Input;

namespace OsuFrameworkDesigner.Game.Containers;

[Cached]
public class Composer : CompositeDrawable {
	public readonly Bindable<Tool> Tool = new();
	Container<Tool> tools;

	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.ComposerBackgroundDefault );

	TransformContainer content;
	UnmaskableContainer components;
	UnmaskableContainer snapMarkers;

	SnapMarker marker1 = new();

	public readonly SelectionTool SelectionTool = new();

	public Composer () {
		this.Fill();
		Masking = true;
		AddInternal( background = new Box().Fill() );
		AddInternal( new DrawSizePreservingFillContainer {
			TargetDrawSize = new Vector2( 500 ),
			Child = content = new TransformContainer().Fit().Center().WithChildren(
				components = new UnmaskableContainer().Center(),
				snapMarkers = new UnmaskableContainer { Alpha = 0 }.Center()
			)
		}.Fill() );
		AddInternal( tools = new Container<Tool>().Fill() );
		AddInternal( snapMarkers.CreateProxy() );
	}

	public IEnumerable<IComponent> Components => components.OfType<IComponent>();
	public IEnumerable<IComponent> ComponentsReverse => components.Children.Reverse().OfType<IComponent>();
	public void Add<T> ( T component ) where T : Drawable, IComponent {
		components.Add( component );
		ComponentAdded?.Invoke( component, null );
	}
	public void Remove<T> ( T component ) where T : Drawable, IComponent {
		components.Remove( component );
		ComponentRemoved?.Invoke( component, null );
	}
	public void RemoveRange ( IEnumerable<IComponent> components ) {
		foreach ( var i in components ) {
			this.components.Remove( i.AsDrawable() );
			ComponentRemoved?.Invoke( i, null );
		}
	}

	public delegate void HierarchyChangedHandler ( IComponent component, IComponent? parent );
	public event HierarchyChangedHandler? ComponentRemoved;
	public event HierarchyChangedHandler? ComponentAdded;

	public IEnumerable<IComponent> ComponentsNear ( Vector2 point ) { // TODO use this point to optimize in the future
		return Components.SelectMany( x => x.GetNestedComponents() );
	}

	public Vector2 Snap ( Vector2 point, IComponent source, UIEvent? context = null )
		=> Snap( point, source.Yield(), context );
	public Vector2 Snap ( Vector2 point, IEnumerable<IComponent> source, UIEvent? context = null ) {
		snapMarkers.Clear( disposeChildren: false );

		if ( context?.ControlPressed == true )
			return point;

		foreach ( var i in ComponentsNear( point ).Except( source ).SelectMany( x => IHasSnapGuides.PointGuidesFrom( x, this ) ) ) {
			if ( i.IsInRange( point ) ) {
				snapMarkers.Add( marker1 );
				marker1.Position = i.Point;
				return i.Point;
			}
		}

		return point.Round();
	}

	bool showSnaps;
	public bool ShowSnaps {
		get => showSnaps;
		set {
			showSnaps = value;
			snapMarkers.Alpha = value ? 1 : 0;
		}
	}

	public Vector2 ToContentSpace ( Vector2 screenSpace )
		=> content.ToLocalSpace( screenSpace ) - content.DrawSize / 2 + content.Position;
	public Quad ToContentSpace ( Quad screenSpaceQuad ) {
		var offset = content.Position - content.DrawSize / 2;
		var quad = content.ToLocalSpace( screenSpaceQuad );

		return new Quad(
			quad.TopLeft + offset,
			quad.TopRight + offset,
			quad.BottomLeft + offset,
			quad.BottomRight + offset
		);
	}

	public Vector2 ContentToScreenSpace ( Vector2 contentSpace )
		=> content.ToScreenSpace( contentSpace - content.Position + content.DrawSize / 2 );

	protected override void LoadComplete () {
		base.LoadComplete();
		Tool.BindValueChanged( v => {
			var oldTool = v.OldValue;
			if ( oldTool != null ) {
				oldTool.StopUsing();
				oldTool.FadeOut( 200 );
			}

			var tool = v.NewValue;
			if ( tool != null ) {
				if ( tool.Parent == tools ) {
					tool.FadeIn( 200 );
				}
				else {
					tools.Add( tool );
					tool.FadeInFromZero( 200 );
				}
				tools.ChangeChildDepth( tool, (float)-Time.Current );
				tool.BeginUsing();
			}
		}, true );
	}

	bool isDragged;
	protected override bool OnMouseDown ( MouseDownEvent e ) {
		if ( e.Button is MouseButton.Middle ) {
			isDragged = true;
			return true;
		}

		return false;
	}

	public override bool Contains ( Vector2 screenSpacePos ) => isDragged || base.Contains( screenSpacePos );
	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		if ( isDragged ) {
			e.Target = content;
			content.Position -= Vector2.Divide( e.Delta, content.Scale );
			return true;
		}

		return base.OnMouseMove( e );
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		if ( e.Button is MouseButton.Middle ) {
			isDragged = false;
		}
		else base.OnMouseUp( e );
	}

	float zoom = 0;
	protected override bool OnScroll ( ScrollEvent e ) {
		zoom += e.ScrollDelta.Y / 4;
		content.Scale = new( MathF.Pow( 2, zoom ) );

		return true;
	}

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.ComposerBackground );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}
}
