Shader "Custom/AnimatedGrass2D"
{
    Properties
    {
        [Header(Texture)]
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Wind Wave Animation)]
        [Toggle(WIND_ENABLED)] _WindEnabled ("Enable Wind", Float) = 1
        _WindSpeed ("Wind Speed", Range(0, 5)) = 1.0
        _WindFrequency ("Wind Frequency", Range(0, 10)) = 2.0
        _WindAmplitude ("Wind Amplitude", Range(0, 0.1)) = 0.02
        _WindDirection ("Wind Direction", Vector) = (1, 0.3, 0, 0)

        [Header(Interactive Grass)]
        [Toggle(INTERACTION_ENABLED)] _InteractionEnabled ("Enable Interaction", Float) = 1
        _InteractionRadius ("Interaction Radius", Range(0, 5)) = 2.0
        _InteractionStrength ("Interaction Strength", Range(0, 0.2)) = 0.05
        _InteractionRecovery ("Recovery Speed", Range(0, 10)) = 3.0
        _InteractionPosition ("Interaction Position", Vector) = (0, 0, 0, 0)

        [Header(Color Variation)]
        [Toggle(COLOR_VARIATION_ENABLED)] _ColorVariationEnabled ("Enable Color Variation", Float) = 0
        _ColorVariationAmount ("Color Variation Amount", Range(0, 0.3)) = 0.1
        _ColorVariationScale ("Color Variation Scale", Range(0.1, 10)) = 2.0

        [Header(Shimmer Effect)]
        [Toggle(SHIMMER_ENABLED)] _ShimmerEnabled ("Enable Shimmer", Float) = 0
        _ShimmerSpeed ("Shimmer Speed", Range(0, 3)) = 1.0
        _ShimmerIntensity ("Shimmer Intensity", Range(0, 1)) = 0.3

        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10 // OneMinusSrcAlpha
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0 // Off
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull [_Cull]
        Lighting Off
        ZWrite [_ZWrite]
        Blend [_SrcBlend] [_DstBlend]

        Pass
        {
            Name "Sprite"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fragment _ PIXELSNAP_ON
            #pragma shader_feature WIND_ENABLED
            #pragma shader_feature INTERACTION_ENABLED
            #pragma shader_feature COLOR_VARIATION_ENABLED
            #pragma shader_feature SHIMMER_ENABLED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;

                // Wind
                half _WindSpeed;
                half _WindFrequency;
                half _WindAmplitude;
                half4 _WindDirection;

                // Interaction
                half _InteractionRadius;
                half _InteractionStrength;
                half _InteractionRecovery;
                half4 _InteractionPosition;

                // Color Variation
                half _ColorVariationAmount;
                half _ColorVariationScale;

                // Shimmer
                half _ShimmerSpeed;
                half _ShimmerIntensity;
            CBUFFER_END

            // Simple noise function
            float noise(float2 pos)
            {
                return frac(sin(dot(pos, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                float3 worldPos = TransformObjectToWorld(IN.vertex.xyz);
                float2 offset = float2(0, 0);

                #ifdef WIND_ENABLED
                    // Wind wave animation
                    float time = _Time.y * _WindSpeed;
                    float2 windDir = normalize(_WindDirection.xy);
                    float wave = sin(worldPos.x * _WindFrequency + worldPos.y * _WindFrequency * 0.5 + time) * _WindAmplitude;
                    offset += windDir * wave;
                #endif

                #ifdef INTERACTION_ENABLED
                    // Interactive grass displacement
                    float2 distanceVector = worldPos.xy - _InteractionPosition.xy;
                    float distance = length(distanceVector);

                    if (distance < _InteractionRadius && distance > 0.01)
                    {
                        float influence = 1.0 - (distance / _InteractionRadius);
                        influence = smoothstep(0.0, 1.0, influence);

                        // Push away from interaction point
                        float2 pushDirection = normalize(distanceVector);
                        offset += pushDirection * influence * _InteractionStrength;

                        // Add recovery oscillation
                        float recovery = sin(_Time.y * _InteractionRecovery) * 0.5 + 0.5;
                        offset *= 1.0 - (recovery * 0.3);
                    }
                #endif

                // Apply offset
                worldPos.xy += offset;

                OUT.worldPos = worldPos;
                OUT.vertex = TransformWorldToHClip(worldPos);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord) * IN.color;

                #ifdef COLOR_VARIATION_ENABLED
                    // Add subtle color variation
                    float variation = noise(IN.worldPos.xy * _ColorVariationScale);
                    c.rgb *= 1.0 + (variation - 0.5) * _ColorVariationAmount;
                #endif

                #ifdef SHIMMER_ENABLED
                    // Add shimmer effect
                    float shimmer = sin(IN.worldPos.x * 2.0 + IN.worldPos.y + _Time.y * _ShimmerSpeed) * 0.5 + 0.5;
                    shimmer = pow(shimmer, 3.0); // Sharpen the shimmer
                    c.rgb += shimmer * _ShimmerIntensity;
                #endif

                c.rgb *= c.a;
                return c;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
