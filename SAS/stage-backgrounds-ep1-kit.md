# Episode 1 Stage Backgrounds — generation kit
*v1.0 · 2026-07-14 · 8 masters + 1 variant for "The Listener" dialogue stage. Wired per-dialogue via `CaseGraph.stageBackground` (already shipped — assign the sprite on each Resolve_E1_* asset and the cinematic stage swaps it in).* 

## The idea: light follows the search
The episode runs Day 1–6, and the backgrounds trace one deliberate arc — **from closed night to first light** as Ally gets closer to Dot. Acts 1–2 live in lamplit darkness (studio, bar, night water); the turn at L9/L10 breaks into pre-dawn blue and allotment first-light; L11's fog hill carries the episode's only warm door; L12 says goodnight over the harbour. Players never read this consciously. They feel it.

## How the images are displayed (composition rules — every prompt)
- **Portrait, 1080×2340 minimum** (current Rivermouth master is 863×1822 — go bigger this round; the image is cropped/covered to fill a 1080×1920 stage).
- **Bottom ~16% is covered by the dialogue text strip** — nothing narratively important in the bottom band.
- **A character bust (~460px) stands in the lower-left third** during dialogue — keep that zone atmospheric, not detailed. Key landmark interest belongs in the **upper half and right side**.
- **Shown under a dark scrim (up to 75% black) during dialogue** — every image needs at least one strong warm emissive source (lamp, window, screen glow, dawn sky) that survives heavy dimming. If it reads at 25% brightness, it works.
- No people, no readable text/signage (abstract marks fine), no vehicles with readable plates.

## Global style block (paste with EVERY background; attach bg_rivermouth_night.png as the style anchor)
```
Match the approved Ally Quinn background style (reference attached): painterly
noir illustration, deep navy shadow masses, warm amber practical lights,
restrained teal accents, soft volumetric night air. Grounded and grimy-real,
not gothic, not fantasy. Portrait composition ~1080x2340. Strong single
warm light source that stays readable when the image is darkened to 25%
brightness. Lower-left third kept simple and atmospheric (a character
portrait stands there); focal landmark in the upper half or right side.
No people, no readable words or numbers, no logos.
```

---

## The set

### BG-1 · Ally's studio, night — "the rig"
*Used by: L1 The Tip Line, L2 The Forty Seconds, L3 Three Years of Goodnights (Act 1 spine — highest screen time in the episode).*
A cramped attic podcast studio at night: desk with the chrome broadcast mic and mixing desk (her canon gear), waveform glow on a monitor, corkboard and cassette shelves behind, one warm desk lamp, rain-flecked dormer window upper-right with Havenbay rooftops beyond. The answering machine sits in the pool of lamplight.

### BG-1B · Studio variant — "on air" *(cheap variant of BG-1, same camera)*
*Used by: L5 Case Alert, L12 Goodnight, Harbour (the two broadcast beats).*
Identical scene, transformed by light: desk lamp off, red ON AIR glow (unlabelled red lamp — no readable text), monitor waveform brighter, window now deep pre-dawn blue in L12's spirit. One generation with the base image as reference.

### BG-2 · The Rusty Anchor, interior night
*Used by: L4 The Volume-Up. Permanent canon set — will be reused every season (Gerald's booth, Mo's bar).*
Dockside bar interior, warm amber everything (its signature lighting): brass rail, dark wood, Gerald's corner booth with a low-hanging shaded bulb, bottles glinting behind the bar upper-right, port window with black harbour water beyond. Founded-1924 wear in every surface.

### BG-3 · 11 Chandler Road, kitchen interior — "the cold kettle"
*Used by: L6 The Cold Kettle.*
A tidy widow's terrace kitchen frozen mid-life: kettle on the stove, one cup set out, floral wallpaper, net curtains with grey daylight leaking through the window upper-right, a hallway door ajar into darkness. Unsettling *because* it's neat. Coolest, greyest palette of the set — the one interior with no warm source, just pale window light (that window is the emissive anchor).

