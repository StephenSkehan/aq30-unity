#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using System.IO;

public static class LeadCardPrefabCreator
{
    const string PrefabsFolder = "Assets/UI/Prefabs";
    const string LeadCardPath  = PrefabsFolder + "/LeadCard.prefab";
    const string TierPopupPath = PrefabsFolder + "/TierSetPopup.prefab";

    // -------- Menu Items --------

    [MenuItem("AQ/UI/Create Lead Card Prefab")]
    public static void CreateLeadCardPrefab()
    {
        EnsureFolder(PrefabsFolder);

        var go = new GameObject("LeadCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360, 220);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot    = new Vector2(0, 1);

        var img = go.GetComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = true;

        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth  = 360;
        le.preferredHeight = 220;
        le.flexibleWidth   = 0;
        le.flexibleHeight  = 0;

        // Title
        var title = CreateText(go.transform, "Text_Title", "Demo Lead", 32, TextAlignmentOptions.Left);
        AnchorTopStretch(title.rectTransform, 14, 14, 12, 52);

        // Lead ID (tiny QA tag)
        var leadId = CreateText(go.transform, "Text_LeadId", "#L-123", 18, TextAlignmentOptions.Right);
        AnchorTopStretch(leadId.rectTransform, 12, 12, 10, 30);

        // Objective
        var objective = CreateText(go.transform, "Text_Objective", "Collect deli CCTV", 26, TextAlignmentOptions.Left);
        AnchorTopStretch(objective.rectTransform, 14, 14, 54, 92);

        // Requirements row
        var reqRow = CreateRow(go.transform, "RequirementsRow", 8, TextAnchor.MiddleLeft);
        AnchorTopStretch(reqRow, 14, 14, 96, 156);

        // Three requirement slots
        var slot1 = CreateRequirementSlot(reqRow, "Req_1");
        var slot2 = CreateRequirementSlot(reqRow, "Req_2");
        var slot3 = CreateRequirementSlot(reqRow, "Req_3");

        // Rewards row (optional for now)
        var rewRow = CreateRow(go.transform, "RewardsRow", 8, TextAnchor.MiddleLeft);
        AnchorTopStretch(rewRow, 14, 14, 160, 200);
        rewRow.gameObject.SetActive(false);

        // Actor Anchor (overlay above card)
        var actor = new GameObject("ActorAnchor", typeof(RectTransform), typeof(Image));
        var actorRT = actor.GetComponent<RectTransform>();
        actorRT.SetParent(go.transform, false);
        actorRT.sizeDelta     = new Vector2(96, 96);
        actorRT.anchorMin     = new Vector2(0.5f, 1f);
        actorRT.anchorMax     = new Vector2(0.5f, 1f);
        actorRT.pivot         = new Vector2(0.5f, 0f);
        actorRT.anchoredPosition = new Vector2(0, 18);
        actor.GetComponent<Image>().color = new Color(1, 1, 1, 0); // transparent by default

        // Button (click whole card)
        var cardButton = go.GetComponent<Button>();
        var colors = cardButton.colors;
        colors.fadeDuration = 0.05f;
        cardButton.colors   = colors;

        // Try to add & wire the runtime LeadCardPresenter (reflection; succeeds if present)
        TryWireLeadCardPresenter(
            root: go,
            background: img,
            title: title,
            objective: objective,
            leadId: leadId,
            requirementsRow: reqRow,
            rewardRow: rewRow,
            slot1: slot1,
            slot2: slot2,
            slot3: slot3,
            actorImage: actor.GetComponent<Image>(),
            wholeCardButton: cardButton
        );

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, LeadCardPath);
        UnityEngine.Object.DestroyImmediate(go);
        Selection.activeObject = prefab;

        Debug.Log($"Created/updated Lead Card prefab at {LeadCardPath}");
    }

