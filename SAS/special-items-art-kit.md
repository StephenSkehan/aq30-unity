# Special Items Art Kit — the Case Kit (2026-08-07)

Eight pieces: seven item icons + one HUD corner-button icon. Delivery unblocks
the placeholder monogram chips (SK/BK/CC…) already shipping in the kit popup,
Mo's Back Room, and the Case Kit corner button.

## Global block (prepend to every prompt — same render language as the item kit)

> Stylized painterly illustration, hand-painted digital gouache look with
> visible confident brushwork and simplified but dimensional forms, matching a
> noir detective mobile game's item icons. Single object, centred, front
> three-quarter view from roughly 15 degrees above. Soft upper-left key light,
> restrained warm rim light. TRANSPARENT BACKGROUND with straight alpha — no
> backdrop, no plate, no pedestal, no cast shadow on the ground. No readable
> text, numerals, or logos anywhere on the object. Palette: worn brass, aged
> steel, deep navy, cream paper, with one restrained amber or teal accent.
> 1024x1024. Must stay legible at 84px.

**Delivery warning (history has bitten us twice):** straight-alpha PNG, not
white-matte or checkerboard composites. If the tool can't do true alpha,
deliver on flat #FF00FF and note it — we key it here.

## The seven item icons → `Assets/Resources/App/UI/Specials/`

| File | Item | Prompt (after global block) |
|---|---|---|
| special_skeletonkey.png | Skeleton Key | An ornate vintage skeleton key in aged brass, long shaft, three-tooth bit, clover-shaped bow with a small amber glass inset catching the light. Slight patina in the grooves; the metal polished where fingers would hold it. |
| special_boxknife.png | Box Knife | A retro utility box knife, angled steel blade one-third extended from a worn brass-and-navy handle, thumb slider visible. A short curl of cut twine loops beside the blade to say what it does. Blade edge catches a thin cold highlight. |
| special_carboncopy.png | Carbon Copy | Two sheets of paper fanned apart at a slight angle: the top sheet cream, the sheet beneath identical but a shade cooler, with a leaf of inky blue-black carbon paper peeking between them at one corner. Faint illegible typewritten strokes only — no readable words. |
| special_boltcutters.png | Bolt Cutters | Compact bolt cutters with short teal rubber-wrapped handles and heavy forged steel jaws, slightly open, one cleanly cut padlock shackle falling away beside the jaws. Honest tool wear on the pivot bolt. |
| special_searchwarrant.png | Search Warrant | A folded legal document in heavy cream paper, tri-fold slightly sprung open, closed with a deep red wax seal bearing an abstract scale-of-justice impression. A navy ribbon tail under the seal. Illegible formal script strokes only. |
| special_evidencetag.png | Evidence Tag | A manila evidence tag with a reinforced ring hole and a short loop of butcher's twine, blank ruled lines where writing would go (no writing), one corner softly dog-eared. A thin amber border printed around the tag edge. |
| special_cassette.png | Tip-Line Cassette | A worn audio cassette tape, smoke-grey shell, both reels visible with unequal tape wound, a hand-applied cream label with illegible handwritten strokes. One corner of the label lifting. Slight scuffs; a loved object, not a broken one. |
| special_trashcan.png | Trash Can *(added 2026-08-12: the Bolt Cutters confirm shows the target going in the bin, not a red cross)* | A small vintage office waste bin in aged ribbed steel, round with a slightly flared rim, dents and honest wear on the body, the hinged lid tipped open a hand's width as if something was just dropped in. A single crumpled cream paper ball rests at the rim about to fall inside. One restrained teal accent on the lid handle. |

## The corner button icon → `Assets/Resources/App/UI/Icons/`

| File | Item | Prompt (after global block) |
|---|---|---|
| ui_btn_case_kit.png | Case Kit button | A compact worn leather detective's field case, deep brown with brass corner caps and a brass clasp, lid open a hand's width showing the suggestion of tools inside (key bow, tag, folded paper — silhouettes only). Reads as a single strong shape at 142px, matching the weight of the locker safe and evidence corkboard button icons. |

## Import + integration (zero code once delivered)

- Drop PNGs at the paths above; Unity imports as single sprites.
- Code already prefers `Resources/App/UI/Specials/special_<id>` and falls back
  to the monogram chip when absent — each icon lights up the moment it lands.
- The corner button (`SpecialsTrayView.BuildHUD`) still uses the KIT text label;
  when ui_btn_case_kit.png lands, swap it in the same way the locker button
  swapped (`ui_btn_evidence_board` precedent) — one small code change, ask
  Claude.
- Acceptance test per icon: sits beside the item-family icons at 84px chip size
  and reads as the same game (render language standard, 2026-07-15).
