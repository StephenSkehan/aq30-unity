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

using AQ.App.UI.Packages;
using UnityEngine;

namespace AQ.App.Leads.Packages
{
    public static class FourKeysSliceBootstrap
    {
        public const string PrefKey = "aq.dev.fk_slice";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (PlayerPrefs.GetInt(PrefKey, 0) != 1) return;

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

            var go = new GameObject("FourKeysSlice");
            var runtime = go.AddComponent<PackageRuntimeMB>();
            runtime.catalog = catalog;
            runtime.repository = repo;
            var presenter = go.AddComponent<PackageBeatPresenterMB>();
            presenter.runtime = runtime;
            presenter.dialogueRunner = Object.FindFirstObjectByType<DialogueRunner>(FindObjectsInactive.Include);

            Debug.Log("[FourKeysSlice] installed: " + catalog.packages.Count + " packages over " + db.Leads.Count + " cards.");
        }
    }
}
