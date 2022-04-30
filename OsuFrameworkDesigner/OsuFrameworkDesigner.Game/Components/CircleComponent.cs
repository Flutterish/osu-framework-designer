using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Components;

public class CircleComponent : CompositeDrawable, IComponent {
	public static readonly PropDescription FillProto = PropDescriptions.FloatProp with { Name = "Fill", Category = "Shape" };
	public static readonly PropDescription SweepEndProto = PropDescriptions.FloatProp with { Name = "End", Category = "Shape" };
	public static readonly PropDescription SweepStartProto = PropDescriptions.FloatProp with { Name = "Start", Category = "Shape" };

	TwoSidedCircularProgress circle;

	public readonly DrawableProps TransformProps;
	public readonly Prop<float> Fill = new( 1, FillProto );
	public readonly Prop<float> SweepEnd = new( 1, SweepEndProto );
	public readonly Prop<float> SweepStart = new( 0, SweepStartProto );

	public CircleComponent () {
		Origin = Anchor.Centre;
		TransformProps = new( this );
		AddInternal( circle = new TwoSidedCircularProgress().Center().Fill() );
		circle.Current.Value = 1;

		(SweepStart, SweepEnd).BindValueChanged( ( s, e ) => {
			var delta = s.WrappedDistanceTo( e, 1 );

			if ( delta == 0 && s != e ) {
				delta = 2;
			}

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

	public override bool Contains ( Vector2 screenSpacePos )
		=> InternalChild.Contains( screenSpacePos );

	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties => TransformProps.Append( Fill ).Append( SweepStart ).Append( SweepEnd );
	public Blueprint<IComponent> CreateBlueprint ()
		=> new CircleBlueprint();
}
