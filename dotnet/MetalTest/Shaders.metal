#include <metal_stdlib>
using namespace metal;

struct vertex_t
{
    float4 color [[attribute(0)]];
    float2 pos [[attribute(1)]];
};

struct VertexOut
{
    float4 color;
    float4 pos [[position]];
};

vertex VertexOut vertexShader(const device vertex_t *vertexArray [[buffer(0)]], unsigned int vid [[vertex_id]])
{
    vertex_t in = vertexArray[vid];
    VertexOut out;
    out.color = in.color;
    out.pos = float4(in.pos, 0, 1);
    return out;
}

fragment float4 fragmentShader(VertexOut interpolated [[stage_in]])
{
    return interpolated.color;
}
