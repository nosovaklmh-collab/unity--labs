using UnityEngine;

// Капсула идёт по точкам маршрута (Path) и возвращается в начало.
// Это динамический объект: его освещение берётся из Light Probes.
public class CorridorWalker : MonoBehaviour
{
    public Transform path;        // объект Path с дочерними точками
    public float speed = 2f;      // м/с
    public float pause = 0.5f;    // остановка в каждой точке, сек

    private int index = 0;
    private float wait = 0f;

    void Update()
    {
        if (path == null || path.childCount == 0) return;
        if (wait > 0f) { wait -= Time.deltaTime; return; }

        Vector3 target = path.GetChild(index).position;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            index = (index + 1) % path.childCount;   // после последней точки — снова к первой
            wait = pause;
        }
    }
}
