using UnityEngine;

public class SC_FloatingAndRotating : MonoBehaviour
{
    [Header("回転の設定")]
    [Tooltip("1秒あたりの回転角度"), SerializeField] private Vector3 rotateSpeed = new Vector3(50f, 50f, 50f); // 1秒あたりの回転角度

    [Header("上下浮遊の設定")]
    [Tooltip("上下にどれくらい動くか"), SerializeField] private float floatAmplitude = 0.25f; // 浮遊する振幅（上下にどれくらい動くか）
    [Tooltip("浮遊スピード"), SerializeField] private float floatFrequency = 3f;   // 浮遊するスピード（周期）

    private Vector3 startPosition;

    [Header("Alpha値の設定")]
    [Tooltip("Alpha値の下限"), SerializeField] private float minAlpha = 0.25f; // Alpha値の下限
    [Tooltip("Alpha値の上限"), SerializeField] private float maxAlpha = 0.75f;   // Alpha値の上限
    [Tooltip("Alpha値の変更スピード"), SerializeField] private float alphaSpeed = 1f; // Alpha値の変更スピード

    private Material material;


    void Start()
    {
        // 初期位置を記憶
        startPosition = transform.position;

        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // 回転処理
        transform.Rotate(rotateSpeed * Time.deltaTime);

        // 上下浮遊処理
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Alpha値変更
        float t = Mathf.PingPong(Time.time * alphaSpeed, 1f);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }
}