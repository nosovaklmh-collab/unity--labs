using UnityEngine;

// ЛР «Физика и контроллер персонажа»
// Часть 1: CharacterController, IsGrounded() через Physics.SphereCast, склоны, ступеньки
// Часть 2: v0 и g из формул (h, t), асимметричная гравитация gUp < gDown
// Часть 3: coyote time, jump buffer, обрезка скорости при отпускании, контроль в воздухе
// Часть 4: вывод скорости, isGrounded и таймеров на экран
// Часть 5: рывок (Dash) — Left Shift
//
// Клавиши: WASD — ходьба, Пробел — прыжок, Shift — рывок,
//          1/2/3 — пресеты, V — вариативный/фиксированный прыжок, C — coyote+buffer вкл/выкл
[RequireComponent(typeof(CharacterController))]
public class JumpController : MonoBehaviour
{
    public enum Preset { Custom, Symmetric, Arcade, Heavy }

    [Header("Пресет (Custom = значения из Inspector)")]
    public Preset preset = Preset.Custom;

    [Header("Движение")]
    public float moveSpeed = 7f;
    [Range(0f, 1f)] public float airControl = 1f;   // 1 = полный, 0.5–0.8 = ограниченный, 0 = нет
    public float acceleration = 70f;

    [Header("Прыжок: целевые метрики")]
    public float jumpHeight = 2f;        // h, м
    public float totalAirTime = 0.8f;    // полное время полёта при симметричной гравитации, с

    [Header("Асимметричная гравитация")]
    public float fallMultiplier = 1.5f;  // gDown = gUp * fallMultiplier
    public float maxFallSpeed = 40f;

    [Header("Отзывчивость")]
    [Range(0.08f, 0.15f)] public float coyoteTime = 0.1f;     // 80–150 мс
    [Range(0.10f, 0.20f)] public float jumpBufferTime = 0.15f; // 100–200 мс
    public bool variableJump = true;
    [Range(0.2f, 0.8f)] public float jumpCutMultiplier = 0.5f; // во сколько раз режем vy при отпускании

    [Header("Проверка земли (SphereCast)")]
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;

    [Header("Склоны")]
    public float maxSlopeAngle = 45f;    // до этого угла поднимаемся, круче — соскальзываем
    public float slideSpeed = 8f;

    [Header("Рывок (Dash)")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public int maxDashes = 1;            // сколько рывков в воздухе до приземления

    // --- вычисляемые ---
    private float gUp, gDown, v0;

    private CharacterController cc;
    private Vector3 velocity;
    private bool grounded;
    private float slopeAngle;
    private Vector3 groundNormal = Vector3.up;
    private float coyoteCounter, bufferCounter;
    private bool assistOn = true;
    private float dashTimer;
    private int dashesLeft;
    private Vector3 dashDir;

    // статистика последнего прыжка
    private bool inAir;
    private float startY, maxY, airTime;
    private string lastJump = "—";

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.stepOffset = 0.3f;            // ступеньки до 30 см проходим без прыжка
        if (preset != Preset.Custom) ApplyPreset(preset);
        Recalculate();
    }

    void OnValidate() { Recalculate(); }

    // Часть 2: расчёт по формулам
    // t_up = T/2,  g = 2h / t_up²,  v0 = sqrt(2 g h)  →  проверка: T = 2 v0 / g
    void Recalculate()
    {
        float tUp = totalAirTime / 2f;
        gUp = 2f * jumpHeight / (tUp * tUp);
        v0 = Mathf.Sqrt(2f * gUp * jumpHeight);
        gDown = gUp * fallMultiplier;
    }

