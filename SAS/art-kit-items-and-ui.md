# Art Generation Kit — Item Icons & UI Sprites
*v1.3 · 2026-07-12 (contradiction pass: single-object→cluster, no-text absolute, occupancy/tier rule, generator↔family map, separate UI-symbol block, technical QC, self-contained bg) · Attach the Ally key art as PALETTE/LIGHTING reference — not a geometry reference — with every item generation.*

> **⚠ STATUS REVERSED 2026-07-13 — the whole board is being regenerated PRE-RELEASE.** The earlier note here ("Wave 1 is shipped; regen is post-launch polish") is **superseded.** The live board icons predate the v1.3 spec and carry three flaws (baked drop-shadows, inconsistent size, off-universe), so **all 93 on-board icons — 53 items + 30 generator-chain + 10 currency — are being regenerated before the Aug 8 beta.** The authoritative, filename-exact kit for that sweep is **`SAS/item-icon-regeneration-kit.md`** — use it for every item/generator/currency batch. The material below stays valid as reference (the global block, the four detailed item families, the QC/manifest/proof batch), but the *count and scope* live in the regen kit. The **Wave 2 (Press), Ep2 stamps, and Ep1 stamps** work below is unchanged.

> **Reference-block index:** items use the **item-icon block** (below); flat UI marks (stamps) use the separate **flat UI-symbol block** (§ stamps); the background uses its own **environment block** (§ bg); 9-slice panels/buttons are **built programmatically, not generated** (§ 9-slice). Do not cross these.*

## Part 1 — Item icons (Wave 1: the four live families, 33 items + 3 generators)

### Global item style block (paste with every batch)

```
Match the approved Ally Quinn item-icon reference style: stylized painterly
3D illustration, rich saturated local colour, clean sculpted forms and soft
cinematic lighting.
Render ONE COHESIVE ICON COMPOSITION: either one object OR a tightly grouped
cluster that reads as one silhouette. No scenery, room, tabletop or environmental
background.
View from approximately 15 degrees above. Keep the same camera direction and
ground plane across the entire family. Centre the composition with 10-12% safe
margin on every side; nothing cropped. Overall occupancy may increase gradually
from low tier to hero tier, approximately 62-82% (do NOT fill every tier to the
same size - that flattens the value ladder).
Fully transparent PNG background. No border, frame, cast shadow, floor shadow,
watermark, readable words, letters, numerals, denominations, dates, barcodes or
logos. Abstract marks and simple non-linguistic symbols are allowed.
Soft key from upper-left, restrained warm rim from the right. Chunky outer
silhouette; avoid fragile thin elements and excessive micro-detail. The icon
must remain identifiable at 96, 64 AND 48 pixels.
```

### Batch A — Forensic Tools (5) · palette: steel grey, teal case-accents, clinical white
| File | Item | Art notes |
|---|---|---|
| forensic_tools_t01 | Cotton Swab | single swab, angled, sterile packet hint |
| forensic_tools_t02 | Evidence Bag | zip bag with a strong OPAQUE zip rim and visible folds (controlled, not fully realistic transparency, so it survives on alpha), a red tamper strip of repeating diagonal bars (no letters), something indistinct inside |
| forensic_tools_t03 | Full Forensic Case | small hard **closed** case, latched lid, corner guards + handle — kept shut so it differs from the open hero kit at t05 |
| forensic_tools_t04 | UV Light | handheld UV torch, violet glow at the lens |
| forensic_tools_t05 | Complete Forensic Kit | premium large case, gleaming instruments, the family's "hero" |

### Batch B — Fingerprint Evidence (6) · palette: ink navy, white card stock, silver
| File | Item | Art notes |
|---|---|---|
| fingerprint_evidence_t01 | Partial Dusted Print | dusting brush over a half-revealed print on dark surface |
| fingerprint_evidence_t02 | Lifted Print Tape | clear tape strip holding a print, curling slightly |
| fingerprint_evidence_t03 | Fingerprint Card | classic ten-print card, inked prints |
| fingerprint_evidence_t04 | Labeled Prints | fanned trio of tagged print cards (red/blue/green tabs) |
| fingerprint_evidence_t05 | Digital Scan In Progress | handheld scanner, print mid-scan, cyan scanline |
| fingerprint_evidence_t06 | Database Match | tablet showing twin aligned fingerprints joined by green linking brackets / a green check symbol (no MATCH text); bold simplified print spirals, not fine ridge detail |

