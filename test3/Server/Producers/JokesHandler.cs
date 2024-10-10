

    // Utility class for generating topics
    public static class JokesGenerator
    {
        private static readonly List<string> _availableTopics = new List<string> { "JOKE 1", "JOKE 2", "JOKE 3" };

        public static string GetRandomJokes()
        {
            Random rnd = new Random();
            return _availableTopics[rnd.Next(_availableTopics.Count)];
        }
    }