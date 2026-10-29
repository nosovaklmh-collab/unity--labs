using UnityEngine;
using UnityEngine.Events;

// «Лампада» — выключатель / рычаг. Первый элемент головоломок.
// Подойдите и нажмите E: выключатель переключается и вызывает событие.
// В Inspector в On Switched можно добавить что угодно: включить свет, открыть дверь, опустить лифт.
//
// Вешается на объект с коллайдером (кубик-выключатель на стене).
public class LampadaLightSwitch : MonoBehaviour, IInteractable
{
    public Light[] lightsToToggle;       // лампы, которые включает выключатель
    public bool isOn = false;
    public UnityEvent<bool> OnSwitched;  // как UnityEvent из ЛР «События»

    [Header("Внешний вид")]
    public Transform lever;              // необязательно: что повернуть при переключении
    public float leverAngle = 40f;

    void Start()
    {
        Apply();
    }

    public void Interact(GameObject actor)
    {
        isOn = !isOn;
        Apply();
        OnSwitched?.Invoke(isOn);
        Debug.Log($"{name}: {(isOn ? "включено" : "выключено")}");
    }

    void Apply()
    {
        foreach (var l in lightsToToggle)
            if (l != null) l.enabled = isOn;

        if (lever != null)
            lever.localRotation = Quaternion.Euler(isOn ? -leverAngle : leverAngle, 0f, 0f);
    }
}
