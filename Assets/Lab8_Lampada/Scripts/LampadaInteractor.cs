using UnityEngine;

// «Лампада» — взаимодействие с предметами по клавише E.
// Ищет рядом с девочкой объекты, у которых есть скрипт с интерфейсом IInteractable
// (интерфейс уже есть в проекте — из ЛР «Инвентарь»), и вызывает у ближайшего Interact().
// Над экраном появляется подсказка «E — взаимодействовать».
//
// Вешается на Player.
public class LampadaInteractor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public float radius = 1.2f;                    // как далеко «дотягивается» девочка
    public Vector3 handOffset = new Vector3(0f, 0.6f, 0.4f); // точка чуть впереди, на уровне рук

    private IInteractable current;
    private string currentName;

    void Update()
    {
        FindNearest();

        if (current != null && Input.GetKeyDown(interactKey))
            current.Interact(gameObject);
    }

    void FindNearest()
    {
        current = null;
        float best = float.MaxValue;
        Vector3 hand = transform.TransformPoint(handOffset);

        // Collide — чтобы находить и триггеры (например, рычаг с Is Trigger)
        Collider[] hits = Physics.OverlapSphere(hand, radius, ~0, QueryTriggerInteraction.Collide);
        foreach (var col in hits)
        {
            if (col.transform.IsChildOf(transform)) continue;        // себя не считаем
            var it = col.GetComponentInParent<IInteractable>();
            if (it == null) continue;

            float d = (col.ClosestPoint(hand) - hand).sqrMagnitude;
            if (d < best)
            {
                best = d;
                current = it;
                currentName = col.name;
            }
        }
    }

    void OnGUI()
    {
        if (current == null) return;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height - 80, Screen.width, 30), $"[{interactKey}] {currentName}", style);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(handOffset), radius);
    }
}
