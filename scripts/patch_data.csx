// Gunvein 汉化补丁：
// 1. 新建 4 个 CJK 字体 fnt_cn_20/24/48/72（新纹理页 + PageItem + 字形表，IDE 字体结构）
// 2. 替换 fnt_inteptor 的纹理与字形（fnt_cn_48 图集，48px）
// 3. CodeImportGroup 重编译 boghog_functions 中 4 行字体初始化，指向新字体
// 4. 应用 408 条字符串翻译
using System.Text;
using UndertaleModLib.Util;

var log = new StringBuilder();
string baseDir = @"D:\GunveinCN";

string Unesc(string s)
{
    var sb = new StringBuilder();
    for (int i = 0; i < s.Length; i++)
    {
        if (s[i] == '\\' && i + 1 < s.Length)
        {
            char n = s[i + 1];
            if (n == 'n') { sb.Append('\n'); i++; continue; }
            if (n == 't') { sb.Append('\t'); i++; continue; }
            if (n == 'r') { sb.Append('\r'); i++; continue; }
            if (n == '\\') { sb.Append('\\'); i++; continue; }
        }
        sb.Append(s[i]);
    }
    return sb.ToString();
}

// ---------- 读入翻译表 ----------
var translations = new Dictionary<string, string>();
foreach (var line in File.ReadAllLines(baseDir + @"\dump\translations.tsv"))
{
    int t = line.IndexOf('\t');
    if (t < 0) continue;
    translations[Unesc(line.Substring(0, t))] = Unesc(line.Substring(t + 1));
}
log.AppendLine("translations loaded: " + translations.Count);

