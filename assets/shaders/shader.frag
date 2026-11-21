#version 410

in vec3 f_Normal;
in vec2 f_Uv;

out vec4 out_Color;

uniform vec4 u_Color;
uniform sampler2D u_Texture;

void main() {
    out_Color = texture(u_Texture, f_Uv) * u_Color;
}
