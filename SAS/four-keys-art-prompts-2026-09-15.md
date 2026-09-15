# THE FRIENDS WITH FOUR KEYS · IMAGE PROMPT KIT · 2026-09-15

*Companion to `four-keys-art-inventory-2026-09-15.md`. Written for ChatGPT Images 2.5 (released 2026-09-08; better at holding the subject of an attached reference across turns, which this kit leans on). Every appearance below is PROPOSED: the first approved image becomes canon and gets written into the bible, per the first-approved-image rule. Render language is the locked standard: stylized painterly illustration, never photoreal.*

---

## 0. How to work the model

**One chat per character or place.** Images 2.5 keeps the subject stable across turns inside a conversation, so do the neutral portrait, approve it, then ask for the emotion sheet in the same thread. Do not start a fresh chat for variants.

**References, every time.** Attach the images named in each block. Say "match the attached art style" for style anchors and "this exact person" for identity anchors. Never say "match this character" about a style anchor: that is how Ally's face leaks into strangers.

| Job | Attach |
|---|---|
| Any portrait, first frame | `char_ally_neutral_f01.png` and `char_del_neutral_f01.png` as STYLE anchors |
| Any portrait, emotion sheet | the approved neutral of the same character as IDENTITY anchor |
| Any background | `bg_rivermouth_night.png` and `bg_e1_chandler_street.png` as STYLE anchors |
| Any beat illustration | `bg_rivermouth_night.png` plus one item icon (`food_gifts_t04.png`) as STYLE anchors |

