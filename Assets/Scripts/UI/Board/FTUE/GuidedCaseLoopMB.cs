using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App;
using AQ.App.Leads;
using AQ.App.UI.Board;

/// <summary>
/// Guided Case Loop (SAS/feature-ftue-onboarding-v1.md, I1 — Stephen-ruled
/// 2026-08-21). The L1 choreography teaches ONE pre-seeded merge; this walks
/// the first FULL loop on the next lead: tap the generator (energy visibly
/// spent) → items drop → merge a pair (ghost demo shows how) → the lead goes
/// Ready → PROCEED. Soft guidance in the house style: input stays free, every
/// step advances on the player's own action, doing things out of order simply
/// skips steps. Directive banner copy is ≤8 words (all lines Stephen-ruled
/// 2026-08-21). One-shot: stage flag survives relaunch mid-loop.
/// </summary>
public sealed class GuidedCaseLoopMB : MonoBehaviour
{
    // 0 = pending, 1 = done or ceded
    public const string StageKey = "aq.ftue.guided_loop.stage";

    enum Step { Boot, Generator, Merge, Quiet, Proceed, Done }
    Step _step = Step.Boot;

    MergeBoardController _board;
    LeadsRepository      _repo;

    RectTransform _bannerRoot;
    TextMeshProUGUI _bannerLabel;
    CanvasGroup _bannerCg;
    bool _dialogueOpen;

    readonly List<BoardTileView> _pulseTiles = new List<BoardTileView>();

