using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

[Cached]
public abstract class Blueprint<T> : ErrorBoundry {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	public T Value { get; private set; } = default!;

	protected virtual void OnApply () { }
	public void Apply ( T value ) {
		Value = value;
		OnApply();
	}

	protected virtual void OnFree () { }
	public void Free () {
		OnFree();
		Value = default!;
	}

	protected override void Update () {
		base.Update();
		PositionSelf();
	}

	protected virtual void PositionSelf () {
		if ( Value is Drawable drawable ) {
			PositionOnDrawable( drawable );
		}
	}

	protected void PositionOnDrawable ( Drawable drawable ) {
		(Position, Size, Shear, var rot) = Parent.ToLocalSpace( drawable.ScreenSpaceDrawQuad ).Decompose();
		Rotation = rot / MathF.PI * 180;
	}
}