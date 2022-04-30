using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Components;

public class PolygonComponent : CompositeDrawable, IComponent {
	public static readonly PropDescription CornerCountProto = PropDescriptions.IntProp with { Name = "Count", Category = "Corners" };
	public readonly PolygonDrawable Polygon;

	public readonly DrawableProps TransformProps;
	new public readonly Prop<float> CornerRadius = new( PropDescriptions.CornerRadius );
	public readonly Prop<int> CornerCount = new( 3, CornerCountProto );

	public PolygonComponent () {
		TransformProps = new( this );
		AddInternal( Polygon = new PolygonDrawable().Fill() );

		CornerRadius.BindValueChanged( v => Polygon.CornerRadius = v.NewValue );
		CornerCount.BindValueChanged( v => Polygon.SideCount = v.NewValue );
	}

	public override bool Contains ( Vector2 screenSpacePos )
		=> InternalChild.Contains( screenSpacePos );

	public Blueprint<IComponent> CreateBlueprint ()
		=> new PolygonBlueprint();
	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties => TransformProps.Append( CornerRadius ).Append( CornerCount );
}
