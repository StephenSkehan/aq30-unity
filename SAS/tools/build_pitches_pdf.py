"""Build the round-3.1 pitch PDF: md -> styled html -> pdf (house pattern).

Wide agency-map tables (7 columns of prose) and the forced-ranking table do not
survive A4 portrait as tables, so they are re-rendered as cards. Everything else
stays a table, including the section 6 comparison grid.

Usage:  python SAS/tools/build_pitches_pdf.py
Requires: markdown (pip), Chrome for --headless --print-to-pdf.
"""
import io, os, re, subprocess, sys, time
import markdown

ROOT = r"C:\users\user\dev\aq30-unity"

# Default target is the working pitch document. Pass a stem to build any other
# SAS markdown file with the same house styling:
#   python SAS/tools/build_pitches_pdf.py episode-1-story-pack
STEM_IN  = sys.argv[1] if len(sys.argv) > 1 else "episode-1-pitches-v3-murder-pilot"
STEM_OUT = sys.argv[2] if len(sys.argv) > 2 else (
    STEM_IN if len(sys.argv) > 1 else "episode-1-pitches-v3.1")

SRC  = os.path.join(ROOT, "SAS", STEM_IN + ".md")
HTML = os.path.join(ROOT, "SAS", STEM_OUT + ".html")
PDF  = os.path.join(ROOT, "SAS", STEM_OUT + ".pdf")

CHROME = [
    r"C:\Program Files\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
]

INLINE = markdown.Markdown(extensions=["tables"])


def inline(text):
    """Render one table cell's inline markdown, without the wrapping <p>."""
    INLINE.reset()
    out = INLINE.convert(text.strip())
    out = re.sub(r"^<p>|</p>$", "", out.strip())
    return out


def split_row(line):
    line = line.strip()
    if line.startswith("|"):
        line = line[1:]
    if line.endswith("|"):
        line = line[:-1]
    return [c.strip() for c in line.split("|")]


def is_sep(line):
    return bool(re.match(r"^\s*\|?[\s:\-|]+\|[\s:\-|]*$", line)) and "-" in line


def agency_cards(header, rows):
    out = ['<div class="cards">']
    for r in rows:
        if len(r) < 7:
            r = r + [""] * (7 - len(r))
        num, phase, dec, play, persists, cls, callback = r[:7]
        out.append('<div class="card">')
        out.append(
            f'<div class="chead"><span class="cid">{inline(num)}</span>'
            f'<span class="cphase">{inline(phase)}</span>'
            f'<span class="ccls">class {inline(cls)}</span></div>'
        )
        out.append(f'<div class="cdec">{inline(dec)}</div>')
        out.append(
            f'<dl><dt>Changes in play</dt><dd>{inline(play)}</dd>'
            f'<dt>Persists</dt><dd>{inline(persists)}</dd>'
            f'<dt>Callback</dt><dd>{inline(callback)}</dd></dl>'
        )
        out.append("</div>")
    out.append("</div>")
    return "\n".join(out)


def rank_cards(header, rows):
    out = ['<div class="cards">']
    for r in rows:
        if len(r) < 5:
            r = r + [""] * (5 - len(r))
        dim, first, second, third, reason = r[:5]
        out.append('<div class="card rank">')
        out.append(f'<div class="cdec">{inline(dim)}</div>')
        out.append(
            f'<div class="order"><b>1.</b> {inline(first)} &nbsp;·&nbsp; '
            f'<b>2.</b> {inline(second)} &nbsp;·&nbsp; <b>3.</b> {inline(third)}</div>'
        )
        out.append(f'<div class="reason">{inline(reason)}</div>')
        out.append("</div>")
    out.append("</div>")
    return "\n".join(out)


def preprocess(md_text):
    """Replace the wide tables with card HTML before the markdown pass."""
    lines = md_text.split("\n")
    out, i = [], 0
    while i < len(lines):
        line = lines[i]
        if line.strip().startswith("|") and i + 1 < len(lines) and is_sep(lines[i + 1]):
            header = split_row(line)
            j = i + 2
            rows = []
            while j < len(lines) and lines[j].strip().startswith("|"):
                rows.append(split_row(lines[j]))
                j += 1
            first = header[0].strip().lower() if header else ""
            if len(header) == 7 and first == "#":
                out.append(agency_cards(header, rows))
                i = j
                continue
            if len(header) == 5 and first == "dimension":
                out.append(rank_cards(header, rows))
                i = j
                continue
        out.append(line)
        i += 1
    return "\n".join(out)


