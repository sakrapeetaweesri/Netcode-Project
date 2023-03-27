using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    #region Animation
    public static IEnumerator PopCoroutine(RectTransform rect, float expectedScale, float speed)
    {
        float scale = 0f;
        rect.localScale = new Vector2(scale, scale);

        float maxScale = expectedScale * 1.3f;
        while (scale < maxScale)
        {
            scale = Mathf.Clamp(scale + Time.deltaTime * speed, 0f, maxScale);
            rect.localScale = new Vector2(scale, scale);

            yield return null;
        }

        while (scale > expectedScale)
        {
            scale = Mathf.Clamp(scale - Time.deltaTime * (speed * 0.8f), expectedScale, maxScale);
            rect.localScale = new Vector2(scale, scale);

            yield return null;
        }
    }
    public static IEnumerator SlideCoroutine(RectTransform rect, Vector2 startPoint, Vector2 destination, float speed)
    {
        rect.anchoredPosition = startPoint;

        float maxDistance = (destination - startPoint).sqrMagnitude;
        float currentDistance = 0;

        while (currentDistance < maxDistance)
        {
            Vector2 moveDir = destination - rect.anchoredPosition;
            rect.anchoredPosition += speed * Time.deltaTime * moveDir;

            currentDistance = (rect.anchoredPosition - startPoint).sqrMagnitude;

            yield return null;
        }

        rect.anchoredPosition = destination;
    }
    public static IEnumerator FadeCoroutine(CanvasGroup canvasGroup, bool fadeIn, float speed)
    {
        float alpha;
        float targetAlpha;

        if (fadeIn)
        {
            alpha = 0f;
            targetAlpha = 1f;
        }
        else
        {
            alpha = 1f;
            targetAlpha = 0f;
            speed = -speed;
        }

        canvasGroup.alpha = alpha;
        while (Mathf.Abs(alpha - targetAlpha) > 0f)
        {
            alpha = Mathf.Clamp(alpha + Time.deltaTime * speed, 0f, 1f);
            canvasGroup.alpha = alpha;

            yield return null;
        }
    }
    #endregion
}