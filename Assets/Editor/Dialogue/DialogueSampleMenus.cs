using UnityEditor; using UnityEngine;

public static class DialogueSampleMenus
{
    const string GraphPath = "Assets/Content/Ep01/CaseGraph_Sample.asset";
    const string PanelPath = "Assets/UI/Dialogue/DialoguePanel.prefab";

    [MenuItem("AQ/Dialogue/Create Sample CaseGraph")]
    public static void CreateGraph(){
        if(!AssetDatabase.IsValidFolder("Assets/Content")) AssetDatabase.CreateFolder("Assets","Content");
        if(!AssetDatabase.IsValidFolder("Assets/Content/Ep01")) AssetDatabase.CreateFolder("Assets/Content","Ep01");

        var g = ScriptableObject.CreateInstance<CaseGraph>();
        g.startId = "START";
        g.nodes = new CaseGraph.Node[]{
            new CaseGraph.Node{ id="START", speaker="Lena", line="I think I'm being followed.", nextId="ASK_CHOICE" },
            new CaseGraph.Node{ id="ASK_CHOICE", speaker="You", line="What do you want to do?",
                choices = new CaseGraph.Choice[]{
                    new CaseGraph.Choice{ text="Ask for details", nextId="DETAILS" },
                    new CaseGraph.Choice{ text="Tell her to stay put", nextId="STAY" }
                }},
            new CaseGraph.Node{ id="DETAILS", speaker="Lena", line="Tall, dark coat, keeps the same pace.", nextId="END" },
            new CaseGraph.Node{ id="STAY",    speaker="Lena", line="Okay. I won't move.", nextId="END" },
            new CaseGraph.Node{ id="END",     speaker="System", line="End of demo." }
        };
        AssetDatabase.CreateAsset(g, GraphPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = g;
        Debug.Log("[Dialogue] Sample CaseGraph created at "+GraphPath);
    }

    [MenuItem("AQ/Dialogue/Drop Dialogue Demo")]
    public static void DropDemo(){
        // Panel
        var panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath);
        if(panelPrefab == null){ Debug.LogWarning("[Dialogue] Missing DialoguePanel prefab. Run AQ/Prefabs/Make DialoguePanel."); return; }
        var panelInstance = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab);

        // Runner
        var runnerGO = new GameObject("DialogueDriver", typeof(DialogueRunner));
        var runner = runnerGO.GetComponent<DialogueRunner>();
        var graph = AssetDatabase.LoadAssetAtPath<CaseGraph>(GraphPath);
        if(graph == null){ Debug.LogWarning("[Dialogue] Sample graph missing. Run AQ/Dialogue/Create Sample CaseGraph."); return; }
        runner.Graph = graph;

        // Hook controller (find it in the instantiated canvas)
        var ctl = panelInstance.GetComponentInChildren<DialogueController>(true);
        runner.Panel = ctl;

        // Keep things tidy in hierarchy
        panelInstance.transform.SetAsLastSibling();
        Selection.activeGameObject = runnerGO;

        Debug.Log("[Dialogue] Dropped DialoguePanel + Runner and wired sample graph.");
    }
}
