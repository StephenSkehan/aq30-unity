// Assets/Editor/AQ/Icons/MergeChainsTool.cs
// One-file tool: chain definitions, importer (forces Sprite settings), and preview window.
// Keeps everything in one assembly to avoid "type not found" across asmdefs.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AQ.Editor.MergeIcons
{
    // ---------------------------------------------------------------------
    //  Chain data + helpers
    // ---------------------------------------------------------------------
    public static class MergeChainIcons
    {
        public const string BasePath = "Assets/Art/UI/Icons/MergeChains";

        // Chain → ordered, expected file names for tiers T1..Tn
        public static readonly Dictionary<string, string[]> ExpectedFiles = new()
        {
            ["stakeout_fuel"] = new[]
            {
                "stakeout_fuel_t1_paper_cup.png",
                "stakeout_fuel_t2_hot_coffee_cup.png",
                "stakeout_fuel_t3_coffee_and_donut.png",
                "stakeout_fuel_t4_burger.png",
                "stakeout_fuel_t5_burger_fries_drink.png",
                "stakeout_fuel_t6_takeaway_feast_caddy.png",
            },
            ["fingerprint_evidence"] = new[]
            {
                "fingerprint_evidence_t1_partial_dusted_print.png",
                "fingerprint_evidence_t2_lifted_print_tape.png",
                "fingerprint_evidence_t3_fingerprint_card.png",
                "fingerprint_evidence_t4_labeled_prints.png",
                "fingerprint_evidence_t5_digital_scan_in_progress.png",
                "fingerprint_evidence_t6_database_match.png",
            },
            // Your current forensic tools set (5 tiers)
            ["forensic_tools"] = new[]
            {
                "forensic_tools_t1_cotton_swab.png",
                "forensic_tools_t2_evidence_bag.png",
                "forensic_tools_t3_full_forensic_case_black.png",
                "forensic_tools_t4_uv_light.png",
                "forensic_tools_t5_complete_forensic_kit_transparent_soft.png",
            },
        };

        public static IEnumerable<string> GetChains() => ExpectedFiles.Keys.OrderBy(k => k);
        public static string GetMasterFolder(string chainId) => $"{BasePath}/{chainId}/master";
        public static int GetTierCount(string chainId)
            => ExpectedFiles.TryGetValue(chainId, out var list) ? list.Length : 0;

        public static string GetTierPath(string chainId, int tierIndex0)
        {
            if (!ExpectedFiles.TryGetValue(chainId, out var files)) return null;
            if (tierIndex0 < 0 || tierIndex0 >= files.Length) return null;
            return $"{GetMasterFolder(chainId)}/{files[tierIndex0]}";
        }

        public static Sprite LoadTierSprite(string chainId, int tierIndex0)
        {
            var path = GetTierPath(chainId, tierIndex0);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static Texture2D LoadTierTexture(string chainId, int tierIndex0)
        {
            var path = GetTierPath(chainId, tierIndex0);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static string GetNiceTierLabel(int index0) => $"T{index0 + 1}";
    }

    // ---------------------------------------------------------------------
    //  Importer / fixer (forces Sprite import settings)
    // ---------------------------------------------------------------------
    public static class MergeIconsImporter
    {
        private const float DefaultPPU = 96f;

        [MenuItem("AQ/Icons/Import All (OneDrive)")]
        public static void ImportAll()
        {
            foreach (var chain in MergeChainIcons.GetChains())
            {
                var count = ForceSpriteImportForChain(chain);
                Debug.Log($"[Importer] Chain '{chain}' => {MergeChainIcons.GetTierCount(chain)} tier(s). Forced Sprite on {count} texture(s).");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("AQ/Icons/Fix Selected Chain (Force Sprite Type)")]
        public static void FixSelectedChain()
        {
            var chain = EditorPrefs.GetString("MergeChainsWindow.SelectedChain", string.Empty);
            if (string.IsNullOrEmpty(chain))
            {
                EditorUtility.DisplayDialog("Merge Icons", "No chain selected in Merge Chains window.", "OK");
                return;
            }

            var count = ForceSpriteImportForChain(chain);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Merge Icons", $"Done. Forced Sprite import on '{chain}' ({count} file(s)).", "OK");
        }

        public static int ForceSpriteImportForChain(string chainId)
        {
            int changed = 0;
            int tiers = MergeChainIcons.GetTierCount(chainId);
            for (int i = 0; i < tiers; i++)
            {
                var path = MergeChainIcons.GetTierPath(chainId, i);
                if (string.IsNullOrEmpty(path)) continue;

                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;

                bool dirty = false;

                if (imp.textureType != TextureImporterType.Sprite)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (imp.spriteImportMode != SpriteImportMode.Single)
                {
                    imp.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }
                if (imp.mipmapEnabled)
                {
                    imp.mipmapEnabled = false;
                    dirty = true;
                }
                if (!imp.alphaIsTransparency)
                {
                    imp.alphaIsTransparency = true;
                    dirty = true;
                }
                if (!imp.sRGBTexture)
                {
                    imp.sRGBTexture = true;
                    dirty = true;
                }
                if (imp.spritePixelsPerUnit != DefaultPPU)
                {
                    imp.spritePixelsPerUnit = DefaultPPU;
                    dirty = true;
                }

                if (dirty)
                {
                    imp.SaveAndReimport();
                    changed++;
                }
            }
            return changed;
        }
    }

    // ---------------------------------------------------------------------
    //  Preview window
    // ---------------------------------------------------------------------
    public class MergeChainsWindow : EditorWindow
    {
        private Vector2 _chainsScroll;
        private Vector2 _previewScroll;
        private string _search = "";
        private string _selectedChain;
        private int _previewPixels = 96;

        private static readonly int[] PreviewSizes = { 48, 64, 96, 128, 192, 256 };

        [MenuItem("AQ/Icons/Merge Chains")]
        public static void Open()
        {
            var w = GetWindow<MergeChainsWindow>("Merge Chains");
            w.minSize = new Vector2(780, 420);
            w.Show();
        }

        private void OnEnable()
        {
            _selectedChain = MergeChainIcons.GetChains().FirstOrDefault();
            EditorPrefs.SetString("MergeChainsWindow.SelectedChain", _selectedChain ?? "");
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawChainList();
            DrawChainPreview();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Merge Chains", GUILayout.Width(90));

                GUILayout.Space(8);
                GUILayout.Label("Search", GUILayout.Width(42));
                _search = GUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.MinWidth(120));

                GUILayout.FlexibleSpace();

                GUILayout.Label("Preview", GUILayout.Width(50));
                _previewPixels = EditorGUILayout.IntPopup(_previewPixels, PreviewSizes.Select(v => v.ToString()).ToArray(), PreviewSizes, GUILayout.Width(70));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    Repaint();

                if (GUILayout.Button("Re-import (OneDrive)", EditorStyles.toolbarButton, GUILayout.Width(130)))
                {
                    MergeIconsImporter.ImportAll();
                    Repaint();
                }

                if (GUILayout.Button("Fix Selected Chain", EditorStyles.toolbarButton, GUILayout.Width(130)))
                {
                    MergeIconsImporter.FixSelectedChain();
                    Repaint();
                }
            }
        }

        private void DrawChainList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(260)))
            {
                _chainsScroll = EditorGUILayout.BeginScrollView(_chainsScroll, GUILayout.ExpandHeight(true));

                foreach (var chain in MergeChainIcons.GetChains())
                {
                    if (!string.IsNullOrEmpty(_search) && !chain.ToLowerInvariant().Contains(_search.ToLowerInvariant()))
                        continue;

                    var tiers = MergeChainIcons.GetTierCount(chain);
                    var label = $"{chain}   ({tiers} tier{(tiers == 1 ? "" : "s")})";
                    var selected = chain == _selectedChain;

                    var style = new GUIStyle(EditorStyles.label);
                    if (selected) style.fontStyle = FontStyle.Bold;

                    var rect = GUILayoutUtility.GetRect(new GUIContent(label), style);
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        _selectedChain = chain;
                        EditorPrefs.SetString("MergeChainsWindow.SelectedChain", _selectedChain);
                        Repaint();
                    }
                    GUI.Label(rect, label, style);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawChainPreview()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Chain:", GUILayout.Width(42));
                    GUILayout.Label(string.IsNullOrEmpty(_selectedChain) ? "(none)" : _selectedChain, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    if (!string.IsNullOrEmpty(_selectedChain))
                    {
                        if (GUILayout.Button("Ping Asset", GUILayout.Width(80)))
                        {
                            var path = MergeChainIcons.GetMasterFolder(_selectedChain);
                            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                            if (obj != null) EditorGUIUtility.PingObject(obj);
                        }
                        if (GUILayout.Button("Reveal", GUILayout.Width(60)))
                        {
                            var path = MergeChainIcons.GetMasterFolder(_selectedChain);
                            EditorUtility.RevealInFinder(path);
                        }
                    }
                }

                EditorGUILayout.Space(4);
                var tiers = string.IsNullOrEmpty(_selectedChain) ? 0 : MergeChainIcons.GetTierCount(_selectedChain);
                if (tiers <= 0)
                {
                    EditorGUILayout.HelpBox("Select a chain to preview.", MessageType.Info);
                    return;
                }

                _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll);

                using (new EditorGUILayout.VerticalScope())
                {
                    const int columns = 4;
                    int col = 0;

                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < tiers; i++)
                    {
                        DrawTile(_selectedChain, i);

                        col++;
                        if (col >= columns)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            col = 0;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(6);
                EditorGUILayout.HelpBox("Preview shows true pixel sizes. Switch to 48 to sanity-check UI legibility.", MessageType.None);

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawTile(string chainId, int tierIndex0)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(_previewPixels + 24)))
            {
                var sprite = MergeChainIcons.LoadTierSprite(chainId, tierIndex0);
                var preview = (Texture)(sprite ? AssetPreview.GetAssetPreview(sprite) : null);

                // Fallback to Texture2D if sprite is missing (wrong import type)
                if (preview == null)
                {
                    var tex = MergeChainIcons.LoadTierTexture(chainId, tierIndex0);
                    if (tex) preview = AssetPreview.GetAssetPreview(tex);
                }

                var r = GUILayoutUtility.GetRect(_previewPixels, _previewPixels, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.15f, 0.15f)); // subtle backdrop

                if (preview != null)
                    GUI.DrawTexture(r, preview, ScaleMode.ScaleToFit, true);
                else
                    EditorGUI.LabelField(r, "(missing)", EditorStyles.centeredGreyMiniLabel);

                EditorGUILayout.Space(2);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(42)))
                    {
                        var path = MergeChainIcons.GetTierPath(chainId, tierIndex0);
                        var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (obj) EditorGUIUtility.PingObject(obj);
                    }
                    if (GUILayout.Button("Copy Name", GUILayout.Width(80)))
                    {
                        var path = MergeChainIcons.GetTierPath(chainId, tierIndex0);
                        EditorGUIUtility.systemCopyBuffer = System.IO.Path.GetFileName(path);
                    }
                }

                GUILayout.Label(MergeChainIcons.GetNiceTierLabel(tierIndex0), EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            }
        }
    }
}
#endif
