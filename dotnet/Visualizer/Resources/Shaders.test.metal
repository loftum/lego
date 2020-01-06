#include <metal_stdlib>
#include <simd/simd.h>
using namespace metal;

struct TestVertexIn {
    float3 position  [[attribute(0)]];
    float3 normal    [[attribute(1)]];
};

struct TestVertexOut {
    float4 position [[position]];
    float4 worldNormal;
};

struct TestVertexUniforms {
    float4x4 viewProjectionMatrix;
    float4x4 normalMatrix;
};

#define LightCount 3

struct TestFragmentUniforms {
    float3 cameraWorldPosition;
    float3 ambientLightColor;
    float3 specularColor;
    float specularPower;
};

vertex TestVertexOut vertex_test(TestVertexIn vertexIn [[stage_in]],
                             constant TestVertexUniforms &uniforms [[buffer(1)]])
{
    TestVertexOut vertexOut;
    //vertexOut.position = float4(vertexIn.position, 1);
    vertexOut.position = uniforms.viewProjectionMatrix * float4(vertexIn.position, 1);
    vertexOut.worldNormal = normalize(uniforms.normalMatrix * float4(vertexIn.normal, 0));
    
    return vertexOut;
}

fragment float4 fragment_test(TestVertexOut fragmentIn [[stage_in]],
                              constant TestFragmentUniforms &uniforms [[buffer(0)]])
{
    //return float4(1.0, 0.7, 0.0, 1.0);
    //return float4(uniforms.ambientLightColor, 1);
    
//    float3 baseColor(0.5, 0.5, 0.5);
//    float3 specularColor = uniforms.specularColor;
//
    float4 N = normalize(fragmentIn.worldNormal);
    //float3 V = normalize(uniforms.cameraWorldPosition - fragmentIn.worldPosition);
    return float4(abs(N.xyz), 1);
}
