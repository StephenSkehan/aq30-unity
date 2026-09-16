using System.Collections.Generic;
using AQ.App.Episodes;
using AQ.App.Leads;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// THE story-flag store (unified 2026-08-18; folded into the save aggregate
    /// 2026-08-27, schema 1.0.0). Historically there were two stores —
    /// NarrativeFlags (nar_flag_*) and DialogueFlags (dlg_flag_*) — with identical
    /// APIs and identical flag NAMES landing in different PlayerPrefs keys, so a
    /// read through the wrong store compiled fine and silently never matched
    /// (this shipped at least two defects: the silenced hint system, the armed
    /// DropRoller gates). Both old classes forward here.
    ///
    /// The fold: a lead activation writes its state into BoardSaveSystem's
    /// aggregate but used to write its NarrativeFlags into PlayerPrefs — two
    /// stores, one transaction, and a crash between them could separate the
    /// halves (robustness rule 1's exact bug class; e1.ep01.complete is one of
    /// those flags). Flags now mutate memory only; BoardSaveSystem exports them
    /// into the same atomic snapshot as the lead state that set them.
    ///
    /// Modes: after ImportState (BoardSaveSystem.Awake, every play boot) the
    /// store is memory-backed. Before it — edit-mode tooling, tests, dev scenes
    /// with no save system — it behaves exactly as the pre-fold PlayerPrefs
    /// store, lazy legacy migration included.
    ///
    /// Migration without enumeration: PlayerPrefs cannot list its keys, so the
    /// null-import path probes the DOMAIN of names instead — every flag content
    /// declares (lead flags, .seen convention, dialogue setsFlag/requiresFlag)
    /// plus a system list frozen at fold time. A flag no content ever reads
    /// cannot matter; a flag content reads is by construction in the probe.
    /// Known cosmetic loss, ruled acceptable: hint one-shots and QA flags set by
    /// builds older than the frozen list re-arm once.
    /// </summary>
    public static class GameFlags
    {
        private const string Prefix    = "flag_";
        private const string LegacyNar = "nar_flag_";
        private const string LegacyDlg = "dlg_flag_";

        // null = passthrough (pre-import). Non-null = memory-backed by the aggregate.
        private static HashSet<string> _state;
        // Names the probe migrated; their prefs keys are deleted after the first
        // successful aggregate write (DeleteLegacyKeys), never before.
        private static List<string> _probedNames;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _state = null;
            _probedNames = null;
        }

        public static void Set(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;

            if (_state != null)
            {
                if (_state.Add(flag))
                    Debug.Log($"[GameFlags] Set: {flag}");
                return;
            }

            PlayerPrefs.SetInt(Prefix + flag, 1);
            PlayerPrefs.Save();
            Debug.Log($"[GameFlags] Set: {flag}");
        }

        public static bool Has(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;

            if (_state != null) return _state.Contains(flag);

            if (PlayerPrefs.GetInt(Prefix + flag, 0) == 1) return true;

            // Lazy legacy migration (see class doc) — passthrough mode only.
            if (PlayerPrefs.GetInt(LegacyNar + flag, 0) == 1 ||
                PlayerPrefs.GetInt(LegacyDlg + flag, 0) == 1)
            {
                PlayerPrefs.SetInt(Prefix + flag, 1);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        public static void Clear(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            _state?.Remove(flag);
            // Prefs variants are deleted in BOTH modes: a cleared flag must not
            // resurrect from a lingering legacy key on a future probe.
            PlayerPrefs.DeleteKey(Prefix + flag);
            PlayerPrefs.DeleteKey(LegacyNar + flag);
            PlayerPrefs.DeleteKey(LegacyDlg + flag);
            PlayerPrefs.Save();
            Debug.Log($"[GameFlags] Cleared: {flag}");
        }

        // ---- Save-aggregate fold (ExportState/ImportState/StateHash pattern) ----

        /// <summary>Sorted for a deterministic file; BoardSaveSystem.TrySave calls this.</summary>
        public static List<string> ExportState()
        {
            var list = _state != null ? new List<string>(_state) : new List<string>();
            list.Sort(System.StringComparer.Ordinal);
            return list;
        }

        /// <summary>
        /// Hydrate from the aggregate. Non-null = a 1.0.0 save's flag list.
        /// Null = pre-1.0.0 save or fresh boot: reset and probe known names out
        /// of PlayerPrefs (see class doc). Runs in BoardSaveSystem.Awake so every
        /// Start-order reader, whatever its tie-break order, sees hydrated flags.
        /// </summary>
        public static void ImportState(List<string> flags)
        {
            if (flags != null)
            {
                _state = new HashSet<string>();
                foreach (var f in flags)
                    if (!string.IsNullOrEmpty(f)) _state.Add(f);
                return;
            }

            _state = new HashSet<string>();
            _probedNames = GameFlagsLegacyProbe.KnownNames();
            foreach (var name in _probedNames)
            {
                if (PlayerPrefs.GetInt(Prefix + name, 0) == 1 ||
                    PlayerPrefs.GetInt(LegacyNar + name, 0) == 1 ||
                    PlayerPrefs.GetInt(LegacyDlg + name, 0) == 1)
                {
                    _state.Add(name);
                }
            }
            if (_state.Count > 0)
                Debug.Log($"[GameFlags] Migrated {_state.Count} legacy flag(s) into the aggregate");
        }

        /// <summary>Order-insensitive, mixed into BoardSaveSystem.SnapshotHash: a flag write must trigger the debounced save.</summary>
        public static int StateHash()
        {
            if (_state == null) return 0;
            unchecked
            {
                int h = 0;
                foreach (var f in _state) h += f.GetHashCode(); // commutative on purpose
                return h * 31 + _state.Count;
            }
        }

        /// <summary>
        /// Remove the probed legacy PlayerPrefs keys — called by BoardSaveSystem
        /// only AFTER a successful aggregate write, so a crash between probe and
        /// first save re-probes the same values and loses nothing.
        /// </summary>
        public static void DeleteLegacyKeys()
        {
            if (_probedNames == null) return;
            foreach (var name in _probedNames)
            {
                PlayerPrefs.DeleteKey(Prefix + name);
                PlayerPrefs.DeleteKey(LegacyNar + name);
                PlayerPrefs.DeleteKey(LegacyDlg + name);
            }
            PlayerPrefs.Save();
            _probedNames = null;
        }

        /// <summary>QA reset: empty the store but stay memory-backed (ClearSave path).</summary>
        public static void ResetForNewSave()
        {
            if (_state != null) _state = new HashSet<string>();
        }

        /// <summary>Test seam: back to passthrough (pre-import) mode.</summary>
        public static void ResetForTests() => ResetStatics();
    }

    /// <summary>
    /// The migration probe's name domain. Content-derived names track whatever
    /// the catalog's databases declare; the system list is FROZEN at fold time —
    /// a pre-fold install can only hold flags a pre-fold build could write, so
    /// the frozen list is complete by construction (new system flags are born
    /// inside the aggregate and never need probing).
    /// </summary>
    internal static class GameFlagsLegacyProbe
    {
        private static readonly string[] SystemNames =
        {
            // Hint one-shots: HintService.FlagPrefix ("aq.hint.") + its id set.
            // Duplicated here because HintService lives in Assembly-CSharp, which
            // this assembly cannot reference; the list is frozen, not synced.
            "aq.hint.casecash", "aq.hint.tick", "aq.hint.help", "aq.hint.energy",
            "aq.hint.stash", "aq.hint.locker", "aq.hint.boardzoom", "aq.hint.dossier",
            "aq.hint.evidence", "aq.hint.casekit", "aq.hint.swap", "aq.hint.longpress",
            // Drop-table character gate (GeneratorTypeSO.requiresStoryFlag).
            "aq.char.arthur.active",
        };

        public static List<string> KnownNames()
        {
            var names = new HashSet<string>(SystemNames);

            var catalog = EpisodeRuntime.Catalog;
            if (catalog != null)
                foreach (var entry in catalog.Episodes)
                    CollectDatabase(entry != null ? entry.database : null, names);

            return new List<string>(names);
        }

        private static void CollectDatabase(LeadsDatabase db, HashSet<string> names)
        {
            if (db == null) return;
            foreach (var lead in db.Leads)
            {
                if (lead == null) continue;
                AddAll(names, lead.NarrativeFlags);
                AddOne(names, lead.requiresFlag);
                AddOne(names, lead.forbidsFlag);
                if (!string.IsNullOrEmpty(lead.leadId))
                    names.Add("aq.lead." + lead.leadId + ".seen");
                CollectGraph(lead.resolutionDialogue, names);
            }
        }

        private static void CollectGraph(CaseGraph graph, HashSet<string> names)
        {
            if (graph == null || graph.nodes == null) return;
            foreach (var node in graph.nodes)
            {
                if (node == null) continue;
                AddOne(names, node.setsFlag);
                AddOne(names, node.requiresFlag);
                if (node.choices == null) continue;
                foreach (var choice in node.choices)
                    if (choice != null) AddOne(names, choice.requiresFlag);
            }
        }

        private static void AddAll(HashSet<string> names, string[] flags)
        {
            if (flags == null) return;
            foreach (var f in flags) AddOne(names, f);
        }

        private static void AddOne(HashSet<string> names, string flag)
        {
            if (!string.IsNullOrEmpty(flag)) names.Add(flag);
        }
    }
}
