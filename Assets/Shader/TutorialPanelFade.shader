Shader "Custom/TutorialPanelFade"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.2,0.8,1,0.8)
        _EmissionColor("Emission", Color) = (0.2,1,1,1)
        _FadeHeight("Fade Height", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _EmissionColor;
            float _FadeHeight;

            Varyings vert (Attributes input)
            {
                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = pos.positionCS;
                output.worldPos = pos.positionWS;

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float fade =
                    saturate(
                        1 -
                        (input.worldPos.y / _FadeHeight));

                half4 col = _BaseColor;

                col.rgb += _EmissionColor.rgb * 0.25;

                col.a *= fade;

                return col;
            }

            ENDHLSL
        }
    }
}