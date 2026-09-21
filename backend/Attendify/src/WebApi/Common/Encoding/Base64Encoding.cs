namespace Attendify.Common.Encoding;

public static class Base64Encoding
{
    public static string Encode(byte[] value)
    {
        return Convert.ToBase64String(value);
    }

    public static byte[] Decode(string base64String)
    {
        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                "The provided value is not valid Base64.",
                nameof(base64String),
                exception
            );
        }
    }
}
