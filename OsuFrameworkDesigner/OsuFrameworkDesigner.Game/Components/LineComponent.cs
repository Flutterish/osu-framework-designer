using osu.Framework.Extensions.IEnumerableExtensions;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Components;

public class LineComponent : CompositeDrawable, IComponent, IHasMatrix {
	Box box;

	public readonly Prop<float> Radius = new( 1 ) { Category = "Shape" };
	public readonly Prop<float> StartX = new( "X" ) { Category = "Start" };
	public readonly Prop<float> StartY = new( "Y" ) { Category = "Start" };
	public readonly Prop<float> EndX = new( "X" ) { Category = "End" };
	public readonly Prop<float> EndY = new( "Y" ) { Category = "End" };

	public LineComponent () {
		Origin = Anchor.CentreLeft;
		AddInternal( box = new Box().Fill() );

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
	}

	void updateRotation () {
		var diff = new Vector2( EndX, EndY ) - new Vector2( StartX, StartY );
		Rotation = MathF.Atan2( diff.Y, diff.X ) / MathF.PI * 180;
	}

	public Blueprint<IComponent> CreateBlueprint ()
		=> new LineBlueprint( this );
	string IComponent.Name => Name;
	public IEnumerable<IProp> Properties =>
		StartX.Yield().Append( StartY )
		.Append( EndX ).Append( EndY )
		.Append( Radius );

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

	public void Offset ( Vector2 offset ) {
		StartX.Value += offset.X;
		StartY.Value += offset.Y;
		EndX.Value += offset.X;
		EndY.Value += offset.Y;
	}
}
