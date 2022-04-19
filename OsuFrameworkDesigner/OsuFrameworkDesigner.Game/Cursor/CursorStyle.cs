namespace OsuFrameworkDesigner.Game.Cursor;

public enum CursorStyle {
	Default,
	Pointer,
	ResizeHorizontal,
	ResizeVertical,
	ResizeSW,
	ResizeNW,
	ResizeDiagonal,
	ResizeOrthogonal
}

public interface IUsesCursorStyle : IDrawable {
	CursorStyle CursorStyle { get; }
}