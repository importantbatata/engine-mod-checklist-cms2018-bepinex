using System;
using System.Reflection;
using HarmonyLib;

public sealed partial class EngineBuildChecklist
{
    private void PatchGameplayInput()
    {
        foreach (MethodInfo method in typeof(InputManager).GetMethods(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            if (!method.Name.StartsWith("Gameplay", StringComparison.Ordinal) &&
                method.Name != "CameraMoveX" && method.Name != "CameraMoveY" &&
                method.Name != "CameraScroll")
                continue;

            string prefix = method.ReturnType == typeof(bool) ? "BlockButton" :
                method.ReturnType == typeof(float) ? "BlockAxis" : null;
            if (prefix != null && method.GetParameters().Length == 0)
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(EngineBuildChecklist), prefix));
        }
    }

    private static bool BlockButton(ref bool __result)
    {
        if (instance == null || !instance.open)
            return true;

        __result = false;
        return false;
    }

    private static bool BlockAxis(ref float __result)
    {
        if (instance == null || !instance.open)
            return true;

        __result = 0f;
        return false;
    }
}
