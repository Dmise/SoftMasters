using WebApp.Data;
using WebApp.Models;

namespace WebApp.Utilities
{
    public class DBWorker
    {
        public static SMDbContext _dbContext;

        public DBWorker(SMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static void Configure(SMDbContext dbContext) //TODO Dependency Injection. Configure in Program.cs  перенести методы из контроллера в отдельный класс
        {
            _dbContext = dbContext;
        }
        private int GetCarsInDBAmount()
        {
            return _dbContext.Cars.Count();
        }

        public static int GetOperationInDBAmount
        {
            get { 
                return _dbContext.Operations.Count();
            }
        }

        public async Task LoadToDataBaseAsync(string filePath)
        {
            List<InvoiceXML> invList;
            XMLWorker.CreateInvoicesList(filePath, out invList);
            // read data from DB to local RAM
            var stationInDBRAM = _dbContext.Stations.ToList(); // можно использовать  LINQ чтобы подгрузить только значения полей, а не сущнсоти полность _dbContext.Trains.Select(t => t.TrainId).ToList();
            var savedTrainsRAM = _dbContext.Trains.ToList();
            var operationsNamesInDBRAM = _dbContext.OperationNames.ToList();
            var freightsInDBRAM = _dbContext.Freights.ToList();
            var invoicesInDBRAM = _dbContext.Invoices.ToList();
            var savedCompositionRAM = _dbContext.Compositions.ToList();
            var savedOperationsRAM = _dbContext.Operations.ToList();

            List<Car> savedCars = new List<Car>();
            
            
            
                                 
            //Parallel.For<> .//TODO сделать данный цикл параллельным  Parallel.For Loop
            // TODO chunks

            try
            {
                // https://metanit.com/sharp/tutorial/12.4.php
                // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallelloopresult?view=net-6.0
                ParallelLoopResult result = Parallel.ForEach<InvoiceXML>(invList, ProcessRow);
                

            _dbContext.SaveChanges();
            LogStorage.Add("Данные загружены на сервер");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            void ProcessRow(InvoiceXML row)
            {
                var messageToLog = $"Обработка строки. Вагон: {row.CarNumber}; Время операции: {row.WhenLastOperation}"; // TODO jquery ajax  динамическая загрузка строк лога на html страницу 
                LogStorage.Add(messageToLog);

                Station? currentRowToStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.ToStationName);
                Station? currentRowFromStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.FromStationName);
                Station? currentRowLastStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.LastStationName);
                //work with stations
                if (currentRowToStation == null)
                {
                    currentRowToStation = new Station { Name = row.ToStationName };
                    _dbContext.Stations.Add(currentRowToStation);
                    stationInDBRAM.Add(currentRowToStation);
                }

                if (currentRowFromStation == null)
                {
                    currentRowFromStation = new Station { Name = row.FromStationName };
                    _dbContext.Stations.Add(currentRowFromStation);
                    stationInDBRAM.Add(currentRowFromStation);
                }

                if (currentRowLastStation == null)
                {
                    currentRowLastStation = new Station { Name = row.LastStationName };
                    _dbContext.Stations.Add(currentRowLastStation);
                    stationInDBRAM.Add(currentRowLastStation);
                }
                // _dbContext.Entry(currentRowLastStation).State = EntityState.Detached;  MARK

                //work with trains
                Train? currentRowTrain = savedTrainsRAM.FirstOrDefault(t => t.TrainId == row.TrainNumber);
                if (currentRowTrain == null)
                {
                    currentRowTrain = _dbContext.Trains.FirstOrDefault(t => t.TrainId == row.TrainNumber);
                    if (currentRowTrain == null)
                    {
                        currentRowTrain = new Train
                        {
                            TrainId = row.TrainNumber,
                            FromStationName = currentRowFromStation.Name,
                            ToStationName = currentRowToStation.Name,
                            fromStation = currentRowFromStation,
                            toStation = currentRowToStation
                        };
                        _dbContext.Trains.Add(currentRowTrain);
                    }
                    savedTrainsRAM.Add(currentRowTrain);

                }

                //work with Opernames
                OperationName? currentOperName = operationsNamesInDBRAM.FirstOrDefault(o => o.Name == row.LastOperationName);
                if (currentOperName == null)
                {
                    currentOperName = new OperationName
                    {
                        Name = row.LastOperationName
                    };
                    _dbContext.OperationNames.Add(currentOperName);
                    operationsNamesInDBRAM.Add(currentOperName);
                }

                // work with Freights
                Freight? currentFreight = freightsInDBRAM.FirstOrDefault(f => f.Name == row.FreightEtsngName);

                if (currentFreight == null)
                {
                    currentFreight = new Freight { Name = row.FreightEtsngName };
                    _dbContext.Freights.Add(currentFreight);
                    freightsInDBRAM.Add(currentFreight);
                }


                //work with Compositions (CombinedTrainIndex)
                Composition? currentRowCompisition = savedCompositionRAM.FirstOrDefault(c => c.CombinedTrainIndex == row.TrainIndexCombined);
                if (currentRowCompisition == null)
                {

                    currentRowCompisition = _dbContext.Compositions.FirstOrDefault(c => c.CombinedTrainIndex == row.TrainIndexCombined);
                    if (currentRowCompisition == null)
                    {

                        currentRowCompisition = new Composition
                        {
                            CombinedTrainIndex = row.TrainIndexCombined,
                            TrainId = currentRowTrain.TrainId,
                            Train = currentRowTrain
                        };
                        _dbContext.Compositions.Add(currentRowCompisition);
                        savedCompositionRAM.Add(currentRowCompisition);
                    }
                }

                //process invoices 
                Invoice? currentInvoice = invoicesInDBRAM.FirstOrDefault(i => i.InvoiceNumber == row.InvoiceNum);
                if (currentInvoice == null)
                {
                    currentInvoice = new Invoice { InvoiceNumber = row.InvoiceNum };
                    _dbContext.Invoices.Add(currentInvoice);
                    invoicesInDBRAM.Add(currentInvoice);
                }

                //process cars                              
                Car? currentRowCar = savedCars.FirstOrDefault(c => c.CarNumber == row.CarNumber);
                if (currentRowCar == null)
                {
                    currentRowCar = _dbContext.Cars.FirstOrDefault(c => c.CarNumber == row.CarNumber);
                    if (currentRowCar == null)
                    {
                        currentRowCar = new Car
                        {
                            CarNumber = row.CarNumber,
                            PositionInTarin = row.PositionInTrain,
                            InvoiceNumber = currentInvoice.InvoiceNumber,
                            FreightName = currentFreight.Name,
                            Weight = row.FreightTotalWeightKg,
                            CompositionNumber = currentRowCompisition.CombinedTrainIndex,
                            _Composition = currentRowCompisition,
                            _Freight = currentFreight,
                            _Invoice = currentInvoice
                        };
                        _dbContext.Cars.Add(currentRowCar);
                    }
                    savedCars.Add(currentRowCar);
                }

                Operation? currentRowOperation = savedOperationsRAM.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == DateTime.Parse(row.WhenLastOperation));
                if (currentRowOperation == null)
                {
                    currentRowOperation = _dbContext.Operations.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == DateTime.Parse(row.WhenLastOperation));
                    if (currentRowOperation == null)
                    {
                        currentRowOperation = new Operation
                        {
                            WhenLastOperation = DateTime.Parse(row.WhenLastOperation),
                            CarNumber = row.CarNumber,
                            LastOperationName = row.LastOperationName,
                            StationName = row.LastStationName,
                            _Car = currentRowCar,
                            _OperationName = currentOperName,
                            _Station = currentRowLastStation
                        };
                        _dbContext.Operations.Add(currentRowOperation);
                    }
                    savedOperationsRAM.Add(currentRowOperation);
                }
            }
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

    // https://stackoverflow.com/questions/40602585/update-database-using-ef-with-chunks-runs-very-slowly
    public static class EntityFrameworkUtil
    {
        public static IEnumerable<T> QueryInChunksOf<T>(this IQueryable<T> queryable, int chunkSize) // TODO change to IQueryable
        {
            return queryable.QueryChunksOfSize(chunkSize).SelectMany(chunk => chunk);
        }

        public static IEnumerable<T[]> QueryChunksOfSize<T>(this IQueryable<T> queryable, int chunkSize)
        {
            int chunkNumber = 0;
            while (true)
            {
                var query = (chunkNumber == 0)
                    ? queryable
                    : queryable.Skip(chunkNumber * chunkSize);
                var chunk = query.Take(chunkSize).ToArray();
                if (chunk.Length == 0)
                    yield break;
                yield return chunk;
                chunkNumber++;
            }
        }
    }
}
