using UnityEngine;
using UnityEditor;

// ЛР5: массовое применение рекомендованных настроек импорта.
// Меню: Tools → Asset Pipeline → Apply Recommended Model Settings
// Выделять нужно ФАЙЛЫ моделей в окне Project, а не объекты в сцене.
// Scale Factor сознательно не трогаем: у разных файлов он разный.
public static class ModelImportPresetApplier
{
    const string MenuPath = "Tools/Asset Pipeline/Apply Recommended Model Settings";

    [MenuItem(MenuPath)]
    static void ApplyToSelection()
    {
        int applied = 0;

        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;   // выделен не файл модели — пропускаем

            importer.importBlendShapes = false;                         // морфинг низкополигональным моделям не нужен
            importer.meshCompression = ModelImporterMeshCompression.Off; // без потерь точности
            importer.isReadable = false;                                 // не держать копию меша в памяти

            importer.SaveAndReimport();
            applied++;
            Debug.Log($"Настройки применены: {path}");
        }

        Debug.Log(applied == 0
            ? "В выделении не найдено файлов моделей."
            : $"Готово. Настройки применены к {applied} ассетам.");
    }
}