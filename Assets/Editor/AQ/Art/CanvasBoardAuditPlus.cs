#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Art
{
    public static class CanvasBoardAuditPlus
    {
        [MenuItem("AQ/Art/Audit Canvas_Board (Deep, tolerant)")]
        public static void RunTolerant()
        {
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas)
            {
                Debug.LogWarning("[AQ Art] Canvas_Board not found in scene.");
                return;
            }

            // Try to find AvatarChip anywhere under Canvas_Board
            var avatarChip = FindDeep(canvas.transform, "AvatarChip");
            if (!avatarChip)
            {
                Debug.LogWarning("[AQ Art] AvatarChip not found under Canvas_Board.");
                return;
            }

            // Look for EpisodeChip with tolerant rules:
            //  - exact direct child named "EpisodeChip" (ideal)
            //  - OR any descendant whose normalized name equals "episodechip"
            //  - accepts "(Clone)" and trims whitespace; case-insensitive
            var direct = avatarChip.Find("EpisodeChip");
            var deep   = FindDeepNormalized(avatarChip, "episodechip");

            if (direct)
            {
                Debug.Log($"[AQ Art] ✓ EpisodeChip present as direct child of AvatarChip (path: {PathOf(direct)}).");
            }
            else if (deep)
            {
                Debug.Log($"[AQ Art] ⚠ EpisodeChip found as descendant (not direct): {PathOf(deep)}. " +
                          $"Use 'AQ/Art/Fix/Normalize EpisodeChip Under AvatarChip' to reparent/rename (non-destructive).");
            }
            else
            {
                // No match: show a compact child inventory to help spot near-misses
                var childNames = string.Join(", ", avatarChip.Cast<Transform>().Select(t => $"\"{t.name}\""));
                Debug.LogWarning($"[AQ Art] ✗ EpisodeChip not located under AvatarChip.\n" +
                                 $"Immediate children: [{childNames}]\n" +
                                 $"Tip: name must be literally 'EpisodeChip' (case-insensitive; no trailing spaces).");
            }

            // Extra: quick Canvas summary (helps keep parity with your deep audit)
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler)
            {
                Debug.Log($"[AQ Art] Canvas scaler: {scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight:0.00}");
            }
        }

        [MenuItem("AQ/Art/Fix/Normalize EpisodeChip Under AvatarChip")]
        public static void NormalizeEpisodeChip()
        {
            var canvas = GameObject.Find("Canvas_Board");
            if (!canvas) { Debug.LogWarning("[AQ Art] Canvas_Board not found."); return; }

            var avatarChip = FindDeep(canvas.transform, "AvatarChip");
            if (!avatarChip) { Debug.LogWarning("[AQ Art] AvatarChip not found."); return; }

            var chip = avatarChip.Find("EpisodeChip") ?? FindDeepNormalized(avatarChip, "episodechip");
            if (!chip)
            {
                // Create a minimal holder if absolutely nothing is found
                var go = new GameObject("EpisodeChip", typeof(RectTransform));
                go.transform.SetParent(avatarChip, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(64, 24);
                Debug.Log("[AQ Art] Created EpisodeChip placeholder under AvatarChip.");
            }
            else
            {
                if (chip.parent != avatarChip)
                {
                    // Preserve local values while moving under AvatarChip
                    Undo.RecordObject(chip, "Normalize EpisodeChip");
                    chip.SetParent(avatarChip, worldPositionStays: false);
                }
                // Normalize name
                if (chip.name != "EpisodeChip")
                    chip.name = "EpisodeChip";

                // Make sure it's active
                chip.gameObject.SetActive(true);

                Debug.Log($"[AQ Art] EpisodeChip normalized at: {PathOf(chip)}");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("AQ/Art/Debug/List AvatarChip Children")]
        public static void ListAvatarChipChildren()
        {
            var canvas = GameObject.Find("Canvas_Board");
            var avatar = canvas ? FindDeep(canvas.transform, "AvatarChip") : null;
            if (!avatar) { Debug.LogWarning("[AQ Art] AvatarChip not found."); return; }

            var lines = avatar.Cast<Transform>().Select(t => $" - {t.name}");
            Debug.Log("[AQ Art] AvatarChip immediate children:\n" + string.Join("\n", lines));
        }

        // ---------- helpers ----------
        private static Transform FindDeep(Transform root, string exactName)
        {
            foreach (Transform t in root)
            {
                if (t.name == exactName) return t;
                var found = FindDeep(t, exactName);
                if (found) return found;
            }
            return null;
        }

        private static Transform FindDeepNormalized(Transform root, string normalizedTarget)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (Normalize(t.name) == normalizedTarget)
                    return t;
            }
            return null;
        }

        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Trim();
            // drop common Unity suffixes like (Clone)
            if (s.EndsWith("(Clone)")) s = s.Substring(0, s.Length - "(Clone)".Length).TrimEnd();
            return s.ToLowerInvariant();
        }

        private static string PathOf(Transform t)
        {
            var path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }
    }
}
#endif
