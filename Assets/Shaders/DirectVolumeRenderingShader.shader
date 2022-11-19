Shader "VolumeRendering/DirectVolumeRenderingShader"
{
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
        _SegmentTex("Segment Texture (Generated)", 3D) = "" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma multi_compile __ MOUSEDOWN_ON
            #pragma multi_compile __ DiggingWidget ErasingWidget
            #pragma multi_compile __ TF2D_ON            
            #pragma multi_compile __ LIGHTING_ON
            #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
            #pragma multi_compile __ DVR_BACKWARD_ON
            #pragma multi_compile __ RAY_TERMINATE_ON
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            RWStructuredBuffer<float4> buffer: register(u1);
            RWStructuredBuffer<float4> buffer_Mask: register(u2);

            #include "UnityCG.cginc"
            #include "SDF.cginc"

            #define MOUSEDOWN_ON DiggingWidget || ErasingWidget            

            struct vert_in
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct frag_out
            {
                float4 colour : SV_TARGET;
#if DEPTHWRITE_ON
                float depth : SV_DEPTH;
#endif
            };

            sampler3D _DataTex;
            sampler3D _GradientTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;
            sampler3D _SegmentTex;

            float _MinVal;
            float _MaxVal;

            float _CircleSize[10];
            float _LensIndexs[10];

            float4 _WidgetPos[10];           // Mouse Click pos
            float4 _WidgetRecorder[10];
            int _WidgetNums = 0;
            int _RecordNums = 0;
            float4x4 _RenderRotate;
            float4x4 _RotateMatrix[10];
            float4x4 _RotateMatrixInverse[10];

            float4 _camLocalPos[6];
            float4x4 _CamToWorld[6];

            float _localDepth;
            float _depthNP;

            float _maxDataVal;
            int _isoCount;
            float _isoRange[10];
            float4 _isoCluster[10];

            struct RayInfo
            {
                float3 startPos;
                float3 endPos;
                float3 direction;
                float2 aabbInters;
            };

            struct RaymarchInfo
            {
                RayInfo ray;
                int numSteps;
                float numStepsRecip;
                float stepSize;
            };

            float3 getViewRayDir(float3 vertexLocal, int k)
            {
                if(unity_OrthoParams.w == 0)
                {
                    // Perspective
                    return normalize(ObjSpaceViewDir(float4(vertexLocal, 0.0f)));
                }
                else
                {
                    // Orthographic
                    float3 camfwd = mul(_CamToWorld[k], _camLocalPos[k].xyz);
                    float4 camfwdobjspace = mul(unity_WorldToObject, camfwd);
                    return normalize(camfwdobjspace);
                }
            }

            // Find ray intersection points with axis aligned bounding box
            float2 intersectAABB(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
            {
                float3 tMin = (boxMin - rayOrigin) / rayDir;
                float3 tMax = (boxMax - rayOrigin) / rayDir;
                float3 t1 = min(tMin, tMax);
                float3 t2 = max(tMin, tMax);
                float tNear = max(max(t1.x, t1.y), t1.z);
                float tFar = min(min(t2.x, t2.y), t2.z);
                return float2(tNear, tFar);
            };

            // Get a ray for the specified fragment (back-to-front)
            RayInfo getRayBack2Front(float3 vertexLocal, int k)
            {
                RayInfo ray;
                ray.direction = getViewRayDir(vertexLocal, k);
                ray.startPos = vertexLocal + float3(0.5f, 0.5f, 0.5f);
                // Find intersections with axis aligned boundinng box (the volume)
                ray.aabbInters = intersectAABB(ray.startPos, ray.direction, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));

                // Check if camera is inside AABB
                const float3 farPos = ray.startPos + ray.direction * ray.aabbInters.y - float3(0.5f, 0.5f, 0.5f);
                float4 clipPos = UnityObjectToClipPos(float4(farPos, 1.0f));
                ray.aabbInters += min(clipPos.w, 0.0);

                ray.endPos = ray.startPos + ray.direction * ray.aabbInters.y;
                return ray;
            }

            // Get a ray for the specified fragment (front-to-back)
            RayInfo getRayFront2Back(float3 vertexLocal, int k)
            {
                RayInfo ray = getRayBack2Front(vertexLocal, k);
                ray.direction = -ray.direction;
                float3 tmp = ray.startPos;
                ray.startPos = ray.endPos;
                ray.endPos = tmp;
                return ray;
            }

            RaymarchInfo initRaymarch(RayInfo ray, int maxNumSteps)
            {
                RaymarchInfo raymarchInfo;
                raymarchInfo.stepSize = 1.732f/*greatest distance in box*/ / maxNumSteps;
                raymarchInfo.numSteps = (int)clamp(abs(ray.aabbInters.x - ray.aabbInters.y) / raymarchInfo.stepSize, 1, maxNumSteps);
                raymarchInfo.numStepsRecip = 1.0 / raymarchInfo.numSteps;
                return raymarchInfo;
            }

            // Gets the colour from a 1D Transfer Function (x = density)
            float4 getTF1DColour(float density)
            {
                return tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));
            }

            // Gets the colour from a 2D Transfer Function (x = density, y = gradient magnitude)
            float4 getTF2DColour(float density, float gradientMagnitude)
            {
                return tex2Dlod(_TFTex, float4(density, gradientMagnitude, 0.0f, 0.0f));
            }

            // Gets the density at the specified position
            float getDensity(float3 pos)
            {
                return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f));
            }

            float getSegCluser(float3 pos)
            {
                return tex3Dlod(_SegmentTex, float4(pos.x, pos.y, pos.z, 0.0f));
            }

            // Gets the gradient at the specified position
            float3 getGradient(float3 pos)
            {
                return tex3Dlod(_GradientTex, float4(pos.x, pos.y, pos.z, 0.0f)).rgb;
            }

            // Performs lighting calculations, and returns a modified colour.
            float3 calculateLighting(float3 col, float3 normal, float3 lightDir, float3 eyeDir, float specularIntensity)
            {
                float ndotl = max(lerp(0.0f, 1.5f, dot(normal, lightDir)), 0.5f); // modified, to avoid volume becoming too dark
                float3 diffuse = ndotl * col;
                float3 v = eyeDir;
                float3 r = normalize(reflect(-lightDir, normal));
                float rdotv = max( dot( r, v ), 0.0 );
                float3 specular = pow(rdotv, 32.0f) * float3(1.0f, 1.0f, 1.0f) * specularIntensity;
                return diffuse + specular;
            }

            // Converts local position to depth value
            float localToDepth(float3 localPos)
            {
                float4 clipPos = UnityObjectToClipPos(float4(localPos, 1.0f));

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                return (clipPos.z / clipPos.w) * 0.5 + 0.5;
#else
                return clipPos.z / clipPos.w;
#endif
            }           

            int findClusterIndex(float density)
            {
                int clusterIndex = -1;
                for (int i = 0; i < _isoCount; i++)
                {
                    if (_isoRange[i] > (density * _maxDataVal))
                    {
                        clusterIndex = i;
                        return clusterIndex;
                    }
                }
                return clusterIndex;
            }
            float2 findIsoRange(float index)
            {
                float2 val;
                val.x = _isoCluster[index].x / _maxDataVal;
                val.y = (_isoCluster[index].y + 1) / _maxDataVal;
                return val;
            }

            frag_in vert_main (vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float4 B2F(float dp, RayInfo ray, float zDepthFX, float zDepthFY, float Findex, float noiseVal) {
#define MAX_NUM_STEPS 512
#define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

                RaymarchInfo raymarchInfo = initRaymarch(ray, MAX_NUM_STEPS);

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                ray.startPos += (2.0f * ray.direction * raymarchInfo.stepSize) * noiseVal;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float3 rPos, bPos;
                int clusterIndex = -1;
                float maxAcc = -1.0f;

                float3 maxPos = float3(-1.0f, -1.0f, -1.0f);
                float isoVal;
                float minBoundAcc = 0; // init
                int pIndex = -1;
                int isoIndex;
                float3 lastPos;
                float lastDensity;
                float segDensity;
                int lastIndex = -1;

                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);


                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);

                    pIndex = findClusterIndex(density);

                    float4 src = getTF1DColour(density);

                    if (density < _MinVal || density > _MaxVal)   continue;
                    if (abs(dp) == 1 || abs(dp) == 4) {
                        if ((currPos.x <= zDepthFX) || (currPos.x < zDepthFY && pIndex == Findex)) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 2 || abs(dp) == 5) {
                        if ((currPos.y <= zDepthFX) || (currPos.y < zDepthFY && pIndex == Findex)) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 3 || abs(dp) == 6) {
                        if ((currPos.z <= zDepthFX) || (currPos.z < zDepthFY && pIndex == Findex)) {
                            continue;
                        }
                    }

                    col.rgb = src.a * src.rgb + (1.0f - src.a) * col.rgb;
                    col.a = src.a + (1.0f - src.a) * col.a;

                    if (col.a >= 1.0f)
                        break;

                    if (clusterIndex == -1) {
                        maxAcc = max((col.a - minBoundAcc), maxAcc);
                        maxPos = currPos;
                        rPos = currPos;
                        bPos = currPos;
                        lastDensity = density;
                        lastPos = currPos;
                        lastIndex = pIndex;
                        clusterIndex = pIndex;
                        minBoundAcc = col.a;
                        isoIndex = pIndex;
                        segDensity = getSegCluser(lastPos);
                    }
                    else {
                        if (clusterIndex != pIndex) {

                            if ((col.a - minBoundAcc) > maxAcc) {
                                maxAcc = max((col.a - minBoundAcc), maxAcc);
                                minBoundAcc = col.a;
                                clusterIndex = pIndex;
                                rPos = lastPos;
                                bPos = currPos;
                                isoIndex = lastIndex;

                                lastPos = currPos;
                                lastIndex = pIndex;
                                segDensity = getSegCluser(rPos);
                            }

                        }
                    }
                }

                if (abs(dp) == 1) {
                    return float4(rPos.x, bPos.x, isoIndex, segDensity);
                }
                else if (abs(dp) == 2) {
                    return float4(rPos.y, bPos.y, isoIndex, segDensity);
                }
                else if (abs(dp) == 3) {
                    return float4(rPos.z, bPos.z, isoIndex, segDensity);
                }
                else {
                    return float4(0, 0, -1, 0);
                }
            }

            float4 B2F_Inverse(float dp, RayInfo ray, float zDepthIBX, float zDepthIBY, float Bindex, float noiseVal) {
#define MAX_NUM_STEPS 512
#define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

                RaymarchInfo raymarchInfo = initRaymarch(ray, MAX_NUM_STEPS);

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                ray.startPos += (2.0f * ray.direction * raymarchInfo.stepSize) * noiseVal;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float3 rPos, bPos;
                int clusterIndex = -1;
                float maxAcc = -1.0f;

                float3 maxPos = float3(-1.0f, -1.0f, -1.0f);
                float isoVal;
                float minBoundAcc = 0; // init
                int pIndex = -1;
                int isoIndex;
                float3 lastPos;
                float lastDensity;
                float segDensity;
                int lastIndex = -1;

                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);


                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);

                    pIndex = findClusterIndex(density);

                    float4 src = getTF1DColour(density);

                    if (density < _MinVal || density > _MaxVal)   continue;

                    if (abs(dp) == 1) {
                        if ((((currPos.x >= zDepthIBX)) || (currPos.x > zDepthIBY && pIndex == Bindex))) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 2) {
                        if ((((currPos.y >= zDepthIBX)) || (currPos.y > zDepthIBY && pIndex == Bindex))) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 3) {
                        if ((((currPos.z >= zDepthIBX)) || (currPos.z > zDepthIBY && pIndex == Bindex))) {
                            continue;
                        }
                    }

                    col.rgb = src.a * src.rgb + (1.0f - src.a) * col.rgb;
                    col.a = src.a + (1.0f - src.a) * col.a;

                    if (col.a >= 1.0f)
                        break;

                    if (clusterIndex == -1) {
                        maxAcc = max((col.a - minBoundAcc), maxAcc);
                        maxPos = currPos;
                        rPos = currPos;
                        bPos = currPos;
                        lastDensity = density;
                        lastPos = currPos;
                        lastIndex = pIndex;
                        clusterIndex = pIndex;
                        minBoundAcc = col.a;
                        isoIndex = pIndex;
                        segDensity = getSegCluser(lastPos);
                    }
                    else {
                        if (clusterIndex != pIndex) {

                            if ((col.a - minBoundAcc) > maxAcc) {
                                maxAcc = max((col.a - minBoundAcc), maxAcc);
                                minBoundAcc = col.a;
                                clusterIndex = pIndex;
                                rPos = lastPos;
                                bPos = currPos;
                                isoIndex = lastIndex;

                                lastPos = currPos;
                                lastIndex = pIndex;
                                segDensity = getSegCluser(rPos);
                            }
                        }
                    }
                }

                if (abs(dp) == 1 || abs(dp) == 4) {
                    return float4(rPos.x, bPos.x, isoIndex, segDensity);
                }
                else if (abs(dp) == 2 || abs(dp) == 5) {
                    return float4(rPos.y, bPos.y, isoIndex, segDensity);
                }
                else if (abs(dp) == 3 || abs(dp) == 6) {
                    return float4(rPos.z, bPos.z, isoIndex, segDensity);
                }
                else {
                    return float4(1.0f, 1.0f, -1.0f, 0.0f);
                }
            }

            float4 F2B(float dp, RayInfo ray, float zDepthFX, float zDepthFY, float Findex, float noiseVal) {
#define MAX_NUM_STEPS 512
#define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

                RaymarchInfo raymarchInfo = initRaymarch(ray, MAX_NUM_STEPS);

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                ray.startPos += (2.0f * ray.direction * raymarchInfo.stepSize) * noiseVal;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float3 rPos, bPos;
                int clusterIndex = -1;
                float maxAcc = -1.0f;

                float3 maxPos = float3(-1.0f, -1.0f, -1.0f);
                float isoVal;
                float minBoundAcc = 0; // init
                int pIndex = -1;
                int isoIndex;
                float3 lastPos;
                float lastDensity;
                float segDensity;
                int lastIndex = -1;

                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);

                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);


                    pIndex = findClusterIndex(density);

                    float4 src = getTF1DColour(density);
                    // Apply visibility window
                    if (density < _MinVal || density > _MaxVal) continue;

                    if (abs(dp) == 1 || abs(dp) == 4) {
                        if ((currPos.x <= zDepthFX) || (currPos.x < zDepthFY && pIndex == Findex)) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 2 || abs(dp) == 5) {
                        if ((currPos.y <= zDepthFX) || (currPos.y < zDepthFY && pIndex == Findex)) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 3 || abs(dp) == 6) {
                        if ((currPos.z <= zDepthFX) || (currPos.z < zDepthFY && pIndex == Findex)) {
                            continue;
                        }
                    }

                    if (density > _MinVal && density < _MaxVal)
                    {

                        src.rgb *= src.a;
                        col = (1.0f - col.a) * src + col;
                    }

                    if (col.a >= 1.0f)
                        break;

                    //// Early ray termination
                    //if (col.a > OPACITY_THRESHOLD) {
                    //    break;
                    //}

                    if (clusterIndex == -1) {
                        maxAcc = max((col.a - minBoundAcc), maxAcc);
                        maxPos = currPos;
                        rPos = currPos;
                        bPos = currPos;
                        lastDensity = density;
                        lastPos = currPos;
                        lastIndex = pIndex;
                        clusterIndex = pIndex;
                        minBoundAcc = col.a;
                        isoIndex = pIndex;
                        segDensity = getSegCluser(lastPos);
                        // buffer[0] = float4(clusterIndex, col.a, minBoundAcc, maxAcc);
                    }
                    else {
                        if (clusterIndex != pIndex) {

                            if ((col.a - minBoundAcc) > maxAcc) {
                                maxAcc = max((col.a - minBoundAcc), maxAcc);
                                minBoundAcc = col.a;
                                clusterIndex = pIndex;
                                rPos = lastPos;
                                bPos = currPos;
                                isoIndex = lastIndex;

                                lastPos = currPos;
                                lastIndex = pIndex;
                                segDensity = getSegCluser(rPos);
                                //buffer[0] = float4(clusterIndex, col.a, minBoundAcc, maxAcc);
                            }
                        }
                    }
                }

                if (abs(dp) == 1) {
                    return float4(rPos.x, bPos.x, isoIndex, segDensity);
                }
                else if (abs(dp) == 2) {
                    return float4(rPos.y, bPos.y, isoIndex, segDensity);
                }
                else if (abs(dp) == 3) {
                    return float4(rPos.z, bPos.z, isoIndex, segDensity);
                }
                else {
                    return float4(0, 0, -1, 0);
                }
            }

            float4 F2B_Inverse(float dp, RayInfo ray, float zDepthIBX, float zDepthIBY, float Bindex, float noiseVal) {
#define MAX_NUM_STEPS 512
#define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

                RaymarchInfo raymarchInfo = initRaymarch(ray, MAX_NUM_STEPS);

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                ray.startPos += (2.0f * ray.direction * raymarchInfo.stepSize) * noiseVal;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float3 rPos, bPos;
                int clusterIndex = -1;
                float maxAcc = -1.0f;

                float3 maxPos = float3(-1.0f, -1.0f, -1.0f);
                float isoVal;
                float minBoundAcc = 0; // init
                int pIndex = -1;
                int isoIndex;
                float3 lastPos;
                float lastDensity;
                float segDensity;
                int lastIndex = -1;

                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);

                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);

                    pIndex = findClusterIndex(density);

                    float4 src = getTF1DColour(density);
                    // Apply visibility window
                    if (density < _MinVal || density > _MaxVal) continue;

                    if (abs(dp) == 1 || abs(dp) == 4) {
                        if ((((currPos.x >= zDepthIBX)) || (currPos.x > zDepthIBY && pIndex == Bindex))) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 2 || abs(dp) == 5) {
                        if ((((currPos.y >= zDepthIBX)) || (currPos.y > zDepthIBY && pIndex == Bindex))) {
                            continue;
                        }
                    }
                    else if (abs(dp) == 3 || abs(dp) == 6) {
                        if ((((currPos.z >= zDepthIBX)) || (currPos.z > zDepthIBY && pIndex == Bindex))) {
                            continue;
                        }
                    }

                    if (density > _MinVal && density < _MaxVal)
                    {
                        src.rgb *= src.a;
                        col = (1.0f - col.a) * src + col;
                    }

                    if (col.a >= 1.0f)
                        break;

                    //// Early ray termination
                    //if (col.a > OPACITY_THRESHOLD) {
                    //    break;
                    //}

                    if (clusterIndex == -1) {
                        maxAcc = max((col.a - minBoundAcc), maxAcc);
                        maxPos = currPos;
                        rPos = currPos;
                        bPos = currPos;
                        lastDensity = density;
                        lastPos = currPos;
                        lastIndex = pIndex;
                        clusterIndex = pIndex;
                        minBoundAcc = col.a;
                        isoIndex = pIndex;
                        segDensity = getSegCluser(lastPos);
                    }
                    else {
                        if (clusterIndex != pIndex) {

                            if ((col.a - minBoundAcc) > maxAcc) {
                                maxAcc = max((col.a - minBoundAcc), maxAcc);
                                minBoundAcc = col.a;
                                clusterIndex = pIndex;
                                rPos = lastPos;
                                bPos = currPos;
                                isoIndex = lastIndex;

                                lastPos = currPos;
                                lastIndex = pIndex;
                                segDensity = getSegCluser(rPos);
                            }
                        }
                    }
                }

                if (abs(dp) == 1) {
                    return float4(rPos.x, bPos.x, isoIndex, segDensity);
                }
                else if (abs(dp) == 2) {
                    return float4(rPos.y, bPos.y, isoIndex, segDensity);
                }
                else if (abs(dp) == 3) {
                    return float4(rPos.z, bPos.z, isoIndex, segDensity);
                }
                else {
                    return float4(1.0f, 1.0f, -1.0f, 0.0f);
                }
            }

            float4 AFF2B(RayInfo ray, float3 r1, float3 r2, float noiseVal) {
#define MAX_NUM_STEPS 512
#define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

                RaymarchInfo raymarchInfo = initRaymarch(ray, MAX_NUM_STEPS);

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                ray.startPos += (2.0f * ray.direction * raymarchInfo.stepSize) * noiseVal;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float3 rPos, bPos;
                int clusterIndex = -1;
                float maxAcc = -1.0f;

                float3 maxPos = float3(-1.0f, -1.0f, -1.0f);
                float isoVal;
                float minBoundAcc = 0; // init
                int pIndex = -1;
                int isoIndex;
                float3 lastPos;
                float lastDensity;
                float segDensity;
                int lastIndex = -1;

                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);

                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);
                    if (density < _MinVal || density > _MaxVal) continue;
                    pIndex = findClusterIndex(density);

                    float4 src = getTF1DColour(density);

                    bool inside = false;

                    for (int j = 0; j < _WidgetNums - 1; j++) {
                        if (!(sdCylinder(currPos, r1, r2, _CircleSize[j], _RotateMatrix[j]))) {
                            float2 visIsoRange = findIsoRange((int)buffer[j].z);
                            if ((density >= visIsoRange.x && density <= visIsoRange.y)) {
                                const float segDensity1 = getSegCluser(currPos);
                                if (round(segDensity1) == buffer[j].w) {
                                    inside = true;
                                }
                            }
                        }
                    }

                    if (inside) continue;

                    // Apply visibility window
                    if (density < _MinVal || density > _MaxVal) continue;

                    if (density > _MinVal && density < _MaxVal)
                    {
                        src.rgb *= src.a;
                        col = (1.0f - col.a) * src + col;
                    }

                    if (col.a >= 1.0f)
                        break;


                    if (clusterIndex == -1) {
                        maxAcc = max((col.a - minBoundAcc), maxAcc);
                        maxPos = currPos;
                        rPos = currPos;
                        bPos = currPos;
                        lastDensity = density;
                        lastPos = currPos;
                        lastIndex = pIndex;
                        clusterIndex = pIndex;
                        minBoundAcc = col.a;
                        isoIndex = pIndex;
                        segDensity = getSegCluser(lastPos);
                    }
                    else {
                        if (clusterIndex != pIndex) {

                            if ((col.a - minBoundAcc) > maxAcc) {
                                maxAcc = max((col.a - minBoundAcc), maxAcc);
                                minBoundAcc = col.a;
                                clusterIndex = pIndex;
                                rPos = lastPos;
                                bPos = currPos;
                                isoIndex = lastIndex;

                                lastPos = currPos;
                                lastIndex = pIndex;
                                segDensity = getSegCluser(rPos);
                            }
                        }
                    }
                }

                return float4(rPos.x, rPos.y, isoIndex, rPos.z);
            }


            // Direct Volume Rendering
            frag_out frag_dvr(frag_in i)
            {
                #define MAX_NUM_STEPS 512
                #define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

#ifdef DVR_BACKWARD_ON
                RayInfo ray = getRayBack2Front(i.vertexLocal, 0);
#else
                RayInfo ray = getRayFront2Back(i.vertexLocal, 0);
#endif
                RaymarchInfo raymarchInfo = initRaymarch(ray, MAX_NUM_STEPS);

                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

                // Create a small random offset in order to remove artifacts
                float noiseVal = tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;
                ray.startPos += (2.0f * ray.direction * raymarchInfo.stepSize) * noiseVal;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
#ifdef DVR_BACKWARD_ON
                float tDepth = 0.0f;
#else
                float tDepth = raymarchInfo.numStepsRecip * (raymarchInfo.numSteps - 1);
#endif
                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const float t = iStep * raymarchInfo.numStepsRecip;
                    const float3 currPos = lerp(ray.startPos, ray.endPos, t);

#if MOUSEDOWN_ON
#if DiggingWidget
                    float3 minDep = float3(0.0f, 0.0f, -2.0f), minDepY = float3(0.0f, 0.0f, -2.0f), minDepZ = float3(0.0f, 0.0f, -2.0f);
                    float3 maxDep = float3(1.0f, 1.0f, -2.0f), maxDepY = float3(1.0f, 1.0f, -2.0f), maxDepZ = float3(1.0f, 1.0f, -2.0f);
                    int pIndex = -1;
                    float zDepth = 0.0f;
                    float zDepthInverse = 1.0f;
                    float zDepthFX = 0.0f, zDepthFY = 0.0f;
                    float Findex = -1.0f;
                    float zDepthBX = 1.0f, zDepthBY = 1.0f;
                    float Bindex = -1.0f;

                    pIndex = findClusterIndex(getDensity(currPos));
                    float dp = 0.0f;
                    for (uint jStep = 0; jStep < _WidgetNums; jStep++) {

                        float3 r1 = float3(_WidgetPos[jStep].x, _WidgetPos[jStep].y, _WidgetPos[jStep].z);// -0.215 // _WidgetPos[jStep].z
                        float3 r2 = float3(_WidgetPos[jStep].x, _WidgetPos[jStep].y, _WidgetPos[jStep].z + (-(_WidgetPos[jStep].z)) * 2); // 0.215 //         

                        bool insideLens = false;
                        if (_LensIndexs[jStep] == 0.1f) { // Cylinder
                            insideLens = sdCylinder(currPos, r1, r2, _CircleSize[jStep], _RotateMatrix[jStep]);
                        }
                        else if (_LensIndexs[jStep] == 0.2f) { // Box
                            insideLens = sdBox(currPos, float3((_CircleSize[jStep] - 0.01f) / 2, (_CircleSize[jStep] - 0.01f) / 2, 0.5f), _RotateMatrix[jStep], float3(_WidgetPos[jStep].x, _WidgetPos[jStep].y, 0.0f), ((_CircleSize[jStep] - 0.01f) / 2));
                        }
                        else if (_LensIndexs[jStep] == 0.3f) { // Vesica
                            insideLens = sdVesica(currPos, float3(_WidgetPos[jStep].x, _WidgetPos[jStep].y, 0.0f), 0.5f, r2.z - r1.z, _CircleSize[jStep], _RotateMatrix[jStep]);
                        }

                        if (insideLens) {

                            float4 visData = float4(0.0f, 0.0f, -2.0f, 0.0f);
                            float4 visDataInverse = float4(1.0f, 1.0f, -2.0f, 0.0f);
                            if (_depthNP > 0) {
                                if (_WidgetRecorder[jStep].z > 0) {

                                    for (uint dStep = 0; dStep < _WidgetRecorder[jStep].z; dStep++) {
                                        visData = F2B(_localDepth, getRayFront2Back(i.vertexLocal, 0), zDepthFX, zDepthFY, Findex, noiseVal);
                                        if (_WidgetPos[jStep].w == 1) {
                                            minDep = visData;
                                        }
                                        else if (_WidgetPos[jStep].w == 2) {
                                            minDepY = visData;
                                        }
                                        else if (_WidgetPos[jStep].w == 3) {
                                            minDepZ = visData;
                                        }
                                        zDepthFX = visData.x;
                                        zDepthFY = visData.y;
                                        Findex = visData.z;
                                    }
                                }

                                if (_WidgetRecorder[jStep].w > 0) {

                                    for (uint dStep = 0; dStep < _WidgetRecorder[jStep].w; dStep++) {
                                        visDataInverse = B2F_Inverse(_localDepth, getRayBack2Front(i.vertexLocal, 0), zDepthBX, zDepthBY, Bindex, noiseVal);
                                        if (_WidgetPos[jStep].w == 1) {
                                            maxDep = visDataInverse;
                                        }
                                        else if (_WidgetPos[jStep].w == 2) {
                                            maxDepY = visDataInverse;
                                        }
                                        else if (_WidgetPos[jStep].w == 3) {
                                            maxDepZ = visDataInverse;
                                        }
                                        zDepthBX = visDataInverse.x;
                                        zDepthBY = visDataInverse.y;
                                        Bindex = visDataInverse.z;
                                    }
                                }
                            }
                            else {
                                if (_WidgetRecorder[jStep].z > 0) {

                                    for (uint dStep = 0; dStep < _WidgetRecorder[jStep].z; dStep++) {
                                        visData = B2F(_localDepth, getRayBack2Front(i.vertexLocal, 0), zDepthFX, zDepthFY, Findex, noiseVal);
                                        if (_WidgetPos[jStep].w == 1) {
                                            minDep = visData;
                                        }
                                        else if (_WidgetPos[jStep].w == 2) {
                                            minDepY = visData;
                                        }
                                        else if (_WidgetPos[jStep].w == 3) {
                                            minDepZ = visData;
                                        }
                                        zDepthFX = visData.x;
                                        zDepthFY = visData.y;
                                        Findex = visData.z;
                                    }
                                }

                                if (_WidgetRecorder[jStep].w > 0) {

                                    for (uint dStep = 0; dStep < _WidgetRecorder[jStep].w; dStep++) {
                                        visDataInverse = F2B_Inverse(_localDepth, getRayFront2Back(i.vertexLocal, 0), zDepthBX, zDepthBY, Bindex, noiseVal);
                                        if (_WidgetPos[jStep].w == 1) {
                                            maxDep = visDataInverse;
                                        }
                                        else if (_WidgetPos[jStep].w == 2) {
                                            maxDepY = visDataInverse;
                                        }
                                        else if (_WidgetPos[jStep].w == 3) {
                                            maxDepZ = visDataInverse;
                                        }
                                        zDepthBX = visDataInverse.x;
                                        zDepthBY = visDataInverse.y;
                                        Bindex = visDataInverse.z;
                                    }
                                }
                            }
                        }
                    }

                    if ((currPos.x <= minDep.x) || (currPos.x < 1.0f && pIndex == minDep.z)) {
                        continue;
                    }
                    if ((((currPos.x >= maxDep.x)) || (currPos.x > 0.0f && pIndex == maxDep.z))) {
                        continue;
                    }

                    if ((currPos.y <= minDepY.x) || (currPos.y < 1.0f && pIndex == minDepY.z)) {
                        continue;
                    }
                    if ((((currPos.y >= maxDepY.x)) || (currPos.y > 0.0f && pIndex == maxDepY.z))) {
                        continue;
                    }

                    if ((currPos.z <= minDepZ.x) || (currPos.z < 1.0f && pIndex == minDepZ.z)) {
                        continue;
                    }
                    if ((((currPos.z >= maxDepZ.x)) || (currPos.z > 0.0f && pIndex == maxDepZ.z))) {
                        continue;
                    }

#elif ErasingWidget
                    bool isInside = false;
                    float sameIsoSeg = -1.0f;

                    if (_WidgetNums == 0) {
                        for (int s = 0; s < 10; s++) {
                            buffer[s] = float4(0, 0, 0, 0);
                            buffer_Mask[s] = float4(0, 0, 0, 0);
                        }
                    }

                    for (uint jStep = 0; jStep < _WidgetNums; jStep++) {

                        float3 r1 = float3(_WidgetPos[jStep].x, _WidgetPos[jStep].y, _WidgetPos[jStep].z);
                        float3 r2 = float3(_WidgetPos[jStep].x, _WidgetPos[jStep].y, _WidgetPos[jStep].z + (-(_WidgetPos[jStep].z)) * 2);
                        float2 selectCindex;

                        if (buffer[jStep].x == 0) {

                            if ((sdCylinder(currPos, r1, r2, 0.0005f, _RotateMatrix[jStep]))) {
                                float4 visData = float4(0.0f, 0.0f, -2.0f, 0.0f);
                                float4 visDataInverse = float4(1.0f, 1.0f, -2.0f, 0.0f);

                                if (_WidgetRecorder[jStep].z > 0) {
                                    for (uint dStep = 0; dStep < _WidgetRecorder[jStep].z; dStep++) {
                                        visData = AFF2B(getRayFront2Back(i.vertexLocal, 0), r1, r2, noiseVal);
                                        selectCindex = float2(visData.z, round(getSegCluser(float3(visData.x, visData.y, visData.w))));
                                    }
                                }
                                if (_WidgetRecorder[jStep].w > 0) {
                                    for (uint dStep = 0; dStep < _WidgetRecorder[jStep].w; dStep++) {
                                        visDataInverse = AFF2B(getRayFront2Back(i.vertexLocal, 0), r1, r2, noiseVal);
                                        selectCindex = float2(visDataInverse.z, round(getSegCluser(float3(visDataInverse.x, visDataInverse.y, visDataInverse.w))));
                                    }
                                }

                                if (jStep == _WidgetNums - 1) {
                                    if (buffer[jStep].x == 0) {
                                        buffer[jStep] = max(buffer[jStep], float4(1, 0, selectCindex.x, selectCindex.y));
                                        buffer_Mask[jStep] = buffer[jStep];
                                    }
                                }
                            }

                        }
                        const float density1 = getDensity(currPos);
                        if (!(sdCylinder(currPos, r1, r2, _CircleSize[jStep], _RotateMatrix[jStep]))) {

                            float2 visIsoRange = findIsoRange((int)buffer[jStep].z);
                            if ((density1 >= visIsoRange.x && density1 <= visIsoRange.y)) {
                                const float segDensity = getSegCluser(currPos);
                                if (round(segDensity) == buffer[jStep].w) {
                                    isInside = true;
                                }
                            }
                        }
                    }

                    if (isInside) continue;
#endif
#else

#endif

                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);

                    // Apply visibility window
                    if (density < _MinVal || density > _MaxVal) continue;

                    // Calculate gradient (needed for lighting and 2D transfer functions)
