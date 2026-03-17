using UnityEngine;

public class CS_Camera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offsets")]
    // 通常視点
    // プレイヤー中心から「上 + 後ろ」に配置することで
    [SerializeField] private  Vector3 normalOffset = new Vector3(0, 3, -6);

    // 肩越し視点
    // プレイヤーの右肩側に寄せることで
    [SerializeField] private Vector3 shoulderOffset = new Vector3(1.5f, 1.8f, -3);

    [Header("Settings")]
    // カメラの追従スピード
    // 値が大きいほど素早く追従、小さいほど滑らか
    [SerializeField] private float smoothSpeed = 10f;

    //外部制御用
    // 現在のカメラモード
    // true  : 肩越し視点
    // false : 通常視点
    // 外部（Playerなど）から SetShoulderMode() で切り替える
    private bool isShoulder = false;

    //シェイク
    // シェイクが有効かどうか
    private bool enableShake = false;

    // シェイクの残り時間
    private float shakeTimer = 0f;

    // シェイクの総時間（開始時に設定）
    private float shakeDuration = 0f;

    // シェイクの強さ（振れ幅）
    private float shakePower = 0f;

    // =========================
    // 外部から設定する関数
    // =========================

    // カメラの追従対象を設定する関数
    // 主にプレイヤーや注視したいオブジェクトを指定する
    // 例：cam.SetTarget(player.transform);
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // カメラのモードを切り替える関数
    // true  : 肩越し視点（戦闘・エイム用）
    // false : 通常視点（探索用）
    // 外部（Playerなど）から入力に応じて切り替える
    // 例：cam.SetShoulderMode(true);
    public void SetShoulderMode(bool enable)
    {
        isShoulder = enable;
    }

    // カメラシェイク（揺れ）を開始する関数
    // duration : 揺れる時間（秒）
    // power    : 揺れの強さ（値が大きいほど激しい）
    // 例：cam.StartShake(0.2f, 0.3f);
    public void StartShake(float duration, float power)
    {
        enableShake = true;
        shakeDuration = duration;
        shakePower = power;
        shakeTimer = duration;
    }

    // =========================
    // カメラ更新
    // =========================

    void LateUpdate()
    {
        if (!target) return;

        Vector3 offset = isShoulder ? shoulderOffset : normalOffset;

        Vector3 desiredPosition = target.position + target.rotation * offset;

        Vector3 finalPos = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        //シェイク
        if (enableShake && shakeTimer > 0f)
        {
            float t = shakeTimer / shakeDuration;
            float currentPower = shakePower * t;

            finalPos += Random.insideUnitSphere * currentPower;

            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
                enableShake = false;
        }

        transform.position = finalPos;

        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        transform.LookAt(lookTarget);
    }


}
