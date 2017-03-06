public static class StringExtension
{
    public static string[] Split(this string str, string splitter)
    {
        return str.Split(new[] { splitter }, System.StringSplitOptions.None);
    }

    public static string Trunc(this string value, byte maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}