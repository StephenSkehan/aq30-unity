"""Apply GPT rewrite pass one (2026-09-16) plus Claude's eleven fixes to the chapter JSON."""
import json, pathlib, re
D = pathlib.Path(__file__).parent / "dialogue"
def A(e, line): return {"speaker": "Ally Quinn", "emotion": e, "line": line}
def R(e, line): return {"speaker": "Ruby Walker", "emotion": e, "line": line}
def N(sp, e, line, variant=None):
    n = {"speaker": sp, "emotion": e, "line": line}
    if variant: n["variant"] = variant
    return n

SIGN = "This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight."

ch2 = [
 {"id": "fk_p02_01", "background": "kitchen", "nodes": [
   A("neutral", "Ruby Walker's kitchen. A pine table under the window, a heater beside it. This is where the four sat with Violet's letter, week after week."),
   A("neutral", "Before the letter, it was Tuesdays. Violet came for years, and Ruby had the heater on before she knocked."),
   R("happy", "Heater on, her at the door, then the same argument we'd had for years. That was the friendship. Tea?"),
   A("neutral", "The argument was about whether Ruby's husband should be told. Told what?"),
   R("worried", "You came for Violet, not my marriage. Leave him out of it. Anyway, tea?")]},
 {"id": "fk_p02_02", "background": "caseboard", "nodes": [
   A("neutral", "Ruby kept her copy of the note. Four went out, all the same, two weeks after Violet died. I photographed hers at the kitchen table. It is under glass on my board now, in Violet's handwriting."),
   A("sad", "If you've got this, I'm dead, and I'm sorry about the timing. Go to my locker at the boatsheds by the slip. The padlock's new and these four keys are the only ones. Go together, all four of you, and read what's there together. Violet."),
   A("neutral", "That's all of it.")]},
 {"id": "fk_p02_03", "background": "kitchen", "nodes": [
   R("neutral", "It isn't any of you four, and I'd know. Someone with my key has been in my house, or I've started imagining things. I can't tell which, and both frighten me."),
   R("neutral", "The first thing was the yard gate. I bolt it every night. One morning in October it was unbolted, and I couldn't swear I'd bolted it."),
   R("neutral", "I changed the front door lock and told only you four where the new spare was. It happened again after that. So I took the key in, told nobody, and it stopped."),
   R("sad", "I don't think I'm going to die. I think I'm going to be sixty and embarrassed. If I'm wrong, nobody needs to know. If I'm right, you'll want to know what I noticed."),
   A("neutral", "Here Ruby's hand goes over the page. The lines about Violet's mother. She picks up again below her hand."),
   R("neutral", "Somebody got the place of my spare from one of you without your knowing, or watched one of you use it. Write down everything odd you've noticed this year. About me, and about who has been near each of you."),
   R("neutral", "Read the four lists together at one table. Since I'm asking you to, here's mine. Liam has owned a third of the yard for eight years and lets the town think he's staff."),
   A("neutral", "Her hand goes down again. One line. She won't read that one either."),
   R("neutral", "Ruby's been on nights since August and won't say why. Margo hasn't been to the slip since summer. And Brad paid me back the van money this autumn, every penny. The one thing this year that went right."),
   R("sad", "Not the police. I've nothing they'd act on, and I won't have them in your lives over a gate and a feeling. You four can do what they won't. Read this together. Be careful. Write legibly. Violet.")],
  "decision": {"prompt": "What does the city hear next week?", "options": [
   {"label": "Air Ruby's whole reading", "flag": "aq.fk.d1.aired", "result": "I'll air every line Ruby read. The words go out in Ruby's voice as much as Violet's, and Ruby carries them round Havenbay."},
   {"label": "Air only verified lines", "flag": "aq.fk.d1.held", "result": "I'll air only lines I can verify elsewhere. The city hears less, and what Ruby covered stays at her table."}]},
  "after": [R("worried", "If you cut what I read, people will think I cut it. The lines under my hand stay there either way.")]},
 {"id": "fk_p02_04", "background": "kitchen", "nodes": [
   A("neutral", "Ruby Walker is in her mid thirties, a practice nurse at the surgery on Wharf Street."),
   R("confused", "You're talking about me like I'm not in the room."),
   A("neutral", "When she is frightened, she talks. She fills the room, gives you a job and makes tea nobody asked for."),
   R("neutral", "Six years sober. The glass is tonic. Say that too, before somebody else decides what it is.")]},
 {"id": "fk_p02_05", "background": "kitchen", "nodes": [
   A("neutral", "Ruby gave me this memory without being asked."),
   R("sad", "Three years ago Vi rang me at four in the morning. I drove over. Neither of us ever said what it was, not that night and not after. Anyway."),
   A("neutral", "I don't know what happened. Ruby did not say, and I am not going to guess.")]},
 {"id": "fk_p02_06", "background": "kitchen", "nodes": [
   R("sad", "When we opened the locker, the first thing was relief. Then shame, because relief meant we'd believed she might name one of us."),
   A("neutral", "The next evening you did the arithmetic."),
   R("worried", "New lock. Only four told. Something moved after. Key taken in, it stopped. Liam found the pot empty, broke a pane, and the back door was latched."),
   A("confused", "But the wall could be climbed. Someone could have watched Violet use the pot."),
   R("angry", "I know. We looked outward: no cleaner, lodger or tradesman, and an estranged sister. Then Monday: me on shift, Margo home, Liam at the yard, Brad at home. Nobody had an alibi. Liam had the least.")]},
 {"id": "fk_p02_07", "background": "kitchen", "nodes": [
   A("neutral", "When Brad's rent stopped, his landlord boxed up the flat and sent everything to Ruby. Police had already searched it."),
   A("sad", "Ruby and I went through the boxes over days. Passport, cards, tools, laptop, clothes and paperwork. Days of careful nothing."),
   R("happy", "He'd put his keys on the table, then move them. Twice, always twice. We used to wait for the second move before we spoke."),
   R("happy", "He said thank you like it cost him. He was up at five to run, and he agreed with whoever had spoken last. Seven years he was the pet of the room."),
   A("neutral", "Four ordinary habits, remembered with affection. That was Brad Collins as Ruby had him.")]},
 {"id": "fk_p02_08", "background": "kitchen", "nodes": [
   A("neutral", "Ruby kept the envelope her note came in. Folded behind the note was a covering slip from Violet's solicitor."),
   A("neutral", "It says the four envelopes were lodged with the firm three weeks before Violet died and posted after the funeral."),
   R("neutral", "Four addressees. Liam, me, Margo, Brad. I've read that list enough times to know the order."),
   A("neutral", "The letter was dated three weeks before Violet died. Police examined it, then returned it to Ruby.")]},
 {"id": "fk_p02_09", "background": "kitchen", "nodes": [
   R("neutral", "Every Saturday we came back to this table with the lists Violet asked for. Every Saturday Liam looked worse."),
   A("confused", "The line about the police. Did that sound like Violet to you?"),
   R("worried", "Never. Liam said that night why she was like that about them. Something from the night her mum died. I let it go. Then the facts bent towards him: the yard, the lines about her mother, the bedroom window, the back door."),
   A("confused", "Each of those could also be a friend helping a friend. Brad kept saying so."),
   R("sad", "Every week. Reasonably. He lost every week. By the fifth we'd settled on Liam. At the sixth Margo stopped arguing and looking at anyone. A week later Brad drove to Kestrel Head. She thought she'd know, and she's dead. Anyway.")]},
 {"id": "fk_p02_10", "background": "studio_onair", "nodes": [
   A("neutral", "This week Ruby Walker read me Violet's letter at the table where the four worked it."),
   A("sad", "By the fifth Saturday the room had settled on Liam. At the sixth Margo went quiet. A week later Brad drove to Kestrel Head."),
   A("neutral", "Brad Collins, as Ruby had him: keys moved twice on the table, thank you like it cost him, up at five to run, and he agreed with whoever spoke last."),
   N("Ally Quinn", "neutral", "Next week I air Ruby's whole reading. " + SIGN, "aq.fk.d1.aired"),
   N("Ally Quinn", "neutral", "Next week I air only verified lines. What Ruby covered stays at her table. " + SIGN, "aq.fk.d1.held")]},
]

