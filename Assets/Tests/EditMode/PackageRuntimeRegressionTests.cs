// Assembly: AQ.Tests.EditMode
// File: Assets/Tests/EditMode/PackageRuntimeRegressionTests.cs
// Purpose: Regression pins for the three package-runtime bugs found in the
//          chapter 1 slice playtests (2026-09-02 / 03):
//          1. The runtime was constructed before its catalog was assigned
//             (AddComponent runs Awake first), so it held zero packages and no
//             beat ever fired. Fix: the service rebuilds lazily when the
//             catalog changes after construction.
//          2. The completion scan only ran on LeadsChanged, which the repository
//             raises only when an activation unlocks a gated card, so the
//             chapter's last card never produced a beat. Fix: a direct
//             activation hook that folds the just-activated id into the scan.
//          3. The install helper must build the object inactive and wire it
//             before activation (verified here by field state; Awake ordering
//             itself is a play-mode fact).
//          Edit mode never runs Awake/OnEnable on plain MonoBehaviours, so the
//          tests drive the public seams (Owns, RescanNow, NotifyBeatDismissed).

using System.Collections.Generic;
using AQ.App;
using AQ.App.Economy;
using AQ.App.Leads;
using AQ.App.Leads.Packages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AQ.Tests.EditMode
{
    public class PackageRuntimeRegressionTests
    {
        private readonly List<Object> _made = new List<Object>();
        private AQ.SharedKernel.Economy.IWallet _priorWallet;
        private readonly List<PackageData> _fired = new List<PackageData>();

        [SetUp]
        public void SetUp()
        {
            // Memory-backed flags, empty: never touch PlayerPrefs from a test.
            GameFlags.ImportState(new List<string>());
            _priorWallet = WalletLocator.Instance;
            WalletLocator.Set(null);
            _fired.Clear();
            PackageRuntimeMB.BeatReady += OnBeat;
        }

        [TearDown]
        public void TearDown()
        {
            PackageRuntimeMB.BeatReady -= OnBeat;
            WalletLocator.Set(_priorWallet);
            GameFlags.ResetForTests();
            foreach (var o in _made) if (o != null) Object.DestroyImmediate(o);
            _made.Clear();
        }

        private void OnBeat(PackageData p) => _fired.Add(p);

        private PackageData MakePackage(string id, params string[] members)
        {
            var p = ScriptableObject.CreateInstance<PackageData>();
            p.packageId = id;
            p.memberCardIds = members;
            p.chapter = 1;
            _made.Add(p);
            return p;
        }

        private PackageCatalog MakeCatalog(params PackageData[] packages)
        {
            var c = ScriptableObject.CreateInstance<PackageCatalog>();
            c.packages.AddRange(packages);
            _made.Add(c);
            return c;
        }

        private GameObject MakeGo(string name)
        {
            var go = new GameObject(name);
            _made.Add(go);
            return go;
        }

        [Test]
        public void Owns_ResolvesWhenCatalogIsAssignedAfterConstruction()
        {
            // The bug: the service was built once, over a null catalog, and never
            // rebuilt. Assign the catalog after the component exists and ask.
            var runtime = MakeGo("rt").AddComponent<PackageRuntimeMB>();
            Assert.IsFalse(runtime.Owns("fk_p01_01a"), "no catalog yet");

            runtime.catalog = MakeCatalog(MakePackage("fk_p01_01", "fk_p01_01a"));
            Assert.IsTrue(runtime.Owns("fk_p01_01a"), "catalog assigned after construction must still resolve");
            Assert.IsFalse(runtime.Owns("not_a_member"));
        }

        [Test]
        public void RescanNow_FoldsInTheJustActivatedId_AndFiresBeatReadyOnce()
        {
            // The bug: the last card's activation unlocked nothing, so LeadsChanged
            // never fired and the final beat was stranded. The direct hook folds
            // the activated id into the scan even when the repository has not
            // recorded it yet.
            var repo = MakeGo("repo").AddComponent<LeadsRepository>(); // no database: empty activated set
            var runtime = MakeGo("rt").AddComponent<PackageRuntimeMB>();
            runtime.repository = repo;
            var pkg = MakePackage("fk_p01_10b", "fk_p01_10b");
            runtime.catalog = MakeCatalog(pkg);

            runtime.RescanNow("fk_p01_10b");
            Assert.AreEqual(1, _fired.Count, "beat fires on the activation hook");
            Assert.AreSame(pkg, _fired[0]);
            CollectionAssert.Contains(runtime.PendingBeats, pkg);

            runtime.RescanNow("fk_p01_10b");
            Assert.AreEqual(1, _fired.Count, "a pending beat is not re-fired by a second scan");
        }

        [Test]
        public void NotifyBeatDismissed_MarksSeen_ClearsPending_AndNeverRefires()
        {
            var repo = MakeGo("repo").AddComponent<LeadsRepository>();
            var runtime = MakeGo("rt").AddComponent<PackageRuntimeMB>();
            runtime.repository = repo;
            var pkg = MakePackage("fk_p01_03", "fk_p01_03a");
            pkg.softCurrency = 10;
            runtime.catalog = MakeCatalog(pkg);

            runtime.RescanNow("fk_p01_03a");
            Assert.AreEqual(1, _fired.Count);

            // No wallet in the locator: pay is skipped, seen still lands (rule 5:
            // seen after display), and the pending list empties.
            runtime.NotifyBeatDismissed(pkg);
            Assert.IsTrue(GameFlags.Has(pkg.BeatSeenFlag), "beat_seen set on dismissal");
            Assert.IsFalse(GameFlags.Has(pkg.BeatPaidFlag), "no wallet, so nothing was paid");
            Assert.IsEmpty(runtime.PendingBeats);

            runtime.RescanNow("fk_p01_03a");
            Assert.AreEqual(1, _fired.Count, "a seen beat never re-fires");
        }

        [Test]
        public void EnsureInstalled_ReturnsNull_ForNullCatalog_OrNoRepository()
        {
            Assert.IsNull(PackageRuntimeMB.EnsureInstalled(null));

            var catalog = MakeCatalog(MakePackage("p", "a"));
            // No LeadsRepository exists in the edit-mode scene for this test.
            foreach (var stray in Object.FindObjectsByType<LeadsRepository>(FindObjectsSortMode.None))
                Object.DestroyImmediate(stray.gameObject);
            LogAssert.Expect(LogType.Warning, "[Packages] no LeadsRepository in scene; package runtime not installed.");
            Assert.IsNull(PackageRuntimeMB.EnsureInstalled(catalog));
        }

        [Test]
        public void EnsureInstalled_WiresCatalogAndRepositoryBeforeActivation()
        {
            var repo = MakeGo("repo").AddComponent<LeadsRepository>();
            var catalog = MakeCatalog(MakePackage("fk_p01_01", "fk_p01_01a"));

            var runtime = PackageRuntimeMB.EnsureInstalled(catalog);
            Assert.IsNotNull(runtime);
            _made.Add(runtime.gameObject);

            Assert.AreSame(catalog, runtime.catalog);
            Assert.AreSame(repo, runtime.repository);
            Assert.IsTrue(runtime.gameObject.activeSelf, "activated after wiring");
            Assert.IsTrue(runtime.Owns("fk_p01_01a"), "the installed runtime knows its members");
            Assert.IsNotNull(runtime.GetComponent<AQ.App.UI.Packages.PackageBeatPresenterMB>(), "presenter installed beside it");
        }
    }
}
