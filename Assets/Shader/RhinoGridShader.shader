Shader "Custom/RhinoGrid"
{
    Properties
    {
        _MainLineColor ("Main Line Color", Color) = (0.2, 0.2, 0.2, 1.0)
        _MinorLineColor ("Minor Line Color", Color) = (0.1, 0.1, 0.1, 1.0)
        _MainLineSpacing ("Main Line Spacing (units)", Float) = 5.0
        _MinorLinesPerMain ("Minor Lines Per Main", Int) = 5
        _LineThickness ("Line Thickness (pixels)", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-100" } // 渲染队列提前，确保在其他物体下面
        LOD 100
        ZWrite On
        Cull Off // 双面渲染，防止从某些角度看不到

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float4x4 _CamProj;
            float4x4 _CamWorld;
            float _LineThickness;
            float _MainLineSpacing;
            int _MinorLinesPerMain;
            float4 _MainLineColor;
            float4 _MinorLineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 将世界坐标转换到相机视图空间
                float4 viewPos = mul(_CamWorld, i.worldPos);
                
                // 计算每个世界单位对应多少屏幕像素，用于像素级别的线宽控制
                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);
                float4 projCorner = mul(_CamProj, float4(1.0, 1.0, viewPos.z, 1.0));
                projCorner.xy /= projCorner.w;
                float2 pixelSizeInWorld = abs(2.0 * viewPos.z * projCorner.xy / screenSize);
                float pixelToWorld = length(pixelSizeInWorld) * 0.5; // 估算一个像素对应的世界单位大小
                
                float lineThicknessWorld = _LineThickness * pixelToWorld;

                // 计算主次网格线
                float minorSpacing = _MainLineSpacing / _MinorLinesPerMain;
                
                // 对世界坐标取模，实现无限重复的网格
                float2 gridUV = fmod(abs(i.worldPos.xz), _MainLineSpacing);
                
                // 计算到最近的次要线的距离
                float distToMinorX = min(gridUV.x, minorSpacing - gridUV.x);
                float distToMinorZ = min(gridUV.y, minorSpacing - gridUV.y);
                float distToMinor = min(distToMinorX, distToMinorZ);
                
                // 计算到最近的主线的距离
                float distToMainX = min(gridUV.x, _MainLineSpacing - gridUV.x);
                float distToMainZ = min(gridUV.y, _MainLineSpacing - gridUV.y);
                float distToMain = min(distToMainX, distToMainZ);
                
                // 确定是主线还是次要线，并计算强度
                float mainLine = smoothstep(lineThicknessWorld, 0.0, distToMain);
                float minorLine = smoothstep(lineThicknessWorld, 0.0, distToMinor) * (1.0 - mainLine);
                
                // 混合颜色
                fixed4 col = lerp(fixed4(0,0,0,0), _MinorLineColor, minorLine);
                col = lerp(col, _MainLineColor, mainLine);
                
                // 只有当线的强度大于0时才渲染，否则完全透明
                clip(col.a - 0.01);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Hidden/InternalErrorShader"
}