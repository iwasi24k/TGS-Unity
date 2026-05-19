using UnityEngine;
using TMPro;

public class SC_ComboUI : MonoBehaviour
{
    [SerializeField] private TMP_Text comboText;

    private void Update()
    {
        if (ComboManager.Instance == null) return;

        int combo = ComboManager.Instance.ComboCount;

        // コンボ0なら非表示
        if (combo <= 0)
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        comboText.gameObject.SetActive(true);

        // 表示更新
        comboText.text = combo + " Combo!";
    }
}
