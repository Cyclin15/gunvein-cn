// Decompile ALL code entries to files
var dir = @"D:\GunveinCN\dump\code2\";
Directory.CreateDirectory(dir);
var ctx = new GlobalDecompileContext(Data, null);
var ok = 0; var fail = 0;
var failsb = new System.Text.StringBuilder();
foreach (var code in Data.Code)
{
    var name = code.Name.Content;
    var safe = name;
    foreach (var c in Path.GetInvalidFileNameChars())
        safe = safe.Replace(c, '_');
    try
    {
        var decompiled = GetDecompiledText(code, ctx, null);
        File.WriteAllText(dir + safe + ".gml", decompiled);
        ok++;
    }
    catch (Exception e)
    {
        fail++;
        failsb.AppendLine(name + " :: " + e.Message);
    }
}
File.WriteAllText(@"D:\GunveinCN\dump\decompile_failures.txt", $"ok={ok} fail={fail}\n" + failsb.ToString());
