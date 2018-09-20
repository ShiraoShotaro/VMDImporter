Shader "AxisLines/ColorZBuffer" {
	SubShader{
		Pass{
		Blend SrcAlpha OneMinusSrcAlpha
		Blend Off
		Cull Off
		ZWrite On
		ZTest Less
		Fog{ Mode Off }
		BindChannels{
		Bind "Vertex", vertex
		Bind "Color", color
	}
	}
	}
}
