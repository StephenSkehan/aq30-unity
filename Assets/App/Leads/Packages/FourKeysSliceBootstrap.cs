// Assembly: AQ.App
// File: Assets/App/Leads/Packages/FourKeysSliceBootstrap.cs
// Purpose: Dev-only entry to the Four Keys chapter 1 vertical slice. When the
//          slice pref is set (AQ > Four Keys Slice menu), the boot swaps the
//          running leads database for the slice database and installs the
//          package runtime + beat presenter, with no scene surgery
//          (BootstrapperAutoAssign pattern). The Listener plays untouched when
//          the pref is off. Known slice limitation, accepted for the feel test:
//          the Listener caseflow orchestrator still runs its own step machine
//          underneath; package beats and board play are what this slice tests.

using System.Collections;
using AQ.App.UI.Packages;
using UnityEngine;

namespace AQ.App.Leads.Packages
{
    // Drives a delayed re-swap: the caseflow orchestrator binds the ep01
    // catalog's Listener database a few frames after AfterSceneLoad, clobbering
    // an early swap, so the slice re-applies its database once the board and
    // wallet have restored and the caseflow has begun. Its own GameObject.
    public sealed class FourKeysSliceDriverMB : MonoBehaviour
    {
        public LeadsDatabase sliceDatabase;
        public LeadsRepository repository;

        private IEnumerator Start()
        {
            if (sliceDatabase == null) yield break;
            if (repository == null) repository = FindFirstObjectByType<LeadsRepository>();
            if (repository == null) yield break;

            // The caseflow binds the ep01 catalog's Listener database a few frames
            // after our AfterSceneLoad swap. Poll for ~4s using only AQ.App types
            // (no cross-assembly board refs): whenever the repo's current leads do
            // not lead with the slice's cards, re-apply. Stops once it sticks for
            // several frames. Re-apply resets activated ids, so we only re-apply
            // when actually clobbered, never on a settled slice board.
            string firstSliceId = sliceDatabase.Leads.Count > 0 ? sliceDatabase.Leads[0].leadId : null;
            int stableFrames = 0;
            for (int i = 0; i < 240 && stableFrames < 20; i++)
            {
                if (!RepoHasSliceCards(firstSliceId))
                {
                    repository.ReplaceFromDatabase(sliceDatabase);
                    stableFrames = 0;
                    Debug.Log("[FourKeysSlice] re-applied slice database after a clobber (" + sliceDatabase.Leads.Count + " cards).");
                }
                else
                {
                    stableFrames++;
                }
                yield return null;
            }
        }

        private bool RepoHasSliceCards(string firstSliceId)
        {
            if (string.IsNullOrEmpty(firstSliceId)) return true;
            foreach (var lead in repository.CurrentLeads)
                if (lead != null && lead.leadId == firstSliceId) return true;
            return false;
        }
    }

    public static class FourKeysSliceBootstrap
    {
        // EditorPrefs, not PlayerPrefs: QA reset is a full PlayerPrefs.DeleteAll
        // (DialogueFlags QA-reset semantics), which was silently disabling the
        // slice on every "QA reset & play". A dev toggle is editor state, not
        // player state. The slice is editor-only until a build needs it.
        public const string PrefKey = "aq.dev.fk_slice";

        public static bool Enabled
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetInt(PrefKey, 0) == 1;
#else
                return false;
#endif
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Enabled) return;

            var db = Resources.Load<LeadsDatabase>("App/FourKeys/FourKeysCh1Database");
            var catalog = Resources.Load<PackageCatalog>("App/FourKeys/FourKeysCh1Catalog");
            if (db == null || catalog == null)
            {
                Debug.LogWarning("[FourKeysSlice] pref set but slice assets missing under Resources/App/FourKeys; slice not installed.");
                return;
            }

            var repo = Object.FindFirstObjectByType<LeadsRepository>();
            if (repo == null)
            {
                Debug.LogWarning("[FourKeysSlice] no LeadsRepository in scene; slice not installed.");
                return;
            }

            var problems = catalog.Validate(db);
            foreach (var problem in problems)
                Debug.LogWarning("[FourKeysSlice] catalog: " + problem);

            repo.ReplaceFromDatabase(db);

            // Built inactive: AddComponent on an active object runs Awake/OnEnable
            // immediately, before the fields below are assigned. The runtime was
            // building its progress service from a null catalog (zero packages),
            // so no beat ever fired (slice playtest 2026-09-02).
            var go = new GameObject("FourKeysSlice");
            go.SetActive(false);
            var runtime = go.AddComponent<PackageRuntimeMB>();
            runtime.catalog = catalog;
            runtime.repository = repo;
            var presenter = go.AddComponent<PackageBeatPresenterMB>();
            presenter.runtime = runtime;
            presenter.dialogueRunner = Object.FindFirstObjectByType<DialogueRunner>(FindObjectsInactive.Include);
            var driver = go.AddComponent<FourKeysSliceDriverMB>();
            driver.sliceDatabase = db;
            driver.repository = repo;
            go.SetActive(true);

            Debug.Log("[FourKeysSlice] installed: " + catalog.packages.Count + " packages over " + db.Leads.Count + " cards.");
        }
    }
}
