using UnityEngine;

// ЂЋампадаї Ч отладочный телепорт. „тобы не проходить игру сначала при каждой проверке.
// ¬ режиме Play нажмите F1, F2, F3Е Ч девочка переноситс€ к точке из списка Points.
// F10 Ч выдать все флаги из списка Give Flags (например, ключ), чтобы проверить запертые двери.
// √алочка Debug Enabled Ч быстро выключить отладку, когда проходите игру Ђпо-честномуї.
// –аботает только в редакторе Unity; в собранной игре клавиши ничего не делают.
//
// ¬ешаетс€ на Player.
public class LampadaDebugTeleport : MonoBehaviour
{
    public bool debugEnabled = true;           // выключите, чтобы клавиши F1Е ничего не делали
    public Transform[] points;                 // точки телепорта: 1 Ч перва€, 2 Ч втора€Е
    public string[] giveFlags = { "key_exit" };// что выдать по клавише 0
    public bool showHint = true;

    private CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!Application.isEditor || !debugEnabled) return;   // в готовой игре отключено

        for (int i = 0; i < points.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i) && points[i] != null)
                TeleportTo(points[i]);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            var flags = GetComponent<LampadaFlags>();
            if (flags != null)
                foreach (var f in giveFlags) flags.Add(f);
        }
    }

    void TeleportTo(Transform point)
    {
        // CharacterController мешает просто помен€ть позицию Ч на мгновение выключаем его
        if (cc != null) cc.enabled = false;
        transform.position = point.position;
        if (cc != null) cc.enabled = true;
        Debug.Log("Ћампада: телепорт к " + point.name);
    }

    void OnGUI()
    {
        if (!Application.isEditor || !debugEnabled || !showHint || points.Length == 0) return;
        GUI.Label(new Rect(10, 10, 600, 22), $"[ќтладка] F1ЦF{Mathf.Min(points.Length, 9)}: телепорт   F10: выдать флаги");
    }
}