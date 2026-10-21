using UnityEngine;

// Шипы: при касании наносят урон игроку. Про UI ничего не знают.
public class Spikes : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;   // урон за одно срабатывание
    [SerializeField] private float cooldown = 1f;     // пауза между ударами, сек

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

        // Единственное, что шипы знают об игроке, — у него есть PlayerHealth
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);
            timeLeft = cooldown;
        }
    }
}
