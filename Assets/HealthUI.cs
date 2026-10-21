using UnityEngine;
using UnityEngine.UI;

// UI здоровья: подписывается на событие игрока и обновляет текст и полоску.
// Про шипы ничего не знает.
public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private Text hpText;     // UI → Legacy → Text
    [SerializeField] private Image hpBar;     // Image Type = Filled

    [SerializeField] private int maxHP = 100;

    void OnEnable()
    {
        // Подписка: «когда игрок получит урон — вызови мой UpdateHP»
        player.OnDamaged += UpdateHP;
    }

    void OnDisable()
    {
        // Отписка — парная к подписке, иначе утечка памяти
        player.OnDamaged -= UpdateHP;
    }

    void Start()
    {
        // Показываем стартовое HP, пока урона ещё не было
        UpdateHP(player.CurrentHP);
    }

    void UpdateHP(int currentHP)
    {
        hpText.text = $"HP: {currentHP}";
        hpBar.fillAmount = (float)currentHP / maxHP;
    }
}
