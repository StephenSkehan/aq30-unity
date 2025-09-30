#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class LeadsFixAll
    {
        // ----------------- PUBLIC MENUS -----------------

        [MenuItem("AQ/UI/Leads/1) Purge Missing Scripts (Scene)")]
        public static void PurgeMissingScriptsInScene()
        {
            int removed = 0;
            foreach (var go in GetAllSceneGameObjects())
                removed += RemoveMissingOn(go);

            Debug.Log($"[AQ Fix] Purged Missing (Mono Script) components: {removed} removed.");
            MarkDirty();
        }

        [MenuItem("AQ/UI/Leads/2) Repair Tier Set Popup in Scene")]
        public static void RepairTierSetPopupInScene()
        {
            // Prefer HUD_Board; fallback to first root
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var hud = GameObject.Find("HUD_Board")?.transform ?? (roots.Length > 0 ? roots[0].transform : null);
            if (!hud) { Debug.LogWarning("[AQ Fix] No suitable parent; open HUD_Board scene."); return; }

            var popupT = GameObject.Find("TierSetPopup")?.transform;
            if (!popupT)
            {
                var go = new GameObject("TierSetPopup", typeof(RectTransform), typeof(CanvasGroup));
                popupT = go.transform;
                popupT.SetParent(hud, false);
                var rt = (RectTransform)popupT;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(520, 280);
            }

            // Clean any dead scripts first.
            RemoveMissingOn(popupT.gameObject);

            // Ensure expected children (readable defaults)
            var cg = popupT.GetComponent<CanvasGroup>();
            cg.alpha = 0; cg.blocksRaycasts = false; cg.interactable = false; // hidden by default

            var title = EnsureTMP_UI(popupT, "Title",
                aMin:new Vector2(0.5f, 1f), aMax:new Vector2(0.5f, 1f), pivot:new Vector2(0.5f, 1f),
                anchoredPos:new Vector2(0, -24), size:new Vector2(480, 48));
            title.text = "STAKEOUT FUEL";
            title.fontSize = 34;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.overflowMode = TextOverflowModes.Truncate;
            title.alignment = TextAlignmentOptions.Midline;

            var grid = EnsureChild(popupT, "IconGrid");
            var gridRT = grid.GetComponent<RectTransform>() ?? grid.gameObject.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.5f, 0.5f);
            gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.pivot     = new Vector2(0.5f, 0.5f);
            gridRT.sizeDelta = new Vector2(480, 120);
            gridRT.anchoredPosition = new Vector2(0, 10);

            var layout = grid.GetComponent<GridLayoutGroup>() ?? grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(64, 64);
            layout.spacing  = new Vector2(12, 12);
            layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            layout.constraintCount = 1;

            // 6 icon children
            for (int i = 0; i < 6; i++)
            {
                var c = EnsureChild(grid, $"Icon{i+1}");
                var img = c.GetComponent<Image>() ?? c.gameObject.AddComponent<Image>();
                img.raycastTarget = false;
                var crt = c.GetComponent<RectTransform>() ?? c.gameObject.AddComponent<RectTransform>();
                crt.sizeDelta = new Vector2(64, 64);
            }

            // Highlight
            var highlight = EnsureChild(grid, "Highlight");
            var hImg = highlight.GetComponent<Image>() ?? highlight.gameObject.AddComponent<Image>();
            hImg.raycastTarget = false;
            hImg.enabled = false; // presenter shows/moves it
            var hrt = highlight.GetComponent<RectTransform>() ?? highlight.gameObject.AddComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(72, 72);

            // Close button (top-right)
            var close = EnsureChild(popupT, "Btn_Close");
            var closeBtn = close.GetComponent<Button>() ?? close.gameObject.AddComponent<Button>();
            var closeImg = close.GetComponent<Image>() ?? close.gameObject.AddComponent<Image>();
            closeImg.raycastTarget = true;
            var crt2 = close.GetComponent<RectTransform>() ?? close.gameObject.AddComponent<RectTransform>();
            crt2.anchorMin = new Vector2(1, 1); crt2.anchorMax = new Vector2(1, 1); crt2.pivot = new Vector2(1, 1);
            crt2.sizeDelta = new Vector2(32, 32); crt2.anchoredPosition = new Vector2(-8, -8);

            // Attach + wire TierSetPopupPresenter IF it exists (no compile-time dependency)
            var presenter = EnsureComponentByName(popupT.gameObject, "AQ.App.UI.Leads.TierSetPopupPresenter", "TierSetPopupPresenter");
            if (presenter != null)
            {
                SetPrivate(presenter, "cg", cg);
                SetPrivate(presenter, "titleText", title);
                SetPrivate(presenter, "iconGrid", grid);
                SetPrivate(presenter, "highlight", hImg);
                SetPrivate(presenter, "closeButton", closeBtn);
                Debug.Log("[AQ Fix] TierSetPopup wired to TierSetPopupPresenter.");
            }
            else
            {
                Debug.LogWarning("[AQ Fix] TierSetPopupPresenter type not found. Popup created but not wired (safe to proceed; add presenter later).");
            }

            MarkDirty();
        }

        [MenuItem("AQ/UI/Leads/3) Repair Lead Cards (Scene)")]
        public static void RepairLeadCardsInScene()
        {
            var contentGO = GameObject.Find("Content_Leads");
            if (!contentGO) { Debug.LogWarning("[AQ Fix] Content_Leads not found."); return; }
            var content = contentGO.transform;

            int cards = 0, tmpFixed = 0, actors = 0, slotsOk = 0, removedMissing = 0;

            foreach (Transform child in content)
            {
                // Tolerant: treat any immediate child that looks like a card
                bool hasTitle = child.Find("Text_Title");
                bool hasReqRow = child.Find("RequirementsRow");
                if (!hasTitle && !hasReqRow) continue;

                cards++;

                // Remove dead scripts on card subtree
                removedMissing += RemoveMissingOn(child.gameObject);

                // Ensure LeadCardPresenter exists (reflection; no compile-time dep)
                var presenter = EnsureComponentByName(child.gameObject,
                    "AQ.App.UI.Leads.LeadCardPresenter", "LeadCardPresenter");

                // TMP: Truncate + NoWrap + raycast off
                foreach (var tmp in child.GetComponentsInChildren<TMP_Text>(true))
                {
                    tmp.textWrappingMode = TextWrappingModes.NoWrap;
                    tmp.overflowMode = TextOverflowModes.Truncate;
                    tmp.raycastTarget = false;
                    tmpFixed++;
                }

                // Actor alpha = 1 (visible)
                var actor = child.Find("ActorAnchor/Image")?.GetComponent<Image>();
                if (actor)
                {
                    var c = actor.color; c.a = 1f; actor.color = c;
                    actors++;
                }

                // Requirements slots
                var row = child.Find("RequirementsRow");
                if (row)
                {
                    var views = new List<Component>(3);

                    for (int i = 1; i <= 3; i++)
                    {
                        var slot = row.Find($"Req_{i}") ?? CreateSlot(row, $"Req_{i}");
                        // Clean any dead scripts on slot only (again)
                        removedMissing += RemoveMissingOn(slot.gameObject);

                        var btn  = slot.GetComponent<Button>() ?? slot.gameObject.AddComponent<Button>();
                        var view = EnsureComponentByName(slot.gameObject,
                            "AQ.App.UI.Leads.RequirementSlotView", "RequirementSlotView");

                        // Ensure Icon + Tick children exist
                        var icon = slot.Find("Icon")?.GetComponent<Image>()
                                   ?? CreateImage(slot, "Icon", new Vector2(40, 40),
                                        new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0, 0));
                        var tick = slot.Find("Tick")?.GetComponent<Image>()
                                   ?? CreateImage(slot, "Tick", new Vector2(20, 20),
                                        new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero);
                        tick.enabled = false;

                        if (view != null)
                        {
                            SetPrivate(view, "icon", icon);
                            SetPrivate(view, "tickBadge", tick);
                            SetPrivate(view, "button", btn);
                        }

                        views.Add(view);
                        slotsOk++;
                    }

                    // Hook slots array into presenter (private field via reflection)
                    if (presenter != null)
                    {
                        var filtered = views.Where(v => v != null).ToArray();
                        SetPrivate(presenter, "slots", filtered);
                    }
                }
            }

            Debug.Log($"[AQ Fix] Cards:{cards}, TMP fixed:{tmpFixed}, Actors visible:{actors}, Slots repaired:{slotsOk}, Missing removed:{removedMissing}.");
            MarkDirty();
        }

        [MenuItem("AQ/UI/Leads/Repair LeadCard Prefab Asset(s)")]
        public static void RepairLeadCardPrefabs()
        {
            var guids = AssetDatabase.FindAssets("LeadCard t:prefab");
            int repaired = 0, removedMissing = 0, slots = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    removedMissing += RemoveMissingOn(root);

                    var card = root.transform;

                    var presenter = EnsureComponentByName(card.gameObject,
                        "AQ.App.UI.Leads.LeadCardPresenter", "LeadCardPresenter");

                    // Ensure RequirementsRow + Req_1/2/3
                    var row = card.Find("RequirementsRow") ?? new GameObject("RequirementsRow", typeof(RectTransform)).transform;
                    row.SetParent(card, false);

                    for (int i = 1; i <= 3; i++)
                    {
                        var slot = row.Find($"Req_{i}") ?? CreateSlot(row, $"Req_{i}");
                        var btn  = slot.GetComponent<Button>() ?? slot.gameObject.AddComponent<Button>();
                        var view = EnsureComponentByName(slot.gameObject,
                            "AQ.App.UI.Leads.RequirementSlotView", "RequirementSlotView");
                        var icon = slot.Find("Icon")?.GetComponent<Image>()
                                   ?? CreateImage(slot, "Icon", new Vector2(40, 40),
                                        new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0, 0));
                        var tick = slot.Find("Tick")?.GetComponent<Image>()
                                   ?? CreateImage(slot, "Tick", new Vector2(20, 20),
                                        new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero);
                        tick.enabled = false;

                        if (view != null)
                        {
                            SetPrivate(view, "icon", icon);
                            SetPrivate(view, "tickBadge", tick);
                            SetPrivate(view, "button", btn);
                        }
                        slots++;
                    }

                    if (presenter != null)
                    {
                        var views = row.GetComponentsInChildren<Transform>(true)
                                       .Where(t => t.parent == row && t.name.StartsWith("Req_"))
                                       .Select(t => t.GetComponent<Component>())
                                       .Where(c => c != null && c.GetType().Name.EndsWith("RequirementSlotView"))
                                       .ToArray();
                        SetPrivate(presenter, "slots", views);
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    repaired++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }

            Debug.Log($"[AQ Fix] Prefabs repaired:{repaired}, Missing removed:{removedMissing}, Slots ensured:{slots}.");
        }

        // ----------------- HELPERS -----------------

        private static IEnumerable<GameObject> GetAllSceneGameObjects()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var r in roots)
                foreach (var t in r.GetComponentsInChildren<Transform>(true))
                    yield return t.gameObject;
        }

        private static int RemoveMissingOn(GameObject go)
        {
            int before = CountMissing(go);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            foreach (Transform t in go.transform)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            int after = CountMissing(go);
            return before - after;
        }

        private static int CountMissing(GameObject go)
        {
            int cnt = 0;
            foreach (var c in go.GetComponents<Component>()) if (!c) cnt++;
            foreach (Transform t in go.transform) foreach (var c in t.GetComponents<Component>()) if (!c) cnt++;
            return cnt;
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t) return t;
            var go = new GameObject(name);
            t = go.transform; t.SetParent(parent, false);
            return t;
        }

        /// <summary>
        /// Ensure a UI (RectTransform + TextMeshProUGUI) object exists.
        /// If a non-UI "Title" already exists (Transform without RectTransform), it is renamed to "Title (legacy)" and a new UI child "Title" is created.
        /// </summary>
        private static TMP_Text EnsureTMP_UI(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var existing = parent.Find(name);
            Transform holder = existing;

            // If existing child is not a RectTransform, preserve it and create a fresh UI object
            if (holder != null && !(holder is RectTransform))
            {
                holder.gameObject.name = name + " (legacy)";
                holder = null;
            }

            if (holder == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                holder = go.transform;
                holder.SetParent(parent, false);
            }

            var rt = holder as RectTransform;
            if (rt == null)
            {
                // Replace with a UI object safely
                var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
                holder = go.transform;
                rt = (RectTransform)holder;
            }

            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;

            var tmp = holder.GetComponent<TMP_Text>() ?? holder.gameObject.AddComponent<TextMeshProUGUI>();
            return tmp;
        }

        private static Transform CreateSlot(Transform row, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var t = go.transform; t.SetParent(row, false);
            var rt = (RectTransform)t;
            rt.sizeDelta = new Vector2(100, 48);
            return t;
        }

        private static Image CreateImage(Transform parent, string name, Vector2 size,
            Vector2 aMin, Vector2 aMax, Vector2 anchoredPos)
        {
            var t = EnsureChild(parent, name);
            var rt = t as RectTransform ?? t.gameObject.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;
            var img = t.GetComponent<Image>() ?? t.gameObject.AddComponent<Image>();
            return img;
        }

        private static void SetPrivate(object obj, string field, object value)
        {
            if (obj == null) return;
            var f = obj.GetType().GetField(field,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(obj, value);
        }

        private static Component EnsureComponentByName(GameObject go, params string[] typeNames)
        {
            var type = FindTypeByNames(typeNames);
            if (type == null) return null;
            return go.GetComponent(type) ?? go.AddComponent(type);
        }

        private static Type FindTypeByNames(params string[] typeNames)
        {
            foreach (var name in typeNames)
            {
                var t = Type.GetType(name);
                if (t != null) return t;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(name);
                    if (t != null) return t;
                    try
                    {
                        t = asm.GetTypes().FirstOrDefault(x => x.Name == name);
                        if (t != null) return t;
                    }
                    catch { /* dynamic assemblies can throw */ }
                }
            }
            return null;
        }

        private static void MarkDirty()
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
#endif
