#version 450 core

#define HAS_BASE_COLOR_TEXTURE          (1 << 0)
#define HAS_NORMAL_TEXTURE              (1 << 1)
#define HAS_METALLIC_ROUGHNESS_TEXTURE  (1 << 2)

#define HAS_TANGENTS                    (1 << 0)

layout (location = 0) in vec3 a_position;
layout (location = 1) in vec3 a_normal;
layout (location = 2) in vec4 a_tangent;
layout (location = 3) in vec2 a_texCoord;

out vec3 worldPos;
out vec3 normal;
out vec2 texCoord;
out mat3 tbn;

uniform mat4 u_model;
uniform mat4 u_view;
uniform mat4 u_projection;

uniform int u_attributes;
uniform int u_vertexAttributes;

void main() {
    // calculate the model view, we use it alot 
    mat4 modelView = u_view * u_model;
    
    // pass the world position
    worldPos = vec3(modelView * vec4(a_position, 1.0f));
    
    // if we have normal textures then take the tangent and 
    // transform it properly 
    if ((u_attributes & HAS_NORMAL_TEXTURE) != 0) {
        vec3 t;
        vec3 b;
        vec3 n = normalize(mat3(modelView) * a_normal);
        if ((u_vertexAttributes & HAS_TANGENTS) != 0) {
            // use the tangets given from the vertex data
            t = normalize(mat3(modelView) * a_tangent.xyz);
            b = cross(n, t) * a_tangent.w;
        } else {
            // Generate a default tangent and bitangent
            vec3 c1 = cross(n, vec3(0.0, 0.0, 1.0));
            vec3 c2 = cross(n, vec3(0.0, 1.0, 0.0));
            if (length(c1) > length(c2)) {
                t = c1;
            } else {
                t = c2;
            }
            t = normalize(t);
            b = normalize(cross(n, t));
        }
        tbn = mat3(t, b, n);
        
    } else {
        // pass the simple normal instead
        mat3 normalModel = mat3(transpose(inverse(modelView)));
        normal = normalModel * a_normal;
    }
    
    // pass if has texture
    if ((u_attributes & (HAS_BASE_COLOR_TEXTURE | HAS_NORMAL_TEXTURE | HAS_METALLIC_ROUGHNESS_TEXTURE)) != 0) {
        texCoord = a_texCoord;
    }
    
    // and finally pass the glPosition
    gl_Position = u_projection * vec4(worldPos, 1.0f);
}
