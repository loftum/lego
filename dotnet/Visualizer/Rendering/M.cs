using System;
using System.Linq;
using Maths;
using OpenTK;
using Vector3 = OpenTK.Vector3;

namespace Visualizer.Rendering
{
    public static class M
    {
        public static Float4x4 Rotate(this Float4x4 matrix, Float3 angles)
        {
            var rotations = new[]
            {
                
                Float4x4.CreateRotation(angles.X, 1, 0, 0),
                Float4x4.CreateRotation(angles.Y, 0, 1, 0),
                Float4x4.CreateRotation(angles.Z, 0, 0, 1)
            };
            return rotations.Aggregate(matrix, Float4x4.Mult);
        }
        
        public static Matrix4 Rotate(this Matrix4 matrix, Vector3 angles)
        {
            var rotations = new[]
            {
                CreateMatrixFromRotation(angles.X, 1, 0, 0),
                CreateMatrixFromRotation(angles.Y, 0, 1, 0),
                CreateMatrixFromRotation(angles.Z, 0, 0, 1)
            };
            return rotations.Aggregate(matrix, Matrix4.Mult);
        }
        
        public static Matrix4 CreateMatrixFromRotation(float radians, float x, float y, float z)
        {
            Vector3 v = Vector3.Normalize(new Vector3(x, y, z));
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            float cosp = 1f - cos;

            var m = new Matrix4
            {
                Row0 = new Vector4(cos + cosp * v.X * v.X, cosp * v.X * v.Y - v.Z * sin, cosp * v.X * v.Z + v.Y * sin, 0f),
                Row1 = new Vector4(cosp * v.X * v.Y + v.Z * sin, cos + cosp * v.Y * v.Y, cosp * v.Y * v.Z - v.X * sin, 0f),
                Row2 = new Vector4(cosp * v.X * v.Z - v.Y * sin, cosp * v.Y * v.Z + v.X * sin, cos + cosp * v.Z * v.Z, 0f),
                Row3 = new Vector4(0f, 0f, 0f, 1f)
            };

            return m;
        }

        public static Matrix4 CreateMatrixFromTranslation(float x, float y, float z)
        {
            var m = Matrix4.Identity;
            m.Row0.W = x;
            m.Row1.W = y;
            m.Row2.W = z;
            m.Row3.W = 1f;
            return m;
        }

        public static Matrix4 CreateMatrixFromPerspective(float fovY, float aspect, float nearZ, float farZ)
        {
            float yscale = 1f / (float)Math.Tan(fovY * .5f);
            float xscale = yscale / aspect;
            float q = farZ / (farZ - nearZ);

            var m = new Matrix4
            {
                Row0 = new Vector4(xscale, 0f, 0f, 0f),
                Row1 = new Vector4(0f, yscale, 0f, 0f),
                Row2 = new Vector4(0f, 0f, q, q * -nearZ),
                Row3 = new Vector4(0f, 0f, 1f, 0f)
            };

            return m;
        }

        public static Matrix4 PerspectiveProjectionFov(float fovRadians, float aspectRatio, float nearZ, float farZ)
        {
            var yScale = (float) (1 / Math.Tan(fovRadians * 0.5));
            var xScale = (float) (yScale / aspectRatio);
            var zRange = (float) (farZ - nearZ);
            var zScale = (float) (-(farZ + nearZ) / zRange);
            var wzScale = (float) (-2 * farZ * nearZ / zRange);
            var xx = xScale;
            var yy = (float)yScale;
            var zz = zScale;
            var zw = -1f;
            var wz = wzScale;
 
            return new Matrix4(new Vector4(xx, 0, 0, 0),
                new Vector4(0, yy, 0, 0),
                new Vector4(0, 0, zz, zw),
                new Vector4(0, 0, wz, 0)
                );
        }
    }
}