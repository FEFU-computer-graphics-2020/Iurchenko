#version 330 core
in vec3 aPosition;
in vec3 aColor;

out vec4 vertexColor;

uniform float scaleFactor;
uniform mat4 model;
uniform mat4 projection;

void main() {
	vec3 newPosition = aPosition;
	gl_Position = projection * model * vec4(newPosition * scaleFactor, 1.0);
	vertexColor = vec4(aColor, 1.0);
}