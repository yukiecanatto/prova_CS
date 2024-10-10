

    // Utility class for generating topics
    public static class TopicGenerator
    {
        private static readonly List<string> _availableTopics = new List<string> { "TIME", "CPU", "NEWS", "SPORTS", "WEATHER" };

        public static string GetRandomTopic()
        {
            Random rnd = new Random();
            return _availableTopics[rnd.Next(_availableTopics.Count)];
        }
    }