// Dump all strings with indices to JSON
using System.Text;
var sb = new StringBuilder();
sb.AppendLine("[");
var list = Data.Strings;
for (int i = 0; i < list.Count; i++)
{
    var s = list[i].Content;
    var escaped = s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
    sb.AppendLine($"  {{\"i\": {i}, \"s\": \"{escaped}\"}}{(i < list.Count - 1 ? "," : "")}");
}
sb.AppendLine("]");
File.WriteAllText(@"D:\GunveinCN\dump\strings.json", sb.ToString());
