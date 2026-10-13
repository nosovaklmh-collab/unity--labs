using UnityEngine;

// Камера плавно следует за игроком. Клавиша Tab переключает вид:
// Сбоку → Сзади → Три четверти (сбоку-сверху-сзади)
public class CameraFollow : MonoBehaviour
{
    public Transform target;                 // перетащить Player
    public float smooth = 6f;                // плавность следования

    [Header("Смещения камеры относительно игрока")]
    public Vector3 sideOffset = new Vector3(12f, 3f, 0f);   // сбоку: трасса идёт слева направо
    public Vector3 behindOffset = new Vector3(0f, 4f, -8f);   // сзади, как в 3D-играх
    public Vector3 angleOffset = new Vector3(9f, 6f, -7f);   // три четверти — видно и высоту, и глубину
    public Vector3 lookOffset = new Vector3(0f, 1f, 2f);    // куда смотрим: чуть выше и впереди игрока

    private int mode = 0;
    private readonly string[] names = { "Сбоку", "Сзади", "Три четверти" };

    void LateUpdate()
    {
        if (target == null) return;
        if (Input.GetKeyDown(KeyCode.Tab)) mode = (mode + 1) % 3;

        Vector3 offset = mode == 0 ? sideOffset : mode == 1 ? behindOffset : angleOffset;
        Vector3 wanted = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, wanted, smooth * Time.deltaTime);

        Quaternion look = Quaternion.LookRotation(target.position + lookOffset - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, smooth * Time.deltaTime);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 210, 10, 200, 22), $"Камера: {names[mode]} (Tab)");
    }
}