using HarmonyLib;
using UnityEngine;

public sealed partial class EngineBuildChecklist
{
    private void PatchCursorLock()
    {
        harmony.Patch(AccessTools.Method(typeof(Cursor3D), "NormalCursorLock", new[] { typeof(bool) }),
            prefix: new HarmonyMethod(typeof(EngineBuildChecklist), "AllowGameCursorLock"));
    }

    private static bool AllowGameCursorLock()
    {
        return instance == null || !instance.open;
    }

    private void LateUpdate()
    {
        if (open)
            UnlockCursor();
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
