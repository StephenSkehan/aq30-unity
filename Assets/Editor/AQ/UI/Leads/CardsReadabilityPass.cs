#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Sets Text_Title, Text_Objective, Text_LeadId on cards under Content_Leads to dark, readable colors.
    /// Also ensures Truncate overflow, NoWrap, raycast off. No other changes.
    /// </summary>
    public static class CardsReadabilityPass
    {
        [MenuItem("AQ/UI/Leads/Apply Card Readability (Dark Text)")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content)
            {
                Debug.LogWarning("[AQ Readability] Content_Leads not found.");
                return;
            }

            var dark      = new Color32(0x11, 0x11, 0x11, 0xFF); // near-black
            var darkMinor = new Color32(0x33, 0x33, 0x33, 0xFF); // for objective

            int changed = 0;
            foreach (Transform card in content)
            {
                changed += SetTMP(card, "Text_Title",     dark);
                changed += SetTMP(card, "Text_Objective", darkMinor);
                changed += SetTMP(card, "Text_LeadId",    darkMinor);
            }

            Debug.Log($"[AQ Readability] Updated {changed} TMP fields to dark colors.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static int SetTMP(Transform card, string childName, Color32 color)
        {
            var t = card.Find(childName)?.GetComponent<TMP_Text>();
            if (!t) return 0;
            t.color = color;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Truncate;
            t.raycastTarget = false;
            return 1;
        }
    }
}
#endif
