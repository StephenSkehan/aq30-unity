#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class AuditDuplicateLeadUI
{
    [MenuItem("AQ/Dev/Audit: Duplicate Lead UI Types")]
    public static void Run()
    {
        var names = new[] { "LeadCardPresenter", "TierSetPopup", "RequirementSlotView" };
        foreach (var n in names)
        {
            var guids = AssetDatabase.FindAssets($"{n} t:Script");
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Debug.Log($"[Audit] {n}: {paths.Length} file(s)\n" + string.Join("\n", paths));
        }
    }
}
#endif
