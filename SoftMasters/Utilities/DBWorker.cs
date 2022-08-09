using WebApp.Data;

namespace WebApp.Utilities
{
    public static class DBWorker
    {
        public static SMDbContext _dbContext;
                
        public static void Configure (SMDbContext dbContext) //TODO Dependency Injection. Configure in Program.cs  перенести методы из контроллера в отдельный класс
        {
            _dbContext = dbContext;
        }
        private static int GetCarsInDBAmount()
        {
            return _dbContext.Cars.Count();
        }

        public static int GetOperationInDBAmount()
        {
            return _dbContext.Operations.Count();
        }
        public static List<string> GetStationsInDb(SMDbContext context)
        {
            var stations = new List<string>();
            foreach(var station in context.Stations)
            {                
              stations.Add(station.Name);                
            }
            return stations;
        }

        
      

    }
}
