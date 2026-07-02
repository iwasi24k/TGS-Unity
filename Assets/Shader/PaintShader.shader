Shader "CustomRenderTexture/PaintShader"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BrushTex("Brush Texture", 2D) = "white" {}
        _FadeSpeed("Fade Speed", Float) = 1.0
     }

     SubShader
     {
         Tags { "RenderType" = "Opaque" }
         LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BrushTex;

            // from C#script
            float2  _BrushUV;
            float   _BrushRadius;
            float   _BrushIntensity;
            float   _FadeSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float _fadeSpeed = _FadeSpeed;

                fixed4 baseColor = tex2D(_MainTex, i.uv);

                baseColor.r = saturate(baseColor.r - (_fadeSpeed));
                float dist = distance(i.uv, _BrushUV);
                float alpha = smoothstep(_BrushRadius, _BrushRadius * 0.5, dist);

                baseColor.r = saturate(baseColor.r + _BrushIntensity * alpha);

                return baseColor;
            }

            ENDCG
        }
    }
}
