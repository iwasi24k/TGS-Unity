using UnityEngine;

[System.Serializable]
public class BossCameraProfile
{
    [Tooltip("演出名（識別用。動作には影響しない）")]
    public string name = "Profile";

    [Tooltip("旋回角オフセット（度）。0で真後ろ、+で右回り込み、-で左回り込み")]
    public float yawOffset = 0f;

    [Tooltip("カメラ距離の倍率。1=通常 / 1.3=引き / 0.7=寄り。0以下は「未設定=1扱い」")]
    public float distanceMul = 1f;

    [Tooltip("カメラ高さへの加算(m)。+で見下ろし気味、-で低いアングル")]
    public float heightOffset = 0f;

    [Tooltip("FOVへの加算(度)。+で広角（引き・スピード感）、-で望遠（圧縮感）")]
    public float fovOffset = 0f;

    [Tooltip("注視点の比率。0=ボスを注視 / 1=プレイヤーを注視 / -1=ベース設定を使用（推奨）")]
    [Range(-1f, 1f)] public float focusWeight = -1f;

    [Tooltip("この構図へ遷移する所要時間(秒)。0以下は「未設定=Return Blend扱い」")]
    public float blendTime = 0.4f;
}

[DisallowMultipleComponent]
public class BossArenaCamera : MonoBehaviour
{
    [Header("対象")]
    [Tooltip("ボスのTransform。空欄/プレハブ参照でもOK（実行時にTag=Bossを自動取得）")]
    [SerializeField] Transform boss;
    [Tooltip("プレイヤーのTransform。空欄でもOK（実行時にTag=Playerを自動取得）")]
    [SerializeField] Transform player;
    [Tooltip("制御するカメラ。空欄なら同じGameObjectのCameraを使用")]
    [SerializeField] Camera targetCamera;

    [Header("ベース構図")]
    [Tooltip("注視点の比率。0=ボス寄り / 1=プレイヤー寄り。0.3〜0.4だと両者の中間やや ボス寄りを見る")]
    [SerializeField, Range(0f, 1f)] float focusWeight = 0.35f;
    [Tooltip("カメラの基準距離(m)。二人が密着している時の距離")]
    [SerializeField] float baseDistance = 9f;
    [Tooltip("ボスとプレイヤーが離れるほどカメラを引く係数。距離 = 基準距離 + 二者間距離×この値")]
    [SerializeField] float separationFactor = 0.55f;
    [Tooltip("カメラの高さ(m)")]
    [SerializeField] float baseHeight = 4.5f;
    [Tooltip("注視点の高さ(m)。キャラの胸〜頭あたり(1.4〜1.6)が自然")]
    [SerializeField] float lookHeight = 1.4f;
    [Tooltip("基準FOV(度)")]
    [SerializeField] float baseFov = 50f;

    [Header("追従の滑らかさ(秒) 小さいほど機敏")]
    [Tooltip("旋回(左右の回り込み)の追従時間。大きいとプレイヤーの横移動に置いていかれる")]
    [SerializeField] float yawDamp = 0.25f;
    [Tooltip("距離変化の追従時間")]
    [SerializeField] float distanceDamp = 0.5f;
    [Tooltip("高さ変化の追従時間")]
    [SerializeField] float heightDamp = 0.4f;
    [Tooltip("FOV変化の追従時間")]
    [SerializeField] float fovDamp = 0.2f;
    [Tooltip("演出プロファイル解除後、通常構図に戻る所要時間(秒)")]
    [SerializeField] float returnBlend = 0.4f;

    [Header("プレイヤー画面内保持")]
    [Tooltip("プレイヤーが画面から外れそうな時、注視点を自動補正して画面内に収める")]
    [SerializeField] bool keepPlayerInFrame = true;
    [Tooltip("画角の何割を「安全域」とするか。0.75なら画面端25%に入った時点で補正開始")]
    [SerializeField, Range(0.5f, 0.95f)] float frameSafeRatio = 0.75f;

    [Header("演出（Directorの着弾揺れ用）")]
    [Tooltip("シェイクの最大振れ幅(m)。AddTraumaで加算された揺れの上限")]
    [SerializeField] float shakeMax = 0.6f;
    [Tooltip("シェイクの減衰速度。大きいほど早く収まる")]
    [SerializeField] float traumaDecay = 1.4f;

