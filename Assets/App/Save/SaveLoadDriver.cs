using UnityEngine;
using AQ.App;
using AQ.App.Leads;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SaveLoadDriver : MonoBehaviour
{
    public ThemeController Theme;
    public DialogueRunner Dialogue;
    public BoardPresenter Board;
    public ThemeRegistry ThemeRegistry; // under Resources
    public LeadsRepository Leads;

    void Awake(){
        if(ThemeRegistry == null) ThemeRegistry = Resources.Load<ThemeRegistry>("Theme/ThemeRegistry");
    }

    void Update(){
        // Hotkeys are editor convenience only. Support both input backends.
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if(kb != null){
            if(kb.sKey.wasPressedThisFrame) Save();
            if(kb.lKey.wasPressedThisFrame) Load();
            if(kb.cKey.wasPressedThisFrame) Clear();
        }
#else
        if (Input.GetKeyDown(KeyCode.S)) Save();
        if (Input.GetKeyDown(KeyCode.L)) Load();
        if (Input.GetKeyDown(KeyCode.C)) Clear();
#endif
    }

    public void Save(){
        var blob = new SaveBlob();
        if(Theme && Theme.ActiveTheme) blob.ActiveThemeName = Theme.ActiveTheme.name;
        if(Dialogue) blob.DialogueNodeId = Dialogue.GetCurrentNodeId();
        if(Board) blob.Board = Board.CaptureBoard();
        if(Leads != null)
        {
            foreach (var lead in Leads.CurrentLeads)
            {
                if (lead == null) continue;
                byte mask = 0;
                var reqs = lead.requirements;
                for (int i = 0; i < reqs.Length && i < 8; i++)
                    if (reqs[i].IsSatisfied) mask |= (byte)(1 << i);
                blob.Leads.Add(new SaveBlob.LeadSaveEntry {
                    LeadId = lead.leadId,
                    RequirementMask = mask,
                    State = lead.state.ToString()
                });
            }
        }
        JsonSaveService.Save(blob);
    }

    public void Load(){
        var blob = JsonSaveService.Load();
        if(blob == null){ Debug.LogWarning("[Save] No save to load."); return; }

        if(Theme && ThemeRegistry){
            var t = ThemeRegistry.FindByName(blob.ActiveThemeName);
            if(t){ Theme.ActiveTheme = t; Theme.ApplyTheme(); }
        }
        if(Dialogue && !string.IsNullOrEmpty(blob.DialogueNodeId)){
            Dialogue.JumpTo(blob.DialogueNodeId);
        }
        if(Board != null && blob.Board != null){
            Board.RestoreBoard(blob.Board);
        }
        if(Leads != null && blob.Leads != null)
        {
            foreach (var entry in blob.Leads)
            {
                LeadData lead = null;
                foreach (var l in Leads.CurrentLeads)
                    if (l != null && l.leadId == entry.LeadId) { lead = l; break; }
                if (lead == null) continue;
                var reqs = lead.requirements;
                for (int i = 0; i < reqs.Length && i < 8; i++)
                    lead.SetRequirementSatisfied(i, (entry.RequirementMask & (1 << i)) != 0);
                if (System.Enum.TryParse<LeadState>(entry.State, out var state))
                    lead.state = state;
            }
            Leads.NotifyChanged();
        }
    }

    public void Clear(){
        JsonSaveService.Clear();
    }
}
