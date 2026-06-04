using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField]
    private float fadeDuration = 0.5f;

    private TransitionEffect transitionEffect;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CreateUI();

        transitionEffect =
            new FadeEffect(canvasGroup, fadeDuration);
    }

    private void CreateUI()
    {
        GameObject canvasObject = new GameObject("FadeCanvas");

        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("FadeImage");

        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect =
            imageObject.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();

        image.color = Color.black;

        canvasGroup =
            imageObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    public static IEnumerator FadeOut()
    {
        yield return Instance.transitionEffect.PlayOut();
    }

    public static IEnumerator FadeIn()
    {
        yield return Instance.transitionEffect.PlayIn();
    }
}