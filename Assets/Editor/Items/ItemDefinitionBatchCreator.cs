using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AQ.App.Items;

namespace AQ.Editor.Items
{
    /// <summary>
    /// One-shot batch creator for all ItemDefinitionSO assets.
    /// Run via: Tools > AQ > Create All Item Definitions
    /// Idempotent: skips assets that already exist at the target path.
    /// </summary>
    public static class ItemDefinitionBatchCreator
    {
        private struct ItemSpec
        {
            public string itemId;
            public string family;
            public int tier;          // 0-based
            public string spritePath; // relative to Assets/Art/Icons/
            public string displayName;
        }

        private static readonly List<ItemSpec> _specs = new()
        {
            // ── Fingerprint Evidence (6 tiers) ─────────────────────────────
            new() { itemId="fingerprint_evidence_t1", family="fingerprint_evidence", tier=0, displayName="Partial Dusted Print",       spritePath="Items/fingerprint_evidence/fingerprint_evidence_t1_partial_dusted_print.png" },
            new() { itemId="fingerprint_evidence_t2", family="fingerprint_evidence", tier=1, displayName="Lifted Print Tape",           spritePath="Items/fingerprint_evidence/fingerprint_evidence_t2_lifted_print_tape.png" },
            new() { itemId="fingerprint_evidence_t3", family="fingerprint_evidence", tier=2, displayName="Fingerprint Card",            spritePath="Items/fingerprint_evidence/fingerprint_evidence_t3_fingerprint_card.png" },
            new() { itemId="fingerprint_evidence_t4", family="fingerprint_evidence", tier=3, displayName="Labeled Prints",              spritePath="Items/fingerprint_evidence/fingerprint_evidence_t4_labeled_prints.png" },
            new() { itemId="fingerprint_evidence_t5", family="fingerprint_evidence", tier=4, displayName="Digital Scan In Progress",    spritePath="Items/fingerprint_evidence/fingerprint_evidence_t5_digital_scan_in_progress.png" },
            new() { itemId="fingerprint_evidence_t6", family="fingerprint_evidence", tier=5, displayName="Database Match",              spritePath="Items/fingerprint_evidence/fingerprint_evidence_t6_database_match.png" },

            // ── Food Gifts (12 tiers) ───────────────────────────────────────
            new() { itemId="food_gifts_t01", family="food_gifts", tier=0,  displayName="Paper Cup",               spritePath="Items/food_gifts/food_gifts_t01_paper_cup.png" },
            new() { itemId="food_gifts_t02", family="food_gifts", tier=1,  displayName="Hot Coffee Cup",           spritePath="Items/food_gifts/food_gifts_t02_hot_coffee_cup.png" },
            new() { itemId="food_gifts_t03", family="food_gifts", tier=2,  displayName="Coffee and Donut",         spritePath="Items/food_gifts/food_gifts_t03_coffee_and_donut.png" },
            new() { itemId="food_gifts_t04", family="food_gifts", tier=3,  displayName="Burger Single",            spritePath="Items/food_gifts/food_gifts_t04_burger_single.png" },
            new() { itemId="food_gifts_t05", family="food_gifts", tier=4,  displayName="Burger, Fries & Drink",    spritePath="Items/food_gifts/food_gifts_t05_burger_fries_drink.png" },
            new() { itemId="food_gifts_t06", family="food_gifts", tier=5,  displayName="Takeaway Caddy",           spritePath="Items/food_gifts/food_gifts_t06_takeaway_caddy.png" },
            new() { itemId="food_gifts_t07", family="food_gifts", tier=6,  displayName="Pasta Bowl",               spritePath="Items/food_gifts/food_gifts_t07_pasta_bowl.png" },
            new() { itemId="food_gifts_t08", family="food_gifts", tier=7,  displayName="Sushi Bento",              spritePath="Items/food_gifts/food_gifts_t08_sushi_bento.png" },
            new() { itemId="food_gifts_t09", family="food_gifts", tier=8,  displayName="Ice Cream Sundae Luxury",  spritePath="Items/food_gifts/food_gifts_t09_ice_cream_sundae_luxury.png" },
            new() { itemId="food_gifts_t10", family="food_gifts", tier=9,  displayName="Steak Plate",              spritePath="Items/food_gifts/food_gifts_t10_steak_plate.png" },
            new() { itemId="food_gifts_t11", family="food_gifts", tier=10, displayName="Surf and Turf with Wine",  spritePath="Items/food_gifts/food_gifts_t11_surf_and_turf_wine.png" },
            new() { itemId="food_gifts_t12", family="food_gifts", tier=11, displayName="Lobster Champagne Banquet",spritePath="Items/food_gifts/food_gifts_t12_lobster_champagne_banquet.png" },

            // ── Forensic Tools (5 tiers) ────────────────────────────────────
            new() { itemId="forensic_tools_t1", family="forensic_tools", tier=0, displayName="Cotton Swab",                    spritePath="Items/forensic_tools/forensic_tools_t1_cotton_swab.png" },
            new() { itemId="forensic_tools_t2", family="forensic_tools", tier=1, displayName="Evidence Bag",                   spritePath="Items/forensic_tools/forensic_tools_t2_evidence_bag.png" },
            new() { itemId="forensic_tools_t3", family="forensic_tools", tier=2, displayName="Full Forensic Case",             spritePath="Items/forensic_tools/forensic_tools_t3_full_forensic_case_black.png" },
            new() { itemId="forensic_tools_t4", family="forensic_tools", tier=3, displayName="UV Light",                      spritePath="Items/forensic_tools/forensic_tools_t4_uv_light.png" },
            new() { itemId="forensic_tools_t5", family="forensic_tools", tier=4, displayName="Complete Forensic Kit",          spritePath="Items/forensic_tools/forensic_tools_t5_complete_forensic_kit_transparent_soft.png" },

            // ── Garage (10 tiers) ───────────────────────────────────────────
            new() { itemId="garage_t01", family="garage", tier=0, displayName="Socket Wrench",           spritePath="Items/garage/garage_t01_socket_wrench_chrome.png" },
            new() { itemId="garage_t02", family="garage", tier=1, displayName="Oil Can",                 spritePath="Items/garage/garage_t02_oil_can.png" },
            new() { itemId="garage_t03", family="garage", tier=2, displayName="Shock Absorbers",         spritePath="Items/garage/garage_t03_shock_absorbers_pair.png" },
            new() { itemId="garage_t04", family="garage", tier=3, displayName="Tyres",                   spritePath="Items/garage/garage_t04_tyres_pile.png" },
            new() { itemId="garage_t05", family="garage", tier=4, displayName="Spark Plug",              spritePath="Items/garage/garage_t05_spark_plug.png" },
            new() { itemId="garage_t06", family="garage", tier=5, displayName="Car Battery",             spritePath="Items/garage/garage_t06_car_battery.png" },
            new() { itemId="garage_t07", family="garage", tier=6, displayName="Chrome Grille",           spritePath="Items/garage/garage_t07_chrome_grille.png" },
            new() { itemId="garage_t08", family="garage", tier=7, displayName="Custom Exhaust",          spritePath="Items/garage/garage_t08_custom_exhaust.png" },
            new() { itemId="garage_t09", family="garage", tier=8, displayName="Big Block Engine",        spritePath="Items/garage/garage_t09_big_block_engine_chrome_extractors.png" },
            new() { itemId="garage_t10", family="garage", tier=9, displayName="Pimped Ride",             spritePath="Items/garage/garage_t10_pimped_ride_hood_up_chrome_engine.png" },

            // ── Helen's Gifts (10 tiers) ────────────────────────────────────
            new() { itemId="helens_gifts_t01", family="helens_gifts", tier=0, displayName="Note",        spritePath="Items/helens_gifts/helens_gifts_t01_note.png" },
            new() { itemId="helens_gifts_t02", family="helens_gifts", tier=1, displayName="Daisy Posy",  spritePath="Items/helens_gifts/helens_gifts_t02_daisy_posie.png" },
            new() { itemId="helens_gifts_t03", family="helens_gifts", tier=2, displayName="Bouquet",     spritePath="Items/helens_gifts/helens_gifts_t03_bouquet.png" },
            new() { itemId="helens_gifts_t04", family="helens_gifts", tier=3, displayName="Cookies",     spritePath="Items/helens_gifts/helens_gifts_t04_cookies.png" },
            new() { itemId="helens_gifts_t05", family="helens_gifts", tier=4, displayName="Chocolate",   spritePath="Items/helens_gifts/helens_gifts_t05_chocolate.png" },
            new() { itemId="helens_gifts_t06", family="helens_gifts", tier=5, displayName="Dessert",     spritePath="Items/helens_gifts/helens_gifts_t06_dessert.png" },
            new() { itemId="helens_gifts_t07", family="helens_gifts", tier=6, displayName="Boots",       spritePath="Items/helens_gifts/helens_gifts_t07_boots.png" },
            new() { itemId="helens_gifts_t08", family="helens_gifts", tier=7, displayName="Scarf",       spritePath="Items/helens_gifts/helens_gifts_t08_scarf.png" },
            new() { itemId="helens_gifts_t09", family="helens_gifts", tier=8, displayName="Perfume",     spritePath="Items/helens_gifts/helens_gifts_t09_perfume.png" },
            new() { itemId="helens_gifts_t10", family="helens_gifts", tier=9, displayName="Locket",      spritePath="Items/helens_gifts/helens_gifts_t10_locket.png" },

            // ── Rusty Anchor (10 tiers) ─────────────────────────────────────
            new() { itemId="rusty_anchor_t01", family="rusty_anchor", tier=0, displayName="Shot Glass",              spritePath="Items/rusty_anchor/rusty_anchor_t01_shot_glass_empty.png" },
            new() { itemId="rusty_anchor_t02", family="rusty_anchor", tier=1, displayName="Short Glass on Ice",      spritePath="Items/rusty_anchor/rusty_anchor_t02_short_glass_ice.png" },
            new() { itemId="rusty_anchor_t03", family="rusty_anchor", tier=2, displayName="Tall Glass Orange",       spritePath="Items/rusty_anchor/rusty_anchor_t03_tall_glass_ice_orange.png" },
            new() { itemId="rusty_anchor_t04", family="rusty_anchor", tier=3, displayName="Beer Bottle",             spritePath="Items/rusty_anchor/rusty_anchor_t04_beer_bottle.png" },
            new() { itemId="rusty_anchor_t05", family="rusty_anchor", tier=4, displayName="Wine Glass Red",          spritePath="Items/rusty_anchor/rusty_anchor_t05_wine_glass_red.png" },
            new() { itemId="rusty_anchor_t06", family="rusty_anchor", tier=5, displayName="Champagne Flute",         spritePath="Items/rusty_anchor/rusty_anchor_t06_champagne_flute.png" },
            new() { itemId="rusty_anchor_t07", family="rusty_anchor", tier=6, displayName="Wine Bottle & Glasses",   spritePath="Items/rusty_anchor/rusty_anchor_t07_wine_bottle_two_glasses.png" },
            new() { itemId="rusty_anchor_t08", family="rusty_anchor", tier=7, displayName="Anchor Signature Cocktail", spritePath="Items/rusty_anchor/rusty_anchor_t08_signature_anchor_cocktail.png" },
            new() { itemId="rusty_anchor_t09", family="rusty_anchor", tier=8, displayName="Whiskey on Ice Premium",  spritePath="Items/rusty_anchor/rusty_anchor_t09_whiskey_on_ice_premium.png" },
            new() { itemId="rusty_anchor_t10", family="rusty_anchor", tier=9, displayName="50-Year Scotch",          spritePath="Items/rusty_anchor/rusty_anchor_t10_scotch_50yo_bottle.png" },

            // ── Corner Diner Generator (10 tiers) ──────────────────────────
            new() { itemId="corner_diner_t01", family="corner_diner", tier=0, displayName="Coffee Crate",        spritePath="Generators/corner_diner/corner_diner_t01_coffee_crate.png" },
            new() { itemId="corner_diner_t02", family="corner_diner", tier=1, displayName="Shoulder Tray",       spritePath="Generators/corner_diner/corner_diner_t02_shoulder_tray.png" },
            new() { itemId="corner_diner_t03", family="corner_diner", tier=2, displayName="Small Coffee Cart",   spritePath="Generators/corner_diner/corner_diner_t03_small_coffee_cart.png" },
            new() { itemId="corner_diner_t04", family="corner_diner", tier=3, displayName="Large Coffee Cart",   spritePath="Generators/corner_diner/corner_diner_t04_large_coffee_cart.png" },
            new() { itemId="corner_diner_t05", family="corner_diner", tier=4, displayName="Donut Shop",          spritePath="Generators/corner_diner/corner_diner_t05_donut_shop.png" },
            new() { itemId="corner_diner_t06", family="corner_diner", tier=5, displayName="Vendor Van",          spritePath="Generators/corner_diner/corner_diner_t06_vendor_van.png" },
            new() { itemId="corner_diner_t07", family="corner_diner", tier=6, displayName="Coffee Truck",        spritePath="Generators/corner_diner/corner_diner_t07_coffee_truck.png" },
            new() { itemId="corner_diner_t08", family="corner_diner", tier=7, displayName="Night Window",        spritePath="Generators/corner_diner/corner_diner_t08_night_window.png" },
            new() { itemId="corner_diner_t09", family="corner_diner", tier=8, displayName="Burger Shop Counter", spritePath="Generators/corner_diner/corner_diner_t09_burger_shop_counter.png" },
            new() { itemId="corner_diner_t10", family="corner_diner", tier=9, displayName="Fast Food Facade",    spritePath="Generators/corner_diner/corner_diner_t10_fast_food_facade.png" },

            // ── Investigation Lab Generator (10 tiers) ──────────────────────
            new() { itemId="investigation_lab_t01", family="investigation_lab", tier=0, displayName="Gloves",         spritePath="Generators/investigation_lab/gen_investigation_lab_t01_gloves.png" },
            new() { itemId="investigation_lab_t02", family="investigation_lab", tier=1, displayName="Mask",           spritePath="Generators/investigation_lab/gen_investigation_lab_t02_mask.png" },
            new() { itemId="investigation_lab_t03", family="investigation_lab", tier=2, displayName="Tubes",          spritePath="Generators/investigation_lab/gen_investigation_lab_t03_tubes.png" },
            new() { itemId="investigation_lab_t04", family="investigation_lab", tier=3, displayName="Lab Coat",       spritePath="Generators/investigation_lab/gen_investigation_lab_t04_coat.png" },
            new() { itemId="investigation_lab_t05", family="investigation_lab", tier=4, displayName="Microscope",     spritePath="Generators/investigation_lab/gen_investigation_lab_t05_microscope.png" },
            new() { itemId="investigation_lab_t06", family="investigation_lab", tier=5, displayName="Respirator",     spritePath="Generators/investigation_lab/gen_investigation_lab_t06_respirator.png" },
            new() { itemId="investigation_lab_t07", family="investigation_lab", tier=6, displayName="DNA Analysis",   spritePath="Generators/investigation_lab/gen_investigation_lab_t07_dna.png" },
            new() { itemId="investigation_lab_t08", family="investigation_lab", tier=7, displayName="Hazmat Suit",    spritePath="Generators/investigation_lab/gen_investigation_lab_t08_hazmat.png" },
            new() { itemId="investigation_lab_t09", family="investigation_lab", tier=8, displayName="Cleanroom Door", spritePath="Generators/investigation_lab/gen_investigation_lab_t09_door_cleanroom.png" },
            new() { itemId="investigation_lab_t10", family="investigation_lab", tier=9, displayName="Research Facility",spritePath="Generators/investigation_lab/gen_investigation_lab_t10_facility_helix.png" },

            // ── Junk Generator (10 tiers) ────────────────────────────────────
            new() { itemId="junk_t01", family="junk", tier=0, displayName="Drawer",    spritePath="Generators/junk/gen_junk_t01_drawer.png" },
            new() { itemId="junk_t02", family="junk", tier=1, displayName="Box",       spritePath="Generators/junk/gen_junk_t02_box.png" },
            new() { itemId="junk_t03", family="junk", tier=2, displayName="Cabinet",   spritePath="Generators/junk/gen_junk_t03_cabinet.png" },
            new() { itemId="junk_t04", family="junk", tier=3, displayName="Cupboard",  spritePath="Generators/junk/gen_junk_t04_cupboard.png" },
            new() { itemId="junk_t05", family="junk", tier=4, displayName="Chest",     spritePath="Generators/junk/gen_junk_t05_chest.png" },
            new() { itemId="junk_t06", family="junk", tier=5, displayName="Locker",    spritePath="Generators/junk/gen_junk_t06_locker.png" },
            new() { itemId="junk_t07", family="junk", tier=6, displayName="Wardrobe",  spritePath="Generators/junk/gen_junk_t07_wardrobe.png" },
            new() { itemId="junk_t08", family="junk", tier=7, displayName="Built-in",  spritePath="Generators/junk/gen_junk_t08_built_in.png" },
            new() { itemId="junk_t09", family="junk", tier=8, displayName="Shed",      spritePath="Generators/junk/gen_junk_t09_shed.png" },
            new() { itemId="junk_t10", family="junk", tier=9, displayName="Safe",      spritePath="Generators/junk/gen_junk_t10_safe.png" },

            // ── Cash Currency (5 tiers) ──────────────────────────────────────
            new() { itemId="currency_cash_t01", family="currency_cash", tier=0, displayName="Single Note",      spritePath="Currency/cash/currency_cash_usd_t01_note_single.png" },
            new() { itemId="currency_cash_t02", family="currency_cash", tier=1, displayName="Double Notes",     spritePath="Currency/cash/currency_cash_usd_t02_notes_double.png" },
            new() { itemId="currency_cash_t03", family="currency_cash", tier=2, displayName="Triple Notes",     spritePath="Currency/cash/currency_cash_usd_t03_notes_triple.png" },
            new() { itemId="currency_cash_t04", family="currency_cash", tier=3, displayName="Small Bundle",     spritePath="Currency/cash/currency_cash_usd_t04_bundle_small_band.png" },
            new() { itemId="currency_cash_t05", family="currency_cash", tier=4, displayName="Pile of Bundles",  spritePath="Currency/cash/currency_cash_usd_t05_pile_small_bundles.png" },

            // ── Platinum Currency (5 tiers) ──────────────────────────────────
            new() { itemId="currency_platinum_t01", family="currency_platinum", tier=0, displayName="Single Ingot",       spritePath="Currency/platinum/currency_platinum_t01_ingot_single.png" },
            new() { itemId="currency_platinum_t02", family="currency_platinum", tier=1, displayName="Double Ingots",      spritePath="Currency/platinum/currency_platinum_t02_ingots_double.png" },
            new() { itemId="currency_platinum_t03", family="currency_platinum", tier=2, displayName="Triple Ingots",      spritePath="Currency/platinum/currency_platinum_t03_ingots_triple.png" },
            new() { itemId="currency_platinum_t04", family="currency_platinum", tier=3, displayName="Small Ingot Pile",   spritePath="Currency/platinum/currency_platinum_t04_ingots_pile_small.png" },
            new() { itemId="currency_platinum_t05", family="currency_platinum", tier=4, displayName="Large Ingot Pile",   spritePath="Currency/platinum/currency_platinum_t05_ingots_pile_large.png" },
        };

        [MenuItem("Tools/AQ/Create All Item Definitions")]
        public static void CreateAll()
        {
            const string rootSpriteDir = "Assets/Art/Icons";
            const string outputDir     = "Assets/ScriptableObjects/Items";

            if (!AssetDatabase.IsValidFolder(outputDir))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Items");
            }

            int created = 0;
            int skipped = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var spec in _specs)
                {
                    string assetPath = $"{outputDir}/{spec.itemId}.asset";
                    if (AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(assetPath) != null)
                    {
                        skipped++;
                        continue;
                    }

                    var so = ScriptableObject.CreateInstance<ItemDefinitionSO>();
                    so.itemId      = spec.itemId;
                    so.family      = spec.family;
                    so.tier        = spec.tier;
                    so.displayName = spec.displayName;

                    string spriteFull = $"{rootSpriteDir}/{spec.spritePath}";
                    so.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFull);
                    if (so.icon == null)
                        Debug.LogWarning($"[ItemDefBatch] Sprite not found: {spriteFull}");

                    AssetDatabase.CreateAsset(so, assetPath);
                    created++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ItemDefBatch] Done — created {created}, skipped {skipped} (already existed).");
        }
    }
}
