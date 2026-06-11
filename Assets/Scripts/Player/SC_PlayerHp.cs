using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SC_PlayerHP : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Animator animator;

    [Header("HP Settings")]
    [SerializeField] private int maxHP = 10;

    private int currentHP;
    public int CurrentHP => currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;

            //‚±‚±‚ÅI—¹ˆ—(”s–k)

        }
        animator.SetTrigger("tKnockback");
    }

    public void Heal(int value)
    {
        currentHP += value;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }
}