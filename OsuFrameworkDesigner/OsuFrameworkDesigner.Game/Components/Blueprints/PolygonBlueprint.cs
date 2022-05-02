using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

// TODO translating this doesnt snap to corners
public class PolygonBlueprint : BasicTransformBlueprint<PolygonComponent> {
	PointHandle radiusHandle;
	PointHandle countHandle;

	Vector2 scale {
		get {
			var ratio = TransformProps.Width.Value.Abs() / TransformProps.Height.Value.Abs();
			if ( ratio > 1 )
				return new Vector2( 1, 1 / ratio ) * TransformProps.SizeFlipAxes;
			else
				return new Vector2( ratio, 1 ) * TransformProps.SizeFlipAxes;
		}
	}

	public override TransformProps TransformProps => Value.TransformProps;
	public PolygonBlueprint () {
		AddInternal( radiusHandle = new PointHandle().Center() );
		AddInternal( countHandle = new PointHandle().Center() );

		radiusHandle.Dragged += e => {
			var pos = ToTargetSpace( e.ScreenSpaceMousePosition ) - TransformProps.Size / 2;
			pos = pos * Math.Max( TransformProps.Width.Value.Abs() / TransformProps.Height.Value.Abs(), 1 );
			var r = ( TransformProps.Height < 0 ) ? pos.Y : -pos.Y;
			var v = Value.Polygon.CornerRadiusAtDistance( r );
			Value.CornerRadius.Value = Math.Clamp( e.AltPressed ? v : v.Round(), 0, Value.Polygon.MaxCornerRadius );
		};

		countHandle.Dragged += e => {
			var pos = ToTargetSpace( e.ScreenSpaceMousePosition ) - TransformProps.Size / 2;
			pos = Vector2.Divide( pos, scale );
			var angle = MathF.Atan2( pos.Y, pos.X ) + MathF.PI / 2;

			Value.CornerCount.Value = (int)Math.Clamp( MathF.Tau / angle, 3, 60 );
		};
	}

	protected override void Update () {
		base.Update();

		radiusHandle.TooltipText = $"Radius {Value.CornerRadius.Value:0}";
		countHandle.TooltipText = $"Count {Value.CornerCount.Value:0}";

		radiusHandle.Position = TargetToLocalSpace( new Vector2( 0, -Value.Polygon.CornerCentreDistance / 2 ) * scale );
		countHandle.Position = TargetToLocalSpace( new Vector2( 0, -Value.Polygon.CornerCentreDistance / 2 - Value.CornerRadius ).Rotate( MathF.Tau / Value.CornerCount ) * scale );
	}
}
