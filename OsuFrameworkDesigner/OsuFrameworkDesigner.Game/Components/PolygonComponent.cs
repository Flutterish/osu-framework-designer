using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Components;

public class PolygonComponent : CompositeDrawable, IComponent {
	public readonly PolygonDrawable Polygon;

	public readonly TransformProps TransformProps;
	new public readonly Prop<float> CornerRadius = new( "Radius" ) { Category = "Corners" };
	public readonly Prop<float> CornerCount = new( 3, "Count" ) { Category = "Corners" };

	public PolygonComponent () {
		TransformProps = new( this );
		AddInternal( Polygon = new PolygonDrawable().Fill() );

		CornerRadius.BindValueChanged( v => Polygon.CornerRadius = v.NewValue );
		CornerCount.BindValueChanged( v => Polygon.SideCount = (int)v.NewValue );
	}

	public Blueprint<IComponent> CreateBlueprint ()
		=> new PolygonBlueprint( this );
	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties => TransformProps.Append( CornerRadius ).Append( CornerCount );
}
