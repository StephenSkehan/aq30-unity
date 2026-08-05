import re, html, io, os, sys

ROOT = r"C:\users\user\dev\aq30-unity"
DLG  = os.path.join(ROOT, "Assets", "Content", "TheListener", "Dialogue")

EMOTIONS = ["neutral", "happy", "sad", "angry", "surprised", "worried", "confused"]

# Lead order + headers (numbers/day/type per VO-LOCK v1.0; Books per v1.1 wiring)
LEADS = [
    ("L1",   "Resolve_E1_Tip",       'e1_tip — "The Tip Line" (Podcast, VO) · Day 1, Monday'),
    ("L2",   "Resolve_E1_Forty",     'e1_forty — "The Forty Seconds" (Evidence) · Day 1'),
    ("L3",   "Resolve_E1_Archive",   'e1_archive — "Three Years of Goodnights" (Data) · Day 1, Monday evening'),
    ("L4",   "Resolve_E1_Bridge",    'e1_bridge — "The Volume-Up" (Discuss — Gerald) · Day 2, Tuesday'),
    ("L4\u00bd", "Resolve_E1_Books", 'e1_books — "Off the Books" (Discuss — Del) · Day 2, Tuesday evening'),
    ("L5",   "Resolve_E1_Pod1",      'e1_pod1 — "Case Alert: Dot Ellis" (Podcast, VO — milestone) · Day 2'),
    ("L6",   "Resolve_E1_Cottage",   'e1_cottage — "The Cold Kettle" (Evidence) · Day 3, Wednesday'),
    ("L7",   "Resolve_E1_Inspector", 'e1_inspector — "The Man Who Came at Noon" (Interview) · Day 3'),
    ("L8",   "Resolve_E1_Boats",     'e1_boats — "The Quiet Boats" (Money Trail) · Day 3'),
    ("L9",   "Resolve_E1_DelCruz",   'e1_delcruz — "Off the Record" (Interview — THE BRANCH) · Day 4, Thursday'),
    ("L10",  "Resolve_E1_Trail",     'e1_trail — "Where Dot Went" (Data) · Day 4, evening'),
    ("L11",  "Resolve_E1_Hills",     'e1_hills — "The Hill Cottage" (Location — CLIMAX) · Day 5, Friday'),
    ("L12",  "Resolve_E1_Close",     'e1_close — "Goodnight, Harbour" (Podcast, VO — milestone) · Day 6, Saturday'),
]

def unescape(s):
    out, i = [], 0
    while i < len(s):
        c = s[i]
        if c == "\\" and i + 1 < len(s):
            n = s[i+1]
            if n == '"': out.append('"'); i += 2; continue
            if n == "\\": out.append("\\"); i += 2; continue
            if n == "n": out.append("\n"); i += 2; continue
            if n == "u" and i + 5 < len(s):
                out.append(chr(int(s[i+2:i+6], 16))); i += 6; continue
        out.append(c); i += 1
    return "".join(out)

def field(block, name):
    m = re.search(rf'^\s*{name}: "(.*)"\s*$', block, re.M)
    if m: return unescape(m.group(1))
    m = re.search(rf'^\s*{name}: (.*?)\s*$', block, re.M)
    return m.group(1) if m else ""

def parse_nodes(path):
    text = io.open(path, encoding="utf-8").read()
    # nodes are "  - id: ..." blocks under "nodes:"
    chunks = re.split(r'^  - id: ', text, flags=re.M)[1:]
    nodes = []
    for ch in chunks:
        nid = ch.splitlines()[0].strip()
        speaker = field(ch, "speaker") or "?"
        line = field(ch, "line")
        emo_raw = field(ch, "emotion")
        try: emo = EMOTIONS[int(emo_raw)]
        except (ValueError, IndexError): emo = "?"
        if line: nodes.append((nid, speaker, emo, line))
    return nodes

rows_html = []
total_nodes = 0
for num, asset, header in LEADS:
    path = os.path.join(DLG, asset + ".asset")
    nodes = parse_nodes(path)
    total_nodes += len(nodes)
    rows = ""
    for i, (nid, speaker, emo, line) in enumerate(nodes, 1):
        emo_txt = f'<span class="em">({emo})</span>' if emo != "neutral" else ""
        rows += (f'<tr><td class="n">{i}</td><td class="sp">{html.escape(speaker)}</td>'
                 f'<td class="line">{emo_txt}{html.escape(line)}</td></tr>\n')
    rows_html.append(
        f'<section><h2><span class="lnum">{num}</span> · {html.escape(header)}</h2>\n'
        f'<table>{rows}</table></section>')

page = """<!DOCTYPE html><html><head><meta charset="utf-8"><title>Episode 1 Production Script</title>
<style>
  @page { margin: 18mm 15mm; }
  body { font-family: Georgia, 'Times New Roman', serif; color: #1c1a17; margin: 0; font-size: 11.5pt; line-height: 1.5; }
  .cover { text-align: center; margin: 90px 0 70px; }
  .cover h1 { font-size: 25pt; letter-spacing: 1px; margin: 0 0 6px; }
  .cover .sub { font-size: 13pt; color: #555; margin-bottom: 28px; }
  .cover .meta { font-size: 10pt; color: #777; line-height: 1.8; }
  section { page-break-inside: avoid; margin-bottom: 26px; }
  h2 { font-size: 12.5pt; border-bottom: 2px solid #1c1a17; padding-bottom: 4px; margin: 26px 0 8px; }
  .lnum { background: #1c1a17; color: #f5f1e8; padding: 1px 8px; border-radius: 3px; font-size: 11pt; }
  table { width: 100%; border-collapse: collapse; }
  td { vertical-align: top; padding: 5px 8px; border-bottom: 1px solid #e3ded2; }
  .n  { width: 22px; color: #999; font-size: 9.5pt; text-align: right; }
  .sp { width: 110px; font-weight: bold; font-size: 10.5pt; }
  .em { color: #8a6d3b; font-style: italic; margin-right: 6px; font-size: 10pt; }
  .line { }
</style></head><body>
<div class="cover">
  <h1>ALLY QUINN: ECHOES OF HAVENBAY</h1>
  <div class="sub">Episode 1 — "The Listener" · Production Script</div>
  <div class="meta">
    Compiled directly from the live in-game dialogue assets · v1.1 line state<br>
    Includes: v1.1 polish pass (L1–L4) · "Off the Books" (Del debut, L4½) · Kestrel Corner Diner rename · em-dash sweep<br>
    NODES_TOTAL script lines across 13 leads · 2026-08-05 · Indigo Chimp Studios
  </div>
</div>
BODY
</body></html>"""

page = page.replace("NODES_TOTAL", str(total_nodes)).replace("BODY", "\n".join(rows_html))
out = os.path.join(ROOT, "SAS", "episode-1-production-script-v1.1.html")
io.open(out, "w", encoding="utf-8").write(page)
print("wrote", out, "-", total_nodes, "nodes")
