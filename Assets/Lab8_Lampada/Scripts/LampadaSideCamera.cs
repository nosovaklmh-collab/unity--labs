using UnityEngine;

// «Лампада» — камера сбоку, как в Little Nightmares.
// Камера смотрит на девочку со стороны (вдоль оси Z), плавно едет за ней
// и чуть «заглядывает» вперёд — туда, куда она идёт.
//
// Вешается на Main Camera. В поле Target перетащите Player.
public class LampadaSideCamera : MonoBehaviour
{
    public Transform target;

    [Header("Положение камеры относительно девочки")]
    public Vector3 offset = new Vector3(0f, 2.2f, -7f);   // чуть выше и перед комнатой
    public float lookHeight = 0.8f;                        // смотрим примерно на уровень груди

    [Header("Плавность")]
    public float followSmooth = 4f;
    public float lookAhead = 1.2f;                         // насколько заглядываем вперёд по X
    public float lookAheadSmooth = 2f;

    [Header("Границы комнаты по X (камера не выезжает за стены)")]
    public bool useBounds = false;
    public float minX = -8f;
    public float maxX = 8f;

    private float currentLookAhead;
    private float lastTargetX;

    void Start()
    {
        if (target == null) return;
        lastTargetX = target.position.x;
        transform.position = target.position + offset;    // сразу ставим камеру на место
    }

    void LateUpdate()
    {
        if (target == null) return;

        // В какую сторону идёт девочка по X: +1 вправо, -1 влево, 0 стоит
        float dx = target.position.x - lastTargetX;
        lastTargetX = target.position.x;
        float dir = Mathf.Abs(dx) > 0.001f ? Mathf.Sign(dx) : 0f;
        currentLookAhead = Mathf.Lerp(currentLookAhead, dir * lookAhead, lookAheadSmooth * Time.deltaTime);

        Vector3 wanted = target.position + offset + Vector3.right * currentLookAhead;
        if (useBounds) wanted.x = Mathf.Clamp(wanted.x, minX, maxX);

        transform.position = Vector3.Lerp(transform.position, wanted, followSmooth * Time.deltaTime);

        Vector3 lookPoint = new Vector3(transform.position.x, target.position.y + lookHeight, target.position.z);
        transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
    }
}
