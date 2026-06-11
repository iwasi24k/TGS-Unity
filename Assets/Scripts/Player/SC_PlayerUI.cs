using UnityEngine;
using UnityEngine.UI;

public class SC_PlayerUI : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private Image hpPointPrefab;
    [SerializeField] private Sprite hpPointAlive;
    [SerializeField] private Sprite hpPointDead;
    
    private Image[] hpPoints;
    public void InitializeHPUI(SC_PlayerHP OwnerHP)
    {
        //HPÇÃUIÇèâä˙âª
        hpPoints = new Image[OwnerHP.GetMaxHP()];
        if (hpPointPrefab != null)
        {
            for (int i = 0; i < OwnerHP.GetMaxHP(); i++)
            {
                //HPÇÃUIÇê∂ê¨
                hpPoints[i] = Instantiate(hpPointPrefab, transform);
            }
        }

        UpdateHPUI(OwnerHP);
    }

    public void UpdateHPUI(SC_PlayerHP OwnerHP)
    {
        for (int i = 0; i < OwnerHP.GetMaxHP(); i++)
        {
            if (i < OwnerHP.GetCurrentHP())
            {
                hpPoints[i].sprite = hpPointAlive;
            }
            else
            {
                hpPoints[i].sprite = hpPointDead;
            }
        }
    }

}
