#ifndef GET_LIGHTING
#define GET_LIGHTING

#define SPECULAR_MULTIPLIER 0.35

//Function which gets and returns lighting info for use in the shader graph
void GetLightingBanded_half(float3 albedo, float3 position, float3 normal, float3 camera_forward, float band_count, float3 shadow_color,
						out float3 color, out float light_amount, out float main_light_inv_cosine, out float camera_inv_cosine){
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
		
		light_amount = main_light_inv_cosine + specular;
		color = albedo * mainLight.color;
		
		//Apply additional lights in the scene
		#ifdef _ADDITIONAL_LIGHTS
		uint additionalLightCount = GetAdditionalLightsCount();
		float lightAbsorbed;
		for(uint i = 0; i < additionalLightCount; i++)
		{
			Light light = GetAdditionalLight(i, position, 1);
			lightAbsorbed = light.shadowAttenuation * light.distanceAttenuation + (saturate(dot(normal, light.direction)) + //inverse cosine
			SPECULAR_MULTIPLIER * saturate(dot(normal, normalize(light.direction + camera_forward)))); //specular
			color += color * lightAbsorbed * light.color; 
			light_amount += lightAbsorbed; //specular	
		}
		
		#endif
		
		//Apply banding effect
		float bandIndex = floor(light_amount * band_count);
		color = (bandIndex <= 0) ? shadow_color : color * (bandIndex / band_count);

		
		//Apply fog
		//color = MixFog(color, ComputeFogFactor(positionWS.z));
		
	#else //For the shader graph preview, just display the albedo partially lit
		main_light_inv_cosine = 0.9;
		camera_inv_cosine = 0.2;
		light_amount = main_light_inv_cosine;
		color = albedo * (floor(light_amount * band_count) / band_count);
		
	#endif
}

void GetLightingBanded_float(float3 albedo, float3 position, float3 normal, float3 camera_forward, float band_count, float3 shadow_color,
						out float3 color, out float light_amount, out float main_light_inv_cosine, out float camera_inv_cosine){
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
		
		light_amount = main_light_inv_cosine + specular;
		color = albedo * mainLight.color;
		
		//Apply additional lights in the scene
		#ifdef _ADDITIONAL_LIGHTS
		uint additionalLightCount = GetAdditionalLightsCount();
		float lightAbsorbed;
		for(uint i = 0; i < additionalLightCount; i++)
		{
			Light light = GetAdditionalLight(i, position, 1);
			lightAbsorbed = light.shadowAttenuation * light.distanceAttenuation + (saturate(dot(normal, light.direction)) + //inverse cosine
			SPECULAR_MULTIPLIER * saturate(dot(normal, normalize(light.direction + camera_forward)))); //specular
			color += color * lightAbsorbed * light.color; 
			light_amount += lightAbsorbed; //specular	
		}
		
		#endif
		
		//Apply banding effect
		float bandIndex = floor(light_amount * band_count);
		color = (bandIndex <= 0) ? shadow_color : color * (bandIndex / band_count);

		
		//Apply fog
		//color = MixFog(color, ComputeFogFactor(positionWS.z));
		
	#else //For the shader graph preview, just display the albedo partially lit
		main_light_inv_cosine = 0.9;
		camera_inv_cosine = 0.2;
		light_amount = main_light_inv_cosine;
		color = albedo * (floor(light_amount * band_count) / band_count);
		
	#endif
}

#endif