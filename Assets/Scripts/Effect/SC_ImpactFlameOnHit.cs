using UnityEngine;

/// 敵に付けるだけで完結するインパクトフレーム。
/// ふっとばし開始＝強攻撃ヒットとみなし、自分の位置でエフェクト＋ヒットストップを出す。
/// ※プレハブの Stop Action = Destroy にしておくこと（自動で消える）。
[RequireComponent(typeof(SC_EnemyStatusManager))]
public class SC_ImpactFrameOnHit : MonoBehaviour
{
    [SerializeField] private GameObject impactFramePrefab; // VFX_Impact_Frame_01

    private SC_EnemyStatusManager _enemy;
    private bool _prevBlownAway;

    void Awake() => _enemy = GetComponent<SC_EnemyStatusManager>();

    void OnEnable()
    {
        if (_enemy != null) _prevBlownAway = _enemy.IsBlownAway();
    }

    void LateUpdate()
    {
        if (_enemy == null) return;

        bool blown = _enemy.IsBlownAway();
        if (blown && !_prevBlownAway)
        {
            SC_HitStop.Instance?.Trigger();
            if (impactFramePrefab != null)
                Instantiate(impactFramePrefab, transform.position, Quaternion.identity);
        }
        _prevBlownAway = blown;
    }
}