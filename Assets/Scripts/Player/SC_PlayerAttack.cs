using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerAttack : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("攻撃用インプットアクション(弱)")]
    [SerializeField] private InputActionReference iaWeakAttack;
    [Tooltip("攻撃用インプットアクション(強)")]
    [SerializeField] private InputActionReference iaStrongAttack;

    [Header("Settings")]
    [Tooltip("攻撃のクールダウン時間")]
    [SerializeField] private float attackCooldown = 1f;
    [Tooltip("攻撃のダメージ量(弱)")]
    [SerializeField] private int weakAttackDamage = 10;
    [Tooltip("攻撃のダメージ量(強)")]
    [SerializeField] private int strongAttackDamage = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");

        var weakAttackInput = iaWeakAttack.action.WasPressedThisFrame();
        var strongAttackInput = iaStrongAttack.action.WasPressedThisFrame();

        if(weakAttackInput)
        {
            //弱攻撃の処理
            if(enemys.Length > 0)
            {
                Debug.Log("弱攻撃が発動しました。");
                foreach (var enemy in enemys)
                {
                    //敵にダメージを与える処理
                    var enemyStatus = enemy.GetComponent<SC_EnemyStatusManager>();
                    if(enemyStatus != null)
                    {
                        enemyStatus.TakeDamage(weakAttackDamage);
                    }
                }
            }

        }

        if(strongAttackInput)
        {
            //強攻撃の処理
            if(enemys.Length > 0)
            {
                Debug.Log("強攻撃が発動しました。");
                foreach (var enemy in enemys)
                {
                    //敵にダメージを与える処理
                    var enemyStatus = enemy.GetComponent<SC_EnemyStatusManager>();
                    if(enemyStatus != null)
                    {
                        enemyStatus.TakeDamage(strongAttackDamage);
                    }
                }
            }
        }
    }
}
