#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AQ.Editor.Art
{
    public static class EpisodeChipVerify
    {
        [MenuItem("AQ/Art/EpisodeChip/Report")]
        public static void Report()
        {
            var avatar = FindAvatar();
            if (!avatar) return;

            var namedChip = avatar.Find("EpisodeChip") as RectTransform;
            var anyEpLike = FindEpLike(avatar);

            Debug.Log($"🔎 EpisodeChip report under TopBar/AvatarChip\n" +
                      $"- Named 'EpisodeChip': {(namedChip ? "FOUND" : "MISSING")}\n" +
                      $"- Any EP-like child  : {(anyEpLike ? $"FOUND ({anyEpLike.name})" : "MISSING")}\n" +
                      $"(Audit expects a child literally named 'EpisodeChip'. If your chip is present but named differently, use AQ → Art → EpisodeChip → Adopt Existing.)");
            if (namedChip)
            {
                DumpRect(namedChip, "EpisodeChip");
            }
            if (anyEpLike && anyEpLike != namedChip)
            {
                DumpRect(anyEpLike, anyEpLike.name);
            }
        }

        [MenuItem("AQ/Art/EpisodeChip/Adopt Existing (rename to 'EpisodeChip')")]
        public static void AdoptExisting()
        {
            var avatar = FindAvatar();
            if (!avatar) return;

            // If already present, just ensure sizing/anchoring.
            var chip = avatar.Find("EpisodeChip") as RectTransform;
            if (!chip)
            {
                // Try to find an EP-like label/container to adopt.
                chip = FindEpLike(avatar);
                if (!chip)
                {
                    Debug.LogWarning("⚠️ No EP-like child found under AvatarChip to adopt. Nothing changed.");
                    return;
                }
                Undo.RecordObject(chip, "Rename to EpisodeChip");
                chip.name = "EpisodeChip";
            }

            // Ensure anchors/size per spec: 64x28, bottom-right overlap (-6, +6)
            EnsureChipRect(chip);

            // If this chip has a TMP child, ensure readable sizing
            var tmp = chip.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp)
            {
                Undo.RecordObject(tmp, "EpisodeChip TMP tweak");
                tmp.fontSize = 22;
#if UNITY_2022_2_OR_NEWER
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
#else
                tmp.enableWordWrapping = false;
#endif
                tmp.alignment = TextAlignmentOptions.Center;
                // teal text on cream pill
                tmp.color = new Color(0.10f, 0.71f, 0.71f, 1f); // ~#21B6B6
            }

            // If no Image on the chip root, add a simple cream pill (you can swap to 9-slice later)
            var img = chip.GetComponent<Image>();
            if (!img) img = Undo.AddComponent<Image>(chip.gameObject);
            img.type = Image.Type.Sliced;
            if (img.sprite == null) img.color = new Color(1f, 0.976f, 0.93f, 1f); // cream

            Debug.Log("✅ Adopted/normalized existing episode chip as 'EpisodeChip'. Audit will be happy.");
        }

        // --- helpers ---
        private static RectTransform FindAvatar()
        {
            var topBar = GameObject.Find("TopBar");
            if (!topBar) { Debug.LogError("❌ TopBar not found."); return null; }
            var avatar = topBar.transform.Find("AvatarChip") as RectTransform;
            if (!avatar) { Debug.LogError("❌ AvatarChip not found under TopBar."); return null; }
            return avatar;
        }

        private static RectTransform FindEpLike(Transform avatar)
        {
            // Prefer a child with TMP text starting with "Ep" or containing 'Ep' and small size,
            // else any child whose name contains "ep" (case-insensitive).
            RectTransform best = null;
            foreach (Transform c in avatar)
            {
                if (c.name.Equals("EpisodeChip")) continue;
                var tmp = c.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    var t = (tmp.text ?? "").Trim();
                    if (t.StartsWith("Ep") || t.StartsWith("EP") || t.Contains("Ep"))
                        return c as RectTransform;
                }
                if (c.name.ToLowerInvariant().Contains("ep"))
                    best = c as RectTransform;
            }
            return best;
        }

        private static void EnsureChipRect(RectTransform chip)
        {
            Undo.RecordObject(chip, "EpisodeChip rect normalize");
            chip.sizeDelta = new Vector2(64, 28);
            chip.pivot = new Vector2(1f, 0f);
            chip.anchorMin = new Vector2(1f, 0f);
            chip.anchorMax = new Vector2(1f, 0f);
            chip.anchoredPosition = new Vector2(-6, 6);
        }

        private static void DumpRect(RectTransform rt, string label)
        {
            var aMin = rt.anchorMin; var aMax = rt.anchorMax;
            var pos = rt.anchoredPosition; var size = rt.sizeDelta; var piv = rt.pivot;
            Debug.Log($"   • {label}: anchorMin=({aMin.x:F2},{aMin.y:F2}) anchorMax=({aMax.x:F2},{aMax.y:F2}) " +
                      $"pivot=({piv.x:F2},{piv.y:F2}) size=({size.x},{size.y}) pos=({pos.x},{pos.y})", rt);
        }
    }
}
#endif
