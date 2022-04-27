using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using osuTK.Input;

namespace OsuFrameworkDesigner.Game.Tools;

public abstract class ShapeTool<T> : Tool where T : Drawable, IComponent {
	protected abstract T CreateShape ();
	protected virtual void UpdateShape ( T shape, Vector2 start, Vector2 end ) { }

	T? shape;
	Vector2 dragStartPosition;
	protected override bool OnDragStart ( DragStartEvent e ) {
		dragFinished = false;

		dragStartPosition = Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition );
		Composer.Add( shape = CreateShape().With( s => {
			s.Colour = Colour4.Green;
		} ) );

		var pos = e.AltPressed ? dragStartPosition : dragStartPosition.Round();
		UpdateShape( shape, pos, pos );
		return true;
	}

	protected override void OnDrag ( DragEvent e ) {
		var end = Composer.ToContentSpace( e.ScreenSpaceMousePosition );
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
		if ( !e.ShiftPressed ) {
			Composer.Tool.Value = Composer.SelectionTool;
		}
	}

	public override void BeginUsing () {
		Composer.SelectionTool.Selection.Clear();
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
