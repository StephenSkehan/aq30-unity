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

    static GuidedCaseLoopMB _active;

    /// <summary>True only while the loop is actively directing the board (its
    /// banner up, its own pulses running). The required-source pulse yields in
    /// these steps but NOT during Quiet — suppressing it for the loop's whole
    /// lifetime muted all guidance mid-case (Stephen playtest 2026-08-22).</summary>
    public static bool OwnsBoard =>
        _active != null && (_active._step == Step.Generator || _active._step == Step.Merge);

    MergeBoardController _board;
    LeadsRepository      _repo;

    RectTransform _bannerRoot;
    TextMeshProUGUI _bannerLabel;
    CanvasGroup _bannerCg;
    bool _dialogueOpen;

    readonly List<BoardTileView> _pulseTiles = new List<BoardTileView>();
    // Non-tile pulse subjects (the green lead card on the proceed step).
    readonly List<Transform> _pulseTransforms = new List<Transform>();
    // The banner sits just above/below its step's subject (Stephen-ruled
    // 2026-08-22, supersedes the fixed hint-slot placement for this banner).
    Transform _bannerTarget;
    float _nextBannerPlaceAt;

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
        _active = this;
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
        MergeBoardController.GeneratorTapped         += OnGeneratorTapped;
        LeadsRuntimeBus.OnLeadStateChanged           += OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              += OnLeadActivated;

        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_start");
        SeedStarterSingles();
        BuildBanner();
        EnterGeneratorStep();
    }

    /// <summary>
    /// I10-light (SAS FTUE, Stephen-ruled 2026-08-21): seed a few SINGLE tier-1
    /// items from the generator's own drop families so the first free minutes
    /// always contain visible near-merges. Singles, never pairs — a pre-made
    /// pair would let the merge step fire before the player ever taps the
    /// generator, skipping the lesson; a single means their own taps complete
    /// the pair. Skipped when the board already has items (returning saves).
    /// </summary>
    void SeedStarterSingles()
    {
        int existingItems = 0;
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v != null && !v.IsEmpty && v.Kind == TileKind.Item) existingItems++;
            }
        if (existingItems >= 3) return;

        var seeded = new HashSet<string>();
        for (int r = 0; r < _board.Rows && seeded.Count < 3; r++)
            for (int c = 0; c < _board.Cols && seeded.Count < 3; c++)
            {
                var v = _board.Get(r, c);
                if (v == null || v.IsEmpty || v.Kind != TileKind.Generator) continue;

                var so = _board.FindGeneratorType(_board.GetFamily(v));
                var cfg = so != null ? so.ConfigForTier(v.Tier) : null;
                if (cfg?.dropTable == null) continue;

                foreach (var e in cfg.dropTable)
                {
                    if (seeded.Count >= 3) break;
                    if (e.type != AQ.App.Generators.DropType.Item || e.itemTier != 0) continue;
                    if (!AQ.App.Generators.DropRoller.IsEligible(e, subGenLocked: true)) continue;
                    if (string.IsNullOrEmpty(e.itemFamily) || !seeded.Add(e.itemFamily)) continue;

                    // Seeds must not just APPEAR (Stephen-ruled 2026-08-22): note
                    // which tiles hold this family/tier already, place, then fly
                    // the newcomer in from the generator that fictionally made it.
                    var before = TilesOf(e.itemFamily, 0);
                    bool placed = _board.PlaceFromOverflow(new AQ.App.Overflow.OverflowTileData
                    {
                        kind   = AQ.App.Overflow.OverflowKind.Item,
                        family = e.itemFamily,
                        tier   = 0
                    }); // full board just refuses — fine
                    if (!placed) continue;

                    foreach (var t in TilesOf(e.itemFamily, 0))
                        if (!before.Contains(t))
                        {
                            StartCoroutine(FlyInSeed(t, v));
                            break;
                        }
                }
            }

        if (seeded.Count > 0)
            AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_seeded");
    }

    readonly List<BoardTileView> _inFlight = new List<BoardTileView>();

    HashSet<BoardTileView> TilesOf(string family, int tier)
    {
        var set = new HashSet<BoardTileView>();
        for (int r = 0; r < _board.Rows; r++)
            for (int c = 0; c < _board.Cols; c++)
            {
                var v = _board.Get(r, c);
                if (v != null && !v.IsEmpty && v.Kind == TileKind.Item &&
                    v.Tier == tier && _board.GetFamily(v) == family)
                    set.Add(v);
            }
        return set;
    }

    /// <summary>Slides a freshly seeded item from its generator to its cell —
    /// spawned things come FROM somewhere (Stephen-ruled 2026-08-22).</summary>
    IEnumerator FlyInSeed(BoardTileView newTile, BoardTileView fromGen)
    {
        if (newTile == null || newTile.itemImage == null) yield break;
        _inFlight.Add(newTile);
        var sprite = newTile.itemImage.sprite;
        newTile.itemImage.enabled = false;

        // Overlay canvas parented to this MB so an early destroy takes the
        // flight with it (OnDestroy restores the hidden icons).
        var go = new GameObject("__SeedFlight", typeof(Canvas));
        go.transform.SetParent(transform, false);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4600;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(go.transform, false);
        var rt = (RectTransform)iconGo.transform;
        var img = iconGo.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        var corners = new Vector3[4];
        ((RectTransform)newTile.itemImage.transform).GetWorldCorners(corners);
        rt.sizeDelta = new Vector2(corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        Vector3 end = (corners[0] + corners[2]) * 0.5f;
        Vector3 start = fromGen != null && fromGen.itemImage != null
            ? fromGen.itemImage.transform.position : end;

        for (float t = 0f; t < 0.32f; t += Time.unscaledDeltaTime)
        {
            if (rt == null) break;
            rt.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / 0.32f));
            yield return null;
        }

        if (go != null) Destroy(go);
        if (newTile != null && newTile.itemImage != null) newTile.itemImage.enabled = true;
        _inFlight.Remove(newTile);
    }

    void OnDestroy()
    {
        if (_active == this) _active = null;
        // Unsubscribing handlers that were never subscribed is a safe no-op.
        DialogueRunner.DialogueOpened -= OnDialogueOpened;
        DialogueRunner.DialogueClosed -= OnDialogueClosed;
        MergeBoardController.BoardCompositionChanged -= OnBoardChanged;
        MergeBoardController.TilesMerged             -= OnTilesMerged;
        MergeBoardController.GeneratorTapped         -= OnGeneratorTapped;
        LeadsRuntimeBus.OnLeadStateChanged           -= OnLeadStateChanged;
        LeadsRuntimeBus.OnLeadActivated              -= OnLeadActivated;
        GhostDragDemoMB.Hide();
        ClearPulse();
        // Any seed still mid-flight dies with us — un-hide its real icon.
        foreach (var t in _inFlight)
            if (t != null && t.itemImage != null) t.itemImage.enabled = true;
        _inFlight.Clear();
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
        _bannerTarget = _pulseTiles.Count > 0 ? _pulseTiles[0].transform : null;
        _nextBannerPlaceAt = 0f;
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_gen_shown");
    }

    void EnterMergeStep(BoardTileView a, BoardTileView b)
    {
        _step = Step.Merge;
        // Gerald copy sheet, Stephen-ruled 2026-08-21. NOTE the standing rule
        // from that ruling: tutorial/UI copy addresses the PLAYER, so no
        // endearments ("love" stays in-story, Gerald to Ally only).
        SetBanner("A matching pair. Drag them together.");
        ClearPulse();
        _pulseTiles.Add(a);
        _pulseTiles.Add(b);
        _bannerTarget = a != null ? a.transform : null;
        _nextBannerPlaceAt = 0f;
        GhostDragDemoMB.Show(a, b);
    }

    void EnterQuiet()
    {
        // Loop demonstrated: stop talking, let them play it out. The tick hint
        // and ProceedHint carry the last mile.
        _step = Step.Quiet;
        SetBanner(null);
        _bannerTarget = null;
        ClearPulse();
        GhostDragDemoMB.Hide();
    }

    void EnterProceedStep()
    {
        _step = Step.Proceed;
        // Gerald copy sheet, Stephen-ruled 2026-08-21.
        SetBanner("Lead's gone green. Tap it and proceed.");
        ClearPulse();
        GhostDragDemoMB.Hide();

        // Pulse the green (Ready) lead card itself, banner just beneath it
        // (Stephen-ruled 2026-08-22). Update() keeps rescanning: the bar often
        // REBUILDS its card views right after the Ready broadcast, so the first
        // scan can run before the ready card exists (or grab a doomed view).
        TryAttachReadyCard();

        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_lead_ready");
    }

    float _nextCardScanAt;

    void TryAttachReadyCard()
    {
        var cards = FindObjectsByType<LeadCardView>(FindObjectsSortMode.None);
        foreach (var card in cards)
            if (card != null && card.IsReadyNow)
            {
                _pulseTransforms.Add(card.transform);
                _bannerTarget = card.transform;
                _nextBannerPlaceAt = 0f;
                return;
            }
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

    void OnGeneratorTapped()
    {
        // The tap-the-kit lesson completes on the tap itself (Stephen-ruled
        // 2026-08-22): banner and pulse retire immediately; the step stays live
        // so the first pair that forms advances to the merge lesson.
        if (_step != Step.Generator) return;
        // I8: gl_gen_shown fires when we point at the kit; this fires when they
        // actually tap it. The drop between the two is the confusion signal --
        // "shown" alone cannot tell us whether the pointer worked.
        AQ.App.Analytics.GameAnalytics.LogFtueEvent("gl_gen_tapped");
        SetBanner(null);
        _bannerTarget = null;
        ClearPulse();
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

        foreach (var t in _pulseTransforms)
            if (t != null) t.localScale = Vector3.one;
        _pulseTransforms.Clear();
    }

    void Update()
    {
        // Card views rebuild often — drop dead transforms, and in the proceed
        // step keep hunting for the live ready card until one sticks.
        _pulseTransforms.RemoveAll(t => t == null);
        if (_step == Step.Proceed && _pulseTransforms.Count == 0 &&
            Time.unscaledTime >= _nextCardScanAt)
        {
            _nextCardScanAt = Time.unscaledTime + 0.5f;
            TryAttachReadyCard();
        }

        // Keep the banner parked by its subject (cheap, twice a second).
        if (_bannerTarget != null && Time.unscaledTime >= _nextBannerPlaceAt)
        {
            _nextBannerPlaceAt = Time.unscaledTime + 0.5f;
            PositionBannerNear(_bannerTarget);
        }

        if (_pulseTiles.Count == 0 && _pulseTransforms.Count == 0) return;
        float phase = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / 0.9f) + 1f) * 0.5f;
        float scale = 1f + 0.12f * phase; // +50% (Stephen-ruled 2026-08-22)
        foreach (var v in _pulseTiles)
            if (v != null && v.itemImage != null && v.itemImage.enabled)
                v.itemImage.transform.localScale = Vector3.one * scale;
        foreach (var t in _pulseTransforms)
            if (t != null) t.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Parks the banner just BELOW its subject (or above when the subject sits
    /// in the lower half of the screen), never covering it (Stephen-ruled
    /// 2026-08-22; supersedes the fixed hint-slot placement for this banner).
    /// </summary>
    void PositionBannerNear(Transform target)
    {
        if (_bannerRoot == null || target == null) return;
        var canvasRect = _bannerRoot.parent as RectTransform;
        if (canvasRect == null) return;

        Vector2 screen = target.position; // overlay canvases: world == screen px
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out var local))
            return;

        float clearance = _bannerRoot.sizeDelta.y * 0.5f + 95f; // closer, still clear of the subject (2026-08-22)
        bool below = screen.y > Screen.height * 0.5f;
        local.y += below ? -clearance : clearance;

        // Stay on screen horizontally.
        float halfCanvas = canvasRect.rect.width * 0.5f;
        float halfBanner = _bannerRoot.sizeDelta.x * 0.5f;
        local.x = Mathf.Clamp(local.x, -(halfCanvas - halfBanner - 8f), halfCanvas - halfBanner - 8f);

        _bannerRoot.anchoredPosition = local;
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
        // 1.5x size + fully opaque (Stephen-ruled 2026-08-22): the directive
        // banner is THE teaching surface, it should not whisper.
        _bannerRoot.sizeDelta = new Vector2(1040f, 144f);
        _bannerRoot.anchorMin = _bannerRoot.anchorMax = new Vector2(0.5f, 0.5f);
        // Fallback slot until a step parks it by its subject (PositionBannerNear).
        _bannerRoot.anchoredPosition = new Vector2(0f, 570f);
        // Hairline border + opaque body (Stephen-ruled 2026-08-22): the outer
        // image is the border line, the inset child carries the fill.
        var img = panel.GetComponent<Image>();
        img.sprite = AQ.App.UI.AQTheme.Rounded;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.62f, 0.50f, 0.30f, 0.9f); // muted amber line
        img.raycastTarget = false;

        var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
        body.transform.SetParent(_bannerRoot, false);
        var brt2 = (RectTransform)body.transform;
        brt2.anchorMin = Vector2.zero;
        brt2.anchorMax = Vector2.one;
        brt2.offsetMin = new Vector2(3f, 3f);
        brt2.offsetMax = new Vector2(-3f, -3f);
        var bodyImg = body.GetComponent<Image>();
        bodyImg.sprite = AQ.App.UI.AQTheme.Rounded;
        bodyImg.type = Image.Type.Sliced;
        bodyImg.color = new Color(0.13f, 0.11f, 0.07f, 1f); // warm dark: directive, not flavor
        bodyImg.raycastTarget = false;
        _bannerCg = panel.GetComponent<CanvasGroup>();
        _bannerCg.blocksRaycasts = false;
        _bannerCg.interactable = false;

        var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(_bannerRoot, false);
        var art = (RectTransform)accent.transform;
        art.anchorMin = new Vector2(0f, 0f);
        art.anchorMax = new Vector2(0f, 1f);
        art.offsetMin = new Vector2(0f, 12f);
        art.offsetMax = new Vector2(12f, -12f);
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
            mrt.sizeDelta = new Vector2(118f, 118f);
            mrt.anchoredPosition = new Vector2(18f, 0f);
            var pimg = pgo.GetComponent<Image>();
            pimg.sprite = mentor;
            pimg.preserveAspect = true;
            pimg.raycastTarget = false;
            textLeft = 152f;
        }

        var txt = new GameObject("Text", typeof(RectTransform));
        txt.transform.SetParent(_bannerRoot, false);
        var trt = (RectTransform)txt.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(textLeft, 10f);
        trt.offsetMax = new Vector2(-20f, -10f);
        _bannerLabel = txt.AddComponent<TextMeshProUGUI>();
        _bannerLabel.fontSize = 48f; // 1.5x (Stephen-ruled 2026-08-22)
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