ch4_new = {
 "fk_p04_09": {"id": "fk_p04_09", "background": "studio", "nodes": [
   A("neutral", "Late in the week a boat owner rang the Tip Line. He works the slip before the tide and knew Violet by sight. She lettered his boat."),
   N("The boat owner", "neutral", "The Monday the notes came, I was down there at twenty past seven. A young man was in the corridor with something flat under his arm. He was near one of the doors. From where I stood, I couldn't tell which door, and I wouldn't swear to the man."),
   A("worried", "That was hours before the four came at night. It is enough to file, not enough to name anyone. It is not going on air.")],
  "decision": {"prompt": "Who gets the first clean look?", "options": [
   {"label": "Send Del the clean tip", "flag": "aq.fk.d2.direct", "result": "Del got the account clean. She asked me to stay off the sheds while they work it, and I said yes. I will not see the corridor again or ask the boat owner twice."},
   {"label": "Map the sheds first", "flag": "aq.fk.d2.market", "result": "I spent a day there first. I gained a second witness and the corridor layout. Del got it a day late, and Liam stopped answering me."}]},
  "after": [A("worried", "Either way, I am holding an unnamed man, something flat and an early hour. Nothing more.")]},
 "fk_p04_10": {"id": "fk_p04_10", "background": "studio_onair", "nodes": [
   A("neutral", "This week, Liam Bryce. He has known Violet since school, drives the boatyard crane and quietly owns a third of the yard."),
   A("neutral", "The letter says he offered twice to take her mother's ashes out by boat. He fixed her bedroom window, rehung her back door and fitted her yard gate."),
   A("neutral", "Every fact happened. Together they can describe a case or a friend helping."),
   N("Ally Quinn", "sad", "Liam no longer answers me. Tonight is one afternoon at his table. " + SIGN, "aq.fk.d2.market"),
   N("Ally Quinn", "worried", "Del asked me to stay off the sheds, and I will. " + SIGN, "aq.fk.d2.direct")]},
}

