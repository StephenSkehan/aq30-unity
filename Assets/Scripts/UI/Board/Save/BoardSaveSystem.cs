using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using AQ.App.CaseFlow;
using AQ.App.Config;
using AQ.App.Economy;
using AQ.App.Generators;
using AQ.App.Leads;
using AQ.App.Overflow;
using AQ.App.Services;
using AQ.SharedKernel.CaseFlow;
using AQ.SharedKernel.Economy;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Saves and loads the board state (items/generators) and global energy in one JSON file.
    /// Atomic write with rolling .prev.json backup. Schema 0.5.0.
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

        /// <summary>
        /// True once this scene's save (or absence of one) has been applied to the
        /// wallet. Restore is destructive (set-to-saved), so anything of real-money
        /// value — IAP credits, restored purchases — must not be granted until this
        /// is true. Goes false again during a scene reload until the new Start() runs.
        /// </summary>
        public static bool WalletRestored { get; private set; }

        /// <summary>Fired right after WalletRestored becomes true.</summary>
        public static event Action WalletRestoreCompleted;

        // --------------- Unity ---------------

        private void Awake()
        {
            WalletRestored = false;

            if (!board)
                board = FindFirstObjectByType<MergeBoardController>();

            _leadsRepo = FindFirstObjectByType<LeadsRepository>();

            var root = Application.persistentDataPath;
            _pathLive = Path.Combine(root, fileName);
            _pathTmp  = _pathLive + ".tmp";
            _pathPrev = Path.Combine(root, Path.GetFileNameWithoutExtension(_pathLive) + ".prev.json");
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
        }

        private void OnDestroy()
        {
            if (_observedWallet != null)
                _observedWallet.Changed -= OnWalletChanged;
        }

        // Premium is real-money value: persist immediately instead of waiting out
        // the debounce window, so a crash can't eat a purchase credit.
        private void OnWalletChanged(WalletChanged e)
        {
            if (e.Currency != Currency.Premium) return;
            TrySave();
            _lastSnapshotHash = SnapshotHash();
            _nextSaveAt = -1f;
        }

        private void LateUpdate()
        {
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
            if (paused) TrySave();
        }

        private void OnApplicationQuit()
        {
            TrySave();
        }

        // --------------- Save / Load ---------------

        [Serializable]
        private sealed class CellDTO
        {
            public int r;
            public int c;
            public string kind;   // "Item" | "Generator"
            public int tier;      // 0-based
            public string family; // e.g. "corner_diner" — empty in legacy saves
        }

        [Serializable]
        private sealed class EnergyDTO
        {
            public int current;
            public string lastTickUtc; // ISO-8601 string
        }

        [Serializable]
        private sealed class WalletDTO
        {
            public int soft;
            public int premium;
        }

        [Serializable]
        private sealed class CaseFlowDTO
        {
            public string episodeId;
            public int stepIndex;
        }

        [Serializable]
        private sealed class LeadStateDTO
        {
            public string leadId;
            public int    runtimeState;
            public bool[] satisfied;
            public bool   activated;
        }

        [Serializable]
        private sealed class SaveDTO
        {
            public string schemaVersion = "0.6.0";
            public string timestampUtc;

            public int rows;
            public int cols;

            public List<CellDTO>      cells    = new List<CellDTO>();
            public EnergyDTO          energy;
            public WalletDTO          wallet;
            public CaseFlowDTO        caseFlow;
            public List<LeadStateDTO> leads    = new List<LeadStateDTO>();
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
            Debug.Log("[Save] BoardSaveSystem cleared");
        }

        public void TrySave()
        {
            if (board == null) return;

            var dto = new SaveDTO
            {
                timestampUtc = DateTime.UtcNow.ToString("o"),
                rows     = board.Rows,
                cols     = board.Cols,
                energy   = BuildEnergyDTO(),
                wallet   = BuildWalletDTO(),
                caseFlow = BuildCaseFlowDTO(),
            };
            FillCells(dto.cells);
            FillLeads(dto.leads);

            string json = JsonUtility.ToJson(dto, prettyPrint: false);
            Directory.CreateDirectory(Path.GetDirectoryName(_pathLive));

            try
            {
                File.WriteAllText(_pathTmp, json, Encoding.UTF8);

                if (File.Exists(_pathPrev))
                    File.Delete(_pathPrev);

                if (File.Exists(_pathLive))
                    File.Move(_pathLive, _pathPrev);

                File.Move(_pathTmp, _pathLive);

                //Debug.Log($"[Save] wrote {dto.cells.Count} cells → {_pathLive}");
            }
            catch (Exception ex)
            {
                if (File.Exists(_pathTmp))
                    File.Delete(_pathTmp);

                Debug.LogError($"[Save] write failed: {ex.Message}\nPath={_pathLive}");
            }
        }

        public void TryLoad()
        {
            if (board == null) return;

            // A crash between the two File.Moves in TrySave can leave only the
            // .prev backup on disk, so a missing/corrupt live file falls back to it.
            if (!LoadFrom(_pathLive))
                LoadFrom(_pathPrev);
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

                ApplyCells(dto);
                ApplyEnergy(dto.energy);
                ApplyWallet(dto.wallet);
                ApplyCaseFlow(dto.caseFlow);
                ApplyLeads(dto.leads);

                Debug.Log($"[Save] loaded {dto.cells.Count} cells, {dto.leads?.Count ?? 0} leads from {path}");
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
                        kind   = v.Kind == TileKind.Generator ? "Generator" : "Item",
                        tier   = v.Tier,
                        family = board.GetFamily(v)
                    });
                }
            }
        }

        private void ApplyCells(SaveDTO dto)
        {
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                    board.Get(r, c)?.Clear();

            foreach (var cell in dto.cells)
            {
                if (cell.r < 0 || cell.c < 0 || cell.r >= board.Rows || cell.c >= board.Cols) continue;
                var v = board.Get(cell.r, cell.c);
                if (v == null) continue;

                var family = string.IsNullOrEmpty(cell.family) ? board.defaultGeneratorFamily : cell.family;

                if (string.Equals(cell.kind, "Generator", StringComparison.OrdinalIgnoreCase))
                {
                    var genSO = board.FindGeneratorType(family);
                    var sprite = genSO != null ? genSO.SpriteForTier(Mathf.Max(0, cell.tier))
                               : (board.generatorSprite != null ? board.generatorSprite
                               : (board.icons != null && board.icons.Count > 0 ? board.icons[0] : null));
                    v.SetGenerator(sprite, Mathf.Max(0, cell.tier));
                    board.AttachGeneratorAnimator(v, family, Mathf.Max(0, cell.tier));
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

            // Apply offline regen: compute ticks since last save
            int offlineTicks = EnergyRuntime.Manager.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);
            int restored = Math.Min(energy.current + offlineTicks, cfg.Cap);

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
            // so current StepIndex may already be > 0.
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
                        int kind = v == null ? 0 : (v.IsEmpty ? 0 : (v.Kind == TileKind.Generator ? 2 : 1));
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

                return h;
            }
        }
    }
}
