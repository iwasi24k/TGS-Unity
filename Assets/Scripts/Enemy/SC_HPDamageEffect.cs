using UnityEngine;
using UnityEngine.UI;

public class SC_HPDamageEffect : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider currentSlider;
    [SerializeField] private Slider damageSlider;

    [Header("Animation")]
    [SerializeField] private float delay = 0.3f;
    [SerializeField] private float speed = 6f;

    private float timer;

    private void Start()
    {
        damageSlider.maxValue = currentSlider.maxValue;
        damageSlider.value = currentSlider.value;
    }

    private void Update()
    {
        if (currentSlider == null || damageSlider == null)
            return;

        damageSlider.maxValue = currentSlider.maxValue;

        // É_ÉÅÅ[ÉWÇéÛÇØÇΩ
        if (currentSlider.value < damageSlider.value)
        {
            timer += Time.deltaTime;

            if (timer >= delay)
            {
                damageSlider.value = Mathf.Lerp(
                    damageSlider.value,
                    currentSlider.value,
                    speed * Time.deltaTime
                );

                // ãﬂÇ√Ç¢ÇΩÇÁÇ“Ç¡ÇΩÇËçáÇÌÇπÇÈ
                if (Mathf.Abs(damageSlider.value - currentSlider.value) < 0.01f)
                {
                    damageSlider.value = currentSlider.value;
                }
            }
        }
        // âÒïú
        else
        {
            timer = 0f;
            damageSlider.value = currentSlider.value;
        }
    }
}