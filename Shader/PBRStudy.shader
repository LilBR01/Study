Shader "Unlit/PBRStudy"
{
    Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1 ,1 ,1 ,1)
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0 //金属度要经过伽马校正
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		_LUT("LUT", 2D) = "white" {}
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Tags {
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM


			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityStandardBRDF.cginc" 

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};

			float4 _Tint;
			float _Metallic;
			float _Smoothness;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _LUT;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.normal = normalize(o.normal);
				return o;
			}

            inline float GGX(float NdotH, float roughness)
            {   
                float a = roughness * roughness;
                float a2 = a * a;
                float d  = (NdotH * a2 - NdotH) * NdotH + 1.0f;
                return a2 / (UNITY_PI * d * d);
            }

            half disneyDiffuse(half NdotV, half NdotL, half LdotH, half perceptualRoughness)
            {
                half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
                
                half lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
                half viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));
                return lightScatter * viewScatter;
            }


            float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
            {
                return F0 + (max(float3(1.0-roughness, 1.0-roughness, 1.0-roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
            }

            half3 FresnelLerp(half F0, half HdotV)
            {
                half t =pow(1-HdotV, 5);
                return F0+(1-F0)*t;
            }

			fixed4 frag(v2f i) : SV_Target
			{
                i.normal = normalize(i.normal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfVector = normalize(lightDir + viewDir);

                float perceptualRoughness = 1 - _Smoothness;
                float roughness = perceptualRoughness * perceptualRoughness;
	            float squareRoughness = roughness * roughness;

                float nl = max(saturate(dot(i.normal, lightDir)), 0.000001);//防止除0
                float nv = max(saturate(dot(i.normal, viewDir)), 0.000001);
                float vh = max(saturate(dot(viewDir, halfVector)), 0.000001);
                float lh = max(saturate(dot(lightDir, halfVector)), 0.000001);
                float nh = max(saturate(dot(i.normal, halfVector)), 0.000001);

                float3 Albedo = _Tint * tex2D(_MainTex, i.uv);
                
                //float lerpSquareRoughness = pow(lerp(0.002, 1, roughness), 2);
                //float D = lerpSquareRoughness / (pow((pow(nh, 2) * (lerpSquareRoughness -1)+1),2) * UNITY_PI);
                float D = GGX(nh, roughness);

                float kInDirectLight = pow(squareRoughness+1, 2) / 8;
                float KInIBL = pow(squareRoughness, 2) / 8;
                float GLeft = nl / lerp(nl, 1, kInDirectLight);
                float GRight = nv / lerp(nv, 1, kInDirectLight);
                float G = GLeft * GRight;
                
                float3 F0 = lerp(unity_ColorSpaceDielectricSpec.rgb, Albedo, _Metallic);
                //float3 F = F0 + (1 - F0) * exp2((-5.55473 * vh - 6.98316) * vh);
                float F = FresnelLerp(F0, vh);

                float3 SpecularResult = (D*G*F*0.25)/(nv*nl);
                
                //kd：漫反射系数（保证能量守恒）
                float3 kd = (1-F)*(1-_Metallic);
                half diffuse = disneyDiffuse(nv, nl, lh, perceptualRoughness);
                float3 diffColor = kd * Albedo * lightColor * diffuse;
				
                float3 specColor = SpecularResult * lightColor * nl * UNITY_PI;
				
                float3 DirectLightResult = diffColor + specColor;

                //ShadeSH9中传入归一化的法线，得到重建的积过分的环境光照信息
                half3 ambient_contrib = ShadeSH9(float4(i.normal, 1));
                float3 ambient = 0.03 * Albedo;
                float3 iblDiffuse = max(half3(0,0,0), ambient.rgb + ambient_contrib);

                //采样用的粗糙度的计算，Unity的粗糙度和采样的mipmap等级关系不是线性的，Unity内使用的转换公式为mip = r(1.7 - 0.7r)
                float3 mip_roughness = perceptualRoughness * (1.7 - 0.7*perceptualRoughness);
                float3 reflectVec = reflect(-viewDir, i.normal);

                half mip = mip_roughness * UNITY_SPECCUBE_LOD_STEPS;
                half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectVec, mip);

                float3 iblSpecular = DecodeHDR(rgbm, unity_SpecCube0_HDR);
                
                float2 envBDRF = tex2D(_LUT, float2(lerp(0, 0.99, nv), lerp(0, 0.99, roughness))).rg;

                float3 Flast = fresnelSchlickRoughness(max(nv, 0.0), F0, roughness);
                float kdLast = (1-Flast) * (1-_Metallic);

				float3 iblDiffuseResult = iblDiffuse * kdLast * Albedo;

                float3 iblSpecularResult = iblSpecular * (Flast * envBDRF.r + envBDRF.g);
				float3 IndirectResult = iblDiffuseResult + iblSpecularResult;

				float4 result = float4(DirectLightResult + IndirectResult, 1);

				return result;
                //return F;
			}
			ENDCG
		}
	}
}
