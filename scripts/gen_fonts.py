#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Generate CJK font atlases + glyph metric JSON for Gunvein Chinese patch.
Renders Fusion Pixel (12px base) at integer multiples to keep pixel-perfect look."""
import json, sys, os
from PIL import Image, ImageFont

sys.path.insert(0, r"D:\GunveinCN")
from translations import T

TOOLS = r"D:\GunveinCN\tools\fonts"
OUT = r"D:\GunveinCN\fonts"
os.makedirs(OUT, exist_ok=True)

FP10 = os.path.join(TOOLS, "10prop", "fusion-pixel-10px-proportional-zh_hans.ttf")
FP12 = os.path.join(TOOLS, "12prop", "fusion-pixel-12px-proportional-zh_hans.ttf")

# ---------- collect charset ----------
strs = json.load(open(r"D:\GunveinCN\dump\strings.json", encoding="utf-8"))
final_contents = []
for e in strs:
    s = e["s"]
    final_contents.append(T.get(s, s))

need = set()
for s in final_contents:
    for ch in s:
        o = ord(ch)
        if o < 0x10000 and (o >= 32 or ch in "\n\r\t"):
            need.add(ch)
# extras: full ASCII, Latin-1 supplement, Greek, Cyrillic, kana, CJK punct, fullwidth forms
extra_ranges = [
    (0x20, 0x7E),      # ASCII
    (0xA0, 0xFF),      # Latin-1
    (0x0370, 0x03FF),  # Greek
    (0x0400, 0x04FF),  # Cyrillic
    (0x3040, 0x30FF),  # kana
    (0x3000, 0x303F),  # CJK punct
    (0xFF00, 0xFF5F),  # fullwidth forms
    (0x2010, 0x2027),  # dashes, ellipsis
    (0x00B7, 0x00B7),  # middle dot
]
for a, b in extra_ranges:
    for o in range(a, b + 1):
        need.add(chr(o))
# only chars the font actually has
charset = sorted(need)

# ---------- font configs ----------
# (name, ttf, base_px, scale)  -> rendered at base_px then upscaled NEAREST by scale
CONFIGS = [
    ("fnt_cn_20", FP10, 10, 2),   # replaces nullfnt_small, 2x size
    ("fnt_cn_24", FP12, 12, 2),   # replaces nullfnt, 2x size
    ("fnt_cn_48", FP12, 12, 4),   # replaces nullfnt_pbig, 2x size
    ("fnt_cn_72", FP12, 12, 6),   # replaces nullfnt_massive, 2x size
]

report = []
for name, ttf_path, base, scale in CONFIGS:
    fpath = os.path.join(OUT, name + ".ttf_check")
    font = ImageFont.truetype(ttf_path, base)
    ascent, descent = font.getmetrics()
    # check coverage
    missing = [ch for ch in charset if font.getlength(ch) == 0]
    # kerning irrelevant
    PAD = 1
    cellH = (ascent + descent + 2 * PAD) * scale
    baseline = (ascent + PAD) * scale  # ink top of a cap glyph = baseline - capheight
    glyphs = []
    for ch in charset:
        if ch in "\n\r\t":
            ch = " "
        if font.getlength(ch) == 0:
            continue
        bbox = font.getbbox(ch)  # (x0,y0,x1,y1) relative to left-ascender origin
        x0, y0, x1, y1 = bbox
        w = max(1, x1 - x0)
        h = max(1, y1 - y0)
        from PIL import ImageDraw
        tmp = Image.new("L", (x1 + 8, y1 + 8), 0)
        ImageDraw.Draw(tmp).text((0, 0), ch, font=font, fill=255, anchor="la")
        img = tmp.crop((x0, y0, x1, y1)) if (x1 > x0 and y1 > y0) else Image.new("L", (1, 1), 0)
        if scale > 1:
            img = img.resize((img.width * scale, img.height * scale), Image.NEAREST)
        shift = round(font.getlength(ch)) * scale
        glyphs.append({
            "ch": ch, "ord": ord(ch), "img": img,
            "ink_top": y0 * scale,  # offset from cell top to ink top
            "shift": shift,
        })
    # pack into atlas: fixed row height = cellH, cells separated by 1*scale gap
    gap = scale
    maxAtlas = 4096
    # choose atlas width
    totalW = sum(g["img"].width + gap for g in glyphs)
    for aw in (512, 1024, 2048, 4096):
        if totalW / aw * cellH <= aw * 0.98:
            break
    # simple shelf packing
    atlas = Image.new("RGBA", (aw, maxAtlas), (0, 0, 0, 0))
    x, y = 0, 0
    placed = []
    for g in glyphs:
        w = g["img"].width
        if x + w + gap > aw:
            x = 0
            y += cellH + gap
        if y + cellH > maxAtlas:
            raise RuntimeError(f"atlas overflow for {name}")
        # paste: white RGB, alpha = glyph coverage
        rgba = Image.new("RGBA", (w, g["img"].height), (255, 255, 255, 0))
        rgba.putalpha(g["img"])
        atlas.paste(rgba, (x, y + g["ink_top"]))
        placed.append({
            "ch": g["ch"], "ord": g["ord"],
            "sx": x, "sy": y + g["ink_top"],
            "sw": w, "sh": g["img"].height,
            "shift": g["shift"], "offset": 0,
        })
        x += w + gap
    # crop atlas to used height, pad to pow2
    usedH = y + cellH
    atlasH = 1
    while atlasH < usedH:
        atlasH *= 2
    atlas = atlas.crop((0, 0, aw, atlasH))
    png = os.path.join(OUT, name + ".png")
    atlas.save(png)
    meta = {
        "name": name, "size": base * scale, "cellH": cellH, "baseline": baseline,
        "ascent": ascent * scale, "descent": descent * scale,
        "atlasW": aw, "atlasH": atlasH, "glyphs": placed,
    }
    json.dump(meta, open(os.path.join(OUT, name + ".json"), "w", encoding="utf-8"),
              ensure_ascii=False)
    report.append(f"{name}: size={base*scale}px glyphs={len(placed)} atlas={aw}x{usedH} missing={len(missing)}")
    if missing:
        ms = "".join(missing)
        report.append(f"   missing chars: {ms[:200]}")

open(os.path.join(OUT, "report.txt"), "w", encoding="utf-8").write("\n".join(report))
print("\n".join(report))
