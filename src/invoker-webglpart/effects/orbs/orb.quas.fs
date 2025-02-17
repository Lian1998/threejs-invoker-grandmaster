uniform float uRand; // 静态随机数
uniform float uRandDinamic; // 动态随机数
uniform sampler2D uMap1; // 球状着色图
uniform sampler2D uMap2; // 能量球状着色图
uniform vec3 uColor1;
uniform vec3 uColor2;
uniform vec3 uColor3;
uniform float uTime; // 渲染总时间
uniform float uLifeTime; // 生命时间

varying vec2 vUv; // uv
varying vec2 vCenter;
varying float vAlpha;

#montage import('@shaders/glsl-noise/simplex2d.glsl');
#montage import('@shaders/value-noise/1d.glsl');
#montage import('@shaders/threejs/colorspace/sRGBToLinear.glsl');
#montage import('./orbhalo.glsl');

void main() {

    // 球状描边相关
    float outerFactor = 0.38; // 要比贴图大一圈 生成一层颜色稍淡的黑边, 有点立体效果
    float innerFactor = 0.2 + valuenoise_smoothed1d(uRandDinamic) * 0.05; // 一点随机值使得边缘有点动态的感觉
    float orbhaloFactor = orbhalo(vUv, vCenter, outerFactor, innerFactor, 0.9);
    float orbhaloStrength = 2.0; // 描边颜色强度
    vec3 orbHaloColor = mix(vec3(0.0), uColor3, orbhaloFactor) * orbhaloStrength; // 描边

    // 获取帧贴图相关参数
    vec2 frameTexUv = vUv; // 此模块的独立uv
    vec2 frameScale = vec2(1.5, 1.5); // 每帧雪碧图的缩放, 只能放大
    float frameNum = 29.0; // 雪碧图包含了多少帧
    float frameSpeedScale = 2.0; // 雪碧图取值时受到时间影响的比例
    float frameIndex = floor(mod((frameNum * uRand + uLifeTime * frameSpeedScale), 1.0) * frameNum); // 计算当前显示的雪碧图帧数 int[0 ~ 28]
    float frameFloating = frameIndex * (1.0 / frameNum); // 雪碧图当前帧漂移的距离
    // 计算雪碧图取值的uv范围
    frameTexUv.x = ((frameTexUv.x / frameScale.x) + (frameScale.x - 1.0) / (2.0 * frameScale.x)) / frameNum + frameFloating;
    frameTexUv.y = (frameTexUv.y / frameScale.y) + (frameScale.y - 1.0) / (2.0 * frameScale.y);
    // 取色(uMap1普通着色球体的通道图)
    vec4 uMap1Color = texture2D(uMap1, frameTexUv);
    // 获取rgb三通道的平均强度
    float uMap1ColorStrength = (uMap1Color.r + uMap1Color.g + uMap1Color.b) / 3.0 * 1.2; 
    // 进行通道平均值的加强
    float wave = glslnoise_simplex2d(vec2(distance(vCenter, vUv) * 5.0 + uTime * frameSpeedScale)); // 计算波形(收缩)因数, uTime为负的话是扩张
    if (uMap1ColorStrength > 0.0) { // 贴图上的通道强度进行加强
        uMap1ColorStrength += 0.2;
        uMap1ColorStrength += wave * 0.2; // 产生碎片波
    }
    uMap1ColorStrength += step(distance(vCenter, vUv), 0.35) * 0.3; // 冰球像素内添加一个基础强度
    vec3 uMap1ColorMixed = mix(uColor2, uColor1, uMap1ColorStrength); // 混合出贴图色

    // 两个高光点的颜色
    vec2 highlight1Pos = vec2(0.3, 0.55); // 高光位置
    vec2 highlight2Pos = vec2(0.7, 0.55);
    vec3 highlightCyan = vec3(160.0 / 255.0, 245.0 / 255.0, 1.0); // 高光颜色
    float highlightSize = 0.13; // 高光点大小
    float highlight1Size = highlightSize + 0.05 * valuenoise_smoothed1d(uRandDinamic); // 大小随机
    float highlight2Size = highlightSize + 0.05 * valuenoise_smoothed1d(uRandDinamic);
    highlight1Pos.x += (valuenoise_smoothed1d(uTime) - 0.5) * 0.03; // 轻微移动
    highlight1Pos.y += (valuenoise_smoothed1d(uTime) - 0.5) * 0.03;
    highlight2Pos.x += (valuenoise_smoothed1d(uTime) - 0.5) * 0.03;
    highlight2Pos.y += (valuenoise_smoothed1d(uTime) - 0.5) * 0.03;
    float orbHighlight1Factor = smoothstep(highlight1Size, 0.0, length(vUv - highlight1Pos));
    float orbHighlight2Factor = smoothstep(highlight2Size, 0.0, length(vUv - highlight2Pos));
    float orbHighlightFactor = orbHighlight1Factor + orbHighlight2Factor;
    vec3 orbHighlightColor = orbHighlightFactor * highlightCyan * clamp(valuenoise_1d(vUv.x * vUv.y * uTime), 0.45, 0.55);

    // gl_FragColor = vec4(vec3(wave), 1.0); // wave Factor
    // gl_FragColor = vec4(vec3(orbhaloFactor), 1.0);  // halo Factor
    // gl_FragColor = vec4(vec3(orbHighlightFactor), 1.0); // highlight Factor
    // gl_FragColor = vec4(orbHighlightColor, 1.0); // highlight Colored
    // gl_FragColor = uMap1Color; // uMap1Color
    // gl_FragColor = vec4(uMap1ColorStrength); // uMap1Color Strength
    // gl_FragColor = vec4(uMap1ColorMixed, uMap1ColorStrength); // uMap1ColorMixed

    // Dota2卡尔该全才的那个版本, 在更新前冰球是有俩高光点的, 更新后冰球特效去除了高光点外壳有一个黑色的颜色递减描边
    // gl_FragColor = vec4(mix(vec3(uMap1ColorMixed.rgb), vec3(0.2), smoothstep(0.0, 1.0, orbhaloFactor)) + orbHighlightColor.rgb, uMap1ColorStrength + orbHighlightFactor);

    float alpha = (uMap1ColorStrength) * vAlpha;
    gl_FragColor = vec4(mix(vec3(uMap1ColorMixed.rgb), vec3(0.25), smoothstep(0.0, 1.0, orbhaloFactor)), alpha);

    gl_FragColor = sRGBToLinear(gl_FragColor);

    #include <colorspace_fragment>

}