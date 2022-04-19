using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class RectangleBlueprint : BasicTransformBlueprint<RectangleComponent> {
	PointHandle cornerRadiusTopLeft;
	PointHandle cornerRadiusTopRight;
	PointHandle cornerRadiusBottomLeft;
	PointHandle cornerRadiusBottomRight;
	int isDragging;

	public RectangleBlueprint ( RectangleComponent value, TransformProps props ) : base( value, props ) {
		void setupEvents ( Handle handle, Vector2 direction, Func<Vector2> getOrigin ) {
			handle.DragStarted += e => isDragging++;
			handle.DragEnded += e => { isDragging--; updateCorners(); };
			handle.Dragged += e => {
				var pos = ToTargetSpace( e.ScreenSpaceMousePosition );
				var r = Extensions.SignedDistance( getOrigin(), direction, pos ) / 2.5f * 2f;
				r = Math.Clamp( r, 0, Value.MaxCornerRadius );

				Value.CornerRadius.Value = e.AltPressed ? r : r.Round();
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
	}

	void updateCorners () {
		float min = isDragging == 0 ? 20 : 0;

		if ( min > Value.MaxCornerRadius || TransformProps.Width.Value < 0 || TransformProps.Height.Value < 0 ) {
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
		var a = ToLocalSpace( Composer.ContentToScreenSpace( Vector2.Zero ) );
		var b = ToLocalSpace( Composer.ContentToScreenSpace( new( r, 0 ) ) );
		r = ( a - b ).Length * 2.5f / 2f;

		cornerRadiusTopLeft.Position = new Vector2( 1, 1 ).Normalized() * r;
		cornerRadiusTopRight.Position = new Vector2( -1, 1 ).Normalized() * r;
		cornerRadiusBottomLeft.Position = new Vector2( 1, -1 ).Normalized() * r;
		cornerRadiusBottomRight.Position = new Vector2( -1, -1 ).Normalized() * r;
	}
}
