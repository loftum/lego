using System;
using OpenTK;

namespace Visualizer.Rendering
{
    public static class MatrixExtensions
    {
        public static Matrix3 GetNormalMatrix(this Matrix4 self)
        {
            var upperLeft = self.GetUpperLeft();
            return upperLeft.Transposed().Inverted();
        }

        private static Matrix3 GetUpperLeft(this Matrix4 self)
        {
            return new Matrix3(self.Row0.X, self.Row0.Y, self.Row0.Z,
                self.Row1.X, self.Row1.Y, self.Row1.Z,
                self.Row2.X, self.Row2.Y, self.Row2.Z);
        }

        public static Matrix3 Transposed(this Matrix3 self)
        {
            Matrix3.Transpose(ref self, out var result);
            return result;
        }

        public static Matrix3 Inverted(this Matrix3 self)
        {
            Invert(ref self, out var result);
            return result;
        }
        
        public static void Invert(ref Matrix3 mat, out Matrix3 result)
        {
            int[] colIdx = { 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1 };
            
            float[,] inverse =
            {
                { mat.R0C0, mat.R0C1, mat.R0C2 },
                { mat.R1C0, mat.R1C1, mat.R1C2 },
                { mat.R2C0, mat.R2C1, mat.R2C2 }
            };
            
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 3; i++)
            {
                float maxPivot = 0.0f;
                for (int j = 0; j < 3; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 3; ++k)
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
                                result = mat;
                                return;
                            }
                        }
                    }
                }
                
                ++(pivotIdx[icol]);
                
                if (irow != icol)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        float f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }
                
                rowIdx[i] = irow;
                colIdx[i] = icol;
                
                float pivot = inverse[icol, icol];
                
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                }
                
                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 3; ++k)
                    inverse[icol, k] *= oneOverPivot;
                
                for (int j = 0; j < 3; ++j)
                {
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 3; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }
            
            for (int j = 2; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 3; ++k)
                {
                    float f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }
            
            result.R0C0 = inverse[0, 0];
            result.R0C1 = inverse[0, 1];
            result.R0C2 = inverse[0, 2];
            result.R1C0 = inverse[1, 0];
            result.R1C1 = inverse[1, 1];
            result.R1C2 = inverse[1, 2];
            result.R2C0 = inverse[2, 0];
            result.R2C1 = inverse[2, 1];
            result.R2C2 = inverse[2, 2];
        }

        public static float[] GetValues(this Vector3 self)
        {
            return new[] { self.X, self.Y, self.Z };
        }
    }
}