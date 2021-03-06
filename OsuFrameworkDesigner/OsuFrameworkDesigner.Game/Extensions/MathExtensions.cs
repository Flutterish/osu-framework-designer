using osu.Framework.Extensions.MatrixExtensions;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Extensions;

public static class MathExtensions {
	public static Vector2 Round ( this Vector2 self )
		=> new( MathF.Round( self.X ), MathF.Round( self.Y ) );

	public static Vector2 Round ( this Vector2 self, float precision )
		=> new( MathF.Round( self.X / precision ) * precision, MathF.Round( self.Y / precision ) * precision );

	public static Vector2 Clamp ( this Vector2 self, Vector2 min, Vector2 max )
		=> new( Math.Clamp( self.X, min.X, max.X ), Math.Clamp( self.Y, min.Y, max.Y ) );

	public static float Round ( this float self )
		=> MathF.Round( self );

	public static float Abs ( this float self )
		=> MathF.Abs( self );

	public static Vector2 Abs ( this Vector2 self )
		=> new Vector2( MathF.Abs( self.X ), MathF.Abs( self.Y ) );

	public static Vector2 ClosestPointToLine ( Vector2 startPoint, Vector2 direction, Vector2 point ) {
		Vector2 w = point - startPoint;

		float b = Vector2.Dot( w, direction ) / direction.LengthSquared;
		Vector2 pB = startPoint + b * direction;

		return pB;
	}

	public static float SignedDistance ( Vector2 startPoint, Vector2 direction, Vector2 point ) {
		direction.Normalize();
		Vector2 w = point - startPoint;

		return Vector2.Dot( w, direction );
	}

	public static void Deconstruct ( this Vector2 self, out float x, out float y ) {
		x = self.X;
		y = self.Y;
	}

	public static float ToDegrees ( this float radians )
		=> radians / MathF.PI * 180;

	public static float ToRadians ( this float degrees )
		=> degrees / 180 * MathF.PI;

	public static float Mod ( this float v, float mod ) {
		v = v % mod;
		if ( v < 0 )
			return mod + v;
		else
			return v;
	}

	public static int Mod ( this int v, int mod ) {
		v = v % mod;
		if ( v < 0 )
			return mod + v;
		else
			return v;
	}

	public static float WrappedDistanceTo ( this float from, float to, float unit )
		=> Mod( to - from + unit / 2, unit ) - unit / 2;
	public static float AngleTo ( this float from, float to )
		=> Mod( to - from + MathF.PI, MathF.Tau ) - MathF.PI;

	public static float ClosestEquivalentWrappedValue ( this float current, float target, float unit )
		=> current + current.WrappedDistanceTo( target, unit );
	public static float ClosestEquivalentAngle ( this float current, float target )
		=> current + current.AngleTo( target );

	public static Vector2 Rotate ( this Vector2 vector, float radians ) {
		var cos = MathF.Cos( radians );
		var sin = MathF.Sin( radians );

		return new Vector2( vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos );
	}

	public static (Vector2 translation, Vector2 scale, Vector2 shear, float rotation) Decompose ( this Matrix3 matrix ) {
		// M = T*R*Z*S
		// shear Y can be decomposed into scale, rotation and shear X. This simplifies this calculation, but means we can
		// parametrize the proportion of shear values in the future if we want
		//
		//     [ cosθSx              sinθSx             0 ] -> this row allows us to extract Sx and θ
		// M = [ -cosθSyZx - sinθSy  cosθSy - sinθSyZx  0 ]
		//     [ Tx                  Ty                 1 ] -> this row is raw translation

		var Tx = matrix.Row2.X;
		var Ty = matrix.Row2.Y;
		matrix.Row2.X = 0;
		matrix.Row2.Y = 0;

		var a = matrix.Row0.X;
		var b = matrix.Row0.Y;

		var theta = MathF.Atan2( b, a );

		MatrixExtensions.RotateFromRight( ref matrix, -theta );
		// ignoring translation and unrotating gives us this matrix:
		//
		//      [ Sx    0  0 ] -> simpler way to get Sx without a square root
		// M` = [ -SyZx Sy 0 ] -> this row gives is Sy and Zx
		//      [ 0     0  1 ]

		var Sx = matrix.Row0.X;
		var c = matrix.Row1.X;
		var d = matrix.Row1.Y;

		var Sy = d;
		var Zx = Sy == 0 ? 0 : ( -c / Sy );

		return (
			translation: new( Tx, Ty ),
			scale: new( Sx, Sy ),
			shear: new( Zx, 0 ),
			rotation: theta
		);
	}

