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

    private bool isStar = false;
    public void SetStar(bool value) => isStar = value;

    private void Awake()
    {
        currentHP = maxHP;
        
        if(!Knockback) Knockback = GetComponent<SC_PlayerKnockback>();

        if (playerUI)
        {
            playerUI.InitializeHPUI(this);
        }

    }

    public void TakeDamage(int damage, Vector3 sourcePsition)
    {
        Vector3 lookpos = sourcePsition;
        lookpos.y = transform.position.y; // プレイヤーの高さに合わせる
        transform.LookAt(lookpos);

        if (isStar)
        {
            return;
        }

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
            // プレイヤーが死亡したときの処理
            SceneManager.LoadScene("Scene Result");
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

        SC_EffectManager.Instance.PlayEffect("Heal", this.transform.position);
    }
}