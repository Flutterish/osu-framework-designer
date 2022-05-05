using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using osuTK.Input;

namespace OsuFrameworkDesigner.Game.Tools;

public abstract class ShapeTool<T> : Tool where T : Drawable, IComponent {
	protected abstract T CreateShape ();
	protected abstract void UpdateShape ( T shape, Vector2 start, Vector2 end );

	T? shape;
	Vector2 dragStartPosition;
	protected override bool OnDragStart ( DragStartEvent e ) {
		dragFinished = false;

		dragStartPosition = Composer.Snap( Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition ), Array.Empty<IComponent>(), e );
		shape = CreateShape().With( s => {
			s.Colour = Colour4.Green;
		} );

		shape.Name = shape.NameOrDefault( Composer );
		Composer.Add( shape );

		var pos = e.AltPressed ? dragStartPosition : dragStartPosition.Round();
		UpdateShape( shape, pos, pos );
		return true;
	}

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		Composer.Snap( Composer.ToContentSpace( e.ScreenSpaceMousePosition ), Array.Empty<IComponent>(), e );
		return base.OnMouseMove( e );
	}

	protected override void OnDrag ( DragEvent e ) {
		var end = Composer.Snap( Composer.ToContentSpace( e.ScreenSpaceMousePosition ), shape!, e );
		if ( e.AltPressed ) {
			UpdateShape( shape!, dragStartPosition, end );
		}
		else {
			UpdateShape( shape!, dragStartPosition.Round(), end.Round() );
		}
	}

	bool dragFinished = true; // due to drag end firing before mouse down, we need to keep track if it was interrupted or finished
	protected override void Update () {
		base.Update();

		if ( !IsDragged ) {
			dragFinished = true;
		}
	}
	protected override void OnDragEnd ( DragEndEvent e ) {
		Composer.SelectionTool.Selection.Add( shape! );
		Composer.TrackedProps.Flush( shape! );
		if ( !e.ShiftPressed ) {
			Composer.Tool.Value = Composer.SelectionTool;
		}
	}

	public override void BeginUsing () {
		Composer.ShowSnaps = true;
		Composer.SelectionTool.Selection.Clear();
	}

	public override void StopUsing () {
		Composer.ShowSnaps = false;
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		if ( e.Button is MouseButton.Right ) {
			if ( shape != null && !dragFinished ) {
				Composer.Remove( shape );
				Composer.SelectionTool.Selection.Remove( shape );
				shape = null;
			}
			Composer.Tool.Value = Composer.SelectionTool;
			return true;
		}

		return base.OnMouseDown( e );
	}
}
