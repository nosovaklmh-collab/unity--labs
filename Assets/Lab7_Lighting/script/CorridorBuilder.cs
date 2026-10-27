using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Строит коридор с двумя тупиками одной кнопкой (в режиме редактора, без Play):
// выделите объект с этим скриптом → в Inspector нажмите ⋮ у компонента → «Построить коридор».
// Вся геометрия помечается Static (нужно для запекания).
// Дополнительно создаются: путь для капсулы, капсула Player и сетка Light Probes.
public class CorridorBuilder : MonoBehaviour
{
    public bool buildCeiling = false;     // потолок (тогда свет «солнца» внутрь не попадёт)
    public bool createPlayerAndPath = true;
    public bool createLightProbes = true;

    private Transform root;
    private Material matFloor, matWall, matGoal;

    [ContextMenu("Построить коридор")]
    public void Build()
    {
        Transform old = transform.Find("Corridor");
        if (old != null) DestroyImmediate(old.gameObject);
        root = new GameObject("Corridor").transform;
        root.SetParent(transform, false);

        matFloor = MakeMaterial("Mat_Floor", new Color(0.75f, 0.75f, 0.75f));  // светло-серый
        matWall  = MakeMaterial("Mat_Wall",  new Color(0.35f, 0.35f, 0.38f));  // тёмно-серый
        matGoal  = MakeMaterial("Mat_Goal",  new Color(0.95f, 0.8f, 0.3f));    // цель в конце

        // Пол основного коридора: x от -2 до 2, z от -10 до 10
        Box("Floor", new Vector3(0, -0.1f, 0), new Vector3(4, 0.2f, 20), matFloor, true);

        // Левая стена с проёмом в тупик (z от -4 до -1)
        Box("Wall_L_1", new Vector3(-2.1f, 1.5f, -7f),   new Vector3(0.2f, 3, 6),  matWall, true);
        Box("Wall_L_2", new Vector3(-2.1f, 1.5f, 4.5f),  new Vector3(0.2f, 3, 11), matWall, true);
        // Правая стена с проёмом в тупик (z от 2 до 5)
        Box("Wall_R_1", new Vector3(2.1f, 1.5f, -4f),    new Vector3(0.2f, 3, 12), matWall, true);
        Box("Wall_R_2", new Vector3(2.1f, 1.5f, 7.5f),   new Vector3(0.2f, 3, 5),  matWall, true);
        // Торцевая стена в конце коридора
        Box("Wall_End", new Vector3(0, 1.5f, 10.1f),     new Vector3(4.4f, 3, 0.2f), matWall, true);

        // Тупик слева
        Box("DeadEnd_L_Floor", new Vector3(-3.5f, -0.1f, -2.5f), new Vector3(3, 0.2f, 3),     matFloor, true);
        Box("DeadEnd_L_Back",  new Vector3(-5.1f, 1.5f, -2.5f),  new Vector3(0.2f, 3, 3.4f),  matWall, true);
        Box("DeadEnd_L_Side1", new Vector3(-3.6f, 1.5f, -4.1f),  new Vector3(3.2f, 3, 0.2f),  matWall, true);
        Box("DeadEnd_L_Side2", new Vector3(-3.6f, 1.5f, -0.9f),  new Vector3(3.2f, 3, 0.2f),  matWall, true);

        // Тупик справа
        Box("DeadEnd_R_Floor", new Vector3(3.5f, -0.1f, 3.5f),   new Vector3(3, 0.2f, 3),     matFloor, true);
        Box("DeadEnd_R_Back",  new Vector3(5.1f, 1.5f, 3.5f),    new Vector3(0.2f, 3, 3.4f),  matWall, true);
        Box("DeadEnd_R_Side1", new Vector3(3.6f, 1.5f, 1.9f),    new Vector3(3.2f, 3, 0.2f),  matWall, true);
        Box("DeadEnd_R_Side2", new Vector3(3.6f, 1.5f, 5.1f),    new Vector3(3.2f, 3, 0.2f),  matWall, true);

        if (buildCeiling)
            Box("Ceiling", new Vector3(0, 3.1f, 0), new Vector3(4.4f, 0.2f, 20.4f), matWall, true);

        // Цель в конце коридора — её подсвечивают акцентным Spot Light
        Box("Goal", new Vector3(0, 0.6f, 8.5f), new Vector3(1, 1.2f, 1), matGoal, true);

        if (createPlayerAndPath) CreatePlayerAndPath();
        if (createLightProbes) CreateProbes();

        Debug.Log("Коридор построен. Дальше: добавьте свет и нажмите Generate Lighting.");
    }

    void CreatePlayerAndPath()
    {
        // Маршрут: вход → левый тупик → коридор → правый тупик → конец
        Vector3[] points =
        {
            new Vector3(0, 1, -9),
            new Vector3(0, 1, -2.5f), new Vector3(-3.8f, 1, -2.5f), new Vector3(0, 1, -2.5f),
            new Vector3(0, 1, 3.5f),  new Vector3(3.8f, 1, 3.5f),   new Vector3(0, 1, 3.5f),
            new Vector3(0, 1, 7f)
        };
        Transform path = new GameObject("Path").transform;
        path.SetParent(root, false);
        for (int i = 0; i < points.Length; i++)
        {
            var p = new GameObject("Point" + i).transform;
            p.SetParent(path, false);
            p.localPosition = points[i];
        }

        // Капсула — динамический объект (НЕ Static)
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.SetParent(root, false);
        player.transform.localPosition = points[0];
        player.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Mat_Player", Color.white);
        var walker = player.AddComponent<CorridorWalker>();
        walker.path = path;
    }

    void CreateProbes()
    {
        var go = new GameObject("LightProbes");
        go.transform.SetParent(root, false);
        var group = go.AddComponent<LightProbeGroup>();
        var list = new System.Collections.Generic.List<Vector3>();

        // Сетка вдоль коридора: шаг 2 м, две высоты (0.5 и 1.5 м), три ряда по ширине
        for (float z = -9; z <= 9; z += 2)
            for (float x = -1.5f; x <= 1.5f; x += 1.5f)
            {
                list.Add(new Vector3(x, 0.5f, z));
                list.Add(new Vector3(x, 1.5f, z));
            }
        // Дополнительные зонды в тупиках (там резкая смена освещения)
        foreach (var c in new[] { new Vector3(-3.5f, 0, -2.5f), new Vector3(3.5f, 0, 3.5f) })
            for (float dx = -1f; dx <= 1f; dx += 1f)
                for (float dz = -1f; dz <= 1f; dz += 1f)
                {
                    list.Add(c + new Vector3(dx, 0.5f, dz));
                    list.Add(c + new Vector3(dx, 1.5f, dz));
                }
        group.probePositions = list.ToArray();
    }

    void Box(string name, Vector3 pos, Vector3 scale, Material mat, bool isStatic)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(root, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = mat;
#if UNITY_EDITOR
        g.isStatic = isStatic;   // Static — для запекания освещения
#endif
    }

    Material MakeMaterial(string name, Color color)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
#if UNITY_EDITOR
        string folder = "Assets/Lab_Lighting/Materials";
        if (!AssetDatabase.IsValidFolder("Assets/Lab_Lighting")) AssetDatabase.CreateFolder("Assets", "Lab_Lighting");
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Lab_Lighting", "Materials");
        string path = $"{folder}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;     // уже есть — не перезаписываем ваши настройки
        var m = new Material(sh) { color = color };
        m.SetFloat("_Smoothness", 0.2f);
        AssetDatabase.CreateAsset(m, path);
        return m;
#else
        return new Material(sh) { color = color };
#endif
    }
}
