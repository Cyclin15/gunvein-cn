import json

def unesc(s):
    out = []
    i = 0
    while i < len(s):
        if s[i] == '\\' and i + 1 < len(s):
            n = s[i + 1]
            if n == 'n':
                out.append('\n'); i += 2; continue
            if n == 't':
                out.append('\t'); i += 2; continue
            if n == 'r':
                out.append('\r'); i += 2; continue
            if n == '\\':
                out.append('\\'); i += 2; continue
        out.append(s[i]); i += 1
    return ''.join(out)

T = json.load(open("dump/translations.json", encoding="utf-8"))
data = open("dump/translations.tsv", encoding="utf-8").read()
lines = data.split("\n")
print("tsv lines:", len(lines), "last:", repr(lines[-1][:30]))
d = {}
badtab = []
for ln in lines:
    if ln == "":
        continue
    t = ln.find("\t")
    if t < 0:
        badtab.append(ln)
        continue
    d[unesc(ln[:t])] = unesc(ln[t + 1:])
print("parsed:", len(d), "no-tab lines:", len(badtab))
for b in badtab[:10]:
    print("  NOTAB:", repr(b[:60]))
missing = [k for k in T if k not in d]
extra = [k for k in d if k not in T]
print("missing from tsv parse:", len(missing))
for k in missing[:10]:
    print("  MISS:", repr(k[:60]))
print("extra:", len(extra))
for k in extra[:10]:
    print("  EXTRA:", repr(k[:60]))
vm = [(k, T[k], d[k]) for k in T if k in d and T[k] != d[k]]
print("value mismatch:", len(vm))
for k, a, b in vm[:5]:
    print("  VM:", repr(k[:40]), repr(a[:30]), repr(b[:30]))
# also check \r presence in raw file
print("raw \\r count:", data.count("\r"))
