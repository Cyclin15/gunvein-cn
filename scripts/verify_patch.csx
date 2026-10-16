using System.Text;
using UndertaleModLib.Util;
var sb = new StringBuilder();
sb.AppendLine("fonts: " + Data.Fonts.Count);
foreach (var f in Data.Fonts)
    sb.AppendLine("  " + f.Name.Content + " em=" + f.EmSize + " asc=" + f.Ascender + " ascOff=" + f.AscenderOffset + " lineH=" + f.LineHeight + " glyphs=" + f.Glyphs.Count + " tex=" + (f.Texture != null ? f.Texture.Name.Content : "null"));
var it = Data.Fonts[1];
string zhInfo = "NOT FOUND";
foreach (var g in it.Glyphs)
    if (g.Character == 0x4E2D) { zhInfo = "src=(" + g.SourceX + "," + g.SourceY + "," + g.SourceWidth + "," + g.SourceHeight + ") shift=" + g.Shift; break; }
sb.AppendLine("inteptor glyph U+4E2D(中): " + zhInfo);
var fnt = Data.Fonts[2];
var img = fnt.Texture.TexturePage.TextureData.Image;
sb.AppendLine("fnt_cn_10 tex: " + fnt.Texture.TexturePage.Name.Content + " " + img.Width + "x" + img.Height + " format=" + img.Format);
int found = 0;
foreach (var s in Data.Strings)
{
    if (s.Content != null && (s.Content.Contains("开始游戏") || s.Content.Contains("排行榜") || s.Content.Contains("碎片")))
    { found++; if (found <= 6) sb.AppendLine("  STRG: " + s.Content.Replace("\n", "\n")); }
}
sb.AppendLine("cn strings found: " + found);
foreach (var code in Data.Code)
{
    if (code.Name.Content != "gml_GlobalScript_boghog_functions") continue;
    var ctx = new GlobalDecompileContext(Data, null);
    var txt = GetDecompiledText(code, ctx, null);
    foreach (var ln in txt.Split('\n'))
    {
        var t = ln.Trim();
        if (t.StartsWith("global.fnt_mid") || t.StartsWith("global.fnt_small") || t.StartsWith("global.fnt_massive") || t.StartsWith("global.fnt_big"))
            sb.AppendLine("  CODE: " + t);
    }
    break;
}
File.WriteAllText(@"D:\GunveinCN\dump\verify_patch.txt", sb.ToString());
