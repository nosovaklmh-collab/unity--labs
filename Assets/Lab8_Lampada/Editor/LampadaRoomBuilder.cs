using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// «Лампада» — строит сцену прототипа одной кнопкой.
// Меню Unity: Lampada → Создать пустую комнату (прототип)
//
// Что появится:
//   • тёмная комната (пол, задняя и боковые стены, потолок)
//   • стол (под ним можно проползти на корточках), ящик (можно толкать), ступенька (запрыгнуть)
//   • выключатель на стене + лампочка под потолком (проверка клавиши E)
//   • Player: капсула-заглушка + лампа в руке + все скрипты
//   • камера сбоку, лунный свет, туман, пост-обработка (виньетка, зерно, приглушённые цвета)
// Сцена сохраняется в Assets/Lab8_Lampada/Scenes/Lampada_Prototype.unity
public static class LampadaRoomBuilder
{
    const string Root = "Assets/Lab8_Lampada";

    [MenuItem("Lampada/Создать пустую комнату (прототип)")]
    public static void Build()
    {
        // Если в открытой сцене есть несохранённые изменения — Unity спросит, сохранить ли их
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder(Root, "Materials");
        EnsureFolder(Root, "Scenes");
        EnsureFolder(Root, "Settings");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Материалы ----------
        Material floorMat = GetMaterial("Floor", new Color(0.18f, 0.13f, 0.10f));   // старые доски
        Material wallMat  = GetMaterial("Wall",  new Color(0.22f, 0.24f, 0.22f));   // выцветшие обои
        Material woodMat  = GetMaterial("Wood",  new Color(0.30f, 0.20f, 0.12f));
        Material girlMat  = GetMaterial("Girl_Placeholder", new Color(0.72f, 0.75f, 0.82f)); // бледная ночная рубашка
        Material flameMat = GetMaterial("Lamp_Flame", new Color(1f, 0.75f, 0.4f), emission: true);

        // ---------- Комната ----------
        var room = new GameObject("Room").transform;
        Box("Floor",     new Vector3(0f, -0.25f, 1f),  new Vector3(18f, 0.5f, 8f), floorMat, room);
        Box("BackWall",  new Vector3(0f, 2.5f, 4.25f), new Vector3(18f, 5f, 0.5f), wallMat, room);
        Box("LeftWall",  new Vector3(-9.25f, 2.5f, 1f), new Vector3(0.5f, 5f, 8f), wallMat, room);
        Box("RightWall", new Vector3(9.25f, 2.5f, 1f),  new Vector3(0.5f, 5f, 8f), wallMat, room);
        Box("Ceiling",   new Vector3(0f, 5.25f, 1f),   new Vector3(18f, 0.5f, 8f), wallMat, room);

        // Стол: столешница на высоте 0.9 м — стоя не пройти, на корточках можно
        var table = new GameObject("Table").transform;
        table.SetParent(room);
        Box("Top", new Vector3(3f, 0.95f, 2f), new Vector3(2.2f, 0.1f, 1.4f), woodMat, table);
        Box("Leg1", new Vector3(2.0f, 0.45f, 1.4f), new Vector3(0.1f, 0.9f, 0.1f), woodMat, table);
        Box("Leg2", new Vector3(4.0f, 0.45f, 1.4f), new Vector3(0.1f, 0.9f, 0.1f), woodMat, table);
        Box("Leg3", new Vector3(2.0f, 0.45f, 2.6f), new Vector3(0.1f, 0.9f, 0.1f), woodMat, table);
        Box("Leg4", new Vector3(4.0f, 0.45f, 2.6f), new Vector3(0.1f, 0.9f, 0.1f), woodMat, table);

        // Ящик, который можно толкать
        var crate = Box("Crate (толкать)", new Vector3(-3f, 0.35f, 2f), new Vector3(0.7f, 0.7f, 0.7f), woodMat, room);
        var rb = crate.gameObject.AddComponent<Rigidbody>();
        rb.mass = 8f;
        rb.linearDamping = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Ступенька, на которую надо запрыгнуть
        Box("Step (запрыгнуть)", new Vector3(6.5f, 0.3f, 2f), new Vector3(1.2f, 0.6f, 1.2f), woodMat, room);

        // Лампочка под потолком (сначала выключена) и выключатель на стене
        var bulbGO = new GameObject("CeilingBulb");
        bulbGO.transform.SetParent(room);
        bulbGO.transform.position = new Vector3(-6f, 4.5f, 1.5f);
        var bulb = bulbGO.AddComponent<Light>();
        bulb.type = LightType.Point;
        bulb.color = new Color(1f, 0.9f, 0.7f);
        bulb.range = 7f;
        bulb.intensity = 3f;
        bulb.shadows = LightShadows.Soft;

        var sw = Box("LightSwitch (E)", new Vector3(-6f, 1.0f, 3.95f), new Vector3(0.3f, 0.4f, 0.1f), woodMat, room);
        var switchScript = sw.gameObject.AddComponent<LampadaLightSwitch>();
        switchScript.lightsToToggle = new[] { bulb };
        switchScript.isOn = false;
        bulb.enabled = false;

        // ---------- Девочка (заглушка) ----------
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0.05f, 1.5f);
        player.transform.rotation = Quaternion.LookRotation(Vector3.right);   // смотрит вправо

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.2f;
        cc.radius = 0.25f;
        cc.center = new Vector3(0f, 0.6f, 0f);
        cc.stepOffset = 0.2f;
        cc.skinWidth = 0.03f;

