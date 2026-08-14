# Feature Spec — Location Board (evidence board v2)

**Status:** SKETCH / awaiting rulings
**Date:** 2026-08-14 (Stephen's concept, cohort round week)
**Supersedes:** lead-card pins + PHASE banners on the evidence board. Cast row, zoom/pan, plaque all stay.

---

## Concept (Stephen, 2026-08-14)

Remove the lead index cards completely. Replace them with **partial-image polaroids of locations** (Del's table, the studio, the blue gate). Tapping a location works like the character portraits: a modal lists related dialogue scenes for replay, plus a **location history** — one detail revealed free, further details (including new images) purchasable with CaseCash.

## Why the data already supports it

`CaseGraph.stageBackground` carries the scene's backdrop sprite per dialogue. Locations therefore **auto-populate from data** exactly as the cast row does from portraits:

- Cast row = WHO (lead front portraits + node portraits)
- Location pins = WHERE (stage backgrounds of resolved dialogues)

A resolved lead whose graph has no `stageBackground` buckets to **The Studio** (the default backdrop is the studio scrim), so every replay the cards offered survives the migration. No replay access is lost.

## Art: zero new renders for v1

11 Ep1 location backgrounds exist in `Assets/Art/UI/Backgrounds/`. Polaroids are **code-side partial crops** (RawImage + uvRect, center-weighted) in the same polaroid frame as character pins. The proposed Ep1 location set:

| Location (display) | Sprites | Canon hook |
|---|---|---|
| The Studio | bg_e1_studio, _dawn, _onair | Ally's booth; the Tip Line lives here |
| Chandler Road | bg_e1_chandler_street, _kitchen | Dot's house, the blue gate at eleven |
| The Allotments | bg_e1_allotments | Dot's escape route |
| The Hill Cottage | bg_e1_hill_cottage | Vera, Larkhill |
| The Moorings | bg_e1_moorings | Quiet boats |
| The Rusty Anchor | bg_e1_rusty_anchor | Mo's bar, Gerald's booth |
| Kestrel Corner Diner | bg_e1_diner_exterior | Del's watchful table |
| Del's Bench | bg_e1_del_bench | ⚠ venue reconciliation pending (canon 2026-08-03: Del's spot is the diner table, NOT a waterfront bench; L9 "her bench" unresolved) — fold into Kestrel Corner or keep separate, ruling needed |

(`bg_rivermouth_night` = scene backdrop, `ui_backdrop_caseboard` = retired stand-in; both excluded.)

## The modal (mirrors CharacterProfileModal + DossierPopup)

- Polaroid header (larger crop), location name, one-line canon epigraph
- **Scenes here:** replay rows for every resolved dialogue staged at this location (the cards' old job)
- **Location history:** dossier-paper entries — first detail FREE on discovery, subsequent details priced in CC, only the next locked detail visible (dossier convention)
- **Image rewards:** some purchased details unlock the **full wide view** of the location (full-bleed viewer of the existing bg render = a real reward, zero new art). Later: Stephen renders alternate angles as premium details.
- Title-bar treatment with ? help, per app-wide standard.

## Architecture (all proven patterns)

```
Assets/Scripts/UI/EvidenceBoard/LocationCatalog.cs   // code-defined: sprite key -> location, name, epigraph, detail entries + prices (DossierCatalog pattern)
Assets/Scripts/UI/EvidenceBoard/LocationService.cs   // prefs aq.locations.state, spend reason "location", analytics location_detail_unlocked (DossierService pattern)
Assets/Scripts/UI/EvidenceBoard/LocationPhotoPin.cs  // polaroid crop pin (CharacterPhotoPin pattern)
Assets/Scripts/UI/EvidenceBoard/LocationModal.cs     // modal (CharacterProfileModal + DossierPopup patterns)
```

Modified: `EvidenceBoardScreen.PopulateBoard()` — location pins replace `LeadCardPin` rows; `LeadCardPin.cs` + PHASE banner code retire.

Discovery rule: a location pins to the board when its first scene resolves (same flag data the cards used). History detail 1 unlocks free at pin time.

## Economy (RULING NEEDED)

Details are **pure lore + images: zero tap displacement, zero item grants** — the safest sink class, and it lands in the tail where sinks are scarce (dossiers' logic). Proposed: 7 locations × 3 paid details, banded 40/80/120 CC per location ≈ **1,680 CC full set**, alongside dossiers' 2,500. Combined optional-lore sinks ≈ 4,200 CC against 1,410/episode income + cold case tail = long-tail collection goals, nothing blocking.

## Strings + phase banners (RULING NEEDED)

`boardConnections` strings currently join cards. Options: (a) retire strings with the cards for v1, revisit as location↔location relationships (Chandler Road ↔ Allotments escape route is a natural); (b) keep them, drawn between locations. Phase banners: retire with the cards (the board reads as geography now, not chronology) unless ruled otherwise.

## Content

Per-location history copy = canon-first content work (bible: pump house in earshot, quiz night table six, back-to-the-wall sightlines, Patrick Callahan's 1924 tavern). Claude drafts, Stephen approves before wiring. No em dashes. Ally-voice epigraphs.

## Effort

~1.5–2 days build once rulings land + a content-approval pass on the history copy.
