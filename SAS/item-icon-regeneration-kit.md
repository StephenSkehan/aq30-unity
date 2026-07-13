# Item-Icon Regeneration Kit — full board sweep (PRE-RELEASE)
*v1.0 · 2026-07-13 · Authoritative kit for regenerating **all 93 on-board icons** before the Aug 8 beta. Supersedes the "Wave 1 = shipped, regen is post-launch" note in `art-kit-items-and-ui.md` — that call is reversed: **every item, generator, and currency icon is being regenerated now.***

## Why this exists
The current on-board icons predate the v1.3 style spec and share three flaws (observed live):
1. **Baked drop shadows** — the runtime already casts a tile shadow, so a shadow painted into the PNG double-shadows and reads wrong.
2. **Inconsistent size** — icons vary in framing/scale tile-to-tile, so the value ladder (small early tier → big hero tier) is muddy.
3. **Off-universe** — colour/lighting/finish don't sit inside the Ally Quinn noir palette.

Each maps 1:1 to a rule in the **Global item style block** below. The fix is a straight regeneration against that block. Nothing about the game's data changes.

## The drop-in principle (READ THIS — it makes the whole sweep low-risk)
**Keep every filename byte-for-byte identical to the list in this doc.** When a new PNG overwrites the old one at the same path, Unity keeps the existing `.meta` / GUID, so every `ItemDefinitionSO.icon` reference stays wired. This sweep is therefore:

> generate → **overwrite the exact same filename** → let Unity reimport (GUID unchanged) → commit via Git LFS.

No `ItemDefinitionBatchCreator` edits, no re-wiring, no ScriptableObject churn. **Do NOT rename anything** — the filename inconsistencies (`t1` vs `t01`, `gen_` prefixes, `_usd`) are load-bearing; leave them exactly as written here.

---

## Global item style block (paste with EVERY item/generator/currency batch)
*Attach the Ally key art as PALETTE / LIGHTING reference (not a geometry reference) with every generation.*

```
Match the approved Ally Quinn item-icon reference style: stylized painterly
3D illustration, rich saturated local colour, clean sculpted forms and soft
cinematic lighting, sitting inside a noir true-crime palette (deep navy shadow,
warm amber key, restrained teal accents).
Render ONE COHESIVE ICON COMPOSITION: either one object OR a tightly grouped
cluster that reads as one silhouette. No scenery, room, tabletop or environmental
background.
View from approximately 15 degrees above. Keep the same camera direction and
ground plane across the entire family. Centre the composition with 10-12% safe
margin on every side; nothing cropped. Overall occupancy may increase gradually
from low tier to hero tier, approximately 62-82% (do NOT fill every tier to the
same size - that flattens the value ladder).
Fully transparent PNG background, straight (non-premultiplied) alpha, sRGB.
NO border, frame, cast shadow, floor shadow, drop shadow, ground contact shadow,
vignette, watermark, readable words, letters, numerals, denominations, dates,
barcodes or logos. Abstract marks and simple non-linguistic symbols are allowed.
Soft key from upper-left, restrained warm rim from the right. Chunky outer
silhouette; avoid fragile thin elements and excessive micro-detail. The icon
must remain identifiable at 96, 64 AND 48 pixels.
```

**The three-flaw kill-switch** (call these out to the generator every time):
- *no baked shadow of any kind* — the sentence is repeated twice in the block on purpose.
- *centre + 10-12% margin + 62-82% occupancy ladder* — fixes the size inconsistency.
- *"sitting inside a noir true-crime palette" + attached Ally key art* — fixes universe-fit.

**Tier-ladder rule:** every tier must differ from the tier below in **at least TWO** of: outer silhouette · object count · vertical height · footprint · premium material · family accent motif. Occupancy rises ~62%→82% low-to-hero.

**Batch discipline:** generate one family in ONE session; once the first icon is approved, attach it as an extra style anchor for the rest of that family so angle/lighting/finish stay locked. Output `.png`, 1024×1024, transparent.

---

## PROOF BATCH — run these FIVE and approve before any bulk generation
The stress test that exposes nearly every failure mode before you manufacture 88 consistent mistakes:
1. `food_gifts_t04_burger_single` — single hero-ish object, universe-fit + no-shadow check
2. `forensic_tools_t2_evidence_bag` — the alpha/transparency + no-halo torture test
3. `rusty_anchor_t06_champagne_flute` — thin-stem silhouette-collapse test
4. `helens_gifts_t01_note` — abstract-script / no-readable-text test
5. `gen_junk_t01_drawer` — generator-pedestal / storage-object test

