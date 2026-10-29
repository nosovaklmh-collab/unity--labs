using System;
using UnityEngine;

// «Лампада» — маленькая лампа в руках девочки.
// F — зажечь / погасить. Пламя слегка мерцает.
// Свет помогает видеть подсказки, но враг замечает освещённую героиню
// (это связь с ЛР7 «Освещение и скрытность»).
//
// Вешается на Player. В поле Lamp Light перетащите Point Light (дочерний объект у руки).
public class LampadaLamp : MonoBehaviour
{
    public Light lampLight;
    public KeyCode toggleKey = KeyCode.F;
    public bool startLit = true;

    [Header("Мерцание пламени")]
    public float baseIntensity = 2f;
    public float flickerAmount = 0.25f;  // насколько сильно «дышит» огонь
    public float flickerSpeed = 3f;

    // Событие для других скриптов (враг, UI, звук) — как в лекции про события
    public event Action<bool> OnLampToggled;

    public bool IsLit { get; private set; }

    void Start()
    {
        SetLit(startLit);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetLit(!IsLit);

        // Мерцание через шум Перлина — выглядит как живой огонь, а не как мигание
        if (IsLit && lampLight != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) - 0.5f;
            lampLight.intensity = baseIntensity + noise * 2f * flickerAmount * baseIntensity;
        }
    }

    public void SetLit(bool lit)
    {
        IsLit = lit;
        if (lampLight != null) lampLight.enabled = lit;
        OnLampToggled?.Invoke(lit);
    }
}
