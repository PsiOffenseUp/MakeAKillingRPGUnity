#ifndef WAVY_CIRCLE
#define WAVY_CIRCLE

#define TAU 6.283852
#define AMPLITUDE_SCALE 0.65
#define COS_SCALE 0.15
#define COS_SPEED 0.45
#define NOISE_SCALE 0.85

//Function which returns whether a polar coord is in a circle with noise on the surface
void InCircle_half(float2 polar_coord, float wave_size, float radius, float speed, float wavelength, float noise, float outline_thickness, 
					out bool in_circle, out bool on_border){			
		//Figure out whether the point is in the circle
		float sine = sin(TAU * wavelength * polar_coord.y + speed * _Time.y);
		float cosine = cos(2 * TAU * wavelength * polar_coord.y + speed * COS_SPEED * _Time.y);
		float boundary = radius + NOISE_SCALE * sign(sine) * noise * AMPLITUDE_SCALE * wave_size * sine * sine + AMPLITUDE_SCALE * COS_SCALE * cosine * cosine;
		in_circle = polar_coord.x < boundary - outline_thickness;  
		on_border = polar_coord.x < boundary && !in_circle;
}

//Function which returns whether a polar coord is in a circle with noise on the surface
void InCircle_float(float2 polar_coord, float wave_size, float radius, float speed, float wavelength, float noise, float outline_thickness, 
					out bool in_circle, out bool on_border){			
		//Figure out whether the point is in the circle
		float sine = sin(TAU * wavelength * polar_coord.y + speed * _Time.y);
		float cosine = cos(2 * TAU * wavelength * polar_coord.y + speed * COS_SPEED * _Time.y);
		float boundary = radius + NOISE_SCALE * sign(sine) * noise * AMPLITUDE_SCALE * wave_size * sine * sine + AMPLITUDE_SCALE * COS_SCALE * cosine * cosine;
		in_circle = polar_coord.x < boundary - outline_thickness;  
		on_border = polar_coord.x < boundary && !in_circle;
}

#endif