Approve all five (checked at 96/64/48 px on a light tile, dark tile AND checkerboard) — *then* proceed family by family.

---

# PART 1 — ITEM FAMILIES (53 icons)

### food_gifts (12) · palette: warm diner reds/creams, appetizing · `Assets/Art/Icons/Items/food_gifts/`
| Filename | Item | Art note |
|---|---|---|
| food_gifts_t01_paper_cup.png | Paper Cup | plain takeaway cup, simple sleeve — the rawest tier |
| food_gifts_t02_hot_coffee_cup.png | Hot Coffee Cup | diner ceramic mug, single steam curl |
| food_gifts_t03_coffee_and_donut.png | Coffee and Donut | cup + glazed donut, classic pairing |
| food_gifts_t04_burger_single.png | Burger Single | one stacked dressed burger, glossy bun |
| food_gifts_t05_burger_fries_drink.png | Burger, Fries & Drink | combo cluster reading as one silhouette |
| food_gifts_t06_takeaway_caddy.png | Takeaway Caddy | cardboard caddy, several items peeking |
| food_gifts_t07_pasta_bowl.png | Pasta Bowl | twirled pasta, herb flecks |
| food_gifts_t08_sushi_bento.png | Sushi Bento | tidy compartmented bento box |
| food_gifts_t09_ice_cream_sundae_luxury.png | Ice Cream Sundae Luxury | tall glass, layered scoops, cherry |
| food_gifts_t10_steak_plate.png | Steak Plate | steak with char lines + sides |
| food_gifts_t11_surf_and_turf_wine.png | Surf and Turf with Wine | steak + lobster tail + wine glass cluster |
| food_gifts_t12_lobster_champagne_banquet.png | Lobster Champagne Banquet | whole lobster + champagne — **family hero** |

### rusty_anchor (10) · palette: amber liquids, brass, warm bar light, dark-wood hints · `Assets/Art/Icons/Items/rusty_anchor/`
| Filename | Item | Art note |
|---|---|---|
| rusty_anchor_t01_shot_glass_empty.png | Shot Glass | simple glass, a catch of amber light |
| rusty_anchor_t02_short_glass_ice.png | Short Glass on Ice | tumbler, two ice cubes, condensation |
| rusty_anchor_t03_tall_glass_ice_orange.png | Tall Glass Orange | highball, orange slice on rim |
| rusty_anchor_t04_beer_bottle.png | Beer Bottle | brown bottle, abstract anchor motif on label (no words) |
| rusty_anchor_t05_wine_glass_red.png | Wine Glass Red | red wine, elegant stem |
| rusty_anchor_t06_champagne_flute.png | Champagne Flute | rising bubbles — keep stem chunky enough to survive 48px |
| rusty_anchor_t07_wine_bottle_two_glasses.png | Wine Bottle & Glasses | bottle + two glasses grouped |
| rusty_anchor_t08_signature_anchor_cocktail.png | Anchor Signature Cocktail | showy layered cocktail, tiny anchor pick |
| rusty_anchor_t09_whiskey_on_ice_premium.png | Whiskey on Ice Premium | crystal tumbler, large sphere ice, deep amber |
| rusty_anchor_t10_scotch_50yo_bottle.png | 50-Year Scotch | aged bottle, thick wax seal with five radial marks (no numeral), brass — **family hero** |

### garage (10) · palette: gun-metal, chrome, motor-oil amber, worn workshop warmth · `Assets/Art/Icons/Items/garage/`
*A hot-rod build ladder: bare tool → finished custom car.*
| Filename | Item | Art note |
|---|---|---|
| garage_t01_socket_wrench_chrome.png | Socket Wrench | single chrome socket wrench, angled — rawest tier |
| garage_t02_oil_can.png | Oil Can | classic long-spout oil can, amber sheen |
| garage_t03_shock_absorbers_pair.png | Shock Absorbers | a pair of coil-over shocks, coils catching light |
| garage_t04_tyres_pile.png | Tyres | a small stack of performance tyres |
| garage_t05_spark_plug.png | Spark Plug | a **boxed set of four** iridium plugs (not one tiny plug — needs mass to beat t04) |
| garage_t06_car_battery.png | Car Battery | heavy performance battery, brass terminals |
| garage_t07_chrome_grille.png | Chrome Grille | a gleaming chrome front grille |
| garage_t08_custom_exhaust.png | Custom Exhaust | chrome twin custom exhaust / headers |
| garage_t09_big_block_engine_chrome_extractors.png | Big Block Engine | chromed big-block engine with extractors |
| garage_t10_pimped_ride_hood_up_chrome_engine.png | Pimped Ride | finished hot rod, hood up, chrome engine — **family hero** |

