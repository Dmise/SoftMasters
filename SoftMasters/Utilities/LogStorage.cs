namespace WebApp.Utilities
{
    public static class LogStorage
    {
        private static List<string> _logs = new List<string> {"Начало логирования" };
        public static void Add(string str)
        {
            if(_logs.Count == 6)
            {
                _logs.RemoveAt(0);
            }
            _logs.Add(str);
        }
        public static string[] Getlog
        {
            get
            {
                return _logs.ToArray();
            }
        }
    }
}
