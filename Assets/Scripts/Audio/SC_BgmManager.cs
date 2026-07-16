using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// 1曲分のデータ。Intro(1回だけ)→Loop(繰り返し) の構成に対応する。
/// Introが無い曲はLoopのみを指定すればよい。
/// </summary>
[System.Serializable]
public class BgmTrack
{
    [Tooltip("曲を識別する名前。シーン対応表やスクリプトから呼ぶ時のキーになる")]
    public string id = "NewTrack";

    [Tooltip("イントロ（1回だけ再生）。無い曲は空欄でOK")]
    public AudioClip introClip;

    [Tooltip("ループ本体（必須）")]
    public AudioClip loopClip;

    [Tooltip("ループ本体を繰り返すか。Result等の単発曲はオフ")]
    public bool loop = true;

    [Tooltip("この曲の音量（曲ごとの音量差を吸収する）")]
    [Range(0f, 1f)] public float volume = 1f;

    [Tooltip("イントロ→ループの繋ぎ微調整(秒)。-で早める / +で遅らせる。通常は0でぴったり繋がる")]
    public float introToLoopOffset = 0f;

    [Tooltip("この曲へ切り替わる時のフェードイン時間(秒)。0で即時")]
    public float fadeInTime = 0f;

    [Tooltip("この曲から切り替わる時のフェードアウト時間(秒)。0で即時カット")]
    public float fadeOutTime = 1f;
}

/// <summary>シーン名 → 自動再生する曲 の対応</summary>
[System.Serializable]
public class SceneBgmEntry
{
    [Tooltip("シーン名。'Scene Game' / 'Scene_Game' どちらの表記でも一致する")]
    public string sceneName;

    [Tooltip("再生する曲のID")]
    public string trackId;

    [Tooltip("クリア済み(GameData.Result.IsCleared==true)の時に優先する曲ID。Resultシーン用。空欄なら未使用")]
    public string trackIdOnCleared;

    [Tooltip("シーン読込から再生開始までの待ち時間(秒)")]
    public float startDelay = 0f;
}

/// <summary>
/// 全シーン共通のBGMマネージャ。
/// ・Intro→Loop をサンプル単位で繋いで再生（PlayScheduled）
/// ・曲切替はクロスフェード
/// ・シーン名から自動再生 / 雑魚戦の Before→Intro→Loop / ボス出現の自動検知
/// ・音量、ミュート、ポーズ、ダッキング
/// DontDestroyOnLoadで常駐するため、全シーンに同じプレハブを置いておけばよい（重複は自動で破棄）。
/// </summary>
[DisallowMultipleComponent]
public class SC_BgmManager : MonoBehaviour
{
    public static SC_BgmManager Instance { get; private set; }

    //================================================================
    // Inspector
    //================================================================
    [Header("曲リスト")]
    [Tooltip("この game に登場する全てのBGMを登録する")]
    [SerializeField] private List<BgmTrack> tracks = new();

    [Header("シーン→曲 対応表")]
    [Tooltip("シーンを読み込んだ時に自動再生する曲。対応の無いシーンでは下の設定に従う")]
    [SerializeField] private List<SceneBgmEntry> sceneTracks = new();

    [Tooltip("対応表に無いシーンではBGMを止める。オフなら鳴らしっぱなしで引き継ぐ")]
    [SerializeField] private bool stopIfSceneNotMapped = true;

    [Header("雑魚戦（Before → ドア破壊 → Intro → Loop）")]
    [Tooltip("戦闘前に流す曲のID。シーン対応表でGameシーンにこれを指定しておく")]
    [SerializeField] private string beforeBattleTrackId = "BattleBefore";

    [Tooltip("ドアを吹っ飛ばした時に切り替える曲のID")]
    [SerializeField] private string battleTrackId = "Battle";

    [Tooltip("ドア破壊から戦闘BGM開始までの遅延(秒)。演出に合わせて微調整する")]
    [SerializeField] private float battleStartDelay = 0f;

