// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

float opExtrussion(float3 p, float sdf, float h)
{
    float2 w = float2(sdf, abs(p.z) - h);
    return min(max(w.x, w.y), 0.0) + length(max(w, 0.0));
}

bool sdVesica(float3 currPos, float3 s, float r, float dp, float d, float4x4 RotateMatrix)
{
    r = r - d;
    d = d + d;
    float3 pos = currPos - float3(0.5f, 0.5f, 0.5f);
    float3 planeSpacePos = mul(RotateMatrix, float4(pos, 1.0f)).xyz;
    planeSpacePos = planeSpacePos - s;
    planeSpacePos = abs(planeSpacePos);

    float b = sqrt(r * r - d * d); // can delay this sqrt
    float results = ((planeSpacePos.y - b) * d > planeSpacePos.x * b)
            ? length(planeSpacePos - float2(0.0, b))
            : length(planeSpacePos - float2(-d, 0.0)) - r;

    return opExtrussion(currPos, results, dp) < 0.0f;
}

bool sdBox(float3 currPos, float3 b, float4x4 RotateMatrix, float3 s, float r)
{
    float3 pos = currPos - float3(0.5f, 0.5f, 0.5f);
    float3 p = mul(RotateMatrix, float4(pos, 1.0f)).xyz;
    p = p - s;
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) < r;
}

bool sdCylinder(float3 currPos, float3 a, float3 b, float r, float4x4 RotateMatrix)
{   
    float3 pos = currPos - float3(0.5f, 0.5f, 0.5f);    

    float3 planeSpacePos = mul(RotateMatrix, float4(pos, 1.0f)).xyz;

    float3  ba = b - a;
    float3 pa = planeSpacePos - a;
    float baba = dot(ba, ba);
    float paba = dot(pa, ba);
    
    float t = paba / baba;
    float3 c = a + t * ba;    
    
    float x = length(pa * baba - ba * paba) - r * baba;
    float y = abs(paba - baba * 0.5) - baba * 0.5;
    float x2 = x * x;
    float y2 = y * y * baba;

    float d = (max(x, y) < 0.0) ? -min(x2, y2) : (((x > 0.0) ? x2 : 0.0) + ((y > 0.0) ? y2 : 0.0));

    return (sign(d) * sqrt(abs(d)) / baba) <= 0.0f;
}
