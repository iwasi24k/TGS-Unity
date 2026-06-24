using UnityEngine;

[System.Serializable]
public class BossCameraProfile
{
    public string name = "Profile";
    public float yawOffset = 0f;             // 旋回角オフセット
    public float distanceMul = 1f;           // ベース距離への倍率
    public float heightOffset = 0f;          // 高さ加算
    public float fovOffset = 0f;             // FOV加算
    [Range(-1f, 1f)] public float focusWeight = -1f; // 負ならベース値を使用
    public float blendTime = 0.4f;           // 遷移所要
}

[DisallowMultipleComponent]
public class BossArenaCamera : MonoBehaviour
{
    [Header("対象")]
    [SerializeField] Transform boss;
    [SerializeField] Transform player;
    [SerializeField] Camera targetCamera;

    [Header("ベース構図")]
    [SerializeField, Range(0f, 1f)] float focusWeight = 0.3f;
    [SerializeField] float baseDistance = 9f;
    [SerializeField] float separationFactor = 0.55f;
    [SerializeField] float baseHeight = 4.5f;
    [SerializeField] float lookHeight = 1.4f;
    [SerializeField] float baseFov = 50f;

    [Header("減衰(秒)")]
    [SerializeField] float yawDamp = 0.35f;
    [SerializeField] float distanceDamp = 0.5f;
    [SerializeField] float heightDamp = 0.4f;
    [SerializeField] float fovDamp = 0.2f;
    [SerializeField] float returnBlend = 0.4f;

    [Header("障害物回避")]
    [SerializeField] bool avoidObstacles = false;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float cameraRadius = 0.3f;
    [SerializeField] float minDistance = 2.5f;

    [Header("演出")]
    [SerializeField] float shakeMax = 0.6f;
    [SerializeField] float traumaDecay = 1.4f;

    Camera cam;
    Transform camTr;
    BossCameraProfile profile;

    // 平滑値
    float yaw, yawVel, dist, distVel, height, heightVel, fov, fovVel;
    float bYaw, bDistMul = 1f, bHeight, bFov, bFocus;
    Vector3 lastDir = Vector3.forward;
    float trauma, fovPunch, fovPunchVel;

    void Awake()
    {
        cam = targetCamera ? targetCamera : (TryGetComponent(out Camera c) ? c : Camera.main);
        if (!cam) { Debug.LogWarning("BossArenaCamera: カメラを解決できません"); return; }
        camTr = cam.transform;

        dist = baseDistance;
        height = baseHeight;
        fov = baseFov;
        bFocus = focusWeight;
        if (boss && player) yaw = YawTo(player.position - boss.position);
    }

    void LateUpdate()
    {
        if (!cam || !boss || !player) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        BlendProfile(dt);

        Vector3 bossPos = boss.position;
        Vector3 playerPos = player.position;
        Vector3 focusPoint = Vector3.Lerp(bossPos, playerPos, Mathf.Clamp01(bFocus));

        // 旋回
        yaw = Mathf.SmoothDampAngle(yaw, YawTo(playerPos - bossPos) + bYaw, ref yawVel, yawDamp, Mathf.Infinity, dt);

        // 距離
        float sep = Vector2.Distance(new Vector2(bossPos.x, bossPos.z), new Vector2(playerPos.x, playerPos.z));
        float targetDist = (baseDistance + sep * separationFactor) * Mathf.Max(0.1f, bDistMul);
        dist = Mathf.SmoothDamp(dist, targetDist, ref distVel, distanceDamp, Mathf.Infinity, dt);

        height = Mathf.SmoothDamp(height, baseHeight + bHeight, ref heightVel, heightDamp, Mathf.Infinity, dt);

        Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        Vector3 pos = focusPoint + dir * dist + Vector3.up * height;

        Vector3 lookAt = focusPoint + Vector3.up * lookHeight;
        if (avoidObstacles) pos = ResolveObstacle(lookAt, pos);

        if (trauma > 0f)
        {
            pos += Random.insideUnitSphere * (trauma * trauma * shakeMax);
            trauma = Mathf.Max(0f, trauma - traumaDecay * dt);
        }

        Vector3 toLook = lookAt - pos;
        Quaternion rot = toLook.sqrMagnitude > 1e-5f ? Quaternion.LookRotation(toLook, Vector3.up) : camTr.rotation;
        camTr.SetPositionAndRotation(pos, rot);

        fovPunch = Mathf.SmoothDamp(fovPunch, 0f, ref fovPunchVel, 0.25f, Mathf.Infinity, dt);
        fov = Mathf.SmoothDamp(fov, baseFov + bFov + fovPunch, ref fovVel, fovDamp, Mathf.Infinity, dt);
        cam.fieldOfView = fov;
    }

    // 公開API
    public void SetProfile(BossCameraProfile p) => profile = p;
    public void ClearProfile() => profile = null;
    public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);
    public void PunchFov(float amount) => fovPunch += amount;

    // 内部
    void BlendProfile(float dt)
    {
        float tYaw = 0f, tDistMul = 1f, tHeight = 0f, tFov = 0f, tFocus = focusWeight, blend = returnBlend;
        if (profile != null)
        {
            tYaw = profile.yawOffset;
            tDistMul = profile.distanceMul;
            tHeight = profile.heightOffset;
            tFov = profile.fovOffset;
            tFocus = profile.focusWeight < 0f ? focusWeight : profile.focusWeight;
            blend = Mathf.Max(0.0001f, profile.blendTime);
        }
        float k = 1f - Mathf.Exp(-dt / blend); // フレームレート非依存
        bYaw = Mathf.Lerp(bYaw, tYaw, k);
        bDistMul = Mathf.Lerp(bDistMul, tDistMul, k);
        bHeight = Mathf.Lerp(bHeight, tHeight, k);
        bFov = Mathf.Lerp(bFov, tFov, k);
        bFocus = Mathf.Lerp(bFocus, tFocus, k);
    }

    float YawTo(Vector3 flat)
    {
        flat.y = 0f;
        if (flat.sqrMagnitude < 1e-4f) flat = lastDir;
        else { flat.Normalize(); lastDir = flat; }
        return Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
    }

    Vector3 ResolveObstacle(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float d = dir.magnitude;
        if (d < 0.001f) return to;
        dir /= d;
        return Physics.SphereCast(from, cameraRadius, dir, out var hit, d, obstacleMask, QueryTriggerInteraction.Ignore)
            ? from + dir * Mathf.Max(minDistance, hit.distance - 0.05f)
            : to;
    }
}