using UnityEngine;
using UnityEngine.UI;

// UI здоровья: подписывается на события игрока и обновляет текст и полоску.
// Про шипы и аптечки ничего не знает.
public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private Text hpText;     // UI → Legacy → Text
    [SerializeField] private Image hpBar;     // Image Type = Filled

    [Header("Вариант 5: цвет полоски")]
    [SerializeField] private int lowHPThreshold = 30;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color lowColor = Color.red;

    void OnEnable()
    {
        // Подписка: «когда HP изменится — вызови мой UpdateHP»
        player.OnDamaged += UpdateHP;
        player.OnHealed += UpdateHP;      // вариант 2: тот же обработчик и для лечения
    }

    void OnDisable()
    {
        // Отписка — парная к каждой подписке, иначе утечка памяти
        player.OnDamaged -= UpdateHP;
        player.OnHealed -= UpdateHP;
    }

    void Start()
    {
        // Показываем стартовое HP, пока событий ещё не было
        UpdateHP(player.CurrentHP);
    }

    void UpdateHP(int currentHP)
    {
        hpText.text = $"HP: {currentHP}";
        hpBar.fillAmount = (float)currentHP / player.MaxHP;

        // Вариант 5: при HP < 30 полоска красная
        hpBar.color = currentHP < lowHPThreshold ? lowColor : normalColor;
    }
}