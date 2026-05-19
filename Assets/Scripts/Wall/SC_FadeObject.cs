using UnityEngine;

public class SC_FadeObject : MonoBehaviour
{
    private Material mat;

    [SerializeField] private float fadeAlpha = 0.2f;
    [SerializeField] private float fadeSpeed = 10f;

    private float targetAlpha = 1f;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        // URP Transparent‰»
        mat.SetFloat("_Surface", 1);

        mat.renderQueue = 3000;
    }

    void Update()
    {
        Color color = mat.color;

        color.a = Mathf.Lerp(
            color.a,
            targetAlpha,
            Time.deltaTime * fadeSpeed
        );

        mat.color = color;
    }

    public void FadeOut()
    {
        targetAlpha = fadeAlpha;
    }

    public void FadeIn()
    {
        targetAlpha = 1f;
    }
}