    void Update()
    {
        HandleHotkeys();
        Recalculate();

        grounded = IsGrounded();
        bool onWalkableGround = grounded && slopeAngle <= maxSlopeAngle;
        if (onWalkableGround) dashesLeft = maxDashes;

        // ---------- Coyote time ----------
        if (onWalkableGround && velocity.y <= 0f) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        // ---------- Jump buffer ----------
        if (Input.GetButtonDown("Jump")) bufferCounter = jumpBufferTime;
        else bufferCounter -= Time.deltaTime;

        bool jumpRequested = Input.GetButtonDown("Jump") || (assistOn && bufferCounter > 0f);
        bool canJump = (onWalkableGround && velocity.y <= 0f) || (assistOn && coyoteCounter > 0f);

        if (jumpRequested && canJump)
        {
            velocity.y = v0;
            bufferCounter = 0f;   // буфер израсходован — повторный прыжок невозможен
            coyoteCounter = 0f;   // койот израсходован
            StartStats();
        }

        // ---------- Вариативная высота: обрезка скорости только на подъёме ----------
        if (variableJump && Input.GetButtonUp("Jump") && velocity.y > 0f)
            velocity.y *= jumpCutMultiplier;

        // ---------- Рывок ----------
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashTimer <= 0f && dashesLeft > 0)
        {
            Vector3 inDir = InputDirection();
            dashDir = inDir.sqrMagnitude > 0.01f ? inDir.normalized : transform.forward;
            dashTimer = dashDuration;
            if (!onWalkableGround) dashesLeft--;
        }

        if (dashTimer > 0f)
        {
            // во время рывка гравитация отключена, скорость фиксированная
            dashTimer -= Time.deltaTime;
            velocity = dashDir * dashSpeed;
            if (dashTimer <= 0f) velocity = dashDir * moveSpeed;
        }
        else
        {
            ApplyGravity(onWalkableGround);
            ApplyHorizontal(onWalkableGround);
        }

