namespace Shore;

public static class Helper
{
    public static string Repeat(this string repeater, int amount)
    {
        return string.Join("", Enumerable.Repeat(repeater, amount));
    }
}