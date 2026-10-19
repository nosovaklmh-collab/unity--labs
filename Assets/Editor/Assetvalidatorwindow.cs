using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Доп. 3: окно валидатора с кнопкой запуска и прокручиваемым списком предупреждений.
// Меню: Tools → Asset Pipeline → Validator Window
public class AssetValidatorWindow : EditorWindow
{
    private readonly List<AssetValidator.Warning> warnings = new List<AssetValidator.Warning>();
    private Vector2 scroll;
    private int checkedCount = -1;

    [MenuItem("Tools/Asset Pipeline/Validator Window")]
    static void Open()
    {
        GetWindow<AssetValidatorWindow>("Asset Validator");
    }

    void OnGUI()
    {
        GUILayout.Label("Валидатор ассетов", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Выделите объекты в Hierarchy и нажмите «Проверить».", MessageType.None);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Проверить выделенные", GUILayout.Height(28)))
                Run(Selection.gameObjects);
            if (GUILayout.Button("Открыть отчёт .md", GUILayout.Height(28)))
                EditorUtility.RevealInFinder(AssetValidator.ReportPath);
        }

        if (checkedCount < 0) return;

        EditorGUILayout.Space();
        if (warnings.Count == 0)
        {
            EditorGUILayout.HelpBox($"Проверено объектов: {checkedCount}. Валидация пройдена без замечаний.", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox($"Проверено объектов: {checkedCount}. Предупреждений: {warnings.Count}", MessageType.Warning);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var w in warnings)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.HelpBox($"{(w.obj != null ? w.obj.name : "?")}: {w.message}", MessageType.Warning);
                // Кнопка выделяет проблемный объект в сцене
                if (w.obj != null && GUILayout.Button("Найти", GUILayout.Width(60), GUILayout.Height(38)))
                {
                    Selection.activeGameObject = w.obj;
                    EditorGUIUtility.PingObject(w.obj);
                    SceneView.FrameLastActiveSceneView();
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void Run(GameObject[] objects)
    {
        warnings.Clear();
        if (objects.Length == 0)
        {
            ShowNotification(new GUIContent("Ничего не выделено"));
            checkedCount = -1;
            return;
        }
        AssetValidator.Validate(objects, out _, warnings);
        checkedCount = objects.Length;
    }
}