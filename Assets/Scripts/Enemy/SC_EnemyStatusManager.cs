using UnityEngine;
using UnityEngine.UI;

public class SC_EnemyStatusManager : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("HPSlider")]
    [SerializeField]  private Slider hpSlider;

    [Header("Enemy Status")]
    [SerializeField] private int HP = 100;

    [Header("State")]
    [Tooltip("初期状態のState"),SerializeField] private SC_EnemyBaceState initialState;

    private SC_EnemyBaceState currentState;

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

        //SC_EnemyBaceState initialState = GetComponent<SC_EnemyWalk>();
        //if (initialState == null)
        //{
        //    Debug.LogError("初期状態のStateがアタッチされていません。");
        //}

        //TransitionTo(initialState);
    }

    void Update()
    {
        currentState.UpdateState(this);
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

    public void TransitionTo(SC_EnemyBaceState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }
        SC_EnemyBaceState nextState = Instantiate(newState);
        currentState = nextState;
        currentState.Enter(this);
    }
}
