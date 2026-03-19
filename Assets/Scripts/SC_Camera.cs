using UnityEngine;

public class CS_Camera : MonoBehaviour
{
    // Follow Target（追従対象）
    [Header("Follow Target (Player)")]
    [Tooltip("通常時にカメラが追従する対象（プレイヤー）")]
    [SerializeField] private Transform followTarget;

    // Look Target（注視対象）
    [Header("Look Target (Lock-On)")]
    [Tooltip("攻撃モード時に注視する対象（敵など）")]
    private Transform lookAtTarget;

    // Rotation Settings（回転設定）
    private float yaw = 0f;   // 横回転
    private float pitch = 0f; // 縦回転
    [Tooltip("視線入力の感度")]
    [SerializeField] private float sensitivity = 120f;

    // Offset Settings（カメラ位置）
    [Header("Offsets")]

    // 現在のオフセット（モードに応じて補間で変化）
    private Vector3 currentOffset;

    [Tooltip("通常視点のオフセット（上 + 後ろ）")]
    [SerializeField] private Vector3 normalOffset = new Vector3(0, 3, -6);

    [Tooltip("肩越し視点のオフセット（右肩側）")]
    [SerializeField] private Vector3 attackOffset = new Vector3(1.5f, 1.8f, -3);


    // Camera Settings
    [Header("Settings")]
    [Tooltip("offset補間の滑らかさ")]
    [SerializeField] private float smoothSpeed = 10f;


    // Camera State
    private bool isAttackMode = false;   // 攻撃（ロックオン）モード

    // Shake Settings（カメラシェイク）
    private bool enableShake = false;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakePower = 0f;


    // 外部制御API

    /// <summary>
    /// カメラの追従対象を設定する（通常はプレイヤー）
    /// </summary>
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    /// <summary>
    /// 攻撃（ロックオン）モードのON/OFF
    /// </summary>
    public void SetAttackMode(bool enable)
    {
        isAttackMode = enable;
    }

    /// <summary>
    /// 注視対象（ロックオン対象）を設定
    /// </summary>
    public void SetLookAtTarget(Transform target)
    {
        lookAtTarget = target;
    }

    /// <summary>
    /// カメラシェイクを開始する
    /// </summary>
    /// <param name="duration">揺れる時間（秒）</param>
    /// <param name="power">揺れの強さ</param>
    public void StartShake(float duration, float power)
    {
        enableShake = true;
        shakeDuration = duration;
        shakePower = power;
        shakeTimer = duration;
    }


    /// <summary>
    /// カメラの視線入力を追加する
    /// </summary>
    /// <param name="input">スティックの移動量</param>
    public void AddLookInput(Vector2 input)
    {
        yaw += input.x * sensitivity * Time.deltaTime;
        pitch += input.y * sensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -30f, 0f);
    }

    // 初期化
    void Start()
    {
        currentOffset = normalOffset;
    }

    // メイン更新
    void LateUpdate()
    {
        if (!followTarget) return;

        UpdateOffset();
        UpdatePosition();
        UpdateShake();
        UpdateRotation();
    }

    // 位置更新
    private void UpdatePosition()
    {
        Quaternion cameraRot = Quaternion.Euler(pitch, yaw, 0f);

        if (isAttackMode)
        {
            // 肩越し・攻撃中：プレイヤー基準の位置
            Vector3 desiredPos = followTarget.position + followTarget.rotation * currentOffset;
            transform.position = desiredPos;
        }
        else
        {
            // Normal：yaw/pitchでカメラ位置を回す
            Vector3 rotatedOffset = cameraRot * currentOffset;
            Vector3 desiredPos = followTarget.position + rotatedOffset;
            transform.position = desiredPos;
        }
    }

    // 現在のモードに応じたオフセットを取得
    private Vector3 GetCurrentOffset()
    {
        return isAttackMode ? attackOffset : normalOffset;
    }

    // オフセットを滑らかに更新
    private void UpdateOffset()
    {
        Vector3 targetOffset = GetCurrentOffset();
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * smoothSpeed);
    }

    // 回転更新（視線）
    private void UpdateRotation()
    {
        if (isAttackMode && lookAtTarget)
        {
            transform.LookAt(lookAtTarget.position);
        }
        else
        {
            // 通常：プレイヤーを見る
            Vector3 lookPos = followTarget.position + Vector3.up * 1.5f;
            transform.LookAt(lookPos);
        }
    }

    // シェイク処理
    private void UpdateShake()
    {
        if (!enableShake || shakeTimer <= 0f) return;

        float t = shakeTimer / shakeDuration;
        float power = shakePower * t;

        transform.position += Random.insideUnitSphere * power;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0f)
            enableShake = false;
    }
}