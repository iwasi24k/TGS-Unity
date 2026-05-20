using System.Collections;
using UnityEngine;

/// 衝撃の瞬間だけ時間を止めるヒットストップ
public class SC_HitStop : MonoBehaviour
{
    public static SC_HitStop Instance { get; private set; }
    public static bool IsActive { get; private set; } // SC_DisplaySlow が参照する

    [Tooltip("停止時間（秒）")]
    [SerializeField] private float defaultDuration = 0.08f;

    [Tooltip("停止中の timeScale。0=完全停止 / 0.1前後=超スロー")]
    [SerializeField][Range(0f, 0.2f)] private float freezeTimeScale = 0f;

    private Coroutine _routine;
    private float _prevTimeScale = 1f;

    void Awake() => Instance = this;

    /// ヒットストップ発動。引数省略で defaultDuration を使用。
    public void Trigger(float duration = -1f)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Run(duration < 0f ? defaultDuration : duration));
    }

    private IEnumerator Run(float duration)
    {
        IsActive = true;
        _prevTimeScale = Time.timeScale;
        Time.timeScale = freezeTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = _prevTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        IsActive = false;
    }
}