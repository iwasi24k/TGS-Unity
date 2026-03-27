using UnityEngine;
using UnityEngine.UI;

public enum EnemyState
{
    Idle,
    Walk,
    Attack,
    BlowAway,
}

public class SC_EnemyStatusManager : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("HPSlider")]
    [SerializeField]  private Slider hpSlider;

    [Header("Enemy Status")]
    [SerializeField] private int HP = 100;
    [Tooltip("敵の吹っ飛ぶ力")]
    [SerializeField] private float BlowAwayPower = 20f;

    private EnemyState currentPerformance = EnemyState.Idle;
    void Start()
    {
        if(hpSlider == null)
        {
            Debug.LogError("HPスライダーがアタッチされていません。");
        }
        else
        {
            hpSlider.maxValue = hpSlider.value = HP;
        }
    }

    void Update()
    {
        switch (currentPerformance)
        {
            case EnemyState.Idle:
                // 待機状態の処理
                break;
            case EnemyState.Walk:
                // 歩行状態の処理
                break;
            case EnemyState.Attack:
                // 攻撃状態の処理
                break;
            case EnemyState.BlowAway:
                // 吹っ飛び状態の処理
                break;
        }
    }

    /* : 以下、各ステータスの管理用関数。　外部から呼び出して仕様。 : */
    public int GetHP()
    {
        return HP;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        hpSlider.value = HP;

        if (HP < 0)
        {
            HP = 0;
        }
    }

    public void SetEnemyState(EnemyState next)
    {
        currentPerformance = next;
    }

}