V, P = "aq.fk.d3.verbatim", "aq.fk.d3.paraphrase"
panel_nodes = lambda flag: [
   N("Ally Quinn", "neutral", "Two signwriters answered my request about the panel. Neither knew the other had written.", flag),
   N("Ally Quinn", "neutral", "Both describe a black ground, gold capitals and an ampersand. Her own alphabet, done better than she had ever done it.", flag),
   N("Ally Quinn", "surprised", "Both put a crack across one corner. Both put a small mark at the bottom right, and both say it was not her name.", flag),
   N("Ally Quinn", "neutral", "Two people who have never met, describing one object from memory. I have pinned their letters side by side.", flag)]
ch7_new = {
 "fk_p07_01v": {"id": "fk_p07_01v", "background": "studio", "nodes": [
   N("Ally Quinn", "worried", "I aired Margo's exact words. By morning they had travelled beyond this podcast, detached from the care Tessa had put around them.", V),
   N("Ally Quinn", "worried", "A stranger up the coast recognised the phrasing as part of a private message and contacted Tessa.", V),
   N("Ally Quinn", "sad", "Tessa rang frightened. She had agreed to the words, not to being found through them.", V),
   N("Ally Quinn", "neutral", "She has stopped answering me. I still have the screenshot. I no longer have her trust.", V)]},
 "fk_p07_01p": {"id": "fk_p07_01p", "background": "studio", "nodes": [
   N("Ally Quinn", "neutral", "I paraphrased Margo's message. No quotation, no app and no Sunday journey went out.", P),
   N("Ally Quinn", "neutral", "Nobody listening could trace the sentence back to the private room where Margo had put it.", P),
   N("Ally Quinn", "neutral", "Tessa stayed out of the story and kept answering when I called.", P),
   N("Ally Quinn", "worried", "The city heard less. I kept the person who trusted me.", P)]},
 "fk_p07_02v": {"id": "fk_p07_02v", "background": "caseboard", "nodes": [N("Ally Quinn", "neutral", "Separate from what happened to Tessa, two signwriters answered my request about the panel. Neither knew the other had written.", V)] + panel_nodes(V)[1:]},
 "fk_p07_02p": {"id": "fk_p07_02p", "background": "caseboard", "nodes": panel_nodes(P)},
 "fk_p07_03": {"id": "fk_p07_03", "background": "studio", "nodes": [
   A("neutral", "Ruby's envelope held more than the key and note. Folded behind them was the solicitor's covering slip."),
   N("Liam Bryce", "sad", "I haven't said a word to you since the sheds. That was mine to carry, not yours. Ruby says you've got the slip. I'll tell you what I know about that night, if you want it.", "aq.fk.d2.market"),
   N("Ally Quinn", "neutral", "Ruby slid it across the table and said, take it, I've read it enough. I have read it a dozen times since.", "aq.fk.d2.direct"),
   A("neutral", "Four addressees, three streets and one post office box. Posted after the funeral, after Friday's last collection, with a Saturday postmark. The solicitor confirms Violet lodged the keys three weeks before she died.")]},
}

