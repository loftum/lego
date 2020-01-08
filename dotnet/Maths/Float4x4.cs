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

        
        
        
    }
}