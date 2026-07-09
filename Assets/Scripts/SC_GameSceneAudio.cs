using UnityEngine;

public class SC_GameSceneAudio : MonoBehaviour
{
    public static SC_GameSceneAudio Instance { get; private set; }

    [Header("BGM(3曲をここにアサイン)")]
    [Tooltip("戦闘前のループBGM")]
    [SerializeField] private AudioClip beforeBattleClip; // BeforeBattle_Demo
    [Tooltip("戦闘BGMのイントロ(1回だけ再生)")]
    [SerializeField] private AudioClip battleIntroClip;  // Battle_Intr_Demo
    [Tooltip("戦闘BGMの本体(ループ再生)")]
    [SerializeField] private AudioClip battleLoopClip;   // Battle_Demo

    private AudioSource loopSource;  // BeforeBattle / Battle本体の再生用
    private AudioSource introSource; // イントロ専用
    private bool battleStarted;

    void Awake()
    {
        Instance = this;

        loopSource = CreateSource("BGM_Loop");
        introSource = CreateSource("BGM_Intro");

        // 事前ロード
        if (battleIntroClip != null) battleIntroClip.LoadAudioData();

        // 戦闘前BGMをループ再生開始
        if (beforeBattleClip != null)
        {
            loopSource.clip = beforeBattleClip;
            loopSource.loop = true;
            loopSource.PlayScheduled(AudioSettings.dspTime + 0.1);
        }
    }

    public void StartBattleBgm()
    {
        if (battleStarted) return;
        if (battleIntroClip == null || battleLoopClip == null) return;
        battleStarted = true;

        loopSource.Stop(); // BeforeBattleを止める

        double startTime = AudioSettings.dspTime + 0.1;

        double introLength = (double)battleIntroClip.samples / battleIntroClip.frequency;

        introSource.clip = battleIntroClip;
        introSource.loop = false;
        introSource.PlayScheduled(startTime);

        loopSource.clip = battleLoopClip;
        loopSource.loop = true;
        loopSource.PlayScheduled(startTime + introLength);
    }

    private AudioSource CreateSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        return src;
    }
}