        // Тело-заглушка: потом замените на модель девочки
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body (заменить на модель)";
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.transform.SetParent(player.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        body.GetComponent<Renderer>().sharedMaterial = girlMat;

        // «Нос» — чтобы было видно, куда она смотрит
        var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "FacingMarker";
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        nose.transform.SetParent(player.transform, false);
        nose.transform.localPosition = new Vector3(0f, 0.95f, 0.25f);
        nose.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        nose.GetComponent<Renderer>().sharedMaterial = girlMat;

        // Лампа в руке
        var lampGO = new GameObject("Lamp");
        lampGO.transform.SetParent(player.transform, false);
        lampGO.transform.localPosition = new Vector3(0.25f, 0.7f, 0.25f);

        var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Flame";
        Object.DestroyImmediate(flame.GetComponent<Collider>());
        flame.transform.SetParent(lampGO.transform, false);
        flame.transform.localScale = Vector3.one * 0.12f;
        flame.GetComponent<Renderer>().sharedMaterial = flameMat;
        flame.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        var lampLight = lampGO.AddComponent<Light>();
        lampLight.type = LightType.Point;
        lampLight.color = new Color(1f, 0.72f, 0.42f);
        lampLight.range = 5f;
        lampLight.intensity = 2f;
        lampLight.shadows = LightShadows.Soft;

        player.AddComponent<LampadaGirlController>();
        var lamp = player.AddComponent<LampadaLamp>();
        lamp.lampLight = lampLight;
        player.AddComponent<LampadaInteractor>();

        // ---------- Камера ----------
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 40f;                       // узкий угол — «кукольный» вид
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGO.AddComponent<AudioListener>();
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

        var follow = camGO.AddComponent<LampadaSideCamera>();
        follow.target = player.transform;
        follow.offset = new Vector3(0f, 1.8f, -7f);
        follow.useBounds = true;
        follow.minX = -5f;
        follow.maxX = 5f;
        camGO.transform.position = player.transform.position + follow.offset;
        camGO.transform.LookAt(player.transform.position + Vector3.up * 0.8f);

        // ---------- Свет и атмосфера ----------
        var moonGO = new GameObject("Moonlight");
        var moon = moonGO.AddComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.55f, 0.65f, 1f);
        moon.intensity = 0.15f;
        moon.shadows = LightShadows.Soft;
        moonGO.transform.rotation = Quaternion.Euler(35f, 20f, 0f);

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.06f, 0.09f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f);
        RenderSettings.fogDensity = 0.03f;

        // ---------- Пост-обработка ----------
        var volGO = new GameObject("Global Volume");
        var volume = volGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = CreateProfile();

        // ---------- Сохраняем ----------
        string scenePath = Root + "/Scenes/Lampada_Prototype.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Selection.activeGameObject = player;
        Debug.Log("Лампада: сцена создана и сохранена → " + scenePath + ". Нажмите Play!");
    }

    // ===================== вспомогательные методы =====================

    static Transform Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go.transform;
    }

    static Material GetMaterial(string name, Color color, bool emission = false)
    {
        string path = $"{Root}/Materials/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;     // уже создан раньше — берём готовый

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        mat = new Material(shader);

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else mat.color = color;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);

        if (emission)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 4f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static VolumeProfile CreateProfile()
    {
        string path = Root + "/Settings/Lampada_PostProcess.asset";
        AssetDatabase.DeleteAsset(path);  // пересоздаём при каждом запуске

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.45f);
        vignette.smoothness.Override(0.5f);
        AssetDatabase.AddObjectToAsset(vignette, profile);

        var grain = profile.Add<FilmGrain>(true);
        grain.intensity.Override(0.35f);
        AssetDatabase.AddObjectToAsset(grain, profile);

        var colors = profile.Add<ColorAdjustments>(true);
        colors.saturation.Override(-35f);   // приглушённые цвета, как в Little Nightmares
        colors.contrast.Override(15f);
        AssetDatabase.AddObjectToAsset(colors, profile);

        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.6f);     // мягкое свечение огонька лампы
        bloom.threshold.Override(0.9f);
        AssetDatabase.AddObjectToAsset(bloom, profile);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            AssetDatabase.CreateFolder(parent, child);
    }
}