        cc.Move(velocity * Time.deltaTime);
        UpdateStats();
    }

    // Часть 1: проверка земли через SphereCast вниз от ног
    public bool IsGrounded()
    {
        float r = cc.radius * 0.9f;
        Vector3 origin = SphereOrigin();
        float dist = groundCheckDistance + cc.skinWidth + 0.1f;

        if (Physics.SphereCast(origin, r, Vector3.down, out RaycastHit hit, dist, groundMask,
                               QueryTriggerInteraction.Ignore))
        {
            groundNormal = hit.normal;
            // уточняем нормаль лучом из центра: на рёбрах и стыках у сферы нормаль бывает косой
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit ray, dist + r + 0.3f, groundMask,
                                QueryTriggerInteraction.Ignore))
                groundNormal = ray.normal;

            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            return true;
        }
        groundNormal = Vector3.up;
        slopeAngle = 0f;
        return false;
    }

    Vector3 SphereOrigin()
    {
        // центр нижней полусферы капсулы, чуть приподнятый
        float bottom = cc.center.y - cc.height / 2f + cc.radius;
        return transform.position + Vector3.up * (bottom + 0.1f);
    }

    void ApplyGravity(bool onWalkableGround)
    {
        // Асимметричная гравитация: на подъёме gUp, на спуске gDown
        float g = velocity.y > 0f ? gUp : gDown;
        velocity.y -= g * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);

        if (onWalkableGround && velocity.y < 0f)
            velocity.y = -2f;   // прижимаем к земле, чтобы не дрожать на стыках и спусках
    }

    void ApplyHorizontal(bool onWalkableGround)
    {
        Vector3 target = InputDirection() * moveSpeed;
        float control = onWalkableGround ? 1f : airControl;

        Vector3 horiz = new Vector3(velocity.x, 0f, velocity.z);
        horiz = Vector3.MoveTowards(horiz, target, acceleration * control * Time.deltaTime);

        // Склон круче порога — соскальзываем
        if (grounded && slopeAngle > maxSlopeAngle)
        {
            Vector3 slide = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized * slideSpeed;
            horiz = new Vector3(slide.x, 0f, slide.z);
            velocity.y = Mathf.Min(velocity.y, slide.y);
        }

        velocity.x = horiz.x;
        velocity.z = horiz.z;
    }

    Vector3 InputDirection()
    {
        Vector3 d = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        return d.sqrMagnitude > 1f ? d.normalized : d;
    }

    // ---------- Пресеты ----------
    public void ApplyPreset(Preset p)
    {
        preset = p;
        switch (p)
        {
            case Preset.Symmetric: // чистый расчёт, без асимметрии — точка отсчёта
                moveSpeed = 7f; airControl = 1f; acceleration = 70f;
                jumpHeight = 2f; totalAirTime = 0.8f; fallMultiplier = 1f;
                break;
            case Preset.Arcade:    // аркадный: полный контроль, быстрое падение
                moveSpeed = 8f; airControl = 1f; acceleration = 90f;
                jumpHeight = 2f; totalAirTime = 0.8f; fallMultiplier = 1.5f;
                break;
            case Preset.Heavy:     // тяжёлый: ограниченный контроль (60%), сильная гравитация вниз
                moveSpeed = 6f; airControl = 0.6f; acceleration = 25f;
                jumpHeight = 1.5f; totalAirTime = 0.8f; fallMultiplier = 2.5f;
                break;
        }
        Recalculate();
        Debug.Log($"Пресет {p}: v0 = {v0:F2}, gUp = {gUp:F2}, gDown = {gDown:F2}");
    }

    void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyPreset(Preset.Symmetric);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyPreset(Preset.Arcade);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyPreset(Preset.Heavy);
        if (Input.GetKeyDown(KeyCode.V)) variableJump = !variableJump;
        if (Input.GetKeyDown(KeyCode.C)) assistOn = !assistOn;
    }

    // ---------- Замер прыжка ----------
    void StartStats() { inAir = true; startY = maxY = transform.position.y; airTime = 0f; }

    void UpdateStats()
    {
        if (!inAir) return;
        airTime += Time.deltaTime;
        maxY = Mathf.Max(maxY, transform.position.y);
        if (grounded && velocity.y <= 0f && airTime > 0.05f)
        {
            inAir = false;
            lastJump = $"высота {maxY - startY:F2} м, полёт {airTime:F2} с";
            Debug.Log("Прыжок: " + lastJump);
        }
    }

    // Часть 4: значения на экране
    void OnGUI()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        GUI.Box(new Rect(10, 10, 400, 230), "");
        float y = 15; float dy = 20;
        void L(string t) { GUI.Label(new Rect(20, y, 390, 22), t, s); y += dy; }

        Vector3 hv = new Vector3(velocity.x, 0, velocity.z);
        L($"Пресет: {preset}   (1 Симметр. / 2 Аркадный / 3 Тяжёлый)");
        L($"Скорость: гориз. {hv.magnitude:F1} м/с, верт. {velocity.y:F1} м/с");
        L($"isGrounded: {(grounded ? "ДА" : "нет")}   угол склона: {slopeAngle:F0}° (порог {maxSlopeAngle}°)");
        L($"Coyote: {Mathf.Max(0, coyoteCounter) * 1000:F0} / {coyoteTime * 1000:F0} мс");
        L($"Buffer: {Mathf.Max(0, bufferCounter) * 1000:F0} / {jumpBufferTime * 1000:F0} мс");
        L($"Coyote+Buffer: {(assistOn ? "ВКЛ" : "ВЫКЛ")} (C)   Прыжок: {(variableJump ? "вариативный" : "фиксированный")} (V)");
        L($"v0 = {v0:F2}   gUp = {gUp:F2}   gDown = {gDown:F2}");
        L($"Контроль в воздухе: {airControl * 100:F0}%   Рывки: {dashesLeft}/{maxDashes} (Shift)");
        L($"Последний прыжок: {lastJump}");
    }

    void OnDrawGizmos()
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        if (cc == null) return;
        Vector3 o = SphereOrigin();
        float r = cc.radius * 0.9f;
        float dist = groundCheckDistance + cc.skinWidth + 0.1f;
        Gizmos.color = grounded ? (slopeAngle > maxSlopeAngle ? Color.yellow : Color.green) : Color.red;
        Gizmos.DrawWireSphere(o, r);                          // начало каста
        Gizmos.DrawWireSphere(o + Vector3.down * dist, r);    // конец каста
        Gizmos.DrawLine(o, o + Vector3.down * dist);
    }
}