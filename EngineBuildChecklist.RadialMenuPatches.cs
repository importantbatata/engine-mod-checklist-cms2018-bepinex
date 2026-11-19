using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

public sealed partial class EngineBuildChecklist
{
    private const string ChecklistOption = "local_engine_build_checklist";

    private PieMenuController closingMenu;
    private bool openFromRadial;

    private void PatchRadialMenu()
    {
        harmony.Patch(AccessTools.Method(typeof(GameManager), "CheckOptionIsAvaiable", new[] { typeof(string) }),
            postfix: new HarmonyMethod(typeof(EngineBuildChecklist), "SetChecklistAvailability"));
        PatchPieMenu("PrepareIcons", "AppendChecklistOption", "SetChecklistIcon");
        PatchPieMenu("SetPieMenuDescription", "SetChecklistDescription", null);
        PatchPieMenu("CheckSelectedOption", "SelectChecklist", null);
        PatchPieMenu("OnCompleteCloseAnimation", null, "OpenAfterRadialCloses");
    }

    private void PatchPieMenu(string target, string prefix, string postfix)
    {
        Type plugin = typeof(EngineBuildChecklist);
        harmony.Patch(AccessTools.Method(typeof(PieMenuController), target),
            prefix: prefix == null ? null : new HarmonyMethod(plugin, prefix),
            postfix: postfix == null ? null : new HarmonyMethod(plugin, postfix));
    }

    private static void AppendChecklistOption(ref string[] __0, string ___PreviousValueFromIni, FileLoader.IniFile ___pieMenuConfig)
    {
        if (instance == null || __0 == null)
            return;

        try
        {
            __0 = BuildChecklistOptions(__0, ___PreviousValueFromIni, ___pieMenuConfig);
        }
        catch (Exception error)
        {
            instance.Logger.LogWarning("Could not add the checklist to the radial menu: " + error);
        }
    }

    private static string[] BuildChecklistOptions(string[] original, string menuKey, FileLoader.IniFile config)
    {
        foreach (string option in original)
        {
            if (option == ChecklistOption)
                return original;
        }

        GameScript game = GameScript.Get();
        GameMode mode = GameMode.Get();
        if (game == null || mode == null || game.GetEngineStand() == null)
            return original;

        bool standWheel = menuKey == "!EngineStand" ||
            (!string.IsNullOrEmpty(menuKey) && menuKey.EndsWith("EngineStand", StringComparison.Ordinal)) ||
            (game.IsOnMachine() && game.GetMachineOnMouseOverType() == IOSpecialType.EngineStand);
        if (!standWheel)
            standWheel = Array.Exists(original, delegate(string option)
            {
                return option != null && option.StartsWith("engine_", StringComparison.Ordinal);
            });

        if (!standWheel)
        {
            // The car and stand can share the same garage wheel depending on what the raycast hit.
            // Offer quick access from that wheel while there is an engine on the stand.
            if (!IsBuildMode(mode.GetPreviousMode()) || !HasStandEngine(game))
                return original;

            // Submenus reuse the previous INI key; only extend the top-level options.
            if (config == null || string.IsNullOrEmpty(menuKey))
                return original;

            string[] primaryOptions = config.GetString(menuKey, string.Empty).Split(',');
            if (primaryOptions.Length != original.Length)
                return original;

            for (int i = 0; i < primaryOptions.Length; i++)
            {
                if (primaryOptions[i].Trim() != original[i])
                    return original;
            }
        }

        var options = new string[original.Length + 1];
        Array.Copy(original, options, original.Length);
        options[options.Length - 1] = ChecklistOption;
        instance.Logger.LogInfo("Added checklist to radial wheel (" + menuKey + ").");
        return options;
    }

    private static bool HasStandEngine(GameScript game)
    {
        if (game == null || game.GetEngineStand() == null)
            return false;

        foreach (GameObject root in game.GetObjectsOnEngineStand())
        {
            if (root != null && root.transform.IsChildOf(game.GetEngineStand()))
                return true;
        }
        return false;
    }

    private static void SetChecklistAvailability(string __0, ref bool __result)
    {
        if (__0 == ChecklistOption)
            __result = HasStandEngine(GameScript.Get());
    }


    private static void SetChecklistIcon(List<string> ___NameList, List<Transform> ___PathList)
    {
        int index = ___NameList.IndexOf(ChecklistOption);
        if (index < 0 || index >= ___PathList.Count || ___PathList[index] == null)
            return;

        var image = ___PathList[index].GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            Sprite icon = Resources.Load<Sprite>("Sprites/PieMenu/list_open");
            image.sprite = icon != null ? icon : Resources.Load<Sprite>("Sprites/PieMenu/engine_new");
        }
    }

    private static bool SetChecklistDescription(int __0, List<string> ___NameList,
        UnityEngine.UI.Text ___TitleText, UnityEngine.UI.Text ___DescText, Vector3 ___TitleTextPositionWithBonusText)
    {
        if (!IsChecklistOption(__0, ___NameList))
            return true;

        ___TitleText.text = "Engine Build Checklist";
        ___TitleText.rectTransform.anchoredPosition = ___TitleTextPositionWithBonusText;
        ___DescText.text = HasStandEngine(GameScript.Get())
            ? "View engine parts and add them to the shopping list."
            : "Place an engine on the stand first.";
        return false;
    }

    [HarmonyPriority(Priority.First)]
    private static void SelectChecklist(PieMenuController __instance, ref int ___CurrOption, List<string> ___NameList)
    {
        if (instance == null || !IsChecklistOption(___CurrOption, ___NameList))
            return;

        if (HasStandEngine(GameScript.Get()))
            instance.closingMenu = __instance;
        // Use the same close and return-to-game path as releasing the wheel without a selection.
        ___CurrOption = -1;
    }

    private static void OpenAfterRadialCloses(PieMenuController __instance)
    {
        if (instance == null || instance.closingMenu != __instance)
            return;

        instance.closingMenu = null;
        instance.openFromRadial = true;
    }

    private static bool IsChecklistOption(int index, List<string> names)
    {
        return index >= 0 && index < names.Count && names[index] == ChecklistOption;
    }
}