### helens_gifts (10) · palette: soft warm sentimental, muted florals + keepsake metals · `Assets/Art/Icons/Items/helens_gifts/`
*A courtship/keepsake ladder. Different object types per tier, so silhouettes differ naturally — still ramp occupancy low→high.*
| Filename | Item | Art note |
|---|---|---|
| helens_gifts_t01_note.png | Note | a folded handwritten note tied with twine — **abstract script strokes only, no readable words** |
| helens_gifts_t02_daisy_posie.png | Daisy Posy | a small tied posy of daisies |
| helens_gifts_t03_bouquet.png | Bouquet | a fuller wrapped bouquet, ribbon |
| helens_gifts_t04_cookies.png | Cookies | a small plate/box of cookies |
| helens_gifts_t05_chocolate.png | Chocolate | a ribboned box of chocolates, lid ajar |
| helens_gifts_t06_dessert.png | Dessert | an elegant plated dessert |
| helens_gifts_t07_boots.png | Boots | a pair of stylish boots |
| helens_gifts_t08_scarf.png | Scarf | a folded silk scarf, soft sheen |
| helens_gifts_t09_perfume.png | Perfume | an elegant perfume bottle, warm glass |
| helens_gifts_t10_locket.png | Locket | an opened heart locket on a fine chain — **family hero**, sentimental |

### fingerprint_evidence (6) · palette: ink navy, white card stock, silver · `Assets/Art/Icons/Items/fingerprint_evidence/`
*Note the single-digit `t1`–`t6` filenames.*
| Filename | Item | Art note |
|---|---|---|
| fingerprint_evidence_t1_partial_dusted_print.png | Partial Dusted Print | dusting brush over a half-revealed print on a dark surface |
| fingerprint_evidence_t2_lifted_print_tape.png | Lifted Print Tape | clear tape strip holding a print, curling slightly |
| fingerprint_evidence_t3_fingerprint_card.png | Fingerprint Card | classic ten-print card, inked prints (no readable text) |
| fingerprint_evidence_t4_labeled_prints.png | Labeled Prints | fanned trio of tagged cards (red/blue/green tabs, no words) |
| fingerprint_evidence_t5_digital_scan_in_progress.png | Digital Scan In Progress | handheld scanner, print mid-scan, cyan scanline |
| fingerprint_evidence_t6_database_match.png | Database Match | tablet showing twin aligned prints joined by a green check / linking brackets (no MATCH text); bold simplified spirals — **family hero** |

### forensic_tools (5) · palette: steel grey, teal case-accents, clinical white · `Assets/Art/Icons/Items/forensic_tools/`
*Single-digit `t1`–`t5`; note `t3` and `t5` have long filenames — keep them exact.*
| Filename | Item | Art note |
|---|---|---|
| forensic_tools_t1_cotton_swab.png | **Dusting Brush** | a single fingerprint dusting brush — chunky round head, angled handle, **no bag**; smallest footprint in the family. *(Swapped from cotton swab 2026-07-13: a dusting brush fits the fingerprint-themed family and reads sturdier at 48px. Keep the filename as-is for GUID stability; update the ScriptableObject `displayName` "Cotton Swab" → "Dusting Brush" — see note below.)* |
| forensic_tools_t2_evidence_bag.png | Evidence Bag | zip bag with a strong OPAQUE zip rim + folds (controlled, not full transparency, so it survives on alpha), red tamper strip of diagonal bars (no letters), something indistinct inside |
| forensic_tools_t3_full_forensic_case_black.png | Full Forensic Case | small hard **closed** case, latched lid, sturdy corner guards + handle — deliberately shut so it reads distinctly from the open hero kit at t5 |
| forensic_tools_t4_uv_light.png | UV Light | handheld UV torch, violet glow at the lens |
| forensic_tools_t5_complete_forensic_kit_transparent_soft.png | Complete Forensic Kit | premium large **open** case, foam-fit gleaming instruments (print card + dusting brush + UV light) — **family hero** |

> **⚠ t1 displayName follow-on (data, not art):** the art for `forensic_tools_t1` is now a dusting brush, but its `displayName` in `Assets/ScriptableObjects/Items/forensic_tools_t1.asset` (and the source line in `Assets/Editor/Items/ItemDefinitionBatchCreator.cs`) still reads **"Cotton Swab"**. Change it to **"Dusting Brush"** so the tile-info popup matches the icon. **Do NOT** rename the `.asset`, the `itemId` (`forensic_tools_t1`), or the PNG filename — only the `displayName` string changes. Pure one-line edit; GUIDs and wiring are untouched.

