using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class RectangleComponent : CompositeDrawable, IComponent {
	Sprite box;
	Container? roundedContainer;

	public readonly DrawableProps TransformProps;
	new public readonly Prop<float> CornerRadius = new( PropDescriptions.CornerRadius );

	public RectangleComponent () {
		TransformProps = new( this );
		AddInternal( box = new Sprite { Texture = Texture.WhitePixel }.Fill() );

		CornerRadius.BindValueChanged( v => updateLayout() );
		TransformProps.Width.BindValueChanged( v => updateLayout() );
		TransformProps.Height.BindValueChanged( v => updateLayout() );

		TransformProps.Texture.BindValueChanged( v => box.Texture = v.NewValue );
	}

	public float MaxCornerRadius => Math.Min( DrawSize.X.Abs(), DrawSize.Y.Abs() ) / 2f;

	void updateLayout () {
		if ( CornerRadius.Value > 0 ) {
			if ( roundedContainer is null ) {
				RemoveInternal( box );
				AddInternal( roundedContainer = new Container { Child = box, Masking = true }.Fill() );
			}

			roundedContainer.CornerRadius = Math.Min( CornerRadius.Value, MaxCornerRadius );
			roundedContainer.CornerExponent = 2;
		}
		else if ( roundedContainer != null ) {
			roundedContainer.Remove( box );
			RemoveInternal( roundedContainer );
			roundedContainer = null;
			AddInternal( box );
		}
	}
	public override bool Contains ( Vector2 screenSpacePos ) {
		float cRadius = CornerRadius * 0.8f * CornerExponent / 2 + 0.2f * CornerRadius;
		float cExponent = CornerExponent;

		var normalized = DrawRectangle.Normalize();
		// Select a cheaper contains method when we don't need rounded edges.
		if ( cRadius == 0.0f )
			return normalized.Contains( ToLocalSpace( screenSpacePos ) );

		var localSpacePos = ToLocalSpace( screenSpacePos );
		normalized = normalized.Shrink( cRadius );
		float distX = Math.Max( 0.0f, Math.Max( localSpacePos.X - normalized.Right, normalized.Left - localSpacePos.X ) );
		float distY = Math.Max( 0.0f, Math.Max( localSpacePos.Y - normalized.Bottom, normalized.Top - localSpacePos.Y ) );

		return MathF.Pow( distX, cExponent ) + MathF.Pow( distY, cExponent ) <= Math.Pow( cRadius, cExponent );
	}

	public Blueprint<IComponent> CreateBlueprint ()
		=> new RectangleBlueprint();
	string IComponent.Name { get => Name; set => Name = value; }
	public IEnumerable<IProp> Properties => TransformProps.Append( CornerRadius );
}
