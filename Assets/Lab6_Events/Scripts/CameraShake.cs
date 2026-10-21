using System.Collections;
using UnityEngine;

// Вариант 9: при получении урона камера коротко трясётся.
// Подписывается на то же событие OnDamaged, что и UI.
// Выполняется после CameraFollow, поэтому тряска добавляется поверх следования.
[DefaultExecutionOrder(100)]
public class CameraShake : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private float duration = 0.25f;   // сколько трясётся, сек
    [SerializeField] private float strength = 0.2f;    // сила тряски, м

    private float timeLeft;

    void OnEnable() { player.OnDamaged += Shake; }    // подписка
    void OnDisable() { player.OnDamaged -= Shake; }    // отписка

    void Shake(int currentHP)
    {
        timeLeft = duration;
    }

    void LateUpdate()
    {
        if (timeLeft <= 0f) return;
        timeLeft -= Time.deltaTime;

        // Случайный сдвиг, затухающий к концу тряски
        float k = timeLeft / duration;
        transform.position += Random.insideUnitSphere * strength * k;
    }
}