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
		var topLeft = Parent.ToLocalSpace( drawable.ScreenSpaceDrawQuad.TopLeft );
		Position = topLeft;
		var a = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Vector2.Zero ) );
		var b = Parent.ToLocalSpace( Composer.ContentToScreenSpace( new( 1, 0 ) ) );
		var scale = ( a - b ).Length;
		Size = drawable.DrawSize * drawable.Scale * scale;
		Shear = drawable.Shear;
		Rotation = drawable.Rotation;
	}
}