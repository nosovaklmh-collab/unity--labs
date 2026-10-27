using UnityEngine;

// Вид от первого лица: камера — «глаза» игрока.
// Мышь влево-вправо поворачивает игрока, вверх-вниз — наклоняет голову.
// Повесьте на Main Camera, которая лежит ВНУТРИ объекта Player.
// Esc — освободить курсор, клик в окне Game — снова захватить.
public class FirstPersonLook : MonoBehaviour
{
    public float sensitivity = 2f;    // чувствительность мыши
    public float minPitch = -60f;     // насколько можно смотреть вниз
    public float maxPitch = 60f;      // насколько можно смотреть вверх

    private Transform body;           // игрок (родитель камеры)
    private float pitch;

    void Start()
    {
        body = transform.parent;
        Cursor.lockState = CursorLockMode.Locked;   // курсор прячется и не уходит из окна
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        if (Input.GetMouseButtonDown(0))      { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mx = Input.GetAxis("Mouse X") * sensitivity;
        float my = Input.GetAxis("Mouse Y") * sensitivity;

        body.Rotate(0f, mx, 0f);                         // поворот всего игрока
        pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);  // наклон головы
    }
}
