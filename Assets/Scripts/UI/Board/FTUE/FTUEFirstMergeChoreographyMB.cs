using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App;
using AQ.App.CaseFlow;
using AQ.App.Leads;
using AQ.App.Overflow;
using AQ.App.UI.Board;

/// <summary>
/// FTUE first-merge choreography (spec locked 2026-07-20):
///   ① two pre-seeded audio T1 items on the board when the goal opens (L1, ~45s
///     of intro dialogue; deterministic, zero taps to payoff)
///   ② soft guide — pulse/highlight the pair plus a slight dim on every other
///     cell; input stays completely free (no hard lock, no tutorial wall)
///   ③ on merge — sparkle (BoardFxPlayer's merge burst), a short beat, then
///     nodes 4–5 of Resolve_E1_Tip auto-play with no card tap.
///
/// Self-installs like DialogueStageMB; the scene file is never mutated and the
/// dialogue asset stays whole (DialogueRunner node-range boot plays N1–N3 up
/// front and resumes at N4). Any state it does not recognise falls back to the
/// normal card-tap flow. Lives in Assembly-CSharp to reach both AQ.App and the
/// board controller.
/// </summary>
public sealed class FTUEFirstMergeChoreographyMB : MonoBehaviour
{
    // 0 = untouched, 1 = pair seeded + intro span played, 2 = done (or ceded to normal flow)
    public const string StageKey = "aq.ftue.first_merge.stage";

    const string LeadId       = "e1_tip";
    const string SeedFamily   = "audio_investigation";
    const int    SeedTier     = 0; // A-T1
    const string SeedItemId   = "audio_investigation_t1";
    const string TargetItemId = "audio_investigation_t2"; // the merged goal item
    const string IntroStart   = "E1_L1_N1";
    const string IntroEnd     = "E1_L1_N3";
    const string PayoffStart  = "E1_L1_N4";

    const float SparkleBeatSeconds = 0.9f; // merge burst is ~0.35s; leave a beat after
    const float DimFactor = 0.72f;

    MergeBoardController  _board;
    LeadsRepository       _repo;
    CaseFlowLeadBridgeMB  _bridge;
    LeadData              _lead;

    bool _guiding;
    bool _payoffStarted;
    readonly List<BoardTileView> _pulseTargets = new List<BoardTileView>();
    readonly List<Image>         _pulseHighlights = new List<Image>();
    readonly Dictionary<Image, Color> _dimmed = new Dictionary<Image, Color>();

