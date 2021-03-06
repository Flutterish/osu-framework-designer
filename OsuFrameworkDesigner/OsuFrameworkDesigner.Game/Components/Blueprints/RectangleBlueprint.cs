using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class RectangleBlueprint : BasicTransformBlueprint<RectangleComponent> {
	PointHandle cornerRadiusTopLeft;
	PointHandle cornerRadiusTopRight;
	PointHandle cornerRadiusBottomLeft;
	PointHandle cornerRadiusBottomRight;
	int isDragging;

	public override TransformProps TransformProps => Value.TransformProps;
	public RectangleBlueprint () {
		void setupEvents ( Handle handle, Vector2 direction, Func<Vector2> getOrigin ) {
			handle.DragStarted += e => isDragging++;
			handle.DragEnded += e => { isDragging--; updateCorners(); };
			handle.Dragged += e => {
				var pos = ToTargetTopLeftSpace( e.ScreenSpaceMousePosition );
				var r = MathExtensions.SignedDistance( getOrigin(), direction * TransformProps.SizeFlipAxes, pos ) / MathF.Sqrt( 2 );
				r = Math.Clamp( e.AltPressed ? r : r.Round(), 0, Value.MaxCornerRadius );

				Value.CornerRadius.Value = r;
			};
		}

		AddInternal( cornerRadiusTopLeft = new PointHandle { Anchor = Anchor.TopLeft } );
		setupEvents( cornerRadiusTopLeft, new( 1, 1 ), () => Value.Position );
		AddInternal( cornerRadiusTopRight = new PointHandle { Anchor = Anchor.TopRight } );
		setupEvents( cornerRadiusTopRight, new( -1, 1 ), () => Value.Position + new Vector2( Value.Width, 0 ) );
		AddInternal( cornerRadiusBottomLeft = new PointHandle { Anchor = Anchor.BottomLeft } );
		setupEvents( cornerRadiusBottomLeft, new( 1, -1 ), () => Value.Position + new Vector2( 0, Value.Height ) );
		AddInternal( cornerRadiusBottomRight = new PointHandle { Anchor = Anchor.BottomRight } );
		setupEvents( cornerRadiusBottomRight, new( -1, -1 ), () => Value.Position + new Vector2( Value.Width, Value.Height ) );
	}

	protected override void Update () {
		base.Update();
		updateCorners();

		cornerRadiusTopLeft.TooltipText =
		cornerRadiusTopRight.TooltipText =
		cornerRadiusBottomLeft.TooltipText =
		cornerRadiusBottomRight.TooltipText =
			$"Radius {Value.CornerRadius.Value:0.##}";
	}

	void updateCorners () {
		float min = isDragging == 0 ? 20 : 0;

		if ( min > Value.MaxCornerRadius ) {
			cornerRadiusTopLeft.Hide();
			cornerRadiusTopRight.Hide();
			cornerRadiusBottomLeft.Hide();
			cornerRadiusBottomRight.Hide();

			return;
		}
		else {
			cornerRadiusTopLeft.Show();
			cornerRadiusTopRight.Show();
			cornerRadiusBottomLeft.Show();
			cornerRadiusBottomRight.Show();
		}

		var r = Math.Clamp( Value.CornerRadius.Value, min, Value.MaxCornerRadius );
		var a = Parent.ToLocalSpace( Composer.ContentToScreenSpace( Vector2.Zero ) );
		var b = Parent.ToLocalSpace( Composer.ContentToScreenSpace( new( r, 0 ) ) );
		r = ( a - b ).Length;

		cornerRadiusTopLeft.Position = new Vector2( 1, 1 ) * Value.Scale.Abs() * r;
		cornerRadiusTopRight.Position = new Vector2( -1, 1 ) * Value.Scale.Abs() * r;
		cornerRadiusBottomLeft.Position = new Vector2( 1, -1 ) * Value.Scale.Abs() * r;
		cornerRadiusBottomRight.Position = new Vector2( -1, -1 ) * Value.Scale.Abs() * r;
	}
}
