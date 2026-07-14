# Episode 1 Stage Backgrounds — generation kit
*v1.1 · 2026-07-14 · 8 masters + 2 studio lighting variants for "The Listener" dialogue stage. Wired per-dialogue via `CaseGraph.stageBackground` (already shipped — assign the sprite on each Resolve_E1_* asset and the cinematic stage swaps it in). v1.1 applies external review: BG-6 time-of-day corrected to the locked script, BG-7 labelled as a reconstruction, broadcast variant split for L5 vs L12, cool-light exceptions documented, scrim spec standardised, style-only reference language added.*

## The idea: light follows the search
The episode runs Day 1–6, and the backgrounds trace one deliberate arc — **from closed night to first light** as Ally gets closer to Dot. Acts 1–2 live in lamplit darkness (studio, bar, night water); the turn comes at L9's open midday horizon and L10's first-light reconstruction of Dot's escape; L11's fog hill carries the episode's only warm door; L12 says goodnight as the studio window shifts toward dawn. Players never read this consciously. They feel it.

## How the images are displayed (composition rules — every prompt)
- **Portrait, 1284×2778 or larger** (true phone ratio; the current Rivermouth master is 863×1822 — go bigger this round). Keep vertical bleed: nothing essential in the top or bottom crop margins of the 1080×1920 stage crop.
- **Bottom ~16% is covered by the dialogue text strip** — nothing narratively important in the bottom band.
- **A character bust (~460px) stands in the lower-RIGHT third** during dialogue (right-anchored so the cast faces into the scene) — keep that zone atmospheric, not detailed. Key landmark interest belongs in the **upper half and left side**.
- **Scrim spec:** production presentation is a **35% black scrim** (the stage eases to this when a dialogue has its own backdrop); the **75% scrim is the stress test** (default board-backdrop dimming). Approve each image at BOTH levels. Every image needs at least one strong emissive anchor that survives the stress test — usually a warm practical (lamp, window, screen glow, dawn sky); BG-3 and BG-4 are the documented cool-light exceptions.
- No people, no readable text/signage (abstract marks fine), no vehicles with readable plates.

## Approval overlay (review every generation like this)
Check each image at: full resolution → the 1080×1920 stage crop → with the bottom-16% strip and a ~460px lower-right character silhouette overlaid → under 35% scrim → under 75% scrim → with Ally's portrait actually placed over it. This exposes composition failures far faster than judging attractive full-screen paintings in isolation.

## Global style block (paste with EVERY background; attach bg_rivermouth_night.png as the style anchor)
```
Match the approved Ally Quinn background style (reference attached): painterly
noir illustration, deep navy shadow masses, warm amber practical lights,
restrained teal accents, soft volumetric night air. Grounded and grimy-real,
not gothic, not fantasy. Use the attached reference ONLY for painterly
rendering, lighting, palette and atmospheric depth — do NOT copy its street,
bridge, moon, architecture or composition unless this prompt explicitly asks
for them. Portrait composition, 1284x2778 or larger. Strong single emissive
light source that stays readable when the image is darkened to 25%
brightness. Lower-RIGHT third kept simple and atmospheric (a character
portrait stands there); focal landmark in the upper half or left side.
No people, no readable words or numbers, no logos.
```

---

## The set

### BG-1 · Ally's studio, night — "the rig"
*Used by: L1 The Tip Line, L2 The Forty Seconds, L3 Three Years of Goodnights (Act 1 spine — highest screen time in the episode). This becomes a PERMANENT series location: once approved, lock the room geometry and prop placement (mic, mixing desk, cassette shelves, corkboard, dormer window, lamp) — all later studio variants are edits of this master, never regenerations.*
A cramped attic podcast studio at night, high detail biased LEFT: chrome broadcast mic and mixing desk left-middle, warm desk lamp upper-left, the answering machine in its pool of lamplight left of centre, corkboard and cassette shelves in the upper background, rain-flecked dormer window upper-left with Havenbay rooftops beyond. Monitor shows abstract waveform bars (no pseudo-text). Lower-right: dark chair edge and soft wall shadow only.

### BG-1B · Studio variant — "on air, investigation night" *(lighting edit of BG-1, same camera)*
*Used by: L5 Case Alert.*
Identical room, transformed by light: desk lamp off, a small red rectangular tally lamp glowing (visible but restrained — a recording light, not a nightclub; no "ON AIR" text), monitor waveform brighter, window still deep night.

### BG-1C · Studio variant — "on air, closing episode" *(lighting edit of BG-1, same camera)*
*Used by: L12 Goodnight, Harbour.*
Same geometry as BG-1B with the temperature turned toward resolution: tally lamp softer, room warmth eased up, and the dormer window shifting to first blue dawn — the episode's last image echoes its arc. Both variants must be edits/controlled relights of the approved BG-1 master; preserving exact room geometry matters more than saving a file.

### BG-2 · The Rusty Anchor, interior night
*Used by: L4 The Volume-Up. Permanent canon set — will be reused every season (Gerald's booth, Mo's bar). Strict geometry consistency once approved.*
Dockside bar interior, warm amber everything (its signature lighting): brass rail, dark wood, Gerald's corner booth with a low-hanging shaded bulb, bottles glinting behind the bar upper-left (abstract unlabelled glass and paper shapes — no readable brand labels), port window upper-left with black harbour water beyond. A century of dockside wear in the wood, brass and upholstery.