---

# PART 2 — GENERATOR CHAINS (30 icons)
Generators are **also 10-tier merge chains** on the board, so they get the same treatment — but each tier reads as a **fixture on a subtle base pedestal**, more massive/permanent than a loose item. The chain climbs from a humble source to a landmark. Same global block, plus "sits on a subtle base/pedestal; reads as a permanent installation, not a hand-held object."

### corner_diner (10) · the food source · palette: warm diner reds/creams (matches food_gifts) · `Assets/Art/Icons/Generators/corner_diner/`
| Filename | Item | Art note |
|---|---|---|
| corner_diner_t01_coffee_crate.png | Coffee Crate | a wooden crate of coffee supplies — humblest source |
| corner_diner_t02_shoulder_tray.png | Shoulder Tray | a vendor's shoulder tray of cups |
| corner_diner_t03_small_coffee_cart.png | Small Coffee Cart | a little two-wheel coffee cart |
| corner_diner_t04_large_coffee_cart.png | Large Coffee Cart | a bigger cart with awning |
| corner_diner_t05_donut_shop.png | Donut Shop | a small donut counter |
| corner_diner_t06_vendor_van.png | Vendor Van | a food vendor van |
| corner_diner_t07_coffee_truck.png | Coffee Truck | a larger coffee truck |
| corner_diner_t08_night_window.png | Night Window | a lit takeaway night window; dark menu board bears cream horizontal menu strokes (no readable words) |
| corner_diner_t09_burger_shop_counter.png | Burger Shop Counter | a burger counter with griddle glow |
| corner_diner_t10_fast_food_facade.png | Fast Food Facade | a full lit diner facade — **chain hero** |

### investigation_lab (10) · the forensic source · palette: steel grey, teal, clinical white · `Assets/Art/Icons/Generators/investigation_lab/`
*Filenames carry the `gen_investigation_lab_` prefix — keep exact.*
| Filename | Item | Art note |
|---|---|---|
| gen_investigation_lab_t01_gloves.png | Gloves | a pair of nitrile gloves — humblest source |
| gen_investigation_lab_t02_mask.png | Mask | a surgical mask |
| gen_investigation_lab_t03_tubes.png | Tubes | a rack of sample test tubes |
| gen_investigation_lab_t04_coat.png | Lab Coat | a hung/folded lab coat |
| gen_investigation_lab_t05_microscope.png | Microscope | a lab microscope |
| gen_investigation_lab_t06_respirator.png | Respirator | a half-face respirator |
| gen_investigation_lab_t07_dna.png | DNA Analysis | a gel/helix readout device, teal glow |
| gen_investigation_lab_t08_hazmat.png | Hazmat Suit | a hazmat suit |
| gen_investigation_lab_t09_door_cleanroom.png | Cleanroom Door | a sealed cleanroom door, edge light |
| gen_investigation_lab_t10_facility_helix.png | Research Facility | a facility with a helix motif — **chain hero** |

### junk (10) · the "old evidence drawer" storage source · palette: worn wood, brass, dust, dim warm · `Assets/Art/Icons/Generators/junk/`
*Filenames carry the `gen_junk_` prefix — keep exact. Escalating storage furniture.*
| Filename | Item | Art note |
|---|---|---|
| gen_junk_t01_drawer.png | Drawer | a single worn wooden drawer — humblest source |
| gen_junk_t02_box.png | Box | a battered box, lid ajar |
| gen_junk_t03_cabinet.png | Cabinet | a small cabinet |
| gen_junk_t04_cupboard.png | Cupboard | a taller cupboard |
| gen_junk_t05_chest.png | Chest | a wooden chest, brass fittings |
| gen_junk_t06_locker.png | Locker | a metal locker |
| gen_junk_t07_wardrobe.png | Wardrobe | a large wardrobe |
| gen_junk_t08_built_in.png | Built-in | a built-in shelving/cabinet unit |
| gen_junk_t09_shed.png | Shed | a small storage shed |
| gen_junk_t10_safe.png | Safe | a heavy floor safe, brass dial — **chain hero** |

---

# PART 3 — CURRENCY CHAINS (10 icons)
Currency families are on-board merge tiles too. Same global block. **Hard rule: NO readable numerals or denominations** — cash is abstract engraving/detail only; value is read from the pile size, never printed.

