using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossCameraRule
{
    [Tooltip("このカメラ演出を適用する攻撃State（複数可）")]
    public List<BossAttackStateType> states = new();

    [Tooltip("適用するカメラ構図")]
    public BossCameraProfile profile = new();

    [Header("着弾の揺れ（使わない場合は全て0のままでOK）")]
    [Tooltip("State開始から何秒後に揺らすか。0なら揺れ演出なし")]
    public float impactDelay = 0f;
    [Tooltip("揺れの強さ(0〜1)。0.3=小 / 0.6=大")]
    public float impactTrauma = 0f;
    [Tooltip("揺れと同時のFOVパンチ(度)。+で一瞬広角に")]
    public float impactFovPunch = 0f;
}

[DisallowMultipleComponent]
public class BossCameraDirector : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("制御するボスカメラ")]
    [SerializeField] BossArenaCamera arenaCamera;
    [Tooltip("ボスのStatusManager。プレハブ参照/空欄でもOK（実行時にTag=Bossを自動取得）")]
    [SerializeField] SC_EnemyStatusManager bossStatus;

    [Header("攻撃→カメラ 対応表")]
    [SerializeField] List<BossCameraRule> rules = new();

    [Header("ダウン演出")]
    [SerializeField]
    BossCameraProfile downProfile = new()
    {
        name = "Down",
        distanceMul = 0.7f,
        heightOffset = -1f,
        fovOffset = -6f,
        focusWeight = 0.05f,
        blendTime = 0.6f
    };

    readonly Dictionary<int, BossCameraRule> map = new();
    int currentIndex = int.MinValue;
    bool wasDown;
    float impactTimer = -1f;
    BossCameraRule pendingImpact;

    void Awake()
    {
        foreach (var r in rules)
        {
            if (r == null) continue;
            foreach (var s in r.states) map[(int)s] = r;
        }
    }

    void Update()
    {
        // bossStatusが未取得（またはプレハブアセット参照）なら、シーン上の実体を探す
        if (bossStatus == null || !bossStatus.gameObject.scene.IsValid())
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null)
            {
                bossStatus = bossObj.GetComponent<SC_EnemyStatusManager>();
                if (bossStatus != null)
                    Debug.Log("BossCameraDirector: シーン上のBossを取得しました : " + bossStatus.name);
            }
            if (bossStatus == null)
            {
                // ボス不在の間は演出を確実に解除しておく
                if (currentIndex != int.MinValue && arenaCamera != null)
                {
                    arenaCamera.ClearProfile();
                    currentIndex = int.MinValue;
                    wasDown = false;
                    CancelImpact();
                }
                return;
            }
            // 新しいボスを取得したら状態監視をリセット
            currentIndex = int.MinValue;
            wasDown = false;
        }

        if (!arenaCamera) return;

        // ダウン最優先
        bool down = bossStatus.IsBossDown();
        if (down != wasDown)
        {
            wasDown = down;
            if (down) { arenaCamera.SetProfile(downProfile); CancelImpact(); }
            else currentIndex = int.MinValue;   // 復帰後に現Stateのルールを再適用させる
        }
        if (down) { TickImpact(); return; }

        // 現在Stateの変化を監視
        int idx = bossStatus.GetCurrentStateIndex();
        if (idx != currentIndex)
        {
            currentIndex = idx;
            Apply(idx);
        }
        TickImpact();
    }

    void Apply(int idx)
    {
        CancelImpact();
        if (map.TryGetValue(idx, out var rule))
        {
            arenaCamera.SetProfile(rule.profile);
            if (rule.impactDelay > 0f && (rule.impactTrauma > 0f || rule.impactFovPunch != 0f))
            {
                pendingImpact = rule;
                impactTimer = 0f;
            }
        }
        else arenaCamera.ClearProfile();
    }

    void TickImpact()
    {
        if (pendingImpact == null) return;
        impactTimer += Time.deltaTime;
        if (impactTimer < pendingImpact.impactDelay) return;

        if (pendingImpact.impactTrauma > 0f) arenaCamera.AddTrauma(pendingImpact.impactTrauma);
        if (pendingImpact.impactFovPunch != 0f) arenaCamera.PunchFov(pendingImpact.impactFovPunch);
        CancelImpact();
    }

    void CancelImpact() { impactTimer = -1f; pendingImpact = null; }
}