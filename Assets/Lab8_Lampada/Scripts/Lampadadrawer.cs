using UnityEngine;
using UnityEngine.Events;

// «Лампада» — ящик комода. E — выдвинуть, ещё раз E — задвинуть.
// В одном ящике ключ (Has Key): ключ выдаётся один раз, это флаг у игрока (LampadaFlags).
// Ящик может быть заперт (Required Flag): например, на кодовый замок,
// и откроется, только когда девочка увидела код на стене (флаг "code_seen").
//
// Вешается на ящик (кубик с коллайдером).
public class LampadaDrawer : MonoBehaviour, IInteractable
{
    public bool hasKey = false;
    public string giveFlag = "key_exit";            // какой флаг получает девочка

    [Header("Замок (необязательно)")]
    public string requiredFlag = "";                // пусто — не заперт; "code_seen" — нужен код со стены
    public string lockedText = "Заперто. На замке крошечные цифры...";

    [Header("Движение")]
    public Vector3 openOffset = new Vector3(0f, 0f, -0.35f); // куда выдвигается (к камере)
    public float openSpeed = 1f;

    [Header("Подсказки")]
    public string emptyText = "Пусто...";
    public string foundText = "Внутри ключ!";

    public UnityEvent OnOpened;                     // например, звук скрипа
    public UnityEvent OnClosed;

    private bool isOpen;
    private bool keyTaken;
    private Vector3 closedPosition;
    private string message;
    private float messageTimer;

    void Start()
    {
        closedPosition = transform.position;
    }

    public void Interact(GameObject actor)
    {
        var flags = actor.GetComponent<LampadaFlags>();

        // Уже открыт — просто задвигаем
        if (isOpen)
        {
            isOpen = false;
            OnClosed?.Invoke();
            return;
        }

        // Заперт и кода ещё нет
        if (!string.IsNullOrEmpty(requiredFlag) && (flags == null || !flags.Has(requiredFlag)))
        {
            Show(lockedText);
            return;
        }

        isOpen = true;
        OnOpened?.Invoke();

        if (hasKey && !keyTaken)
        {
            keyTaken = true;
            if (flags != null) flags.Add(giveFlag);
            else Debug.LogWarning("Лампада: на Player нет скрипта LampadaFlags — ключ некуда положить");
            Show(foundText);
        }
        else
        {
            Show(emptyText);
        }
    }

    void Update()
    {
        Vector3 target = isOpen ? closedPosition + openOffset : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
        if (messageTimer > 0f) messageTimer -= Time.deltaTime;
    }

    void Show(string text)
    {
        message = text;
        messageTimer = 2.5f;
    }

    void OnGUI()
    {
        if (messageTimer <= 0f) return;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        style.normal.textColor = new Color(1f, 0.85f, 0.6f);
        GUI.Label(new Rect(0, Screen.height - 130, Screen.width, 34), message, style);
    }
}
