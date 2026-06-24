using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossCameraRule
{
    public List<BossAttackStateType> states = new();
    public BossCameraProfile profile = new();

    [Header("着弾の揺れ")]
    public float impactDelay = 0f;     // State開始から何秒後に揺らすか
    public float impactTrauma = 0f;    // 揺れの強さ(0..1)
    public float impactFovPunch = 0f;  // 着弾でFOVを動かす量(負で寄り)
}

[DisallowMultipleComponent]
public class BossCameraDirector : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] BossArenaCamera arenaCamera;
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
            foreach (var s in r.states) map[(int)s] = r; // enum値 = stateList番号
        }
    }

    void Update()
    {
        if (!arenaCamera || !bossStatus) return;

        // ダウン最優先
        bool down = bossStatus.IsBossDown();
        if (down != wasDown)
        {
            wasDown = down;
            if (down) { arenaCamera.SetProfile(downProfile); CancelImpact(); }
            else currentIndex = int.MinValue; // 復帰時に再評価
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