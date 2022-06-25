Shader "Unlit/HmapShader"
{
    Properties
    {
		_Offset("Offset", vector) = (0, 0, 0, 0)
		_WaveAmplitude("Wave amplitude", float) = 0.5
		_WaveFrequency("Wave frequency", float) = 1
		_WaveStretch("Wave stretch", float) = 2
		_WaveFbmGain("Wave fBm gain", float) = 0.3
		_WaveFbmReso("Wave fBm resolution", range(0, 1)) = 1
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
			#pragma require 2darray

			#include "UnityCG.cginc"
			#include "Waves.cginc"

			struct VertexInput
			{
				float4 pos : POSITION;			
			};

            struct VertexOutput
            {
				float4 pos : SV_POSITION;
				float2 worldPos : TEXCOORD0;
            };

			uint _Seed;
			float3 _Offset;
			float _WaveAmplitude;
			float _WaveFrequency;
			float _WaveStretch;
			float _WaveFbmGain;
			float _WaveFbmReso;

			float4 calcTerrain(float3 pos, float amp, float freq, float stretch, float gain, float resolutionFactor)
			{
				pos.x /= stretch;
				float numIter = calcFbmNumIterFromGrad(resolutionFactor, freq, 15, pos.xy);
				float4 noise = fbmNoise(pos, numIter, amp, freq, gain);
				return noise;
			}
			
			//Vertex shader
            VertexOutput vert(VertexInput input)
            {
				VertexOutput output;
				//Clip space
				output.pos = UnityObjectToClipPos(input.pos);

				//World space
				output.worldPos = mul(unity_ObjectToWorld, input.pos);
				return output;
            }

			//Fragment shader
			fixed4 frag(VertexOutput input) : SV_Target
			{
				//Sample height (w) and normal (xyz)
				float4 terrain = calcTerrain(
					float3(input.worldPos + _Offset.xy, _Offset.z),
					_WaveAmplitude,
					_WaveFrequency,
					_WaveStretch,
					_WaveFbmGain,
					_WaveFbmReso
				);
				return terrain.w * 0.5f + 0.5f; //Return normalized height
            }

            ENDCG
        }
    }
}
