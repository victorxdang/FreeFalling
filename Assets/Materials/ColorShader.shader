
Shader "Mobile/DiffuseColor" 
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
	}
		SubShader
		{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		fixed4 _Color;

		struct Input 
		{
			bool fill;
		};

		void surf(Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG
	}

	Fallback "Mobile/Diffuse"
}
