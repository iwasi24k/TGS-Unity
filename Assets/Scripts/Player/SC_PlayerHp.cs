using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_PlayerHP : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Animator animator;

    [Header("HP Settings")]
    [SerializeField] private int maxHP = 10;
    public int GetMaxHP() => maxHP;

    private int currentHP;
    public int GetCurrentHP() => currentHP;

    private void Awake()
    {
        currentHP = maxHP;
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"currentHP: {currentHP}, damage: {damage}");
        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;

            Debug.Log("Player is defeated!");
            //‚±‚±‚ÅI—¹ˆ—(”s–k)
            SceneManager.LoadScene("Scene_Result");
        }
        if(animator)
        { 
            animator.SetTrigger("tKnockback");
        }
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