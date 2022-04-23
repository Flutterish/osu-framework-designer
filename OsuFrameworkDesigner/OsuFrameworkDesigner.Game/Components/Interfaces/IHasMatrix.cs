namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasMatrix : IHasPosition, IHasOrigin, IHasScale, IHasSize, IHasShear, IHasRotation {
	Matrix3 Matrix { get; set; }

	new public static IHasMatrix? From ( IComponent component ) {
		if ( component is IHasMatrix m )
			return m;

		if (
			IHasPosition.From( component ) is IHasPosition pos
			&& IHasShear.From( component ) is IHasShear shear
			&& IHasSize.From( component ) is IHasSize size
			&& IHasScale.From( component ) is IHasScale scale
			&& IHasOrigin.From( component ) is IHasOrigin origin
			&& IHasRotation.From( component ) is IHasRotation rot
		) {
			return new Impl {
				X = pos.X,
				Y = pos.Y,
				Width = size.Width,
				Height = size.Height,
				Rotation = rot.Rotation,
				ScaleX = scale.ScaleX,
				ScaleY = scale.ScaleY,
				ShearX = shear.ShearX,
				ShearY = shear.ShearY,
				OriginX = origin.OriginX,
				OriginY = origin.OriginY
			};
		}

		return null;
	}

	private struct Impl : IHasMatrix {
		public Prop<float> X { get; set; }
		public Prop<float> Y { get; set; }
		public Prop<float> Width { get; set; }
		public Prop<float> Height { get; set; }
		public Prop<float> Rotation { get; set; }
		public Prop<float> ScaleX { get; set; }
		public Prop<float> ScaleY { get; set; }
		public Prop<float> ShearX { get; set; }
		public Prop<float> ShearY { get; set; }
		public Prop<float> OriginX { get; set; }
		public Prop<float> OriginY { get; set; }

		public Matrix3 Matrix {
			get {
				var di = new DrawInfo();
				var originPosition = new Vector2( Width.Value, Height.Value ) * new Vector2( OriginX.Value, OriginY.Value );
				var pos = new Vector2( X.Value, Y.Value ) + originPosition;
				var drawScale = new Vector2( ScaleX.Value, ScaleY.Value );
				var shear = new Vector2( ShearX.Value, ShearY.Value );

				di.ApplyTransform( pos, drawScale, Rotation.Value, shear, originPosition );

				return di.Matrix;
			}
			set {

			}
		}
	}
}