    [Header("ロックオン連携")]
    [Tooltip("プレイヤーのSC_PlayerTarget（空欄なら自動取得）")]
    [SerializeField] SC_PlayerTarget scTarget;
    [Tooltip("ロックオン時のカメラオフセット（SC_PlayerCameraのTargetCameraOffset相当）")]
    [SerializeField] Vector3 lockOnOffset = new Vector3(0f, 2f, -3f);
    [Tooltip("ロックオン時の注視点の高さ(m)")]
    [SerializeField] float lockOnLookHeight = 1.5f;
    [Tooltip("ロックオン⇔通常構図の切替速度")]
    [SerializeField] float lockOnBlendSpeed = 5f;
    [Tooltip("ロックオン時のFOV加算(度)")]
    [SerializeField] float lockOnFovOffset = 5f;

    Camera cam;
    Transform camTr;
    BossCameraProfile profile;

    // 平滑値
    float yaw, yawVel, dist, distVel, height, heightVel, fov, fovVel;
    float bYaw, bDistMul = 1f, bHeight, bFov, bFocus;
    Vector3 lastDir = Vector3.forward;
    float trauma, fovPunch, fovPunchVel;
    float lockBlend;                 // 0=通常構図 1=ロックオン構図
    float framePull, framePullVel;   // プレイヤー画面内保持の補正量

    // 演出プロファイル適用中か（ターゲット可否の判定に使用）
    public bool IsDirecting => profile != null;

    void Awake()
    {
        cam = targetCamera ? targetCamera : (TryGetComponent(out Camera c) ? c : Camera.main);
        if (!cam) { Debug.LogWarning("BossArenaCamera: カメラを解決できません"); return; }
        camTr = cam.transform;

        dist = baseDistance;
        height = baseHeight;
        fov = baseFov;
        bFocus = focusWeight;
    }

    void OnEnable()
    {
        // ボス戦開始時（GameObjectが有効化された時）に初期方位をリセット
        ResolveReferences();
        if (IsSceneInstance(boss) && IsSceneInstance(player))
            yaw = YawTo(player.position - boss.position);
        lockBlend = 0f;
        framePull = 0f;
    }

    void LateUpdate()
    {
        // プレハブ参照や破棄済み参照なら、シーン上の実体を探し直す
        if (!IsSceneInstance(boss) || !IsSceneInstance(player) || scTarget == null)
            ResolveReferences();

        if (!cam || !IsSceneInstance(boss) || !IsSceneInstance(player)) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        BlendProfile(dt);

        Vector3 bossPos = boss.position;
        Vector3 playerPos = player.position;
        Vector3 focusPoint = Vector3.Lerp(bossPos, playerPos, Mathf.Clamp01(bFocus));

        // 旋回：ボス→プレイヤー方向の背後に回り込む
        yaw = Mathf.SmoothDampAngle(yaw, YawTo(playerPos - bossPos) + bYaw, ref yawVel, yawDamp, Mathf.Infinity, dt);

        // 距離：離れるほど引く
        float sep = Vector2.Distance(new Vector2(bossPos.x, bossPos.z), new Vector2(playerPos.x, playerPos.z));
        float targetDist = (baseDistance + sep * separationFactor) * Mathf.Max(0.1f, bDistMul);
        dist = Mathf.SmoothDamp(dist, targetDist, ref distVel, distanceDamp, Mathf.Infinity, dt);

        height = Mathf.SmoothDamp(height, baseHeight + bHeight, ref heightVel, heightDamp, Mathf.Infinity, dt);

        Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        Vector3 pos = focusPoint + dir * dist + Vector3.up * height;
        Vector3 lookAt = focusPoint + Vector3.up * lookHeight;

        //----------------------------------------------------------
        // ロックオン構図（演出プロファイル中は無効）
        //----------------------------------------------------------
        GameObject lockTarget = (!IsDirecting && scTarget != null) ? scTarget.GetCurrentTarget() : null;
        lockBlend = Mathf.MoveTowards(lockBlend, lockTarget != null ? 1f : 0f, dt * lockOnBlendSpeed);

        if (lockBlend > 0f && lockTarget != null)
        {
            Vector3 lockPos = CalcLockOnCameraPos(playerPos, lockTarget.transform.position);
            Vector3 lockLook = (playerPos + lockTarget.transform.position) * 0.5f
                               + Vector3.up * lockOnLookHeight;

            pos = Vector3.Lerp(pos, lockPos, lockBlend);
            lookAt = Vector3.Lerp(lookAt, lockLook, lockBlend);
        }

        //----------------------------------------------------------
        // プレイヤー画面内保持：画面端に近づいたら注視点をプレイヤー側へ寄せる
        //----------------------------------------------------------
        if (keepPlayerInFrame)
        {
            Vector3 playerHead = playerPos + Vector3.up * lookHeight;
            float targetPull = ComputeFramePull(pos, lookAt, playerHead);
            framePull = Mathf.SmoothDamp(framePull, targetPull, ref framePullVel, 0.15f, Mathf.Infinity, dt);
            if (framePull > 0.001f)
                lookAt = Vector3.Lerp(lookAt, playerHead, framePull);
        }

        if (trauma > 0f)
        {
            pos += Random.insideUnitSphere * (trauma * trauma * shakeMax);
            trauma = Mathf.Max(0f, trauma - traumaDecay * dt);
        }

        Vector3 toLook = lookAt - pos;
        Quaternion rot = toLook.sqrMagnitude > 1e-5f ? Quaternion.LookRotation(toLook, Vector3.up) : camTr.rotation;
        camTr.SetPositionAndRotation(pos, rot);

        fovPunch = Mathf.SmoothDamp(fovPunch, 0f, ref fovPunchVel, 0.25f, Mathf.Infinity, dt);
        float lockFov = lockOnFovOffset * lockBlend;
        fov = Mathf.SmoothDamp(fov, baseFov + bFov + fovPunch + lockFov, ref fovVel, fovDamp, Mathf.Infinity, dt);
        cam.fieldOfView = fov;
    }

