using UnityEngine;
using UnityEngine.UIElements;

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
    [SerializeField] Slider hpSlider;

    [Header("Enemy Status")]
    [SerializeField] private int HP = 100;
    
    private EnemyState currentPerformance = EnemyState.Idle;
    void Start()
    {
        if(hpSlider == null)
        {
            Debug.LogError("HPスライダーがアタッチされていません。");
        }
        else
        {
            hpSlider.highValue = hpSlider.value = HP;
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