// ---------- 字体构造 ----------
// 返回新建字体；结构仿照 IDE 生成字体（含 Graze Counter GM 官方中文字体 txt_china_t）：
// 统一框高 cellH、框内基线行 = baseline、Ascender = baseline、AscenderOffset = 0
UndertaleFont BuildFont(string name)
{
    var metricsLines = File.ReadAllLines(baseDir + @"\fonts\" + name + ".tsv");
    string name_ = null; int em = 0, cellH = 0, baseline = 0, atlasW = 0, atlasH = 0;
    var glyphRows = new List<int[]>();
    foreach (var line in metricsLines)
    {
        var p = line.Split('\t');
        if (p[0] == "META")
        {
            name_ = p[1]; em = int.Parse(p[2]); cellH = int.Parse(p[3]);
            baseline = int.Parse(p[4]); atlasW = int.Parse(p[5]); atlasH = int.Parse(p[6]);
        }
        else if (p[0] == "G")
        {
            var a = new int[7];
            for (int i = 0; i < 7; i++) a[i] = int.Parse(p[i + 1]);
            glyphRows.Add(a);
        }
    }

    var png = File.ReadAllBytes(baseDir + @"\fonts\" + name + ".png");
    var img = GMImage.FromPng(png);

    var tex = new UndertaleEmbeddedTexture();
    tex.Name = Data.Strings.MakeString("Texture " + Data.EmbeddedTextures.Count);
    tex.TextureData = new UndertaleEmbeddedTexture.TexData();
    tex.TextureData.Image = img;
    tex.TextureWidth = img.Width;
    tex.TextureHeight = img.Height;
    Data.EmbeddedTextures.Add(tex);

    var item = new UndertaleTexturePageItem();
    item.Name = Data.Strings.MakeString("PageItem " + Data.TexturePageItems.Count);
    item.TexturePage = tex;
    item.SourceX = 0; item.SourceY = 0;
    item.SourceWidth = (ushort)atlasW; item.SourceHeight = (ushort)atlasH;
    item.TargetX = 0; item.TargetY = 0;
    item.TargetWidth = (ushort)atlasW; item.TargetHeight = (ushort)atlasH;
    item.BoundingWidth = (ushort)atlasW; item.BoundingHeight = (ushort)atlasH;
    Data.TexturePageItems.Add(item);

    var font = new UndertaleFont();
    font.Name = Data.Strings.MakeString(name);
    font.DisplayName = Data.Strings.MakeString(name);
    font.EmSizeIsFloat = true;
    font.EmSize = em;
    font.Bold = false; font.Italic = false;
    font.RangeStart = 32;
    font.Charset = 0; font.AntiAliasing = 1;
    font.ScaleX = 1; font.ScaleY = 1;
    font.Ascender = (uint)baseline;   // 框内基线行
    font.SDFSpread = 0;
    font.LineHeight = (uint)cellH;
    font.AscenderOffset = 0;
    font.Texture = item;
    font.Glyphs = new UndertalePointerList<UndertaleFont.Glyph>();
    ushort maxCh = 32;
    foreach (var a in glyphRows)
    {
        var g = new UndertaleFont.Glyph();
        g.Character = (ushort)a[0];
        g.SourceX = (ushort)a[1];
        g.SourceY = (ushort)a[2];
        g.SourceWidth = (ushort)a[3];
        g.SourceHeight = (ushort)a[4];
        g.Shift = (short)a[5];
        g.Offset = (short)a[6];
        g.UnknownAlwaysZero = 0;
        font.Glyphs.Add(g);
        if (g.Character > maxCh) maxCh = g.Character;
    }
    font.RangeEnd = (uint)maxCh;
    Data.Fonts.Add(font);
    log.AppendLine("font built: " + name + " em=" + em + " cellH=" + cellH + " baseline=" + baseline
        + " atlas=" + atlasW + "x" + atlasH + " glyphs=" + glyphRows.Count
        + " tex=" + tex.Name.Content + " item=" + item.Name.Content);
    return font;
}

BuildFont("fnt_cn_20");
BuildFont("fnt_cn_24");
BuildFont("fnt_cn_48");
BuildFont("fnt_cn_72");

// ---------- 替换 fnt_inteptor ----------
UndertaleFont inteptor = null;
foreach (var f in Data.Fonts)
    if (f.Name.Content == "fnt_inteptor") { inteptor = f; break; }
if (inteptor == null) throw new Exception("fnt_inteptor not found");

{
    var metricsLines = File.ReadAllLines(baseDir + @"\fonts\fnt_cn_48.tsv");
    int cellH = 0, baseline = 0, atlasW = 0, atlasH = 0, em = 0;
    var glyphRows = new List<int[]>();
    foreach (var line in metricsLines)
    {
        var p = line.Split('\t');
        if (p[0] == "META")
        {
            em = int.Parse(p[2]);
            cellH = int.Parse(p[3]); baseline = int.Parse(p[4]);
            atlasW = int.Parse(p[5]); atlasH = int.Parse(p[6]);
        }
        else if (p[0] == "G")
        {
            var a = new int[7];
            for (int i = 0; i < 7; i++) a[i] = int.Parse(p[i + 1]);
            glyphRows.Add(a);
        }
    }
    var png = File.ReadAllBytes(baseDir + @"\fonts\fnt_cn_48.png");
    var img = GMImage.FromPng(png);

    var tex = new UndertaleEmbeddedTexture();
    tex.Name = Data.Strings.MakeString("Texture " + Data.EmbeddedTextures.Count);
    tex.TextureData = new UndertaleEmbeddedTexture.TexData();
    tex.TextureData.Image = img;
    tex.TextureWidth = img.Width;
    tex.TextureHeight = img.Height;
    Data.EmbeddedTextures.Add(tex);

    var item = new UndertaleTexturePageItem();
    item.Name = Data.Strings.MakeString("PageItem " + Data.TexturePageItems.Count);
    item.TexturePage = tex;
    item.SourceX = 0; item.SourceY = 0;
    item.SourceWidth = (ushort)atlasW; item.SourceHeight = (ushort)atlasH;
    item.TargetX = 0; item.TargetY = 0;
    item.TargetWidth = (ushort)atlasW; item.TargetHeight = (ushort)atlasH;
    item.BoundingWidth = (ushort)atlasW; item.BoundingHeight = (ushort)atlasH;
    Data.TexturePageItems.Add(item);

    inteptor.EmSize = em;
    inteptor.Ascender = (uint)baseline;
    inteptor.AscenderOffset = 0;
    inteptor.LineHeight = (uint)cellH;
    inteptor.Texture = item;
    var glyphs = new UndertalePointerList<UndertaleFont.Glyph>();
    ushort maxCh = 32;
    foreach (var a in glyphRows)
    {
        var g = new UndertaleFont.Glyph();
        g.Character = (ushort)a[0];
        g.SourceX = (ushort)a[1];
        g.SourceY = (ushort)a[2];
        g.SourceWidth = (ushort)a[3];
        g.SourceHeight = (ushort)a[4];
        g.Shift = (short)a[5];
        g.Offset = (short)a[6];
        g.UnknownAlwaysZero = 0;
        glyphs.Add(g);
        if (g.Character > maxCh) maxCh = g.Character;
    }
    inteptor.Glyphs = glyphs;
    inteptor.RangeEnd = (uint)maxCh;
    log.AppendLine("fnt_inteptor replaced: glyphs=" + glyphRows.Count + " asc=" + inteptor.Ascender
        + " lineH=" + inteptor.LineHeight + " tex=" + tex.Name.Content);
}

// ---------- 代码补丁：字体初始化指向新字体 ----------
var ctx = new GlobalDecompileContext(Data, null);
var importer = new CodeImportGroup(Data, ctx, null);
string charset = " {}()<>[]~-+=#*%&^',._/:;!?@0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
importer.QueueFindReplace("gml_GlobalScript_boghog_functions",
    "global.fnt_mid = font_add_sprite_ext(s_nullfnt, \"" + charset + "\", false, 0);",
    "global.fnt_mid = fnt_cn_24;", true);
importer.QueueFindReplace("gml_GlobalScript_boghog_functions",
    "global.fnt_small = font_add_sprite_ext(s_nullfnt_small, \"" + charset + "\", false, 0);",
    "global.fnt_small = fnt_cn_20;", true);
importer.QueueFindReplace("gml_GlobalScript_boghog_functions",
    "global.fnt_massive = font_add_sprite_ext(s_nullfnt_massive, \"" + charset + "\", false, 0);",
    "global.fnt_massive = fnt_cn_72;", true);
importer.QueueFindReplace("gml_GlobalScript_boghog_functions",
    "global.fnt_big = font_add_sprite_ext(s_nullfnt_pbig, \"" + charset + "\", false, 0);",
    "global.fnt_big = fnt_cn_48;", true);
importer.Import(true);
log.AppendLine("code import done");

// 验证重编译结果
foreach (var code in Data.Code)
{
    if (code.Name.Content != "gml_GlobalScript_boghog_functions") continue;
    var txt = GetDecompiledText(code, ctx, null);
    var lines = txt.Split('\n');
    foreach (var ln in lines)
    {
        var s = ln.Trim();
        if (s.StartsWith("global.fnt_"))
            log.AppendLine("  >> " + s);
    }
    break;
}

// ---------- 字符串翻译 ----------
int hits = 0;
var matchedKeys = new HashSet<string>();
foreach (var s in Data.Strings)
{
    string c = s.Content;
    if (c == null) continue;
    string v;
    if (translations.TryGetValue(c, out v))
    {
        s.Content = v;
        hits++;
        matchedKeys.Add(c);
    }
}
var unmatched = new List<string>();
foreach (var k in translations.Keys)
    if (!matchedKeys.Contains(k)) unmatched.Add(k);
log.AppendLine("string replacements: " + hits + " (translation entries: " + translations.Count + ", matched keys: " + matchedKeys.Count + ")");
log.AppendLine("unmatched keys: " + unmatched.Count);
foreach (var k in unmatched)
    log.AppendLine("  MISS: " + k.Replace("\n", "\\n"));

log.AppendLine("fonts now: " + Data.Fonts.Count + ", textures: " + Data.EmbeddedTextures.Count + ", pageitems: " + Data.TexturePageItems.Count);
File.WriteAllText(baseDir + @"\dump\patch_log.txt", log.ToString());
