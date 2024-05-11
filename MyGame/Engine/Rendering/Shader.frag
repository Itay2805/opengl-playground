#version 450 core

#define HAS_BASE_COLOR_TEXTURE          (1 << 0)
#define HAS_NORMAL_TEXTURE              (1 << 1)
#define HAS_METALLIC_ROUGHNESS_TEXTURE  (1 << 2)
#define HAS_ALPHA_MASK                  (1 << 3)

// the inputs from the vertex
in vec3 worldPos;
in vec3 normal;
in vec2 texCoord;
in mat3 tbn;

out vec4 fragColor;

// the base color factor 
uniform vec4 u_baseColorFactor;
uniform float u_metallicFactor;
uniform float u_roughnessFactor;
uniform float u_alphaCutoff;
uniform float u_normalScale;

// the textures
uniform sampler2D u_baseColorTexture;
uniform sampler2D u_normalTexture;
uniform sampler2D u_metallicRoughnessTexture;

// the position of our light, for now we will only have one 
uniform vec3 u_lightPosition;
uniform vec3 u_lightColor;

// the position of the camera
uniform vec3 u_viewPosition;

// the material attributes
uniform int u_attributes;

const float PI = 3.14159265359;

float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
    float num = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness) {
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;
    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
    return num / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main() {
    vec3 albedo = u_baseColorFactor.rgb;
    float metallic = u_metallicFactor;
    float roughness = u_roughnessFactor;
    float ao = 0.0;

    float alpha = u_baseColorFactor.a;
    
    // modify the albedo by the texture 
    if ((u_attributes & HAS_BASE_COLOR_TEXTURE) != 0) {
        vec4 color = texture(u_baseColorTexture, texCoord);
        albedo *= color.rgb;
        alpha *= color.a;
    }

    // Handle alpha masking based on the cutoff value 
    if ((u_attributes & HAS_ALPHA_MASK) != 0) {
        if (alpha < u_alphaCutoff)
            discard;
    }

    // modify the metallic and roughness values by the texture
    if ((u_attributes & HAS_METALLIC_ROUGHNESS_TEXTURE) != 0) {
        vec4 color = texture(u_metallicRoughnessTexture, texCoord);
        metallic *= color.b;
        roughness *= color.g;
    }
    
    // if we have a normal map then use the calculated TBN 
    // to get the normal, otherwise just get the one from the 
    // vertex shader
    vec3 finalNormal;
    if ((u_attributes & HAS_NORMAL_TEXTURE) != 0) {
        finalNormal = texture(u_normalTexture, texCoord).rgb * 2.0 - 1.0;
        finalNormal *= vec3(u_normalScale, u_normalScale, 1.0);
        finalNormal = normalize(finalNormal);
        finalNormal = normalize(tbn * finalNormal);
    } else {
        finalNormal = normalize(normal);
    }
    
    // flip the normal if not front facing 
    if (!gl_FrontFacing) {
        finalNormal = -finalNormal;
    }
    
    vec3 N = finalNormal;
    vec3 V = normalize(u_viewPosition - worldPos);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    // calculate per-light radiance
    vec3 L = normalize(u_lightPosition - worldPos);
    vec3 H = normalize(V + L);
    float distance = length(u_lightPosition - worldPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance = u_lightColor * attenuation;

    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular = numerator / denominator;

    // add to outgoing radiance Lo
    float NdotL = max(dot(N, L), 0.0);
    vec3 Lo = (kD * albedo / PI + specular) * radiance * NdotL;

    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color = ambient + Lo;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));

    fragColor = vec4(color, 1.0);
}