### currency_cash (5) · palette: worn noir-green banknotes, ink detail · `Assets/Art/Icons/Currency/cash/`
*Filenames carry the `currency_cash_usd_` segment — keep exact.*
| Filename | Item | Art note |
|---|---|---|
| currency_cash_usd_t01_note_single.png | Single Note | one worn folded banknote, abstract engraving (no numerals/words) |
| currency_cash_usd_t02_notes_double.png | Double Notes | two notes fanned |
| currency_cash_usd_t03_notes_triple.png | Triple Notes | three notes fanned |
| currency_cash_usd_t04_bundle_small_band.png | Small Bundle | a single banded bundle |
| currency_cash_usd_t05_pile_small_bundles.png | Pile of Bundles | a small pile of banded bundles — **hero** |

### currency_platinum (5) · palette: cool silver-white, premium sheen · `Assets/Art/Icons/Currency/platinum/`
| Filename | Item | Art note |
|---|---|---|
| currency_platinum_t01_ingot_single.png | Single Ingot | one platinum ingot, cool rim light |
| currency_platinum_t02_ingots_double.png | Double Ingots | two stacked ingots |
| currency_platinum_t03_ingots_triple.png | Triple Ingots | three stacked in a small pyramid |
| currency_platinum_t04_ingots_pile_small.png | Small Ingot Pile | a small pile of ingots |
| currency_platinum_t05_ingots_pile_large.png | Large Ingot Pile | a large gleaming pile — **hero** |

---

## Technical acceptance & QC (every asset)
- PNG, **straight (non-premultiplied) alpha**, **sRGB**.
- **No** opaque matte, **no** white/black edge halo, no semi-transparent noise outside the shape.
- ≥ **10–12% safe margin**; silhouette never cropped; centre pivot.
- **No baked shadow of any kind** (runtime adds the tile shadow).
- Reads at **96 / 64 / 48 px**; approved on a contact sheet over **light tile, dark tile AND checkerboard**.
- Currency/stamps: survives **greyscale** (meaning must not depend on colour alone).

## Import & commit workflow (per family)
1. Generate the family (proof batch first for the pilot); approve at 96/64/48 px.
2. **Overwrite the existing PNGs in place** at the exact paths above — do NOT rename, do NOT move, do NOT create new `.meta` files. Unity reimports and keeps the GUID.
3. In Unity confirm the importer still reads **Sprite (2D and UI)**, mipmaps OFF, alpha ON (these are already set on the current `.meta`; overwriting the PNG doesn't disturb them).
4. Play the board — the tiles should now be shadow-free, consistently framed, and on-palette. Capture a before/after via `AQ ▸ Dev ▸ Capture Game View Screenshot`.
5. Commit through **Git LFS** (the PNGs are LFS-tracked via `.gitattributes`) — one commit per family keeps the history legible and LFS diffs small.

## Asset manifest (fill as you approve — reproducibility)
Track per approved icon: `filename · this kit v1.0 · reference images used · generation ID · approval date · Unity reimport confirmed (GUID unchanged Y/N)`. A tiny table is enough; it lets any re-roll match the family.

## Out of scope for this sweep (noted, not touched)
- **`press` family** — referenced by the `gen_junk` drop-table but has no ScriptableObjects and no art; it's the Episode 2 dependency (Batch F in `art-kit-items-and-ui.md`), generated separately when Ep2 content lands.
- **`stakeout_fuel`** — a legacy alias that reuses `food_gifts` art; regenerating food_gifts refreshes it automatically. No separate generation.
- **Legacy duplicates under `Assets/Art/UI/Icons/MergeChains/`** — superseded by `Assets/Art/Icons/Items/`; not on the live board, leave alone.
- **The Ep2 branch stamps and Press items** — flat-UI-symbol / Wave-2 work, still tracked in `art-kit-items-and-ui.md`.

## Recommended generation order (biggest visible win first)
1. **food_gifts** (12) — the most-merged family and the first tiles a player touches. Run the proof batch here, lock the look.
2. **rusty_anchor** (10), **garage** (10), **helens_gifts** (10) — the other big item ladders.
3. **fingerprint_evidence** (6), **forensic_tools** (5) — the investigation items.
4. **junk / investigation_lab / corner_diner** generator chains (30).
5. **currency** cash + platinum (10).

That order means the board looks fixed for the earliest-game tiles almost immediately, and any style adjustment you discover in family #1 propagates cheaply to the rest.
