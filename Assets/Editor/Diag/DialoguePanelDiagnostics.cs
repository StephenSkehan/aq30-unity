using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

#if UNITY_EDITOR
public class DialoguePanelDiagnostics : EditorWindow
{
    private GameObject prefab;
    private Vector2 scrollPos;

    [MenuItem("Tools/Diagnose DialoguePanel Prefab")]
    static void ShowWindow()
    {
        GetWindow<DialoguePanelDiagnostics>("DialoguePanel Diagnostics");
    }

    void OnGUI()
    {
        GUILayout.Label("DialoguePanel Prefab Diagnostics", EditorStyles.boldLabel);
        GUILayout.Space(10);

        prefab = (GameObject)EditorGUILayout.ObjectField("DialoguePanel Prefab:", prefab, typeof(GameObject), false);

        if (GUILayout.Button("Auto-Find DialoguePanel Prefab"))
        {
            string[] guids = AssetDatabase.FindAssets("DialoguePanel t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"Found prefab at: {path}");
            }
            else
            {
                Debug.LogError("Could not find DialoguePanel prefab!");
            }
        }

        GUILayout.Space(10);

        if (prefab != null && GUILayout.Button("Analyze Structure"))
        {
            AnalyzePrefab();
        }

        GUILayout.Space(20);

        // Display results
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.EndScrollView();
    }

    void AnalyzePrefab()
    {
        Debug.Log("========================================");
        Debug.Log("DIALOGUEPANEL PREFAB ANALYSIS");
        Debug.Log("========================================\n");

        // Analyze root
        AnalyzeGameObject(prefab, 0);

        // Analyze all children recursively
        AnalyzeChildren(prefab.transform, 1);

        Debug.Log("\n========================================");
        Debug.Log("ANALYSIS COMPLETE");
        Debug.Log("========================================");
    }

    void AnalyzeChildren(Transform parent, int depth)
    {
        foreach (Transform child in parent)
        {
            AnalyzeGameObject(child.gameObject, depth);
            AnalyzeChildren(child, depth + 1);
        }
    }

    void AnalyzeGameObject(GameObject go, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}┌─ [{go.name}]");

        // Active state
        Debug.Log($"{indent}│  Active: {go.activeSelf}");

        // RectTransform
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log($"{indent}│  === RectTransform ===");
            Debug.Log($"{indent}│  AnchorMin: {rt.anchorMin}");
            Debug.Log($"{indent}│  AnchorMax: {rt.anchorMax}");
            Debug.Log($"{indent}│  AnchoredPosition: {rt.anchoredPosition}");
            Debug.Log($"{indent}│  SizeDelta: {rt.sizeDelta}");
            Debug.Log($"{indent}│  Pivot: {rt.pivot}");

            // Calculate actual screen position (if possible)
            if (rt.anchorMin != rt.anchorMax)
            {
                Debug.Log($"{indent}│  OffsetMin (Left/Bottom): {rt.offsetMin}");
                Debug.Log($"{indent}│  OffsetMax (Right/Top): {rt.offsetMax}");
            }
        }

        // Components
        Component[] components = go.GetComponents<Component>();
        Debug.Log($"{indent}│  === Components ({components.Length}) ===");
        foreach (Component comp in components)
        {
            if (comp == null) continue;
            if (comp is RectTransform) continue; // Already shown

            string compInfo = $"{indent}│    - {comp.GetType().Name}";

            // Special handling for specific component types
            if (comp is Image img)
            {
                compInfo += $" [Color: {img.color}, Sprite: {(img.sprite ? img.sprite.name : "None")}]";
            }
            else if (comp is TMPro.TextMeshProUGUI tmpText)
            {
                compInfo += $" [Text: \"{tmpText.text.Substring(0, Mathf.Min(20, tmpText.text.Length))}...\", Size: {tmpText.fontSize}]";
            }
            else if (comp is Text legacyText)
            {
                compInfo += $" [Text: \"{legacyText.text.Substring(0, Mathf.Min(20, legacyText.text.Length))}...\", Size: {legacyText.fontSize}]";
            }
            else if (comp is Button btn)
            {
                compInfo += $" [Interactable: {btn.interactable}]";
            }
            else if (comp is Animator anim)
            {
                compInfo += $" [Controller: {(anim.runtimeAnimatorController ? anim.runtimeAnimatorController.name : "None")}]";
            }

            Debug.Log(compInfo);
        }

        Debug.Log($"{indent}└─");
    }
}
#endif