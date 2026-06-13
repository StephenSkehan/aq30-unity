using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AQ.App.Content
{
    /// <summary>
    /// Loads CaseGraph assets from Addressables system by label—enables episodic content delivery.
    /// </summary>
    public class AddressableGraphLoader : MonoBehaviour
    {
        [Tooltip("Label to load CaseGraph assets by (default: Ep01).")]
        public string Label = "Ep01";
        
        public DialogueRunner Runner;

        void Start()
        {
            if (Runner == null) Runner = FindFirstObjectByType<DialogueRunner>();
            LoadGraphs();
        }

        public void LoadGraphs()
        {
            Addressables.InitializeAsync().Completed += _ => {
                Addressables.LoadAssetsAsync<CaseGraph>(Label, OnGraphLoaded).Completed += OnAllComplete;
            };
        }

        CaseGraph _loaded;

        void OnGraphLoaded(CaseGraph g)
        {
            if (g != null && _loaded == null) _loaded = g;
        }

        void OnAllComplete(AsyncOperationHandle<IList<CaseGraph>> op)
        {
            if (op.Status != AsyncOperationStatus.Succeeded || _loaded == null)
            {
                Debug.LogWarning("[Addr] No CaseGraph assets found for label '" + Label + "'.");
                return;
            }

            Debug.Log("[Addr] Loaded CaseGraph '" + _loaded.name + "' via label '" + Label + "'.");

            if (Runner == null) Runner = FindFirstObjectByType<DialogueRunner>();
            if (Runner != null) Runner.BootWithGraph(_loaded);
        }

        /// <summary>
        /// Menu or button can call this at runtime to clear cache between tests.
        /// </summary>
        public void ClearAddressablesCache()
        {
            Addressables.ClearResourceLocators();
            Caching.ClearCache();
            Debug.Log("[Addr] Cleared Addressables cache.");
        }
    }
}