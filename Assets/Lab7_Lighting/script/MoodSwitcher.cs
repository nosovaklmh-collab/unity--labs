using UnityEngine;
using UnityEngine.Rendering;

// Доп. задание 1: смена настроения на лету по клавише M.
// Меняет Volume-профиль (постобработку), цвет и яркость «солнца» и туман.
public class MoodSwitcher : MonoBehaviour
{
    public Volume volume;              // Global Volume
    public Light sun;                  // Directional Light

    [Header("Мирное")]
    public VolumeProfile calmProfile;
    public Color calmSunColor = new Color(1f, 0.94f, 0.86f);
    public float calmSunIntensity = 1f;
    public Color calmFogColor = new Color(1f, 0.85f, 0.6f);
    public float calmFogDensity = 0.02f;

    [Header("Боевое")]
    public VolumeProfile combatProfile;
    public Color combatSunColor = new Color(0.6f, 0.7f, 1f);
    public float combatSunIntensity = 0.4f;
    public Color combatFogColor = new Color(0.25f, 0.3f, 0.45f);
    public float combatFogDensity = 0.06f;

    private bool combat = false;

    void Start() { Apply(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            combat = !combat;
            Apply();
        }
    }

    void Apply()
    {
        volume.profile = combat ? combatProfile : calmProfile;
        sun.color = combat ? combatSunColor : calmSunColor;
        sun.intensity = combat ? combatSunIntensity : calmSunIntensity;
        RenderSettings.fog = true;
        RenderSettings.fogColor = combat ? combatFogColor : calmFogColor;
        RenderSettings.fogDensity = combat ? combatFogDensity : calmFogDensity;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 25), $"Настроение: {(combat ? "боевое" : "мирное")} (M)");
    }
}