    [Tooltip("Beforeを切る時のフェード時間(秒)。0でスパッと切り替わる（推奨）")]
    [SerializeField] private float beforeFadeOutTime = 0f;

    [Header("ボス戦（Tag=Bossの出現を自動検知）")]
    [Tooltip("ボス出現を自動で検知して曲を切り替える")]
    [SerializeField] private bool autoDetectBoss = true;

    [Tooltip("ボス曲のID")]
    [SerializeField] private string bossTrackId = "Boss";

    [Tooltip("検知するタグ")]
    [SerializeField] private string bossTag = "Boss";

    [Tooltip("検知の間隔(秒)。毎フレーム探さないための間引き")]
    [SerializeField] private float bossCheckInterval = 0.25f;

    [Tooltip("ボス出現から曲が切り替わるまでの遅延(秒)")]
    [SerializeField] private float bossStartDelay = 0f;

    [Tooltip("ボス撃破時にBGMをフェードアウトさせる")]
    [SerializeField] private bool fadeOutOnBossDefeat = true;

    [Tooltip("ボス撃破時のフェードアウト時間(秒)")]
    [SerializeField] private float bossDefeatFadeTime = 1.5f;

    [Header("音量")]
    [Tooltip("BGM全体の音量")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.7f;

    [Tooltip("起動時に前回の音量設定(PlayerPrefs)を読み込む")]
    [SerializeField] private bool saveVolumeToPrefs = true;

    [Tooltip("音量の保存キー")]
    [SerializeField] private string volumePrefsKey = "BGM_Volume";

    [Tooltip("出力先のAudioMixerGroup。使わないなら空欄でOK")]
    [SerializeField] private AudioMixerGroup mixerGroup;

    [Header("共通タイミング")]
    [Tooltip("再生予約を何秒先に入れるか。小さすぎると頭が欠ける。0.05〜0.2が目安")]
    [SerializeField] private float scheduleLeadTime = 0.1f;

    [Tooltip("同じ曲を再度Playした時に、頭から再生し直す。オフなら無視して継続")]
    [SerializeField] private bool restartOnSameTrack = false;

    [Header("デバッグ")]
    [Tooltip("曲の切替をConsoleに出す")]
    [SerializeField] private bool debugLog = false;

    //================================================================
    // 内部
    //================================================================
    private class Deck
    {
        public AudioSource intro;
        public AudioSource loop;
        public BgmTrack track;
        public double introStartDsp;
        public double loopStartDsp;
        public float currentVol;   // フェード用 0〜1
        public float targetVol;
        public float fadeRate;     // 1秒あたりの変化量。0で即時
        public bool active;

        public bool IsPlaying => active && (intro.isPlaying || loop.isPlaying
                                            || AudioSettings.dspTime < loopStartDsp);
    }

    private Deck deckA, deckB;
    private Deck current;          // 今鳴っているデッキ
    private float duckMul = 1f;    // ダッキング係数
    private float duckTarget = 1f;
    private float duckRate = 0f;
    private bool isMuted;
    private bool isPaused;
    private double pauseDsp;

    private bool battleStarted;    // ドア破壊済みか
    private bool bossStarted;      // ボス曲に入ったか
    private bool bossDefeatHandled;
    private float bossCheckTimer;

    private float pendingDelay = -1f;   // 遅延再生用
    private string pendingTrackId;

    public string CurrentTrackId => current != null && current.track != null ? current.track.id : null;
    public bool IsPlaying => current != null && current.IsPlaying;

    //================================================================
    // 初期化
    //================================================================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);   // 各シーンに置いても重複しないようにする
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        deckA = CreateDeck("Deck_A");
        deckB = CreateDeck("Deck_B");

        if (saveVolumeToPrefs && PlayerPrefs.HasKey(volumePrefsKey))
            masterVolume = PlayerPrefs.GetFloat(volumePrefsKey);

        PreloadAll();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneTrack(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private Deck CreateDeck(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        return new Deck
        {
            intro = CreateSource(go, "Intro"),
            loop = CreateSource(go, "Loop"),
            currentVol = 0f,
            targetVol = 0f
        };
    }

    private AudioSource CreateSource(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f;      // 2D
        src.volume = 0f;
        src.ignoreListenerPause = true;
        src.ignoreListenerVolume = false;
        if (mixerGroup != null) src.outputAudioMixerGroup = mixerGroup;
        return src;
    }

    // 予約再生の頭欠け防止のため、あらかじめ全クリップを読み込む
    private void PreloadAll()
    {
        foreach (var t in tracks)
        {
            if (t == null) continue;
            if (t.introClip != null) t.introClip.LoadAudioData();
            if (t.loopClip != null) t.loopClip.LoadAudioData();
        }
    }

    //================================================================
    // 毎フレーム処理（フェード・ボス検知・遅延再生）
    //================================================================
    private void Update()
    {
        // Time.timeScaleの影響を受けないようにunscaledを使う（ヒットストップ・スロー対策）
        float dt = Time.unscaledDeltaTime;

        UpdateDeckFade(deckA, dt);
        UpdateDeckFade(deckB, dt);

        // ダッキング
        if (duckRate > 0f) duckMul = Mathf.MoveTowards(duckMul, duckTarget, duckRate * dt);
        else duckMul = duckTarget;

        ApplyVolumes();

        // 遅延再生
        if (pendingDelay >= 0f)
        {
            pendingDelay -= dt;
            if (pendingDelay <= 0f)
            {
                pendingDelay = -1f;
                PlayImmediate(pendingTrackId);
            }
        }

        UpdateBossDetection(dt);
    }

    private void UpdateDeckFade(Deck d, float dt)
    {
        if (d == null) return;

        if (d.fadeRate <= 0f) d.currentVol = d.targetVol;
        else d.currentVol = Mathf.MoveTowards(d.currentVol, d.targetVol, d.fadeRate * dt);

        // フェードアウトが完了したデッキは停止する
        if (d.active && d.currentVol <= 0f && d.targetVol <= 0f)
        {
            d.intro.Stop();
            d.loop.Stop();
            d.active = false;
            d.track = null;
        }
    }

    private void ApplyVolumes()
    {
        float master = isMuted ? 0f : masterVolume * duckMul;
        ApplyDeckVolume(deckA, master);
        ApplyDeckVolume(deckB, master);
    }

    private void ApplyDeckVolume(Deck d, float master)
    {
        if (d == null) return;
        float v = master * d.currentVol * (d.track != null ? d.track.volume : 1f);
        d.intro.volume = v;
        d.loop.volume = v;
    }

    private void UpdateBossDetection(float dt)
    {
        // 撃破フェードアウト
        if (fadeOutOnBossDefeat && bossStarted && !bossDefeatHandled
            && SC_Field.Instance != null && SC_Field.Instance.IsBossDefeated())
        {
            bossDefeatHandled = true;
            Stop(bossDefeatFadeTime);
            if (debugLog) Debug.Log("[BGM] ボス撃破 → フェードアウト");
        }

        if (!autoDetectBoss || bossStarted) return;

        bossCheckTimer -= dt;
        if (bossCheckTimer > 0f) return;
        bossCheckTimer = bossCheckInterval;

        GameObject boss = null;
        try { boss = GameObject.FindGameObjectWithTag(bossTag); }
        catch (UnityException) { autoDetectBoss = false; Debug.LogWarning("[BGM] タグ '" + bossTag + "' が未定義のためボス検知を停止"); return; }

        if (boss == null) return;

        bossStarted = true;
        if (debugLog) Debug.Log("[BGM] ボス出現を検知 → " + bossTrackId);
        Play(bossTrackId, bossStartDelay);
    }

    //================================================================
    // 公開API
    //================================================================

    /// <summary>曲を再生する。delayを指定するとその秒数後に開始する</summary>
    public void Play(string trackId, float delay = 0f)
    {
        if (string.IsNullOrEmpty(trackId)) return;

        if (delay > 0f)
        {
            pendingTrackId = trackId;
            pendingDelay = delay;
            return;
        }
        PlayImmediate(trackId);
    }

    /// <summary>再生中の曲をフェードアウトして止める。fadeTime省略時は曲の設定値</summary>
    public void Stop(float fadeTime = -1f)
    {
        pendingDelay = -1f;
        FadeOutDeck(current, fadeTime);
        current = null;
    }

    /// <summary>一時停止（ポーズメニュー用）</summary>
    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        pauseDsp = AudioSettings.dspTime;
        PauseDeck(deckA);
        PauseDeck(deckB);
    }

    /// <summary>一時停止から復帰</summary>
    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        double shift = AudioSettings.dspTime - pauseDsp;
        ResumeDeck(deckA, shift);
        ResumeDeck(deckB, shift);
    }

