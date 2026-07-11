# UI/UX Improvement Plan — AQ30 vs. Genre Standard
*Comparison basis: current build screenshot vs. Gossip Harbor · 2026-07-11*
*Owner tags: [CLAUDE] = code/scene/import/programmatic, I do it. [STEPHEN] = art generation runs, decisions, approvals. [BOTH] = my prompt kit + your generation + my import.*

## The diagnosis in one paragraph

Gossip Harbor's screen wins not through better individual assets but through **systemization**: one rendered art style across every item, one palette, one font family, rounded-container framing, and a state overlay on everything the player can care about (checkmarks on order-matching items, bolts on energy items, timers on cooldowns, sparkles on new spawns). Our screen currently mixes photoreal icons, emoji-grade icons, a hairline-grid board floating on an unrelated photographic background, default-font text, and zero state communication. The fix is not "more art" — it's one style pass plus a UI component system.

---

## A. The Board (V1 MUST)

1. **Board container**: replace the white grid with a rounded-corner board panel — soft 9-slice frame, drop shadow, sitting on the scene like a physical object. [CLAUDE code, BOTH for the frame sprite]
2. **Cells**: two-tone checkerboard tint (light/lighter navy-paper), no hairlines, small corner radius per cell, inner padding so items never touch cell edges. [CLAUDE — can be done with two flat sprites]
3. **Cell hover/selection state**: subtle highlight under a dragged item's target cell. [CLAUDE]
4. **Background integration**: the scene art behind the board gets a dark gradient scrim so the board reads as the focal plane. [CLAUDE]
5. **Board sizing**: consistent margins; board width-fits with breathing room; currently cells are large and the board area is visually empty. Consider 7×9 grid density vs. tile size tuning after the restyle. [CLAUDE, STEPHEN approves feel]

## B. Item Iconography (V1 MUST — the single biggest visual lever)

6. **One unified item style.** Regenerate ALL item icons (7 families × 10 tiers + generators) in the Ally key-art style: painterly-stylized, consistent camera angle (~15° top-down), consistent lighting, transparent background, similar silhouette weight. This is the difference between "asset flip" and "real game" at first glance. [BOTH — I write the item-icon prompt kit with per-family palettes + a style anchor; you run generation; I batch-import with the GUID pipeline]
7. **Tier progression legibility**: within a family, tiers grow in visual richness/complexity (GH does size+ornament progression). Encode in the prompt kit per tier. [BOTH]
8. **Item scale normalization**: every icon occupies ~80% of cell, centered, with a soft contact shadow rendered under items at runtime. [CLAUDE]
9. **Generator visual identity**: generators sit on a small "platform/base" sprite + keep the energy badge (done) + a charge/pulse treatment distinct from items. [CLAUDE + one base sprite via BOTH]

## C. State Overlays & Feedback (V1 MUST — Gossip Harbor's real secret)

