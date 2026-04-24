using UnityEditor;
using UnityEngine;
using AQ.App.Items;
using AQ.App.Leads;

namespace AQ.Editor.Items
{
    public static class DemoLeadSetup
    {
        private struct TierSpec
        {
            public string itemId;
            public int    tier;
            public string displayName;
            public string spritePath;
        }

        private static readonly TierSpec[] StakeoutFuelTiers = new TierSpec[]
        {
            new TierSpec { itemId="stakeout_fuel_t1", tier=0, displayName="Paper Cup",      spritePath="Assets/Art/Icons/Items/food_gifts/food_gifts_t01_paper_cup.png" },
            new TierSpec { itemId="stakeout_fuel_t2", tier=1, displayName="Hot Coffee",     spritePath="Assets/Art/Icons/Items/food_gifts/food_gifts_t02_hot_coffee_cup.png" },
            new TierSpec { itemId="stakeout_fuel_t3", tier=2, displayName="Coffee & Donut", spritePath="Assets/Art/Icons/Items/food_gifts/food_gifts_t03_coffee_and_donut.png" },
            new TierSpec { itemId="stakeout_fuel_t4", tier=3, displayName="Burger",         spritePath="Assets/Art/Icons/Items/food_gifts/food_gifts_t04_burger_single.png" },
            new TierSpec { itemId="stakeout_fuel_t5", tier=4, displayName="Combo Meal",     spritePath="Assets/Art/Icons/Items/food_gifts/food_gifts_t05_burger_fries_drink.png" },
        };

        [MenuItem("Tools/AQ/Fix Demo Lead Portraits")]
        public static void FixPortraits()
        {
            const string spritePath = "Assets/Art/Characters/Ally/char_ally_neutral_f01.png";
            const string leadDir    = "Assets/App/Leads/Data";

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogError($"[FixPortraits] Sprite not found at {spritePath}. Check import settings (textureType must be Sprite).");
                return;
            }
            Debug.Log($"[FixPortraits] Loaded sprite: {sprite.name}");

            int fixed_ = 0;
            string[] guids = AssetDatabase.FindAssets("t:LeadData", new[] { leadDir });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var lead = AssetDatabase.LoadAssetAtPath<LeadData>(path);
                if (lead == null) continue;

                var so = new SerializedObject(lead);
                var prop = so.FindProperty("actorPortrait");
                if (prop == null) { Debug.LogWarning($"[FixPortraits] 'actorPortrait' field not found on {lead.name}"); continue; }

