using System.Collections.Generic;
using UnityEngine;

public sealed partial class EngineBuildChecklist
{
    private readonly List<Texture2D> styleTextures = new List<Texture2D>();
    private GUIStyle panelStyle;
    private GUIStyle bodyStyle;
    private GUIStyle titleStyle;
    private GUIStyle headingStyle;
    private GUIStyle partHeadingStyle;
    private GUIStyle installedStyle;
    private GUIStyle missingStyle;
    private GUIStyle summaryStyle;
    private GUIStyle partStyle;
    private GUIStyle closeStyle;
    private Color accent;

    private void OnGUI()
    {
        if (!open)
            return;

        if (partStyle == null)
            CreateStyles();

        GUI.depth = -100;
        Color oldColor = GUI.color;
        Matrix4x4 oldMatrix = GUI.matrix;
        float scale = Mathf.Clamp(Screen.height / 900f, 0.85f, 2.5f);
        float screenWidth = Screen.width / scale;
        float screenHeight = Screen.height / scale;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f)) * oldMatrix;
        Fill(new Rect(0, 0, screenWidth, screenHeight), new Color(0f, 0f, 0f, 0.62f));
        float width = Mathf.Min(720f, screenWidth - 32f);
        float height = Mathf.Min(620f, screenHeight - 32f);
        Rect panel = new Rect((screenWidth - width) / 2f, (screenHeight - height) / 2f, width, height);
        GUI.Box(panel, GUIContent.none, panelStyle);
        Fill(new Rect(panel.x + 18, panel.y + 54, width - 36, 1), accent);
        GUI.Label(new Rect(panel.x + 18, panel.y, width - 115, 54), "ENGINE BUILD CHECKLIST", titleStyle);
        bool close = GUI.Button(new Rect(panel.xMax - 82, panel.y + 12, 64, 30), "Close", closeStyle);

        const float countWidth = 90f;
        Rect content = new Rect(panel.x + 18, panel.y + 68, width - 36, height - 115);
        GUILayout.BeginArea(content);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Part", partHeadingStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label("Installed", headingStyle, GUILayout.Width(countWidth));
        GUILayout.Label("Missing", headingStyle, GUILayout.Width(countWidth + 18f));
        GUILayout.EndHorizontal();
        scroll = GUILayout.BeginScrollView(scroll, false, true);
        if (rows.Count == 0)
            GUILayout.Label("Place an engine or engine block on the stand to see its parts.", bodyStyle);

        foreach (Row row in rows)
        {
            Rect line = GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true));
            if (GUI.Button(line, GUIContent.none, partStyle))
                AddToShoppingList(row);

            GUI.Label(new Rect(line.x + 8f, line.y, line.width - 2f * countWidth - 12f, line.height),
                row.Name, bodyStyle);
            Rect count = new Rect(line.xMax - 2f * countWidth, line.y, countWidth, line.height);
            GUI.Label(count, row.Installed.ToString(), row.Installed > 0 ? installedStyle : headingStyle);
            count.x += countWidth;
            GUI.Label(count, row.Missing.ToString(), row.Missing > 0 ? missingStyle : headingStyle);
            DrawRowDivider(line, scale);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        Fill(new Rect(panel.x + 18, panel.yMax - 42f, width - 36, 1f), new Color32(74, 76, 79, 255));
        GUI.Label(new Rect(panel.x + 18, panel.yMax - 41f, width * 0.65f - 22, 41f), feedback, bodyStyle);
        GUI.Label(new Rect(panel.x + width * 0.65f, panel.yMax - 41f, width * 0.35f - 18, 41f),
            string.Format("{0} installed  /  {1} missing", installed, missing), summaryStyle);
        GUI.color = oldColor;
        GUI.matrix = oldMatrix;
        if (close)
            Close();
    }

    private void CreateStyles()
    {
        // Borrow the game's shopping-list typeface at runtime.
        GameObject prefab = Resources.Load<GameObject>("UI/ShopListElement");
        UnityEngine.UI.Text shopText = prefab == null ? null : prefab.GetComponentInChildren<UnityEngine.UI.Text>(true);
        Font font = shopText != null && shopText.font != null ? shopText.font : GUI.skin.font;

        accent = new Color32(226, 155, 62, 255);
        Color textColor = new Color32(238, 239, 240, 255);
        Color muted = new Color32(184, 188, 193, 255);
        bodyStyle = TextStyle(font, 18, textColor, TextAnchor.MiddleLeft);
        titleStyle = TextStyle(font, 24, textColor, TextAnchor.MiddleLeft);
        titleStyle.fontStyle = FontStyle.Bold;
        summaryStyle = TextStyle(font, 16, muted, TextAnchor.MiddleRight);
        headingStyle = TextStyle(font, 16, muted, TextAnchor.MiddleCenter);
        partHeadingStyle = TextStyle(font, 16, muted, TextAnchor.MiddleLeft);
        partHeadingStyle.padding = new RectOffset(8, 0, 0, 0);
        installedStyle = TextStyle(font, 18, new Color32(152, 202, 124, 255), TextAnchor.MiddleCenter);
        missingStyle = TextStyle(font, 18, accent, TextAnchor.MiddleCenter);

        partStyle = new GUIStyle();
        panelStyle = new GUIStyle { border = new RectOffset(16, 16, 16, 16) };
        panelStyle.normal.background = RoundedTexture(new Color32(33, 35, 38, 255), 16);

        partStyle.border = new RectOffset(6, 6, 6, 6);
        partStyle.hover.background = RoundedTexture(new Color32(67, 61, 51, 255), 6);
        partStyle.active.background = RoundedTexture(new Color32(100, 79, 49, 255), 6);
        partStyle.margin = new RectOffset(0, 0, 1, 1);
        closeStyle = TextStyle(font, 14, textColor, TextAnchor.MiddleCenter);
        closeStyle.border = partStyle.border;
        closeStyle.normal.background = RoundedTexture(new Color32(49, 52, 56, 255), 6);
        closeStyle.hover.background = partStyle.hover.background;
        closeStyle.active.background = partStyle.active.background;
        closeStyle.hover.textColor = textColor;
        closeStyle.active.textColor = textColor;
    }

    private static GUIStyle TextStyle(Font font, int size, Color color, TextAnchor alignment)
    {
        var style = new GUIStyle { font = font, fontSize = size, alignment = alignment, clipping = TextClipping.Clip };
        style.normal.textColor = color;
        return style;
    }

    private Texture2D RoundedTexture(Color color, int radius)
    {
        int size = 2 * radius + 2;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, radius - Mathf.Min(x, size - 1 - x) - 0.5f);
                float dy = Mathf.Max(0f, radius - Mathf.Min(y, size - 1 - y) - 0.5f);
                float alpha = Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        styleTextures.Add(texture);
        return texture;
    }

    private static void DrawRowDivider(Rect row, float scale)
    {
        if (Event.current.type != EventType.Repaint || row.width <= 16f)
            return;

        // Draw inside the scroll view's local row rectangle.
        float thickness = 1.5f / scale;
        const float bottomInset = 2f;
        Fill(new Rect(row.xMin + 8f, row.yMax - bottomInset - thickness,
            row.width - 16f, thickness), new Color32(62, 64, 68, 255));
    }

    private static void Fill(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
