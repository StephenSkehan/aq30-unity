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

    // Per-episode data (FtueChoreographyConfig on the EpisodeCatalog entry; null
    // entry = the Listener's shipped constants). Resolved in Start once the
    // caseflow has begun its episode.
    AQ.App.FTUE.FtueChoreographyConfig _cfg;
    string LeadId       => _cfg.leadId;
    string SeedFamily   => _cfg.seedFamily;
    int    SeedTier     => _cfg.seedTier;
    string SeedItemId   => _cfg.seedItemId;
    string TargetItemId => _cfg.targetItemId;
    string PayoffStart  => string.IsNullOrEmpty(_cfg.payoffStartNodeId) ? null : _cfg.payoffStartNodeId;
    bool   TapMode      => _cfg.GuidesGeneratorTap;

    const float SparkleBeatSeconds = 0.9f; // merge burst is ~0.35s; leave a beat after
    const float TapBeatSeconds     = 0.45f; // drop fly-in, then the story
    const float DimFactor = 0.72f;

    MergeBoardController  _board;
    LeadsRepository       _repo;
    CaseFlowLeadBridgeMB  _bridge;
    DialogueRunner        _runner;
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

        // Hold while the boot overlay (logo card + FTUE promo film) still covers
        // the game — the intro dialogue must not boot underneath it.
        while (AQ.App.UI.StudioSplashMB.Showing) yield return null;

        // Episode data: the catalog entry's config, else the Listener's constants.
        _cfg = AQ.App.Episodes.EpisodeRuntime.Current?.ftue
               ?? AQ.App.FTUE.FtueChoreographyConfig.ListenerDefaults();
        _runner = FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);

        // The repository binds the episode's database during the save restore;
        // give the guided card a short grace period before deciding it is gone.
        for (int i = 0; i < 180 && (_lead = FindLead()) == null; i++) yield return null;
        if (_lead == null || _lead.RuntimeState == LeadState.Blocked)
        {
            // First card already resolved (or content changed) — nothing to choreograph, ever.
            if (_lead == null) Stage = 2;
            Destroy(gameObject);
            yield break;
        }

        // The proceed hint would burn its one-time flag on a card the player never
        // tapped; suppress it until the payoff closes. The generator-tap arrow is
        // suppressed only in merge mode (it would fight the guided merge); in tap
        // mode the arrow IS the guidance.
        SuppressHint("ProceedHint");
        if (!TapMode) SuppressHint("GeneratorTapHint");

        if (Stage == 0)
        {
            SeedIfNeeded();

            var intro = _cfg.introGraph != null ? _cfg.introGraph : _lead.resolutionDialogue;
            if (intro != null)
            {
                // Stage stays 0 until the intro CLOSES (OnIntroClosed): stamping 1
                // here meant any kill during the ~57s N1–N3 intro permanently
                // skipped the case setup — the resume path jumps straight to the
                // guide and the payoff later references "the message you just
                // heard" that the player never heard. Seeding is idempotent, so
                // replaying this block on relaunch is safe.
                DialogueRunner.DialogueClosed += OnIntroClosed;
                _bridge.PlayIntroForFtue(intro, _cfg.introStartNodeId, _cfg.introEndAfterNodeId);
                AQ.App.Analytics.GameAnalytics.LogFtueEvent("l1_intro_start");
                Debug.Log($"[FTUEChoreo] Intro booted ({intro.name}) for '{LeadId}' ({(TapMode ? "tap" : "merge")} mode).");
                yield break; // guide starts when the intro closes
            }
            Debug.LogWarning("[FTUEChoreo] First card has no intro graph — skipping intro.");
            Stage = 1; // no intro to protect — mark the seed pass done
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
        Stage = 1; // intro fully shown — safe to skip it on any future relaunch
        // Package episodes: the intro WAS the first package's beat; its completion
        // must pay without re-presenting. Rule 5: flagged only now, after display.
        if (!string.IsNullOrEmpty(_cfg.prePlayedPackageId))
            GameFlags.Set("pkg." + _cfg.prePlayedPackageId + ".beat_preplayed");
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("l1_intro_done");
        StartGuide();
    }

    // ---------------- seeding ----------------

    void SeedIfNeeded()
    {
        if (_cfg.seedCount <= 0) return; // tap mode: the generator makes the item
        if (_lead.RuntimeState == LeadState.Ready ||
            (!string.IsNullOrEmpty(TargetItemId) && CountBoardItems(TargetItemId) > 0))
            return; // goal already met somehow — don't add clutter

        int have = CountBoardItems(SeedItemId);
        for (int i = have; i < _cfg.seedCount; i++)
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
        if (TapMode)
        {
            MergeBoardController.GeneratorTapped += OnGuidedGeneratorTap;
            GeneratorTapHintMB.EnsureInstalled(); // the gold arrow is the "do this"
        }
        ApplyGuideVisuals();
    }

    void StopGuideSubscriptions()
    {
        MergeBoardController.BoardCompositionChanged -= OnBoardChanged;
        LeadsRuntimeBus.OnLeadStateChanged           -= OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              -= OnLeadActivated;
        MergeBoardController.GeneratorTapped         -= OnGuidedGeneratorTap;
        GhostDragDemoMB.Hide();
        _guiding = false;
    }

    // Tap mode: the ruled first tap is deterministic (structure v2.2: "one A-T1").
    // The drop table is weighted, not fixed, so if the guided tap did not yield
    // the seed item, place one as if the generator had made it.
    void OnGuidedGeneratorTap()
    {
        if (!_guiding || _payoffStarted || !TapMode) return;
        StartCoroutine(EnsureGuidedDropNextFrame());
    }

    IEnumerator EnsureGuidedDropNextFrame()
    {
        yield return null;
        if (!_guiding || _payoffStarted) yield break;
        var lead = FindLead();
        if (lead == null || lead.RuntimeState == LeadState.Ready) yield break;
        if (CountBoardItems(SeedItemId) > 0) yield break;
        bool placed = _board.PlaceFromOverflow(new OverflowTileData
        {
            kind   = OverflowKind.Item,
            family = SeedFamily,
            tier   = SeedTier
        });
        Debug.Log(placed
            ? "[FTUEChoreo] Guided tap missed the seed item; placed one deterministically."
            : "[FTUEChoreo] Guided tap missed and the board refused a placement; the player taps again.");
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
        StartCoroutine(FinishWhenPayoffDialogueDone());
    }

    // The payoff normally ends when its dialogue closes. Package episodes may
    // open no dialogue at all (the beat was the intro), so also finish when no
    // runner is talking a frame after the proceed.
    IEnumerator FinishWhenPayoffDialogueDone()
    {
        DialogueRunner.DialogueClosed += OnPayoffClosed;
        yield return null;
        if (this == null) yield break;
        if (_runner == null || !_runner.gameObject.activeInHierarchy)
            OnPayoffClosed();
    }

    void ApplyGuideVisuals()
    {
        ClearGuideVisuals();

        var targets = new List<BoardTileView>();
        bool goalOnBoard = false;

        if (TapMode)
        {
            // Guide the generator tap: pulse every generator, dim the rest. No
            // ghost drag (there is nothing to merge yet). Once the seed item is
            // on the board the checker makes the card Ready and the payoff runs.
            for (int r = 0; r < _board.Rows; r++)
                for (int c = 0; c < _board.Cols; c++)
                {
                    var v = _board.Get(r, c);
                    if (v == null || v.IsEmpty) continue;
                    if (v.Kind == TileKind.Generator) { _pulseTargets.Add(v); continue; }
                    Dim(FindImage(v, "Bg"));
                    Dim(v.itemImage);
                }
            if (_pulseTargets.Count == 0) CedeToNormalFlow();
            return;
        }

        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v == null || v.Kind != TileKind.Item) continue;
                var id = _board.GetItemId(v);
                if (id == SeedItemId) targets.Add(v);
                else if (!string.IsNullOrEmpty(TargetItemId) && id == TargetItemId) goalOnBoard = true;
            }

        if (targets.Count == 0)
        {
            // Merged goal on board (or Ready imminent): stay quiet and wait for the
            // checker. Neither present: the pair is gone (lockered) — cede to the
            // normal card-tap flow rather than pulse at nothing.
            GhostDragDemoMB.Hide();
            if (!goalOnBoard && _lead.RuntimeState != LeadState.Ready)
                CedeToNormalFlow();
            return;
        }

        // A pulse says "these matter"; only a demonstration says "do this"
        // (Stephen-ruled 2026-08-21): loop a translucent copy of one seed item
        // sliding onto its twin until the player performs the merge.
        if (targets.Count >= 2)
            GhostDragDemoMB.Show(targets[0], targets[1]);
        else
            GhostDragDemoMB.Hide();

        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v == null) continue;

                if (targets.Contains(v))
                {
                    // No blue Highlight ring (Stephen-ruled 2026-08-22): the
                    // scale pulse + ghost-drag demonstration carry the teaching;
                    // the ring read as noise.
                    _pulseTargets.Add(v);
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

        // Gentle come-hither: scale pulse on the pair (+50% amplitude,
        // Stephen-ruled 2026-08-22).
        float phase = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / 0.9f) + 1f) * 0.5f;
        float scale = 1f + 0.105f * phase;
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
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("l1_first_merge");
        StopGuideSubscriptions();
        ClearGuideVisuals();
        StartCoroutine(PayoffRoutine());
    }

    IEnumerator PayoffRoutine()
    {
        // Let the merge sparkle (or the drop fly-in) land before the story takes over.
        yield return new WaitForSecondsRealtime(TapMode ? TapBeatSeconds : SparkleBeatSeconds);

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
        Debug.Log($"[FTUEChoreo] Guided action landed — auto-proceeded '{LeadId}'.");

        // Package episodes may open no dialogue here (the beat was the intro):
        // finish now rather than wait for a close that never comes.
        yield return null;
        if (this != null && (_runner == null || !_runner.gameObject.activeInHierarchy))
            OnPayoffClosed();
    }

    void OnPayoffClosed()
    {
        DialogueRunner.DialogueClosed -= OnPayoffClosed;
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("l1_payoff_done");
        GeneratorTapHintMB.EnsureInstalled();
        ProceedHintMB.EnsureInstalled();
        GuidedCaseLoopMB.EnsureInstalled(); // I1: the full loop walks next (L2)
        if (this != null) Destroy(gameObject);
    }

    // ---------------- fallback ----------------

    void CedeToNormalFlow()
    {
        Debug.Log("[FTUEChoreo] Ceding to normal card-tap flow.");
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("l1_ceded");
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
