#version 410

layout(location=0)in vec3 v_Position;
layout(location=1)in vec3 v_Normal;
layout(location=2)in vec2 v_Uv;

uniform mat4 u_Model;
uniform mat4 u_View;
uniform mat4 u_Projection;

out vec3 f_Normal;
out vec2 f_Uv;

void main() {
    f_Normal = v_Normal;
    f_Uv = v_Uv;
    gl_Position = vec4(v_Position, 1.0) * u_Model * u_View * u_Projection;
}
