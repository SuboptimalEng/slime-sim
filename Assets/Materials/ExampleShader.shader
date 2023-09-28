Shader "Unlit/ExampleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        // note: maybe add "RenderPipeline" = "UniversalPipeline" "Queue"="AlphaTest+51"
        // todo: why do we need to do this though -> maybe an issue with unity itself?
        // note: also looks like material may be transparent
        // IMPORTANT: maybe change material render from "shader" -> "transparent" or set to "2501"
        // IMPORTANT: maybe change assets/settings/urp-highfidelity-renderer.asset rendering to differed
        // IMPORTANT: maybe keep assets/settings/urp-highfidelity-renderer.asset as forward + disable depth priming
        // https://forum.unity.com/threads/urp-shader-example-doesnt-work.1408087/
        Tags { "RenderType"="Opaque" }
        // Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        // Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest+51" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // note: semicolon
        // struct VertexInput
        struct Attributes
        {
            float4 position : POSITION;
            float2 uv : TEXCOORD0;
        };

        // note: semicolon
        // note: can call this varyings instead
        // struct VertexOutput
        struct Varyings
        {
            // note: sv_position means pixel position
            float4 position : SV_POSITION;
            float2 uv : TEXCOORD0;

        };

        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // note: no semicolon
            Varyings vert(Attributes i)
            {
                Varyings o;
                o.position = TransformObjectToHClip(i.position.xyz);
                o.uv = i.uv;
                return o;
            }

            // note: no semicolon
            float4 frag(Varyings i) : SV_TARGET
            {
                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return baseTex * 0.9 * _BaseColor;
            }

            ENDHLSL
        }
    }
}
