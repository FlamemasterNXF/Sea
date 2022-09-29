namespace Shore;

[AttributeUsage(AttributeTargets.Method)]
public class KeywordAttribute: Attribute
{
    public string keyword;

    public KeywordAttribute(string keyword) => this.keyword = keyword;
}