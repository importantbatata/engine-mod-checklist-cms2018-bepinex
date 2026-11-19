using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

[BepInPlugin(PluginId, "Engine Build Checklist", "1.0.11")]
public sealed partial class EngineBuildChecklist : BaseUnityPlugin
{
    private const string PluginId = "local.cms18.enginebuildchecklist";

    private sealed class Row
    {
        public string Name;
        public int Installed;
        public int Missing;
    }

    private static EngineBuildChecklist instance;
    private readonly List<Row> rows = new List<Row>();
    private Harmony harmony;
    private ConfigEntry<KeyboardShortcut> shortcut;
    private GameScript activeGame;
    private bool open;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;
    private Vector2 scroll;
    private string feedback;
    private float refreshAt;
    private int installed;
    private int missing;
    private bool firstRefresh;

    private void Awake()
    {
        shortcut = Config.Bind("Controls", "ToggleChecklist", new KeyboardShortcut(KeyCode.F8),
            "Open or close the engine stand checklist in the garage.");
        harmony = new Harmony(PluginId);
        try
        {
            PatchGameplayInput();
            PatchCursorLock();
            PatchRadialMenu();
            instance = this;
            Logger.LogInfo("Ready. Press " + shortcut.Value + " in the garage to open the engine checklist.");
        }
        catch
        {
            harmony.UnpatchSelf();
            throw;
        }
    }

    private void Update()
    {
        if (open)
        {
            if (shortcut.Value.IsDown() || Input.GetKeyDown(KeyCode.Escape) ||
                activeGame == null || !CanShowChecklist())
            {
                Close();
                return;
            }
        }
        else
        {
            bool radial = openFromRadial;
            openFromRadial = false;
            if (radial && !HasStandEngine(GameScript.Get()))
                return;
            if (!radial && !shortcut.Value.IsDown())
                return;

            if (!CanShowChecklist())
            {
                GameMode mode = GameMode.Get();
                Logger.LogInfo("Checklist shortcut received outside garage mode (" +
                    (mode == null ? "no active mode" : mode.GetCurrentMode().ToString()) + ").");
                return;
            }

            activeGame = GameScript.Get();
            if (activeGame == null || activeGame.GetEngineStand() == null || UIManager.Get() == null)
            {
                Logger.LogInfo("Checklist shortcut received before the garage and engine stand were ready.");
                return;
            }

            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            scroll = Vector2.zero;
            feedback = "Click a part to add one to the shopping list.";
            open = true;
            UnlockCursor();
            refreshAt = 0f;
            firstRefresh = true;
        }

        if (Time.unscaledTime < refreshAt)
            return;

        refreshAt = Time.unscaledTime + 0.25f;
        try
        {
            RefreshParts();
            if (firstRefresh)
            {
                firstRefresh = false;
                Logger.LogInfo(string.Format("Checklist opened in {0} with {1} installed and {2} missing parts.",
                    GameMode.Get().GetCurrentMode(), installed, missing));
            }
        }
        catch (Exception error)
        {
            Close();
            Logger.LogError(error);
        }
    }

    private static bool CanShowChecklist()
    {
        GameMode mode = GameMode.Get();
        if (mode == null)
            return false;

        return IsBuildMode(mode.GetCurrentMode());
    }

    private static bool IsBuildMode(gameMode current)
    {
        return current == gameMode.Garage || current == gameMode.GarageAssemble ||
            current == gameMode.GarageDisassemble || current == gameMode.ExamineCondition ||
            current == gameMode.PartSelect || current == gameMode.PartSelectMount ||
            current == gameMode.PartUnMount;
    }

    private void RefreshParts()
    {
        rows.Clear();
        installed = 0;
        missing = 0;
        var byId = new Dictionary<string, Row>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<PartScript>();
        GameInventory inventory = GameInventory.Instance;

        foreach (GameObject root in activeGame.GetObjectsOnEngineStand())
        {
            if (root == null || inventory == null)
                continue;

            // Missing parts can be hidden. Read their mount state directly.
            foreach (PartScript part in root.GetComponentsInChildren<PartScript>(true))
            {
                if (part == null || !seen.Add(part))
                    continue;

                bool isMissing = part.IsUnmounted();
                string id = isMissing ? part.GetID() : part.GetIDWithTuned();
                if (string.IsNullOrEmpty(id))
                    continue;

                // Match the stand loader's exclusion for SpecialGroup 1.
                GameInventory.PartProperty property = inventory.GetItemProperty(id);
                if (property != null && property.SpecialGroup == 1)
                    continue;

                Row row;
                if (!byId.TryGetValue(id, out row))
                {
                    row = new Row { Name = inventory.GetItemLocalizeName(id) };
                    if (string.IsNullOrEmpty(row.Name))
                        row.Name = id;
                    byId.Add(id, row);
                    rows.Add(row);
                }

                if (isMissing)
                {
                    row.Missing++;
                    missing++;
                }
                else
                {
                    row.Installed++;
                    installed++;
                }
            }
        }

        rows.Sort(delegate(Row a, Row b)
        {
            int order = (b.Missing > 0).CompareTo(a.Missing > 0);
            return order != 0 ? order : StringComparer.CurrentCultureIgnoreCase.Compare(a.Name, b.Name);
        });
    }

    private void AddToShoppingList(Row row)
    {
        try
        {
            UIManager ui = UIManager.Get();
            if (ui == null)
                return;

            // The game's own add action passes the localized part name.
            ui.AddToShopList(row.Name);
            feedback = "Added to shopping list: " + row.Name;
        }
        catch (Exception error)
        {
            feedback = "Could not add the part to the shopping list.";
            Logger.LogError(error);
        }
    }

    private void Close()
    {
        if (!open)
            return;

        open = false;
        activeGame = null;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    private void OnDisable()
    {
        Close();
    }

    private void OnDestroy()
    {
        Close();
        if (instance == this)
            instance = null;
        foreach (Texture2D texture in styleTextures)
            Destroy(texture);
        if (harmony != null)
            harmony.UnpatchSelf();
    }
}
