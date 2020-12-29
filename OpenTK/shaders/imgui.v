#version 330 core
uniform mat4 uProjection;

in vec2 aPosition;
in vec2 aUV;
in vec4 aColor;

out vec4 vertexColor;
out vec2 uv;

void main() {
	gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
	vertexColor = aColor;
	uv = aUV;
}