#version 330 core
uniform sampler2D Texture;

in vec4 vertexColor;
in vec2 uv;

out vec4 fragColor;

void main() {
	fragColor = vertexColor * texture2D(Texture, uv.st) ;
}