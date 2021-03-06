using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Containers;
using OsuFrameworkDesigner.Game.Cursor;
using System.Diagnostics.CodeAnalysis;

namespace OsuFrameworkDesigner.Game.Tools;

public class Handle : CompositeDrawable, IUsesCursorStyle, IUsesCursorRotation, IHasTooltip {
	[Resolved( canBeNull: true )]
	protected Blueprint<IComponent>? Blueprint { get; private set; }
	[Resolved( canBeNull: true )]
	protected Composer? Composer { get; private set; }
	protected IEnumerable<IComponent> Source
		=> Blueprint is ISelection s ? s.SelectedComponents : Blueprint?.Value.Yield() ?? Array.Empty<IComponent>();

	[MemberNotNullWhen( true, nameof( Composer ) )]
	protected bool HandlesSnapEvents => CanHandleSnapEvents && ( SnapDragged != null || SnapDragStarted != null || SnapDragEnded != null );
	protected bool CanHandleSnapEvents => Composer != null;

	Vector2 snap ( Vector2 screenSpacePoint, UIEvent? context ) {
		var point = Composer!.ToContentSpace( screenSpacePoint );

		if ( Type is HandleType.Point ) {
			return Composer.Snap( point, Source, context );
		}
		else /* if ( Type is HandleType.Line ) */ {
			if ( LineSnapDirection is float dir ) {
				var direction = Composer.ToContentSpace( ScreenSpaceDrawQuad.TopLeft ) - Composer.ToContentSpace( ScreenSpaceDrawQuad.TopRight );
				return Composer.Snap( point, direction.Rotate( dir ), Source, context );
			}
			else if ( DrawWidth > DrawHeight ) {
				var direction = Composer.ToContentSpace( ScreenSpaceDrawQuad.TopLeft ) - Composer.ToContentSpace( ScreenSpaceDrawQuad.TopRight );
				return Composer.Snap( point, direction, Source, context );
			}
			else {
				var direction = Composer.ToContentSpace( ScreenSpaceDrawQuad.TopLeft ) - Composer.ToContentSpace( ScreenSpaceDrawQuad.BottomLeft );
				return Composer.Snap( point, direction, Source, context );
			}
		}
	}

	SnapResult currentSnappedDragState;
	protected override bool OnDragStart ( DragStartEvent e ) {
		if ( HandlesSnapEvents ) {
			Composer.ShowSnaps = true;
			var downPosition = snap( e.ScreenSpaceMouseDownPosition, e );
			var position = snap( e.ScreenSpaceMousePosition, e );
			currentSnappedDragState = new() {
				DownPosition = downPosition,
				LastPosition = downPosition,
				Position = position
			};

			SnapDragStarted?.Invoke( currentSnappedDragState );
		}
		DragStarted?.Invoke( e );
		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		if ( HandlesSnapEvents ) {
			var position = snap( e.ScreenSpaceMousePosition, e );
			currentSnappedDragState = currentSnappedDragState with {
				LastPosition = currentSnappedDragState.Position,
				Position = position
			};

			SnapDragged?.Invoke( currentSnappedDragState );
		}
		Dragged?.Invoke( e );
	}

	protected override void OnDragEnd ( DragEndEvent e ) {
		if ( HandlesSnapEvents ) {
			Composer.ShowSnaps = false;
			var position = snap( e.ScreenSpaceMousePosition, e );
			currentSnappedDragState = currentSnappedDragState with {
				LastPosition = currentSnappedDragState.Position,
				Position = position
			};

			SnapDragEnded?.Invoke( currentSnappedDragState );
		}
		DragEnded?.Invoke( e );
	}

	public void HandleSnappedTranslate (
		Func<List<LineGuide>, List<HandlePointGuide>, Vector2> populateGuides,
		Action<Vector2> drag
	) {
		List<LineGuide> lineGuides = new();
		List<HandlePointGuide> pointGuides = new();
		Vector2 offset = Vector2.Zero;

		SnapDragStarted += e => {
			lineGuides.Clear();
			pointGuides.Clear();
			offset = populateGuides( lineGuides, pointGuides );
		};
		Dragged += e => {
			var delta = Composer!.ToContentSpace( e.ScreenSpaceMousePosition ) - Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition );
			var target = offset + delta;

			foreach ( var p in pointGuides ) {
				var point = p.Point + delta;
				var snapped = Composer.Snap( point, Source, out var success, e, snapLines: p.SnapToLines );
				if ( success ) {
					drag( target + snapped - point );
					return;
				}
			}

			foreach ( var line in lineGuides ) {
				var point = line.StartPoint + delta;
				var snapped = Composer.Snap( point, line.Direction, Source, out var success, e, snapPoints: false );
				if ( success ) {
					drag( target + snapped - point );
					return;
				}
			}

			drag( Composer.Snap( target, Source, e ) );
		};
	}

	public virtual void ClearEvents () {
		DragStarted = null;
		Dragged = null;
		DragEnded = null;
		SnapDragStarted = null;
		SnapDragged = null;
		SnapDragEnded = null;
	}

	public event Action<DragStartEvent>? DragStarted;
	public event Action<DragEvent>? Dragged;
	public event Action<DragEndEvent>? DragEnded;
	public event Action<SnapResult>? SnapDragStarted;
	public event Action<SnapResult>? SnapDragged;
	public event Action<SnapResult>? SnapDragEnded;

	public HandleType Type = HandleType.Point;
	public float? LineSnapDirection;
	public CursorStyle CursorStyle { get; set; }
	public float CursorRotation { get; set; }
	public LocalisableString TooltipText { get; set; }
}

public enum HandleType {
	Point,
	Line
}

public struct HandlePointGuide {
	public Vector2 Point;
	public bool SnapToLines;

	public static implicit operator HandlePointGuide ( Vector2 p )
		=> new() { Point = p };
}

/// <summary>
/// A mouse event in content space
/// </summary>
public readonly struct SnapResult {
	public Vector2 DownPosition { get; init; }
	public Vector2 LastPosition { get; init; }
	public Vector2 Position { get; init; }
}

public class CornerHandle : Handle {
	Container border;
	Box background;
	Bindable<Colour4> backgroundColor = new( Theme.SelectionHandleDefault );
	Bindable<Colour4> selectionColor = new( Theme.SelectionDefault );

	public CornerHandle () {
		Origin = Anchor.Centre;
		AddInternal( border = new Container {
			Size = new( 12 ),
			Child = background = new Box().Fill(),
			Masking = true,
			BorderThickness = 4
		}.Center() );
		Size = new( 28 );
	}

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.SelectionHandle );
		selectionColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		selectionColor.BindValueChanged( v => border.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}

public class PointHandle : Handle {
	Circle background;
	Bindable<Colour4> backgroundColor = new( Theme.SelectionHandleDefault );
	Bindable<Colour4> selectionColor = new( Theme.SelectionDefault );

	public PointHandle () {
		Origin = Anchor.Centre;
		AddInternal( new Container {
			Size = new( 16 ),
			Child = background = new Circle { BorderThickness = 4 }.Fill(),
		}.Center() );
		Size = new( 24 );
	}

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.SelectionHandle );
		selectionColor.BindTo( colours.Selection );
		background.FadeColour( backgroundColor );
		selectionColor.BindValueChanged( v => background.BorderColour = v.NewValue, true );
		FinishTransforms( true );
	}
}