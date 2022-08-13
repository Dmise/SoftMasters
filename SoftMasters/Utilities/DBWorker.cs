using WebApp.Data;
using WebApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using SoftMasters.test.Utilities;

namespace WebApp.Utilities
{
    public static class DBWorker
    {
        private static SMDbContext? _dbContext;

        private static readonly object _dblock = new object();
        private static readonly object _lockStations = new object();
        private static readonly object _lockTrains = new object();
        private static readonly object _lockCompositions = new object();
        private static readonly object _lockInvoices = new object();
        private static readonly object _lockOperations = new object();
        private static readonly object _lockCars = new object();


        //public DBWorker(SMDbContext dbContext)
        //{
        //    if(_dbContext == null)
        //        _dbContext = dbContext;
        //}

        public static void Configure(SMDbContext dbContext) //TODO Dependency Injection. Configure in Program.cs  перенести методы из контроллера в отдельный класс
        {
            _dbContext = dbContext;
        }
        
        public static List<int> GetTrainsIDs
        {
            get
            {
                if(_dbContext != null)
                    return _dbContext.Trains.Select(t => t.TrainId).ToList();
                return new List<int>();
            }
        }

        public static int GetOperationInDBAmount
        {
            get
            {
                var cars = _dbContext.Cars.ToList();
                var d = _dbContext;
                return _dbContext.Operations.Count();
            }
        }

        public async static Task LoadToDataBaseAsync(string filePath)
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

            List<Car> savedCars = _dbContext.Cars.ToList();

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

                Station? currentRowToStation;
                Station? currentRowFromStation;
                Station? currentRowLastStation;
                Invoice? currentInvoice;
                Car? currentRowCar;
                Freight? currentRowFreight;
                Train? currentRowTrain;
                Composition? currentRowCompisition;
                Operation? currentRowOperation;
                OperationName? currentOperName;



