# Art Generation Kit — Item Icons & UI Sprites
*v1 · 2026-07-11 · Attach the Ally key art (app icon image) as STYLE reference with every generation. Never as character reference.*

## Part 1 — Item icons (Wave 1: the four live families, 33 items + 3 generators)

### Global item style block (paste with every batch)

```
Match the attached image's art style: stylized painterly 3D-illustration, rich
saturated color, soft cinematic lighting. Render a SINGLE OBJECT as a game item
icon: viewed from slightly above (about 15 degrees), centered, filling ~80% of
a square canvas, on a fully TRANSPARENT background. No text, no border, no
watermark, no drop shadow (the game adds shadows at runtime). Clean, chunky,
readable silhouette that stays legible at 96 pixels. Consistent light: soft key
from upper-left, gentle warm rim from the right.
```

### Batch A — Forensic Tools (5) · palette: steel grey, teal case-accents, clinical white
| File | Item | Art notes |
|---|---|---|
| forensic_tools_t01 | Cotton Swab | single swab, angled, sterile packet hint |
| forensic_tools_t02 | Evidence Bag | clear zip bag, red EVIDENCE tape strip, something indistinct inside |
| forensic_tools_t03 | Full Forensic Case | small hard case, opened lid showing foam-fit tools |
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
| fingerprint_evidence_t06 | Database Match | tablet showing matched prints, green MATCH glow |

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
| rusty_anchor_t10 | 50-Year Scotch | aged bottle, wax seal, faded 50 on the label — the family hero |

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
| gen_corner_diner | Corner Diner takeout window | mini diner counter with menu board |
| gen_junk | Old evidence drawer | worn wooden drawer unit, mixed odds and ends spilling |

**Tier progression rule across all families:** each tier reads visibly "more" than the last — bigger, richer, shinier, more elements — so a player can rank two icons at a glance.
**Batch discipline:** generate a family in ONE session with the first approved icon of that family attached as an extra style anchor for the rest. Filename exactly as listed (.png, 1024×1024). Drop finished batches in `OneDrive/AllyQuinn Game/Images/Production 2026 Images/Items/` — I take it from there.
**Acceptance per icon:** transparent background · silhouette readable at 96px · consistent angle/lighting with its family · no text/shadow.

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
| ui_stamp_public | "PUBLIC TRUTH" stamp | distressed red rubber-stamp ring, no text needed (text is overlaid) |
| ui_stamp_protected | "PROTECTED TRUTH" stamp | same, in steel blue |
| bg_rivermouth_night | Scene background | portrait 1080×1920: stylized Havenbay riverside street at night, swing-bridge silhouette distant, amber lamps, deep navy sky — quiet, noir, matches key art. Keep the central 60% low-detail (board covers it) |

If any 9-slice sprite generates poorly (uneven corners), tell me and I build it programmatically instead — items and the background are where generation genuinely wins.
