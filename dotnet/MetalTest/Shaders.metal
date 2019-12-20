#include <metal_stdlib>
using namespace metal;

struct vertex_t
{
    float4 color;
    float2 pos;
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
    out.pos = float4(in.pos.x, in.pos.y, 0, 1);
    switch(vid){
        case 0:
            out.color = float4(1, 0, 0, 1);
            out.pos = float4(-1, -1, 0, 1);
            break;
        case 1:
            out.color = float4(0, 1, 0, 1);
            out.pos = float4(0, 1, 0, 1);
            break;
        case 2:
            out.color = float4(0, 0, 1, 1);
            out.pos = float4(1, -1, 0, 1);
            break;
    }
    
    return out;
}

fragment float4 fragmentShader(VertexOut interpolated [[stage_in]])
{
    return interpolated.color;
}
