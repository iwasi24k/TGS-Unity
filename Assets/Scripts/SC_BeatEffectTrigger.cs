using UnityEngine;


public class SC_BeatEffectTrigger : MonoBehaviour
{
    public static SC_BeatEffectTrigger Instance { get; private set; }

    void Awake() => Instance = this;

    
    /// 弱攻撃ヒット時（現在は何もしない）
    public void OnWeakHit()
    {
        // 現在は演出なし
    }

  
    /// 強攻撃ヒット時にスロー発動
    public void OnStrongHit()
    {
        TriggerSlow();
    }

 
    /// 2拍分のスローを発動する
    private void TriggerSlow()
    {
        if (SC_DisplaySlow.Instance == null) return;
        if (SC_GameSceneAudio.Instance == null) return;

        float duration = 1.0f; // スロー時間

        SC_DisplaySlow.Instance.Enter(duration);
    }
}