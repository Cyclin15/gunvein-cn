import os
import zipfile

SRC_WIN = r"D:\GunveinCN\out\data.win"
SRC_TXT = r"D:\GunveinCN\dist\安装说明.txt"
DST = r"D:\GunveinCN\dist\Gunvein简体中文汉化补丁.zip"

# confirm source is the v2 patched file
print("data.win size:", os.path.getsize(SRC_WIN))

with zipfile.ZipFile(DST, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=1) as z:
    z.write(SRC_WIN, "data.win")
    z.write(SRC_TXT, "安装说明.txt")

print("zip size:", os.path.getsize(DST))
with zipfile.ZipFile(DST) as z:
    for i in z.infolist():
        print(" ", i.filename, i.file_size, "->", i.compress_size)