**Sizes.** Ask for the aspect in words; the model picks the nearest. Portraits are 3:4 upright (the game's files are 1086 by 1448). Backgrounds are taller than any preset: generate the tallest portrait the model offers, then in a second turn say "extend the canvas top and bottom to a 9:19.5 phone screen, continuing the sky above and the ground below, nothing new added", and export at 1284 by 2778 or larger. Beat illustrations are 3:2 landscape.

**Judge, then edit.** Never reroll a near miss. Say what is wrong in one sentence ("the eyes are too large for a 44-year-old; reduce them a third") and let 2.5 edit in place. Reroll only when the composition itself is wrong.

**Hard rules that override any prompt below**

- Stylized painterly illustration. If it looks like a photograph, it fails.
- No readable words, numbers or logos anywhere. Signwriting and chalk boards use convincing but illegible letterforms.
- Blue is lighting, never material colour.
- Nobody in this cast wears teal (Ally's), moss green (Dot's) or a navy fuel-dock vest (Rosa's).
- **All five friends are lit the same warm civilian key.** The old rule that antagonists read cold and underlit is suspended for this episode: the lighting must not tell the player which of them it was.
- The gilded panel is never depicted, in any asset, at any size.

---

## 1. Portrait global block (paste first, then one character block)

```
Match the attached images' art style exactly: stylized painterly 3D-illustration
look, expressive slightly enlarged eyes, clean sculpted forms, soft cinematic key
light with a warm rim light, rich saturated local colour against a plain very
dark navy background (#0A1220) with a soft vignette. This is a dialogue portrait
for a true-crime podcast game set in a small coastal city; it will be shown as a
bust over painted night scenes, so it must read as illustration, not photograph.

Format: bust, head and shoulders to mid-chest, three-quarter view facing slightly
toward the viewer's left, eyes toward camera. Upright 3:4. Hands out of frame.
One person only, centred, full head in frame with margin above the hair. No
scenery, no props unless the character block names one, no text, no watermark.
Resting expression with personality, never blank. Use the attached portraits ONLY
for rendering style, lighting and eye treatment; this is a different person.
```

## 2. Character blocks

Generate in this order: Ruby, Liam, Violet, Margo, Brad, Tessa, then the rest. After each approved neutral, run the emotion sheet block in section 3 (Ruby and Liam only; everyone else needs the single frame).

### Ruby Walker · full emotion set · `char_ruby_{emotion}_f01.png`
```
Ruby Walker, woman of 39, practice nurse at a small harbour surgery. Medium
build with a nurse's straight-backed, ready posture. Warm light-brown skin, dark
brown eyes that are quick and a little too bright, as if she has not slept.
Dark hair with copper lowlights pulled into a loose, practical low knot that is
coming undone on one side. Small gold studs. A soft cardigan in dusty plum over
a pale grey scoop-neck top; a lanyard tucked away out of sight; a thin silver
bracelet. Skin that has seen night shifts: faint shadows under the eyes,
otherwise healthy. Expression for the neutral frame: attentive and composed with
a nervous energy just under it, the mouth ready to speak, someone who fills a
silence before it can frighten her. Warm kitchen key light from the left, the
suggestion of an amber practical behind her that stays inside the vignette.
```
*Palette: plum, pale grey, copper. She is warm, tired and brave. The tonic glass and the piano are bible facts, not props for the bust. Sad must be held, not weeping; happy is brief and real.*

### Liam Bryce · full emotion set · `char_liam_{emotion}_f01.png`
```
Liam Bryce, man of 44, crane driver at a boatyard who secretly owns a third of
it. A big man: broad shoulders that fill the frame edge to edge, thick neck,
heavy forearms implied by the set of the shoulders. Ruddy weathered fair skin,
pale blue-grey eyes, a face that gives little away. Sandy hair going grey,
cropped short; a few days of grey stubble. A faded rust-orange work fleece with
a frayed collar over a charcoal thermal; a small folded pair of ear defenders
hooked at the collar. Expression for the neutral frame: steady, watchful,
tired in the way of a man who has been the suspect at a kitchen table for a
month and has stopped arguing; a stillness that could be patience or grief.
Warm workshop key light, a cool grey fill from the harbour side so the face
has depth, no drama in the lighting.
```
*Palette: rust, charcoal, sand. He must not read as menacing: no scowl, no shadowed eyes. Worried is inward; sad is the man who found her.*

### Violet Moore · single frame · `char_violet_neutral_f01.png`
```
Violet Moore, woman of 44, signwriter, seen as she was in life. Lean and
angular, an artisan's upright posture. Pale skin with a wind-touched flush,
grey-green eyes with a critical, amused focus, the look of someone ranking your
handwriting while you talk. Dark auburn hair cut in a blunt bob to the jaw with
a straight fringe, one streak paler at the temple. A faded indigo work shirt
with the sleeves rolled, a fleck of gold leaf at one cuff, a pencil pushed
through the hair above one ear. A fine sable brush with a green-taped ferrule
tucked in the shirt pocket, its tip just visible. Expression: dry, exacting,
private; a woman who would rather be right than liked and is usually both.
Warm studio key with a cooler top light, as if from a skylight.
```
*Palette: indigo, gold fleck, auburn. She is dead before the story starts; this frame is for the evidence board and her case file. No teal.*

### Margo Rivera · single frame · `char_margo_neutral_f01.png`
```
Margo Rivera, woman of 44, accountant who kept the books for half the harbour.
Athletic and contained, a swimmer's shoulders under a precise silhouette. Olive
skin, dark eyes that are level and hard to read, strong straight brows. Black
hair in a severe short crop, immaculate. No jewellery except a plain steel
watch on the wrist that is out of frame. A tailored charcoal blazer over a
white shirt buttoned to the collar. Expression: exact, guarded, ten minutes
early to everything; the first hint of a private life held behind the eyes.
Cool-neutral office key light with a warm rim so she does not read as cold.
```
*Palette: charcoal, white, steel. Do not soften her; the script earns her warmth later through what others say.*

### Brad Collins · single frame · `char_brad_neutral_f01.png`
```
Brad Collins, man of 30, signwriter, Violet's apprentice for seven years, the
young one of the group. Slight, neat, likeable. Light skin with a runner's
leanness, warm brown eyes that agree with you, an easy open face. Mid-brown
hair, short and tidy, a little product in it. A clean cream canvas work jacket
over a heather-grey tee, a thin leather cord at the neck. Nothing worn or
weathered about him. Expression: pleasant, attentive, eager to be liked; the
face that agrees with whoever spoke last. Exactly the same warm civilian key
light as Ruby and Liam, no cool fill, no shadow across the eyes.
```
*Palette: cream, heather, tan. He must be the least interesting face in the row. Any hint of menace in the render is a spoiler and fails.*

### Tessa · single frame · `char_tessa_neutral_f01.png`
```
Tessa, woman of 42, lives alone two hours up the coast; Margo's partner of
eleven years whom nobody in Havenbay knew existed. Soft build, rounded
shoulders drawn slightly in. Fair freckled skin, hazel eyes that keep checking
the edge of the frame. Ash-blonde hair, shoulder length, tucked behind one ear.
A chunky oatmeal knit jumper with a high loose neck she could hide her chin in.
No jewellery except a plain ring on a fine chain, just visible at the neckline.
Expression: kind, wary, braced; a woman who has just said yes to something that
frightens her. Warm lamplight key from low on the left, soft and domestic.
```
*Palette: oatmeal, ash, hazel. She is fear held gently. No moss green, no cardigan (Dot).*

### Violet's sister · single frame · `char_sister_angry_f01.png` (angry is her only line; also save as neutral)
```
Violet Moore's older sister, woman of 50, administers the estate and did not
get on with her. A family resemblance to a lean angular woman with grey-green
eyes, but heavier, more set, more tired. Pale skin, the same eyes gone flinty,
short practical grey-brown hair. A sensible dark raincoat with the collar up
over a burgundy top. Expression: contained anger and old hurt, jaw set, chin
up, someone met in the street who has decided to say it once and plainly.
Flat daylight key, slightly cool, no warmth in the fill.
```

### The café owner · single frame · `char_cafeowner_neutral_f01.png`
```
Owner of the harbour café, woman of 58, opens at six every morning. Solid and
warm, sleeves rolled, a striped apron over a navy tee, a pencil behind one ear,
reading glasses pushed up into greying curls. Brown skin with a kitchen flush,
dark laughing eyes that have gone sober for the moment. Expression: fond,
sorry, careful with what she knows about a regular who died. Warm morning
window light.
```
*If Stephen rules that the harbour café is the Kestrel Corner Diner, keep this face; it becomes the diner's owner.*

### The cinema owner · single frame · `char_cinemaowner_happy_f01.png`
```
Owner of the Regent cinema on the front, man of 66. Round, theatrical,
delighted by his own stories. Silver swept-back hair, a waxed moustache, a
cardinal-red waistcoat over a white shirt with the sleeves gartered, a brass
watch chain. Expression: mid-anecdote, eyes bright, remembering an argument he
enjoyed. Warm foyer key with a faint marquee glow, no readable letters.
```

### The first signwriter · single frame · `char_signwriter1_neutral_f01.png`
```
A working signwriter, man of 60, from a town up the coast. Long face, bald on
top with grey at the sides, a grey moustache, lined skin. A paint-flecked brown
canvas apron over a checked shirt, a mahlstick just visible across the bottom
edge of the frame. Expression: judicious, unwilling to swear to anything, a
craftsman assessing another craftsman's hand. Neutral workshop daylight.
```

### The second signwriter · single frame · `char_signwriter2_confused_f01.png`
```
A working signwriter, woman of 48, from a town up the coast. Strong plain face,
dark hair in a short crop, gold hoop earrings, a smear of gilding size on one
cheekbone. A grey work smock buttoned to the throat. Expression: puzzled and a
little unsettled, brows drawn, someone who could not place a hand and is rare
for it. Neutral workshop daylight.
```
*Budget option: generate the first only and reuse.*

### A yard hand · single frame · `char_yardhand_happy_f01.png`
```
A boatyard hand, man of 24. Wiry, sun-browned, a woollen beanie, a hi-vis vest
faded to pale peach over a hooded sweatshirt, a smudge of antifoul blue on the
jaw. Expression: grinning, telling you something he is not supposed to. Flat
cold morning light off the water, one warm rim from the yard lamp.
```

### The boat owner · single frame · `char_boatowner_neutral_f01.png`
```
A boat owner who works the slip before the tide, man of 55. Weather-cut face,
close grey beard, a knitted navy watch cap, an old waxed jacket with the collar
turned up. Pale eyes narrowed against a wind that is not in the frame.
Expression: plain, reluctant, a man reporting something he wishes he had not
seen about someone who cannot answer it. Pre-dawn blue-grey ambient with a
single warm sodium rim.
```

## 3. Emotion sheet block (Ruby and Liam; attach the approved neutral)

```
Using the attached image as this exact person: produce a MODEL SHEET showing
SEVEN portraits of the same character in two rows (4 above, 3 below), each
labelled beneath in small grey type: 1 NEUTRAL, 2 HAPPY, 3 SAD, 4 ANGRY,
5 SURPRISED, 6 WORRIED, 7 CONFUSED. Every cell: identical face, hair, clothing,
lighting, camera angle and head size to the attached reference, bust crop,
hands out of frame, plain dark navy background. Only the facial expression
changes. Keep every expression believable for a grown adult in a true-crime
story: SAD is held grief, not tears; ANGRY is set and quiet, not shouting;
HAPPY is a real but brief warmth; SURPRISED is a caught breath, not a gasp.
```
Then, in the same chat: "Render cell 3 (SAD) alone at full resolution, upright 3:4, identical to the sheet." Repeat per cell you need. Import each as `char_{name}_{emotion}_f01.png`.

---

## 4. Background global block (paste first, then one scene block)

```
Match the attached backgrounds' art style: stylized painterly ILLUSTRATION for a
mobile game, matching painted item icons and character portraits, NOT photoreal,
no photographic texture, no lens or depth-of-field realism. Painterly noir:
deep navy shadow masses, warm amber practical lights, restrained teal accents,
soft night air. Grounded and slightly grimy, never gothic or fantasy. Use the
attached references ONLY for rendering, lighting, palette and atmospheric depth;
do not copy their streets, bridge, moon or composition.

Tall upright phone composition. One strong emissive light source that stays
readable when the whole image is darkened by 75 percent. Keep the lower-right
third simple and atmospheric: a character portrait stands there in dialogue.
Nothing important in the bottom sixth: a text strip covers it. Focal interest
in the upper half or on the left. No people, no readable words or numbers, no
logos, no vehicles with plates.
```

## 5. Scene blocks

Order by screen time: kitchen, boatsheds, lane, then the rest. Each becomes one file in `Assets/Art/UI/Backgrounds/`, 1284 by 2778 or larger.

### BG-FK1 · Ruby's kitchen, night · `bg_fk_ruby_kitchen.png`
```
A small terraced-house kitchen at night in a coastal town. A scrubbed pine
table under the window at left, two mismatched chairs, a chipped mug and a
glass of clear tonic with a slice, a portable electric heater glowing orange
beside the table leg. Through the window, black harbour dark and one distant
amber light. On the far wall a cork strip with folded papers. Through an open
doorway at the right, the dim shape of an upright piano in the front room.
Warm overhead pendant as the emissive anchor; the heater a second, lower glow.
Lived-in, tidy under strain. The lower-right third: plain floor and the edge of
the doorway, quiet.
```

### BG-FK2 · The slip boatsheds, first light · `bg_fk_boatsheds.png`
```
A corridor of brick boat lockers along the back wall of a harbour slipway at
twenty to six in the morning. A row of narrow brick bays with heavy steel
doors, each door painted a different faded colour; one bay's hasp is shrouded
in a steel hood with a heavy closed-shackle padlock. Wet concrete, coiled rope,
a stack of fish crates. Beyond the corridor's end, the slip runs down to grey
water with the first cold light on it. Emissive anchor: one caged bulkhead
lamp above the middle door, warm against the blue dawn. No camera on any wall.
Lower-right third: plain wet concrete.
```

### BG-FK3 · The lane behind Violet's house, night · `bg_fk_violet_lane.png`
```
A narrow back lane behind a row of small houses in a coastal town, night. High
brick and rendered yard walls on both sides, bins, a leaning bicycle. On the
left, one yard gate in weathered timber, well made, newer than the wall around
it, latched from inside; over the wall the dark upper storey of a house with
one unlit back-bedroom window. A single sodium lamp on a bracket down the lane
as the emissive anchor, throwing long amber light on wet cobbles. Everything
else navy. Lower-right third: bare lane and wall, atmospheric.
```

### BG-FK4 · The town front and the Regent, evening · `bg_fk_regent_front.png`
```
A small seaside town's main front at dusk. Left and centre, a modest 1930s
cinema with a horizontal marquee: six letter positions, three of them filled
with glowing glyphs that are shapes, not readable letters, three empty; an
aluminium ladder chained to the marquee underside. Painted shop fascias along
the row, their signwriting suggested with elegant illegible strokes in gold and
cream. The sea a dark band at the far right edge. Emissive anchor: the marquee
bulbs, warm white and pink. Lower-right third: empty pavement and railings.
```

### BG-FK5 · Margo's office, shut · `bg_fk_margo_office.png`
```
A small accountant's office seen from the street through a shopfront window,
overcast day. Venetian blinds down with a gap at one side; through the gap, a
desk squared to the millimetre, a closed ledger, a pen laid parallel to its
edge, an empty chair pushed in. A vinyl sign panel on the glass with its
lettering reduced to plain abstract bars. Cold grey daylight, the room inside
dim; the emissive anchor is a small desk lamp someone left on, warm behind the
blind. Lower-right third: pavement and the base of the window frame.
```

### BG-FK6 · The harbour café, early morning · `bg_fk_harbour_cafe.png`
```
Inside a small harbour café at six in the morning, looking toward the counter
and the window. Above the counter, three chalk menu boards lettered in a
signwriter's hand, beautiful and entirely illegible; a fourth board with a
different, clumsier hand. Steam from an urn, a tray of cups, a paper on the
counter. Through the window the harbour going from blue to grey. Emissive
anchor: the warm pendant over the counter and the urn's glow. Lower-right
third: an empty table and a chair back, kept simple.
```
*If the café is ruled to be the Kestrel Corner Diner, replace the window view with the diner's, keep the boards.*

### BG-FK7 · Tessa's cottage, evening · `bg_fk_tessa_cottage.png`
```
A small stone cottage interior two hours up the coast, evening. A low sitting
room with a wood burner glowing, a deep armchair with a folded throw, a side
table with two mugs, a window looking onto a dark field and a strip of distant
sea. A framed photograph on the mantel turned very slightly to the wall. Warm
firelight as the emissive anchor, cool blue dusk through the glass. Private,
soft, a room nobody visits. Lower-right third: rug and the arm of a second
chair.
```

### BG-FK8 · Kestrel Head car park, night · `bg_fk_kestrel_head.png`
```
A headland car park at night above the sea. A gravel apron with a low timber
rail and a tilted information board (blank face), one empty parking bay marked
by a single vehicle-shaped absence of gravel dust. Beyond the rail the land
drops to black rock and a heaving pale sea; a lighthouse beam far to the left
sweeps the horizon as the emissive anchor. Wind in the grass, a sky of torn
cloud with a low moon behind it. No cars. Lower-right third: gravel and the
end of the rail, quiet.
```

### BG-FK9 · The inland halt, before dawn · `bg_fk_inland_halt.png`
```
A tiny rural railway halt at a quarter to six in the morning, one platform, a
timber shelter, a single lit window in a small ticket office. Behind the halt a
dark wooded ridge climbs away to the left. Frost on the platform edge, a
timetable case with a blank sheet inside, the rails catching the first grey.
Emissive anchor: the ticket office window, warm, and one platform lamp. No
train, no people, wet bootprints faint on the platform. Lower-right third:
empty platform.
```

---

## 6. Beat illustration block (paste first, then one subject)

```
Match the attached images' art style: stylized painterly illustration, the same
world as the game's item icons and night backgrounds, not photoreal. A single
still image shown in a small framed panel above a caption, so it must read at a
glance: one clear subject, strong value contrast, warm practical light against
navy shadow. Landscape 3:2. No readable words or numbers; any handwriting,
chalk or signwriting is convincing but illegible. No people unless the subject
names them.
```

| Package | Subject block |
|---|---|
| fk_p02_02 The Note Under Glass | `A short handwritten note on cream paper, four lines in a confident signwriter's hand with a single-word sign-off, pressed under a square of glass and pinned to a cork board with one brass pin. Beside it a small brass key on a loop of string. Warm desk lamp from the left; the glass catches one soft highlight.` |
| fk_p03_01 The Harbour Café | `Three chalk menu boards above a café counter, lettered in an elegant signwriter's hand, illegible; a fourth board in a clumsier hand. Steam rising from an urn below, morning light from a side window.` |
| fk_p03_04 The Regent | `A small seaside cinema marquee at dusk, six letter positions with three glowing glyph-shapes back up and three dark, an aluminium ladder chained underneath, a job left unfinished. Marquee bulbs warm against a blue-grey sky.` |
| fk_p04_01 The Boatyard | `A boatyard at morning: one tall crane with slings hanging empty, a row of hulls up on timber blocks, a puddle reflecting the crane cab with its door shut. Cold daylight, one warm lamp in the cab.` |
| fk_p04_08 The Boatsheds | `A corridor of brick boat lockers with heavy steel doors at first light, one hasp under a steel shroud with a heavy padlock, wet concrete, a caged bulkhead lamp burning above the middle door. No camera, no people.` |
| fk_p05_01 The Office | `An accountant's desk seen through the gap in a lowered venetian blind: a closed ledger squared to the desk edge, a pen laid parallel, an empty chair pushed in, a desk lamp left on. Grey daylight outside, warm lamp inside.` |
| fk_p06_02 The Coast Road | `A two-lane coast road running north along cliffs under a low grey sky, no traffic, a single bent road sign with a blank face, sea to the right, a long way of not much ahead. Late afternoon, one break of pale light on the water.` |
| fk_p07_02v Side by Side / fk_p07_02p Half of Another | `Two handwritten letters pinned side by side on a cork board with their matching lines joined by red thread, the writing illegible. Warm desk lamp.` Then in the same chat: `Same board and lamp, but one letter only, and beside it a small index card with a few handwritten lines; no red thread.` Save the first as v, the second as p. |

Never include the panel itself in any of these, even as a shape under a cloth.

---

## 7. Import map

| Asset | File | Folder | Wiring (Claude does this) |
|---|---|---|---|
| Ruby, Liam emotion frames | `char_ruby_{emotion}_f01.png`, `char_liam_{emotion}_f01.png` | `Assets/Art/Characters/Ruby/`, `/Liam/` | Portrait GUIDs mapped into the generator's speaker table; regenerate chapters 2 to 10 dialogue assets; evidence board and case files pick the tokens up |
| Single frames | `char_{name}_neutral_f01.png` (or the named emotion) | `Assets/Art/Characters/{Name}/` | As above; name tokens added to the board's name map |
| Backgrounds | `bg_fk_*.png` | `Assets/Art/UI/Backgrounds/` | New location catalog entries keyed by sprite fragment (`ruby_kitchen`, `boatsheds`, `violet_lane`, `regent`, `margo_office`, `harbour_cafe`, `tessa_cottage`, `kestrel_head`, `inland_halt`) with epigraph and four history lines; scene backgrounds re-pointed in the generator's background table |
| Beat illustrations | `beat_fk_p02_02.png` etc. | `Assets/Art/UI/Beats/` | Assigned to each package's beatArt; caption copy added |

Sprite import settings follow the existing files: Sprite (2D and UI), no mipmaps, compression as the current backgrounds.

## 8. Approval checklist (per image, before it is saved as canon)

1. Could it sit beside Ally's portrait and under the merge tiles and look like one game? If it looks like a photo, back to the model.
2. Portraits: three-quarter left, eyes to camera, hands out, plain navy ground, no teal, no moss, no Ally in the face.
3. Brad reads pleasant and forgettable. If he reads as a suspect, fail.
4. Backgrounds: check at the phone crop, with the bottom sixth covered, with a 460 px bust in the lower right, at 35 percent and 75 percent darkening. The emissive anchor must survive the dark test.
5. No readable text anywhere. No panel anywhere.
6. File named, folder placed, then hand to Claude for wiring.
