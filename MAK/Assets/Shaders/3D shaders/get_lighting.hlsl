#ifndef GET_LIGHTING
#define GET_LIGHTING

#define SPECULAR_MULTIPLIER 0.35

//Function which gets and returns lighting info for use in the shader graph
void GetLighting_half(float3 albedo, float3 position, float3 normal, float3 camera_forward,
						out float3 color, out float main_light_inv_cosine, out float camera_inv_cosine){
	#ifndef SHADERGRAPH_PREVIEW
	
		//Get shadow information
		float positionCS = TransformWorldToHClip(position); //Position of pixel in world space
		#if SHADOWS_SCREEN
			float shadowCoord = ComputeScreenPos(position);
		#else
			float shadowCoord = TransformWorldToShadowCoord(position);
		#endif
		
		Light mainLight = GetMainLight(shadowCoord, position, 1); //Get the main light for the scene and shadow info
		
		main_light_inv_cosine = saturate(dot(normal, mainLight.direction)); //Find inv cosine between light direction and surfce normal
		camera_inv_cosine = saturate(dot(normal, camera_forward));
		float specular = SPECULAR_MULTIPLIER * saturate(dot(normal, normalize(mainLight.direction + camera_forward)));
		
		color = albedo * mainLight.color * (main_light_inv_cosine + specular);
		
		//Apply additional lights in the scene
		#ifdef _ADDITIONAL_LIGHTS
		uint additionalLightCount = GetAdditionalLightsCount();
		for(uint i = 0; i < additionalLightCount; i++)
		{
			Light light = GetAdditionalLight(i, position, 1);
			color += albedo * light.color * 
			(saturate(dot(normal, light.direction)) + //inverse cosine
			SPECULAR_MULTIPLIER * saturate(dot(normal, normalize(light.direction + camera_forward)))); //specular
		}
		
		#endif
		
		//Apply fog
		//color = MixFog(color, ComputeFogFactor(positionWS.z));
		
	#else //For the shader graph preview, just display the albedo partially lit
		main_light_inv_cosine = 0.9;
		camera_inv_cosine = 0.2;
		color = albedo * main_light_inv_cosine;
		
	#endif
}

void GetLighting_float(float3 albedo, float3 position, float3 normal, float3 camera_forward,
						out float3 color, out float main_light_inv_cosine, out float camera_inv_cosine){
	#ifndef SHADERGRAPH_PREVIEW
	
		//Get shadow information
		float positionCS = TransformWorldToHClip(position); //Position of pixel in world space
		#if SHADOWS_SCREEN
			float shadowCoord = ComputeScreenPos(position);
		#else
			float shadowCoord = TransformWorldToShadowCoord(position);
		#endif
		
		Light mainLight = GetMainLight(shadowCoord, position, 1); //Get the main light for the scene and shadow info
		
		main_light_inv_cosine = saturate(dot(normal, mainLight.direction)); //Find inv cosine between light direction and surfce normal
		camera_inv_cosine = saturate(dot(normal, camera_forward));
		float specular = SPECULAR_MULTIPLIER * saturate(dot(normal, normalize(mainLight.direction + camera_forward)));
		
		color = albedo * mainLight.color * (main_light_inv_cosine + specular);
		
		//Apply additional lights in the scene
		#ifdef _ADDITIONAL_LIGHTS
		uint additionalLightCount = GetAdditionalLightsCount();
		for(uint i = 0; i < additionalLightCount; i++)
		{
			Light light = GetAdditionalLight(i, position, 1);
			color += albedo * light.color * 
			(saturate(dot(normal, light.direction)) + //inverse cosine
			SPECULAR_MULTIPLIER * saturate(dot(normal, normalize(light.direction + camera_forward)))); //specular
		}
		
		#endif
		
		//Apply fog
		//color = MixFog(color, ComputeFogFactor(positionWS.z));
		
	#else //For the shader graph preview, just display the albedo partially lit
		main_light_inv_cosine = 0.9;
		camera_inv_cosine = 0.2;
		color = albedo * main_light_inv_cosine;
		
	#endif
}

#endif