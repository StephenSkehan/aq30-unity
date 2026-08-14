using UnityEditor;
using UnityEngine;
using AQ.App.UI.EvidenceBoard;
using LocationService = AQ.App.UI.EvidenceBoard.LocationService; // UnityEngine has a GPS type of the same name

namespace AQ.EditorTools
{
    public static class QALocations
    {
        [MenuItem("AQ/QA/Locations Reset")]
        public static void Reset()
        {
            LocationService.ResetAll();
            Debug.Log("[QALocations] location history state cleared");
        }

        [MenuItem("AQ/QA/Locations Status")]
        public static void Status()
        {
            var sb = new System.Text.StringBuilder("[QALocations] ");
            foreach (var def in LocationCatalog.All)
                sb.Append($"{def.key}={LocationService.UnlockedCount(def.key)}/{def.details.Length} ");
            Debug.Log(sb.ToString());
        }
    }
}
