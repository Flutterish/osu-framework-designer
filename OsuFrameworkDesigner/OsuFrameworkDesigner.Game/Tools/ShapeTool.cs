using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Tools;

public abstract class ShapeTool<T> : Tool where T : Drawable, IComponent {
	protected abstract T CreateShape ();
	protected virtual void UpdateShape ( T shape ) { }

	T? shape;
	Vector2 dragStartPosition;
	protected override bool OnDragStart ( DragStartEvent e ) {
		dragStartPosition = Composer.ToContentSpace( e.ScreenSpaceMouseDownPosition );
		Composer.Add( shape = CreateShape().With( s => {
			s.Position = e.AltPressed ? dragStartPosition : dragStartPosition.Round();
			s.Colour = Colour4.Green;
		} ) );

		UpdateShape( shape );
		return true;
	}

	protected virtual Vector2 SizeAt ( Vector2 cursorDelta ) {
		return cursorDelta;
	}
	protected override void OnDrag ( DragEvent e ) {
		var size = SizeAt( Composer.ToContentSpace( e.ScreenSpaceMousePosition ) - dragStartPosition );
		if ( e.AltPressed ) {
			shape!.Position = dragStartPosition;
			shape.Size = size;
		}
		else {
			shape!.Position = dragStartPosition.Round();
			shape.Size = size.Round();
		}

		UpdateShape( shape );
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
