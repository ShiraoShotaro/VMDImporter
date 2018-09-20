Shader "AxisLines/Color" {
	SubShader{
		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			Blend Off
			Cull Off
			ZWrite Off
			ZTest Less
			Fog{ Mode Off }
			BindChannels{
				Bind "Vertex", vertex
				Bind "Color", color
			}
		}
	}
}
