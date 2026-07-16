using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerCamera : MonoBehaviour
{
    private const float LOOK_HEIGHT = 1.5f;          // LookAt時の高さオフセット
    private const float ZERO_THRESHOLD = 0.0001f;    // ゼロベクトル判定閾値
    private const float MOVE_SPEED_THRESHOLD = 0.3f; // この速度(m/s)未満なら方位を更新しない

    [Header("Ref")]
    [Tooltip("プレイヤーのTransform"), SerializeField] private Transform playerTransform;
    [Tooltip("ロックオン対象情報"), SerializeField] private SC_PlayerTarget scTarget;
    [Tooltip("移動入力"), SerializeField] private InputActionReference iaMove;

    [Header("Setting")]
    [Tooltip("ロックオン時のカメラ位置"), SerializeField] private Vector3 TargetCameraOffset = new Vector3(0, 2, -3);
    [Tooltip("カメラ回転速度"), SerializeField] private float CameraRotateSpeed = 8f;
    [Tooltip("横移動時のカメラ位置補正"), SerializeField] private float CameraHorizontalOffset = 0.5f;
    [Tooltip("モード切り替えブレンド速度"), SerializeField] private float targetBlendSpeed = 1f;

    [Header("斜め前カメラ（通常時）")]
    [Tooltip("見下ろし角（度）大きいほど真上寄り"), SerializeField] private float cameraPitch = 45f;
    [Tooltip("プレイヤーからの基準距離"), SerializeField] private float nonTargetDistance = 6f;
    [Tooltip("カメラの高さ補正（+で上げる/-で下げる。角度・距離は変えない）"), SerializeField] private float nonTargetHeightOffset = 0f;

    [Header("移動方向追従（通常時）")]
    [Tooltip("移動方向へ向きを合わせる速さ"), SerializeField] private float yawFollowSpeed = 3f;
    [Tooltip("速度の平滑化速度。小さいほど旋回がゆったりする"), SerializeField] private float velSmoothSpeed = 8f;
    [Tooltip("カメラ側へ向かって走った時に回り込まない保護。1=完全に保護 / 0=常に追従（酔いやすい）"), SerializeField, Range(0f, 1f)] private float backwardGuard = 1f;

    [Header("敵認知（通常時・近くの敵の方へ向きを寄せる）")]
    [Tooltip("敵へ向きを寄せる強さ（0で無効）"), SerializeField, Range(0f, 1f)] private float awareEnemyBlend = 0.25f;
    [Tooltip("捉える対象とみなす扇の角度（度）"), SerializeField] private float awareEnemyAngle = 120f;
    [Tooltip("捉える対象とみなす距離"), SerializeField] private float awareEnemyDistance = 10f;

    [Header("カメラシェイク")]
    [SerializeField] private float cameraShakeDecay = 5f;

    [Header("FOV演出")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float targetFOV = 65f;
    [SerializeField] private float dashFOV = 70f;
    [SerializeField] private float fovSpeed = 1f;

    [Header("Ref")]
    [Tooltip("動的ズーム・敵認知用のEnemyManager"), SerializeField] private SC_EnemyManager enemyManager;

    [Header("動的ズーム")]
    [Tooltip("敵が近い時の最大引き距離"), SerializeField] private float zoomOutDistance = 3f;
    [Tooltip("ズームアウトが始まる距離"), SerializeField] private float zoomTriggerDistance = 8f;
    [Tooltip("ズーム変化速度"), SerializeField] private float zoomSpeed = 2f;

    private Camera goMainCamera;
    private Transform cameraTransform;    // goMainCamera.transform のキャッシュ
    private bool isTargeting = false;     // ロックオン中か
    private float cameraShakeIntensity = 0f;
    private float currentFOV;
    private bool isDashing = false;
    private float targetingBlend = 0f;    // 通常とロックオンのブレンド
    private float currentZoomOffset = 0f;
    private float currentYaw = 0f;        // 通常カメラの方位角
    private Vector3 lastPlayerPos;
    private Vector3 smoothedVel;          // 平滑化した水平速度

    void Start()
    {
        goMainCamera = GetComponent<Camera>();
        if (goMainCamera == null) goMainCamera = Camera.main;
        if (scTarget == null) scTarget = FindFirstObjectByType<SC_PlayerTarget>();
        if (playerTransform == null && scTarget != null) playerTransform = scTarget.transform;
        if (enemyManager == null) enemyManager = FindFirstObjectByType<SC_EnemyManager>();

        if (iaMove == null)
            Debug.LogError("移動用のInputActionReferenceがアタッチされていません。");
        if (playerTransform == null)
            Debug.LogError("playerTransformがアタッチされていません。");

        if (goMainCamera != null) cameraTransform = goMainCamera.transform;

        if (playerTransform != null)
        {
            lastPlayerPos = playerTransform.position;
            currentYaw = playerTransform.eulerAngles.y;
        }

        currentFOV = normalFOV;
        if (goMainCamera != null) goMainCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (playerTransform == null || goMainCamera == null) return;

        GameObject target = scTarget != null ? scTarget.GetCurrentTarget() : null;
        isTargeting = target != null;
        targetingBlend = Mathf.Lerp(targetingBlend, isTargeting ? 1f : 0f, Time.deltaTime * targetBlendSpeed);

        List<GameObject> enemies = enemyManager != null ? enemyManager.GetEnemies() : null;
        UpdateDynamicZoom(enemies);

        // 通常時：移動方向へ緩く追従＋近くの敵の方へ少し寄せる
        UpdateFollowYaw(enemies);

        float dist = nonTargetDistance + currentZoomOffset;
        Quaternion orbitRot = Quaternion.Euler(cameraPitch, currentYaw, 0f);
        Vector3 lookDir = orbitRot * Vector3.forward;
        Vector3 pivot = playerTransform.position + Vector3.up * LOOK_HEIGHT;
        Vector3 normalPos = pivot - lookDir * dist + Vector3.up * nonTargetHeightOffset;
        Quaternion normalRot = orbitRot;

        //ロックオン
        Vector3 targetPos = normalPos;
        Quaternion targetRot = normalRot;
        if (isTargeting)
        {
            Vector2 inputVal = iaMove != null ? iaMove.action.ReadValue<Vector2>() : Vector2.zero;
            targetPos = CalcTargetCameraPos(inputVal, target);
            Vector3 lookPoint = (playerTransform.position + target.transform.position) * 0.5f + Vector3.up * LOOK_HEIGHT;
            Vector3 dir = lookPoint - targetPos;
            if (dir.sqrMagnitude > ZERO_THRESHOLD) targetRot = Quaternion.LookRotation(dir);
            currentYaw = targetRot.eulerAngles.y;
        }

        //位置、回転ともにブレンド
        Vector3 desiredPos = Vector3.Lerp(normalPos, targetPos, targetingBlend);
        Quaternion desiredRot = Quaternion.Slerp(normalRot, targetRot, targetingBlend);

        // 位置はラグなしで即追従
        cameraTransform.position = desiredPos;
        cameraTransform.rotation = Quaternion.Slerp(
            cameraTransform.rotation, desiredRot, Time.deltaTime * CameraRotateSpeed);

        UpdateFOV();
        ApplyCameraShake();
    }

    // 通常カメラの方位を、移動方向へ緩く追従させ、範囲内に敵がいれば少しそちらへ寄せる
    private void UpdateFollowYaw(List<GameObject> enemies)
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // 実移動から速度を求め、平滑化する
        Vector3 rawVel = (playerTransform.position - lastPlayerPos) / dt;
        lastPlayerPos = playerTransform.position;

        Vector3 flatVel = Vector3.ProjectOnPlane(rawVel, Vector3.up);
        smoothedVel = Vector3.Lerp(smoothedVel, flatVel, dt * velSmoothSpeed);

        float followWeight = 0f;
        float desiredYaw = currentYaw;

        if (smoothedVel.magnitude > MOVE_SPEED_THRESHOLD)
        {
            Vector3 moveDir = smoothedVel.normalized;
            desiredYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;

            // カメラ側（手前）へ走っている時は回り込まない：
            // 奥へ移動=1 / 真横=0.5 / 手前へ移動=0 になる重みを掛ける
            Vector3 camFwdFlat = Quaternion.Euler(0f, currentYaw, 0f) * Vector3.forward;
            float toward = Vector3.Dot(moveDir, camFwdFlat);          // 1=奥 / -1=手前
            float guard = Mathf.Clamp01(toward * 0.5f + 0.5f);        // 0〜1
            followWeight = Mathf.Lerp(1f, guard, backwardGuard);
        }

        // 範囲内に敵がいれば、その方向へ向きを少し寄せて画面に捉える
        if (awareEnemyBlend > 0f)
        {
            GameObject awareEnemy = FindAwareEnemy(enemies);
            if (awareEnemy != null)
            {
                Vector3 toEnemyFlat = Vector3.ProjectOnPlane(
                    awareEnemy.transform.position - playerTransform.position, Vector3.up);
                if (toEnemyFlat.sqrMagnitude > ZERO_THRESHOLD)
                {
                    float enemyYaw = Mathf.Atan2(toEnemyFlat.x, toEnemyFlat.z) * Mathf.Rad2Deg;
                    desiredYaw = Mathf.LerpAngle(desiredYaw, enemyYaw, awareEnemyBlend);
                    // 敵を捉える時は停止中でも向きを寄せる
                    followWeight = Mathf.Max(followWeight, awareEnemyBlend);
                }
            }
        }

        if (followWeight > 0f)
            currentYaw = Mathf.LerpAngle(currentYaw, desiredYaw, dt * yawFollowSpeed * followWeight);
    }

    // 範囲内にいる最も近い敵を返す
    private GameObject FindAwareEnemy(List<GameObject> enemies)
    {
        if (enemies == null || enemies.Count == 0) return null;

        // 探索の中心方向：動いていれば進行方向、止まっていればカメラの前方
        Vector3 searchDir = smoothedVel.magnitude > MOVE_SPEED_THRESHOLD
            ? smoothedVel.normalized
            : Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;

        float cosHalfAngle = Mathf.Cos(awareEnemyAngle * 0.5f * Mathf.Deg2Rad);
        float sqrAwareDist = awareEnemyDistance * awareEnemyDistance;
        GameObject closest = null;
        float closestSqr = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            Vector3 toEnemy = enemy.transform.position - playerTransform.position;
            float sqrDist = toEnemy.sqrMagnitude;
            if (sqrDist > sqrAwareDist) continue;

            Vector3 toEnemyFlat = Vector3.ProjectOnPlane(toEnemy, Vector3.up);
            if (toEnemyFlat.sqrMagnitude < ZERO_THRESHOLD) continue;
            toEnemyFlat.Normalize();
            if (Vector3.Dot(searchDir, toEnemyFlat) < cosHalfAngle) continue;

            if (sqrDist < closestSqr)
            {
                closestSqr = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }

    // 近くの敵の距離に応じてズームアウト
    private void UpdateDynamicZoom(List<GameObject> enemies)
    {
        float targetZoom = 0f;

        if (enemies != null && enemies.Count > 0)
        {
            float closestSqrDist = float.MaxValue;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null || !enemy.activeInHierarchy) continue;
                float sqrDist = (enemy.transform.position - playerTransform.position).sqrMagnitude;
                if (sqrDist < closestSqrDist) closestSqrDist = sqrDist;
            }

            float closestDist = Mathf.Sqrt(closestSqrDist);
            float zoomRatio = 1f - Mathf.Clamp01(closestDist / zoomTriggerDistance);
            targetZoom = zoomRatio * zoomOutDistance;
        }

        currentZoomOffset = Mathf.Lerp(currentZoomOffset, targetZoom, Time.deltaTime * zoomSpeed);
    }

    // ロックオン時のカメラ位置を計算する（プレイヤーの後ろ・対象と反対側）
    private Vector3 CalcTargetCameraPos(Vector2 inputVal, GameObject target)
    {
        Vector3 moveoffset = TargetCameraOffset + new Vector3(inputVal.x * CameraHorizontalOffset, 0f, 0f);

        Vector3 dirFromTarget = Vector3.ProjectOnPlane(
            playerTransform.position - target.transform.position, Vector3.up);

        if (dirFromTarget.sqrMagnitude < ZERO_THRESHOLD) dirFromTarget = -playerTransform.forward;
        dirFromTarget.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, dirFromTarget).normalized;

        Vector3 pos = playerTransform.position
            + camRight * moveoffset.x
            + Vector3.up * moveoffset.y
            + dirFromTarget * Mathf.Abs(moveoffset.z);

        return pos;
    }

    // 視野角調整
    private void UpdateFOV()
    {
        float desiredFOV = isDashing ? dashFOV
                         : isTargeting ? targetFOV
                         : normalFOV;

        currentFOV = Mathf.Lerp(currentFOV, desiredFOV, Time.deltaTime * fovSpeed);
        goMainCamera.fieldOfView = currentFOV;
    }

    // カメラシェイク
    private void ApplyCameraShake()
    {
        if (cameraShakeIntensity <= 0f) return;

        cameraTransform.position += Random.insideUnitSphere * cameraShakeIntensity;
        cameraShakeIntensity = Mathf.Lerp(cameraShakeIntensity, 0f, Time.deltaTime * cameraShakeDecay);
    }

    public void TriggerCameraShake(float intensity) => cameraShakeIntensity = intensity;
    public void SetDashing(bool value) => isDashing = value;
}