// SPDX-License-Identifier: MIT
// File: Assets/Editor/UI/Board/PopulateBoardWithIcons.cs
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.UI.Board
{
    public static class PopulateBoardWithIcons
    {
        [MenuItem("AQ/UI/Board/Populate/Stakeout (Icons/MergeChains/stakeout_fuel/master)")]
        public static void PopulateStakeout()
            => PopulateFromFolder("Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master");

        [MenuItem("AQ/UI/Board/Populate/From Folder…")]
        public static void PopulateFromFolderMenu()
        {
            var abs = EditorUtility.OpenFolderPanel("Pick a sprite folder under Assets", Application.dataPath, "");
            if (string.IsNullOrEmpty(abs)) return;

            if (!abs.Replace('\\', '/').StartsWith(Application.dataPath.Replace('\\', '/')))
            {
                EditorUtility.DisplayDialog("Invalid Folder",
                    "Please choose a folder inside your project’s Assets directory.", "OK");
                return;
            }

            var rel = "Assets" + abs.Substring(Application.dataPath.Length);
            PopulateFromFolder(rel);
        }

        public static void PopulateFromFolder(string assetsRelativeFolder)
        {
            // Find all candidate grids in scene and pick the preferred one deterministically.
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            var grids = UnityEngine.Object.FindObjectsByType<GridLayoutGroup>(FindObjectsSortMode.None);
#else
            var grids = UnityEngine.Object.FindObjectsOfType<GridLayoutGroup>(true);
#endif
            if (grids == null || grids.Length == 0)
            {
                Debug.LogWarning("[AQ] Populate: No GridLayoutGroup found in open scenes.");
                return;
            }

            var grid = DiagnoseMergeBoard.PickPreferredGrid(grids);
            if (grid == null)
            {
                Debug.LogWarning("[AQ] Populate: could not choose a target grid.");
                return;
            }

            var boardRoot = grid.transform;
            if (boardRoot.childCount == 0)
            {
                // Rich diagnostics: list all grids/child counts to show what we saw.
                var details = string.Join("\n", grids.Select(g =>
                    $" - {g.name}  active:{g.gameObject.activeInHierarchy}  children:{g.transform.childCount}  path:{GetHierarchyPath(g.transform)}"));
                Debug.LogWarning($"[AQ] Populate: The chosen GridLayoutGroup has no child slots.\nGrids seen:\n{details}");
                return;
            }

            // Load sprites from folder
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { assetsRelativeFolder });
            var sprites = guids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                .Where(s => s != null)
                .OrderBy(s => s.name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[AQ] Populate: No sprites found in {assetsRelativeFolder}");
                return;
            }

            int applied = 0;
            for (int i = 0; i < boardRoot.childCount; i++)
            {
                var slot = boardRoot.GetChild(i);
                var icon = EnsureIconImage(slot);

                var sprite = sprites[i % sprites.Count];
                icon.sprite = sprite;
                icon.enabled = true;
                icon.preserveAspect = true;
                icon.color = Color.white;

                var rt = icon.rectTransform;
                rt.anchorMin = new Vector2(0.1f, 0.1f);
                rt.anchorMax = new Vector2(0.9f, 0.9f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.SetAsLastSibling();

                applied++;
            }

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[AQ] Populated {applied} slots with {sprites.Count} sprites from: {assetsRelativeFolder}");
        }

        private static Image EnsureIconImage(Transform slot)
        {
            var t = slot.Find("Icon");
            if (t == null)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(slot, false);
                t = go.transform;
            }

            var img = t.GetComponent<Image>();
            if (img == null) img = t.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        private static string GetHierarchyPath(Transform t)
        {
            System.Collections.Generic.List<string> parts = new();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
#endif
