# -*- coding: utf-8 -*-
"""Dump 418 draft structure: paragraphs (style+text), tables, image refs."""
import docx, sys, io
from docx.oxml.ns import qn

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

PATH = r"Assets\从纸艺仿生到机械仿真设计应用研究报告_修订稿_418.docx"
d = docx.Document(PATH)
body = d.element.body

out = io.StringIO()
rels = d.part.rels

media = {}
for rid, rel in rels.items():
    if "media" in str(rel.target_ref):
        media[rid] = str(rel.target_ref).split("/")[-1]

pi = ti = 0
for child in body.iterchildren():
    tag = child.tag.split("}")[-1]
    if tag == "p":
        text = "".join(n.text or "" for n in child.iter(qn("w:t")))
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
        out.write(f"TABLE[{ti}] rows={len(rows)} cols={len(rows[0].findall(qn('w:tc'))) if rows else 0}\n")
        for tr in rows[:6]:
            tcs = tr.findall(qn("w:tc"))
            out.write("    " + " | ".join("".join(n.text or "" for n in tc.iter(qn("w:t")))[:80] for tc in tcs) + "\n")
        ti += 1

with open("_report_edit/_report_418_structure.txt", "w", encoding="utf-8") as f:
    f.write(out.getvalue())
print("paras:", pi, "tables:", ti, "media:", len(media))
