using osu.Framework.Extensions.IEnumerableExtensions;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class LineComponent : CompositeDrawable, IComponent, IHasMatrix, IHasSnapGuides {
	public static readonly PropDescription RadiusProto = PropDescriptions.FloatProp with { Name = "Radius", Category = "Shape" };
	public static readonly PropDescription StartProto = PropDescriptions.Vector2Prop with { Name = "Start", Category = "Line" };
	public static readonly PropDescription EndProto = PropDescriptions.Vector2Prop with { Name = "End", Category = "Line" };
	Box box;

	public readonly Prop<float> Radius = new( 1, RadiusProto );
	public readonly Prop<Vector2> Start = new( StartProto );
	public readonly Prop<Vector2> End = new( EndProto );
	public readonly Prop<Colour4> FillColour = new( Colour4.Green, PropDescriptions.FillColour );

	public LineComponent () {
		Origin = Anchor.CentreLeft;
		AddInternal( box = new Box { EdgeSmoothness = new( 1 ) }.Fill() );

		Radius.BindValueChanged( v => Height = v.NewValue * 2, true );
		Start.BindValueChanged( v => {
			Position = v.NewValue;
			var length = ( Start.Value - End ).Length;
			Width = length;

			updateRotation();
		} );

		End.BindValueChanged( v => {
			var length = ( Start.Value - End ).Length;
			Width = length;

			updateRotation();
		} );

		FillColour.ValueChanged += v => Colour = v.NewValue;
		Name.BindValueChanged( x => base.Name = x.NewValue, true );
	}

	void updateRotation () {
		var diff = End.Value - Start;
		Rotation = MathF.Atan2( diff.Y, diff.X ) / MathF.PI * 180;
	}

	public Blueprint<IComponent> CreateBlueprint ()
		=> new LineBlueprint();
	new public IProp<string> Name { get; } = new Prop<string>( "Line", PropDescriptions.Name );
	public IEnumerable<IProp> Properties =>
		Start.Yield<IProp>().Append( End )
		.Append( Radius ).Append( FillColour );

	public Matrix3 Matrix {
		get {
			var start = Start.Value;
			var end = End.Value;
			var diff = end - start;
			var rot = MathF.Atan2( diff.Y, diff.X );

			return IHasMatrix.CreateMatrix( start, new Vector2( diff.Length, 0 ), Vector2.Zero, rot );
		}
		set {
			var (start, diff, _, rot) = value.Decompose();
			Start.Value = start;
			End.Value = start + diff.Rotate( rot );
		}
	}

	public IEnumerable<PointGuide> PointGuides {
		get {
			yield return Start.Value;
			yield return End.Value;
		}
	}
	public IEnumerable<LineGuide> LineGuides {
		get {
			yield return new() {
				StartPoint = Start,
				EndPoint = End
			};
		}
	}
}
