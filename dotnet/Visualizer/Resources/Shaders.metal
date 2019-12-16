#include <metal_stdlib>
using namespace metal;

struct Uniforms {
    // transforms vertices and normals of the model into camera coordinates
    float4x4 modelMatrix;
    //
    float4x4 viewProjectionMatrix;
    float3x3 normalMatrix;
};

struct VertexIn {
    float3 position [[attribute(0)]];
    float3 normal [[attribute(1)]];
    float2 textCoords [[attribute(2)]];
};

struct VertexOut {
    // position in clip-space
    float4 position [[position]];
    // surface normal in world coordinates
    float3 worldNormal;
    // position of vertex in world coordinates
    float3 worldPosition;
    // texture coordinates
    //float2 textCoords;
};

vertex VertexOut vertex_main(
     VertexIn in [[stage_in]], // stage_in: built for us by loading data according to vertexDescriptor
     constant Uniforms &uniforms [[buffer(1)]]
)
{
    // Matrix multiplication is read from right to left
    float4 worldPosition = uniforms.modelMatrix * float4(in.position, 1);
    
    VertexOut out;
    
    // position -> clip space
    out.position = uniforms.viewProjectionMatrix * worldPosition;
    
    // position -> world space
    out.worldPosition = worldPosition.xyz;
    
    // normal -> world space, for calculating lighting and reflections
    out.worldNormal = uniforms.normalMatrix * in.normal;
    
    //out.textCoords = in.textCoords;
    return out;
}

constant float3 ambientIntensity = 0.2;
constant float3 lightPosition(2, 2, 2); // Light position in world space
constant float3 lightColor(1, 1, 1);
constant float3 baseColor(1.0, 0, 0);
constant float3 worldCameraPosition(0, 0, 2);
constant float specularPower = 200;

fragment float4 fragment_main(VertexOut fragmentIn [[stage_in]],
                              texture2d<float, access::sample> baseColorTexture [[texture(0)]],
                              sampler baseColorSampler [[sampler(0)]])
{
    // color from texture (jpg)
    //float3 color = baseColorTexture.sample(baseColorSampler, fragmentIn.textCoords).rgb;
    
    //return float4(1, 0, 0, 1);
    //float3 normal = normalize(fragmentIn.eyeNormal.xyz);
    //return float4(abs(normal), 1);
    
    float3 normal = normalize(fragmentIn.worldNormal.xyz);
    float3 color = abs(normal);
    
    float3 N = normalize(fragmentIn.worldNormal.xyz);
    float3 L = normalize(lightPosition - fragmentIn.worldPosition.xyz);
    float3 diffuseIntensity = saturate(dot(N, L));
    
    float3 V = normalize(worldCameraPosition - fragmentIn.worldPosition);
    float3 H = normalize(L + V);
    float specularBase = saturate(dot(N, H));
    float specularIntensity = powr(specularBase, specularPower);
    
    
    //float3 finalColor = saturate(ambientIntensity + diffuseIntensity) * lightColor * baseColor;
    float3 finalColor = saturate(ambientIntensity + diffuseIntensity) * lightColor * color + specularIntensity * lightColor;
    return float4(finalColor, 1);
}
