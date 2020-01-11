using System;
using System.Runtime.InteropServices;

namespace Maths
{
    public interface IMatrix
    {
        float[,] ToGrid();
    }

    /// <summary>
    /// Column-major 4x4 matrix, like in Swift / metal
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Float4x4 : IMatrix
    {
        public static Float4x4 Identity => new Float4x4(
            1f, 0, 0, 0,
            0, 1f, 0, 0,
            0, 0, 1f, 0,
            0, 0, 0, 1f);
        
        public Float4 Col0;
        public Float4 Col1;
        public Float4 Col2;
        public Float4 Col3;
        
        public Float4 Row0 => new Float4(Col0.X, Col1.X, Col2.X, Col3.X);
        public Float4 Row1 => new Float4(Col0.Y, Col1.Y, Col2.Y, Col3.Y);
        public Float4 Row2 => new Float4(Col0.Z, Col1.Z, Col2.Z, Col3.Z);
        public Float4 Row3 => new Float4(Col0.W, Col1.W, Col2.W, Col3.W);

        public float M11 { get => Col0.X; set => Col0.X = value; }
        public float M12 { get => Col1.X; set => Col1.X = value; }
        public float M13 { get => Col2.X; set => Col2.X = value; }
        public float M14 { get => Col3.X; set => Col3.X = value; }
        public float M21 { get => Col0.Y; set => Col0.Y = value; }
        public float M22 { get => Col1.Y; set => Col1.Y = value; }
        public float M23 { get => Col2.Y; set => Col2.Y = value; }
        public float M24 { get => Col3.Y; set => Col3.Y = value; }
        public float M31 { get => Col0.Z; set => Col0.Z = value; }
        public float M32 { get => Col1.Z; set => Col1.Z = value; }
        public float M33 { get => Col2.Z; set => Col2.Z = value; }
        public float M34 { get => Col3.Z; set => Col3.Z = value; }
        public float M41 { get => Col0.W; set => Col0.W = value; }
        public float M42 { get => Col1.W; set => Col1.W = value; }
        public float M43 { get => Col2.W; set => Col2.W = value; }
        public float M44 { get => Col3.W; set => Col3.W = value; }
        
        
        public Float4x4(Float4 col0, Float4 col1, Float4 col2, Float4 col3)
        {
            Col0 = col0;
            Col1 = col1;
            Col2 = col2;
            Col3 = col3;
        }
        
        public Float4x4(
            float m00,
            float m01,
            float m02,
            float m03,
            float m10,
            float m11,
            float m12,
            float m13,
            float m20,
            float m21,
            float m22,
            float m23,
            float m30,
            float m31,
            float m32,
            float m33)
        {
            Col0 = new Float4(m00, m10, m20, m30);
            Col1 = new Float4(m01, m11, m21, m31);
            Col2 = new Float4(m02, m12, m22, m32);
            Col3 = new Float4(m03, m13, m23, m33);
        }

        public Float4x4 Transposed()
        {
            return new Float4x4(Row0, Row1, Row2, Row3);
        }

        public Float4x4 Inverted() => Invert(this);

        public static Float4x4 Mult(Float4x4 left, Float4x4 right)
        {
            return new Float4x4(
                left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
                left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
                left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
                left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
                left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
                left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
                left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
                left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
                left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
                left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
                left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
                left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
                left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
                left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
                left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
                left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
            
            // return new Float4x4(
            //     left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
            //     left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
            //     left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
            //     left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
            //     
            //     left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
            //     left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
            //     left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
            //     left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
            //     
            //     left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
            //     left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
            //     left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
            //     left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
            //     
            //     left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
            //     left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
            //     left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
            //     left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
        }

        public float[,] ToGrid()
        {
            return new [,]
            {
                { Col0.X, Col1.X, Col2.X, Col3.X }, 
                { Col0.Y, Col1.Y, Col2.Y, Col3.Y }, 
                { Col0.Z, Col1.Z, Col2.Z, Col3.Z }, 
                { Col0.W, Col1.W, Col2.W, Col3.W }
            };
        }

        public static Float4x4 CreateTranslation(Float3 vector) => CreateTranslation(vector.X, vector.Y, vector.Z); 
        
        public static Float4x4 CreateTranslation(float x, float y, float z)
        {
            return new Float4x4(
                1f, 0, 0, x,
                0, 1f, 0, y,
                0, 0, 1f, z,
                0, 0, 0, 1f
                );
        }
        
        public static Float4x4 CreateRotation(float radians, float x, float y, float z)
        {
            var v = new Float3(x, y, z);
            v.Normalize();
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            var cosp = 1f - cos;

            return new Float4x4(
                cos + cosp * v.X * v.X, cosp * v.X * v.Y - v.Z * sin, cosp * v.X * v.Z + v.Y * sin, 0f,
                cosp * v.X * v.Y + v.Z * sin, cos + cosp * v.Y * v.Y, cosp * v.Y * v.Z - v.X * sin, 0f,
                cosp * v.X * v.Z - v.Y * sin, cosp * v.Y * v.Z + v.X * sin, cos + cosp * v.Z * v.Z, 0f,
                0f, 0f, 0f, 1f);
        }
        
        public static Float4x4 CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            if (fovy <= 0 || fovy > Math.PI)
                throw new ArgumentOutOfRangeException("fovy");
            if (aspect <= 0)
                throw new ArgumentOutOfRangeException("aspect");
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            
            var yMax = zNear * (float)Math.Tan(0.5f * fovy);
            var yMin = -yMax;
            var xMin = yMin * aspect;
            var xMax = yMax * aspect;

            return CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }
        
        public static Float4x4 CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");
            
            var x = (2.0f * zNear) / (right - left);
            var y = (2.0f * zNear) / (top - bottom);
            var a = (right + left) / (right - left);
            var b = (top + bottom) / (top - bottom);
            var c = -(zFar + zNear) / (zFar - zNear);
            var d = -(2.0f * zFar * zNear) / (zFar - zNear);
            
            return new Float4x4(
                x, 0, 0, a,
                0, y, 0, b,
                0, 0, c, d,
                0, 0, -1, 0);
        }
        
