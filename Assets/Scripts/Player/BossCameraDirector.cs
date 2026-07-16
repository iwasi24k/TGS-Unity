using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossCameraRule
{
    [Tooltip("このカメラ演出を適用する攻撃State（複数可）")]
    public List<BossAttackStateType> states = new();

    [Tooltip("適用するカメラ構図")]
    public BossCameraProfile profile = new();

    [Header("演出の長さ")]
    [Tooltip("State開始から何秒後に演出を始めるか。0で即座に開始")]
    public float startDelay = 0f;

    [Tooltip("演出を維持する時間(秒)。0なら「Stateが終わるまで」＝従来動作。\n値を入れると、その時間が過ぎた時点で通常構図へ戻る。\nState自体が先に終わっても、次のStateに演出が設定されていなければ残り時間ぶん維持される")]
    public float duration = 0f;

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

    // 進行中のルール
    BossCameraRule activeRule;
    float ruleTimer;
    bool profileApplied;
    bool impactDone;

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
                    CancelRule();
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
            if (down) { arenaCamera.SetProfile(downProfile); CancelRule(); }
            else currentIndex = int.MinValue;   // 復帰後に現Stateのルールを再適用させる
        }
        if (down) return;

        // 現在Stateの変化を監視
        int idx = bossStatus.GetCurrentStateIndex();
        if (idx != currentIndex)
        {
            currentIndex = idx;
            Apply(idx);
        }
        TickRule();
    }

    void Apply(int idx)
    {
        if (map.TryGetValue(idx, out var rule))
        {
            // 新しい演出が設定されているStateなら、それを最優先で上書きする
            activeRule = rule;
            ruleTimer = 0f;
            profileApplied = false;
            impactDone = false;

            if (rule.startDelay <= 0f)
            {
                arenaCamera.SetProfile(rule.profile);
                profileApplied = true;
            }
            return;
        }

        // 演出の無いStateへ移った場合
        // Durationが残っていれば、そのまま維持する（短い攻撃の余韻を伸ばせる）
        if (activeRule != null && activeRule.duration > 0f &&
            ruleTimer < activeRule.startDelay + activeRule.duration)
            return;

        arenaCamera.ClearProfile();
        CancelRule();
    }

    void TickRule()
    {
        if (activeRule == null) return;

        ruleTimer += Time.deltaTime;

        // 開始待ち
        if (!profileApplied && ruleTimer >= activeRule.startDelay)
        {
            arenaCamera.SetProfile(activeRule.profile);
            profileApplied = true;
        }

        // 着弾の揺れ（State開始からの経過時間で判定）
        if (!impactDone && activeRule.impactDelay > 0f && ruleTimer >= activeRule.impactDelay)
        {
            if (activeRule.impactTrauma > 0f) arenaCamera.AddTrauma(activeRule.impactTrauma);
            if (activeRule.impactFovPunch != 0f) arenaCamera.PunchFov(activeRule.impactFovPunch);
            impactDone = true;
        }

        // 演出終了（Durationが0なら「Stateが終わるまで」なので何もしない）
        if (profileApplied && activeRule.duration > 0f &&
            ruleTimer >= activeRule.startDelay + activeRule.duration)
        {
            arenaCamera.ClearProfile();
            CancelRule();
        }
    }

    void CancelRule()
    {
        activeRule = null;
        ruleTimer = 0f;
        profileApplied = false;
        impactDone = false;
    }
}