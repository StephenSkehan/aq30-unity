using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    public CaseGraph Graph;
    public DialogueController Panel;

    string _currentId;
    DialogueTyper _bodyTyper;
    DialogueTyper _speakerTyper;
    bool _booted = false;

    void Start(){
        if (Graph != null) InternalBoot(Graph);
        // If Graph is null, we wait for BootWithGraph() (Addressables case).
    }

    public void BootWithGraph(CaseGraph g){
        Graph = g;
        if (!_booted) InternalBoot(g);
        else JumpTo(Graph.startId);
    }

    void InternalBoot(CaseGraph g){
        if (g == null || Panel == null){
            Debug.LogWarning("[Dialogue] Missing Graph or Panel."); return;
        }
        // Attach typers (or reuse)
        if (Panel.Body){ _bodyTyper = Panel.Body.GetComponent<DialogueTyper>() ?? Panel.Body.gameObject.AddComponent<DialogueTyper>(); }
        if (Panel.Speaker){ _speakerTyper = Panel.Speaker.GetComponent<DialogueTyper>() ?? Panel.Speaker.gameObject.AddComponent<DialogueTyper>(); }
        if (_bodyTyper != null) { _bodyTyper.charsPerSecond = 45f; }
        if (_speakerTyper != null){ _speakerTyper.charsPerSecond = 60f; }

        Panel.AdvanceClicked += OnAdvance;
        Panel.ChoiceClicked  += OnChoice;

        _currentId = string.IsNullOrEmpty(g.startId) ? (g.nodes!=null && g.nodes.Length>0 ? g.nodes[0].id : null) : g.startId;
        _booted = true;
        ShowNode(_currentId);
    }

    void OnDestroy(){
        if (Panel != null){
            Panel.AdvanceClicked -= OnAdvance;
            Panel.ChoiceClicked  -= OnChoice;
        }
    }

    public string GetCurrentNodeId(){ return _currentId; }
    public void JumpTo(string id){ if(!string.IsNullOrEmpty(id)) ShowNode(id); }

    void OnAdvance(){
        if (_bodyTyper != null && _bodyTyper.IsTyping){ _bodyTyper.Skip(); return; }
        if (_speakerTyper != null && _speakerTyper.IsTyping){ _speakerTyper.Skip(); return; }

        var n = Graph.Get(_currentId);
        if (n == null) return;
        if (n.choices != null && n.choices.Length > 0) return;
        if (!string.IsNullOrEmpty(n.nextId)) ShowNode(n.nextId);
        else End();
    }

    void OnChoice(int idx){
        if (_bodyTyper != null && _bodyTyper.IsTyping){ _bodyTyper.Skip(); return; }
        var n = Graph.Get(_currentId);
        if (n == null || n.choices == null || idx < 0 || idx >= n.choices.Length) return;
        var next = n.choices[idx].nextId;
        if (!string.IsNullOrEmpty(next)) ShowNode(next); else End();
    }

    void ShowNode(string id){
        _currentId = id;
        var n = Graph.Get(id);
        if (n == null){ End(); return; }
        if (_speakerTyper != null) _speakerTyper.SetInstant(n.speaker);
        else if (Panel.Speaker) Panel.Speaker.text = n.speaker;
        if (_bodyTyper != null) _bodyTyper.StartTyping(n.line);
        else if (Panel.Body) Panel.Body.text = n.line;
        Panel.BindNode(n);
        Debug.Log("[Dialogue] Node: " + id);
    }

    void End(){
        if (_bodyTyper != null) _bodyTyper.StopTyping();
        if (_speakerTyper != null) _speakerTyper.StopTyping();
        Debug.Log("[Dialogue] End of graph.");
        if (Panel) Panel.gameObject.SetActive(false);
    }
}
