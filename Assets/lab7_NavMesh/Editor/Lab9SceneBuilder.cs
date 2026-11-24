using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

// ЛР «Патрульный враг на NavMesh» — строит сцену в ОТКРЫТОЙ сейчас сцене.
// Меню: Lab9 → Построить сцену NavMesh
// Скрипты врага (EnemyMover, EnemyBrain, Door) — пишете сами по методичке.
public static class Lab9SceneBuilder
{
    const string MatFolder = "Assets/Lab9_NavMesh/Materials";

    [MenuItem("Lab9/Построить сцену NavMesh")]
    public static void Build()
    {
        // Убираем то, что могло остаться от прошлого запуска
        foreach (var n in new[] { "Level", "Door", "Player", "Enemy", "Waypoints" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        var gray  = Mat("Lab9_Floor",  new Color(0.55f, 0.55f, 0.55f));
        var wall  = Mat("Lab9_Wall",   new Color(0.35f, 0.35f, 0.40f));
        var brown = Mat("Lab9_Door",   new Color(0.45f, 0.30f, 0.18f));
        var blue  = Mat("Lab9_Player", new Color(0.20f, 0.45f, 0.90f));
        var red   = Mat("Lab9_Enemy",  new Color(0.85f, 0.20f, 0.20f));

        // ---------- Level: всё, что запекается в NavMesh ----------
        var level = new GameObject("Level");
        Cube("Floor",  new Vector3(0f, -0.5f, 0f),     new Vector3(20f, 1f, 20f),   gray, level.transform);
        Cube("Wall_L", new Vector3(-5.375f, 1f, 0f),   new Vector3(9.25f, 2f, 0.5f), wall, level.transform);
        Cube("Wall_R", new Vector3(3.875f, 1f, 0f),    new Vector3(6.25f, 2f, 0.5f), wall, level.transform);
        // Проход 1.5 м: X от −0.75 до 0.75. Обход 3 м справа: X от 7 до 10.

        var surface = level.AddComponent<NavMeshSurface>();
        surface.agentTypeID = 0;                                  // Humanoid
        surface.collectObjects = CollectObjects.Children;         // только то, что внутри Level
        surface.overrideVoxelSize = true;
        surface.voxelSize = 0.15f;

        // ---------- Дверь: динамическое препятствие ----------
        var door = Cube("Door", new Vector3(0f, 1f, 0f), new Vector3(1.5f, 2f, 0.4f), brown, null);
        var obstacle = door.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.center = Vector3.zero;
        obstacle.size = Vector3.one;           // умножается на Scale двери
        obstacle.carving = true;
        obstacle.carveOnlyStationary = false;  // дверь двигается

        // ---------- Игрок ----------
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, -6f);
        player.GetComponent<Renderer>().sharedMaterial = blue;

        // ---------- Враг (только NavMeshAgent, скрипты добавите сами) ----------
        var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.transform.position = new Vector3(-8f, 1f, -8f);
        enemy.GetComponent<Renderer>().sharedMaterial = red;
        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.agentTypeID = 0;

        // «Нос», чтобы было видно, куда смотрит враг
        var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "Nose";
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        nose.transform.SetParent(enemy.transform, false);
        nose.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
        nose.transform.localScale = Vector3.one * 0.25f;
        nose.GetComponent<Renderer>().sharedMaterial = red;

        // ---------- Точки патруля ----------
        var wps = new GameObject("Waypoints");
        Point("WP_1", new Vector3(-8f, 0f, -8f), wps.transform);
        Point("WP_2", new Vector3(-8f, 0f,  8f), wps.transform);
        Point("WP_3", new Vector3( 8f, 0f,  8f), wps.transform);

        // ---------- Камера сверху-сбоку ----------
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 18f, -16f);
            cam.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(level.scene);
        Selection.activeGameObject = level;
        EditorUtility.DisplayDialog("Lab9",
            "Сцена построена.\n\n" +
            "1) Window → AI → Navigation → Agents → Humanoid: Radius 0.4, Height 1.8, Step 0.3, Slope 45.\n" +
            "2) Выделен Level → в NavMesh Surface нажмите Bake.\n" +
            "3) Сохраните сцену (Cmd+S).", "Ок");
    }

    static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void Point(string name, Vector3 pos, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        // Значок в сцене, чтобы точки было видно
        var icon = EditorGUIUtility.IconContent("sv_label_3").image as Texture2D;
        if (icon != null) EditorGUIUtility.SetIconForObject(go, icon);
    }

    static Material Mat(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder(MatFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Lab9_NavMesh"))
                AssetDatabase.CreateFolder("Assets", "Lab9_NavMesh");
            AssetDatabase.CreateFolder("Assets/Lab9_NavMesh", "Materials");
        }
        string path = $"{MatFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color); else mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