    static int Stage
    {
        get => PlayerPrefs.GetInt(StageKey, 0);
        set { PlayerPrefs.SetInt(StageKey, value); PlayerPrefs.Save(); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstalled();
        SceneManager.sceneLoaded += (_, __) => EnsureInstalled();
    }

    /// <summary>Idempotent install; also called by the L1 choreography when its
    /// payoff closes so the loop begins the moment L1's story beat ends.</summary>
    public static void EnsureInstalled()
    {
        if (Stage >= 1) return;
        // Only relevant once L1's choreography is fully done.
        if (PlayerPrefs.GetInt(FTUEFirstMergeChoreographyMB.StageKey, 0) < 2) return;
        if (GameObject.Find("GuidedCaseLoop") != null) return;
        var go = new GameObject("GuidedCaseLoop");
        go.AddComponent<GuidedCaseLoopMB>();
    }

    IEnumerator Start()
    {
        DialogueRunner.DialogueOpened += OnDialogueOpened;
        DialogueRunner.DialogueClosed += OnDialogueClosed;

        for (int i = 0; i < 600; i++)
        {
            if (_board == null) _board = FindAnyObjectByType<MergeBoardController>();
            if (_repo == null)  _repo  = FindAnyObjectByType<LeadsRepository>();
            if (_board != null && _board.GridReady && _repo != null && BoardSaveSystem.WalletRestored)
                break;
            yield return null;
        }
        if (_board == null || !_board.GridReady || _repo == null)
        {
            Destroy(gameObject);
            yield break;
        }
        while (AQ.App.UI.StudioSplashMB.Showing) yield return null;
        yield return null;

        // The player has already completed a full loop themselves (two or more
        // leads activated, counting L1) — nothing left to teach.
        int activated = 0;
        foreach (var _ in _repo.ActivatedLeadIds) activated++;
        if (activated >= 2)
        {
            Stage = 1;
            Destroy(gameObject);
            yield break;
        }

        // The choreography suppressed this; the guided loop replaces its lesson.
        var genHint = GameObject.Find("GeneratorTapHint");
        if (genHint != null) Destroy(genHint);

        MergeBoardController.BoardCompositionChanged += OnBoardChanged;
        MergeBoardController.TilesMerged             += OnTilesMerged;
        LeadsRuntimeBus.OnLeadStateChanged           += OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              += OnLeadActivated;

        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_start");
        BuildBanner();
        EnterGeneratorStep();
    }

    void OnDestroy()
    {
        // Unsubscribing handlers that were never subscribed is a safe no-op.
        DialogueRunner.DialogueOpened -= OnDialogueOpened;
        DialogueRunner.DialogueClosed -= OnDialogueClosed;
        MergeBoardController.BoardCompositionChanged -= OnBoardChanged;
        MergeBoardController.TilesMerged             -= OnTilesMerged;
        LeadsRuntimeBus.OnLeadStateChanged           -= OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              -= OnLeadActivated;
        GhostDragDemoMB.Hide();
        ClearPulse();
    }

    void OnDialogueOpened(CaseGraph _) { _dialogueOpen = true;  RefreshBannerVisibility(); }
    void OnDialogueClosed()            { _dialogueOpen = false; RefreshBannerVisibility(); }

    // ---------------- steps ----------------

    void EnterGeneratorStep()
    {
        _step = Step.Generator;
        // Copy Stephen-ruled 2026-08-21.
        SetBanner("Tap the kit. Every item helps.");
        PulseGenerators();
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_gen_shown");
    }

    void EnterMergeStep(BoardTileView a, BoardTileView b)
    {
        _step = Step.Merge;
        SetBanner("Drag one onto its twin.");
        ClearPulse();
        _pulseTiles.Add(a);
        _pulseTiles.Add(b);
        GhostDragDemoMB.Show(a, b);
    }

    void EnterQuiet()
    {
        // Loop demonstrated: stop talking, let them play it out. The tick hint
        // and ProceedHint carry the last mile.
        _step = Step.Quiet;
        SetBanner(null);
        ClearPulse();
        GhostDragDemoMB.Hide();
    }

    void EnterProceedStep()
    {
        _step = Step.Proceed;
        // Copy Stephen-ruled 2026-08-21 (cards proceed on tap; there is no
        // separate PROCEED button on the card anymore).
        SetBanner("Tap on green lead card to proceed.");
        ClearPulse();
        GhostDragDemoMB.Hide();
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_lead_ready");
    }

    void Finish(string funnelStep)
    {
        _step = Step.Done;
        Stage = 1;
        AQ.App.Analytics.GameAnalytics.LogFtueEvent(funnelStep);
        GeneratorTapHintMB.EnsureInstalled(); // its own flag decides if it still applies
        Destroy(gameObject);
    }

    // ---------------- events ----------------

    void OnBoardChanged()
    {
        if (_step != Step.Generator) return;
        if (TryFindMergePair(out var a, out var b))
            EnterMergeStep(a, b);
    }

    void OnTilesMerged(string family, int tier)
    {
        if (_step == Step.Merge || _step == Step.Generator)
        {
            AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_first_free_merge");
            EnterQuiet();
        }
    }

    void OnLeadStateChanged(LeadData lead)
    {
        if (_step == Step.Done) return;
        if (lead != null && lead.RuntimeState == LeadState.Ready)
            EnterProceedStep();
    }

    void OnLeadActivated(LeadData _)
    {
        if (_step == Step.Done) return;
        Finish("gl_done");
    }

    // ---------------- board helpers ----------------

    bool TryFindMergePair(out BoardTileView a, out BoardTileView b)
    {
        a = b = null;
        var firstByKey = new Dictionary<(string fam, int tier), BoardTileView>();
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v == null || v.IsEmpty || v.Kind != TileKind.Item) continue;
                if (!_board.IsMergeCandidate(v)) continue;
                var key = (_board.GetFamily(v), v.Tier);
                if (firstByKey.TryGetValue(key, out var first))
                {
                    a = first; b = v;
                    return true;
                }
                firstByKey[key] = v;
            }
        return false;
    }

    void PulseGenerators()
    {
        ClearPulse();
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v != null && !v.IsEmpty && v.Kind == TileKind.Generator)
                    _pulseTiles.Add(v);
            }
    }

    void ClearPulse()
    {
        foreach (var v in _pulseTiles)
            if (v != null && v.itemImage != null)
                v.itemImage.transform.localScale = Vector3.one;
        _pulseTiles.Clear();
    }

    void Update()
    {
        if (_pulseTiles.Count == 0) return;
        float phase = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / 0.9f) + 1f) * 0.5f;
        float scale = 1f + 0.08f * phase;
        foreach (var v in _pulseTiles)
            if (v != null && v.itemImage != null && v.itemImage.enabled)
                v.itemImage.transform.localScale = Vector3.one * scale;
    }

    // ---------------- banner ----------------

    void BuildBanner()
    {
        var canvasGo = new GameObject("__GuidedLoopBanner", typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 3900; // just under the hint chips (4000)
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(canvasGo.transform, false);
        _bannerRoot = (RectTransform)panel.transform;
        _bannerRoot.sizeDelta = new Vector2(760f, 96f);
        _bannerRoot.anchorMin = _bannerRoot.anchorMax = new Vector2(0.5f, 0.5f);
        // Same fixed slot as the hint chips: below the HUD, never over board cells.
        _bannerRoot.anchoredPosition = new Vector2(0f, 570f);
        var img = panel.GetComponent<Image>();
        img.sprite = AQ.App.UI.AQTheme.Rounded;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.13f, 0.11f, 0.07f, 0.96f); // warm dark: directive, not flavor
        img.raycastTarget = false;
        _bannerCg = panel.GetComponent<CanvasGroup>();
        _bannerCg.blocksRaycasts = false;
        _bannerCg.interactable = false;

        var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(_bannerRoot, false);
        var art = (RectTransform)accent.transform;
        art.anchorMin = new Vector2(0f, 0f);
        art.anchorMax = new Vector2(0f, 1f);
        art.offsetMin = new Vector2(0f, 8f);
        art.offsetMax = new Vector2(8f, -8f);
        var aimg = accent.GetComponent<Image>();
        aimg.color = new Color(0.96f, 0.72f, 0.25f, 1f); // case-file amber
        aimg.raycastTarget = false;

        // Gerald leads the tutorial (Stephen concept 2026-08-21): the wise man
        // fronts the directive banner too, one consistent teacher everywhere.
        float textLeft = 28f;
        var mentor = AQ.App.UI.Dossiers.DossierPortraits.Find("gerald");
        if (mentor != null)
        {
            var pgo = new GameObject("Mentor", typeof(RectTransform), typeof(Image));
            pgo.transform.SetParent(_bannerRoot, false);
            var mrt = (RectTransform)pgo.transform;
            mrt.anchorMin = mrt.anchorMax = new Vector2(0f, 0.5f);
            mrt.pivot = new Vector2(0f, 0.5f);
            mrt.sizeDelta = new Vector2(78f, 78f);
            mrt.anchoredPosition = new Vector2(14f, 0f);
            var pimg = pgo.GetComponent<Image>();
            pimg.sprite = mentor;
            pimg.preserveAspect = true;
            pimg.raycastTarget = false;
            textLeft = 104f;
        }

        var txt = new GameObject("Text", typeof(RectTransform));
        txt.transform.SetParent(_bannerRoot, false);
        var trt = (RectTransform)txt.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(textLeft, 10f);
        trt.offsetMax = new Vector2(-20f, -10f);
        _bannerLabel = txt.AddComponent<TextMeshProUGUI>();
        _bannerLabel.fontSize = 34f;
        _bannerLabel.alignment = TextAlignmentOptions.MidlineLeft;
        _bannerLabel.color = new Color(0.94f, 0.92f, 0.86f, 1f);
        _bannerLabel.raycastTarget = false;
        AQ.App.UI.AQTheme.StyleText(_bannerLabel);

        SetBanner(null);
    }

    void SetBanner(string text)
    {
        if (_bannerLabel == null) return;
        _bannerLabel.text = text ?? string.Empty;
        RefreshBannerVisibility();
    }

    void RefreshBannerVisibility()
    {
        if (_bannerCg == null) return;
        bool visible = !_dialogueOpen && !string.IsNullOrEmpty(_bannerLabel != null ? _bannerLabel.text : null);
        _bannerCg.alpha = visible ? 1f : 0f;
    }
}