### Batch C — Rusty Anchor (10) · palette: amber liquids, brass, warm bar light, dark wood hints
| File | Item | Art notes |
|---|---|---|
| rusty_anchor_t01 | Shot Glass | simple, catch of amber light |
| rusty_anchor_t02 | Short Glass on Ice | tumbler, two cubes, condensation |
| rusty_anchor_t03 | Tall Glass Orange | highball, orange slice on rim |
| rusty_anchor_t04 | Beer Bottle | brown bottle, anchor motif on the label |
| rusty_anchor_t05 | Wine Glass Red | red wine, elegant stem |
| rusty_anchor_t06 | Champagne Flute | rising bubbles, celebratory |
| rusty_anchor_t07 | Wine Bottle & Glasses | bottle + two glasses grouped |
| rusty_anchor_t08 | Anchor Signature Cocktail | showy layered cocktail, tiny anchor pick |
| rusty_anchor_t09 | Whiskey on Ice Premium | crystal tumbler, large sphere ice, deep amber |
| rusty_anchor_t10 | 50-Year Scotch | aged bottle, thick wax seal with five radial marks (no numeral), deep amber, brass — the family hero |

### Batch D — Food Gifts / Corner Diner (12) · palette: warm diner reds/creams, appetizing
| File | Item | Art notes |
|---|---|---|
| food_gifts_t01 | Paper Cup | takeaway cup, plain sleeve |
| food_gifts_t02 | Hot Coffee Cup | steam curl, diner ceramic |
| food_gifts_t03 | Coffee and Donut | classic pairing |
| food_gifts_t04 | Burger Single | stacked, dressed, glossy bun |
| food_gifts_t05 | Burger, Fries & Drink | combo grouping |
| food_gifts_t06 | Takeaway Caddy | cardboard caddy, multiple items peeking |
| food_gifts_t07 | Pasta Bowl | twirled pasta, herbs |
| food_gifts_t08 | Sushi Bento | tidy bento box |
| food_gifts_t09 | Ice Cream Sundae Luxury | tall glass, layers, cherry |
| food_gifts_t10 | Steak Plate | steak, char lines, sides |
| food_gifts_t11 | Surf and Turf with Wine | steak + lobster tail + wine glass |
| food_gifts_t12 | Lobster Champagne Banquet | whole lobster + champagne — the family hero |

### Batch E — Generators (3) · each on a subtle base pedestal, more massive/permanent than items
| File | Item | Art notes |
|---|---|---|
| gen_investigation_lab | Investigation Lab kit | desktop forensic station: lamp, microscope, files |
| gen_corner_diner | Corner Diner takeout window | mini diner counter with a dark menu board bearing cream horizontal menu strokes (no readable words) |
| gen_junk | Old evidence drawer | worn wooden drawer unit, mixed odds and ends spilling |

**Generator ↔ family map (authoritative — from the shipped generator SOs, `Assets/App/Generators/`):**

| Generator | Produces (Ep1) | Type | Notes |
|---|---|---|---|
| Investigation Lab (`gen_investigation_lab`) | Forensic Tools + Fingerprint Evidence | multi-fixed | the starting generator |
| Corner Diner (`corner_diner`) | Food Gifts | single-fixed | granted at Ep1 L5 |
| Old Evidence Drawer (`gen_junk`) | Rusty Anchor *(in Ep1)* | flag-gated multi | also drops garage / press / helens_gifts, each locked behind its own `aq.char.*` / `aq.loc.*` story flag; only `rusty_anchor` is unlocked in Ep1 (rusty flag set at L1) |

So four live families are served by three generators — the Lab produces two, and the Junk drawer's family menu grows as story flags fire.

**Tier progression rule (revised v1.3):** don't just make each tier "shinier." Every tier must differ from the tier below in **at least TWO** of: outer silhouette · object count · vertical height · footprint · premium material · family accent motif. Occupancy rises ~62%→82% low-to-hero (see the global block). **Test each icon at 96, 64 AND 48 px**, and inside an actual board tile with the runtime shadow + generator badge + max-tier star present — not just on a blank canvas.
**Batch discipline:** generate a family in ONE session with the first approved icon of that family attached as an extra style anchor for the rest. Filename exactly as listed (.png, 1024×1024). Drop finished batches in `OneDrive/AllyQuinn Game/Images/Production 2026 Images/Items/` — I take it from there.
**Acceptance per icon:** straight-alpha transparent PNG · silhouette readable at 96 / 64 / 48 px · consistent angle+lighting with its family · **no readable text (abstract diegetic marks OK)** · no baked shadow · centred with safe margin. Full technical checklist at the end of this doc.

