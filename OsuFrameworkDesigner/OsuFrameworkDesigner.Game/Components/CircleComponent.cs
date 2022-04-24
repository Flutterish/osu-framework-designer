using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Components;

public class CircleComponent : CompositeDrawable, IComponent {
	TwoSidedCircularProgress circle;

	public readonly TransformProps TransformProps;
	public readonly Prop<float> Fill = new( 1 ) { Category = "Shape" };
	public readonly Prop<float> SweepEnd = new( 1, "End" ) { Category = "Shape" };
	public readonly Prop<float> SweepStart = new( 0, "Start" ) { Category = "Shape" };

	public CircleComponent () {
		Origin = Anchor.Centre;
		TransformProps = new( this );
		AddInternal( circle = new TwoSidedCircularProgress().Center().Fill() );
		circle.Current.Value = 1;

		(SweepStart, SweepEnd).BindValueChanged( ( s, e ) => {
			var delta = s.WrappedDistanceTo( e, 1 );
			if ( delta < 0 ) {
				if ( e < s ) {
					delta = -delta;
					s -= delta;
				}
				else {
					delta += 1;
				}
			}
			else if ( e < s ) {
				s = e;
				delta -= 1;
			}

			circle.Offset = s;
			circle.Current.Value = delta;
		} );

		Fill.ValueChanged += v => {
			circle.InnerRadius = v.NewValue;
		};

		Masking = true;
	}

	protected override void Update () {
		base.Update();

		CornerExponent = 2;
		CornerRadius = Math.Min( DrawSize.X.Abs(), DrawSize.Y.Abs() ) / 2f;
	}

	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties => TransformProps.Append( Fill ).Append( SweepStart ).Append( SweepEnd );
	public Blueprint<IComponent> CreateBlueprint ()
		=> new CircleBlueprint( this, TransformProps );
}
