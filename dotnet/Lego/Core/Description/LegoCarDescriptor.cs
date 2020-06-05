using System.Collections.Generic;
using Maths;

namespace Lego.Core.Description
{
    // Orientation: Nose pointing -X
    //      ____
    //  ---/    \
    //  <--0---0--
    //
    public class LegoCarDescriptor
    {
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public List<DistanceSensorDescriptor> DistanceSensors { get; set; } = new List<DistanceSensorDescriptor>();
    }

    public class DistanceSensorDescriptor
    {
        public Float4x4 ModelMatrix { get; set; }
    }
}