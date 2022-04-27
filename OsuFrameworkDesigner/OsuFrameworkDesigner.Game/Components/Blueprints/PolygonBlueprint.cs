using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class PolygonBlueprint : BasicTransformBlueprint<PolygonComponent> {
	PointHandle radiusHandle;

	public PolygonBlueprint ( PolygonComponent value ) : base( value, value.TransformProps ) {
		AddInternal( radiusHandle = new PointHandle().Center() );

		radiusHandle.Dragged += e => {
			var pos = ToTargetSpace( e.ScreenSpaceMousePosition ) - TransformProps.Size / 2;
			pos = pos * Math.Max( TransformProps.Width / TransformProps.Height, 1 );
			var r = -pos.Y;
			var v = Value.Polygon.CornerRadiusAtDistance( r );
			Value.CornerRadius.Value = Math.Clamp( e.AltPressed ? v : v.Round(), 0, Value.Polygon.MaxCornerRadius );
		};
	}

	protected override void Update () {
		base.Update();

		radiusHandle.TooltipText = $"Radius {Value.CornerRadius.Value:0}";
		radiusHandle.Position = TargetToLocalSpace( new( 0, -Value.Polygon.CornerCentreDistance / 2 / Math.Max( TransformProps.Width / TransformProps.Height, 1 ) ) );
	}
}
