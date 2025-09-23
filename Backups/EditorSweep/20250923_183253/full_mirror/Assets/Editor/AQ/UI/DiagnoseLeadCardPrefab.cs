#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AQ.App.Leads;

namespace AQ.EditorTools.UI
{
    public static class DiagnoseLeadCardPrefab
    {
        [MenuItem("AQ/UI/Diag → Print LeadsBarView card prefab wiring")]
        public static void Print()
        {
            var lb = Object.FindFirstObjectByType<LeadsBarView>();
            if (!lb) { Debug.LogError("[Diag] No LeadsBarView in the open scene."); return; }

            var so = new SerializedObject(lb);
            var cardPrefabProp = so.FindProperty("cardPrefab");
            var prefabComp = cardPrefabProp?.objectReferenceValue as LeadCardView;

            if (!prefabComp)
            {
                Debug.LogError("[Diag] LeadsBarView.cardPrefab is NULL or not a LeadCardView.");
                return;
            }

            var prefabPath = AssetDatabase.GetAssetPath(prefabComp.gameObject);
            var lpvSO = new SerializedObject(prefabComp);
            var reqPrefabProp = lpvSO.FindProperty("requirementItemPrefab");
            var reqRootProp = lpvSO.FindProperty("requirementsRoot");

            Debug.Log($"[Diag] LeadsBarView.cardPrefab -> {prefabPath}");
            Debug.Log($"[Diag] requirementItemPrefab = {(reqPrefabProp?.objectReferenceValue ? reqPrefabProp.objectReferenceValue.name : "NULL")}");
            Debug.Log($"[Diag] requirementsRoot     = {(reqRootProp?.objectReferenceValue ? reqRootProp.objectReferenceValue.name : "NULL")}");
            Selection.activeObject = prefabComp.gameObject; // ping it
        }
    }
}
#endif
