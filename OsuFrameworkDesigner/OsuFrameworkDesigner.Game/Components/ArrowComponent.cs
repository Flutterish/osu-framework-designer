namespace OsuFrameworkDesigner.Game.Components;

public class ArrowComponent : LineComponent {
	Box arrowHeadLeft;
	Box arrowHeadRight;

	public ArrowComponent () {
		AddInternal( arrowHeadLeft = new() {
			Origin = Anchor.TopRight,
			Anchor = Anchor.CentreRight,
			Rotation = 45
		} );
		AddInternal( arrowHeadRight = new() {
			Origin = Anchor.BottomRight,
			Anchor = Anchor.CentreRight,
			Rotation = -45
		} );

		Radius.BindValueChanged( v => {
			arrowHeadRight.Height =
			arrowHeadLeft.Height = v.NewValue * 2;

			arrowHeadRight.X =
			arrowHeadLeft.X = v.NewValue;

			arrowHeadRight.Width =
			arrowHeadLeft.Width = v.NewValue * 10;
		}, true );
	}
}
