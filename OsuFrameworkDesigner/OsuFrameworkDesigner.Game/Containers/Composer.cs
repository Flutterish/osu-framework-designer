using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.PolygonExtensions;
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

	public IEnumerable<IComponent> VisibleComponentsNear ( Vector2 point ) { // TODO use this point to optimize in the future
		return components.Where( x => x.ScreenSpaceDrawQuad.Intersects( ScreenSpaceDrawQuad ) ).OfType<IComponent>().SelectMany( x => x.GetNestedComponents() );
	}

	public IEnumerable<IComponent> ComponentsNear ( Vector2 point ) { // TODO use this point to optimize in the future
		return Components.SelectMany( x => x.GetNestedComponents() );
	}

	SnapMarker marker1 = new();
	SnapMarker marker2 = new();
	SnapLine lineMarker = new();
	SnapMarker marker3 = new();
	SnapMarker marker4 = new();
	SnapLine lineMarker2 = new();
	SnapMarker marker5 = new();

	public float SnapPixelThreshold => 8 / content.Scale.X;
	public float SnapThreshold => SnapPixelThreshold * SnapPixelThreshold;

	public bool TrySnap ( Vector2 point, IEnumerable<PointGuide> guides, out Vector2 snapped ) {
		snapMarkers.Clear( disposeChildren: false );

		if ( guides.Any() ) {
			var best = guides.MinBy( x => x.SnapRatingFor( point ) );
			if ( best.SnapRatingFor( point ) <= SnapThreshold ) {
				snapMarkers.Add( marker1 );
				marker1.Position = best.Point;
				snapped = best.Point;
				return true;
			}
		}

		snapped = point;
		return false;
	}

	public bool TrySnap ( Vector2 point, IEnumerable<LineGuide> guides, out Vector2 snapped, bool useIntersections = true ) {
		snapMarkers.Clear( disposeChildren: false );

		if ( guides.Any() ) {
			if ( useIntersections ) {
				var pairs = guides.SelectMany( ( x, i ) => guides.Skip( i + 1 ).Select( y => (First: x, Second: y) ) );

				var intersectionGuides = pairs
					.Where( lines => MathF.Abs( Vector2.Dot( lines.First.Direction.Normalized(), lines.Second.Direction.Normalized() ) ) < 0.9999f )
					.Select( lines => (
						guide: new PointGuide { Point = MathExtensions.IntersectLines( lines.First.StartPoint, lines.First.Direction, lines.Second.StartPoint, lines.Second.Direction ) },
						lines: lines
					) );

				if ( intersectionGuides.Any() ) {
					var bestIntersection = intersectionGuides.MinBy( x => x.guide.SnapRatingFor( point ) );
					if ( bestIntersection.guide.SnapRatingFor( point ) <= SnapThreshold ) {
						snapMarkers.Add( marker1 );
						snapMarkers.Add( marker2 );
						snapMarkers.Add( marker3 );
						snapMarkers.Add( marker4 );
						snapMarkers.Add( marker5 );
						snapMarkers.Add( lineMarker );
						snapMarkers.Add( lineMarker2 );
						marker1.Position = bestIntersection.lines.First.StartPoint;
						marker2.Position = bestIntersection.lines.First.EndPoint;
						marker3.Position = bestIntersection.lines.Second.StartPoint;
						marker4.Position = bestIntersection.lines.Second.EndPoint;
						marker5.Position = bestIntersection.guide.Point;
						lineMarker.Connect( bestIntersection.lines.First.StartPoint, bestIntersection.lines.First.EndPoint, bestIntersection.guide.Point );
						lineMarker2.Connect( bestIntersection.lines.Second.StartPoint, bestIntersection.lines.Second.EndPoint, bestIntersection.guide.Point );
						snapped = bestIntersection.guide.Point;
						return true;
					}
				}
			}

			var best = guides.MinBy( x => x.SnapRatingFor( point, out _ ) );
			if ( best.SnapRatingFor( point, out var snappedPoint ) <= SnapThreshold ) {
				snapMarkers.Add( marker1 );
				marker1.Position = best.StartPoint;
				snapMarkers.Add( marker2 );
				marker2.Position = best.EndPoint;
				snapMarkers.Add( lineMarker );
				lineMarker.Connect( best.StartPoint, best.EndPoint, snappedPoint );
				snapped = snappedPoint;
				return true;
			}
		}

		snapped = point;
		return false;
	}

	public Vector2 Snap ( Vector2 point, IComponent source, UIEvent? context = null, bool snapLines = true )
		=> Snap( point, source.Yield(), out _, context, snapLines );
	public Vector2 Snap ( Vector2 point, IComponent source, out bool snapped, UIEvent? context = null, bool snapLines = true )
		=> Snap( point, source.Yield(), out snapped, context, snapLines );
	public Vector2 Snap ( Vector2 point, IEnumerable<IComponent> source, UIEvent? context = null, bool snapLines = true )
		=> Snap( point, source, out _, context, snapLines );
	public Vector2 Snap ( Vector2 point, IEnumerable<IComponent> source, out bool snapped, UIEvent? context = null, bool snapLines = true ) {
		snapMarkers.Clear( disposeChildren: false );
		snapped = false;

		if ( context?.ControlPressed == true )
			return point;

		var snapSources = VisibleComponentsNear( point ).Except( source );
		var pointGuides = snapSources.SelectMany( x => IHasSnapGuides.PointGuidesFrom( x, this ) );
		if ( TrySnap( point, pointGuides, out var snappedPoint ) ) {
			snapped = true;
			return snappedPoint;
		}

		if ( snapLines ) {
			var lineGuides = snapSources.SelectMany( x => IHasSnapGuides.LineGuidesFrom( x, this ) );
			if ( TrySnap( point, lineGuides, out snappedPoint ) ) {
				snapped = true;
				return snappedPoint;
			}
		}

		return point.Round();
	}

	public Vector2 Snap ( Vector2 point, Vector2 direction, IComponent source, out bool snapped, UIEvent? context = null, bool snapPoints = true )
		=> Snap( point, direction, source.Yield(), out snapped, context, snapPoints );
	public Vector2 Snap ( Vector2 point, Vector2 direction, IComponent source, UIEvent? context = null, bool snapPoints = true )
		=> Snap( point, direction, source.Yield(), out _, context, snapPoints );
	public Vector2 Snap ( Vector2 point, Vector2 direction, IEnumerable<IComponent> source, UIEvent? context = null, bool snapPoints = true )
		=> Snap( point, direction, source, out _, context, snapPoints );
	public Vector2 Snap ( Vector2 point, Vector2 direction, IEnumerable<IComponent> source, out bool snapped, UIEvent? context = null, bool snapPoints = true ) {
		direction = direction.Normalized();
		snapMarkers.Clear( disposeChildren: false );
		snapped = false;

		if ( context?.ControlPressed == true )
			return point;

		var snapSources = VisibleComponentsNear( point ).Except( source );
		if ( snapPoints ) {
			var pointGuides = snapSources.SelectMany( x => IHasSnapGuides.PointGuidesFrom( x, this ) );
			if ( TrySnap( point, pointGuides, out var snappedPoint ) ) {
				snapped = true;
				return snappedPoint;
			}
		}

		var lineGuides = snapSources.SelectMany( x => IHasSnapGuides.LineGuidesFrom( x, this ) )
			.Where( x => MathF.Abs( Vector2.Dot( direction, x.Direction.Normalized() ) ) > 0.9999f );
		if ( TrySnap( point, lineGuides, out var snappedPoint2 ) ) {
			snapped = true;
			return snappedPoint2;
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
		zoom = Math.Clamp( zoom + e.ScrollDelta.Y / 4, -8, 8 );
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