def main():
    md_text = io.open(SRC, encoding="utf-8").read()

    # strikethrough is not core markdown; the doc uses it for the voided scores
    md_text = re.sub(r"~~(.+?)~~", r"<del>\1</del>", md_text)

    body_md = preprocess(md_text)
    body = markdown.markdown(body_md, extensions=["tables", "sane_lists", "md_in_html"])

    page = HEAD + body + TAIL
    io.open(HTML, "w", encoding="utf-8").write(page)
    print("wrote", HTML, len(page), "bytes")

    chrome = next((c for c in CHROME if os.path.exists(c)), None)
    if not chrome:
        print("no chrome found, html only")
        return 1
    url = "file:///" + HTML.replace("\\", "/").replace(" ", "%20")
    subprocess.run(
        [chrome, "--headless=new", "--disable-gpu", "--no-sandbox",
         "--user-data-dir=" + os.path.join(os.environ.get("TEMP", "."), "chromepdf"),
         "--no-pdf-header-footer", "--print-to-pdf=" + PDF, url],
        check=True, timeout=180,
    )
    # Chrome has been observed to return before the PDF is fully flushed, which
    # once produced a commit containing a stale file. Wait for the size to settle.
    last, stable = -1, 0
    for _ in range(40):
        size = os.path.getsize(PDF) if os.path.exists(PDF) else 0
        stable = stable + 1 if size == last and size > 0 else 0
        if stable >= 3:
            break
        last = size
        time.sleep(0.25)

    print("wrote", PDF, os.path.getsize(PDF), "bytes")
    return 0


HEAD = """<!DOCTYPE html><html><head><meta charset="utf-8">
<title>Episode 1 Pitches v3.1</title>
<style>
  @page { size: A4; margin: 16mm 15mm 18mm; }
  body { font-family: Georgia, 'Times New Roman', serif; color: #1c1a17; margin: 0;
         font-size: 10.5pt; line-height: 1.55; }
  h1 { font-size: 20pt; letter-spacing: .2px; border-bottom: 3px solid #1c1a17;
       padding-bottom: 8px; margin: 0 0 14px; }
  h2 { font-size: 13.5pt; margin: 26px 0 8px; padding-bottom: 4px;
       border-bottom: 1.5px solid #1c1a17; page-break-after: avoid; }
  h3 { font-size: 11.5pt; margin: 18px 0 6px; color: #3a352d; page-break-after: avoid; }
  p { margin: 0 0 9px; }
  ul, ol { margin: 0 0 10px; padding-left: 20px; }
  li { margin-bottom: 4px; }
  hr { border: 0; border-top: 1px solid #ccc5b8; margin: 22px 0; }
  code { font-family: Consolas, monospace; font-size: 9pt; color: #7a5c1f;
         background: #f6f2e8; padding: 0 3px; border-radius: 2px; }
  del { color: #999; }
  blockquote { border-left: 3px solid #b9ae99; background: #faf8f2; margin: 10px 0;
               padding: 8px 14px; page-break-inside: avoid; }
  blockquote p { margin: 0 0 6px; }
  blockquote p:last-child { margin-bottom: 0; }
  table { width: 100%; border-collapse: collapse; margin: 10px 0 14px;
          font-size: 9pt; table-layout: fixed; page-break-inside: avoid; }
  th { text-align: left; background: #1c1a17; color: #f5f1e8; padding: 5px 7px;
       font-size: 8.5pt; letter-spacing: .3px; }
  td { vertical-align: top; padding: 5px 7px; border-bottom: 1px solid #e3ded2;
       word-wrap: break-word; }
  tr:nth-child(even) td { background: #faf8f2; }
  .cards { margin: 10px 0 16px; }
  .card { border: 1px solid #c9c1b0; border-left: 4px solid #1c1a17; border-radius: 3px;
          padding: 8px 11px 9px; margin-bottom: 9px; background: #fdfcf8;
          page-break-inside: avoid; font-size: 9.5pt; }
  .chead { font-size: 8.5pt; color: #6b6357; margin-bottom: 4px; letter-spacing: .3px; }
  .cid { font-weight: bold; color: #1c1a17; margin-right: 10px; }
  .cphase { margin-right: 10px; }
  .ccls { float: right; font-style: italic; }
  .cdec { font-weight: bold; margin-bottom: 6px; }
  .card dl { margin: 0; }
  .card dt { font-size: 8pt; text-transform: uppercase; letter-spacing: .6px;
             color: #8a6d3b; margin-top: 5px; }
  .card dd { margin: 1px 0 0; }
  .rank .order { margin: 2px 0 5px; font-size: 10pt; }
  .rank .reason { color: #3a352d; }
</style></head><body>
"""

TAIL = "\n</body></html>\n"

if __name__ == "__main__":
    sys.exit(main())
