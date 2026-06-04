using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : TransitionEffect
{
    private readonly CanvasGroup canvasGroup;

    private readonly float duration;

    public FadeEffect(CanvasGroup canvasGroup, float duration)
    {
        this.canvasGroup = canvasGroup;
        this.duration = duration;
    }

    public override IEnumerator PlayOut()
    {
        yield return Fade(0f, 1f);
    }

    public override IEnumerator PlayIn()
    {
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float start, float end)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(start, end, t);

            yield return null;
        }

        canvasGroup.alpha = end;
    }
}