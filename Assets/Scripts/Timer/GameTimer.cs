using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField]
    private float startTime = 60f;

    [SerializeField]
    private bool startOnAwake = true;

    public UnityEvent onTimeUp;

    private float currentTime;
    private bool isRunning;

    /// <summary>
    /// 残り時間（秒）
    /// </summary>
    public float CurrentTime => currentTime;

    /// <summary>
    /// タイマー終了済みか
    /// </summary>
    public bool IsFinished => currentTime <= 0f;

    private void Start()
    {
        ResetTimer();

        if (startOnAwake)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;

            Debug.Log("Time Up!");

            onTimeUp?.Invoke();
        }
    }

    /// <summary>
    /// タイマー開始
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// タイマー停止
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// タイマーリセット
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTime;
    }
}