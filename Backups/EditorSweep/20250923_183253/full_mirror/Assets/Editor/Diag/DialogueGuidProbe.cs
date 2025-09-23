using UnityEngine;
using UnityEditor;

public static class DialogueGuidProbe
{
    [MenuItem("AQ/Diag/Probe Dialogue GUIDs (ReadOnly)")]
    public static void Probe()
    {
        Debug.Log("== AQ Dialogue GUID Probe ==");

        // 1) Expected Resources paths
        string[] expected = {
            "App/UI/Narrative/Prefabs/DialoguePanel",
            "App/UI/Prefabs/DialoguePanel"
        };

        foreach (var path in expected)
        {
            var prefab = Resources.Load<GameObject>(path);
            Debug.Log($"[Check] Resources.Load(\"{path}\") => {(prefab ? "FOUND" : "null")}");
        }

        // 2) UIContracts.asset reference
        var contracts = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
            "Assets/App/UI/Config/UIContracts.asset"
        );
        if (contracts == null)
        {
            Debug.LogError("[Check] UIContracts.asset not found.");
            return;
        }

        // Reflection to pull DialoguePanel field
        var field = contracts.GetType().GetField("DialoguePanel");
        var dialogueRef = field?.GetValue(contracts) as GameObject;
        if (dialogueRef == null)
        {
            Debug.LogError("[Check] UIContracts.DialoguePanel is null in asset.");
            return;
        }

        string contractPath = AssetDatabase.GetAssetPath(dialogueRef);
        string contractGuid = AssetDatabase.AssetPathToGUID(contractPath);

        Debug.Log($"[Check] UIContracts.DialoguePanel => {contractPath} (GUID {contractGuid})");

        // 3) Compare with prefab GUIDs in Resources
        foreach (var path in expected)
        {
            string assetPath = $"Assets/Resources/{path}.prefab";
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            Debug.Log($"    Expected path {assetPath} => GUID {guid}");
        }

        Debug.Log("== End Probe ==");
    }
}