    static int Stage
    {
        get => PlayerPrefs.GetInt(StageKey, 0);
        set { PlayerPrefs.SetInt(StageKey, value); PlayerPrefs.Save(); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        Install();
        SceneManager.sceneLoaded += (_, __) => Install();
    }

    static void Install()
    {
        if (Stage >= 2) return;
        if (GameObject.Find("FTUEFirstMergeChoreography") != null) return;
        var go = new GameObject("FTUEFirstMergeChoreography");
        go.AddComponent<FTUEFirstMergeChoreographyMB>();
    }

    IEnumerator Start()
    {
        // Wait for the board scene to finish booting and restoring its save.
        // Non-board scenes never produce a controller — give up quietly.
        for (int i = 0; i < 600; i++)
        {
            if (_board == null)  _board  = FindAnyObjectByType<MergeBoardController>();
            if (_repo == null)   _repo   = FindAnyObjectByType<LeadsRepository>();
            if (_bridge == null) _bridge = FindAnyObjectByType<CaseFlowLeadBridgeMB>();
            if (_board != null && _board.GridReady && _repo != null && _bridge != null &&
                BoardSaveSystem.WalletRestored)
                break;
            yield return null;
        }
        if (_board == null || !_board.GridReady || _repo == null || _bridge == null ||
            !BoardSaveSystem.WalletRestored)
        {
            Destroy(gameObject);
            yield break;
        }

        // One settle frame so LeadsRepository/LeadRequirementChecker have broadcast.
        yield return null;

        _lead = FindLead();
        if (_lead == null || _lead.RuntimeState == LeadState.Blocked)
        {
            // L1 already resolved (or content changed) — nothing to choreograph, ever.
            if (_lead == null) Stage = 2;
            Destroy(gameObject);
            yield break;
        }

        // The generator-tap and proceed hints would fight the guided merge (and
        // the auto-proceed would burn ProceedHint's one-time flag on a card the
        // player never tapped). Suppress both; restored when the payoff closes.
        SuppressHint("GeneratorTapHint");
        SuppressHint("ProceedHint");

        if (Stage == 0)
        {
            SeedPairIfNeeded();
            Stage = 1;

            if (_lead.resolutionDialogue != null)
            {
                DialogueRunner.DialogueClosed += OnIntroClosed;
                _bridge.PlayIntroForFtue(_lead.resolutionDialogue, IntroStart, IntroEnd);
                Debug.Log("[FTUEChoreo] Pair seeded, intro span N1–N3 booted.");
                yield break; // guide starts when the intro closes
            }
            Debug.LogWarning("[FTUEChoreo] L1 has no resolution dialogue — skipping intro.");
        }

        StartGuide();
    }

    void OnDestroy()
    {
        DialogueRunner.DialogueClosed -= OnIntroClosed;
        DialogueRunner.DialogueClosed -= OnPayoffClosed;
        StopGuideSubscriptions();
        ClearGuideVisuals();
    }

    // ---------------- intro ----------------

    void OnIntroClosed()
    {
        DialogueRunner.DialogueClosed -= OnIntroClosed;
        if (this == null) return;
        StartGuide();
    }

    // ---------------- seeding ----------------

    void SeedPairIfNeeded()
    {
        if (_lead.RuntimeState == LeadState.Ready || CountBoardItems(TargetItemId) > 0)
            return; // goal already met somehow — don't add clutter

        int have = CountBoardItems(SeedItemId);
        for (int i = have; i < 2; i++)
        {
            bool placed = _board.PlaceFromOverflow(new OverflowTileData
            {
                kind   = OverflowKind.Item,
                family = SeedFamily,
                tier   = SeedTier
            });
            if (!placed)
            {
                Debug.LogWarning("[FTUEChoreo] Board refused seed placement — ceding to normal flow.");
                return;
            }
        }
    }

    // ---------------- soft guide ----------------

    void StartGuide()
    {
        if (_payoffStarted) return;

        if (_lead.RuntimeState == LeadState.Ready)
        {
            BeginPayoff();
            return;
        }

        _guiding = true;
        MergeBoardController.BoardCompositionChanged += OnBoardChanged;
        LeadsRuntimeBus.OnLeadStateChanged           += OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              += OnLeadActivated;
        ApplyGuideVisuals();
    }

    void StopGuideSubscriptions()
    {
        MergeBoardController.BoardCompositionChanged -= OnBoardChanged;
        LeadsRuntimeBus.OnLeadStateChanged           -= OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              -= OnLeadActivated;
        _guiding = false;
    }

    void OnBoardChanged()
    {
        if (!_guiding || _payoffStarted) return;
        ApplyGuideVisuals();
    }

    void OnLeadStateChanged(LeadData lead)
    {
        if (lead == null || lead.leadId != LeadId) return;
        if (lead.RuntimeState == LeadState.Ready) BeginPayoff();
    }

    void OnLeadActivated(LeadData lead)
    {
        // Player proceeded L1 themselves (full 5-node dialogue plays — fine).
        if (lead == null || lead.leadId != LeadId || _payoffStarted) return;
        _payoffStarted = true;
        Stage = 2;
        StopGuideSubscriptions();
        ClearGuideVisuals();
        DialogueRunner.DialogueClosed += OnPayoffClosed;
    }

    void ApplyGuideVisuals()
    {
        ClearGuideVisuals();

        var targets = new List<BoardTileView>();
        bool goalOnBoard = false;
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v == null || v.Kind != TileKind.Item) continue;
                var id = _board.GetItemId(v);
                if (id == SeedItemId) targets.Add(v);
                else if (id == TargetItemId) goalOnBoard = true;
            }

