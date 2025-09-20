// Assets/Editor/Diag/DialoguePrefabSanity.cs
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class DialoguePrefabSanity
{
    const string AuthoringPath = "Assets/UI/Dialogue/DialoguePanel.prefab";
    const string ContractPath  = "Assets/Resources/App/UI/Prefabs/DialoguePanel.prefab";

    [MenuItem("AQ/Diag/Check DialoguePanel Contract Prefab")]
    public static void Check()
    {
        var authoring = AssetDatabase.LoadAssetAtPath<GameObject>(AuthoringPath);
        if (!authoring) { Debug.LogError($"[Diag] Missing authoring prefab at {AuthoringPath}"); return; }

        ReportMissingScripts("[Diag] Authoring", authoring);

        var contract = AssetDatabase.LoadAssetAtPath<GameObject>(ContractPath);
        if (!contract)
        {
            Debug.LogWarning($"[Diag] Contract prefab missing at {ContractPath}. Run AQ  Prefabs  Ensure Contract Prefabs.");
            return;
        }

        ReportMissingScripts("[Diag] Contract", contract);
        Debug.Log($"[Diag] Contract prefab present at {ContractPath}");
    }

    static void ReportMissingScripts(string label, GameObject root)
    {
        var count = 0;
        foreach (var c in root.GetComponentsInChildren<Component>(true))
        {
            if (c == null) count++;
        }
        if (count > 0) Debug.LogWarning($"{label}: {count} Missing (Mono Script) components found on {root.name} (and children).");
        else Debug.Log($"{label}: no missing scripts.");
    }
}

