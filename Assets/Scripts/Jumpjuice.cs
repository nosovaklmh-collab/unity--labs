using UnityEngine;

public class JumpJuice : MonoBehaviour
{
    public Transform body;                  // дочерн€€ капсула Body
    public Color upColor = Color.yellow;    // цвет при подъЄме
    public Color downColor = Color.cyan;    // цвет при падении
    public float squashAmount = 0.35f;      // сила сплющивани€ при приземлении
    public float stretchAmount = 0.25f;     // сила выт€гивани€ в полЄте
    public float returnSpeed = 10f;         // как быстро возвращаетс€ форма

    private CharacterController cc;
    private Renderer rend;
    private Color groundColor;
    private Vector3 baseScale;
    private bool wasGrounded = true;
    private float lastFallSpeed;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        rend = body.GetComponent<Renderer>();
        baseScale = body.localScale;
        groundColor = rend.material.color;  // цвет на земле = ваш исходный цвет
    }

    void LateUpdate()
    {
        bool grounded = cc.isGrounded;
        float vy = cc.velocity.y;
        Vector3 target = baseScale;

        if (!grounded)
        {
            // ¬ полЄте выт€гиваемс€ тем сильнее, чем больше скорость
            float s = Mathf.Clamp01(Mathf.Abs(vy) / 10f) * stretchAmount;
            target = new Vector3(baseScale.x * (1 - s * 0.5f), baseScale.y * (1 + s), baseScale.z * (1 - s * 0.5f));
            lastFallSpeed = vy;
        }

        if (grounded && !wasGrounded)
        {
            // ћомент приземлени€: резко сплющиваем
            float s = Mathf.Clamp01(-lastFallSpeed / 15f) * squashAmount;
            body.localScale = new Vector3(baseScale.x * (1 + s), baseScale.y * (1 - s), baseScale.z * (1 + s));
        }

        body.localScale = Vector3.Lerp(body.localScale, target, returnSpeed * Time.deltaTime);

        // ƒержим низ капсулы на земле, чтобы при сплющивании она не "висела"
        body.localPosition = new Vector3(0f, body.localScale.y / baseScale.y - 1f, 0f);

        // ÷вет по состо€нию
        Color c = grounded ? groundColor : (vy > 0f ? upColor : downColor);
        rend.material.color = Color.Lerp(rend.material.color, c, 15f * Time.deltaTime);

        wasGrounded = grounded;
    }
}