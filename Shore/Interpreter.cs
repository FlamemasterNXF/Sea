using System.Reflection;
using System.Text;

namespace Shore;

public static class Interpreter
{
    public static Dictionary<string, MethodInfo> tokenFunctions = new();
    public static List<string> errors = new();

    public static void Init()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        var typeMethods =
            types.Select(t => t.GetMethods().Where(m => m.GetCustomAttributes<KeywordAttribute>().Any()));

        foreach (var methods in typeMethods)
        {
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttributes<KeywordAttribute>().First();
                tokenFunctions.Add(attribute.keyword, method);
            }
        }
    }

    public static string Interpret(string data)
    {
        var split = data.Split("\n");
        StringBuilder sb = new();
        for (var i = 0; i < split.Length; i++)
        {
            try
            {
                var line = split[i];
                if (line.StartsWith("global") || line.StartsWith("local")) line = "def " + line;

                var index = line.IndexOf(" ", StringComparison.Ordinal);
                var token = line[..index];
                line = line[(index + 1)..];

                sb.Append((string) tokenFunctions[token].Invoke(null, new object[] { line })!).Append('\n');
            }
            catch (Exception e)
            {
                errors.Add($"{e.Message} on line ${i}");
            }
        }

        return sb.ToString();
    }

    [Keyword("def")]
    public static string Def(string data)
    {
        var split = data.Split(" ").ToList();

        //A-MOD
        if (split[0] is "global" or "local")
        {
            split[0] = split[0] == "global" ? "public" : "private";
        }

        //MOD
        if (split[1] is "unsigned")
        {
            split[2] = 'u' + split[2];
            split.RemoveAt(1);
        }

        //TYPE
        split[1] = split[1] switch
        {
            "byte" or "int8" => "sbyte",
            "int16" => "short",
            "int32" => "int",
            "int64" => "long",
            "float32" => "float",
            "float64" => "double",
            "float128" => "decimal",

            "ubyte" or "uint8" => "byte",
            "uint16" => "ushort",
            "uint32" => "uint",
            "uint64" => "ulong",

            _ => split[1]
        };

        var together = string.Join(" ", split);
        return (together.EndsWith(";") ? together : together + ";");
    }

    [Keyword("print")] public static string Print(string data) => $"Console.WriteLine({data.Trim()});";
}