    [MenuItem("AQ/UI/Create Tier Set Popup Prefab")]
    public static void CreateTierSetPopupPrefab()
    {
        EnsureFolder(PrefabsFolder);

        var root = new GameObject("TierSetPopup", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.zero;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot     = new Vector2(0.5f, 0.5f);

        var dim = root.GetComponent<Image>();
        dim.color = new Color(0, 0, 0, 0.5f);
        dim.raycastTarget = true;

        var cg = root.GetComponent<CanvasGroup>();

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        var prt = panel.GetComponent<RectTransform>();
        prt.SetParent(root.transform, false);
        prt.sizeDelta = new Vector2(900, 360);
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot     = new Vector2(0.5f, 0.5f);
        prt.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        var title = CreateText(panel.transform, "Title", "STAKEOUT FUEL", 36, TextAlignmentOptions.Center);
        AnchorTopStretch(title.rectTransform, 20, 20, 18, 70);

        var content = new GameObject("Content", typeof(RectTransform));
        var crt = content.GetComponent<RectTransform>();
        crt.SetParent(panel.transform, false);
        AnchorTopStretch(crt, 20, 20, 76, 300);

        var hlg = content.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlHeight = true;
        hlg.childControlWidth  = true;

        var item = new GameObject("ItemTemplate", typeof(RectTransform));
        var irt = item.GetComponent<RectTransform>();
        irt.SetParent(content.transform, false);
        irt.sizeDelta = new Vector2(120, 180);
        item.SetActive(false);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        var iconRT = iconGo.GetComponent<RectTransform>();
        iconRT.SetParent(item.transform, false);
        iconRT.anchorMin = new Vector2(0.5f, 1f);
        iconRT.anchorMax = new Vector2(0.5f, 1f);
        iconRT.pivot     = new Vector2(0.5f, 1f);
        iconRT.sizeDelta = new Vector2(100, 100);
        iconRT.anchoredPosition = new Vector2(0, -16);

        var label = CreateText(item.transform, "Label", "T1", 24, TextAlignmentOptions.Center);
        AnchorBottomStretch(label.rectTransform, 8, 8, 12, 72);

        var hiGo = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
        var hiRT = hiGo.GetComponent<RectTransform>();
        hiRT.SetParent(item.transform, false);
        hiRT.anchorMin = Vector2.zero;
        hiRT.anchorMax = Vector2.one;
        hiRT.offsetMin = Vector2.zero;
        hiRT.offsetMax = Vector2.zero;
        var hiImg = hiGo.GetComponent<Image>();
        hiImg.color = new Color(0, 0, 0, 0);
        hiImg.raycastTarget = false;

        var close = new GameObject("Btn_Close", typeof(RectTransform), typeof(Button), typeof(Image));
        var closeRT = close.GetComponent<RectTransform>();
        closeRT.SetParent(panel.transform, false);
        closeRT.sizeDelta = new Vector2(40, 40);
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot     = new Vector2(1f, 1f);
        closeRT.anchoredPosition = new Vector2(-10, -10);
        close.GetComponent<Image>().color = new Color(1, 1, 1, 0.12f);

        // Try to add & wire TierSetPopup via reflection
        TryWireTierSetPopup(root, cg, close.GetComponent<Button>(), title, crt, item);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, TierPopupPath);
        UnityEngine.Object.DestroyImmediate(root);
        Selection.activeObject = prefab;

        Debug.Log($"Created/updated Tier Set Popup prefab at {TierPopupPath}");
    }

    // -------- Reflection wiring --------

    static void TryWireLeadCardPresenter(
        GameObject root,
        Image background,
        TMP_Text title,
        TMP_Text objective,
        TMP_Text leadId,
        RectTransform requirementsRow,
        RectTransform rewardRow,
        GameObject slot1,
        GameObject slot2,
        GameObject slot3,
        Image actorImage,
        Button wholeCardButton)
    {
        var presenterType = FindType("AQ.UI.Leads.LeadCardPresenter");
        var slotType      = FindType("AQ.UI.Leads.RequirementSlotView");

        if (presenterType == null)
        {
            Debug.LogWarning("[LeadCardPrefabCreator] LeadCardPresenter not found. Prefab created without runtime binding (this is OK if you haven’t added runtime yet).");
            return;
        }

        // Add presenter
        var presenter = root.AddComponent(presenterType);

        // Add slot components if present & wire their fields
        Component s1 = (slotType != null) ? slot1.AddComponent(slotType) : null;
        Component s2 = (slotType != null) ? slot2.AddComponent(slotType) : null;
        Component s3 = (slotType != null) ? slot3.AddComponent(slotType) : null;

        if (slotType != null)
        {
            WireField(slotType, s1, "button", slot1.GetComponent<Button>());
            WireField(slotType, s1, "icon",   slot1.transform.Find("Icon").GetComponent<Image>());
            WireField(slotType, s1, "tickOverlay", slot1.transform.Find("Tick").gameObject);
            WireField(slotType, s1, "label",  slot1.transform.Find("Label").GetComponent<TMP_Text>());

            WireField(slotType, s2, "button", slot2.GetComponent<Button>());
            WireField(slotType, s2, "icon",   slot2.transform.Find("Icon").GetComponent<Image>());
            WireField(slotType, s2, "tickOverlay", slot2.transform.Find("Tick").gameObject);
            WireField(slotType, s2, "label",  slot2.transform.Find("Label").GetComponent<TMP_Text>());

            WireField(slotType, s3, "button", slot3.GetComponent<Button>());
            WireField(slotType, s3, "icon",   slot3.transform.Find("Icon").GetComponent<Image>());
            WireField(slotType, s3, "tickOverlay", slot3.transform.Find("Tick").gameObject);
            WireField(slotType, s3, "label",  slot3.transform.Find("Label").GetComponent<TMP_Text>());
        }

        // Presenter fields
        WireField(presenterType, presenter, "background", background);
        WireField(presenterType, presenter, "titleText",  title);
        WireField(presenterType, presenter, "objectiveText", objective);
        WireField(presenterType, presenter, "leadIdText", leadId);
        WireField(presenterType, presenter, "requirementsRow", requirementsRow);
        if (slotType != null)
        {
            var arr = Array.CreateInstance(slotType, 3);
            arr.SetValue(s1, 0);
            arr.SetValue(s2, 1);
            arr.SetValue(s3, 2);
            WireField(presenterType, presenter, "slots", arr);
        }
        WireField(presenterType, presenter, "rewardsRow", rewardRow);
        WireField(presenterType, presenter, "actorAnchor", actorImage);
        WireField(presenterType, presenter, "wholeCardButton", wholeCardButton);
    }

