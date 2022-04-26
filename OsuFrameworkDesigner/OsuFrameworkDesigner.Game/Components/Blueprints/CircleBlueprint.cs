using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class CircleBlueprint : BasicTransformBlueprint<CircleComponent> {
	PointHandle fillHandle;
	PointHandle sweepEndHandle;
	PointHandle sweepStartHandle;
	float sweepHandleRadius;
	float fillHandleAngle;

	protected float ToScaledAngle ( float radians ) {
		var cos = MathF.Cos( radians ) / TransformProps.Width.Value;
		var sin = MathF.Sin( radians ) / TransformProps.Height.Value;
		return MathF.Atan2( sin, cos );
	}

	protected float RadiusAtAngle ( float radians ) {
		var cos = MathF.Cos( radians );
		var sin = MathF.Sin( radians );

		var b = TransformProps.Width.Value;
		var a = TransformProps.Height.Value;

		return ( a * b ) / ( MathF.Sqrt( a * a * sin * sin + b * b * cos * cos ) ) / 2;
	}

	public CircleBlueprint ( CircleComponent value ) : base( value, value.TransformProps ) {
		AddInternal( sweepStartHandle = new PointHandle() );
		AddInternal( sweepEndHandle = new PointHandle() );
		AddInternal( fillHandle = new PointHandle() );

		sweepStartHandle.Dragged += e => {
			var pos = ToTargetSpace( e.ScreenSpaceMousePosition ) - TransformProps.Size / 2;
			var (x, y) = pos;
			var angle = ( MathF.Atan2( y / TransformProps.Height.Value, x / TransformProps.Width.Value ) + MathF.PI / 2 ).Mod( MathF.Tau ) / MathF.Tau;

			Value.SweepStart.Value = Value.SweepStart.Value.ClosestEquivalentWrappedValue( angle, 1 );
			sweepHandleRadius = pos.Length;

			if ( ( Value.SweepEnd - Value.SweepStart ).Abs() > 1 ) {
				var delta = Value.SweepStart.Value.WrappedDistanceTo( Value.SweepEnd, 1 );
				(Value.SweepStart.Value, Value.SweepEnd.Value) = (Value.SweepEnd.Value, Value.SweepEnd.Value + ( delta < 0 ? 1 : -1 ));
			}
		};

		sweepEndHandle.Dragged += e => {
			var pos = ToTargetSpace( e.ScreenSpaceMousePosition ) - TransformProps.Size / 2;
			var (x, y) = pos;
			var angle = ( MathF.Atan2( y / TransformProps.Height.Value, x / TransformProps.Width.Value ) + MathF.PI / 2 ).Mod( MathF.Tau ) / MathF.Tau;

			Value.SweepEnd.Value = Value.SweepEnd.Value.ClosestEquivalentWrappedValue( angle, 1 );
			sweepHandleRadius = pos.Length;

			if ( ( Value.SweepEnd - Value.SweepStart ).Abs() > 1 ) {
				var delta = Value.SweepStart.Value.WrappedDistanceTo( Value.SweepEnd, 1 );
				(Value.SweepStart.Value, Value.SweepEnd.Value) = (Value.SweepStart.Value + ( delta > 0 ? 1 : -1 ), Value.SweepStart.Value);
			}
		};

		fillHandle.Dragged += e => {
			var pos = ToTargetSpace( e.ScreenSpaceMousePosition ) - TransformProps.Size / 2;
			var (x, y) = pos;
			var angle = ( MathF.Atan2( y / TransformProps.Height.Value, x / TransformProps.Width.Value ) + MathF.PI / 2 ).Mod( MathF.Tau );
			fillHandleAngle = angle;
			angle = ToScaledAngle( angle );

			Value.Fill.Value = Math.Clamp( 1 - pos.Length / RadiusAtAngle( angle ), 0, 1 );
		};
	}

	protected override void Update () {
		base.Update();

		var fill = Math.Clamp( Value.Fill.Value, 0, 1 );

		float clamp ( float v, float min, float max )
			=> MathF.Min( MathF.Max( v, min ), max );

		var angle = ToScaledAngle( Value.SweepEnd.Value * MathF.Tau );
		var maxSweepRadius = RadiusAtAngle( angle );
		var minSweepRadius = maxSweepRadius * ( 1 - fill );
		var sweepRadius = sweepEndHandle.IsDragged ? sweepHandleRadius : ( ( minSweepRadius + maxSweepRadius ) / 2 );
		var offset = new Vector2( 0, -clamp( sweepRadius, minSweepRadius, maxSweepRadius ) ).Rotate( angle );
		sweepEndHandle.Position = TargetToLocalSpace( offset + TransformProps.Size / 2 );

		angle = ToScaledAngle( Value.SweepStart.Value * MathF.Tau );
		maxSweepRadius = RadiusAtAngle( angle );
		minSweepRadius = maxSweepRadius * ( 1 - fill );
		sweepRadius = sweepStartHandle.IsDragged ? sweepHandleRadius : ( ( minSweepRadius + maxSweepRadius ) / 2 );
		offset = new Vector2( 0, -clamp( sweepRadius, minSweepRadius, maxSweepRadius ) ).Rotate( angle );
		sweepStartHandle.Position = TargetToLocalSpace( offset + TransformProps.Size / 2 );

		float handleAngle;
		if ( Value.SweepStart.Value == Value.SweepEnd.Value ) {
			handleAngle = Value.SweepStart.Value * MathF.Tau;
		}
		else {
			var delta = Value.SweepStart.Value.WrappedDistanceTo( Value.SweepEnd, 1 ) * MathF.Tau;
			if ( delta == 0 )
				handleAngle = ( Value.SweepStart.Value + 0.5f ) * MathF.Tau;
			else if ( ( delta < 0 ) == ( Value.SweepStart.Value > Value.SweepEnd.Value ) )
				handleAngle = Value.SweepStart.Value * MathF.Tau + delta / 2;
			else
				handleAngle = ( Value.SweepStart.Value + 0.5f ) * MathF.Tau + delta / 2;
		}

		if ( fillHandle.IsDragged ) {
			var maxDistance = handleAngle.AngleTo( Value.SweepStart.Value * MathF.Tau ).Abs();
			var delta = handleAngle.AngleTo( fillHandleAngle );

			if ( delta.Abs() > maxDistance ) {
				handleAngle += MathF.CopySign( maxDistance, delta );
			}
			else {
				handleAngle = fillHandleAngle;
			}
		}

		angle = ToScaledAngle( handleAngle );
		maxSweepRadius = RadiusAtAngle( angle );
		sweepRadius = maxSweepRadius * ( 1 - fill );
		offset = new Vector2( 0, -sweepRadius ).Rotate( angle );
		fillHandle.Position = TargetToLocalSpace( offset + TransformProps.Size / 2 );

		fillHandle.TooltipText = $"Fill {Value.Fill.Value:0.##%}";
		sweepStartHandle.TooltipText = $"Start {Value.SweepStart.Value * 360:0}°";
		sweepEndHandle.TooltipText = $"Sweep {( Value.SweepEnd - Value.SweepStart ) * 360:0}°";
	}
}
