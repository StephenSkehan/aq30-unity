using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using AQ.App.CaseFlow;
using AQ.App.Config;
using AQ.App.Economy;
using AQ.App.Episodes;
using AQ.App.Events;
using AQ.App.Generators;
using AQ.App.Leads;
using AQ.App.Locker;
using AQ.App.Overflow;
using AQ.App.Persistence;
using AQ.App.Presentation;
using AQ.App.Services;
using AQ.SharedKernel.CaseFlow;
using AQ.SharedKernel.Economy;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Saves and loads the board state (items/generators), global energy, wallet,
    /// case flow, leads, locker, Stash and Case Kit specials in one JSON file.
    /// Atomic write with rolling .prev.json backup. Schema 1.0.0: per-episode
    /// state (board cells, caseflow step, lead states, completion) lives in
    /// episode-keyed sections; value-bearing globals stay top-level so a crash
    /// can never separate a transaction's halves (robustness rule 1). DTOs and
    /// the 0.9.0 migration live in AQ.App.Persistence.SaveModel (testable there;
    /// Assembly-CSharp cannot be referenced by test assemblies).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoardSaveSystem : MonoBehaviour
    {
        [Tooltip("If not assigned, will search in scene.")]
        public MergeBoardController board;

        private LeadsRepository _leadsRepo;

        [Header("Save Settings")]
        [Tooltip("Debounce time for saves after a detected change.")]
        public float saveDebounceSeconds = 0.25f;

        [Tooltip("Optional filename override. When empty uses 'board_state.json'.")]
        public string fileName = "board_state.json";

        private string _pathLive;
        private string _pathTmp;
        private string _pathPrev;

        private float _nextSaveAt = -1f;
        private int _lastSnapshotHash;

        // Episode partition (schema 1.0.0). Sections for episodes other than the
        // running one are carried verbatim through every save so switching
        // episodes never destroys dormant progress (§1.4 of the audit was the
        // destroy path). _switchTargetId points the NEXT save's currentEpisodeId
        // at a different episode during a switch (see SwitchToEpisode).
        private readonly List<EpisodeSectionDTO> _dormantSections = new List<EpisodeSectionDTO>();
        private bool _currentEpisodeComplete;
        private string _switchTargetId;
        private IDisposable _caseResolvedSub;

        /// <summary>
        /// True once this scene's save (or absence of one) has been applied to the
        /// wallet. Restore is destructive (set-to-saved), so anything of real-money
        /// value — IAP credits, restored purchases — must not be granted until this
        /// is true. Goes false again during a scene reload until the new Start() runs.
        /// </summary>
        public static bool WalletRestored { get; private set; }

        /// <summary>Fired right after WalletRestored becomes true.</summary>
        public static event Action WalletRestoreCompleted;

        /// <summary>The scene's save system, for SaveNow(). Null between scenes.</summary>
        public static BoardSaveSystem Instance { get; private set; }

        /// <summary>
        /// Persist the aggregate right now (synchronous). For callers that are about
        /// to make a change irreversible on the OUTSIDE — e.g. confirming a StoreKit
        /// transaction — and must know the credit is on disk first. Returns TRUE only
        /// if the write actually landed; false when the scene's save system isn't up,
        /// restore hasn't applied, or the disk write failed — callers making external
        /// commitments MUST check it (a silent no-op here once finalized purchases
        /// whose credit existed only in memory).
        /// </summary>
        public static bool SaveNow()
        {
            var self = Instance;
            if (self == null) return false;
            bool ok = self.TrySave();
            if (ok)
            {
                self._lastSnapshotHash = self.SnapshotHash();
                self._nextSaveAt = -1f;
                self._saveThisFrame = false;
            }
            return ok;
        }

        /// <summary>
        /// Point the save at another episode and persist, ready for a scene
        /// reload to boot it. The outgoing episode's section is written in the
        /// same atomic file, so nothing is lost if the reload never happens.
        /// Returns false (and changes nothing durable) when the write fails —
        /// the caller must not reload on false.
        /// </summary>
        public static bool SwitchToEpisode(string episodeId)
        {
            var self = Instance;
            if (self == null || string.IsNullOrEmpty(episodeId)) return false;

            self._switchTargetId = episodeId;
            bool ok = SaveNow();
            if (!ok)
            {
                self._switchTargetId = null;
                return false;
            }
            EpisodeBootPointer.PendingEpisodeId = episodeId;
            return true;
        }

        // --------------- Unity ---------------

        private void Awake()
        {
            Instance = this;
            WalletRestored = false;

            if (!board)
                board = FindFirstObjectByType<MergeBoardController>();

            _leadsRepo = FindFirstObjectByType<LeadsRepository>();

            var root = Application.persistentDataPath;
            _pathLive = Path.Combine(root, fileName);
            _pathTmp  = _pathLive + ".tmp";
            _pathPrev = Path.Combine(root, Path.GetFileNameWithoutExtension(_pathLive) + ".prev.json");

            // Boot handoff: CaseFlowOrchestratorMB (AQ.App) begins its episode in
            // Start, BEFORE this component's Start restores the aggregate — and it
            // cannot reference this assembly. Park the save's episode pointer where
            // it can read it (all Awakes run before any Start). Story flags hydrate
            // in the same pass: Start-order tie-breaks are arbitrary, so any Start
            // that reads GameFlags must find them already imported. The usual
            // JsonUtility caveat gates the flags import on the schema version —
            // pre-1.0.0 saves take the null path, which probes the legacy
            // PlayerPrefs keys (see GameFlags.ImportState).
            var peeked = PeekSaveDTO();
            EpisodeBootPointer.PendingEpisodeId = PeekEpisodeId(peeked);
            GameFlags.ImportState(
                peeked != null && SaveSchema.AtLeast(peeked.schemaVersion, 1, 0) ? peeked.flags : null);

            // Episode-transition seam: AQ.App UI (resolution screen, selector)
            // cannot reference this assembly, so it calls through EpisodeFlow.
            EpisodeFlow.SwitchHandler    = SwitchToEpisode;
            EpisodeFlow.ProgressProvider = GetProgress;
        }

        private IWallet _observedWallet;

        private void Start()
        {
            TryLoad();
            _lastSnapshotHash = SnapshotHash();

            WalletRestored = true;
            WalletRestoreCompleted?.Invoke();

            // Subscribed after restore so the restore grants themselves don't save.
            _observedWallet = WalletLocator.Instance;
            if (_observedWallet != null)
                _observedWallet.Changed += OnWalletChanged;

            // Completion is detected by CaseResolutionService (AQ.App) and reaches
            // the aggregate over the bus. The complete bit lands in the same
            // debounced snapshot as the closing lead's activation and rewards —
            // one atomic write, never two stores (rule 1).
            _caseResolvedSub = GlobalBus.Bus.Subscribe<CaseResolvedEvent>(OnCaseResolved);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_observedWallet != null)
                _observedWallet.Changed -= OnWalletChanged;
            _caseResolvedSub?.Dispose();
            _caseResolvedSub = null;
            if (EpisodeFlow.SwitchHandler == (Func<string, bool>)SwitchToEpisode)
                EpisodeFlow.SwitchHandler = null;
            if (EpisodeFlow.ProgressProvider == (Func<string, EpisodeProgress>)GetProgress)
                EpisodeFlow.ProgressProvider = null;
        }

        private void OnCaseResolved(CaseResolvedEvent e)
        {
            _currentEpisodeComplete = true;
        }

        private bool _saveThisFrame;

        // Premium is real-money value: persist the same frame instead of waiting
        // out the debounce window, so a crash can't eat a purchase credit.
        // Same-FRAME, not synchronous-mid-mutation: a synchronous save here fired
        // between an ingot spend and its energy grant (ladder refill, Starter Pack
        // legs), persisting a charged-but-undelivered wallet. Deferring to
        // LateUpdate makes the whole transaction one consistent snapshot.
        private void OnWalletChanged(WalletChanged e)
        {
            if (e.Currency != Currency.Premium) return;
            _saveThisFrame = true;
        }

        private void LateUpdate()
        {
            if (_saveThisFrame)
            {
                _saveThisFrame = false;
                TrySave();
                _lastSnapshotHash = SnapshotHash();
                _nextSaveAt = -1f;
                return;
            }

            if (_nextSaveAt > 0f && Time.unscaledTime >= _nextSaveAt)
            {
                TrySave();
                _nextSaveAt = -1f;
                return;
            }

            int h = SnapshotHash();
            if (h != _lastSnapshotHash)
            {
                _lastSnapshotHash = h;
                _nextSaveAt = Time.unscaledTime + Mathf.Max(0.05f, saveDebounceSeconds);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) { TrySave(); LogSessionOneEndOnce(); }
        }

        private void OnApplicationQuit()
        {
            TrySave();
            LogSessionOneEndOnce();
        }

        /// <summary>
        /// I8 funnel terminator. Fires once ever, when the first session ends, so the
        /// funnel has a denominator: every earlier ftue_funnel step is read against the
        /// players who reached the end of session one. Backgrounding counts as an end on
        /// mobile, which is the behaviour we want -- a player who backgrounds and never
        /// returns has ended their first session.
        /// </summary>
        private static void LogSessionOneEndOnce()
        {
            const string key = "aq.ftue.session1_end.logged";
            if (PlayerPrefs.GetInt(key, 0) != 0) return;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            AQ.App.Analytics.GameAnalytics.LogFtueEvent("session1_end");
        }

        // --------------- Episode identity ---------------

        /// <summary>
        /// Started/complete for any episode, from the loaded aggregate: the
        /// running episode answers from live memory, dormant episodes from their
        /// carried sections. Feeds the selector's Complete ✓ / In progress /
        /// Locked states through EpisodeFlow.
        /// </summary>
        public static EpisodeProgress GetProgress(string episodeId)
        {
            var self = Instance;
            if (self == null || string.IsNullOrEmpty(episodeId)) return default;

            string canonical = Canonicalize(episodeId);
            if (canonical == RunningEpisodeId())
                return new EpisodeProgress { Started = true, Complete = self._currentEpisodeComplete };

            foreach (var s in self._dormantSections)
            {
                if (s == null) continue;
                if (Canonicalize(s.episodeId) == canonical)
                    return new EpisodeProgress { Started = true, Complete = s.complete };
            }
            return default;
        }

        /// <summary>
        /// The canonical id of the episode the caseflow is running. Falls back to
        /// the catalog's first episode (then "ep01") in caseflow-less dev scenes.
        /// </summary>
        private static string RunningEpisodeId()
        {
            var svc = CaseFlowLocator.Instance;
            var raw = svc != null ? svc.Current?.Episode.Value : null;
            var catalog = EpisodeRuntime.Catalog;
            if (!string.IsNullOrEmpty(raw))
                return catalog != null ? catalog.CanonicalId(raw) : raw;
            return catalog?.First?.episodeId ?? "ep01";
        }

        private static string Canonicalize(string id)
        {
            var catalog = EpisodeRuntime.Catalog;
            return catalog != null ? catalog.CanonicalId(id) : id;
        }

        /// <summary>
        /// Awake-time parse of the save (live file, then the .prev fallback) for
        /// the episode pointer and the flags — the same fallback order the full
        /// TryLoad uses, so both read the same source of truth.
        /// </summary>
        private SaveDTO PeekSaveDTO()
        {
            foreach (var path in new[] { _pathLive, _pathPrev })
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    var dto = JsonUtility.FromJson<SaveDTO>(File.ReadAllText(path, Encoding.UTF8));
                    if (dto != null) return dto;
                }
                catch { /* unreadable file: the full TryLoad path reports it */ }
            }
            return null;
        }

        private static string PeekEpisodeId(SaveDTO dto)
        {
            if (dto == null) return null;
            if (!string.IsNullOrEmpty(dto.currentEpisodeId)) return dto.currentEpisodeId;
            // Legacy 0.x saves carry the id inside caseFlow.
            if (dto.caseFlow != null && !string.IsNullOrEmpty(dto.caseFlow.episodeId)) return dto.caseFlow.episodeId;
            return null;
        }

        /// <summary>
        /// Bind the leads repository to the running episode's database (catalog-
        /// driven). The scene serializes ep01's database; any other episode swaps
        /// here before lead states are applied. No-op when no catalog resolves.
        /// </summary>
        private void EnsureEpisodeDatabase()
        {
            if (_leadsRepo == null) return;
            var entry = EpisodeRuntime.Current;
            if (entry != null && entry.database != null && _leadsRepo.database != entry.database)
                _leadsRepo.ReplaceFromDatabase(entry.database);
        }

        public static void ClearSave()
        {
            var root     = Application.persistentDataPath;
            var live     = Path.Combine(root, "board_state.json");
            var prev     = Path.Combine(root, "board_state.prev.json");
            var tmp      = live + ".tmp";
            foreach (var p in new[] { live, prev, tmp })
                if (File.Exists(p)) File.Delete(p);
            OverflowBucketService.Clear();
            GeneratorFamilyRegistry.Clear();
            AQ.App.Locker.EvidenceLockerService.Clear();
            UI.Specials.SpecialItemsService.Clear();
            GameFlags.ResetForNewSave();
            EpisodeBootPointer.PendingEpisodeId = null;

            // ClearSave means RESET: nothing may write the aggregate again until
            // the next boot's restore. Without this, the reset flow's own wallet
            // wipe (a Premium change) armed the deferred same-frame save, and
            // LateUpdate re-wrote the full mid-game aggregate AFTER the files
            // were deleted — resurrecting the save the player just reset.
            // TrySave already refuses while !WalletRestored; the next scene's
            // Start() re-arms it after a clean TryLoad.
            WalletRestored = false;
            var self = Instance;
            if (self != null)
            {
                self._saveThisFrame = false;
                self._nextSaveAt = -1f;
                self._dormantSections.Clear();
                self._currentEpisodeComplete = false;
                self._switchTargetId = null;
            }

            Debug.Log("[Save] BoardSaveSystem cleared");
        }

        /// <summary>True only when the aggregate actually reached disk.</summary>
        public bool TrySave()
        {
            if (board == null) return false;
            // After an editor domain reload mid-play the controller's grid is
            // gone; a save from that state persists a phantom board (this is
            // how generator duplicates accumulated into real save files).
            if (!board.GridReady) return false;
            // Never persist before this boot's restore has applied. Unity fires
            // OnApplicationPause(true) on the FIRST play frame when the editor is
            // unfocused — before Start()/TryLoad — and that save would clobber the
            // on-disk aggregate with boot-empty wallet/leads/locker state.
            if (!WalletRestored) return false;

            string runningId = RunningEpisodeId();

            var dto = new SaveDTO
            {
                timestampUtc     = DateTime.UtcNow.ToString("o"),
                energy           = BuildEnergyDTO(),
                wallet           = BuildWalletDTO(),
                locker           = EvidenceLockerService.ExportState(),
                overflow         = OverflowBucketService.ExportState(),
                specials         = UI.Specials.SpecialItemsService.ExportState(),
                flags            = GameFlags.ExportState(),
                currentEpisodeId = _switchTargetId ?? runningId,
            };

            var section = new EpisodeSectionDTO
            {
                episodeId = runningId,
                complete  = _currentEpisodeComplete,
                rows      = board.Rows,
                cols      = board.Cols,
                caseFlow  = BuildCaseFlowDTO(),
            };
            FillCells(section.cells);
            FillLeads(section.leads);
            dto.episodes.Add(section);

            // Dormant episodes ride along verbatim: a save while playing ep02
            // must never drop ep01's section.
            foreach (var dormant in _dormantSections)
                if (dormant != null && dormant.episodeId != runningId)
                    dto.episodes.Add(dormant);

            string json = JsonUtility.ToJson(dto, prettyPrint: false);
            Directory.CreateDirectory(Path.GetDirectoryName(_pathLive));

            try
            {
                AtomicSaveFile.Write(_pathLive, _pathPrev, _pathTmp, json);

                // Locker (0.7.0), Stash (0.8.0), Case Kit specials (0.9.0) and
                // story flags (1.0.0) are folded into the aggregate just written —
                // remove the pre-fold stores so they can't resurrect stale state
                // on a future boot.
                EvidenceLockerService.DeleteLegacyFile();
                OverflowBucketService.DeleteLegacyFile();
                UI.Specials.SpecialItemsService.DeleteLegacyKeys();
                GameFlags.DeleteLegacyKeys();
                return true;
            }
            catch (Exception ex)
            {
                if (File.Exists(_pathTmp))
                    File.Delete(_pathTmp);

                Debug.LogError($"[Save] write failed: {ex.Message}\nPath={_pathLive}");
                return false;
            }
        }

        public void TryLoad()
        {
            if (board == null) return;

            // A crash between the two File.Moves in TrySave can leave only the
            // .prev backup on disk, so a missing/corrupt live file falls back to it.
            if (!LoadFrom(_pathLive) && !LoadFrom(_pathPrev))
            {
                // No readable save: reset locker/specials statics (they survive
                // play sessions when domain reload is off) and migrate their
                // legacy stores if present. Fresh boot still binds the episode's
                // database (a no-op for ep01, whose database the scene serializes).
                EvidenceLockerService.ImportState(null);
                UI.Specials.SpecialItemsService.ImportState(null);
                _dormantSections.Clear();
                _currentEpisodeComplete = false;
                EnsureEpisodeDatabase();
            }
        }

        private bool LoadFrom(string path)
        {
            if (!File.Exists(path)) return false;

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                var dto = JsonUtility.FromJson<SaveDTO>(json);

                if (dto == null || dto.cells == null)
                {
                    Debug.LogWarning($"[Save] load failed (schema mismatch): {path}");
                    return false;
                }

                // 0.x saves: wrap the flat per-episode fields into a section keyed
                // by the id the save recorded ("e1_the_listener" in the wild — the
                // catalog's alias table owns it). Globals pass through untouched.
                SaveSchema.MigrateFlatToSection(dto, RunningEpisodeId());

                ApplyEnergy(dto.energy);
                ApplyWallet(dto.wallet);
                ApplyLocker(dto);
                ApplyOverflow(dto);
                ApplySpecials(dto);

                // Partition: apply the running episode's section; carry the rest
                // dormant. Section ids are canonicalized on the way in so a legacy
                // "e1_the_listener" section is owned by ep01.
                string runningId = RunningEpisodeId();
                _dormantSections.Clear();
                _currentEpisodeComplete = false;
                EpisodeSectionDTO current = null;
                foreach (var s in dto.episodes)
                {
                    if (s == null) continue;
                    if (current == null && Canonicalize(s.episodeId) == runningId)
                        current = s;
                    else
                        _dormantSections.Add(s);
                }

                EnsureEpisodeDatabase();

                if (current != null)
                {
                    ApplyCells(current);
                    ApplyCaseFlow(current.caseFlow);
                    ApplyLeads(current.leads);
                    _currentEpisodeComplete = current.complete;
                    Debug.Log($"[Save] loaded episode '{runningId}': {current.cells.Count} cells, {current.leads?.Count ?? 0} leads (+{_dormantSections.Count} dormant) from {path}");
                }
                else
                {
                    // No section for the running episode: a fresh episode start.
                    // The scene's default board and the database's design-time lead
                    // states ARE the correct initial state — apply nothing.
                    Debug.Log($"[Save] no section for episode '{runningId}' — fresh episode start (+{_dormantSections.Count} dormant) from {path}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save] load failed: {ex.Message}. Path={path}");
                return false;
            }
        }

        // --------------- Helpers ---------------

        private void FillCells(List<CellDTO> outList)
        {
            outList.Clear();
            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    var v = board.Get(r, c);
                    if (v == null || v.IsEmpty) continue;

                    outList.Add(new CellDTO
                    {
                        r = r,
                        c = c,
                        kind   = v.Kind == TileKind.Generator ? "Generator"
                               : v.Kind == TileKind.Special   ? "Special"
                               : "Item",
                        tier   = v.Tier,
                        family = board.GetFamily(v)
                    });
                }
            }
        }

        private void ApplyCells(EpisodeSectionDTO section)
        {
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                    board.Get(r, c)?.Clear();

            foreach (var cell in section.cells)
            {
                if (cell.r < 0 || cell.c < 0 || cell.r >= board.Rows || cell.c >= board.Cols) continue;
                var v = board.Get(cell.r, cell.c);
                if (v == null) continue;

                var family = string.IsNullOrEmpty(cell.family) ? board.defaultGeneratorFamily : cell.family;

                if (string.Equals(cell.kind, "Generator", StringComparison.OrdinalIgnoreCase))
                {
                    var genSO = board.FindGeneratorType(family);
                    int genTier = Mathf.Max(0, cell.tier);
                    // Legacy-save migration (diner 10→6 ruling, 2026-07-18): a save
                    // may hold generator tiers beyond the chain's current maximum —
                    // clamp to the SO's max-tier hero rather than strand an unknown
                    // tier (pre-release QA saves only; ruled acceptable).
                    if (genSO != null && genTier > genSO.maxGeneratorTier)
                    {
                        Debug.LogWarning($"[Save] Generator '{family}' T{genTier + 1} exceeds chain max — clamped to T{genSO.maxGeneratorTier + 1} (legacy save).");
                        genTier = genSO.maxGeneratorTier;
                    }
                    var sprite = genSO != null ? genSO.SpriteForTier(genTier)
                               : (board.generatorSprite != null ? board.generatorSprite
                               : (board.icons != null && board.icons.Count > 0 ? board.icons[0] : null));
                    v.SetGenerator(sprite, genTier);
                    board.AttachGeneratorAnimator(v, family, genTier);
                }
                else if (string.Equals(cell.kind, "Special", StringComparison.OrdinalIgnoreCase))
                {
                    // Board-tile specials (2026-08-12): family holds the SpecialId name.
                    v.SetSpecial(Enum.TryParse<UI.Specials.SpecialId>(family, out var sid)
                        ? UI.Specials.SpecialItemsService.SpriteFor(sid) : null);
                }
                else
                {
                    Sprite icon = board.SpriteForItem(family, Mathf.Max(0, cell.tier));
                    v.SetItem(icon, Mathf.Max(0, cell.tier));
                }

                board.SetFamily(v, family);
            }

            board.FireItemCreatedForCurrentBoard();
        }

        private static EnergyDTO BuildEnergyDTO()
        {
            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem) return null;

            var cfg = EnergyRuntime.Config;
            var mgr = EnergyRuntime.Manager;
            if (cfg == null || mgr == null) return null;

            mgr.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);

            var wallet = WalletLocator.Instance;
            return new EnergyDTO
            {
                current     = wallet?.Get(Currency.Energy) ?? 0,
                lastTickUtc = mgr.LastTickUtc.ToString("o")
            };
        }

        private static void ApplyEnergy(EnergyDTO energy)
        {
            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem) return;

            var cfg = EnergyRuntime.Config;
            if (cfg == null) return;

            if (energy == null)
            {
                if (EnergyRuntime.Manager == null)
                    EnergyRuntime.Manager = new EnergyManager(cfg.Start, cfg.Cap, DateTime.UtcNow);
                return;
            }

            if (!DateTime.TryParse(energy.lastTickUtc, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var last))
                last = DateTime.UtcNow;

            EnergyRuntime.Manager = new EnergyManager(0, cfg.Cap, lastTickUtc: last);

            // Apply offline regen: compute ticks since last save.
            // Regen fills toward the cap only, but a saved balance ABOVE the cap
            // must survive untouched — ingot ladder refills, the Starter Pack and
            // rewarded ads all grant past the cap on purpose (paid value). The old
            // Min-only clamp deleted that over-cap energy on every relaunch.
            int offlineTicks = EnergyRuntime.Manager.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);
            int restored = Math.Max(energy.current, Math.Min(energy.current + offlineTicks, cfg.Cap));

            // Seed wallet with restored balance
            var wallet = WalletLocator.Instance;
            if (wallet != null)
            {
                int existing = wallet.Get(Currency.Energy);
                if (existing > 0) wallet.TrySpend(Currency.Energy, existing);
                wallet.Grant("save.restore", Reward.Energy(restored));
            }
        }

        private static WalletDTO BuildWalletDTO()
        {
            var wallet = WalletLocator.Instance;
            if (wallet == null) return null;
            return new WalletDTO
            {
                soft    = wallet.Get(Currency.Soft),
                premium = wallet.Get(Currency.Premium)
            };
        }

        private static void ApplyWallet(WalletDTO dto)
        {
            if (dto == null) return;
            var wallet = WalletLocator.Instance;
            if (wallet == null) return;

            // Restore is set-to-saved, not additive: it wipes anything granted
            // earlier this boot. Grants of real-money value must wait for
            // WalletRestored (see below).
            int existingSoft = wallet.Get(Currency.Soft);
            if (existingSoft > 0) wallet.TrySpend(Currency.Soft, existingSoft);
            if (dto.soft > 0)     wallet.Grant("save.restore", Reward.Soft(dto.soft));

            int existingPremium = wallet.Get(Currency.Premium);
            if (existingPremium > 0) wallet.TrySpend(Currency.Premium, existingPremium);
            if (dto.premium > 0)     wallet.Grant("save.restore", Reward.Premium(dto.premium));
        }

        private static CaseFlowDTO BuildCaseFlowDTO()
        {
            var svc = CaseFlowLocator.Instance;
            if (svc == null) return null;
            var state = svc.Current;
            return new CaseFlowDTO
            {
                episodeId = state.Episode.Value,
                stepIndex = state.StepIndex
            };
        }

        private static void ApplyCaseFlow(CaseFlowDTO dto)
        {
            if (dto == null) return;
            var svc = CaseFlowLocator.Instance;
            if (svc == null) return;

            // Advance silently from current index to saved index.
            // CaseFlowOrchestratorMB.Start() already ran Begin() + FTUE catch-up,
            // so current StepIndex may already be > 0. The section this DTO came
            // from is keyed to the running episode, so the replay always targets
            // the step list it was recorded against.
            int target  = dto.stepIndex;
            int current = svc.Current.StepIndex;
            for (int i = current; i < target; i++)
                svc.CompleteCurrentStep();
        }

        private void FillLeads(List<LeadStateDTO> outList)
        {
            outList.Clear();
            if (_leadsRepo == null) return;

            foreach (var lead in _leadsRepo.CurrentLeads)
            {
                if (lead == null) continue;
                var dto = new LeadStateDTO
                {
                    leadId       = lead.leadId,
                    runtimeState = (int)lead.RuntimeState,
                    activated    = false,
                    satisfied    = lead.requirements != null
                                   ? Array.ConvertAll(lead.requirements, r => r.IsSatisfied)
                                   : Array.Empty<bool>()
                };
                outList.Add(dto);
            }

            foreach (var id in _leadsRepo.ActivatedLeadIds)
                outList.Add(new LeadStateDTO { leadId = id, activated = true });
        }

        private static void ApplyLocker(SaveDTO dto)
        {
            // JsonUtility auto-instantiates absent [Serializable] class fields, so
            // dto.locker is non-null (and empty) even for pre-0.7.0 saves — importing
            // it directly would silently wipe a migrating locker. Gate on the schema
            // version instead: older saves take the null path, which resets state and
            // migrates the legacy locker_state.json.
            EvidenceLockerService.ImportState(SaveSchema.AtLeast(dto.schemaVersion, 0, 7) ? dto.locker : null);
        }

        private static void ApplyOverflow(SaveDTO dto)
        {
            // Same JsonUtility caveat as ApplyLocker: dto.overflow is an empty list
            // (never null) for pre-0.8.0 saves, so importing it unconditionally would
            // wipe a migrating Stash. Older saves keep the state the bootstrap-time
            // OverflowBucketService.Load() already read from legacy overflow_state.json;
            // the next TrySave folds it in and deletes the legacy file.
            if (SaveSchema.AtLeast(dto.schemaVersion, 0, 8))
                OverflowBucketService.ImportState(dto.overflow);
        }

        private static void ApplySpecials(SaveDTO dto)
        {
            // Same JsonUtility caveat again: dto.specials is auto-instantiated
            // (empty, never null) for pre-0.9.0 saves — importing it directly
            // would wipe a migrating Case Kit. Older saves take the null path,
            // which resets statics and migrates the legacy PlayerPrefs keys.
            UI.Specials.SpecialItemsService.ImportState(
                SaveSchema.AtLeast(dto.schemaVersion, 0, 9) ? dto.specials : null);
        }

        private void ApplyLeads(List<LeadStateDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0 || _leadsRepo == null) return;

            var states = new LeadsRepository.LeadSaveState[dtos.Count];
            for (int i = 0; i < dtos.Count; i++)
            {
                states[i] = new LeadsRepository.LeadSaveState
                {
                    LeadId                = dtos[i].leadId,
                    RuntimeState          = dtos[i].runtimeState,
                    SatisfiedRequirements = dtos[i].satisfied,
                    Activated             = dtos[i].activated
                };
            }
            _leadsRepo.ApplySavedStates(states);
        }

        private int SnapshotHash()
        {
            unchecked
            {
                int h = 17;
                for (int r = 0; r < board.Rows; r++)
                {
                    for (int c = 0; c < board.Cols; c++)
                    {
                        var v = board.Get(r, c);
                        int kind = v == null || v.IsEmpty ? 0
                                 : v.Kind == TileKind.Generator ? 2
                                 : v.Kind == TileKind.Special   ? 3
                                 : 1;
                        int tier = v == null || v.IsEmpty ? -1 : v.Tier;

                        h = h * 31 + r;
                        h = h * 31 + c;
                        h = h * 31 + kind;
                        h = h * 31 + tier;
                    }
                }

                var wallet = WalletLocator.Instance;
                if (wallet != null)
                {
                    h = h * 31 + wallet.Get(Currency.Soft);
                    h = h * 31 + wallet.Get(Currency.Premium);

                    var flags = FeatureFlagsRuntime.Current;
                    if (flags != null && flags.EnergySystem)
                    {
                        h = h * 31 + wallet.Get(Currency.Energy);
                        // Must not depend on wall-clock "now": a now-relative term changes
                        // every second and made the debounced save fire continuously.
                        if (EnergyRuntime.Manager != null)
                            h = h * 31 + EnergyRuntime.Manager.LastTickUtc.GetHashCode();
                    }
                }

                if (_leadsRepo != null)
                {
                    foreach (var lead in _leadsRepo.CurrentLeads)
                    {
                        if (lead == null) continue;
                        h = h * 31 + lead.leadId.GetHashCode();
                        h = h * 31 + (int)lead.RuntimeState;
                    }
                }

                // Locker, Stash, Case Kit and story flags are part of the
                // aggregate: a store/retrieve/purchase/push/grant/consume/flag-set
                // must trigger the same debounced save a board change does. For
                // flags this IS the atomicity fix: the flag lands in the same
                // snapshot as the lead activation that set it.
                h = h * 31 + EvidenceLockerService.StateHash();
                h = h * 31 + OverflowBucketService.StateHash();
                h = h * 31 + UI.Specials.SpecialItemsService.StateHash();
                h = h * 31 + GameFlags.StateHash();

                // Episode identity and completion are aggregate state too: the
                // complete bit arriving must trigger the same debounced save.
                h = h * 31 + RunningEpisodeId().GetHashCode();
                h = h * 31 + (_currentEpisodeComplete ? 1 : 0);

                return h;
            }
        }
    }
}