10. **Requirement-match checkmark**: any board item currently needed by an active lead shows a small green tick badge — GH's most important single UX device; it connects the board to the story constantly. We have live requirement data (LeadRequirementChecker). [CLAUDE]
11. **New-spawn sparkle**: brief sparkle on newly spawned items (partially exists via GeneratorTileAnimator — extend to spawned items). [CLAUDE]
12. **Max-tier glow**: chain-complete items get a subtle premium glow. [CLAUDE]
13. **Merge juice**: squash-and-stretch pop on merge result; small ring burst. (Spark exists; make it feel physical.) [CLAUDE]
14. **Delivery animation**: when a requirement is satisfied, the item flies from board to the lead card, card chip fills, haptic tick (haptics done). [CLAUDE]
15. **Currency fly-to-HUD**: rewards fly as icons to the HUD counters, counters tick up. [CLAUDE]
16. **Progress toasts** (already specced for Ep1's hard leads) get the new visual language. [CLAUDE]

## D. HUD / Top Bar (V1 MUST)

17. **Rebuild the top bar**: portrait in a proper frame (level ring post-launch), then three currency capsules — energy / CaseCash / Platinum Ingots — each: rendered icon + count + "+" button. GH's layout is the correct genre-standard reference. [CLAUDE layout; BOTH for the 3 currency icons + capsule sprite]
18. **Wire the + buttons**: energy + → EnergyOutPopup; ingots + → the ingot store; CaseCash + → (greyed in v1 or opens info). [CLAUDE]
19. **Energy regen timer** under the energy pill ("next ⚡ 1:04" / "FULL") — data already exists in EnergyManager. [CLAUDE]
20. **Settings gear icon** replaces the raw text button. [CLAUDE + icon via BOTH]
21. **Replace the placeholder player portrait** (currently a photo!) with Ally's portrait. [CLAUDE — asset exists]

## E. Lead Cards (V1 MUST — currently broken)

22. **Fix the immediate bugs**: the current card overlaps/clips text over the HUD zone and the case-alert layout collides with its own portrait. [CLAUDE]
23. **Redesign the card**: portrait chip, title (max 2 lines, proper wrapping), requirement chips — each chip = item icon + count (owned/needed), grey→lit→ticked progression — and a reward preview row (CaseCash/energy/ingot icons + amounts, like GH's +132 coin preview). Proceed button with clear enabled/disabled states. [CLAUDE; depends on item icons]
24. **Card states**: locked/in-progress/ready visually distinct (ready = gentle pulse, existing bounce kept). [CLAUDE]
25. **Multiple leads presentation**: horizontal scroll with peek of the next card, snap paging. [CLAUDE]

## F. Typography (V1 MUST, cheap, huge)

26. **License-free font pair**: a rounded display face for titles/buttons + a clean body face (e.g., Fredoka/Baloo + Nunito — both OFL). One TMP font asset each, applied globally; outlines/shadows for over-art readability. [STEPHEN picks from 2–3 options I present; CLAUDE imports + sweeps every text element]

## G. Color System & Popup Skin (V1 MUST)

27. **UI palette tokens** derived from the key art: deep navy (#0A1220), Ally teal, amber lamp-light, paper cream, alert red. Defined once in a UITheme static class; every programmatic UI reads from it. [CLAUDE]
28. **One panel skin**: a 9-slice rounded panel + header + close button used by ALL popups — EnergyOut, TileInfo, GeneratorInfo, Settings, CaseResolution, EvidenceBoard modals — replacing today's flat grey rectangles. [CLAUDE + one panel sprite via BOTH]
29. **Standard button component**: 9-slice, pressed/disabled states, min 44pt touch targets. [CLAUDE + sprite via BOTH]
30. **Modal behavior**: consistent dim, scale-in/out animation (~0.15s), close X placement. [CLAUDE]

## H. Scene Backgrounds (V1 SHOULD)

31. **Stylized Havenbay backdrop(s)** replacing the current mismatched photo-composite: one "Rivermouth night street" master in the key-art style, dimmed under the board scrim. Episode-specific backdrops post-launch. [BOTH — prompt kit + generation + import]

## I. Bottom Corners & Misc (V1 SHOULD)

32. **Overflow bucket + evidence board buttons**: consistent circular icon-button treatment with count badges, matching corner margins. [CLAUDE + icons via BOTH]
33. **Lead progress "X/12"**: restyle into a small case-file chip rather than raw text. [CLAUDE]
34. **Case summary screen**: apply panel skin + PUBLIC/PROTECTED TRUTH flag as a stamped seal graphic. [CLAUDE + one stamp sprite via BOTH]

## J. Post-launch backlog (explicitly not v1)

35. Level/XP badge on portrait + progression system
36. GH-style "order case" helper (offers one of three useful items — pairs with ads)
37. Animated/parallax scene backgrounds; weather
38. Seasonal UI skins; event banners
39. Decorations meta (per market research roadmap)
40. Item collection album (taps the merge-genre collection driver)
41. Animated portrait reactions on lead cards
42. Localized fonts/layout audit

---

## Who does what — summary

**[CLAUDE] — all engineering, ~5–7 working days total for the V1-MUST set:**
UITheme tokens + panel/button components; board container/cells/scrim; state overlay system (checkmarks, sparkles, glow, delivery + currency flights); HUD rebuild + wiring + regen timer; lead card redesign + bug fixes; font import + global sweep; popup reskin; icon/sprite import pipeline (GUIDs, metas, batch scripts); everything testable in editor as I go.

**[STEPHEN] — art generation + decisions, spread across sessions:**
Run the item-icon generation (the big one: ~75 icons — batched by family, maybe 2–3 sessions with the prompt kit); currency/HUD icons; panel/frame/button sprites (or approve my programmatic versions); background generation; font choice; style approvals at each checkpoint; device eyeballing.

**Sequencing note:** this slots as the activated "art polish pass" running parallel to Episode 1 asset production. The V1-MUST set (A–G) must land before the **Aug 8 beta** so testers see the real game. Item icons are the long pole on your side — my prompt kit + the model-sheet style anchor make each batch mechanical. Everything in section J waits for live players.

**Suggested first move:** I build the component system + board restyle + state overlays with placeholder-tinted existing icons (immediate massive improvement, zero art dependency), while you start item-icon generation from the prompt kit I write next. The two streams meet at import.
