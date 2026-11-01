using UnityEngine;
using UnityEngine.Events;

// «Лампада» — головоломка «Высокая ручка».
// Ручка двери висит высоко: с пола девочка до неё не дотягивается.
// Нужно подтолкнуть ящик к двери, запрыгнуть на него и нажать E — дверь уедет вверх.
//
// Вешается на объект-ручку (у ручки должен быть коллайдер).
// В поле Door перетащите саму дверь (ручка может быть её дочерним объектом).
public class LampadaDoor : MonoBehaviour, IInteractable
{
    [Header("Дверь")]
    public Transform door;                              // что двигаем (для поворота — объект-петля)

    [Tooltip("Включено — дверь поворачивается на петлях. Выключено — уезжает вверх.")]
    public bool swing = true;
    public float openAngle = 90f;                       // на сколько градусов открывается (минус — в другую сторону)
    public float swingSpeed = 90f;                      // градусов в секунду

    public Vector3 openOffset = new Vector3(0f, 3f, 0f); // для подъёмной двери: куда уезжает
    public float openSpeed = 1.5f;

    [Header("Условие")]
    public float minPlayerHeight = 0.4f;   // ноги девочки должны быть выше этой высоты (стоит на ящике); 0 — без проверки
    public string requiredFlag = "";       // нужен ли ключ (флаг из LampadaFlags), например "key_exit"; пусто — не нужен

    [Header("Подсказки на экране")]
    public string tooHighText = "Слишком высоко... Нужно на что-то встать.";
    public string openedText = "Дверь открылась";
    public string lockedText = "Заперто. Нужен ключ.";

    public bool isOpen;
    [Tooltip("После первого открытия E открывает и закрывает дверь сколько угодно раз")]
    public bool canClose = true;
    public UnityEvent OnOpened;            // сюда можно добавить звук, свет и т.п.
    public UnityEvent OnClosed;

    private bool unlocked;                 // дверь уже открывали — условия больше не проверяем
    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private string message;
    private float messageTimer;

    void Start()
    {
        if (door == null) door = transform;
        closedPosition = door.position;
        closedRotation = door.localRotation;
        if (isOpen) unlocked = true;       // если дверь открыта с самого начала (для отладки)
    }

    public void Interact(GameObject actor)
    {
        // Уже открывали раньше — просто открываем/закрываем
        if (unlocked)
        {
            if (!canClose && isOpen) return;
            isOpen = !isOpen;
            if (isOpen) OnOpened?.Invoke(); else OnClosed?.Invoke();
            return;
        }

        // Проверяем, стоит ли девочка на чём-то высоком
        if (actor.transform.position.y < minPlayerHeight)
        {
            ShowMessage(tooHighText);
            return;
        }

        // Нужен ли ключ
        if (!string.IsNullOrEmpty(requiredFlag))
        {
            var flags = actor.GetComponent<LampadaFlags>();
            if (flags == null || !flags.Has(requiredFlag))
            {
                ShowMessage(lockedText);
                return;
            }
        }

        unlocked = true;
        isOpen = true;
        ShowMessage(openedText);
        OnOpened?.Invoke();
    }

    void Update()
    {
        if (swing)
        {
            // Поворот вокруг вертикальной оси петли
            Quaternion targetRot = isOpen ? closedRotation * Quaternion.Euler(0f, openAngle, 0f) : closedRotation;
            door.localRotation = Quaternion.RotateTowards(door.localRotation, targetRot, swingSpeed * Time.deltaTime);
        }
        else
        {
            // Подъёмная дверь: плавно едет вверх
            Vector3 target = isOpen ? closedPosition + openOffset : closedPosition;
            door.position = Vector3.MoveTowards(door.position, target, openSpeed * Time.deltaTime);
        }

        if (messageTimer > 0f) messageTimer -= Time.deltaTime;
    }

    void ShowMessage(string text)
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