Shader "Custom/LargeGridShader"
{
    Properties
    {
        _GridSize("Grid Size (Units)", Float) = 100.0
        _MainLineSpacing("Main Line Spacing", Float) = 10.0
        _MinorLinesPerMain("Minor Lines Per Main", Int) = 5
        _MainLineColor("Main Line Color", Color) = (0.3, 0.3, 0.3, 1.0)
        _MinorLineColor("Minor Line Color", Color) = (0.15, 0.15, 0.15, 0.8)
        _LineThickness("Line Thickness (Pixels)", Float) = 1.2
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
        }
        LOD 100

        // 透明物体通常关闭深度写入，但保留深度测试
        ZWrite Off
        ZTest LEqual
        Cull Off

        // 启用 Alpha 混合
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD0;
            };

            float _GridSize;
            float _MainLineSpacing;
            int _MinorLinesPerMain;
            float4 _MainLineColor;
            float4 _MinorLineColor;
            float _LineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 只在网格范围内绘制
                float halfSize = _GridSize * 0.5;
                if (abs(i.worldPos.x) > halfSize || abs(i.worldPos.z) > halfSize)
                {
                    discard;
                }

                // 计算像素在世界空间中的大小（用于自适应线宽）
                float pixelSizeWorld = length(ddx(i.worldPos.xyz)) * _ScreenParams.x;
                float lineThicknessWorld = _LineThickness / pixelSizeWorld;

                float minorSpacing = _MainLineSpacing / _MinorLinesPerMain;
                
                // 主线距离（X 和 Z 方向）
                float distX = min(fmod(i.worldPos.x + halfSize, _MainLineSpacing), _MainLineSpacing - fmod(i.worldPos.x + halfSize, _MainLineSpacing));
                float distZ = min(fmod(i.worldPos.z + halfSize, _MainLineSpacing), _MainLineSpacing - fmod(i.worldPos.z + halfSize, _MainLineSpacing));
                float distToMain = min(distX, distZ);
                
                // 次要线距离
                float minorDistX = min(fmod(i.worldPos.x + halfSize, minorSpacing), minorSpacing - fmod(i.worldPos.x + halfSize, minorSpacing));
                float minorDistZ = min(fmod(i.worldPos.z + halfSize, minorSpacing), minorSpacing - fmod(i.worldPos.z + halfSize, minorSpacing));
                float distToMinor = min(minorDistX, minorDistZ);

                // 使用 smoothstep 创建抗锯齿线条（从线中心向外淡出）
                float mainIntensity = smoothstep(lineThicknessWorld, 0.0, distToMain);
                float minorIntensity = smoothstep(lineThicknessWorld, 0.0, distToMinor) * (1.0 - mainIntensity);

                // 混合颜色：先叠加次要线，再覆盖主线
                fixed4 col = _MinorLineColor * minorIntensity + _MainLineColor * mainIntensity;

                // 确保完全透明区域不渲染（可选，提升性能）
                // 注意：不要用 clip 过于激进，否则会破坏半透明边缘
                if (col.a < 0.01)
                    discard;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Hidden/InternalErrorShader"
}