using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Tools;

namespace OsuFrameworkDesigner.Game.Components.Blueprints;

public class TriangleBlueprint : Blueprint<IComponent> {
	new public TriangleComponent Value => (TriangleComponent)base.Value;

	Handle moveHandle;
	PointHandle handleA;
	PointHandle handleB;
	PointHandle handleC;

	public TriangleBlueprint () {
		AddInternal( moveHandle = new Handle().Fill() );
		AddInternal( handleA = new() );
		AddInternal( handleB = new() );
		AddInternal( handleC = new() );

		handleA.SnapDragged += e => {
			(Value.X1.Value, Value.Y1.Value) = e.Position;
		};
		handleB.SnapDragged += e => {
			(Value.X2.Value, Value.Y2.Value) = e.Position;
		};
		handleC.SnapDragged += e => {
			(Value.X3.Value, Value.Y3.Value) = e.Position;
		};

		var offsetBA = Vector2.Zero;
		var offsetCA = Vector2.Zero;

		moveHandle.HandleSnappedTranslate( ( lines, points ) => {
			var a = new Vector2( Value.X1, Value.Y1 );
			var b = new Vector2( Value.X2, Value.Y2 );
			var c = new Vector2( Value.X3, Value.Y3 );
			offsetBA = b - a;
			offsetCA = c - a;

			points.Add( a );
			points.Add( b );
			points.Add( c );

			lines.Add( new() { StartPoint = a, EndPoint = b } );
			lines.Add( new() { StartPoint = b, EndPoint = c } );
			lines.Add( new() { StartPoint = c, EndPoint = a } );

			return a;
		}, position => {
			(Value.X1.Value, Value.Y1.Value) = position;
			(Value.X2.Value, Value.Y2.Value) = position + offsetBA;
			(Value.X3.Value, Value.Y3.Value) = position + offsetCA;
		} );
	}

	protected override void Update () {
		base.Update();

		Position = Parent.ToLocalSpace( Value.Parent.ToScreenSpace( Value.Position ) );
		handleA.Position = Value.ToSpaceOfOtherDrawable( new Vector2( Value.X1, Value.Y1 ) - Value.Position, this );
		handleB.Position = Value.ToSpaceOfOtherDrawable( new Vector2( Value.X2, Value.Y2 ) - Value.Position, this );
		handleC.Position = Value.ToSpaceOfOtherDrawable( new Vector2( Value.X3, Value.Y3 ) - Value.Position, this );

		handleA.TooltipText = $"{Value.X1.Value:0}, {Value.Y1.Value:0}";
		handleB.TooltipText = $"{Value.X2.Value:0}, {Value.Y2.Value:0}";
		handleC.TooltipText = $"{Value.X3.Value:0}, {Value.Y3.Value:0}";
	}
}
