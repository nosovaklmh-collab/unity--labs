using UnityEngine;
using UnityEditor;
using System.IO;

// Доп. 4: при импорте новой модели автоматически добавляет строку в CREDITS.md
// (файл лежит в корне проекта/репозитория, рядом с папкой Assets).
// Источник определяется по пути: если в нём есть "kenney" или "quaternius".
// Плюс пункт меню Tools → Asset Pipeline → Update CREDITS.md — обновить вручную.
public class CreditsAutoWriter : AssetPostprocessor
{
    static string CreditsPath =>
        Path.Combine(Path.GetDirectoryName(Application.dataPath), "CREDITS.md");

    // Срабатывает автоматически при импорте/реимпорте модели
    void OnPostprocessModel(GameObject model)
    {
        // Учитываем только наши модели из Assets/Models,
        // а служебные модели Unity и пакетов (Packages, TutorialInfo и т.п.) пропускаем
        if (!assetPath.StartsWith("Assets/Models/")) return;
        AddEntry(assetPath);
    }

    // Ручной запуск: проходит по всем моделям в папке Assets/Models
    [MenuItem("Tools/Asset Pipeline/Update CREDITS.md")]
    static void UpdateAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" });
        int added = 0;
        foreach (string guid in guids)
            if (AddEntry(AssetDatabase.GUIDToAssetPath(guid))) added++;

        Debug.Log($"CREDITS.md: найдено моделей {guids.Length}, добавлено новых записей {added}. Файл: {CreditsPath}");
        EditorUtility.RevealInFinder(CreditsPath);   // сразу показать файл в Проводнике
    }

    static bool AddEntry(string path)
    {
        if (!File.Exists(CreditsPath))
            File.WriteAllText(CreditsPath,
                "# CREDITS\n\n| Материал | Источник | Лицензия |\n|----------|----------|----------|\n");

        string name = Path.GetFileNameWithoutExtension(path);
        if (File.ReadAllText(CreditsPath).Contains($"\"{name}\"")) return false;  // запись уже есть

        string lower = path.ToLower();
        string source = lower.Contains("kenney") ? "Kenney.nl"
                      : lower.Contains("quaternius") ? "Quaternius.com"
                      : "указать вручную";
        string license = source == "указать вручную" ? "указать вручную" : "CC0";

        File.AppendAllText(CreditsPath, $"| Модель \"{name}\" | {source} | {license} |\n");
        Debug.Log($"CREDITS.md: добавлена запись о модели {name}");
        return true;
    }
}