using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// «Лампада» — настраивает анимации с Mixamo и собирает Animator девочки одной кнопкой.
// Меню Unity: Lampada → Собрать анимации девочки
//
// Перед запуском положите файлы в папку Assets/Lab8_Lampada/Animations и назовите их ТОЧНО так:
//   Idle.fbx        — стоит (обязательно)
//   Walk.fbx        — идёт (обязательно)
//   Run.fbx         — бежит
//   CrouchIdle.fbx  — сидит на корточках
//   CrouchWalk.fbx  — крадётся на корточках
//   Jump.fbx        — прыжок
// Скрипт сам:
//   1) переключит каждый файл на Humanoid и включит повтор (Loop) там, где нужно;
//   2) создаст GirlAnimator.controller с параметрами Speed, Grounded, Crouch, Jump;
//   3) поставит его в Animator куклы внутри Player.
public static class LampadaAnimatorBuilder
{
    const string AnimFolder = "Assets/Lab8_Lampada/Animations";
    const string ControllerPath = AnimFolder + "/GirlAnimator.controller";

    // имя файла → нужно ли повторять по кругу
    static readonly Dictionary<string, bool> Clips = new Dictionary<string, bool>
    {
        { "Idle", true }, { "Walk", true }, { "Run", true },
        { "CrouchIdle", true }, { "CrouchWalk", true }, { "Jump", false },
    };

    [MenuItem("Lampada/Собрать анимации девочки")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(AnimFolder))
        {
            AssetDatabase.CreateFolder("Assets/Lab8_Lampada", "Animations");
            EditorUtility.DisplayDialog("Лампада",
                "Создана папка Assets/Lab8_Lampada/Animations.\n\n" +
                "Положите туда файлы с Mixamo с именами:\nIdle, Walk, Run, CrouchIdle, CrouchWalk, Jump\n\n" +
                "и снова нажмите Lampada → Собрать анимации девочки.", "Понятно");
            return;
        }

        // ---------- 1. Настраиваем импорт каждого файла ----------
        var found = new Dictionary<string, AnimationClip>();
        foreach (var pair in Clips)
        {
            string path = $"{AnimFolder}/{pair.Key}.fbx";
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;   // файла нет — пропускаем

            SetupImporter(importer, pair.Key, pair.Value);

            var clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
            if (clip != null) found[pair.Key] = clip;
        }

        if (!found.ContainsKey("Idle") || !found.ContainsKey("Walk"))
        {
            EditorUtility.DisplayDialog("Лампада",
                "Не найдены Idle.fbx и/или Walk.fbx в папке\n" + AnimFolder +
                "\n\nПроверьте названия файлов (большие и маленькие буквы тоже важны).", "Ок");
            return;
        }

        // Скорости берём из скрипта управления, чтобы ноги совпадали с движением
        float walk = 2.2f, run = 4.2f, crouch = 1.1f;
        var player = GameObject.FindWithTag("Player");
        var ctrl = player != null ? player.GetComponent<LampadaGirlController>() : null;
        if (ctrl != null) { walk = ctrl.walkSpeed; run = ctrl.runSpeed; crouch = ctrl.crouchSpeed; }

        // ---------- 2. Собираем Animator Controller ----------
        AssetDatabase.DeleteAsset(ControllerPath);   // пересоздаём при каждом запуске
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Crouch", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        // Обычное движение: стоит → идёт → бежит, смешивается по Speed
        var locoState = controller.CreateBlendTreeInController("Locomotion", out BlendTree loco, 0);
        loco.blendType = BlendTreeType.Simple1D;
        loco.blendParameter = "Speed";
        loco.useAutomaticThresholds = false;
        loco.AddChild(found["Idle"], 0f);
        loco.AddChild(found["Walk"], walk);
        if (found.TryGetValue("Run", out var runClip)) loco.AddChild(runClip, run);
        sm.defaultState = locoState;