## Part 1b — Wave 2 (Episode 2 dependencies · FINAL, ready to generate · 2026-07-12)

**Priority ruling: Press = P0, Garage = P1.** Press gates three Ep2 leads (L3 morgue files, L5 episode cutting, L12 the ceremonial Publish) and recurs all season wherever Ally researches, cuts, or publishes. Garage gates one lead (L7, the fused cashbox at Malone's) and its tier art already exists in `SAS/Item Icons/Item Family - Garage/` — import + gap-fill, not regeneration. Timing caveat: if Episode 2 ships in the beta build these are **pre-beta** dependencies; if the launch build is Episode 1 only, the Ep2 timeline (6–8 weeks post-launch) holds.

**Batch discipline (same as Wave 1):** paste the **Global item style block** (Part 1) with every generation, attach the Ally key art as the STYLE anchor, and after the first Press icon is approved attach it as an extra style anchor for the rest of the family. Files `.png`, 1024×1024, transparent background. Drop finished batches in `OneDrive/AllyQuinn Game/Images/Production 2026 Images/Items/`.

### Batch F — Press Items (10) · palette: ink black, newsprint cream, brass, worn Gazette red · reads as *a story becoming permanent record*

**Escalation principle (revised 2026-07-12 for steadier mass):** the ladder climbs by increasing **object count + physical mass + investigative value** — several tiers PAIR objects so the rise never dips (badge+notebook, recorder+tape, viewer+reel). Distinct silhouette at 96 px per tier.

**⚠ NO TEXT rule (kit-wide) — obey it here:** the earlier draft asked for legible words, handwritten labels, archive dates and headlines. **Remove all of that** — generated text is unreliable and the kit forbids it. Represent "paper/press" with **abstract column strokes, redaction bars, colour tabs, grease-pencil rings, generic photo blocks, and unreadable scribble texture** only. No readable letters anywhere.

**⚠ Do NOT depict Arthur (or any face/person).** Generate t01 from the main style reference only; once approved, attach it as the family anchor for t02–t10. Arthur's *archive palette and props* — brown card stock, black ink, brass, worn lanyard, red pencil marks — inspire the family; his face never appears on a badge, paper, or folder.

| File | Item | Per-tier prompt (append to the global style block · NO readable text) |
|---|---|---|
| press_items_t01 | Torn Newsprint Clipping | a single light torn scrap of newspaper, abstract column strokes only (no readable words), soft creased edges, newsprint cream and ink black. The rawest, smallest tier. |
| press_items_t02 | Notepad & Pencil | a spiral-bound flip notepad with a pencil laid diagonally across it, unreadable scribble texture on the page. Thicker object + diagonal pencil — clearly more than t01. |
| press_items_t03 | Press Badge & Notebook | a laminated press badge on a worn brass-clipped lanyard resting on a closed leather notebook — a layered pair, hanging lanyard silhouette. Photo area is a blank block, no face. |
| press_items_t04 | Cassette Recorder & Tape | a chunky handheld cassette recorder with a micro-cassette beside it, blank label block, brass screws catching light. Distinct mechanical silhouette, more mass than t03. |
| press_items_t05 | Bulging Clippings Folder | a thick manila folder over-stuffed with papers, colour tabs and protruding clippings held by a paper-clip, one red grease-pencil ring. A visible collection — much more paper. |
| press_items_t06 | Portable Typewriter | a compact vintage portable typewriter, a blank sheet wound in, keys catching warm light, brass return lever. Large, unmistakable machine silhouette. |
| press_items_t07 | Microfilm Viewer & Reel | a boxed microfilm reel seated in/beside a small tabletop viewer, a short film strip pulled out catching light, brass spindle. Taller technical archive equipment. *(a reel is microfilm, not microfiche — named correctly)* |
| press_items_t08 | Stuffed Morgue File Box | a large corrugated archive box, lid off, several folders standing inside with colour tabs, a redaction bar on one visible sheet. Big box — more mass than the viewer. |
| press_items_t09 | Front-Page Layout Board | a proof/layout board on a slight easel lip so it is NOT a flat low rectangle at the 15° angle — column blocks, a generic photo block, red grease-pencil markup, a raised roller and clipped sheets breaking the top silhouette. The published output. |
| press_items_t10 | The Morgue Cabinet | a tall oak card-catalogue / filing cabinet, one drawer open showing fanned clippings and brass label-holders, a desk-lamp glow on top, worn brass handles — the family **hero**: biggest, richest, warmest. |

*Acceptance per icon (same as Wave 1): transparent background · silhouette readable at 96 px · consistent 15° angle + upper-left key with warm right rim · no text baked in beyond the diegetic headline/label hints · each tier visibly "more" than the one below. Generate t01 first, approve, then attach it as the family style anchor for t02–t10.*

### Batch G — Garage (import pass, no generation unless an icon fails)
Tier art already exists: `SAS/Item Icons/Item Family - Garage/garage_t01`–`t10` (socket wrench → the pimped ride). **Import checklist:** (1) copy the ten PNGs into the item-icon import location; (2) run the GUID/meta pipeline (the Gerald-pattern batch importer) so each gets a stable GUID; (3) confirm each reads at 96 px against the family tier rule; (4) regenerate **only** any tier that fails the silhouette check — no new family, no full regen (ruling 2026-07-12). Garage's only Ep2 use is L7 (the fused cashbox at Malone's).