#if defined(TF2D_ON) || defined(LIGHTING_ON)
                    float3 gradient = getGradient(currPos);
#endif

                    // Apply transfer function
#if TF2D_ON
                    float mag = length(gradient) / 1.75f;
                    float4 src = getTF2DColour(density, mag);
#else
                    float4 src = getTF1DColour(density);
#endif

                    // Apply lighting
#if defined(LIGHTING_ON) && defined(DVR_BACKWARD_ON)
                    src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, ray.direction, 0.3f);
#elif defined(LIGHTING_ON)
                    src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, -ray.direction, 0.3f);
#endif

#ifdef DVR_BACKWARD_ON
                    col.rgb = src.a * src.rgb + (1.0f - src.a) * col.rgb;
                    col.a = src.a + (1.0f - src.a) * col.a;

                    // Optimisation: A branchless version of: if (src.a > 0.15f) tDepth = t;
                    tDepth = max(tDepth, t * step(0.15, src.a));
#else
                    src.rgb *= src.a;
                    col = (1.0f - col.a) * src + col;

                    if (col.a > 0.15 && t < tDepth) {
                        tDepth = t;
                    }
#endif

                    // Early ray termination
#if !defined(DVR_BACKWARD_ON) && defined(RAY_TERMINATE_ON)
                    if (col.a > OPACITY_THRESHOLD) {
                        break;
                    }
#endif
                }

                // Write fragment output
                frag_out output;
                output.colour = col;
#if DEPTHWRITE_ON
                tDepth += (step(col.a, 0.0) * 1000.0); // Write large depth if no hit
                const float3 depthPos = lerp(ray.startPos, ray.endPos, tDepth) - float3(0.5f, 0.5f, 0.5f);
                output.depth = localToDepth(depthPos);
#endif
                return output;
            }

            frag_in vert(vert_in v)
            {
                return vert_main(v);
            }

            frag_out frag(frag_in i)
            {
                return frag_dvr(i);
            }

            ENDCG
        }
    }
}
