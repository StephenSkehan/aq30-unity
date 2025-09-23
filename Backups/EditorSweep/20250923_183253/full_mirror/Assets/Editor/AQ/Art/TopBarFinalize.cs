// Assets/Editor/AQ/Art/TopBarFinalize.cs
// Menu:
//   AQ → Art → TopBar → Audit (Deep)
//   AQ → Art → TopBar → Purge + Finalize

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class TopBarFinalize
    {
        private const string PATH_TOPBAR = "Canvas_Board/HUD_Board/TopBar";

        [MenuItem("AQ/Art/TopBar/Audit (Deep)")]
        public static void AuditMenu()
        {
            var top = Find(PATH_TOPBAR);
            if (!top) { Debug.LogWarning("[AQ Art] TopBar not found."); return; }
            Debug.Log(DumpTree(top));
        }

        [MenuItem("AQ/Art/TopBar/Purge + Finalize")]
        public static void PurgeAndFinalize()
        {
            var top = Find(PATH_TOPBAR);
            if (!top) { Debug.LogWarning("[AQ Art] TopBar not found."); return; }

            Debug.Log(Snapshot(top, "BEFORE"));

            // Normalize TopBar rect + HLG
            NormalizeTopBar(top);

            // 1) Purge only *legacy* containers (exact name match) – DO NOT touch Spacer_Flex
            PurgeLegacy(top);

            // 2) Ensure core pieces exist / sized
            var home   = EnsureHome(top);
            var avatar = EnsureAvatarWithEpisode(top);
            var spacer = EnsureFlexibleSpacer(top);

            var mEnergy  = EnsureMeter(top, "Meter_Energy",  64, 160, 42, 26);
            var mSoft    = EnsureMeter(top, "Meter_Soft",    64, 160, 42,  0);
            var mPremium = EnsureMeter(top, "Meter_Premium", 64, 160, 42,  0);

            // 3) Order: Home, Avatar, Spacer_Flex, Energy, Soft, Premium
            var order = new List<Transform> { home, avatar, spacer, mEnergy, mSoft, mPremium };
            int idx = 0; foreach (var t in order.Where(t => t != null)) t.SetSiblingIndex(idx++);

            // 4) Rebuild now and next frame (handles “layout dirt”)
            Rebuild(top);
            EditorApplication.delayCall += () => { if (top) Rebuild(top); };

            Debug.Log(Snapshot(top, "AFTER"));
            Debug.Log("[AQ Art] TopBar finalized. If anything still looks off, Play→Stop then run once more.");
        }

        // ---------- TopBar normalization ----------
        private static void NormalizeTopBar(Transform top)
        {
            var rtTop = top.GetComponent<RectTransform>();
            float canvasW = rtTop.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
            if (canvasW <= 0) canvasW = 1080;
            rtTop.anchorMin = new Vector2(0, 1);
            rtTop.anchorMax = new Vector2(0, 1);
            rtTop.pivot     = new Vector2(0.5f, 1);
            rtTop.sizeDelta = new Vector2(Mathf.Round(canvasW), 176);
            rtTop.anchoredPosition = new Vector2(canvasW * 0.5f, 0);

            var hlg = top.GetComponent<HorizontalLayoutGroup>() ?? top.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(24, 24, 12, 12);
            hlg.spacing = 24;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
        }

        // ---------- PURGE ----------
        // Only remove *exact* legacy nodes; never touch Spacer_Flex
        private static readonly HashSet<string> LegacyExact = new HashSet<string>
        {
            "Spacer", "Meters", "Legacy", "Old"
        };

        private static void PurgeLegacy(Transform top)
        {
            var toDisable = new List<Transform>();

            foreach (Transform child in top)
            {
                string n = child.name;
                if (LegacyExact.Contains(n)) toDisable.Add(child);
                // Also remove any direct children of a legacy group named above
                if (LegacyExact.Contains(child.parent?.name ?? string.Empty))
                    toDisable.Add(child);
            }

            foreach (var t in toDisable.Distinct())
            {
                if (t == null) continue;
                t.gameObject.SetActive(false);

                foreach (var g in t.GetComponentsInChildren<Graphic>(true)) g.enabled = false;
                foreach (var cr in t.GetComponentsInChildren<CanvasRenderer>(true)) cr.SetAlpha(0f);

                var le = t.GetComponent<LayoutElement>();
                if (le)
                {
                    le.ignoreLayout = true;
                    le.preferredWidth = 0; le.preferredHeight = 0;
                    le.minWidth = 0; le.minHeight = 0;
                    le.flexibleWidth = 0; le.flexibleHeight = 0;
                }
            }
        }

        // ---------- Core builders ----------
        private static Transform EnsureHome(Transform top)
        {
            var home = FindOrCreate(top, "Btn_Home"); Size(home, 96, 96);
            var imgBack = FindOrCreate(home, "Img_Back");
            var back = Ensure<Image>(imgBack); back.type = Image.Type.Sliced; Center(imgBack); Size(imgBack, 96, 96);

            var imgIcon = FindOrCreate(home, "Img_Icon");
            var icon = Ensure<Image>(imgIcon); icon.preserveAspect = true; Center(imgIcon); Size(imgIcon, 72, 72);
            EnsureLE(home, 96, 96, 96);
            return home;
        }

        private static Transform EnsureAvatarWithEpisode(Transform top)
        {
            var avatar = FindOrCreate(top, "AvatarChip");
            EnsureLE(avatar, 128, 128, 128); Size(avatar, 128, 128);

            var imgAvatar = FindOrCreate(avatar, "Img_Avatar");
            var aImg = Ensure<Image>(imgAvatar); aImg.preserveAspect = true; Center(imgAvatar); Size(imgAvatar, 128, 128);

            var imgFrame = FindOrCreate(avatar, "Img_Frame");
            var fImg = Ensure<Image>(imgFrame); fImg.type = Image.Type.Sliced; Center(imgFrame); Size(imgFrame, 128, 128);

            var epi = FindOrCreate(avatar, "EpisodeChip");
            var rtEpi = epi.GetComponent<RectTransform>();
            rtEpi.anchorMin = new Vector2(1, 0); rtEpi.anchorMax = new Vector2(1, 0);
            rtEpi.pivot     = new Vector2(1, 0);
            rtEpi.anchoredPosition = new Vector2(-6, 6);
            Size(epi, 136, 56);

            var epiBG = FindOrCreate(epi, "Img_BG");
            var epiBGImg = Ensure<Image>(epiBG); epiBGImg.type = Image.Type.Sliced; Center(epiBG); MatchSize(epiBG, epi);

            var epiTxt = FindOrCreate(epi, "Txt_Episode");
            Center(epiTxt); MatchSize(epiTxt, epi);
            var epiLabel = Ensure<TMP_Text>(epiTxt);
            if (string.IsNullOrWhiteSpace(epiLabel.text)) epiLabel.text = "Ep1-1";
            epiLabel.fontSize = 34;
            epiLabel.alignment = TextAlignmentOptions.Center;
            epiLabel.color = new Color32(28, 82, 88, 255);

            var ol = epiTxt.GetComponent<Outline>() ?? epiTxt.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0, 0, 0, 0.25f);
            ol.effectDistance = new Vector2(1.2f, -1.2f);

            epi.SetAsLastSibling();
            return avatar;
        }

        private static Transform EnsureFlexibleSpacer(Transform top)
        {
            var spacer = Find(top, "Spacer_Flex") ?? new GameObject("Spacer_Flex", typeof(RectTransform)).transform;
            if (spacer.parent != top) spacer.SetParent(top, false);
            var le = Ensure<LayoutElement>(spacer);
            le.flexibleWidth = 999f; le.minWidth = 0; le.preferredWidth = -1;
            Center(spacer); Size(spacer, 100, 100);
            return spacer;
        }

        private static Transform EnsureMeter(Transform top, string name, int iconSize, int blockWidth, int valueSize, int timerSize)
        {
            var meter = FindOrCreate(top, name);
            Size(meter, 100, 100);

            var hlg = meter.GetComponent<HorizontalLayoutGroup>() ?? meter.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(0,0,0,0); hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            var iconT = FindOrCreate(meter, "Img_Icon");
            Center(iconT); Size(iconT, iconSize, iconSize);
            var icon = Ensure<Image>(iconT); icon.preserveAspect = true;
            var leIcon = Ensure<LayoutElement>(iconT);
            leIcon.preferredWidth = iconSize; leIcon.preferredHeight = iconSize;

            var block = FindOrCreate(meter, "Txt_Block");
            var vlg = block.GetComponent<VerticalLayoutGroup>() ?? block.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0,0,0,0); vlg.spacing = 0;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            var leBlock = Ensure<LayoutElement>(block);
            leBlock.preferredWidth = blockWidth; leBlock.preferredHeight = 96;
            Center(block); Size(block, blockWidth, 96);

            var val = FindOrCreate(block, "Txt_Value");
            Center(val); Size(val, blockWidth, 50);
            var txtVal = Ensure<TMP_Text>(val);
            txtVal.enableAutoSizing = false; txtVal.fontSize = valueSize; txtVal.alignment = TextAlignmentOptions.Center;

            var tim = Find(block, "Txt_Timer");
            if (timerSize > 0)
            {
                tim = tim ?? FindOrCreate(block, "Txt_Timer");
                Center(tim); Size(tim, blockWidth, 40);
                var txtTim = Ensure<TMP_Text>(tim);
                txtTim.enableAutoSizing = false; txtTim.fontSize = timerSize; txtTim.alignment = TextAlignmentOptions.Center;
            }
            else if (tim) Object.DestroyImmediate(tim.gameObject);

            return meter;
        }

        // ---------- Utility ----------
        private static Transform Find(string path) { var go = GameObject.Find(path); return go ? go.transform : null; }
        private static Transform Find(Transform root, string name)
        {
            if (!root) return null;
            var t = root.Find(name); if (t) return t;
            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
        }
        private static Transform FindOrCreate(Transform parent, string name)
        {
            var t = Find(parent, name);
            if (t) return t;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Center(go.transform);
            return go.transform;
        }
        private static void Center(Transform t)
        {
            var rt = t.GetComponent<RectTransform>(); if (!rt) return;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
        private static void Size(Transform t, float w, float h)
        {
            var rt = t.GetComponent<RectTransform>(); if (!rt) return;
            rt.sizeDelta = new Vector2(w, h);
        }
        private static void MatchSize(Transform a, Transform b)
        {
            var ra = a.GetComponent<RectTransform>(); var rb = b.GetComponent<RectTransform>();
            if (ra && rb) ra.sizeDelta = rb.sizeDelta;
        }
        private static T Ensure<T>(Transform t) where T : Component
        {
            var c = t.GetComponent<T>();
            return c ? c : t.gameObject.AddComponent<T>();
        }
        private static void EnsureLE(Transform t, float prefW, float prefH, float minH)
        {
            var le = Ensure<LayoutElement>(t);
            le.preferredWidth = prefW; le.preferredHeight = prefH; le.minHeight = minH;
        }
        private static void Rebuild(Transform top)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(top.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
        }

        // ---------- Diagnostics ----------
        private static string Snapshot(Transform topBar, string tag)
        {
            var rt = topBar.GetComponent<RectTransform>();
            var hlg = topBar.GetComponent<HorizontalLayoutGroup>();
            var lines = new List<string>
            {
                $"[AQ Art] --- TopBar Snapshot ({tag}) ---",
                $"TopBar: size=({rt.sizeDelta.x:0},{rt.sizeDelta.y:0}) pad(L{hlg.padding.left}/R{hlg.padding.right}) spacing={hlg.spacing}"
            };
            foreach (Transform c in topBar)
            {
                if (!c.gameObject.activeInHierarchy) continue;
                var r = c.GetComponent<RectTransform>();
                lines.Add($"  {c.name} size=({(r? r.sizeDelta.x:0):0},{(r? r.sizeDelta.y:0):0})");
                foreach (Transform cc in c)
                {
                    if (!cc.gameObject.activeInHierarchy) continue;
                    var rr = cc.GetComponent<RectTransform>();
                    lines.Add($"    • {cc.name} size=({(rr? rr.sizeDelta.x:0):0},{(rr? rr.sizeDelta.y:0):0})");
                }
            }
            return string.Join("\n", lines);
        }

        private static string DumpTree(Transform root)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[AQ Art] --- Deep Tree Audit ---");
            void Recurse(Transform t, int d)
            {
                var r = t.GetComponent<RectTransform>();
                sb.AppendLine($"{new string(' ', d*2)}- {t.name} size=({(r? r.sizeDelta.x:0):0},{(r? r.sizeDelta.y:0):0})");
                for (int i = 0; i < t.childCount; i++) Recurse(t.GetChild(i), d + 1);
            }
            Recurse(root, 0);
            return sb.ToString();
        }
    }
}
