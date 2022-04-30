using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Components;

public class CircleComponent : CompositeDrawable, IComponent, IHasSnapGuides {
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

	string IComponent.Name { get => Name; set => Name = value; }
	public IEnumerable<IProp> Properties => TransformProps.Append( Fill ).Append( SweepStart ).Append( SweepEnd );
	public Blueprint<IComponent> CreateBlueprint ()
		=> new CircleBlueprint();

	public IEnumerable<PointGuide> PointGuides {
		get {
			var quad = TransformProps.ContentSpaceQuad;
			var matrix = quad.AsMatrix();

			yield return quad.Centre;
			if ( circle.ContainsAngle( -MathF.PI / 2 ) )
				yield return ( quad.TopLeft + quad.TopRight ) / 2;
			if ( circle.ContainsAngle( -MathF.PI ) )
				yield return ( quad.TopLeft + quad.BottomLeft ) / 2;
			if ( circle.ContainsAngle( MathF.PI / 2 ) )
				yield return ( quad.BottomRight + quad.BottomLeft ) / 2;
			if ( circle.ContainsAngle( 0 ) )
				yield return ( quad.BottomRight + quad.TopRight ) / 2;

			yield return Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -( 1 - Fill ) / 2 ).Rotate( SweepStart * MathF.Tau ), matrix );
			yield return Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -0.5f ).Rotate( SweepStart * MathF.Tau ), matrix );

			yield return Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -( 1 - Fill ) / 2 ).Rotate( SweepEnd * MathF.Tau ), matrix );
			yield return Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -0.5f ).Rotate( SweepEnd * MathF.Tau ), matrix );
		}
	}
	public IEnumerable<LineGuide> LineGuides {
		get {
			var quad = TransformProps.ContentSpaceQuad;

			if ( ( (float)circle.Current.Value ).Mod( 1 ) != 0 ) {
				var matrix = quad.AsMatrix();

				var a = Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -( 1 - Fill ) / 2 ).Rotate( SweepStart * MathF.Tau ), matrix );
				var b = Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -0.5f ).Rotate( SweepStart * MathF.Tau ), matrix );
				yield return new() { StartPoint = a, EndPoint = b };

				a = Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -( 1 - Fill ) / 2 ).Rotate( SweepEnd * MathF.Tau ), matrix );
				b = Vector2Extensions.Transform( new Vector2( 0.5f ) + new Vector2( 0, -0.5f ).Rotate( SweepEnd * MathF.Tau ), matrix );
				yield return new() { StartPoint = a, EndPoint = b };
			}

			yield return new() { StartPoint = ( quad.TopLeft + quad.TopRight ) / 2, EndPoint = ( quad.BottomLeft + quad.BottomRight ) / 2 };
			yield return new() { StartPoint = ( quad.TopLeft + quad.BottomLeft ) / 2, EndPoint = ( quad.TopRight + quad.BottomRight ) / 2 };
		}
	}
}
