shader_type spatial;
render_mode cull_disabled, diffuse_toon, specular_schlick_ggx;

uniform vec3 top_color: source_color;
uniform vec3 bottom_color: source_color;
uniform float ambient_occlusion_factor: hint_range(0.0, 1.0, 0.01) = 0.3;
uniform float specular_strength: hint_range(0.0, 1.0, 0.01) = 0.4;
uniform float player_displacement_strength: hint_range(0.0, 1.0, 0.01) = 0.4;
uniform float player_displacement_size: hint_range(0.0, 2.0, 0.01) = 1.0;

uniform vec3 wind_direction; // Use a negative y component to give it an extra touch (For displacement effect and noise scroll direction)
uniform float wind_strength: hint_range(0.0, 1.0, 0.01) = 0.3;
uniform sampler2D wind_noise; // Periln FBM Noise looks Best
uniform float wind_noise_size: hint_range(0.0, 1.0, 0.001) = 0.05; // high values dont work well
uniform float wind_noise_speed: hint_range(0.0, 1.0, 0.001) = 0.1;

uniform float grass_height : hint_range(0.0, 10.0, 0.01) = 1.0; // Grass height

// Array of player positions (up to 10 for example)
uniform vec3 player_positions[10];
uniform int player_count : hint_range(0, 10) = 0; // Number of active players

void vertex() {
    vec3 world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;

    vec3 wind_texture = texture(wind_noise, world_position.xz * wind_noise_size + normalize(-wind_direction.xz) * (TIME + UV.y / 2.5) * wind_noise_speed).rgb;
    vec3 wind_vector = (vec4(wind_texture * normalize(wind_direction) * wind_strength, 0.0) * MODEL_MATRIX).xyz;

    vec3 total_displacement = vec3(0.0);

    // Loop through each player position and apply the displacement effect
    for (int i = 0; i < player_count; i++) {
        vec3 player_position = player_positions[i];

        float player_height = smoothstep(1.0, 0.0, length(player_position.y - world_position.y + 0.3));
        vec3 push_direction = vec3(world_position - player_position) * vec3(1, -0.3, 1);
        float player_position_factor = smoothstep(player_displacement_size, 0.0, length(push_direction));

        vec3 player_push_vector = (vec4(normalize(push_direction), 0.0) * MODEL_MATRIX).xyz;

        total_displacement += player_push_vector * (1.0 - UV.y) * player_position_factor * player_displacement_strength * player_height;
    }

    // Apply total player displacement
    VERTEX += total_displacement;

    // Apply Wind displacement linearly
    VERTEX += wind_vector * (1.0 - UV.y) * (1.0 - length(total_displacement) * 0.7);

    // Apply grass height scaling
    VERTEX.y += grass_height/100.f * UV.y; // Adjusting height based on UV.y

    // A new normal correction, which aligns the normals of the mesh facing upwards no matter the original direction.
    NORMAL = vec3(0.0, 1.0, 0.0);
}

void fragment() {
    vec3 color = mix(bottom_color, top_color, 1.0 - UV.y);

    // Add fake ambient occlusion by darkening the base of the mesh
    float ao_falloff = pow(UV.y, 5.0);
    vec3 ao_color = bottom_color * (1.0 - ambient_occlusion_factor);

    ALBEDO = mix(color, ao_color, ao_falloff);
    ROUGHNESS = 0.4;

    // Increase the Specular with Grass Height
    SPECULAR *= (1.0 - UV.y) * specular_strength;

    // Just removing some funny shading
    if (!FRONT_FACING) {
        NORMAL = -NORMAL;
    }
}
