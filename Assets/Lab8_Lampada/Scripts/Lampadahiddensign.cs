using TMPro;
using UnityEngine;

// «Лампада» — головоломка «Знак на обоях».
// Надпись (цифра) невидима, пока девочка не поднесёт к ней зажжённую лампу.
// Можно добавить лампы, которые должны быть выключены (Must Be Off), —
// тогда знак виден только в темноте.
//
// Вешается на 3D-текст (GameObject → 3D Object → Text - TextMeshPro).
public class LampadaHiddenSign : MonoBehaviour
{
    public LampadaLamp lamp;               // если пусто — найдётся сам
    public float revealDistance = 3f;      // как близко поднести лампу
    public float fadeSpeed = 1.5f;         // как быстро проявляется
    public Light[] mustBeOff;              // необязательно: эти лампы должны быть выключены

    [Header("Подсказка при первом появлении")]
    public string discoverText = "На обоях проступила цифра...";

    private TMP_Text text;
    private float alpha;
    private bool discovered;
    private string message;
    private float messageTimer;

    void Start()
    {
        if (lamp == null) lamp = FindFirstObjectByType<LampadaLamp>();
        text = GetComponent<TMP_Text>();
        Apply(0f);
    }

    bool IsRevealed()
    {
        if (lamp == null || !lamp.IsLit || lamp.lampLight == null) return false;

        foreach (var l in mustBeOff)
            if (l != null && l.enabled) return false;   // горит верхний свет — знак не виден

        float dist = Vector3.Distance(lamp.lampLight.transform.position, transform.position);
        return dist <= revealDistance;
    }

    void Update()
    {
        float target = IsRevealed() ? 1f : 0f;
        alpha = Mathf.MoveTowards(alpha, target, fadeSpeed * Time.deltaTime);
        Apply(alpha);

        if (!discovered && alpha > 0.9f)
        {
            discovered = true;
            message = discoverText;
            messageTimer = 3f;
        }
        if (messageTimer > 0f) messageTimer -= Time.deltaTime;
    }

    void Apply(float a)
    {
        if (text != null) text.alpha = a;
    }

    void OnGUI()
    {
        if (messageTimer <= 0f) return;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        style.normal.textColor = new Color(1f, 0.85f, 0.6f);
        GUI.Label(new Rect(0, Screen.height - 130, Screen.width, 34), message, style);
    }
}