        // На корточках
        bool hasCrouch = found.ContainsKey("CrouchIdle") || found.ContainsKey("CrouchWalk");
        if (hasCrouch)
        {
            var crouchState = controller.CreateBlendTreeInController("Crouch", out BlendTree crouchTree, 0);
            crouchTree.blendType = BlendTreeType.Simple1D;
            crouchTree.blendParameter = "Speed";
            crouchTree.useAutomaticThresholds = false;
            if (found.TryGetValue("CrouchIdle", out var ci)) crouchTree.AddChild(ci, 0f);
            if (found.TryGetValue("CrouchWalk", out var cw)) crouchTree.AddChild(cw, crouch);

            var toCrouch = locoState.AddTransition(crouchState);
            Quick(toCrouch, 0.15f);
            toCrouch.AddCondition(AnimatorConditionMode.If, 0, "Crouch");

            var fromCrouch = crouchState.AddTransition(locoState);
            Quick(fromCrouch, 0.15f);
            fromCrouch.AddCondition(AnimatorConditionMode.IfNot, 0, "Crouch");
        }

        // Прыжок
        if (found.TryGetValue("Jump", out var jumpClip))
        {
            var jumpState = sm.AddState("Jump");
            jumpState.motion = jumpClip;

            var toJump = sm.AddAnyStateTransition(jumpState);
            Quick(toJump, 0.1f);
            toJump.canTransitionToSelf = false;
            toJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");

            var land = jumpState.AddTransition(locoState);
            land.hasExitTime = true;
            land.exitTime = 0.3f;          // хотя бы треть прыжка проиграть
            land.duration = 0.15f;
            land.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        }

        AssetDatabase.SaveAssets();

        // ---------- 3. Ставим контроллер кукле ----------
        string result = "Контроллер создан: " + ControllerPath;
        if (player != null)
        {
            var animator = player.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                result += "\n\nВ Player не найден Animator. Добавьте компонент Animator на куклу и запустите ещё раз.";
            }
            else
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                EditorSceneManager.MarkSceneDirty(player.scene);
                result += "\nПоставлен в Animator объекта: " + animator.gameObject.name;

                if (animator.avatar == null || !animator.avatar.isHuman)
                    result += "\n\n⚠ У куклы нет Humanoid-аватара — анимации не заиграют. " +
                              "Выделите FBX куклы → Rig → Animation Type: Humanoid → Apply, " +
                              "затем в Animator куклы в поле Avatar выберите её аватар.";
            }
        }
        else
        {
            result += "\n\nВ сцене нет объекта с тегом Player — перетащите GirlAnimator в Animator куклы вручную.";
        }

        string missing = string.Join(", ", Clips.Keys.Where(k => !found.ContainsKey(k)));
        if (missing.Length > 0) result += "\n\nНе найдены (можно добавить позже): " + missing;

        Debug.Log("Лампада: " + result);
        EditorUtility.DisplayDialog("Лампада — анимации", result + "\n\nСохраните сцену (Ctrl+S) и нажмите Play!", "Ок");
    }

    // Humanoid + свой аватар у каждого файла Mixamo, повтор и «запекание» корня
    static void SetupImporter(ModelImporter importer, string clipName, bool loop)
    {
        if (importer.animationType != ModelImporterAnimationType.Human ||
            importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
        }

        var clips = importer.defaultClipAnimations;
        if (clips == null || clips.Length == 0) return;

        foreach (var c in clips)
        {
            c.name = clipName;
            c.loopTime = loop;
            c.lockRootRotation = true;            // Root Rotation: Bake Into Pose
            c.keepOriginalOrientation = true;
            c.lockRootPositionXZ = true;          // Root XZ: Bake Into Pose (персонаж не «уезжает»)
            c.keepOriginalPositionXZ = true;
            c.lockRootHeightY = loop;             // для прыжка высоту не запекаем — прыгает скрипт
            c.heightFromFeet = true;              // ноги на полу
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    // Переход без ожидания конца анимации
    static void Quick(AnimatorStateTransition t, float duration)
    {
        t.hasExitTime = false;
        t.duration = duration;
    }
}