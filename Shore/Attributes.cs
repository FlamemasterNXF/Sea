namespace Shore;

[AttributeUsage(AttributeTargets.Method)]
public class KeywordAttribute : Attribute
{
    public readonly string keyword;

    public KeywordAttribute(string keyword) => this.keyword = keyword;
}