### New UI sprites (Ep2 branch stamps) · differ by SYMBOL, not colour alone
**Use the FLAT UI-SYMBOL block below — do NOT append the 3D item block** (the two render languages fight). Attach the Ally key art for palette only.

```
Render a FLAT graphic UI STAMP, orthographic (no 3D perspective, no cast shadow),
minimal depth, high contrast, even/fixed stroke weight — like a real distressed
rubber stamp. Transparent PNG. Source canvas 512×512 with a clear ~12% empty
exterior margin. The central symbol occupies a minimum ~45% diameter and its
lines stay ≥ 6 px thick when scaled to 64 px. Leave an empty text-safe band across
the middle third for a runtime label (no baked text, letters or numbers). The
distress/ink-worn texture must never break or fill the central symbol.
```

| File | Prompt (append to the FLAT UI-SYMBOL block above) |
|---|---|
| ui_stamp_shielded | a distressed **shield-shaped (or octagonal)** rubber-stamp border in steel blue, ink-worn and uneven, with a small clean **SHIELD** silhouette centered inside; empty band top and bottom for runtime text ("WITNESS SHIELDED"). No baked text. Transparent background. |
| ui_stamp_daylight | a distressed **circular sunburst** ring in warm amber-gold, with a small **SUN-with-rays** silhouette centered inside; empty band for runtime text ("TRUTH IN DAYLIGHT"). No baked text. Transparent background. |

**Stamp acceptance (both):** readable at **48 and 64 px**; the central symbol survives in **greyscale** (colour is secondary); a clear **text-safe band** for the runtime label; the distress texture must **not** erase the symbol; and the two use **materially different outer silhouettes** — a **shield-shaped / octagonal** border for WITNESS SHIELDED, a **circular sunburst** border for TRUTH IN DAYLIGHT — so they differ by shape as well as symbol.

