namespace OsuFrameworkDesigner.Game.Cursor;

public enum CursorStyle {
	Default,
	Pointer,
	ResizeHorizontal,
	ResizeVertical,
	ResizeSW,
	ResizeNW,
	ResizeDiagonal,
	ResizeOrthogonal,
	Rotate
}

public interface IUsesCursorStyle : IDrawable {
	CursorStyle CursorStyle { get; }
}

public interface IUsesCursorRotation : IUsesCursorStyle {
	float CursorRotation { get; }
}