                prop.objectReferenceValue = sprite;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(lead);
                Debug.Log($"[FixPortraits] Set portrait on {lead.name} → {sprite.name}");
                fixed_++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FixPortraits] Done — fixed {fixed_} lead(s).");
        }

        [MenuItem("Tools/AQ/Setup Demo Leads")]
        public static void Run()
        {
            const string soDir   = "Assets/ScriptableObjects/Items";
            const string leadDir = "Assets/App/Leads/Data";

            // ── 1. Create stakeout_fuel ItemDefinitionSOs ─────────────────
            var fuelSOs = new ItemDefinitionSO[StakeoutFuelTiers.Length];
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < StakeoutFuelTiers.Length; i++)
                {
                    var spec = StakeoutFuelTiers[i];
                    string path = soDir + "/" + spec.itemId + ".asset";
                    var existing = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(path);
                    if (existing != null) { fuelSOs[i] = existing; continue; }

                    var so = ScriptableObject.CreateInstance<ItemDefinitionSO>();
                    so.itemId      = spec.itemId;
                    so.family      = "stakeout_fuel";
                    so.tier        = spec.tier;
                    so.displayName = spec.displayName;
                    so.icon        = AssetDatabase.LoadAssetAtPath<Sprite>(spec.spritePath);
                    AssetDatabase.CreateAsset(so, path);
                    fuelSOs[i] = so;
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reload after save so GUIDs are valid
            for (int i = 0; i < fuelSOs.Length; i++)
                if (fuelSOs[i] == null)
                    fuelSOs[i] = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(soDir + "/" + StakeoutFuelTiers[i].itemId + ".asset");

            // ── 2. Append stakeout_fuel SOs to ItemRegistry ───────────────
            var registry = Object.FindAnyObjectByType<ItemRegistry>();
            if (registry != null)
            {
                var regSo = new SerializedObject(registry);
                var itemsProp = regSo.FindProperty("_items");
                foreach (var fuel in fuelSOs)
                {
                    if (fuel == null) continue;
                    bool found = false;
                    for (int i = 0; i < itemsProp.arraySize; i++)
                        if (itemsProp.GetArrayElementAtIndex(i).objectReferenceValue == fuel) { found = true; break; }
                    if (!found)
                    {
                        itemsProp.arraySize++;
                        itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1).objectReferenceValue = fuel;
                    }
                }
                regSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(registry);
            }

            // ── 3. Create demo LeadData assets ────────────────────────────
            var portrait = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Art/Characters/Ally/char_ally_neutral_f01.png");

            var lead1 = MakeLead(leadDir + "/Lead_Demo_StakeoutDiner.asset",
                "demo_stake_diner", "Stake Out the Diner",
                "Watch the corner diner for suspicious activity",
                LeadState.Available, portrait,
                new string[]  { "Paper Cup" },
                new ItemDefinitionSO[] { fuelSOs[0] });

            var lead2 = MakeLead(leadDir + "/Lead_Demo_GetBethTalking.asset",
                "demo_beth_talking", "Get Beth Talking",
                "Buy Beth a coffee to loosen her up",
                LeadState.Available, portrait,
                new string[]  { "Hot Coffee", "Coffee & Donut" },
                new ItemDefinitionSO[] { fuelSOs[1], fuelSOs[2] });

            var lead3 = MakeLead(leadDir + "/Lead_Demo_LateNightMeet.asset",
                "demo_late_night", "The Late Night Meet",
                "Bring food — the informant only talks over a proper meal",
                LeadState.Blocked, portrait,
                new string[]  { "Burger" },
                new ItemDefinitionSO[] { fuelSOs[3] });

            // ── 4. Replace LeadsDatabase contents ─────────────────────────
            var db = AssetDatabase.LoadAssetAtPath<LeadsDatabase>("Assets/App/Leads/LeadsDatabase.asset");
            if (db == null) { Debug.LogError("[DemoLeadSetup] LeadsDatabase not found."); return; }

            var dbSo = new SerializedObject(db);
            var leadsProp = dbSo.FindProperty("leads");
            leadsProp.arraySize = 3;
            leadsProp.GetArrayElementAtIndex(0).objectReferenceValue = lead1;
            leadsProp.GetArrayElementAtIndex(1).objectReferenceValue = lead2;
            leadsProp.GetArrayElementAtIndex(2).objectReferenceValue = lead3;
            dbSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(db);

            AssetDatabase.SaveAssets();

            if (registry != null)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(registry.gameObject.scene);

            Debug.Log("[DemoLeadSetup] Done — 5 stakeout_fuel SOs, 3 demo leads created, database updated.");
        }

        private static LeadData MakeLead(string path, string leadId, string title, string subtitle,
            LeadState state, Sprite portrait, string[] labels, ItemDefinitionSO[] defs)
        {
            var lead = AssetDatabase.LoadAssetAtPath<LeadData>(path);
            if (lead == null)
            {
                lead = ScriptableObject.CreateInstance<LeadData>();
                AssetDatabase.CreateAsset(lead, path);
            }

            var so = new SerializedObject(lead);
            so.FindProperty("leadId").stringValue   = leadId;
            so.FindProperty("title").stringValue    = title;
            so.FindProperty("subtitle").stringValue = subtitle;
            so.FindProperty("state").enumValueIndex = (int)state;
            so.FindProperty("actorPortrait").objectReferenceValue = portrait;

            var reqsProp = so.FindProperty("requirements");
            reqsProp.arraySize = labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                var el = reqsProp.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("label").stringValue                   = labels[i];
                el.FindPropertyRelative("satisfied").boolValue                 = false;
                el.FindPropertyRelative("itemDefinition").objectReferenceValue = defs[i];
                el.FindPropertyRelative("icon").objectReferenceValue           = null;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(lead);
            return lead;
        }
    }
}
