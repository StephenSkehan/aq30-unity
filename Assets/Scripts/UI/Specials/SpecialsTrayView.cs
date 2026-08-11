using AQ.App.Leads;
using AQ.App.Locker;
using AQ.App.Overflow;
using AQ.App.UI.Board;
using AQ.App.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Specials
{
    /// <summary>
    /// The Case Kit corner button (beside the Stash) + kit popup + targeting
    /// overlay. Button input is a raw Update() poll (boot-created canvas — the
    /// GR-unreliable class); the popup and overlay are created on demand, where
    /// standard Buttons are fine (GeneratorInfoPopup precedent).
    /// </summary>
    public class SpecialsTrayView : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("[SpecialsTrayView]");
            DontDestroyOnLoad(go);
            go.AddComponent<SpecialsTrayView>();
        }

        private RectTransform _root;
        private Text _badge;
        private const float SIZE = 142f; // grid-square parity with locker/stash/evidence

        private static GameObject _popup;
        private static GameObject _overlay;

        private void Awake()
        {
            try { BuildHUD(); }
            catch (System.Exception e) { Debug.LogError($"[SpecialsTray] BuildHUD failed: {e}"); }
        }

        private void OnEnable()
        {
            SpecialItemsService.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => SpecialItemsService.Changed -= Refresh;

        private void Update()
        {
            if (_root == null || !_root.gameObject.activeSelf || _popup != null || _overlay != null) return;

            bool tapped = false;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                tapped = RectContains(Input.GetTouch(0).position);
            else if (Input.GetMouseButtonDown(0))
                tapped = RectContains(Input.mousePosition);

            if (tapped) ShowKit();
        }

        private bool RectContains(Vector2 screenPos)
        {
            var corners = new Vector3[4];
            _root.GetWorldCorners(corners);
            return screenPos.x >= corners[0].x && screenPos.x <= corners[2].x &&
                   screenPos.y >= corners[0].y && screenPos.y <= corners[2].y;
        }

        private void BuildHUD()
        {
            var canvasGO = new GameObject("SpecialsCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Beside the Stash (locker 24..166, stash 178..320 -> kit 332..474).
            var go = new GameObject("KitRoot");
            go.transform.SetParent(canvasGO.transform, false);
            _root = go.AddComponent<RectTransform>();
            _root.sizeDelta = new Vector2(SIZE, SIZE);
            _root.anchorMin = _root.anchorMax = new Vector2(0f, 0f);
            _root.pivot = new Vector2(0f, 0f);
            _root.anchoredPosition = new Vector2(332f, 219f);

            var bg = new GameObject("Bg").AddComponent<Image>();
            bg.transform.SetParent(_root, false);
            AQTheme.Round(bg, new Color(0f, 0f, 0f, 0.55f));
            Stretch((RectTransform)bg.transform);

            // Delivered icon art preferred (2026-08-10); KIT monogram fallback.
            var btnSprite = Resources.Load<Sprite>("App/UI/Icons/ui_btn_case_kit");
            if (btnSprite != null)
            {
                var iconRt = new GameObject("Icon").AddComponent<RectTransform>();
                iconRt.transform.SetParent(_root, false);
                Stretch(iconRt);
                iconRt.offsetMin = new Vector2(6f, 6f);
                iconRt.offsetMax = new Vector2(-6f, -6f);
                var iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.sprite = btnSprite;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }
            else
            {
                var lbl = new GameObject("Lbl").AddComponent<TextMeshProUGUI>();
                lbl.transform.SetParent(_root, false);
                Stretch((RectTransform)lbl.transform);
                lbl.text = "KIT";
                lbl.fontSize = 34f;
                lbl.color = AQTheme.Paper;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.raycastTarget = false;
                AQTheme.StyleText(lbl, display: true);
            }

            var capRT = new GameObject("Caption").AddComponent<RectTransform>();
            capRT.transform.SetParent(_root, false);
            capRT.anchorMin = new Vector2(0f, 0f);
            capRT.anchorMax = new Vector2(1f, 0f);
            capRT.pivot = new Vector2(0.5f, 0f);
            capRT.sizeDelta = new Vector2(0f, 22f);
            capRT.anchoredPosition = new Vector2(0f, 2f);
            var cap = capRT.gameObject.AddComponent<TextMeshProUGUI>();
            cap.text = "CASE KIT";
            cap.fontSize = 13f;
            cap.color = AQTheme.PaperDim;
            cap.alignment = TextAlignmentOptions.Center;
            cap.raycastTarget = false;
            AQTheme.StyleText(cap, display: true);

            // Count badge, same construction as the Stash badge (text child AFTER bg).
            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(_root, false);
            var badgeRT = badgeGO.AddComponent<RectTransform>();
            badgeRT.sizeDelta = new Vector2(36f, 36f);
            badgeRT.anchorMin = badgeRT.anchorMax = new Vector2(1f, 1f);
            badgeRT.pivot = new Vector2(1f, 1f);
            badgeRT.anchoredPosition = new Vector2(8f, -8f);
            var badgeBg = new GameObject("BadgeBg").AddComponent<Image>();
            badgeBg.transform.SetParent(badgeGO.transform, false);
            Stretch((RectTransform)badgeBg.transform);
            AQTheme.Round(badgeBg, AQTheme.Amber);
            var badgeTxtGO = new GameObject("BadgeText");
            badgeTxtGO.transform.SetParent(badgeGO.transform, false);
            Stretch(badgeTxtGO.AddComponent<RectTransform>());
            _badge = badgeTxtGO.AddComponent<Text>();
            _badge.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _badge.fontSize = 20;
            _badge.fontStyle = FontStyle.Bold;
            _badge.alignment = TextAnchor.MiddleCenter;
            _badge.color = new Color(0.1f, 0.07f, 0.02f, 1f);
        }

        private int _lastTotal = -1;

        private void Refresh()
        {
            if (_root == null) return;
            int specials = SpecialItemsService.TotalCount;
            int total = specials + SpecialItemsService.Cassettes.Count;

            // Announce gains (lead rewards grant from AQ.App, which can't toast).
            if (_lastTotal >= 0 && specials > _lastTotal)
                ToastService.Show("case_kit_gain", "Added to the Case Kit.", 2f);
            _lastTotal = specials;

            _root.gameObject.SetActive(total > 0);
            if (total <= 0) return;
            _badge.transform.parent.gameObject.SetActive(specials > 0);
            _badge.text = specials.ToString();
        }

        // ---- kit popup ----

        private static void ShowKit()
        {
            if (_popup != null) return;

            _popup = new GameObject("__CaseKitPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_popup);
            var canvas = _popup.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            var scaler = _popup.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var dim = MakeStretchImage(_popup.transform, AQTheme.Scrim);

            var rows = new System.Collections.Generic.List<(SpecialId id, int count)>();
            foreach (var kv in SpecialItemsService.Catalog)
                if (SpecialItemsService.CountOf(kv.Key) > 0)
                    rows.Add((kv.Key, SpecialItemsService.CountOf(kv.Key)));
            var cassettes = SpecialItemsService.Cassettes;

            float rowH = 118f;
            float panelH = 300f + (rows.Count + cassettes.Count) * (rowH + 12f);
            var panel = new GameObject("Panel").AddComponent<RectTransform>();
            panel.transform.SetParent(_popup.transform, false);
            AQTheme.StylePanel(panel);
            AQTheme.PopIn(panel);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(680f, panelH);

            float y = panelH / 2f - 58f;
            AddLabel(panel, "CASE KIT", 46f, AQTheme.Paper, new Vector2(0f, y), true);
            y -= 50f;
            AddLabel(panel, "Tools of the trade. Tap USE, then tap a board tile.", 24f, AQTheme.PaperDim, new Vector2(0f, y), false);
            y -= 50f;

            foreach (var (id, count) in rows)
            {
                BuildSpecialRow(panel, id, count, new Vector2(0f, y - rowH / 2f));
                y -= rowH + 12f;
            }
            foreach (var clip in cassettes)
            {
                BuildCassetteRow(panel, clip, new Vector2(0f, y - rowH / 2f));
                y -= rowH + 12f;
            }

            var close = MakeButton(panel, "CLOSE", AQTheme.Teal, new Vector2(0f, -panelH / 2f + 75f), new Vector2(280f, 90f), 40f);
            close.onClick.AddListener(CloseKit);
        }

        private static void CloseKit()
        {
            if (_popup == null) return;
            Object.Destroy(_popup);
            _popup = null;
        }

        private static void BuildSpecialRow(RectTransform panel, SpecialId id, int count, Vector2 pos)
        {
            var (name, desc, _) = SpecialItemsService.Catalog[id];
            var row = MakeRow(panel, pos);

            AddMonogram(row, Monogram(id), id.ToString());
            var txt = AddRowText(row, $"{name}  <color=#E8A33D>x{count}</color>\n<size=20><color=#A5A092>{desc}</color></size>");

            var use = MakeButton(row, "USE", AQTheme.Steel, new Vector2(620f / 2f - 90f, 0f), new Vector2(156f, 74f), 30f);
            use.onClick.AddListener(() =>
            {
                CloseKit();
                BeginUse(id);
            });
        }

        private static void BuildCassetteRow(RectTransform panel, string clipPath, Vector2 pos)
        {
            var row = MakeRow(panel, pos);
            AddMonogram(row, "TC", "cassette");
            AddRowText(row, "Tip-Line Cassette\n<size=20><color=#A5A092>A kept voice. Play it again.</color></size>");
            var play = MakeButton(row, "PLAY", AQTheme.Steel, new Vector2(620f / 2f - 90f, 0f), new Vector2(156f, 74f), 30f);
            play.onClick.AddListener(() =>
            {
                var clip = Resources.Load<AudioClip>(clipPath);
                if (clip == null) { ToastService.Show("cassette_missing", "The tape is blank.", 2f); return; }
                var go = new GameObject("__CassettePlayer");
                Object.DontDestroyOnLoad(go);
                var src = go.AddComponent<AudioSource>();
                src.spatialBlend = 0f;
                src.volume = Audio.AudioSettingsService.DialogueVolume;
                src.clip = clip;
                src.Play();
                Object.Destroy(go, clip.length + 0.5f);
            });
        }

        // ---- use / targeting ----

        private static void BeginUse(SpecialId id)
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            if (id == SpecialId.EvidenceTag) { UseEvidenceTag(board); return; }

            ShowTargetOverlay(id, board);
        }

        private static void UseEvidenceTag(MergeBoardController board)
        {
            var checker = LeadRequirementChecker.Instance;
            if (checker == null) { ToastService.Show("tag_none", "No open leads to tag.", 2f); return; }

            foreach (var itemId in checker.NeededItemIds)
            {
                Items.ItemDefinitionSO def = null;
                foreach (var d in board.ItemDefinitions)
                    if (d != null && d.itemId == itemId) { def = d; break; }
                if (def == null || def.tier > 2) continue; // <= T3 (Stephen-ruled)

                if (!EvidenceLockerService.CanStore)
                {
                    ToastService.Show("tag_locker_full", "Locker full. Free a slot for the tagged item.", 2f);
                    return;
                }

                // Show what will happen and ask first (Stephen-ruled 2026-08-11).
                var defCap = def;
                SpecialConfirmPopup.Show("Evidence Tag",
                    $"{defCap.displayName} is filed in the locker for the open lead.",
                    new System.Collections.Generic.List<Sprite> { SpecialSprite("evidencetag") },
                    new System.Collections.Generic.List<Sprite> { board.SpriteForItem(defCap.family, defCap.tier) },
                    false,
                    onConfirm: () =>
                    {
                        EvidenceLockerService.TryStore(
                            new OverflowTileData { kind = OverflowKind.Item, family = defCap.family, tier = defCap.tier },
                            defCap.itemId);
                        SpecialItemsService.Consume(SpecialId.EvidenceTag);
                        ToastService.Show("tag_done", $"Evidence Tag: {defCap.displayName} filed in the locker.", 2.5f);
                    },
                    onCancel: null);
                return;
            }
            ToastService.Show("tag_none", "No taggable requirement right now (Tier 3 or below).", 2.5f);
        }

        private static void ShowTargetOverlay(SpecialId id, MergeBoardController board)
        {
            if (_overlay != null) return;

            _overlay = new GameObject("__SpecialTargetOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_overlay);
            var canvas = _overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000; // above HUD, below popups — board stays visible
            var scaler = _overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var (name, _, _) = SpecialItemsService.Catalog[id];
            var banner = new GameObject("Banner").AddComponent<RectTransform>();
            banner.transform.SetParent(_overlay.transform, false);
            banner.anchorMin = new Vector2(0f, 1f);
            banner.anchorMax = new Vector2(1f, 1f);
            banner.pivot = new Vector2(0.5f, 1f);
            banner.sizeDelta = new Vector2(0f, 120f);
            banner.anchoredPosition = new Vector2(0f, -430f);
            var bImg = banner.gameObject.AddComponent<Image>();
            AQTheme.Round(bImg, new Color(0.08f, 0.11f, 0.19f, 0.96f));
            bImg.raycastTarget = false;
            AddLabel(banner, $"{name}: tap a tile on the board", 30f, AQTheme.Paper, new Vector2(0f, 14f), true);
            AddLabel(banner, "or tap here to cancel", 22f, AQTheme.PaperDim, new Vector2(0f, -26f), false);

            var driver = _overlay.AddComponent<TargetDriver>();
            driver.Init(id, board, banner);
        }

        private sealed class TargetDriver : MonoBehaviour
        {
            private SpecialId _id;
            private MergeBoardController _board;
            private RectTransform _banner;
            private bool _confirming;

            public void Init(SpecialId id, MergeBoardController board, RectTransform banner)
            {
                _id = id;
                _board = board;
                _banner = banner;
            }

            private void Update()
            {
                if (_confirming) return; // the confirm popup owns input

                Vector2? tap = null;
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                    tap = Input.GetTouch(0).position;
                else if (Input.GetMouseButtonDown(0))
                    tap = Input.mousePosition;
                if (tap == null) return;

                if (RectTransformUtility.RectangleContainsScreenPoint(_banner, tap.Value, null))
                {
                    Done();
                    return;
                }

                var tile = _board != null ? _board.TileAtScreenPoint(tap.Value) : null;
                if (tile == null || tile.IsEmpty) return; // keep targeting

                // Show what will happen and ask first (Stephen-ruled 2026-08-11).
                if (!BuildPreview(tile, out var desc, out var before, out var after, out var unknown))
                    return; // refusal toast shown, keep targeting

                _confirming = true;
                var (name, _, _) = SpecialItemsService.Catalog[_id];
                SpecialConfirmPopup.Show(name, desc, before, after, unknown,
                    onConfirm: () => { _confirming = false; if (Apply(tile)) Done(); },
                    onCancel:  () => _confirming = false);
            }

            /// <summary>Preview of the effect on this tile. False = refused (toast
            /// already shown). Mirrors Apply()'s guards without mutating anything.</summary>
            private bool BuildPreview(BoardTileView tile, out string desc,
                                      out System.Collections.Generic.List<Sprite> before,
                                      out System.Collections.Generic.List<Sprite> after,
                                      out bool afterUnknown)
            {
                desc = null;
                before = new System.Collections.Generic.List<Sprite> { tile.Payload.sprite };
                after = null;
                afterUnknown = false;

                switch (_id)
                {
                    case SpecialId.SkeletonKey:
                    {
                        if (tile.Kind != TileKind.Item) { Refuse("That one won't turn. (Top tier, or not an item.)"); return false; }
                        var family = _board.FamilyOf(tile);
                        var up = _board.SpriteForItem(family, tile.Tier + 1);
                        if (up == null) { Refuse("That one won't turn. (Top tier, or not an item.)"); return false; }
                        desc = $"{NameOf(family, tile.Tier)} goes up a tier to {NameOf(family, tile.Tier + 1)}.";
                        after = new System.Collections.Generic.List<Sprite> { up };
                        return true;
                    }
                    case SpecialId.BoxKnife:
                    {
                        if (tile.Kind != TileKind.Item || tile.Tier < 1) { Refuse("Nothing to cut there. (Tier 2+ items only.)"); return false; }
                        var family = _board.FamilyOf(tile);
                        var down = _board.SpriteForItem(family, tile.Tier - 1);
                        desc = $"{NameOf(family, tile.Tier)} is cut into two of {NameOf(family, tile.Tier - 1)}.";
                        after = new System.Collections.Generic.List<Sprite> { down, down };
                        return true;
                    }
                    case SpecialId.CarbonCopy:
                    {
                        if (tile.Kind != TileKind.Item || tile.Tier > 3) { Refuse("Copies stop at Tier 4."); return false; }
                        var family = _board.FamilyOf(tile);
                        desc = $"{NameOf(family, tile.Tier)} is duplicated.";
                        after = new System.Collections.Generic.List<Sprite> { tile.Payload.sprite, tile.Payload.sprite };
                        return true;
                    }
                    case SpecialId.BoltCutters:
                    {
                        if (tile.Kind == TileKind.Generator && _board.CountGeneratorsOnBoard() <= 1)
                        { Refuse("Keep at least one generator on the board."); return false; }
                        desc = "This tile is removed from the board for good.";
                        after = new System.Collections.Generic.List<Sprite>(); // empty = removed box
                        return true;
                    }
                    case SpecialId.SearchWarrant:
                    {
                        if (tile.Kind != TileKind.Item) { Refuse("Serve it on an item."); return false; }
                        var family = _board.FamilyOf(tile);
                        bool anyLeft = false;
                        foreach (var d in _board.ItemDefinitions)
                            if (d != null && d.family == family &&
                                !Common.ItemDiscoveryService.IsDiscovered(family, d.tier))
                            { anyLeft = true; break; }
                        if (!anyLeft) { Refuse("Nothing left to uncover in that family."); return false; }
                        desc = "The next undiscovered item in this family is revealed.";
                        afterUnknown = true;
                        return true;
                    }
                }
                return false;
            }

            private string NameOf(string family, int tier)
            {
                foreach (var d in _board.ItemDefinitions)
                    if (d != null && d.family == family && d.tier == tier)
                        return d.displayName;
                return "Tier " + (tier + 1);
            }

            private bool Apply(BoardTileView tile)
            {
                switch (_id)
                {
                    case SpecialId.SkeletonKey:
                        if (!_board.UpgradeTile(tile)) { Refuse("That one won't turn. (Top tier, or not an item.)"); return false; }
                        SpecialItemsService.Consume(_id);
                        ToastService.Show("sp_key", "Skeleton Key: one tier up.", 1.6f);
                        return true;

                    case SpecialId.BoxKnife:
                        if (!_board.SplitTile(tile, out var toStash)) { Refuse("Nothing to cut there. (Tier 2+ items only.)"); return false; }
                        SpecialItemsService.Consume(_id);
                        ToastService.Show("sp_knife", toStash ? "Box Knife: split. Spare went to the Stash." : "Box Knife: split into two.", 1.8f);
                        return true;

                    case SpecialId.CarbonCopy:
                        if (tile.Kind != TileKind.Item || tile.Tier > 3) { Refuse("Copies stop at Tier 4."); return false; }
                        if (!_board.DuplicateTile(tile, out var copyStash)) { Refuse("Can't copy that."); return false; }
                        SpecialItemsService.Consume(_id);
                        ToastService.Show("sp_copy", copyStash ? "Carbon Copy: duplicate in the Stash." : "Carbon Copy: duplicated.", 1.8f);
                        return true;

                    case SpecialId.BoltCutters:
                        if (!_board.CutTile(tile)) { Refuse("Keep at least one generator on the board."); return false; }
                        SpecialItemsService.Consume(_id);
                        ToastService.Show("sp_cut", "Bolt Cutters: cleared.", 1.5f);
                        return true;

                    case SpecialId.SearchWarrant:
                        return ApplyWarrant(tile);
                }
                return false;
            }

            private bool ApplyWarrant(BoardTileView tile)
            {
                if (tile.Kind != TileKind.Item) { Refuse("Serve it on an item."); return false; }
                var family = _board.FamilyOf(tile);
                Items.ItemDefinitionSO reveal = null;
                foreach (var d in _board.ItemDefinitions)
                {
                    if (d == null || d.family != family) continue;
                    if (Common.ItemDiscoveryService.IsDiscovered(family, d.tier)) continue;
                    if (reveal == null || d.tier < reveal.tier) reveal = d;
                }
                if (reveal == null) { Refuse("Nothing left to uncover in that family."); return false; }

                Common.ItemDiscoveryService.Mark(family, reveal.tier);
                SpecialItemsService.Consume(SpecialId.SearchWarrant);
                ToastService.Show("sp_warrant", $"Search Warrant: {reveal.displayName} revealed.", 2.2f);
                Common.ItemFamilyPopup.Show(family, reveal.tier);
                return true;
            }

            private static void Refuse(string msg) => ToastService.Show("sp_refuse", msg, 2f);

            private void Done()
            {
                Object.Destroy(_overlay);
                _overlay = null;
            }
        }

        // ---- shared UI helpers ----

        private static string Monogram(SpecialId id) => id switch
        {
            SpecialId.SkeletonKey   => "SK",
            SpecialId.BoxKnife      => "BK",
            SpecialId.CarbonCopy    => "CC",
            SpecialId.BoltCutters   => "BC",
            SpecialId.SearchWarrant => "SW",
            SpecialId.EvidenceTag   => "ET",
            _ => "?"
        };

        private static RectTransform MakeRow(RectTransform panel, Vector2 pos)
        {
            var row = new GameObject("Row").AddComponent<RectTransform>();
            row.transform.SetParent(panel, false);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.5f);
            row.pivot = new Vector2(0.5f, 0.5f);
            row.sizeDelta = new Vector2(620f, 118f);
            row.anchoredPosition = pos;
            var bg = row.gameObject.AddComponent<Image>();
            AQTheme.Round(bg, AQTheme.Card);
            bg.raycastTarget = false;
            return row;
        }

        /// <summary>Resources sprite for a special, or null pre-art-delivery.</summary>
        public static Sprite SpecialSprite(string key) =>
            Resources.Load<Sprite>("App/UI/Specials/special_" + key.ToLowerInvariant());

        private static void AddMonogram(RectTransform row, string text, string spriteKey = null)
        {
            var chip = new GameObject("Chip").AddComponent<RectTransform>();
            chip.transform.SetParent(row, false);
            chip.anchorMin = chip.anchorMax = new Vector2(0f, 0.5f);
            chip.pivot = new Vector2(0f, 0.5f);
            chip.sizeDelta = new Vector2(84f, 84f);
            chip.anchoredPosition = new Vector2(14f, 0f);
            var img = chip.gameObject.AddComponent<Image>();
            AQTheme.Round(img, AQTheme.BoardFrame);
            img.raycastTarget = false;

            // Real art wins when delivered (SAS/special-items-art-kit.md);
            // display-font monogram is the pre-delivery placeholder.
            var sprite = spriteKey != null ? SpecialSprite(spriteKey) : null;
            if (sprite != null)
            {
                var icon = new GameObject("Icon").AddComponent<Image>();
                icon.transform.SetParent(chip, false);
                Stretch((RectTransform)icon.transform);
                ((RectTransform)icon.transform).offsetMin = new Vector2(6f, 6f);
                ((RectTransform)icon.transform).offsetMax = new Vector2(-6f, -6f);
                icon.sprite = sprite;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                return;
            }

            var t = new GameObject("M").AddComponent<TextMeshProUGUI>();
            t.transform.SetParent(chip, false);
            Stretch((RectTransform)t.transform);
            t.text = text;
            t.fontSize = 34f;
            t.color = AQTheme.Amber;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            AQTheme.StyleText(t, display: true);
        }

        private static TMP_Text AddRowText(RectTransform row, string text)
        {
            var rt = new GameObject("Txt").AddComponent<RectTransform>();
            rt.transform.SetParent(row, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(112f, 8f);
            rt.offsetMax = new Vector2(-186f, -8f);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 26f;
            tmp.color = AQTheme.Paper;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp);
            return tmp;
        }

        private static Image MakeStretchImage(Transform parent, Color color)
        {
            var img = new GameObject("Img").AddComponent<Image>();
            img.transform.SetParent(parent, false);
            img.color = color;
            Stretch((RectTransform)img.transform);
            return img;
        }

        private static void AddLabel(RectTransform parent, string text, float size, Color color, Vector2 pos, bool bold)
        {
            var rt = new GameObject("Lbl").AddComponent<RectTransform>();
            rt.transform.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620f, 60f);
            rt.anchoredPosition = pos;
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            AQTheme.StyleText(tmp, display: bold);
        }

        private static Button MakeButton(RectTransform parent, string label, Color color, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            AQTheme.StyleButton(go.GetComponent<Image>(), color);

            var lbl = new GameObject("Label").AddComponent<RectTransform>();
            lbl.transform.SetParent(rt, false);
            Stretch(lbl);
            var tmp = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = AQTheme.Paper;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: true);
            return go.GetComponent<Button>();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
