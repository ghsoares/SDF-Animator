shader_type canvas_item;

// Default uniforms
uniform vec2 cameraPos;
uniform vec2 cameraZoom = vec2(1f);
uniform vec2 canvasSize = vec2(1024, 600);
uniform bool antialised = false;
uniform float antialisingPixels = 2.0;

// Compiled uniforms goes here
//{uniforms}

// Varying
varying vec2 vertexPos;
varying float time;

// Default functions
float smin(float a, float b, float k) {
  float h = clamp(0.5 + 0.5*(a-b)/k, 0.0, 1.0);
  return mix(a, b, h) - k*h*(1.0-h);
}

float smax(float a, float b, float k) {
	return smin(a, b, -k);
}

float combine(float a, float b, float k) {
	return smin(a, b, k);
}

float intersect(float a, float b, float k) {
	return smax(a, b, k);
}

float subtract(float a, float b, float k) {
	return smax(a, -b, k);
}

float blend(float a, float b, float k, int blendMode) {
	if (blendMode == 1) return intersect(a, b, k);
	if (blendMode == 2) return subtract(a, b, k);

	return combine(a, b, k);
}

// Position transformation functions goes here
//{posTrFunctions}

// SDF functions goes here
//{sdfFunctions}

// Main scene function, must sync with the scene change
float Scene(vec2 p) {
	float dst = antialisingPixels;
	// Scene code goes here
	//{scene}
	
	//{sceneBlend}
	
	return dst;
}

void vertex() {
	vertexPos = VERTEX;
	vertexPos -= canvasSize * .5;
	vertexPos *= cameraZoom;
	vertexPos += cameraPos;
	
	time = TIME;
}

void fragment() {
	float sc = Scene(vertexPos);
	
	//COLOR.rgb = vec3(fract(sc / 4.0));
	
	sc = antialised ? 1f - clamp(sc / (antialisingPixels * cameraZoom.x), 0.0, 1.0) : (sc < 0f ? 1f : 0f);
	
	COLOR.a *= sc;
}