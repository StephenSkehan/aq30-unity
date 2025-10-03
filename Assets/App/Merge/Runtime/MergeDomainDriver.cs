using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AQ.App.UI.Board {
    // LEGACY: Disabled during sprint. MergeBoardController owns the board.
    [DisallowMultipleComponent]
    public sealed class MergeDomainDriver : MonoBehaviour {
#if UNITY_EDITOR
        private void OnValidate() { /* no-op */ }
#endif
    }
}
