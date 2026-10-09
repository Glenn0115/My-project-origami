# -*- coding: utf-8 -*-
"""Dump docx body structure: paragraphs (style+text), tables (dims+texts), image refs."""
import docx, sys, io
from docx.oxml.ns import qn

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

PATH = r"Assets\艺仿生到机械仿真设计应用研究报告_108_排版.docx"
d = docx.Document(PATH)
body = d.element.body

out = io.StringIO()
rels = d.part.rels

# map rId -> media filename
media = {}
for rid, rel in rels.items():
    if "media" in str(rel.target_ref):
        media[rid] = str(rel.target_ref).split("/")[-1]

pi = 0  # paragraph index
ti = 0  # table index
for child in body.iterchildren():
    tag = child.tag.split("}")[-1]
    if tag == "p":
        text = "".join(n.text or "" for n in child.iter(qn("w:t")))
        # check for images
        imgs = []
        for blip in child.iter(qn("a:blip")):
            rid = blip.get(qn("r:embed"))
            if rid:
                imgs.append(f"{rid}:{media.get(rid, '?')}")
        style = child.find(qn("w:pPr") + "/" + qn("w:pStyle"))
        stname = style.get(qn("w:val")) if style is not None else "-"
        out.write(f"P[{pi}] (st={stname}) {text!r}" + (f"  IMG[{','.join(imgs)}]" if imgs else "") + "\n")
        pi += 1
    elif tag == "tbl":
        rows = child.findall(qn("w:tr"))
        cells_text = []
        for tr in rows[:4]:
            tcs = tr.findall(qn("w:tc"))
            cells_text.append(" | ".join("".join(n.text or "" for n in tc.iter(qn("w:t")))[:60] for tc in tcs))
        out.write(f"TABLE[{ti}] rows={len(rows)} cols={len(rows[0].findall(qn('w:tc'))) if rows else 0}\n")
        for r in cells_text:
            out.write(f"    {r}\n")
        ti += 1

with open("_report_edit/_report_structure.txt", "w", encoding="utf-8") as f:
    f.write(out.getvalue())
print("paras:", pi, "tables:", ti)
print("media files:", len(media))
