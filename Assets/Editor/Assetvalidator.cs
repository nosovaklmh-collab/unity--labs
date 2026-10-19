using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;

// ЛР5: валидатор ассетов.
// Меню: Tools → Asset Pipeline → Validate Selected Models
// Проверяет: габариты (масштаб), пивот, PBR-параметры материалов, карты нормалей.
// Доп. задание: количество треугольников и материал по умолчанию (Default-Material).
public static class AssetValidator
{
    const float MinHeight = 0.1f;
    const float MaxHeight = 4f;
    const float PivotTolerance = 0.05f;
    const int MaxTriangles = 20000;               // доп. 1: порог треугольников
    public const string ReportPath = "Assets/AssetValidationReport.md";

    // Одно предупреждение: к какому объекту относится и текст (используется в окне)
    public struct Warning
    {
        public GameObject obj;
        public string message;
    }

    [MenuItem("Tools/Asset Pipeline/Validate Selected Models")]
    static void ValidateSelection()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("Выделите в иерархии хотя бы один объект для проверки.");
            return;
        }

        int total = Validate(Selection.gameObjects, out string report, null);

        Debug.Log(report);
        Debug.Log(total == 0
            ? "Валидация пройдена без замечаний."
            : $"Найдено предупреждений: {total}. Отчёт сохранён: {ReportPath}");
    }

    // Общий метод проверки: вызывается и из меню, и из окна валидатора
    public static int Validate(GameObject[] objects, out string reportText, List<Warning> warnings)
    {
        var report = new StringBuilder();
        report.AppendLine("# Отчёт валидации ассетов\n");
        report.AppendLine($"Дата проверки: {System.DateTime.Now:dd.MM.yyyy HH:mm}\n");
        int total = 0;

        foreach (var obj in objects)
        {
            report.AppendLine($"## {obj.name}");
            total += CheckBoundsAndPivot(obj, report, warnings);
            total += CheckMaterials(obj, report, warnings);
            total += CheckTriangles(obj, report, warnings);
            report.AppendLine();
        }

        report.AppendLine($"**Итого предупреждений: {total}**");

        File.WriteAllText(ReportPath, report.ToString());
        AssetDatabase.Refresh();   // без этого Unity может не заметить новый файл

        reportText = report.ToString();
        return total;
    }

    static int Warn(GameObject obj, string msg, StringBuilder report, List<Warning> warnings)
    {
        report.AppendLine("- (!) " + msg);
        warnings?.Add(new Warning { obj = obj, message = msg });
        return 1;
    }

    // ---------- 1–2. Габариты и пивот ----------
    static int CheckBoundsAndPivot(GameObject obj, StringBuilder report, List<Warning> warnings)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return Warn(obj, "Не найдено ни одного Renderer.", report, warnings);

        // Общие габариты всех частей модели
        Bounds combined = renderers[0].bounds;
        foreach (var r in renderers)
            combined.Encapsulate(r.bounds);

        int count = 0;
        float height = combined.size.y;

        if (height < MinHeight || height > MaxHeight)
            count += Warn(obj, $"Подозрительная высота: {height:F2} м (ожидается {MinHeight}-{MaxHeight} м).",
                          report, warnings);
        else
            report.AppendLine($"- (OK) Высота в разумных пределах: {height:F2} м.");

        // Пивот = позиция объекта. Сравниваем с нижней точкой модели.
        float pivotOffset = combined.min.y - obj.transform.position.y;
        if (Mathf.Abs(pivotOffset) > PivotTolerance)
            count += Warn(obj, $"Пивот не у основания: смещение {pivotOffset:F2} м относительно нижней точки модели.",
                          report, warnings);
        else
            report.AppendLine("- (OK) Пивот у основания модели.");

        return count;
    }

    // ---------- 3–4. Материалы и карты нормалей ----------
    static int CheckMaterials(GameObject obj, StringBuilder report, List<Warning> warnings)
    {
        int count = 0;
        var renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null)
                {
                    count += Warn(obj, $"У \"{r.name}\" пустой слот материала.", report, warnings);
                    continue;
                }

                // Доп. 2: материал по умолчанию — не назначен осознанно
                string matPath = AssetDatabase.GetAssetPath(mat);
                bool isDefault = mat.name == "Default-Material"
                              || matPath == "Resources/unity_builtin_extra"
                              || matPath.StartsWith("Packages/com.unity.render-pipelines.universal");
                if (isDefault)
                {
                    count += Warn(obj, $"Материал \"{mat.name}\" — стандартный материал Unity: материал не назначен осознанно.",
                                  report, warnings);
                    continue;
                }

                // PBR-параметры по умолчанию
                if (mat.HasProperty("_Metallic") && mat.HasProperty("_Smoothness"))
                {
                    float metallic = mat.GetFloat("_Metallic");
                    float smoothness = mat.GetFloat("_Smoothness");

                    if (Mathf.Approximately(metallic, 0f) && Mathf.Approximately(smoothness, 0.5f))
                        count += Warn(obj, $"Материал \"{mat.name}\": Metallic=0, Smoothness=0.5 — похоже на значения по умолчанию.",
                                      report, warnings);
                    else
                        report.AppendLine($"- (OK) Материал \"{mat.name}\": Metallic={metallic:F2}, Smoothness={smoothness:F2}.");
                }

                // Карта нормалей должна быть импортирована как Normal Map
                if (mat.HasProperty("_BumpMap"))
                {
                    Texture normalMap = mat.GetTexture("_BumpMap");
                    if (normalMap != null)
                    {
                        string texPath = AssetDatabase.GetAssetPath(normalMap);
                        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;

                        if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                            count += Warn(obj, $"Текстура \"{normalMap.name}\" подключена как Normal Map, но Texture Type = {importer.textureType}.",
                                          report, warnings);
                    }
                }
            }
        }
        return count;
    }

    // ---------- Доп. 1. Количество треугольников ----------
    static int CheckTriangles(GameObject obj, StringBuilder report, List<Warning> warnings)
    {
        int tris = 0;
        foreach (var mf in obj.GetComponentsInChildren<MeshFilter>())
            if (mf.sharedMesh != null) tris += mf.sharedMesh.triangles.Length / 3;
        foreach (var smr in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (smr.sharedMesh != null) tris += smr.sharedMesh.triangles.Length / 3;

        if (tris > MaxTriangles)
            return Warn(obj, $"Слишком много треугольников: {tris} (порог {MaxTriangles}).", report, warnings);

        report.AppendLine($"- (OK) Треугольников: {tris}.");
        return 0;
    }
}