    // 公開API
    public void SetProfile(BossCameraProfile p) => profile = p;
    public void ClearProfile() => profile = null;
    public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);
    public void PunchFov(float amount) => fovPunch += amount;

    // 内部
    // シーン上の実体か（プレハブアセット参照・破棄済みを弾く）
    static bool IsSceneInstance(Transform t) => t != null && t.gameObject.scene.IsValid();

    void ResolveReferences()
    {
        if (!IsSceneInstance(boss))
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null) boss = bossObj.transform;
        }
        if (!IsSceneInstance(player))
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        if (scTarget == null || !scTarget.gameObject.scene.IsValid())
        {
            scTarget = FindFirstObjectByType<SC_PlayerTarget>();
        }
    }

    // プレイヤーが安全域からどれだけはみ出しているか(0〜1)を返す
    float ComputeFramePull(Vector3 camPos, Vector3 lookAt, Vector3 playerHead)
    {
        Vector3 fwd = lookAt - camPos;
        if (fwd.sqrMagnitude < 1e-5f) return 0f;

        // カメラのビュー空間でのプレイヤー位置
        Quaternion view = Quaternion.LookRotation(fwd, Vector3.up);
        Vector3 local = Quaternion.Inverse(view) * (playerHead - camPos);

        // カメラの真横〜背後にいる異常時は強めに補正
        if (local.z <= 0.01f) return 1f;

        // 安全域の半画角（縦・横）
        float halfV = fov * 0.5f * frameSafeRatio;
        float aspect = cam ? cam.aspect : 16f / 9f;
        float halfH = Mathf.Atan(Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * aspect) * Mathf.Rad2Deg * frameSafeRatio;

        float yawAng = Mathf.Abs(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg);
        float pitchAng = Mathf.Abs(Mathf.Atan2(local.y, local.z) * Mathf.Rad2Deg);

        float over = Mathf.Max(
            (yawAng - halfH) / Mathf.Max(1f, halfH),
            (pitchAng - halfV) / Mathf.Max(1f, halfV));

        return Mathf.Clamp01(over);
    }

    // ロックオン時のカメラ位置（プレイヤーの後ろ・対象と反対側）SC_PlayerCameraと同じ考え方
    Vector3 CalcLockOnCameraPos(Vector3 playerPos, Vector3 targetPos)
    {
        Vector3 dirFromTarget = Vector3.ProjectOnPlane(playerPos - targetPos, Vector3.up);
        if (dirFromTarget.sqrMagnitude < 1e-4f) dirFromTarget = -player.forward;
        dirFromTarget.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, dirFromTarget).normalized;

        return playerPos
            + camRight * lockOnOffset.x
            + Vector3.up * lockOnOffset.y
            + dirFromTarget * Mathf.Abs(lockOnOffset.z);
    }

    void BlendProfile(float dt)
    {
        float tYaw = 0f, tDistMul = 1f, tHeight = 0f, tFov = 0f, tFocus = focusWeight, blend = returnBlend;
        if (profile != null)
        {
            tYaw = profile.yawOffset;
            // 0以下は未設定とみなし1（等倍）扱い。0のままだと距離が潰れて激ズームになる
            tDistMul = profile.distanceMul > 0f ? profile.distanceMul : 1f;
            tHeight = profile.heightOffset;
            tFov = profile.fovOffset;
            tFocus = profile.focusWeight < 0f ? focusWeight : profile.focusWeight;
            // 0以下は未設定とみなしreturnBlendを使用。0だと1フレームでスナップする
            blend = profile.blendTime > 0f ? profile.blendTime : Mathf.Max(0.0001f, returnBlend);
        }
        float k = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, blend)); // フレームレート非依存
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
}