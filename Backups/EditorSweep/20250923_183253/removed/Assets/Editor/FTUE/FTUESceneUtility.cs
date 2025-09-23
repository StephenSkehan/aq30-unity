using UnityEditor; using UnityEngine;

public static class FTUESceneUtility
{
    [MenuItem("AQ/FTUE/Drop Arrow & Attach")]
    public static void Drop(){
        var pres = Object.FindFirstObjectByType<BoardPresenter>();
        if(pres == null || pres.GridRoot == null || pres.GridRoot.childCount == 0){
            Debug.LogWarning("[FTUE] No board or items found to attach to."); return;
        }
        var path = "Assets/UI/FTUE/FTUE_Arrow.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(prefab == null){ Debug.LogWarning("[FTUE] Prefab missing, run AQ/Prefabs/Make FTUE Arrow."); return; }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var ftue = go.GetComponent<FTUEHintController>();
        var first = pres.GridRoot.GetChild(0) as RectTransform;
        ftue.Target = first;
        go.transform.SetParent(pres.GridRoot, false);
        go.transform.SetAsLastSibling();
        Debug.Log("[FTUE] Dropped arrow and attached to "+first.name);
        Selection.activeGameObject = go;
    }
}