        if (targets.Count == 0)
        {
            // Merged goal on board (or Ready imminent): stay quiet and wait for the
            // checker. Neither present: the pair is gone (lockered) — cede to the
            // normal card-tap flow rather than pulse at nothing.
            if (!goalOnBoard && _lead.RuntimeState != LeadState.Ready)
                CedeToNormalFlow();
            return;
        }

        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v == null) continue;

                if (targets.Contains(v))
                {
                    _pulseTargets.Add(v);
                    var hl = FindImage(v, "Highlight");
                    if (hl != null)
                    {
                        if (!_dimmed.ContainsKey(hl)) _dimmed[hl] = hl.color;
                        hl.enabled = true;
                        _pulseHighlights.Add(hl);
                    }
                    continue;
                }

                Dim(FindImage(v, "Bg"));
                Dim(v.itemImage);
            }
    }

    void ClearGuideVisuals()
    {
        foreach (var v in _pulseTargets)
            if (v != null && v.itemImage != null)
                v.itemImage.transform.localScale = Vector3.one;
        _pulseTargets.Clear();

        foreach (var hl in _pulseHighlights)
            if (hl != null) hl.enabled = false;
        _pulseHighlights.Clear();

        foreach (var kv in _dimmed)
            if (kv.Key != null) kv.Key.color = kv.Value;
        _dimmed.Clear();
    }

    void Dim(Image img)
    {
        if (img == null || _dimmed.ContainsKey(img)) return;
        var original = img.color;
        _dimmed[img] = original;
        img.color = new Color(original.r * DimFactor, original.g * DimFactor,
                              original.b * DimFactor, original.a);
    }

    static Image FindImage(BoardTileView v, string childName)
    {
        var t = v.transform.Find(childName);
        return t != null ? t.GetComponent<Image>() : null;
    }

    void Update()
    {
        if (!_guiding || _pulseTargets.Count == 0) return;

        // Gentle come-hither: scale pulse on the pair, alpha pulse on their rings.
        float phase = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / 0.9f) + 1f) * 0.5f;
        float scale = 1f + 0.07f * phase;
        foreach (var v in _pulseTargets)
            if (v != null && v.itemImage != null && v.itemImage.enabled)
                v.itemImage.transform.localScale = Vector3.one * scale;

        foreach (var hl in _pulseHighlights)
            if (hl != null && _dimmed.TryGetValue(hl, out var baseCol))
            {
                var c = baseCol;
                c.a = Mathf.Lerp(0.25f, 0.75f, phase);
                hl.color = c;
            }
    }

    // ---------------- payoff ----------------

    void BeginPayoff()
    {
        if (_payoffStarted) return;
        _payoffStarted = true;
        StopGuideSubscriptions();
        ClearGuideVisuals();
        StartCoroutine(PayoffRoutine());
    }

    IEnumerator PayoffRoutine()
    {
        // Let the merge sparkle land before the story takes over.
        yield return new WaitForSecondsRealtime(SparkleBeatSeconds);

        // Re-validate: the beat is long enough for a player card-tap to race us.
        var lead = FindLead();
        if (lead == null || lead.RuntimeState != LeadState.Ready)
        {
            CedeToNormalFlow();
            yield break;
        }

        Stage = 2;
        DialogueRunner.DialogueClosed += OnPayoffClosed;
        _bridge.ProceedForFtue(lead, PayoffStart);
        Debug.Log("[FTUEChoreo] First merge landed — auto-proceeded L1, payoff N4–N5 booted.");
    }

    void OnPayoffClosed()
    {
        DialogueRunner.DialogueClosed -= OnPayoffClosed;
        GeneratorTapHintMB.EnsureInstalled();
        ProceedHintMB.EnsureInstalled();
        if (this != null) Destroy(gameObject);
    }

    // ---------------- fallback ----------------

    void CedeToNormalFlow()
    {
        Debug.Log("[FTUEChoreo] Ceding to normal card-tap flow.");
        Stage = 2;
        StopGuideSubscriptions();
        ClearGuideVisuals();
        GeneratorTapHintMB.EnsureInstalled();
        ProceedHintMB.EnsureInstalled();
        Destroy(gameObject);
    }

    // ---------------- helpers ----------------

    LeadData FindLead()
    {
        if (_repo == null) return null;
        var leads = _repo.CurrentLeads;
        for (int i = 0; i < leads.Count; i++)
            if (leads[i] != null && leads[i].leadId == LeadId) return leads[i];
        return null;
    }

    int CountBoardItems(string itemId)
    {
        int n = 0;
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v != null && v.Kind == TileKind.Item && _board.GetItemId(v) == itemId) n++;
            }
        return n;
    }

    static void SuppressHint(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Destroy(go);
    }
}
