using System;
using System.Runtime.InteropServices;

namespace Maths
{
    
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Float4x4
    {
        public static Float4x4 Identity => new Float4x4(Float4.UnitX, Float4.UnitY, Float4.UnitZ, Float4.UnitW);
        
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

        public static Float4x4 Mult(ref Float4x4 left, ref Float4x4 right)
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
        }

        private float[,] ToGrid()
        {
            return new [,]
            {
                { Col0.X, Col1.X, Col2.X, Col3.X }, 
                { Col0.Y, Col1.Y, Col2.Y, Col3.Y }, 
                { Col0.Z, Col1.Z, Col2.Z, Col3.Z }, 
                { Col0.W, Col1.W, Col2.W, Col3.W }
            };
        }
        
        
        public Float4x4 Invert(Float4x4 mat)
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            // convert the matrix to an array for easy looping
            float[,] inverse = mat.ToGrid();
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                float maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                float absVal = Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                return mat;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        float f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                    //return mat;
                }

                // Scale row so it has a unit diagonal
                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    float f = inverse[k, ir];
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
            return Mult(ref left, ref right);
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
    }
}