        public Float4x4 Invert(Float4x4 mat)
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            // convert the matrix to an array for easy looping
            var inverse = mat.ToGrid();
            var icol = 0;
            var irow = 0;
            for (var ii = 0; ii < 4; ii++)
            {
                // Find the largest pivot value
                var maxPivot = 0.0f;
                for (var jj = 0; jj < 4; jj++)
                {
                    if (pivotIdx[jj] != 0)
                    {
                        for (var kk = 0; kk < 4; ++kk)
                        {
                            if (pivotIdx[kk] == -1)
                            {
                                var absVal = Math.Abs(inverse[jj, kk]);
                                if (!(absVal > maxPivot)) continue;
                                maxPivot = absVal;
                                irow = jj;
                                icol = kk;
                            }
                            else if (pivotIdx[kk] > 0)
                            {
                                return mat;
                            }
                        }
                    }
                }

                ++pivotIdx[icol];

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (var kk = 0; kk < 4; ++kk)
                    {
                        var f = inverse[irow, kk];
                        inverse[irow, kk] = inverse[icol, kk];
                        inverse[icol, kk] = f;
                    }
                }

                rowIdx[ii] = irow;
                colIdx[ii] = icol;

                var pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                    //return mat;
                }

                // Scale row so it has a unit diagonal
                var oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (var k = 0; k < 4; ++k)
                {
                    inverse[icol, k] *= oneOverPivot;
                }
                    

                // Do elimination of non-diagonal elements
                for (var jj = 0; jj < 4; ++jj)
                {
                    // check this isn't on the diagonal
                    if (icol != jj)
                    {
                        var f = inverse[jj, icol];
                        inverse[jj, icol] = 0.0f;
                        for (var kk = 0; kk < 4; ++kk)
                        {
                            inverse[jj, kk] -= inverse[icol, kk] * f;
                        }
                    }
                }
            }

            for (var jj = 3; jj >= 0; --jj)
            {
                var ir = rowIdx[jj];
                var ic = colIdx[jj];
                for (var k = 0; k < 4; ++k)
                {
                    var f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }
            
            mat.Col0 = new Float4(inverse[0, 0], inverse[0, 1], inverse[0, 2], inverse[0, 3]);
            mat.Col1 = new Float4(inverse[1, 0], inverse[1, 1], inverse[1, 2], inverse[1, 3]);
            mat.Col2 = new Float4(inverse[2, 0], inverse[2, 1], inverse[2, 2], inverse[2, 3]);
            mat.Col3 = new Float4(inverse[3, 0], inverse[3, 1], inverse[3, 2], inverse[3, 3]);
            return mat;
        }
        
        public static Float4x4 operator *(Float4x4 left, Float4x4 right)
        {
            return Mult(left, right);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Float4x4 left, Float4x4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Float4x4 left, Float4x4 right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is Float4x4 other && Equals(other);
        }
        
        public bool Equals(Float4x4 other)
        {
            return
                Col0 == other.Col0 &&
                Col1 == other.Col1 &&
                Col2 == other.Col2 &&
                Col3 == other.Col3;
        }
        
        public override int GetHashCode()
        {
            return Col0.GetHashCode() ^ Col1.GetHashCode() ^ Col2.GetHashCode() ^ Col3.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"{Col0.X},{Col1.X},{Col2.X},{Col3.X}\n{Col0.Y},{Col1.Y},{Col2.Y},{Col3.Y}\n{Col0.Z},{Col1.Z},{Col2.Z},{Col3.Z}\n{Col0.W},{Col1.W},{Col2.W},{Col3.W}";
        }
    }
}