using UnityEngine;

// Доп. задание 2: мерцание лампы — эффект аварийного освещения.
// Повесьте на Point Light. Работает только для Realtime/Mixed света
// (запечённый свет в рантайме не меняется).
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public float baseIntensity = 2f;
    public float amplitude = 1.2f;    // насколько сильно мерцает
    public float speed = 6f;          // частота основной синусоиды
    public bool randomGlitches = true; // редкие «провалы», как у неисправной лампы

    private Light lamp;

    void Awake() { lamp = GetComponent<Light>(); }

    void Update()
    {
        float t = Time.time * speed;
        // Сумма двух синусоид разной частоты — мерцание выглядит неравномерным
        float wave = Mathf.Sin(t) * 0.6f + Mathf.Sin(t * 2.7f) * 0.4f;
        float intensity = baseIntensity + wave * amplitude;

        if (randomGlitches && Random.value < 0.02f) intensity = 0.1f;   // короткий провал

        lamp.intensity = Mathf.Max(0f, intensity);
    }
}
