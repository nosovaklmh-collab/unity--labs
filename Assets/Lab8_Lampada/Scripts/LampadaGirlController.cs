using UnityEngine;

// «Лампада» — управление девочкой в стиле Little Nightmares (вид сбоку, 2.5D).
//
// Клавиши:
//   A / D (или ← →)   — идти влево / вправо
//   W / S (или ↑ ↓)   — шаг вглубь комнаты / к камере
//   Left Shift        — бежать
//   C или Left Ctrl   — присесть (ползти под мебелью)
//   Пробел            — прыжок
//
// Вешается на объект Player вместе с CharacterController.
// Animator необязателен: если он есть, скрипт передаёт в него параметры
//   Speed (float), Grounded (bool), Crouch (bool), Jump (trigger).
[RequireComponent(typeof(CharacterController))]
public class LampadaGirlController : MonoBehaviour
{
    [Header("Скорость")]
    public float walkSpeed = 2.2f;      // девочка маленькая — ходит медленно
    public float runSpeed = 4.2f;
    public float crouchSpeed = 1.1f;
    public float acceleration = 12f;    // как быстро разгоняется и тормозит
    public float turnSpeed = 720f;      // градусов в секунду при повороте

    [Header("Глубина (ось Z)")]
    [Range(0f, 1f)] public float depthMultiplier = 0.6f; // вглубь ходим медленнее, чем вбок

    [Header("Прыжок и гравитация")]
    public float jumpHeight = 0.8f;
    public float gravity = -20f;

    [Header("Приседание")]
    public float standHeight = 1.2f;
    public float crouchHeight = 0.7f;

    [Header("Толкание ящиков")]
    public float pushPower = 2f;

    [Header("Анимация (необязательно)")]
    public Animator animator;           // перетащите сюда Animator модели девочки

    // Эти значения читают другие скрипты (например, враг или звук шагов)
    public bool IsCrouching { get; private set; }
    public bool IsRunning { get; private set; }
    public float CurrentSpeed { get; private set; }

    private CharacterController cc;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;     // двигает скрипт, а не анимация
        SetHeight(standHeight);
    }

    void Update()
    {
        bool grounded = cc.isGrounded;

        // ---------- 1. Читаем клавиши ----------
        float x = Input.GetAxisRaw("Horizontal");                    // A/D
        float z = Input.GetAxisRaw("Vertical") * depthMultiplier;    // W/S
        Vector3 input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // ---------- 2. Приседание ----------
        bool crouchKey = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);
        if (crouchKey && !IsCrouching)
        {
            IsCrouching = true;
            SetHeight(crouchHeight);
        }
        else if (!crouchKey && IsCrouching && CanStandUp())
        {
            IsCrouching = false;
            SetHeight(standHeight);
        }

        // ---------- 3. Выбираем скорость ----------
        IsRunning = Input.GetKey(KeyCode.LeftShift) && !IsCrouching && input.sqrMagnitude > 0.01f;
        float speed = IsCrouching ? crouchSpeed : (IsRunning ? runSpeed : walkSpeed);

        // Плавный разгон и торможение
        Vector3 target = input * speed;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, acceleration * Time.deltaTime);
        CurrentSpeed = horizontalVelocity.magnitude;

        // ---------- 4. Прыжок и гравитация ----------
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;                                  // прижимаем к полу

        if (grounded && !IsCrouching && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // v = √(2gh)
            if (animator != null) SetTrigger("Jump");
        }
        verticalVelocity += gravity * Time.deltaTime;

        // ---------- 5. Двигаем персонажа ----------
        Vector3 move = horizontalVelocity + Vector3.up * verticalVelocity;
        cc.Move(move * Time.deltaTime);

        // ---------- 6. Поворачиваем лицом по направлению движения ----------
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(new Vector3(input.x, 0f, input.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // ---------- 7. Анимация ----------
        if (animator != null)
        {
            SetFloat("Speed", CurrentSpeed);
            SetBool("Grounded", cc.isGrounded);
            SetBool("Crouch", IsCrouching);
        }
    }

    // Меняем рост капсулы, чтобы ноги оставались на полу
    void SetHeight(float h)
    {
        cc.height = h;
        cc.center = new Vector3(0f, h / 2f, 0f);
    }

    // Можно ли встать: нет ли над головой стола или полки
    bool CanStandUp()
    {
        Vector3 head = transform.position + Vector3.up * (crouchHeight - 0.05f);
        float distance = standHeight - crouchHeight + 0.05f;
        return !Physics.SphereCast(head, cc.radius * 0.9f, Vector3.up, out _, distance,
                                   ~0, QueryTriggerInteraction.Ignore);
    }

    // Толкаем ящики с Rigidbody, когда упираемся в них
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;                     // стоим сверху — не толкаем

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        body.linearVelocity = pushDir * pushPower;
    }

    // ---------- Безопасная работа с Animator ----------
    // Если в Animator Controller нет такого параметра, просто ничего не делаем (без ошибок в Console)
    bool HasParam(string name, AnimatorControllerParameterType type)
    {
        if (animator.runtimeAnimatorController == null) return false;
        foreach (var p in animator.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }

    void SetFloat(string n, float v) { if (HasParam(n, AnimatorControllerParameterType.Float)) animator.SetFloat(n, v, 0.1f, Time.deltaTime); }
    void SetBool(string n, bool v) { if (HasParam(n, AnimatorControllerParameterType.Bool)) animator.SetBool(n, v); }
    void SetTrigger(string n) { if (HasParam(n, AnimatorControllerParameterType.Trigger)) animator.SetTrigger(n); }
}