### BG-3 · 11 Chandler Road, kitchen interior — "the cold kettle"
*Used by: L6 The Cold Kettle.*
A tidy widow's terrace kitchen frozen mid-life: kettle on the stove, one cup set out, floral wallpaper, net curtains with grey daylight leaking through the window upper-left, a hallway door ajar into darkness. Unsettling *because* it's neat — NO police tape, overturned furniture, broken glass or ominous red details; the neatness is the horror. **Exception to the warm-practical rule: the pale cool window is the sole emissive anchor — do not add a glowing lamp.** Paint the window ~20–30% brighter than feels final so it survives the scrim.

### BG-4 · Chandler Road street, overcast noon — "the man who came at noon"
*Used by: L7 The Man Who Came at Noon.*
Rivermouth terrace row at flat overcast noon — the episode's only true daylight, and it should feel *wrong*, exposed. Brick terraces, Dot's door with its dead porch lamp, gulls overhead. A pale van far down the street: small, partially obscured by terrace perspective and street furniture, no driver, no plates, NOT spotlighted — it must read as background clutter until the dialogue points at it, never as a visual spoiler. **Exception to the warm-practical rule: the bright overcast sky is the sole emissive anchor.** Low-contrast grey-green light.

### BG-5 · Rivermouth moorings, night — "the quiet boats"
*Used by: L8 The Quiet Boats.*
Working wharf at night: fishing boats dark and low in black water with only partial edge highlights (restrained rigging/deck detail — this is a threatening pattern, not a busy maritime illustration), coiled rope and crates, a single sodium dock light whose long amber reflection forms the dominant visual axis, the swing-bridge silhouette far upper-left (ties to the existing street master). Oily calm water. Cash-quiet menace.

### BG-6 · Del's bench, waterfront midday — "off the record"
*Used by: L9 Off the Record (the branch — Del's lunch break per the locked script).*
Harbor Ward seafront bench facing the water, empty, at cold overcast midday: flat blue-grey water, pale winter sky, railings, Kestrel Point lighthouse pinprick on the horizon upper-left. Cool police palette (civilians warm / cops cool) without pretending lunchtime is dawn — the first wide-open horizon in the episode, contrasting the closed rooms before it. The pale sky is the emissive anchor.

### BG-7 · Reconstructed route: the allotments at first light — "where Dot went"
*Used by: L10 Where Dot Went. NOTE: this is a RECONSTRUCTION — the player is visualising Dot's Day-1 escape while Ally traces it on Day 4, not literally standing there at dawn.*
Rivermouth allotments at dawn with a slight misted-memory treatment — softer edges, gentler contrast than the literal locations: leaning sheds, bean canes, mist between plots, and the key symbol — a gate standing OPEN onto a lane, upper-left to left-middle, where a bus-stop pole silhouette (simple pole + small blank timetable case, nothing sign-like or readable) stands against the brightening sky (the emissive anchor — the episode's first real warmth of daylight). Her escape route, seen with relief, not fear.

### BG-8 · The hill cottage, Chapel Lane, fog — "behind that door"
*Used by: L11 The Hill Cottage. The emotional hero of the set.*
Larkhill lane climbing into thick fog, dry-stone wall, and Vera's low stone cottage left of centre — modest and practical, NOT picturesque or fairy-tale — with ONE warmly lit window and a lit door lamp: the brightest warm elements in the entire episode, though not overexposed, and no human silhouette in the doorway. Everything else dissolves into fog. The warmth suggests safety without cancelling the surrounding danger; the door light must survive the stress scrim — it's what the whole arc has been walking toward.

---

## Wiring map (my side, once art lands)
| Background | CaseGraphs (`stageBackground`) — verified asset names |
|---|---|
| BG-1 studio night | Resolve_E1_Tip (L1), Resolve_E1_Forty (L2), Resolve_E1_Archive (L3) |
| BG-1B on-air, investigation | Resolve_E1_Pod1 (L5) |
| BG-1C on-air, closing | Resolve_E1_Close (L12) |
| BG-2 Rusty Anchor | Resolve_E1_Bridge (L4 The Volume-Up — Gerald names the swing-bridge sound) |
| BG-3 Chandler kitchen | Resolve_E1_Cottage (L6 The Cold Kettle) |
| BG-4 Chandler street noon | Resolve_E1_Inspector (L7) |
| BG-5 moorings night | Resolve_E1_Boats (L8) |
| BG-6 Del's bench midday | Resolve_E1_DelCruz (L9) |
| BG-7 allotments reconstruction | Resolve_E1_Trail (L10) |
| BG-8 hill cottage fog | Resolve_E1_Hills (L11) |

## Integration notes
- Import to `Assets/Art/UI/Backgrounds/bg_e1_<slug>.png` (Sprite 2D/UI, LFS), assign per graph — no code needed, the stage swap shipped 2026-07-14.
- One stage tweak lands with the art: when a dialogue **has** a `stageBackground`, the stage scrim eases to ~0.35 instead of 0.75 (the backdrop *is* the stage; full darkening is only right for the default board backdrop).
- The board backdrop stays `bg_rivermouth_night` throughout Ep1 — the evolution lives in the dialogue stage. (Optional post-v1: swap the board backdrop per phase via NarrativeFlags.)
- Generation order if streaming: **BG-1 first** (three leads use it, and it calibrates the interior style), then BG-8 (the emotional payoff), then the rest in story order.
- Deliver with **straight alpha not needed** (full-bleed scenes) — but same no-checkerboard rule as icons: flat full-frame PNG.