	/// <summary>
	/// Decomposes a quad as if it was a matrix where
	/// (0,0) maps to top left,
	/// (1,0) maps to top right,
	/// (0,1) maps to bottom left and
	/// (1,1) maps to bottom right.
	/// The bottom left vertex is unused as it can possibly result in an invalid 3x3 matrix
	/// </summary>
	public static (Vector2 translation, Vector2 scale, Vector2 shear, float rotation) Decompose ( this Quad quad ) {
		//     [ cosθSx              sinθSx             0 ][ x ]   [ x(cosθSx) - y(cosθSyZx + sinθSy) + Tx ]
		// M = [ -cosθSyZx - sinθSy  cosθSy - sinθSyZx  0 ][ y ] = [ x(sinθSx) + y(cosθSy - sinθSyZx) + Ty ]
		//     [ Tx                  Ty                 1 ][ 1 ]   [ 1                                     ]

		var (Tx, Ty) = quad.TopLeft;

		// [ x` ]   [ x(cosθSx) - y(cosθSyZx + sinθSy) ]
		// [ y` ] = [ x(sinθSx) + y(cosθSy - sinθSyZx) ]

		var tr = quad.TopRight - quad.TopLeft;
		var br = quad.BottomRight - quad.TopLeft;

		var Sx = tr.Length;
		var theta = MathF.Atan2( tr.Y, tr.X );

		br = br.Rotate( -theta );

		var Sy = br.Y;
		var Zx = Sy == 0 ? 0 : ( ( Sx - br.X ) / Sy );

		return (
			translation: new( Tx, Ty ),
			scale: new( Sx, Sy ),
			shear: new( Zx, 0 ),
			rotation: theta
		);
	}

	/// <summary>
	/// Decomposes a quad into a matrix where
	/// (0,0) maps to top left,
	/// (1,0) maps to top right,
	/// (0,1) maps to bottom left and
	/// (1,1) maps to bottom right.
	/// The bottom left vertex is unused as it can possibly result in an invalid 3x3 matrix
	/// </summary>
	public static Matrix3 AsMatrix ( this Quad quad ) {
		var (translation, scale, shear, rotation) = quad.Decompose();

		return IHasMatrix.CreateMatrix( translation, scale, shear, rotation );
	}

	/// <summary>
	/// Rounds the top corner of a isosceles triangle. If the returned height would be less than 0, both it and the radius are capped.
	/// https://www.desmos.com/calculator/txty6davsf
	/// </summary>
	/// <returns>
	/// The height and a possibly capped radius of the circle which rounds the triangle such that its tangent to the sides of the triangle.
	/// Also, the angle in radians between the 2 radii of the circle connecting to the sides of the triangle.
	/// </returns>
	public static (float y, float radius, float angle) RoundTriangle ( float height, float sideSlope, float radius ) {
		var m1 = 1 / sideSlope;
		var m2 = -m1;
		var b1 = height;
		var d = m2 + 1 / m2;
		var r = radius;

		var m = 1 + m2 * m2;
		var n = -2 * b1 - 2 * m2 * m2 * b1;
		var o = b1 * b1 + m2 * m2 * b1 * b1 - d * d * r * r;
		var root = MathF.Sqrt( n * n - 4 * m * o );
		var b2 = MathF.Min( ( root - n ) / 2 / m, ( -root - n ) / 2 / m );

		var cornerAngle = 2 * ( MathF.PI / 2 - MathF.Atan2( m2, 1 ) );
		var cappedCornerRadius = radius;
		if ( b2 < 0 ) {
			b2 = 0;
			var x = b1 / d;
			var y = m2 * x;
			cappedCornerRadius = MathF.Sqrt( x * x + y * y );
		}

		return (b2, cappedCornerRadius, cornerAngle);
	}

	/// <summary>
	/// Given the computation from <see cref="RoundTriangle(float, float, float)"/>, computes the input radius given the output circle height
	/// </summary>
	public static float RoundedTriangleRadius ( float height, float sideSlope, float distance ) {
		var q = distance;
		if ( q > 0.5f ) {
			return 0;
		}

		var m1 = 1 / sideSlope;
		var m2 = -m1;
		var b1 = height;
		var d = m2 + 1 / m2;

		var x = ( b1 - q ) / d;
		var y = m2 * x;
		return MathF.Sqrt( x * x + y * y );
	}

	/// <summary>
	/// Whether the rectangle contains a point which also works with negatively sized rectangles
	/// </summary>
	/// <param name="rectangle"></param>
	/// <returns></returns>
	public static bool NormalizedContains ( this RectangleF rectangle, Vector2 point ) {
		if ( rectangle.Width < 0 ) {
			rectangle.X += rectangle.Width;
			rectangle.Width = -rectangle.Width;
		}
		if ( rectangle.Height < 0 ) {
			rectangle.Y += rectangle.Height;
			rectangle.Height = -rectangle.Height;
		}

		return rectangle.Contains( point );
	}

	public static Vector2 IntersectLines ( Vector2 pointOnA, Vector2 directionA, Vector2 pointOnB, Vector2 directionB ) {
		directionA.Normalize();
		directionB.Normalize();

		var d = pointOnB - pointOnA;
		var v1d = Vector2.Dot( directionA, d );
		var v2d = Vector2.Dot( directionB, d );
		var v1v2 = Vector2.Dot( directionA, directionB );

		var b = ( v2d - v1v2 * v1d ) / ( v1v2 * v1v2 - 1 );

		return pointOnB + b * directionB;
	}
}
