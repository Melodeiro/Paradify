namespace web
{
    public class Helper
    {
        static string[] wordsToRemove = new string[] { "feat", "-", "&", " x " };

        public static string SetSearchReturnUrl(string controllerName, string searhQuery)
        {
            return "~/" + controllerName + "?q=" + searhQuery;
        }

        public static string CleanQuery(string searhQuery)
        {
            foreach (string word in wordsToRemove)
            {
                searhQuery = searhQuery.ToLower().Replace(word, "");
            }

            return searhQuery;
        }
    }
}