using UnityEngine;
using UnityEngine.Rendering;

// Доп. задание 4: стелс через Light Probes.
// Враг (например, куб в конце коридора) видит игрока, только если игрок
// близко И стоит на свету. Освещённость берётся из ближайших Light Probes.
public class StealthEnemy : MonoBehaviour
{
    public Transform player;
    public float viewDistance = 8f;
    public float lightThreshold = 0.35f;   // ниже — игрок «в тени»

    private Renderer rend;
    private Color baseColor;
    private float playerLight;
    private bool sees;

    static readonly Vector3[] dirs = { Vector3.up, Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    readonly Color[] results = new Color[5];

    void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
    }

    void Update()
    {
        // Освещённость в точке игрока: интерполяция ближайших зондов
        LightProbes.GetInterpolatedProbe(player.position, null, out SphericalHarmonicsL2 sh);
        sh.Evaluate(dirs, results);
        float sum = 0f;
        foreach (var c in results) sum += c.grayscale;
        playerLight = sum / results.Length;

        bool close = Vector3.Distance(transform.position, player.position) < viewDistance;
        sees = close && playerLight > lightThreshold;

        rend.material.color = sees ? Color.red : baseColor;   // враг краснеет, когда видит
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 35, 400, 25),
            $"Освещённость игрока: {playerLight:F2}  →  {(sees ? "ВИДЕН" : "скрыт")}");
    }
}
