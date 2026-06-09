using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerCamera : MonoBehaviour
{
    private const float LOOK_HEIGHT = 1.5f;    // LookAt時の高さオフセット
    private const float ZERO_THRESHOLD = 0.0001f; // ゼロベクトル判定閾値
    private const float INPUT_DEAD_ZONE = 0.01f;   // 入力デッドゾーン
    private const float EULER_FLIP = 180f;    // eulerAngles折り返し判定

    [Header("Ref")]
    private Camera goMainCamera;
    [Tooltip("プレイヤーのTransform"), SerializeField] private Transform playerTransform;
    [Tooltip("ターゲット情報"), SerializeField] private SC_PlayerTarget scTarget;
    [Tooltip("移動入力"), SerializeField] private InputActionReference iaMove;

    [Header("Setting")]
    [Tooltip("非ターゲット時のカメラ位置"), SerializeField] private Vector3 NonTargetCameraOffset = new Vector3(0, 3, -5);
    [Tooltip("ターゲット時のカメラ位置"), SerializeField] private Vector3 TargetCameraOffset = new Vector3(0, 2, -2);
    [Tooltip("カメラ移動速度"), SerializeField] private float CameraMoveSpeed = 5f;
    [Tooltip("ダッシュ時カメラ移動速度"), SerializeField] private float DashCameraMoveSpeed = 15f;
    [Tooltip("ターゲット時カメラ移動速度"), SerializeField] private float TargetingCameraMoveSpeed = 10f;
    [Tooltip("カメラ回転速度"), SerializeField] private float CameraRotateSpeed = 8f;
    [Tooltip("横移動時のカメラ位置補正"), SerializeField] private float CameraHorizontalOffset = 0.5f;
    [Tooltip("ターゲット切り替えブレンド速度"), SerializeField] private float targetBlendSpeed = 1f;

    [Header("障害物回避")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float cameraRadius = 0.3f;

    [Header("カメラシェイク")]
    [SerializeField] private float cameraShakeDecay = 5f;

    [Header("アクティブ演出")]
    [SerializeField] private float tiltAmount = 0.1f;
    [SerializeField] private float tiltSpeed = 0.01f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float targetFOV = 65f;
    [SerializeField] private float dashFOV = 70f;
    [SerializeField] private float fovSpeed = 1f;

    [Header("敵認知カメラ")]
    [SerializeField] private SC_Field field;
    [SerializeField] private float awareEnemyAngle = 60f;
    [SerializeField] private float awareEnemyDistance = 10f;
    [SerializeField] private float awareEnemyBlend = 0.3f;

    [Header("カメラ角度制限")]
    [Tooltip("カメラの最小仰角（小さいほど上を向く）"), SerializeField] private float minPitch = 10f;
    [Tooltip("カメラの最大俯角（大きいほど下を向く）"), SerializeField] private float maxPitch = 50f;

    [Header("動的ズーム")]
    [Tooltip("敵が近い時の最大引き距離"), SerializeField] private float zoomOutDistance = 3f;
    [Tooltip("ズームアウトが始まる距離"), SerializeField] private float zoomTriggerDistance = 8f;
    [Tooltip("ズーム変化速度"), SerializeField] private float zoomSpeed = 2f;

    private bool isTargeting = false;
    private float cameraShakeIntensity = 0f;
    private float currentTilt = 0f;
    private float currentFOV = 60f;
    private bool isDashing = false;
    private float targetingBlend = 0f;
    private float currentZoomOffset = 0f;

    void Start()
    {
        goMainCamera = GetComponent<Camera>();
        if (goMainCamera == null) goMainCamera = Camera.main;
        if (scTarget == null) scTarget = FindFirstObjectByType<SC_PlayerTarget>();
        if (playerTransform == null && scTarget != null) playerTransform = scTarget.transform;
        if (field == null) field = FindFirstObjectByType<SC_Field>();

        if (iaMove == null)
            Debug.LogError("移動用のInputActionReferenceがアタッチされていません。");
        if (playerTransform == null)
            Debug.LogError("playerTransformがアタッチされていません。");

        currentFOV = normalFOV;
        goMainCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector2 inputVal = iaMove.action.ReadValue<Vector2>();
        GameObject target = scTarget.GetCurrentTarget();

        isTargeting = target != null;
        float blendGoal = isTargeting ? 1f : 0f;
        targetingBlend = Mathf.Lerp(targetingBlend, blendGoal, Time.deltaTime * targetBlendSpeed);

        List<GameObject> enemies = field != null ? field.GetEnemies() : null;

        UpdateDynamicZoom(enemies);

        Vector3 zoomedOffset = NonTargetCameraOffset + new Vector3(0f, 0f, -currentZoomOffset);
        Vector3 normalDesiredPos = AvoidObstacle(playerTransform.position + zoomedOffset);
        Vector3 targetDesiredPos = isTargeting ? CalcTargetCameraPos(inputVal, target) : normalDesiredPos;
        Vector3 desiredPos = Vector3.Lerp(normalDesiredPos, targetDesiredPos, targetingBlend);

        float currentSpeed = isDashing ? DashCameraMoveSpeed
                           : isTargeting ? TargetingCameraMoveSpeed
                           : CameraMoveSpeed;

        goMainCamera.transform.position = Vector3.Lerp(
            goMainCamera.transform.position, desiredPos, currentSpeed * Time.deltaTime);

        // 注視点の計算
        Vector3 playerLookAt = playerTransform.position + Vector3.up * LOOK_HEIGHT;
        Vector3 lookTarget;

        if (isTargeting)
        {
            // ターゲット時：プレイヤーと敵の中間を注視
            lookTarget = (playerTransform.position + target.transform.position) * 0.5f + Vector3.up * LOOK_HEIGHT;
        }
        else
        {
            // 通常時：前方の認知エネミーをカメラに映す
            GameObject awareEnemy = FindAwareEnemy(inputVal, enemies);
            lookTarget = awareEnemy != null
                ? Vector3.Lerp(
                    playerLookAt,
                    (playerTransform.position + awareEnemy.transform.position) * 0.5f + Vector3.up * LOOK_HEIGHT,awareEnemyBlend)
                : playerLookAt;
        }

        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - goMainCamera.transform.position);
        goMainCamera.transform.rotation = Quaternion.Slerp(
            goMainCamera.transform.rotation, desiredRot, Time.deltaTime * CameraRotateSpeed);

        UpdateTilt(inputVal);
        UpdateFOV(isTargeting);
        ApplyCameraShake();
    }

    //近くの敵の距離に応じてズームアウトして攻撃しやすくする
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

            //ズーム比率の計算にはsqrtが必要なので1回だけ使用
            float closestDist = Mathf.Sqrt(closestSqrDist);
            float zoomRatio = 1f - Mathf.Clamp01(closestDist / zoomTriggerDistance);
            targetZoom = zoomRatio * zoomOutDistance;
        }

        currentZoomOffset = Mathf.Lerp(currentZoomOffset, targetZoom, Time.deltaTime * zoomSpeed);
    }

    //ターゲット時のカメラ位置を計算する
    private Vector3 CalcTargetCameraPos(Vector2 inputVal, GameObject target)
    {
        Vector3 moveoffset = TargetCameraOffset + new Vector3(inputVal.x * CameraHorizontalOffset, 0f, 0f);

        Vector3 dirFromTarget = Vector3.ProjectOnPlane(
            playerTransform.position - target.transform.position, Vector3.up);

        if (dirFromTarget.sqrMagnitude < ZERO_THRESHOLD) dirFromTarget = -playerTransform.forward;
        dirFromTarget.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, dirFromTarget).normalized;

        return playerTransform.position
            + camRight * moveoffset.x
            + Vector3.up * moveoffset.y
            + dirFromTarget * Mathf.Abs(moveoffset.z);
    }

    //プレイヤーの向かう方向にいる最も近いエネミーを返す
    private GameObject FindAwareEnemy(Vector2 inputVal, List<GameObject> enemies)
    {
        if (enemies == null || enemies.Count == 0) return null;

        Vector3 searchDir = inputVal.sqrMagnitude > INPUT_DEAD_ZONE
            ? Vector3.ProjectOnPlane(
                goMainCamera.transform.forward * inputVal.y +
                goMainCamera.transform.right * inputVal.x,
                Vector3.up).normalized
            : Vector3.ProjectOnPlane(goMainCamera.transform.forward, Vector3.up).normalized;

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

            Vector3 toEnemyFlat = Vector3.ProjectOnPlane(toEnemy, Vector3.up).normalized;
            if (Vector3.Dot(searchDir, toEnemyFlat) < cosHalfAngle) continue;

            if (sqrDist < closestSqr)
            {
                closestSqr = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }

    //傾けるやつ
    private void UpdateTilt(Vector2 input)
    {
        float targetTilt = isDashing ? 0f : -input.x * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        goMainCamera.transform.rotation *= Quaternion.AngleAxis(currentTilt, Vector3.forward);
    }

    //視野角調整
    private void UpdateFOV(bool targeting)
    {
        float desiredFOV = isDashing ? dashFOV
                         : targeting ? targetFOV
                         : normalFOV;

        currentFOV = Mathf.Lerp(currentFOV, desiredFOV, Time.deltaTime * fovSpeed);
        goMainCamera.fieldOfView = currentFOV;
    }

    //カメラシェイク
    private void ApplyCameraShake()
    {
        if (cameraShakeIntensity <= 0f) return;

        goMainCamera.transform.position += Random.insideUnitSphere * cameraShakeIntensity;
        cameraShakeIntensity = Mathf.Lerp(cameraShakeIntensity, 0f, Time.deltaTime * cameraShakeDecay);
    }

    //障害物回避(ターゲット時のみ)
    private Vector3 AvoidObstacle(Vector3 desiredPos)
    {
        Vector3 origin = playerTransform.position + Vector3.up * LOOK_HEIGHT;
        Vector3 dir = desiredPos - origin;
        float dist = dir.magnitude;

        if (Physics.SphereCast(origin, cameraRadius, dir.normalized, out RaycastHit hit, dist, obstacleLayer))
            return origin + dir.normalized * (hit.distance - cameraRadius);

        return desiredPos;
    }

    public void TriggerCameraShake(float intensity) => cameraShakeIntensity = intensity;
    public void SetDashing(bool value) => isDashing = value;
}