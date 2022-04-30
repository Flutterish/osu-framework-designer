﻿using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class RectangleComponent : CompositeDrawable, IComponent {
	Box box;
	Container? roundedContainer;

	public readonly DrawableProps TransformProps;
	new public readonly Prop<float> CornerRadius = new( PropDescriptions.CornerRadius );

	public RectangleComponent () {
		TransformProps = new( this );
		AddInternal( box = new Box().Fill() );

		CornerRadius.BindValueChanged( v => updateLayout() );
		TransformProps.Width.BindValueChanged( v => updateLayout() );
		TransformProps.Height.BindValueChanged( v => updateLayout() );
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

	public override bool Contains ( Vector2 screenSpacePos )
		=> InternalChild.Contains( screenSpacePos );

	public Blueprint<IComponent> CreateBlueprint ()
		=> new RectangleBlueprint();
	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties => TransformProps.Append( CornerRadius );
}
