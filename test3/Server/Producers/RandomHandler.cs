

    // Random event generator
    public static class RandomEventGenerator
    {
        private static readonly List<string> _events = new List<string> { "Event A", "Event B", "Event C" };

        public static string GenerateEvent()
        {
            Random rnd = new Random();
            return _events[rnd.Next(_events.Count)];
        }
    }