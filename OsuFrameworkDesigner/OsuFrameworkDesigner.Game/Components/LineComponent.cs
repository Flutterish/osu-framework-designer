using osu.Framework.Extensions.IEnumerableExtensions;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class LineComponent : CompositeDrawable, IComponent, IHasMatrix, IHasSnapGuides {
	public static readonly PropDescription RadiusProto = PropDescriptions.FloatProp with { Name = "Radius", Category = "Shape" };
	public static readonly PropDescription StartXProto = PropDescriptions.FloatProp with { Name = "X", Category = "Start" };
	public static readonly PropDescription StartYProto = PropDescriptions.FloatProp with { Name = "Y", Category = "Start" };
	public static readonly PropDescription EndXProto = PropDescriptions.FloatProp with { Name = "X", Category = "End" };
	public static readonly PropDescription EndYProto = PropDescriptions.FloatProp with { Name = "Y", Category = "End" };
	Box box;

	public readonly Prop<float> Radius = new( 1, RadiusProto );
	public readonly Prop<float> StartX = new( StartXProto );
	public readonly Prop<float> StartY = new( StartYProto );
	public readonly Prop<float> EndX = new( EndXProto );
	public readonly Prop<float> EndY = new( EndYProto );
	public readonly Prop<Colour4> FillColour = new( Colour4.Green, PropDescriptions.FillColour );

	public LineComponent () {
		Origin = Anchor.CentreLeft;
		AddInternal( box = new Box { EdgeSmoothness = new( 1 ) }.Fill() );

		Radius.BindValueChanged( v => Height = v.NewValue * 2, true );
		(StartX, StartY).BindValueChanged( ( x, y ) => {
			Position = new( x, y );
			var length = ( new Vector2( StartX, StartY ) - new Vector2( EndX, EndY ) ).Length;
			Width = length;

			updateRotation();
		} );

		(EndX, EndY).BindValueChanged( ( x, y ) => {
			var length = ( new Vector2( StartX, StartY ) - new Vector2( EndX, EndY ) ).Length;
			Width = length;

			updateRotation();
		} );

		FillColour.ValueChanged += v => Colour = v.NewValue;
		Name.BindValueChanged( x => base.Name = x.NewValue, true );
	}

	void updateRotation () {
		var diff = new Vector2( EndX, EndY ) - new Vector2( StartX, StartY );
		Rotation = MathF.Atan2( diff.Y, diff.X ) / MathF.PI * 180;
	}

	public Blueprint<IComponent> CreateBlueprint ()
		=> new LineBlueprint();
	new public IProp<string> Name { get; } = new Prop<string>( "Line", PropDescriptions.Name );
	public IEnumerable<IProp> Properties =>
		StartX.Yield<IProp>().Append( StartY )
		.Append( EndX ).Append( EndY )
		.Append( Radius ).Append( FillColour );

	public Matrix3 Matrix {
		get {
			var start = new Vector2( StartX.Value, StartY.Value );
			var end = new Vector2( EndX.Value, EndY.Value );
			var diff = end - start;
			var rot = MathF.Atan2( diff.Y, diff.X );

			return IHasMatrix.CreateMatrix( start, new Vector2( diff.Length, 0 ), Vector2.Zero, rot );
		}
		set {
			var (start, diff, _, rot) = value.Decompose();
			(StartX.Value, StartY.Value) = start;
			(EndX.Value, EndY.Value) = start + diff.Rotate( rot );
		}
	}

	public IEnumerable<PointGuide> PointGuides {
		get {
			yield return new Vector2( StartX, StartY );
			yield return new Vector2( EndX, EndY );
		}
	}
	public IEnumerable<LineGuide> LineGuides {
		get {
			yield return new() {
				StartPoint = new Vector2( StartX, StartY ),
				EndPoint = new Vector2( EndX, EndY )
			};
		}
	}
}
