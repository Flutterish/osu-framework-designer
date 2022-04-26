using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public abstract class Blueprint<T> : CompositeDrawable {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	public T Value { get; private set; }

	protected Blueprint ( T value ) {
		Value = value;
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