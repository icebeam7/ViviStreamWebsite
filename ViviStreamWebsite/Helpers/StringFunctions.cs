namespace ViviStreamWebsite.Helpers
{
    public static class StringFunctions
    {
        public static string ReplaceChars(string text)
        {
            return text.ToLower().Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u');
        }

        public static string RemoveChars(string text)
        {
            return text.Replace("-", string.Empty).Replace("/", string.Empty).Replace("&", string.Empty)
                        .Replace("\'", string.Empty).Replace("\"", string.Empty).Replace(":", string.Empty)
                        .Replace(".", string.Empty).Replace(",", string.Empty).Replace("#", string.Empty)
                        .Replace("(", string.Empty).Replace(")", string.Empty);
        }
    }
}
