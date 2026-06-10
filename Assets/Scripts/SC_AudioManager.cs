using UnityEngine;

public class SC_AudioManager : MonoBehaviour
{
    public static SC_AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float bpm = 120f;

    public float BeatDuration => 60f / bpm;

    // BGMÄ¶ŠJn‚ÌDSP
    public double BgmStartDspTime { get; private set; }

    void Awake()
    {
        Instance = this;

        // Play On Awake ‚Ì‘ã‚í‚è‚É‚±‚±‚ÅÄ¶‚µ‚ÄDSP‚ğ‹L˜^
        BgmStartDspTime = AudioSettings.dspTime + 0.1;
        bgmSource.PlayScheduled(BgmStartDspTime);
    }
}