using UnityEngine;
using UnityEngine.Rendering;

// Доп. задание 4: стелс — игрок в тени не виден врагу, на свету — виден.
// Освещённость игрока = свет из Light Probes (отражённый/запечённый)
//                      + прямой свет ближайших ламп (с проверкой, не загорожена ли лампа стеной).
// Повесьте на «врага» (например, куб Goal), в поле Player перетащите игрока.
public class StealthEnemy : MonoBehaviour
{
    public Transform player;
    public float viewDistance = 8f;      // дальше этого враг не видит вообще
    public float lightThreshold = 0.3f;  // ниже — игрок «в тени»
    public float probeWeight = 3f;       // во сколько раз усилить вклад зондов (они дают маленькие числа)

    private Renderer rend;
    private Color baseColor;
    private float probeLight, lampLight, totalLight;
    private bool sees;
    private Light[] lights;

    static readonly Vector3[] dirs = { Vector3.up, Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    readonly Color[] results = new Color[5];

    void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
        lights = FindObjectsByType<Light>(FindObjectsSortMode.None);   // все лампы сцены
    }

    void Update()
    {
        Vector3 chest = player.position + Vector3.up * 0.5f;

        // 1. Свет из Light Probes: интерполяция ближайших зондов
        LightProbes.GetInterpolatedProbe(chest, null, out SphericalHarmonicsL2 sh);
        sh.Evaluate(dirs, results);
        float sum = 0f;
        foreach (var c in results) sum += c.grayscale;
        probeLight = sum / results.Length * probeWeight;

        // 2. Прямой свет ламп, которые «видят» игрока
        lampLight = 0f;
        foreach (var l in lights)
        {
            if (l == null || !l.isActiveAndEnabled || l.intensity <= 0f) continue;

            if (l.type == LightType.Directional)
            {
                // Солнце: светит, только если над игроком ничего нет (крыша загораживает)
                if (!Physics.Raycast(chest, -l.transform.forward, 100f))
                    lampLight += l.intensity * 0.5f;
                continue;
            }

            Vector3 toPlayer = chest - l.transform.position;
            float dist = toPlayer.magnitude;
            if (dist > l.range) continue;

            // Прожектор светит только внутри своего конуса
            if (l.type == LightType.Spot &&
                Vector3.Angle(l.transform.forward, toPlayer) > l.spotAngle * 0.5f) continue;

            // Не загорожена ли лампа стеной
            if (Physics.Linecast(l.transform.position, chest, out RaycastHit hit) &&
                !hit.transform.IsChildOf(player)) continue;

            float falloff = 1f - dist / l.range;          // чем дальше, тем слабее
            lampLight += l.intensity * falloff * falloff;
        }

        totalLight = probeLight + lampLight;

        bool close = Vector3.Distance(transform.position, player.position) < viewDistance;
        sees = close && totalLight > lightThreshold;

        rend.material.color = sees ? Color.red : baseColor;   // враг краснеет, когда видит
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 35, 600, 25),
            $"Свет: зонды {probeLight:F2} + лампы {lampLight:F2} = {totalLight:F2}  →  {(sees ? "ВИДЕН" : "скрыт")}");
    }
}