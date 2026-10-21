using UnityEngine;
using UnityEngine.Events;

// Здоровье игрока. Ничего не знает ни про UI, ни про шипы, ни про камеру —
// только объявляет события, а кто хочет, тот подписывается.
public class PlayerHealth : MonoBehaviour
{
    // C#-событие: «я получил урон, вот моё новое HP».
    // event = снаружи можно только += и -=, вызвать может только этот класс.
    public event System.Action<int> OnDamaged;

    // Вариант 2: отдельное событие лечения
    public event System.Action<int> OnHealed;

    // UnityEvent: «я умер». Видно и настраивается в инспекторе мышью.
    public UnityEvent OnDied;

    [SerializeField] private int maxHP = 100;
    [SerializeField] private int hp = 100;   // видно в инспекторе

    public int CurrentHP => hp;              // UI узнаёт стартовое значение
    public int MaxHP => maxHP;

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

    // Вариант 2: лечение. Возвращает true, если удалось вылечить.
    public bool Heal(int amount)
    {
        if (hp <= 0 || hp >= maxHP) return false;  // мёртвого и здорового не лечим
        hp = Mathf.Min(hp + amount, maxHP);
        OnHealed?.Invoke(hp);
        return true;
    }
}