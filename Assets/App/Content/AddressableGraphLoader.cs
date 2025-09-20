using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableGraphLoader : MonoBehaviour
{
    [Tooltip("Label to load CaseGraph assets by (default: Ep01).")]
    public string Label = "Ep01"; //
    public DialogueRunner Runner;

    void Start(){
        if(Runner == null) Runner = FindFirstObjectByType<DialogueRunner>();
        LoadGraphs();
    }

    public void LoadGraphs(){
        Addressables.InitializeAsync().Completed += _ => {
            Addressables.LoadAssetsAsync<CaseGraph>(Label, OnGraphLoaded).Completed += OnAllComplete;
        };
    }

    List<CaseGraph> _loaded = new List<CaseGraph>();
    void OnGraphLoaded(CaseGraph g){ if(g!=null) _loaded.Add(g); }

    void OnAllComplete(AsyncOperationHandle<IList<CaseGraph>> op){
        if(op.Status != AsyncOperationStatus.Succeeded || _loaded.Count == 0){
            Debug.LogWarning("[Addr] No CaseGraph assets found for label '"+Label+"'.");
            return;
        }
        // Pick the first for now; later we can choose by episode id
        var graph = _loaded[0];
        Debug.Log("[Addr] Loaded CaseGraph '"+graph.name+"' via label '"+Label+"'.");
        if(Runner == null) Runner = FindFirstObjectByType<DialogueRunner>();
        if(Runner != null) Runner.BootWithGraph(graph);
    }

    // Menu or button can call this at runtime to clear cache between tests
    public void ClearAddressablesCache(){ Addressables.ClearResourceLocators(); Caching.ClearCache(); Debug.Log("[Addr] Cleared Addressables cache."); }
}