### BG-4 · Chandler Road street, overcast noon — "the man who came at noon"
*Used by: L7 The Man Who Came at Noon.*
Rivermouth terrace row at flat overcast noon — the episode's only true daylight, and it should feel *wrong*, exposed. Brick terraces, Dot's door with its dead porch lamp, a pale van parked far down the street (small, anonymous, no plates), gulls overhead. Low-contrast grey-green light; the emissive anchor is the bright overcast sky itself.

### BG-5 · Rivermouth moorings, night — "the quiet boats"
*Used by: L8 The Quiet Boats.*
Working wharf at night: fishing boats low in black water, coiled rope and crates, a single sodium dock light casting long amber reflections, the swing-bridge silhouette far upper-right (ties to the existing street master). Oily calm water. Cash-quiet menace.

### BG-6 · Del's bench, waterfront pre-dawn — "off the record"
*Used by: L9 Off the Record (the branch).*
Harbor Ward seafront bench facing the water, empty, in cool pre-dawn blue (cop lighting — Del's palette per the cast lighting logic: civilians warm / police cool). Railings, a distant cold streetlamp, Kestrel Point lighthouse pinprick on the horizon upper-right. The first image in the set where the sky has begun to lighten.

### BG-7 · The allotments, first light — "where Dot went"
*Used by: L10 Where Dot Went.*
Rivermouth allotments at dawn: leaning sheds, bean canes, mist between plots, a gate open onto a lane where a bus-stop pole stands against the brightening sky (upper-right emissive anchor — the episode's first real warmth of daylight). This is her escape route, seen with relief, not fear.

### BG-8 · The hill cottage, Chapel Lane, fog — "behind that door"
*Used by: L11 The Hill Cottage.*
Larkhill lane climbing into thick fog, dry-stone wall, and Vera's low stone cottage with ONE warmly lit window and a lit door lamp — the episode's emotional destination, the only warm door in the set. Everything else dissolves into fog. The door light must survive heavy dimming; it's what the whole arc has been walking toward.

---

## Wiring map (my side, once art lands)
| Background | CaseGraphs (`stageBackground`) — verified asset names |
|---|---|
| BG-1 studio night | Resolve_E1_Tip (L1), Resolve_E1_Forty (L2), Resolve_E1_Archive (L3) |
| BG-1B studio on-air | Resolve_E1_Pod1 (L5), Resolve_E1_Close (L12) |
| BG-2 Rusty Anchor | Resolve_E1_Bridge (L4 The Volume-Up — Gerald names the swing-bridge sound) |
| BG-3 Chandler kitchen | Resolve_E1_Cottage (L6 The Cold Kettle) |
| BG-4 Chandler street noon | Resolve_E1_Inspector (L7) |
| BG-5 moorings night | Resolve_E1_Boats (L8) |
| BG-6 Del's bench pre-dawn | Resolve_E1_DelCruz (L9) |
| BG-7 allotments dawn | Resolve_E1_Trail (L10) |
| BG-8 hill cottage fog | Resolve_E1_Hills (L11) |

## Integration notes
- Import to `Assets/Art/UI/Backgrounds/bg_e1_<slug>.png` (Sprite 2D/UI, LFS), assign per graph — no code needed, the stage swap shipped 2026-07-14.
- One stage tweak lands with the art: when a dialogue **has** a `stageBackground`, the stage scrim eases to ~0.35 instead of 0.75 (the backdrop *is* the stage; full darkening is only right for the default board backdrop).
- The board backdrop stays `bg_rivermouth_night` throughout Ep1 — the evolution lives in the dialogue stage. (Optional post-v1: swap the board backdrop per phase via NarrativeFlags.)
- Generation order if streaming: **BG-1 first** (three leads use it, and it calibrates the interior style), then BG-8 (the emotional payoff), then the rest in story order.
- Deliver with **straight alpha not needed** (full-bleed scenes) — but same no-checkerboard rule as icons: flat full-frame PNG.