# Part C: exact old -> new line pairs (GPT's, plus Claude's), applied to line text and speaker labels
pairs = [
 (3, "fk_p03_01", "The harbour café opens at six. Her menu boards are still out the front. Someone else chalks the prices in now.", "The Kestrel Corner Diner opens at six. Her menu boards are still out the front. Someone else chalks the prices in now."),
 (3, "fk_p03_02", "The café owner knew Violet for years. I asked about her last months.", "The diner's owner knew Violet for years. I asked about her last months."),
 (3, "fk_p03_06", "The café owner saw it. The cinema owner saw it.", "The diner's owner saw it. The cinema owner saw it."),
 (3, "fk_p03_10", "on the menu boards at the harbour café and", "on the menu boards at the Kestrel Corner Diner and"),
 (3, "fk_p03_10", "The café owner told me that in her last months", "The diner's owner told me that in her last months"),
 (7, "fk_p07_05", "The café owner told me something else about that autumn.", "The diner's owner told me something else about that autumn."),
 (7, "fk_p07_05", "and the café says she was asking.", "and the diner's owner says she was asking."),
 (7, "fk_p07_08", "He's the old newsman who keeps the back booth at the Rusty Anchor", "He's my grandfather, a retired police detective who keeps the back booth at the Rusty Anchor"),
 (7, "fk_p07_09", "Then the kind line beside the café's line", "Then the kind line beside the diner owner's line"),
 (9, "fk_p09_01", "A café owner, a cinema owner, two signwriters.", "The Kestrel Corner Diner's owner, a cinema owner, two signwriters."),
 (9, "fk_p09_03", "The café owner has it. The cinema owner has it.", "The diner's owner has it. The cinema owner has it."),
 (9, "fk_p09_08", "Tomorrow night I say it into every kitchen in the country with the radio on.", "Tomorrow night I say it into kitchens, cars and headphones across the country."),
 (10, "fk_p10_02", "He moved inside the day. By that evening it wasn't his any more.", "She moved inside the day. By that evening it wasn't hers any more."),
 (10, "fk_p10_05", "What gave him away was never evidence. Keys moved twice on a table.", "What found him was never evidence. That came after. Keys moved twice on a table."),
 (10, "fk_p10_08a", "For a fortnight the sighting lines ran hot", "For eight days the sighting lines ran hot"),
 (10, "fk_p10_08h", "He asked her one question about what had hung on that wall.", "She asked her one question about what had hung on that wall."),
 (10, "fk_p10_10a", "He was arrested this week, four hundred miles from here", "He was arrested this morning, four hundred miles from here"),
 (10, "fk_p10_10h", "He was arrested this week, four hundred miles from here", "He was arrested this morning, four hundred miles from here"),
 (10, "fk_p10_10a", "Tessa told me the exact words were worth it. That's hers to say, and she said it.", "Tessa rang me tonight, the first time since the summer. She said the exact words were worth it. That's hers to say, and she said it."),
 (10, "fk_p10_10h", "Tessa told me the exact words were worth it. That's hers to say, and she said it.", "Tessa rang me tonight, the first time since the summer. She said the exact words were worth it. That's hers to say, and she said it."),
]
speaker_map = {"The café owner": "The diner's owner"}

def load(ch): return json.loads((D/f"ch{ch:02d}.json").read_text(encoding="utf-8"))
def save(ch, data): (D/f"ch{ch:02d}.json").write_text(json.dumps(data, ensure_ascii=False, indent=1), encoding="utf-8")

save(2, ch2)
for ch, repl in ((4, ch4_new), (7, ch7_new)):
    data = load(ch)
    data = [repl.get(p["id"], p) for p in data]
    save(ch, data)

# ch9 p09_03: the crack cross-check no longer depends on D3 (both branches have the same two letters)
d9 = load(9)
for p in d9:
    if p["id"] == "fk_p09_03":
        p["nodes"] = [n for n in p["nodes"] if not n.get("variant")]
        p["nodes"].insert(2, A("neutral", "And it isn't only tonight. The two letters I was sent put a crack in a corner, before anyone had compared notes."))
save(9, d9)

# ch10: the close is a short update released the night of the arrest
d10 = load(10)
for p in d10:
    if p["id"] in ("fk_p10_10a", "fk_p10_10h"):
        p["nodes"].insert(0, A("neutral", "A short update, released tonight, hours after the news. Not an episode. What I know at nine o'clock."))
save(10, d10)

applied = 0; missing = []
for ch in (3, 7, 9, 10):
    data = load(ch)
    for p in data:
        for nd in p["nodes"] + p.get("after", []):
            if nd["speaker"] in speaker_map: nd["speaker"] = speaker_map[nd["speaker"]]
    for (c, pid, old, new) in pairs:
        if c != ch: continue
        hit = False
        for p in data:
            if p["id"] != pid: continue
            for nd in p["nodes"] + p.get("after", []):
                if old in nd["line"]: nd["line"] = nd["line"].replace(old, new); hit = True
        if hit: applied += 1
        else: missing.append((pid, old[:50]))
    save(ch, data)
print("pairs applied", applied, "missing", missing)

# report: words and nodes per scene for ch2, ch4, ch7; longest node overall
for ch in (2, 4, 7):
    for p in load(ch):
        nodes = p["nodes"] + p.get("after", [])
        w = sum(len(n["line"].split()) for n in nodes)
        mx = max(len(n["line"].split()) for n in nodes)
        flag = "  <-- long" if (w > 130 or len(nodes) > 5) else ""
        print(f"{p['id']}: {w:>3} words, {len(nodes)} nodes, longest node {mx}{flag}")
