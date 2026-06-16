using System.Collections;
using UnityEngine;

/// 画面全体のスローモーション
public class SC_DisplaySlow : MonoBehaviour
{
    public static SC_DisplaySlow Instance { get; private set; }

    [Tooltip("スロー時の速度倍率。0.25 = 4分の1速")]
    [SerializeField][Range(0.05f, 1f)] private float slowFactor = 0.25f;

    [Tooltip("突入の鋭さ。小さいほどカチッと入る")]
    [SerializeField] private float enterSmoothTime = 0.05f;

    [Tooltip("復帰のなめらかさ。大きいほどゆっくり戻る")]
    [SerializeField] private float exitSmoothTime = 0.15f;

    private float _target = 1f;
    private float _vel = 0f;
    private Coroutine _autoExit;

    void Awake() => Instance = this;

    void Update()
    {
        if (SC_HitStop.IsActive) return; // HitStop 中は timeScale を触らない

        float smooth = Time.timeScale > _target ? enterSmoothTime : exitSmoothTime;
        Time.timeScale = Mathf.SmoothDamp(Time.timeScale, _target, ref _vel, smooth, float.MaxValue, Time.unscaledDeltaTime);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    /// スロー開始。duration 省略で Exit() まで継続。
    public void Enter(float duration = -1f)
    {
        if (_autoExit != null) StopCoroutine(_autoExit);
        _target = slowFactor;
        if (duration > 0f) _autoExit = StartCoroutine(AutoExit(duration));
    }

    /// 通常速度へ復帰。
    public void Exit()
    {
        if (_autoExit != null) StopCoroutine(_autoExit);
        _target = 1f;
    }

    private IEnumerator AutoExit(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        _target = 1f;
    }
}