**Accessibility rule:** the pair must be distinguishable for a colour-blind player — the **shield vs. sun-rays symbols** carry the distinction, blue/amber is secondary. Both must read at ~120 px on a case-summary card. (Ep1's `ui_stamp_public`/`ui_stamp_protected` are still outstanding too — see the readiness audit; the same symbol-not-colour rule applies if they get a symbol pass.)

## Part 2 — UI sprites (small batch, big effect)

Same style reference. All on transparent background unless noted.

| File | What | Notes |
|---|---|---|
| ui_icon_energy | Energy bolt | matches the existing generator badge language, richer render |
| ui_icon_casecash | CaseCash | banded stack of worn notes, noir-green |
| ui_icon_ingot | Platinum Ingots | three stacked silver-white ingots, cool sheen |
| ui_icon_settings | Settings gear | simple, chunky |
| ui_panel_9slice | Popup panel | rounded rect, deep navy (#0A1220) body, subtle lighter border, uniform corners — MUST have symmetric corners for 9-slicing; flat center |
| ui_button_9slice | Button | rounded pill, teal fill, subtle top highlight; symmetric corners |
| ui_stamp_public | "PUBLIC TRUTH" stamp | **BLOCKING: differ by SYMBOL, not colour** — a distressed **circular megaphone/broadcast** symbol; use the FLAT UI-SYMBOL block. Text overlaid at runtime. |
| ui_stamp_protected | "PROTECTED TRUTH" stamp | **BLOCKING: differ by SYMBOL, not colour** — a distressed **shield/lock** symbol in steel blue; FLAT UI-SYMBOL block. (If already generated as colour-only variants, regenerate with symbols before beta.) |
| bg_rivermouth_night | Scene background | **P0 — DONE & approved (2026-07-12).** Filename `bg_rivermouth_night.png`; source: OneDrive `Images/Production 2026 Images/Backgrounds/`. Full prompt + the **environment block** are recorded in §Environment below. **Gameplay quiet zone (normalized, aspect-independent): x 0.08–0.92, y 0.23–0.78** — no high-contrast edges or isolated highlights inside that rectangle (the board sits there). Brightness is controlled by a runtime scrim, NOT by re-lighting the art. |

**HUD architecture ruling (2026-07-12):** the top bar is **component-built, not a background image.** The old `HUDImage` placeholder (`HUD3_Transparent_0`, 920×238, a big baked panel that forced the ~320 px oversized HUD) is **retired** — do not replace it with another background image. The compact Gossip-Harbor-style bar is assembled in code from `ui_meter_pill_9s` (currency pills), `ui_top_avatar_frame` (portrait), the `ui_top_energy/soft/premium` icons, and `ui_icon_settings` (gear, replacing the "MENU" text button). This is Claude's #2 UI pass (HUD-shrink + board-density), not an art-generation task.

**9-slice panels & buttons (`ui_panel_9slice`, `ui_button_9slice`): BUILD PROGRAMMATICALLY by default — do not generate.** AI is poor at perfectly symmetric corners, flat centres, and pixel-identical opposing edges. Generation genuinely wins only for **items and backgrounds**; precision UI infrastructure is deterministic code.

---

## Technical acceptance & QC (every generated asset)
- PNG, **straight (non-premultiplied) alpha**, **sRGB**.
- **No** opaque matte and **no** white/black edge halo around the silhouette; no semi-transparent noise outside the shape.
- ≥ **10–12% safe margin**, silhouette never cropped; centre pivot unless overridden.
- **No baked cast shadow** (runtime adds it).
- Reads at **96 / 64 / 48 px**; approved on a **contact sheet over light tile, dark tile AND checkerboard**.
- Greyscale / colour-blind check where the asset carries meaning (stamps, MATCH cue).
- Unity import: Sprite (2D and UI), **mipmaps OFF**, alpha transparency ON, sensible max-size/compression; land in the item-icon import folder; stable filename + GUID via the batch pipeline.

## Asset manifest (record per approved asset — reproducibility, not just "one session")
`filename · prompt revision (e.g. kit v1.3) · reference images used · generation/edit ID if available · approval date · OneDrive source path · Unity GUID/import status`. Keep it as a small table so a re-roll or a reviewer can trace any icon.

## Proof batch — run these FIVE before any bulk generation
The reviewer's stress test: **Press t01–t03 · the Forensic Evidence Bag (alpha/transparency test) · one Rusty Anchor stemmed glass (thin-silhouette test) · one flat stamp (flat-UI-block test) · one generator.** These five expose nearly every failure mode (single-object vs cluster, no-text, alpha halo, thin-element collapse, flat-vs-3D) before the pipeline "cheerfully manufactures fifty-seven beautifully consistent mistakes." Approve all five, then proceed family by family.


## Environment block (backgrounds — separate render language from items)
Backgrounds are painterly SCENES, not alpha objects. Use this block (not the item block):
```
Painterly night scene for a noir true-crime mobile game, matching the Ally key
art palette (deep navy #0A1220, amber lamplight, muted). PORTRAIT phone aspect.
Composition must leave a QUIET, low-contrast, darker zone in the normalized
rectangle x 0.08-0.92 / y 0.23-0.78 (a game board sits there) — put all interest
in the top and bottom bands. No people, vehicles, text, UI or watermark. It will
be dimmed by a runtime scrim, so a slightly brighter render than final is fine.
```
The approved `bg_rivermouth_night` full prompt (swing-bridge, amber lamps, wet cobbles, water tower, moon) is the reference instance of this block; regenerate variants or future-episode backdrops from the same rules.
