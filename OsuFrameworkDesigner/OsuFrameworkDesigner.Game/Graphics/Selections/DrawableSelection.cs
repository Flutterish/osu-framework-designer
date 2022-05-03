using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Graphics.Selections;

public abstract class DrawableSelection : CompositeDrawable {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	public Drawable Selection { get; private set; } = null!;

	protected virtual void OnApply () { }
	public void Apply ( Drawable drawable ) {
		Selection = drawable;
		OnApply();
	}

	protected virtual void OnFree () { }
	public void Free () {
		OnFree();
		Selection = null!;
	}
}