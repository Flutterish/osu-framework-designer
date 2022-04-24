using osu.Framework.Extensions.MatrixExtensions;

namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasMatrix {
	Matrix3 Matrix { get; set; }

	public static IHasMatrix? From ( IComponent component ) {
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
				var matrix = Matrix3.Identity;

				var pos = new Vector2( X.Value, Y.Value );
				MatrixExtensions.TranslateFromLeft( ref matrix, pos );

				MatrixExtensions.RotateFromLeft( ref matrix, Rotation.Value / 180 * MathF.PI );

				MatrixExtensions.ShearFromLeft( ref matrix, new( -ShearX.Value, -ShearY.Value ) );

				MatrixExtensions.ScaleFromLeft( ref matrix, new( ScaleX.Value, ScaleY.Value ) );

				return matrix;
			}
			set {
				// M = T*R*Z*S*O
				var m = value;

				// extract translate
				this.X.Value = m.Row2.X;
				this.Y.Value = m.Row2.Y;

				m.Row2.X = 0;
				m.Row2.Y = 0;

				// extract rotation
				var cos = m.Column0.X;
				var sin = m.Column1.X;
				var rot = MathF.Atan2( sin, cos );
				MatrixExtensions.RotateFromRight( ref m, -rot );
				Rotation.Value = rot * 180 / MathF.PI;

				// TODO extract shear
				// extract shear and scale
				// [ X + Xxy -Xx ] = [ e f ]
				// [ -Yy     Y   ]   [ g h ]
				//
				// Y = h
				// -Yy = g -> y = -g/Y
				//
				// -Xx = f -> x = -f/X
				// X + -X(f/X)y = e
				// X = e + fy

				var e = m.Row0.X;
				var f = m.Row1.X;
				var g = m.Row0.Y;
				var h = m.Row1.Y;

				var Y = h;
				var y = -( g / Y );
				var X = e + f * y;
				//var x = -(f / X);

				//ShearX.Value = x;
				//ShearY.Value = y;
				ScaleX.Value = X;
				ScaleY.Value = Y;
				//this.X.Value += m.Row2.X;
				//this.Y.Value += m.Row2.Y;
			}
		}
	}
}
