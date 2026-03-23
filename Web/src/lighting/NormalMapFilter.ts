import { Filter, GlProgram, GpuProgram, Texture, UniformGroup } from 'pixi.js';

// ---------------------------------------------------------------------------
// WGSL shader (WebGPU) — vertex + fragment in one source
// ---------------------------------------------------------------------------
const wgslSource = /* wgsl */`
struct GlobalFilterUniforms {
    uInputSize:    vec4<f32>,
    uInputPixel:   vec4<f32>,
    uInputClamp:   vec4<f32>,
    uOutputFrame:  vec4<f32>,
    uGlobalFrame:  vec4<f32>,
    uOutputTexture:vec4<f32>,
};

struct NormalMapUniforms {
    uLightPosition: vec4<f32>,   // xy = screen pos (pixels), z = height, w = unused
    uLightColor:    vec4<f32>,   // rgb = light colour (0-1), a = unused
    uLightParams:   vec4<f32>,   // x = distanceSq, y = opacity, zw = unused
};

@group(0) @binding(0) var<uniform> gfu: GlobalFilterUniforms;
@group(0) @binding(1) var uTexture:  texture_2d<f32>;
@group(0) @binding(2) var uSampler:  sampler;

@group(1) @binding(0) var<uniform> nmu: NormalMapUniforms;
@group(1) @binding(1) var uNormalMap:        texture_2d<f32>;
@group(1) @binding(2) var uNormalMapSampler: sampler;

struct VSOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) uv: vec2<f32>,
};

fn filterVertexPosition(aPosition: vec2<f32>) -> vec4<f32> {
    var pos = aPosition * gfu.uOutputFrame.zw + gfu.uOutputFrame.xy;
    pos.x = pos.x * (2.0 / gfu.uOutputTexture.x) - 1.0;
    pos.y = pos.y * (2.0 * gfu.uOutputTexture.z / gfu.uOutputTexture.y) - gfu.uOutputTexture.z;
    return vec4<f32>(pos, 0.0, 1.0);
}

fn filterTextureCoord(aPosition: vec2<f32>) -> vec2<f32> {
    return aPosition * (gfu.uOutputFrame.zw * gfu.uInputSize.zw);
}

@vertex
fn mainVertex(@location(0) aPosition: vec2<f32>) -> VSOutput {
    return VSOutput(filterVertexPosition(aPosition), filterTextureCoord(aPosition));
}

@fragment
fn mainFragment(@location(0) uv: vec2<f32>) -> @location(0) vec4<f32> {
    let clampedUv = clamp(uv, gfu.uInputClamp.xy, gfu.uInputClamp.zw);
    let diffuse = textureSample(uTexture, uSampler, clampedUv);

    // Early-out for transparent pixels
    if (diffuse.a < 0.01) { return vec4<f32>(0.0); }

    // Pixel position in screen-space — correct regardless of filter pool texture size
    let pixelPos = clampedUv * gfu.uInputSize.xy;

    // Sample normal map with corrected UV for the separate RenderTexture (1280×720)
    let normalMapSize = vec2<f32>(textureDimensions(uNormalMap, 0));
    let normalMapUv   = pixelPos / normalMapSize;
    let normalSample  = textureSample(uNormalMap, uNormalMapSampler, normalMapUv);

    // Decode normal from [0,1] → [-1,1], flip Y for screen-space convention
    var n = normalSample.rgb * 2.0 - 1.0;
    n.y = -n.y;
    n = normalize(n);

    // Light vector from pixel to light in screen-space (xy in pixels, z = height)
    let lightVec = vec3<f32>(nmu.uLightPosition.xy - pixelPos, nmu.uLightPosition.z);
    let lightDir = normalize(lightVec);

    // Lambertian diffuse
    let nDotL = max(dot(n, lightDir), 0.0);

    // Inverse-square attenuation (unclamped — mirrors C# shader behaviour)
    let actualDistSq = dot(lightVec, lightVec);
    let attenuation  = max(nmu.uLightParams.x / max(actualDistSq, 1.0), 0.0);

    let lit = diffuse.rgb * nmu.uLightColor.rgb * nDotL * nDotL * attenuation * nmu.uLightParams.y;
    return vec4<f32>(lit, diffuse.a);
}
`;

