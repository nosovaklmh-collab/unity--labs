using UnityEngine;

// Простое движение игрока стрелками / WASD (Приложение А)
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        controller.SimpleMove(move * speed); // SimpleMove сам добавляет гравитацию
    }
}