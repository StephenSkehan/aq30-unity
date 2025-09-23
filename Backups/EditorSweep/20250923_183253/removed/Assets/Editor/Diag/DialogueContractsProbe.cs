using UnityEditor;
using UnityEngine;

public static class DialogueContractsProbe
{
    [MenuItem("AQ/Diag/Probe Dialogue Contracts")]
    public static void Probe()
    {
        var prefab = Resources.Load<GameObject>("App/UI/Prefabs/DialoguePanel");
        Debug.Log(prefab ? $"[Probe] Resources prefab OK: {prefab.name}" 
                         : "[Probe] Resources.Load FAILED: App/UI/Prefabs/DialoguePanel");

        // If your project uses a contracts ScriptableObject, its type is likely UIContracts or similar.
        // Update the type + path below if needed.
        var contracts = Resources.Load<ScriptableObject>("App/UI/Config/UIContracts");
        Debug.Log(contracts ? $"[Probe] Contracts asset present: {contracts.name}"
                            : "[Probe] No contracts asset at App/UI/Config/UIContracts");

        if (contracts != null)
        {
            // Try to reflect a field named "DialoguePanel" on the contracts object.
            var field = contracts.GetType().GetField("DialoguePanel");
            if (field != null)
            {
                var value = field.GetValue(contracts) as GameObject;
                Debug.Log(value ? $"[Probe] Contracts.DialoguePanel -> {value.name}"
                                : "[Probe] Contracts.DialoguePanel is NULL");
            }
            else
            {
                Debug.Log("[Probe] Contracts has no field named 'DialoguePanel' (adjust type/path).");
            }
        }
    }
}
