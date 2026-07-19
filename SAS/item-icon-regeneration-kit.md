# Item-Icon Regeneration Kit — full board sweep (PRE-RELEASE)
*v1.4 · 2026-07-17 (audio item delivery status + total-scope arithmetic + transparency wording) · v1.3 2026-07-15 (Render Language Standard) · v1.2 2026-07-15, v1.0 2026-07-13 · Authoritative kit for the pre-beta board-art sweep. Supersedes the "Wave 1 = shipped, regen is post-launch" note in `art-kit-items-and-ui.md` — that call is reversed: **every item, generator, and currency icon is being regenerated now.***

```text
TOTAL PRE-BETA BOARD-ART SCOPE (recounted 2026-07-18 evening from repository state)
  77 in-place regenerations (93 − fingerprint 6 retired 2026-07-15 − corner_diner chain 10 retired 2026-07-18)
 +18 new assets: 6 audio items + 6 Field Kit tiers + 6 NEW corner_diner tiers — ALL DELIVERED+LIVE
    (gen_audio_rig 10 retired pre-integration; corner_diner ladder redesigned 10→6, see below)
  =91 unique deliverables (lab chain 10→6, ruling 2026-07-19) (before Press items and Ep-stamps, tracked in art-kit-items-and-ui.md)
DONE 85/91 (2026-07-19): currency 10 · gen_junk chain 10 · food_gifts 12 · rusty_anchor 10 · helens_gifts 10 · garage 10 · forensic 5 · audio items 6 · field kit 6 · corner_diner chain 6
REMAINING 6: investigation_lab chain 6 (NEW six-tier ladder — ART PENDING; mechanics already live on interim sprites)
```

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
margin on every side; nothing cropped. Fill the frame: maximum recognisable occupancy at EVERY tier (~90%+ content).
[SUPERSEDED 2026-07-19: the old 62-82% occupancy ladder is retired - icons now
occupy the entire grid square in-game (full-cell anchors + margin-trimmed
sprites); recognisability beats size progression. Tier value reads through
SUBJECT escalation, not scale.]
Fully transparent PNG background, straight (non-premultiplied) alpha, sRGB.
NO border, frame, cast shadow, floor shadow, drop shadow, ground contact shadow,
vignette, watermark, readable words, letters, numerals, denominations, dates,
barcodes or logos. Abstract marks and simple non-linguistic symbols are allowed.
Soft key from upper-left, restrained warm rim from the right. Chunky outer
silhouette; avoid fragile thin elements and excessive micro-detail. The icon
must remain identifiable at 96, 64 AND 48 pixels.
```

## Render Language Standard (whole art system · 2026-07-15)

One render language across the entire game: STYLIZED PAINTERLY ILLUSTRATION
for a mobile casual game, matching the approved character portraits and the
item-icon set. Rich painterly rendering, clean sculpted/simplified forms,
saturated local colour, soft cinematic lighting, deep noir values and
atmosphere — but explicitly NOT photorealism: no photographic micro-texture,
no lens/DoF/film-still realism, no hyperreal materials. Backgrounds, items,
generators, currency and portraits must read as ONE illustrated world, because
items sit on the backgrounds and character busts stand over them — a photoreal
backdrop makes the stylized foreground look pasted-on. Mood comes from LIGHTING
and VALUE (navy shadow, amber practicals, fog, contrast), NOT from photographic
detail. Anchor = the item icons + character portraits; backgrounds conform to
them, never the reverse. Acceptance test for any asset: could this sit behind
the character portraits and under the merge tiles and look like one game?

*The item-icon look defined above is the SYSTEM anchor that backgrounds and all other art now conform to. The NEW families (audio_investigation items + gen_audio_rig, Part 4) inherit this standard — generate them in the same illustration language.*

**The three-flaw kill-switch** (call these out to the generator every time):
- *no baked shadow of any kind* — the sentence is repeated twice in the block on purpose.
- *centre + 10-12% margin + 62-82% occupancy ladder* — fixes the size inconsistency.
- *"sitting inside a noir true-crime palette" + attached Ally key art* — fixes universe-fit.

**Tier-ladder rule:** every tier must differ from the tier below in **at least TWO** of: outer silhouette · object count · vertical height · footprint · premium material · family accent motif. Occupancy rises ~62%→82% low-to-hero.

**Batch discipline:** generate one family in ONE session; once the first icon is approved, attach it as an extra style anchor for the rest of that family so angle/lighting/finish stay locked. Output `.png`, 1024×1024, transparent.

---

## PROOF BATCH — run these FIVE and approve before any bulk generation
The stress test that exposes nearly every failure mode before you manufacture 82 consistent mistakes:
1. `food_gifts_t04_burger_single` — single hero-ish object, universe-fit + no-shadow check
2. `forensic_tools_t2_evidence_bag` — the alpha/transparency + no-halo torture test
3. `rusty_anchor_t06_champagne_flute` — thin-stem silhouette-collapse test
4. `helens_gifts_t01_note` — abstract-script / no-readable-text test
5. `gen_junk_t01_drawer` — generator-pedestal / storage-object test

Approve all five (checked at 96/64/48 px on a light tile, dark tile AND checkerboard) — *then* proceed family by family.

---

# PART 1 — ITEM FAMILIES (47 icons)

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
| rusty_anchor_t02_short_glass_ice.png | Glass of Iced Water *(RULED 2026-07-18 — subject changed from a whiskey-style tumbler to a tall clear glass of iced water; the tumbler read too close to T9's premium whiskey at board size. Filename retained, cosmetic)* | tall clear glass, ice cubes, water — unmistakably non-alcoholic |
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

# PART 2 — GENERATOR CHAINS
Generator chains get the same treatment as items — but each tier reads as a **fixture on a subtle base pedestal**, more massive/permanent than a loose item. The chain climbs from a humble source to a landmark. Same global block, plus "sits on a subtle base/pedestal; reads as a permanent installation, not a hand-held object." **Chains are NOT uniformly 10-tier: Field Kit 6 · Corner Diner 6 · investigation_lab 6 (2026-07-19) · gen_junk 10.**

### corner_diner · **✅ SIX-TIER LADDER DELIVERED + LIVE 2026-07-18 (df35827 — the old 10-tier ladder is RETIRED)**
*Ruling: the ten-tier version had duplicate silhouettes (two carts; van vs truck), a backwards donut-shop→van step, and an unreachable hero (T10 = 512 T1 pieces ≈ 82,944 taps at the ~1/162 sub-gen rate; T6 = 32 pieces ≈ 5,184 — still long-term, actually reachable). Progression: supplies → preparation → street service → mobile kitchen → fixed service point → full location. Art direction (recorded): warm diner red + cream enamel, dark navy accents, brass/chrome hardware, soft upper-left top light + restrained warm rim, ~15° above front three-quarter, transparent bg, no cast shadow/plate, no readable text/menus/numerals; each tier changes ≥2 of silhouette/footprint/mobility/height/mass/architectural scale.*
| Filename | Item | Status |
|---|---|---|
| corner_diner_t01_coffee_supply_crate.png | Coffee Supply Crate | ✅ live |
| corner_diner_t02_countertop_coffee_station.png | Countertop Coffee Station | ✅ live |
| corner_diner_t03_coffee_cart.png | Coffee Cart | ✅ live |
| corner_diner_t04_diner_food_truck.png | Diner Food Truck | ✅ live |
| corner_diner_t05_night_service_window.png | Night Service Window | ✅ live |
| corner_diner_t06_corner_diner_facade.png | Corner Diner | ✅ live — **chain hero** |

### investigation_lab · **SIX-TIER LADDER — ✅ ALL SIX DELIVERED + LIVE 2026-07-19 (commits 7dfcda4 art / T1 swap below)**
*Ruling: the ten-tier ladder spent five tiers on PPE (gloves/mask/coat/respirator/hazmat) that neither read as a growing lab nor separated from the forensic items it produces; T10 needed 512 T1 pieces ≈ 79,360 lab taps at the 1/155 sub-gen rate (T6 = 32 ≈ 4,960 — long-term and reachable). Progression: sample preparation → mobile workspace → permanent bench → analysis → controlled environment → complete facility. **Live build: mechanics are six-tier (maxGeneratorTier 5, t07–t10 SOs deleted, save clamp covers legacy tiers); ALL SIX finals delivered transparent-native and live 2026-07-19 (pixel-only overwrites, GUIDs kept; T1 file renamed to its true subject — T2–T6 sprite FILENAMES still carry the old PPE slugs (mask/tubes/coat/microscope/respirator), cosmetic only, out of scope for the T1 swap).** Import target: `Assets/Art/Icons/Generators/investigation_lab/` · 1024×1024 · straight alpha · same minimal-meta import as other generator chains. Art direction: steel grey + clinical cream, dark navy structure, restrained teal illumination, brass/amber status accents; ~15° above front three-quarter; occupancy ~58%→82%; fixture-like, distinct silhouettes at 48–64px; NO PPE, hospital symbolism, tube clutter, readable text or detailed screen UI; avoid overlap with forensic_tools items.*
**T1 CONCEPT SWAP (ruling 2026-07-19, supersedes the row below's original spec):** T1 = **Lab Supply Cupboard** (`gen_investigation_lab_t01_lab_supply_cupboard.png`, live). The Sample Prep Tray was REJECTED — it read as a collection of produced items, not a replenishing source; a benchtop-centrifuge alternative was also tested and REJECTED (unrecognisable to the casual audience at 48px, read as a domestic appliance). The cupboard uses standard merge-source language: narrow cream-enamel/aged-navy cupboard, glass door visibly open (accessible, replenishable), brass hardware, two lit shelves — upper: two forensic dusting brushes upright in a navy holder; lower: three empty fanned evidence bags with navy closures and red/white bands. The visible supplies are representative cues for the Forensic Tools output family only — no drop-table, probability, or item-art implication. Canonical ladder: **Lab Supply Cupboard → Lab Trolley → Lab Workbench → Analysis Station → Cleanroom Airlock → Research Facility.**

| Filename | Item | Art note |
|---|---|---|
| gen_investigation_lab_t01_lab_supply_cupboard.png | Lab Supply Cupboard | ✅ live — see T1 concept-swap ruling above *(original spec, superseded: sample prep tray — shallow steel tray, tube rack + reagent bottles)* |
| gen_investigation_lab_t02_lab_trolley.png | Lab Trolley | compact two-shelf clinical trolley on visible wheels, closed sample bin + one small rack — mobile infrastructure |
| gen_investigation_lab_t03_lab_workbench.png | Lab Workbench | permanent steel-and-cream bench: drawers, task lamp, organised rack; wide fixed silhouette |
| gen_investigation_lab_t04_analysis_station.png | Analysis Station | large microscope + compact monitor + control unit as ONE fixture; monitor shows abstract teal bars only |
| gen_investigation_lab_t05_cleanroom_airlock.png | Cleanroom Airlock | standalone sealed doorway + frame, viewing panel, side control box, teal/amber status lights; architectural silhouette |
| gen_investigation_lab_t06_research_facility.png | Research Facility | compact stylised research-building façade, lit lab windows, restrained abstract helix motif — **family hero** |

*(retired 10-tier PPE ladder: gloves → mask → tubes → coat → microscope → respirator → dna → hazmat → cleanroom door → facility helix — superseded, do not generate)*

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

---

# PART 4 — NEW FAMILY: Audio Investigation (6 items + 10-tier generator chain)
*Added v1.2 2026-07-15 — Stephen-ruled: Audio Investigation replaces the retired fingerprint_evidence AND becomes the game's OPENING family, putting Ally's podcast identity on the board from minute one. "Audio Investigation finds the story. Forensic Tools makes it evidence." These are NEW files (create at the paths below), not overwrites. Generation may use a temporary flat background if the tool demands it, but **final delivery must be genuine straight-alpha transparency** (the global spec's technical acceptance is absolute; a flat background is not an acceptable final asset).*

**STATUS 2026-07-18: FULLY LIVE (commit 46b71a6).** The 6 item icons are imported, scene-wired, and required by L1/L2/L3/L5/L12. The generator side shipped as the six-tier **Field Kit** (`gen_field_kit` — see below; the 10-tier gen_audio_rig spec that used to sit here was RETIRED pre-integration). **audio_investigation_t01 RULED FINAL 2026-07-19:** the spec'd earbuds-in-case read as CUFFLINKS at low res — bare recognisable earbuds are the shipped canon (the Part 4 t01 art note below is superseded).

### audio_investigation items (6) · palette: warm charcoal, chrome, brass, amber VU-meter glow · `Assets/Art/Icons/Items/audio_investigation/`
*The ladder climbs from LISTENING to RECORDING to BROADCASTING to ANALYSING — each tier looks more professional and more coveted than the last. Match the gear already canonized in the BG-1 studio master (chrome broadcast mic, mixing desk, cassette deck): attach EP1-BG1.png as a prop-style reference alongside the Ally key art.*
| Filename | Item | Art note |
|---|---|---|
| audio_investigation_t01_earbuds_case.png | Earbuds in Case | small consumer earbuds coiled in an open pocket case — the rawest listening tier, chunky case so it survives 48px |
| audio_investigation_t02_studio_headphones.png | Studio Headphones | closed-back retro studio cans, coiled cable, worn leather headband — echoes Ally's canon neck-worn pair |
| audio_investigation_t03_recorder_headphones.png | Recorder & Headphones | handheld cassette recorder with small headphones hooked over it — the first working field kit; one silhouette, recorder dominant |
| audio_investigation_t04_broadcast_mic_rig.png | Broadcast Microphone Rig | the chrome broadcast mic (Ally's canon prop) on a desk arm with pop filter — tall, premium podcast silhouette |
| audio_investigation_t05_mixing_console.png | Audio Mixing Console | compact analogue mixing desk, rows of knobs and faders, two amber VU meters glowing — no readable labels |
| audio_investigation_t06_audio_workstation.png | Forensic Audio Workstation | monitor with abstract teal waveform + console + mic as ONE tight cluster — **family hero**, the full analysis rig people covet |

### ~~gen_audio_rig generator chain (10)~~ · **RETIRED PRE-INTEGRATION 2026-07-18 — DO NOT GENERATE**
> **Design ruling: the recorders/receivers/broadcast desks below read too close to the Audio Investigation ITEMS the generator produces.** Shipped instead: the six-tier **Field Kit** (`gen_field_kit`, `Assets/Art/Icons/Generators/field_kit/`) — Utility Pouch → Investigator's Satchel → Field Backpack → Hard Equipment Case → Rolling Gear Trunk → Mobile Field Unit (all 6 delivered as straight-alpha finals and LIVE, commit 46b71a6). The Field Kit is how Ally CARRIES equipment (items listen; forensic proves) and is deliberately generic enough to host additional portable families later — audio-only in Ep1. The table below is preserved for the archive only.

*(archived spec below — do not generate)*
| Filename | Item | Art note |
|---|---|---|
| gen_audio_rig_t01_cassette_box.png | Cassette Box | a worn shoebox of mixed cassettes, one tape propped against it — humblest source (Dot's tapes) |
| gen_audio_rig_t02_tape_crate.png | Tape Crate | wooden crate of reels and tape spines, abstract label strips |
| gen_audio_rig_t03_answering_machine.png | Answering Machine | the tip-line answering machine, one amber message light glowing — canon L1 prop |
| gen_audio_rig_t04_reel_recorder.png | Reel Recorder | tabletop reel-to-reel machine, twin reels, brass trim |
| gen_audio_rig_t05_radio_receiver.png | Radio Receiver | vintage receiver/tuner stack with warm dial glow |
| gen_audio_rig_t06_broadcast_desk.png | Broadcast Desk | small desk unit: mic boom + mixer face + lamp — a workstation becoming permanent |
| gen_audio_rig_t07_studio_booth.png | Studio Booth | corner acoustic booth with foam panels and a hanging mic — room-scale now |
| gen_audio_rig_t08_broadcast_rack.png | Broadcast Rack | full equipment rack tower, stacked units, amber meters |
| gen_audio_rig_t09_radio_mast.png | Radio Mast | rooftop mast/antenna unit on a brick chimney base, signal lamp lit |
| gen_audio_rig_t10_radio_station.png | Radio Station | attic-studio-turned-station facade, lit dormer + mast — **chain hero** ("Echoes of Havenbay" made physical, no readable words) |

**Integration plan (code/data side, after art lands — one session):** create ItemDefinitionSOs (`audio_investigation_t1..t6`) + `GeneratorType_AudioRig` SO; the OPENING default generator becomes the audio rig producing audio_investigation items; the Investigation Lab (forensic source) moves to a granted/later introduction. Requirement re-flavor per the episode logic — L1/L2/L3/L5 → audio items, L6/L9/L10 stay forensic — needs a fresh economy check of the golden-path T1-equivalents before committing. **Press family amendment now locked: press_items_t04 must change from "Cassette Recorder & Tape" to a newsroom telephone + message pad** (see art-kit-items-and-ui.md) so the audio family owns all tape/recorder silhouettes.

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
- **`fingerprint_evidence` — RETIRED 2026-07-15.** Removed from the gen_investigation_lab drop table; every lead requirement (Ep1 + shelved Ghost Student + cold cases) remapped to same-tier forensic_tools. Its replacement is **Audio Investigation — CHOSEN and signed off** (requirement map ruled 2026-07-17; no longer a "front-runner"). Do NOT generate fingerprint art.
- **`press` family** — referenced by the `gen_junk` drop-table but has no ScriptableObjects and no art; it's the Episode 2 dependency (Batch F in `art-kit-items-and-ui.md`), generated separately when Ep2 content lands.
- **`stakeout_fuel`** — a legacy alias that reuses `food_gifts` art; regenerating food_gifts refreshes it automatically. No separate generation.
- **Legacy duplicates under `Assets/Art/UI/Icons/MergeChains/`** — superseded by `Assets/Art/Icons/Items/`; not on the live board, leave alone.
- **The Ep2 branch stamps and Press items** — flat-UI-symbol / Wave-2 work, still tracked in `art-kit-items-and-ui.md`.

## Recommended generation order (biggest visible win first)
1. **food_gifts** (12) — the most-merged family and the first tiles a player touches. Run the proof batch here, lock the look.
2. **rusty_anchor** (10), **garage** (10), **helens_gifts** (10) — the other big item ladders.
3. ~~audio_investigation items (6) + generator chain~~ **✅ ALL DONE: audio items live + six-tier Field Kit live (46b71a6); t01 bare-earbuds RULED FINAL 2026-07-19 (case read as cufflinks at low res). Nothing here blocks anything.**
4. **forensic_tools** (5) — the investigation items.
5. **junk / investigation_lab / corner_diner** generator chains (30).
6. **currency** cash + platinum (10). *(already done)*

That order means the board looks fixed for the earliest-game tiles almost immediately, and any style adjustment you discover in family #1 propagates cheaply to the rest.
