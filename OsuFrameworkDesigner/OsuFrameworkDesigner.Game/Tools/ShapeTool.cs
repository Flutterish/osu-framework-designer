using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Tools;

public abstract class ShapeTool<T> : Tool where T : Drawable, IComponent {
	protected abstract T CreateShape ();
	protected virtual void UpdateShape ( T shape, Vector2 start, Vector2 end ) { }

	T? shape;
	Vector2 dragStartPosition;
	protected override bool OnDragStart ( DragStartEvent e ) {
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

	protected override void OnDragEnd ( DragEndEvent e ) {
		Composer.SelectionTool.Selection.Add( shape! );
		if ( !e.ShiftPressed ) {
			Composer.Tool.Value = Composer.SelectionTool;
		}
	}

	public override void BeginUsing () {
		Composer.SelectionTool.Selection.Clear();
	}
}
