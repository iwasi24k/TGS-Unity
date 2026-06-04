using UnityEngine;

public class SC_MeleeWave : MonoBehaviour
{
    [Tooltip("球体が最大サイズになるまでの時間"), SerializeField]
    private float expandTime = 0.3f;

    [Tooltip("最大サイズになった後、消えるまでの時間"), SerializeField]
    private float fadeTime = 0.2f;

    [Tooltip("最終的な半径"), SerializeField]
    private float targetRadius = 5.0f;

    [Tooltip("開始時の透明度"), SerializeField]
    private float startAlpha = 0.35f;

    private float timer;
    private Renderer renderComponent;
    private Material materialInstance;
    private Color startColor;

    void Start()
    {
        timer = 0f;

        renderComponent = GetComponent<Renderer>();

        if (renderComponent != null)
        {
            materialInstance = renderComponent.material;
            startColor = materialInstance.color;
            startColor.a = startAlpha;
            materialInstance.color = startColor;
        }

        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float totalTime = expandTime + fadeTime;

        if (timer <= expandTime)
        {
            float t = timer / expandTime;
            float diameter = targetRadius * 2.0f;

            transform.localScale = Vector3.one * diameter * t;
        }
        else
        {
            float t = (timer - expandTime) / fadeTime;
            t = Mathf.Clamp01(t);

            float diameter = targetRadius * 2.0f;
            transform.localScale = Vector3.one * diameter;

            if (materialInstance != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startAlpha, 0f, t);
                materialInstance.color = color;
            }
        }

        if (timer >= totalTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetRadius(float radius)
    {
        targetRadius = radius;
    }
}
