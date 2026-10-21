using UnityEngine;
using UnityEngine.Events;

// Здоровье игрока. Ничего не знает ни про UI, ни про шипы —
// только объявляет события, а кто хочет, тот подписывается.
public class PlayerHealth : MonoBehaviour
{
    // C#-событие: «я получил урон, вот моё новое HP».
    // event = снаружи можно только += и -=, вызвать может только этот класс.
    public event System.Action<int> OnDamaged;

    // UnityEvent: «я умер». Видно и настраивается в инспекторе мышью.
    public UnityEvent OnDied;

    [SerializeField] private int hp = 100;   // видно в инспекторе

    public int CurrentHP => hp;              // UI узнаёт стартовое значение

    public void TakeDamage(int amount)
    {
        if (hp <= 0) return;                 // уже мёртв — урон игнорируем
        hp -= amount;
        if (hp < 0) hp = 0;

        // Сообщаем всем подписчикам новое HP (?. — если подписчиков нет, ошибки не будет)
        OnDamaged?.Invoke(hp);

        // При смерти вызываем UnityEvent
        if (hp == 0) OnDied?.Invoke();
    }
}
