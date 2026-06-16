using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_PlayerHP : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private SC_PlayerUI playerUI;
    [SerializeField] private SC_PlayerKnockback Knockback;

    [Header("HP Settings")]
    [SerializeField] private int maxHP = 10;
    public int GetMaxHP() => maxHP;

    private int currentHP;
    public int GetCurrentHP() => currentHP;

    private void Awake()
    {
        currentHP = maxHP;
        
        if(!Knockback) Knockback = GetComponent<SC_PlayerKnockback>();

        if (playerUI)
        {
            playerUI.InitializeHPUI(this);
        }

    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        if(playerUI)
        {
            playerUI.UpdateHPUI(this);
        }

        if(Knockback)
        {
            Knockback.AddKnockback(-this.transform.forward, 0.1f, 0.05f, false);
        }

        if(currentHP <= 0)
        {
            // ƒvƒŒƒCƒ„[‚ªŽ€–S‚µ‚½‚Æ‚«‚Ìˆ—
            SceneManager.LoadScene("Scene_Result");
        }
    }

    public void Heal(int value)
    {
        currentHP += value;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        if (playerUI)
        {
            playerUI.UpdateHPUI(this);
        }
    }
}