// Assets/Scripts/UI/Board/BoardTools.cs
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    /// <summary>
    /// Lightweight helpers for locating the active Merge Board parts without
    /// referencing deprecated types. Safe with Unity 6000 APIs.
    /// </summary>
    public static class BoardTools
    {
        /// <summary>Find the first MergeBoardController in the scene (any stage).</summary>
        public static MergeBoardController FindController()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<MergeBoardController>();
#else
            return Object.FindObjectOfType<MergeBoardController>();
#endif
        }

        /// <summary>Find the board root RectTransform from the active controller, if any.</summary>
        public static RectTransform FindBoardRoot()
        {
            var ctrl = FindController();
            return ctrl ? ctrl.BoardRoot : null;
        }

        /// <summary>Find the GridLayoutGroup on the active board root, if any.</summary>
        public static GridLayoutGroup FindGrid()
        {
            var ctrl = FindController();
            return ctrl ? ctrl.Grid : null;
        }
    }
}