    static void TryWireTierSetPopup(GameObject root, CanvasGroup cg, Button close, TMP_Text title, RectTransform content, GameObject itemTemplate)
    {
        var popupType = FindType("AQ.UI.Leads.TierSetPopup");
        if (popupType == null)
        {
            Debug.LogWarning("[LeadCardPrefabCreator] TierSetPopup not found. Popup prefab created without runtime component (OK if you add runtime later).");
            return;
        }

        var popup = root.AddComponent(popupType);
        WireField(popupType, popup, "canvasGroup", cg);
        WireField(popupType, popup, "closeButton", close);
        WireField(popupType, popup, "titleText",   title);
        WireField(popupType, popup, "content",     content);
        WireField(popupType, popup, "itemTemplate", itemTemplate);
        WireField(popupType, popup, "highlightColor", new Color(0.0f, 0.75f, 0.35f, 0.8f));
    }

    // -------- Creation helpers --------

    static TMP_Text CreateText(Transform parent, string name, string text, int size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.color = Color.white;
        return tmp;
    }

    static RectTransform CreateRow(Transform parent, string name, float spacing, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = alignment;
        layout.childControlHeight = false;
        layout.childControlWidth  = false;
        return rt;
    }

    static GameObject CreateRequirementSlot(RectTransform parent, string name)
    {
        var slot = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        slot.transform.SetParent(parent, false);
        var rt = slot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);
        slot.GetComponent<Image>().color = Color.white;

        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        var iconRT = icon.GetComponent<RectTransform>();
        iconRT.SetParent(slot.transform, false);
        iconRT.anchorMin = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.pivot     = new Vector2(0.5f, 0.5f);
        iconRT.sizeDelta = new Vector2(72, 72);

        var tick = new GameObject("Tick", typeof(RectTransform), typeof(Image));
        var tickRT = tick.GetComponent<RectTransform>();
        tickRT.SetParent(slot.transform, false);
        tickRT.anchorMin = new Vector2(1f, 0f);
        tickRT.anchorMax = new Vector2(1f, 0f);
        tickRT.pivot     = new Vector2(1f, 0f);
        tickRT.anchoredPosition = new Vector2(-6, 6);
        tickRT.sizeDelta = new Vector2(22, 22);
        tick.GetComponent<Image>().color = new Color(0.20f, 0.85f, 0.40f, 1f);
        tick.SetActive(false);

        var lab = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        var labRT = lab.GetComponent<RectTransform>();
        labRT.SetParent(slot.transform, false);
        labRT.anchorMin = new Vector2(0.5f, 0f);
        labRT.anchorMax = new Vector2(0.5f, 0f);
        labRT.pivot     = new Vector2(0.5f, 0f);
        labRT.anchoredPosition = new Vector2(0, -16);
        var tmp = lab.GetComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 18;
        tmp.color = new Color(0.80f, 0.88f, 0.96f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;

        return slot;
    }

    static void AnchorTopStretch(RectTransform rt, float left, float right, float top, float bottomFromTop)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(left, -(bottomFromTop));
        rt.offsetMax = new Vector2(-right, -top);
    }

    static void AnchorBottomStretch(RectTransform rt, float left, float right, float bottom, float topFromBottom)
    {
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, topFromBottom);
    }

    // -------- Reflection utils --------

    static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName);
            if (t != null) return t;
        }
        return null;
    }

    static void WireField(Type t, Component instance, string fieldName, object value)
    {
        if (t == null || instance == null) return;
        var f = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (f == null) return;
        if (value != null && !f.FieldType.IsInstanceOfType(value))
        {
            if (f.FieldType == typeof(Color) && value is Color col)
            {
                f.SetValue(instance, col);
                return;
            }
        }
        f.SetValue(instance, value);
    }

    // -------- Asset helpers --------

    static void EnsureFolder(string path)
    {
        // path must start at Assets
        if (string.IsNullOrEmpty(path)) return;
        var parts = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}
#endif
