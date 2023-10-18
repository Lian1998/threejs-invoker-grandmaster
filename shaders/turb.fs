uniform sampler2D uMap;
uniform sampler2D uMask;
uniform vec2 uResolution;

varying float vAlpha;
varying vec2 vUv;
varying vec2 vCenter;

void main() {

    // gl_FragCoord 左下角00 右上角11
    vec2 uv = gl_FragCoord.xy / uResolution;

    // haze贴图是一个球状图形, 球状的四个角最白, 为v3(0.78)
    vec2 maskUV = vUv;
    float maskFactor = smoothstep(0.48, 0.0, distance(maskUV, vCenter));
    vec2 mask = texture2D(uMask, maskUV).ra - vec2(0.5);
    uv -= mask * maskFactor * 0.08;

    vec4 tex = texture2D(uMap, uv);

    gl_FragColor = vec4(tex.rgb, vAlpha * 0.5);
    // gl_FragColor = vec4(vec3(1.0), vAlpha);

    #include <colorspace_fragment>
}

// Threejs 运行时替换指令 `#include <encodings_fragment|colorspace_fragment>`:
// vec4 LinearTosRGB(in vec4 value) {
//     return vec4(mix(pow(value.rgb, vec3(0.41666)) * 1.055 - vec3(0.055), value.rgb * 12.92, vec3(lessThanEqual(value.rgb, vec3(0.0031308)))), value.a);
// }
// vec4 linearToOutputTexel(vec4 value) { 
//     return LinearToSRGB(value); 
// }
// void main() {
//     ...
//     gl_FragColor = linearToOutputTexel( gl_FragColor );
// }