uniform float uRand; // 静态随机数
uniform float uRandDinamic; // 动态随机数
uniform sampler2D uMap1; // 球状着色图
uniform sampler2D uMap2; // 能量球状着色图
uniform sampler2D uMap3; // garynoise from shadertoy
uniform vec3 uColor1;
uniform vec3 uColor2;
uniform float uTime; // 渲染总时间
uniform float uLifeTime; // 生命时间

varying vec2 vUv;
varying vec2 vCenter;
varying float vAlpha;

#define PI 3.14159265358979323844

#montage import('@shaders/glsl-noise/simplex2d.glsl');
#montage import('@shaders/glsl-noise/simplex3d.glsl');
#montage import('@shaders/value-noise/1d.glsl');
#montage import('@shaders/value-noise/2d.glsl');
#montage import('@shaders/threejs/colorspace/sRGBToLinear.glsl');
#montage import('./orbhalo.glsl');

// 从 graynoise 中获取颜色
float getFireNoise(vec2 mapUv) {
    // https://www.desmos.com/calculator
    vec2 mapFreq = vec2(5.0, 20.0); // 缩放纹理产生更多的细节
    vec2 mapUvFreqed = mapUv * mapFreq;

    // cubic function插值函数模型:  [0.125, 0.875] (x - 0.5) * 0.5 + (x - 0.5) * 0.5 * 0.5 + 0.5
    // 可以使用的噪声来源:
    // 使用由三角面优化片元计算的柏林噪声: glslnoise_simplex2d(mapUvFreqed)
    // 使用由shadertoy提供的静态通道贴图: texture2D(uMap3, mapUv).r
    // 使用由sin波生成的随机噪声: valueNoise2d(mapUvFreqed)
    float n = (texture2D(uMap3, mapUv).r - 0.5) * 0.5 + (texture2D(uMap3, mapUv * 2.0).r - 0.5) * 0.5 * 0.5 + 0.5;

    return n;
}

void main() {

    // 球状描边
    float outerFactor = 0.3;
    float innerFactor = 0.22 + valuenoise_smoothed1d(uRandDinamic) * 0.1;
    float orbhaloFactor = orbhalo(vUv, vCenter, outerFactor, innerFactor, 0.3);
    float orbhaloStrength = 0.5;
    float wave = glslnoise_simplex2d(vec2(distance(vCenter, vUv) * 8.0 - uTime * 2.0)); // 计算波形(收缩)因数, uTime为负的话是扩张
    vec3 orbHaloColor = mix(vec3(0.0), uColor2, orbhaloFactor) * orbhaloStrength * wave; // 描边

    // 迸发火焰色 from shadertoy https://www.shadertoy.com/view/ds3cWB

    // 将坐标转化为极坐标x是中心点到边缘 y是角坐标 
    vec2 polarUv = vUv; // 计算出的极坐标 
    polarUv -= vCenter; // (x=[-0.5, 0.5], y=[-0.5, 0.5])
    polarUv = vec2(length(polarUv) / sqrt(0.5), (atan(polarUv.y, polarUv.x) / (2.0 * PI) + 0.5)); // (r, a) (r=[0.0, 1.0], a=[0.0, 1.0 CCW from W])
    // 参数计算
    vec2 flameUv = polarUv;
    flameUv.x /= 2.0; // 生成火焰贴图的缩放
    float flameSpeed = 0.2;
    float flameTime = uTime * flameSpeed;
    // Get noise for this fragment (in polar coordinates) and time
    float noiseResolution = 0.5;
    float fragmentNoise = getFireNoise(flameUv * vec2(noiseResolution / 2.0, noiseResolution) + vec2(-flameTime, 0.0));
    // Randomize radius based on noise and radius
    float variationR = (fragmentNoise - 0.5);
    variationR *= smoothstep(0.05, 0.4, flameUv.x); // Straighten the flame at the center and make full turbulence outside
    variationR *= 0.25; // Scale variation 0.65
    flameUv.x += variationR; // Randomize!
    // Calculate brightness
    float radius1 = flameUv.x; 
    // Focus (compress dynamics)
    float radius2 = smoothstep(0.2, 0.35, radius1);
    float brightness = 1.0 - radius2;
    // Emit
    vec3 flameColor = mix(vec3(0.9), vec3(0.9, 0.9, 0.6), smoothstep(0.1, 0.2, radius1));
    flameColor = mix(flameColor, vec3(0.9, 0.4, 0.1), smoothstep(0.18, 0.25, radius1));
    // Blend with background
    vec4 flameColor4 = mix(vec4(0.0), vec4(flameColor, 1.0), brightness);

    // 如果有火焰明度使用火焰明度, 否则的话就使用描边色
    float flameStep = step(0.0, flameColor4.r + flameColor4.g + flameColor4.b);
    float alpha = (max(orbhaloFactor * orbhaloStrength, flameColor4.a)) * vAlpha;
    gl_FragColor = vec4(flameStep * orbHaloColor.rgb + flameStep * flameColor4.rgb, alpha);

    gl_FragColor = sRGBToLinear(gl_FragColor);

    #include <colorspace_fragment>

}