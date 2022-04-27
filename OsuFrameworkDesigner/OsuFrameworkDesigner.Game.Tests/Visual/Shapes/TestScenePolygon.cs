using osu.Framework.Testing;
using OsuFrameworkDesigner.Game.Extensions;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Tests.Visual.Shapes;

public class TestScenePolygon : TestScene {
	PolygonDrawable polygon;
	public TestScenePolygon () {
		Add( polygon = new PolygonDrawable {
			Size = new( 400 )
		}.Center() );

		AddSliderStep<int>( "Side Count", 3, 10, 3, s => polygon.SideCount = s );
		AddSliderStep<float>( "Coner Radius", 0, 400, 0, s => polygon.CornerRadius = s );
	}
}
