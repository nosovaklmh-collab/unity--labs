using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Красная вспышка на весь экран. Вызывается через UnityEvent OnDied из инспектора.
public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;    // FlashOverlay
    [SerializeField] private float duration = 0.5f;

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Сразу делаем оверлей непрозрачным
        Color c = flashImage.color;
        c.a = 1f;
        flashImage.color = c;

        // Плавно растворяем за duration секунд
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = 1f - (t / duration);
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
    }
}