    /// <summary>BGM全体の音量を設定（0〜1）</summary>
    public void SetVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        if (saveVolumeToPrefs)
        {
            PlayerPrefs.SetFloat(volumePrefsKey, masterVolume);
            PlayerPrefs.Save();
        }
    }

    public float GetVolume() => masterVolume;

    public void SetMute(bool mute) => isMuted = mute;

    public bool IsMuted() => isMuted;

    /// <summary>一時的に音量を下げる（セリフ・演出用）。volume=1で元に戻す</summary>
    public void Duck(float volume, float fadeTime = 0.2f)
    {
        duckTarget = Mathf.Clamp01(volume);
        duckRate = fadeTime > 0f ? 1f / fadeTime : 0f;
    }

    /// <summary>雑魚戦BGMを開始する（PF_Doorの吹っ飛ばしから呼ばれる）</summary>
    public void StartBattleBgm()
    {
        if (battleStarted) return;
        battleStarted = true;

        if (debugLog) Debug.Log("[BGM] ドア破壊 → 戦闘BGM開始");

        // Beforeを指定のフェードで切る
        if (current != null && current.track != null && current.track.id == beforeBattleTrackId)
            FadeOutDeck(current, beforeFadeOutTime);

        Play(battleTrackId, battleStartDelay);
    }

    //================================================================
    // 再生処理
    //================================================================
    private void PlayImmediate(string trackId)
    {
        BgmTrack track = FindTrack(trackId);
        if (track == null)
        {
            Debug.LogWarning("[BGM] 曲が見つかりません : " + trackId);
            return;
        }
        if (track.loopClip == null)
        {
            Debug.LogWarning("[BGM] Loop Clipが未設定です : " + trackId);
            return;
        }

        // 同じ曲が既に鳴っているなら何もしない（シーンをまたいでも途切れない）
        if (!restartOnSameTrack && current != null && current.track != null
            && current.track.id == track.id && current.IsPlaying)
            return;

        Deck next = (current == deckA) ? deckB : deckA;

        // 切替先が使用中なら強制的に止める
        next.intro.Stop();
        next.loop.Stop();

        FadeOutDeck(current, -1f);   // 今の曲は自身のfadeOutTimeで消す

        double start = AudioSettings.dspTime + Mathf.Max(0.01f, scheduleLeadTime);

        next.track = track;
        next.active = true;
        next.currentVol = track.fadeInTime > 0f ? 0f : 1f;
        next.targetVol = 1f;
        next.fadeRate = track.fadeInTime > 0f ? 1f / track.fadeInTime : 0f;

        if (track.introClip != null)
        {
            next.intro.clip = track.introClip;
            next.intro.loop = false;
            next.introStartDsp = start;
            next.intro.PlayScheduled(start);

            // イントロの正確な長さはサンプル数から求める（秒数のlengthは丸め誤差が出る）
            double introLen = (double)track.introClip.samples / track.introClip.frequency;
            double loopStart = start + introLen + track.introToLoopOffset;

            next.loop.clip = track.loopClip;
            next.loop.loop = track.loop;
            next.loopStartDsp = loopStart;
            next.loop.PlayScheduled(loopStart);

            // イントロをループ開始と同時に確実に終わらせる（重なり・伸びの防止）
            next.intro.SetScheduledEndTime(loopStart);
        }
        else
        {
            next.intro.clip = null;
            next.introStartDsp = 0;

            next.loop.clip = track.loopClip;
            next.loop.loop = track.loop;
            next.loopStartDsp = start;
            next.loop.PlayScheduled(start);
        }

        current = next;
        ApplyVolumes();

        if (debugLog) Debug.Log("[BGM] Play : " + track.id);
    }

    private void FadeOutDeck(Deck d, float fadeTime)
    {
        if (d == null || !d.active) return;

        float t = fadeTime >= 0f ? fadeTime
                : (d.track != null ? d.track.fadeOutTime : 0f);

        d.targetVol = 0f;
        d.fadeRate = t > 0f ? 1f / t : 0f;

        if (t <= 0f)
        {
            d.currentVol = 0f;
            d.intro.Stop();
            d.loop.Stop();
            d.active = false;
            d.track = null;
        }
    }

    private void PauseDeck(Deck d)
    {
        if (d == null || !d.active) return;
        if (d.intro.isPlaying) d.intro.Pause();
        if (d.loop.isPlaying) d.loop.Pause();
    }

    private void ResumeDeck(Deck d, double shift)
    {
        if (d == null || !d.active) return;

        // まだ開始していない予約は、止まっていた時間だけ後ろへずらす
        if (d.intro.clip != null && d.introStartDsp > pauseDsp)
        {
            d.introStartDsp += shift;
            d.intro.SetScheduledStartTime(d.introStartDsp);
        }
        if (d.loopStartDsp > pauseDsp)
        {
            d.loopStartDsp += shift;
            d.loop.SetScheduledStartTime(d.loopStartDsp);
        }

        d.intro.UnPause();
        d.loop.UnPause();
    }

    private BgmTrack FindTrack(string id)
    {
        foreach (var t in tracks)
            if (t != null && t.id == id) return t;
        return null;
    }

    //================================================================
    // シーン連動
    //================================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンをまたいだのでゲーム内フラグをリセット
        battleStarted = false;
        bossStarted = false;
        bossDefeatHandled = false;
        bossCheckTimer = 0f;
        Duck(1f, 0f);

        ApplySceneTrack(scene.name);
    }

    private void ApplySceneTrack(string sceneName)
    {
        SceneBgmEntry entry = FindSceneEntry(sceneName);

        if (entry == null)
        {
            if (stopIfSceneNotMapped) Stop();
            return;
        }

        string id = entry.trackId;

        // Resultシーン用：クリア済みなら勝利曲へ差し替える
        if (!string.IsNullOrEmpty(entry.trackIdOnCleared) && GameData.Result.IsCleared)
            id = entry.trackIdOnCleared;

        Play(id, entry.startDelay);
    }

    // 'Scene Game' と 'Scene_Game' の表記ゆれを吸収して照合する
    private SceneBgmEntry FindSceneEntry(string sceneName)
    {
        foreach (var e in sceneTracks)
            if (e != null && e.sceneName == sceneName) return e;

        string key = Normalize(sceneName);
        foreach (var e in sceneTracks)
            if (e != null && Normalize(e.sceneName) == key) return e;

        return null;
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace(" ", "").Replace("_", "").ToLowerInvariant();
    }

    //================================================================
    // エディタ確認用
    //================================================================
    [ContextMenu("Test / Play Battle")]
    private void TestBattle() => StartBattleBgm();

    [ContextMenu("Test / Stop")]
    private void TestStop() => Stop();
}