using UnityEngine;
using UnityEngine.Events;

// Ловушка: при касании наносит урон игроку. Про UI ничего не знает.
// Один и тот же скрипт используется и для шипов (10 урона, 1 с),
// и для огня (вариант 1: 20 урона, 2 с) — меняются только числа в инспекторе.
public class Spikes : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;   // урон за одно срабатывание
    [SerializeField] private float cooldown = 1f;     // пауза между ударами, сек

    // Вариант 6: событие срабатывания ловушки — в инспекторе к нему
    // подключается AudioSource.Play(), чтобы играл звук
    public UnityEvent OnTriggered;

    private float timeLeft = 0f;

    void Update()
    {
        if (timeLeft > 0f) timeLeft -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)   // игрок вошёл в зону — один раз
    {
        TryDamage(other);
    }

    void OnTriggerStay(Collider other)    // каждый кадр, пока игрок внутри
    {
        // бьём снова, только когда прошла задержка
        if (timeLeft <= 0f) TryDamage(other);
    }

    void TryDamage(Collider other)
    {
        if (timeLeft > 0f) return;

        // Единственное, что ловушка знает об игроке, — у него есть PlayerHealth
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);
            timeLeft = cooldown;
            OnTriggered?.Invoke();        // вариант 6: звук и любые другие эффекты
        }
    }
}