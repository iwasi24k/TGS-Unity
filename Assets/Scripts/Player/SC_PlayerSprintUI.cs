using UnityEngine;
using UnityEngine.UI;

public class SC_PlayerSprintUI : MonoBehaviour
{
    [SerializeField] private Slider sprintSlider;
    [SerializeField] private Image Fill;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void InitializeUI(SC_PlayerSprintManager Owner)
    {
        if (!sprintSlider)
        {
            sprintSlider = GetComponent<Slider>();
        }
        if (!Fill)
        {
            Fill = sprintSlider.fillRect.GetComponent<Image>();
        }

        sprintSlider.minValue = 0f;
        sprintSlider.maxValue = Owner.GetBoostCoolTime();

    }

    public void UpdateUI(SC_PlayerSprintManager Owner)
    {
        if (sprintSlider)
        {
            sprintSlider.value = Owner.GetBoostCoolTime() - Owner.GetCurrentBoostCoolTime();
        }

        if(sprintSlider.value >= sprintSlider.maxValue)
        {
            sprintSlider.value = sprintSlider.maxValue;
            Fill.color = Color.orange;
        }
        else
        {
            Fill.color = Color.yellow;
        }
    }
}