                lock (_lockStations)
                {
                    currentRowToStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.ToStationName);
                    currentRowFromStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.FromStationName);
                    currentRowLastStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.LastStationName);


                    //work with stations
                    if (currentRowToStation == null)
                    {
                        currentRowToStation = new Station { Name = row.ToStationName };
                        lock (_dblock)
                        {
                            _dbContext.Stations.Add(currentRowToStation);
                        }
                        stationInDBRAM.Add(currentRowToStation);
                    }


                    if (currentRowFromStation == null)
                    {
                        currentRowFromStation = new Station { Name = row.FromStationName };
                        lock (_dblock)
                        {
                            _dbContext.Stations.Add(currentRowFromStation);
                        }
                        stationInDBRAM.Add(currentRowFromStation);
                    }

                    if (currentRowLastStation == null)
                    {
                        currentRowLastStation = new Station { Name = row.LastStationName };
                        lock (_dblock)
                        {
                            _dbContext.Stations.Add(currentRowLastStation);
                        }
                        stationInDBRAM.Add(currentRowLastStation);
                    }
                }

                //work with trains
                lock (_lockTrains)
                {
                    currentRowTrain = savedTrainsRAM.FirstOrDefault(t => t.TrainId == row.TrainNumber);
                    if (currentRowTrain == null)
                    {
                        lock (_dblock)
                        {
                            currentRowTrain = _dbContext.Trains.FirstOrDefault(t => t.TrainId == row.TrainNumber);
                        }
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
                            lock (_dblock)
                            {
                                _dbContext.Trains.Add(currentRowTrain);

                            }
                            savedTrainsRAM.Add(currentRowTrain);

                        }
                    }
                }

                //work with Opernames
                lock (_lockOperations)
                {
                    currentOperName = operationsNamesInDBRAM.FirstOrDefault(o => o.Name == row.LastOperationName);
                    if (currentOperName == null)
                    {
                        currentOperName = new OperationName
                        {
                            Name = row.LastOperationName
                        };
                        lock (_dblock)
                        {
                            _dbContext.OperationNames.Add(currentOperName);
                        }
                        operationsNamesInDBRAM.Add(currentOperName);
                    }

                    // work with Freights
                    currentRowFreight = freightsInDBRAM.FirstOrDefault(f => f.Name == row.FreightEtsngName);

                    if (currentRowFreight == null)
                    {
                        currentRowFreight = new Freight { Name = row.FreightEtsngName };
                        lock (_dblock)
                        {
                            _dbContext.Freights.Add(currentRowFreight);
                        }
                        freightsInDBRAM.Add(currentRowFreight);
                    }
                }



                //work with Compositions (CombinedTrainIndex)

                lock (_lockCompositions)
                {
                    currentRowCompisition = savedCompositionRAM.FirstOrDefault(c => c.CombinedTrainIndex == row.TrainIndexCombined);
                    if (currentRowCompisition == null)
                    {
                        lock (_dblock)
                        {
                            currentRowCompisition = _dbContext.Compositions.FirstOrDefault(c => c.CombinedTrainIndex == row.TrainIndexCombined);
                        }
                        if (currentRowCompisition == null)
                        {

                            currentRowCompisition = new Composition
                            {
                                CombinedTrainIndex = row.TrainIndexCombined,
                                TrainId = currentRowTrain.TrainId,
                                Train = currentRowTrain
                            };
                            lock (_dblock)
                            {
                                _dbContext.Compositions.Add(currentRowCompisition);
                            }
                            savedCompositionRAM.Add(currentRowCompisition);
                        }
                    }
                }

                //process invoices 
                lock (_lockInvoices)
                {
                    currentInvoice = invoicesInDBRAM.FirstOrDefault(i => i.InvoiceNumber == row.InvoiceNum);
                    if (currentInvoice == null)
                    {
                        currentInvoice = new Invoice { InvoiceNumber = row.InvoiceNum };
                        _dbContext.Invoices.Add(currentInvoice);
                        invoicesInDBRAM.Add(currentInvoice);
                    }
                }

                //process cars
                lock (_lockCars)
                {
                    currentRowCar = savedCars.FirstOrDefault(c => c.CarNumber == row.CarNumber);
                    if (currentRowCar == null)
                    {
                        lock (_dblock)
                        {
                            currentRowCar = _dbContext.Cars.FirstOrDefault(c => c.CarNumber == row.CarNumber);
                        }
                        if (currentRowCar == null)
                        {
                            currentRowCar = new Car
                            {
                                CarNumber = row.CarNumber,
                                PositionInTarin = row.PositionInTrain,
                                InvoiceNumber = currentInvoice.InvoiceNumber,
                                FreightName = currentRowFreight.Name,
                                Weight = row.FreightTotalWeightKg,
                                CompositionNumber = currentRowCompisition.CombinedTrainIndex,
                                _Composition = currentRowCompisition,
                                _Freight = currentRowFreight,
                                _Invoice = currentInvoice
                            };
                            lock (_dblock)
                            {
                                _dbContext.Cars.Add(currentRowCar);
                            }
                            savedCars.Add(currentRowCar);
                        }
                    }
                }

                lock (_lockOperations)
                {
                    // DateTimeFormatInfo.InvariantInfo
                    // CultureInfo.InvariantCulture   - IFormtProvider
                    // DateTimeStyle.None ; DateTimeStyle.AssumeUniversal;  DateTimeStyle.RoundtripKind
                    //var isParsed = DateTime.TryParseExact(row.WhenLastOperation, dtformat,CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out operationTime); 
                    //operationTime = DateTime.ParseExact(row.WhenLastOperation, dtformat, CultureInfo.CurrentCulture);

                    var operationTime = DateTimeParser.Create(row.WhenLastOperation);

                    if (true)
                    {
                        currentRowOperation = savedOperationsRAM.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == operationTime);
                        if (currentRowOperation == null)
                        {
                            lock (_dblock)
                            {
                                currentRowOperation = _dbContext.Operations.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == operationTime);
                            }
                            if (currentRowOperation == null)
                            {
                                currentRowOperation = new Operation
                                {
                                    WhenLastOperation = operationTime,
                                    CarNumber = row.CarNumber,
                                    LastOperationName = row.LastOperationName,
                                    StationName = row.LastStationName,
                                    _Car = currentRowCar,
                                    _OperationName = currentOperName,
                                    _Station = currentRowLastStation
                                };
                                lock (_dblock)
                                {
                                    _dbContext.Operations.Add(currentRowOperation);
                                }
                            }
                            savedOperationsRAM.Add(currentRowOperation);
                        }
                    }
                    else
                    {
                        LogStorage.Add($"Warning. не получилось преобразовтаь {row.WhenLastOperation} в DateTime, для последующей записи в БД");
                    }
                }
            }
        }         



        public static List<string> GetStationsInDb(SMDbContext context)
        {
            var stations = new List<string>();
            foreach (var station in context.Stations)
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