// ---------------------------------------------------------------------------
// GLSL vertex (standard PixiJS filter vertex)
// ---------------------------------------------------------------------------
const glslVert = /* glsl */`
in vec2 aPosition;
out vec2 vTextureCoord;

uniform vec4 uInputSize;
uniform vec4 uOutputFrame;
uniform vec4 uOutputTexture;

vec4 filterVertexPosition(void) {
    vec2 position = aPosition * uOutputFrame.zw + uOutputFrame.xy;
    position.x = position.x * (2.0 / uOutputTexture.x) - 1.0;
    position.y = position.y * (2.0 * uOutputTexture.z / uOutputTexture.y) - uOutputTexture.z;
    return vec4(position, 0.0, 1.0);
}

vec2 filterTextureCoord(void) {
    return aPosition * (uOutputFrame.zw * uInputSize.zw);
}

void main(void) {
    gl_Position  = filterVertexPosition();
    vTextureCoord = filterTextureCoord();
}
`;

// ---------------------------------------------------------------------------
// GLSL fragment (WebGL2)
// ---------------------------------------------------------------------------
const glslFrag = /* glsl */`
in vec2 vTextureCoord;
out vec4 finalColor;

uniform sampler2D uTexture;
uniform sampler2D uNormalMap;

uniform vec4 uInputSize;
uniform vec4 uInputClamp;

uniform vec4 uLightPosition;   // xy = screen pos (pixels), z = height
uniform vec4 uLightColor;      // rgb = light colour (0-1)
uniform vec4 uLightParams;     // x = distanceSq, y = opacity

void main(void) {
    vec2 uv = clamp(vTextureCoord, uInputClamp.xy, uInputClamp.zw);
    vec4 diffuse = texture(uTexture, uv);

    if (diffuse.a < 0.01) { finalColor = vec4(0.0); return; }

    // Pixel position in screen-space — correct regardless of filter pool texture size
    vec2 pixelPos = uv * uInputSize.xy;

    // Sample normal map with corrected UV for the separate RenderTexture (1280×720)
    vec2 normalMapSize = vec2(textureSize(uNormalMap, 0));
    vec2 normalMapUv   = pixelPos / normalMapSize;
    vec4 normalSample  = texture(uNormalMap, normalMapUv);

    vec3 n = normalSample.rgb * 2.0 - 1.0;
    n.y = -n.y;
    n = normalize(n);

    vec3 lightVec = vec3(uLightPosition.xy - pixelPos, uLightPosition.z);
    vec3 lightDir = normalize(lightVec);

    float nDotL = max(dot(n, lightDir), 0.0);

    float actualDistSq = dot(lightVec, lightVec);
    float attenuation  = max(uLightParams.x / max(actualDistSq, 1.0), 0.0);

    vec3 lit = diffuse.rgb * uLightColor.rgb * nDotL * nDotL * attenuation * uLightParams.y;
    finalColor = vec4(lit, diffuse.a);
}
`;

// ---------------------------------------------------------------------------
// NormalMapFilter class
// ---------------------------------------------------------------------------

export class NormalMapFilter extends Filter {
    private readonly _normalMapUniforms: UniformGroup;

    constructor() {
        const gpuProgram = GpuProgram.from({
            vertex:   { source: wgslSource, entryPoint: 'mainVertex' },
            fragment: { source: wgslSource, entryPoint: 'mainFragment' },
        });

        const glProgram = GlProgram.from({
            vertex:   glslVert,
            fragment: glslFrag,
            name:     'normal-map-filter',
        });

        const normalMapUniforms = new UniformGroup({
            uLightPosition: { value: new Float32Array([0, 0, 75, 0]), type: 'vec4<f32>' },
            uLightColor:    { value: new Float32Array([1, 1, 1, 0]),  type: 'vec4<f32>' },
            uLightParams:   { value: new Float32Array([640000, 1, 0, 0]), type: 'vec4<f32>' },
        });

        const emptySource = Texture.EMPTY.source;

        super({
            gpuProgram,
            glProgram,
            resources: {
                normalMapUniforms,
                uNormalMap:        emptySource,
                uNormalMapSampler: emptySource.style,
            },
        });

        this._normalMapUniforms = normalMapUniforms;
    }

    /** Update light parameters before each render. */
    setLight(
        screenX:    number,
        screenY:    number,
        z:          number,
        color:      { r: number; g: number; b: number },
        distanceSq: number,
        opacity:    number,
    ): void {
        const u = this._normalMapUniforms.uniforms as {
            uLightPosition: Float32Array;
            uLightColor:    Float32Array;
            uLightParams:   Float32Array;
        };
        u.uLightPosition[0] = screenX;
        u.uLightPosition[1] = screenY;
        u.uLightPosition[2] = z;
        u.uLightColor[0] = color.r;
        u.uLightColor[1] = color.g;
        u.uLightColor[2] = color.b;
        u.uLightParams[0] = distanceSq;
        u.uLightParams[1] = opacity;
    }

    /** Swap the normal map texture used by the filter. */
    setNormalMap(texture: Texture): void {
        this.resources['uNormalMap'] = texture.source;
        this.resources['uNormalMapSampler'] = texture.source.style;
    }
}
