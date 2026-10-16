import json

def esc(s):
    return (
        s.replace("\\", "\\\\")
        .replace("\n", "\\n")
        .replace("\r", "\\r")
        .replace("\t", "\\t")
    )

T = json.load(open("dump/translations.json", encoding="utf-8"))
with open("dump/translations.tsv", "w", encoding="utf-8", newline="\n") as f:
    for k, v in T.items():
        f.write(esc(k) + "\t" + esc(v) + "\n")
print("translations.tsv:", len(T))

for n in (20, 24, 48, 72):
    d = json.load(open(f"fonts/fnt_cn_{n}.json", encoding="utf-8"))
    with open(f"fonts/fnt_cn_{n}.tsv", "w", encoding="utf-8", newline="\n") as f:
        f.write(
            f"META\t{d['name']}\t{d['size']}\t{d['cellH']}\t{d['baseline']}"
            f"\t{d['atlasW']}\t{d['atlasH']}\n"
        )
        for g in d["glyphs"]:
            f.write(
                f"G\t{g['ord']}\t{g['sx']}\t{g['sy']}\t{g['sw']}\t{g['sh']}"
                f"\t{g['shift']}\t{g['offset']}\n"
            )
    print(f"fnt_cn_{n}.tsv:", len(d["glyphs"]))
