// Assets/Editor/AQ/Leads/LeadsBarAudit.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class LeadsBarAudit
    {
        [MenuItem("AQ/Leads/Audit LeadsBar Wiring")]
        public static void Audit()
        {
            var bar = Object.FindFirstObjectByType<AQ.App.Leads.LeadsBarView>(FindObjectsInactive.Include);
            if (!bar) { Debug.LogWarning("🔎 No LeadsBarView found in the open scene."); return; }

            var so = new SerializedObject(bar);
            var sr = so.FindProperty("scrollRect")?.objectReferenceValue as ScrollRect;
            var cr = so.FindProperty("contentRoot")?.objectReferenceValue as RectTransform;
            var pf = so.FindProperty("cardPrefab")?.objectReferenceValue;

            Debug.Log("🔎 LeadsBar wiring:");
            Debug.Log($"  • ScrollRect : {(sr ? "OK" : "MISSING")} {(sr ? $"({sr.name})" : "")}", bar);
            Debug.Log($"  • Content    : {(cr ? "OK" : "MISSING")} {(cr ? $"({cr.name})" : "")}", bar);
            Debug.Log($"  • Card Prefab: {(pf ? "OK" : "MISSING")}", bar);

            if (!sr) Debug.Log("👉 In the LeadsBar GameObject, assign: ScrollRect → your ScrollLeads component");
            if (!cr) Debug.Log("👉 In the LeadsBar GameObject, assign: Content Root → Content_Leads");
            if (!pf) Debug.Log("👉 In the LeadsBar GameObject, assign: Card Prefab → your card prefab");
        }
    }
}
#endif
