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
			Value.PointA.Value = e.Position;
		};
		handleB.SnapDragged += e => {
			Value.PointB.Value = e.Position;
		};
		handleC.SnapDragged += e => {
			Value.PointC.Value = e.Position;
		};

		var offsetBA = Vector2.Zero;
		var offsetCA = Vector2.Zero;
		moveHandle.HandleSnappedTranslate( ( lines, points ) => {
			var a = Value.PointA.Value;
			var b = Value.PointB.Value;
			var c = Value.PointC.Value;
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
			Value.PointA.Value = position;
			Value.PointB.Value = position + offsetBA;
			Value.PointC.Value = position + offsetCA;
		} );
	}

	protected override void Update () {
		base.Update();

		Position = Parent.ToLocalSpace( Value.Parent.ToScreenSpace( Value.Position ) );
		handleA.Position = Value.ToSpaceOfOtherDrawable( Value.PointA - Value.Position, this );
		handleB.Position = Value.ToSpaceOfOtherDrawable( Value.PointB - Value.Position, this );
		handleC.Position = Value.ToSpaceOfOtherDrawable( Value.PointC - Value.Position, this );

		handleA.TooltipText = $"{Value.PointA.Value.X:0}, {Value.PointA.Value.Y:0}";
		handleB.TooltipText = $"{Value.PointB.Value.X:0}, {Value.PointB.Value.Y:0}";
		handleC.TooltipText = $"{Value.PointC.Value.X:0}, {Value.PointC.Value.Y:0}";
	}
}
