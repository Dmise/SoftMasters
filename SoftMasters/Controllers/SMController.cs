using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using WebApp.Data;
using WebApp.Models;
using WebApp.Utilities;


namespace WebApp.Controllers
{
    
    public class SMController : Controller
    {
        private string? fileToProcess = null;


        public event Func<string, IActionResult> OnAddToLog;


        private readonly ILogger<SMController> _logger;
        private SMDbContext _dbContext;
        private IWebHostEnvironment _env;

        public SMController(ILogger<SMController> logger, SMDbContext dbContext, IWebHostEnvironment env)
        {
            _logger = logger;
            _dbContext = dbContext;
            _env = env;
            
        }
        
        public ActionResult UpdatePage()
        {
            var model = new SMPageModel();
            return View("SMPage",model);
        }

        public void RefreshPageAsync()
        {          
            
            //await Task.Run(() =>
            //{
            //    var model = new SMPageModel();
            //    return View(model);
            //});
            Response.Redirect(Request.Path, true);
        }

        [Route("/testtasks/softmasters")]
        public IActionResult SMPage()
        {
            DBWorker.Configure(_dbContext);
            var model = new SMPageModel();
            return View(model);
        }

        public IActionResult ClearDataBase()
        {
            var model = new SMPageModel();
            LogStorage.Add("начало зачистки базы данных");

            _dbContext.Operations.FromSqlRaw("DELETE FROM SoftMasters.Operations");
            _dbContext.Invoices.FromSqlRaw("DELETE FROM SoftMasters.Invoices");
            _dbContext.Cars.FromSqlRaw("DELETE FROM SoftMasters.Cars");
            _dbContext.Compositions.FromSqlRaw("DELETE FROM SoftMasters.Compositions");
            _dbContext.Freights.FromSqlRaw("DELETE FROM SoftMasters.Freights");
            _dbContext.OperationNames.FromSqlRaw("DELETE FROM SoftMasters.OperationNames");
            _dbContext.Trains.FromSqlRaw("DELETE FROM SoftMasters.Trains");
            _dbContext.Stations.FromSqlRaw("DELETE FROM SoftMasters.Stations");

            LogStorage.Add("База данных очищена");
            return View("SMPage", model);


            //_dbContext.Operations.FromSqlRaw("TRUNCATE TABLE SoftMasters.Operations");
            //_dbContext.OperationNames.FromSqlRaw("TRUNCATE TABLE SoftMasters.OperationNames");
            //_dbContext.Compositions.FromSqlRaw("TRUNCATE TABLE SoftMasters.Compositions");
            //_dbContext.Cars.FromSqlRaw("TRUNCATE TABLE SoftMasters.Cars");
            //_dbContext.Invoices.FromSqlRaw("TRUNCATE TABLE SoftMasters.Invoices");  // DELETE FROM SoftMasters.Invoices;
            //_dbContext.Freights.FromSqlRaw("TRUNCATE TABLE SoftMasters.Freights");
            //_dbContext.Stations.FromSqlRaw("TRUNCATE TABLE SoftMasters.Stations");
            //_dbContext.Trains.FromSqlRaw("TRUNCATE TABLE SoftMasters.Trains");


        }


        [Route("CreateReport")]
        public async Task<IActionResult> CreateReport(int train) // TODO FromForm
        {
            var model = new SMPageModel();
            model.SelectedTrain = train;
            XMLWorker.CreateReportAsync(train, _env, _dbContext);
            return View("SMPage", model);
        }

        
        public async Task<IActionResult> UploadFile()
        {
            int fileAmount = 1;
            int fileCounter = 0;
            var files = Request.Form.Files;
            if (files != null && files.Count != 0)
            {
                foreach (var file in files)
                {
                    if (fileCounter == fileAmount)
                        break;
                    var filePath = Path.GetTempFileName();

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                        fileCounter++;
                        fileToProcess = filePath;
                    }

                }
                
                try 
                {
                    if (fileToProcess != null)
                    {
                       var dbWorker = new DBWorker(_dbContext);
                       await dbWorker.LoadToDataBaseAsync(fileToProcess);
                    }
                    else
                        LogStorage.Add("файл с исходными XML данными не выбран");  
                }
                catch(DbUpdateException ex)
                {
                    ex.Entries.ToString();
                }
                catch(Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return View("Error");
                }
                return View("SMPage");
            }
            else
            {
                //Это не файл,
                //return Content("<script language='javascript' type='text/javascript'>alert('Thanks for Feedback!');</script>");
                //Page page = HttpContext.Current.CurrentHandler as Page;
                return BadRequest("Нужно выбрать файл, котоырй вы хотите загрузить в базу данных");
            }
        }        

        private IActionResult LoadToDataBaseAsync(string filePath)
        {
            List<InvoiceXML> invList;
            XMLWorker.CreateInvoicesList(filePath, out invList);
            // read data from DB to local RAM
            var stationInDBRAM = _dbContext.Stations.ToList(); // можно использовать  LINQ чтобы подгрузить только значения полей, а не сущнсоти полность _dbContext.Trains.Select(t => t.TrainId).ToList();
            var trainsIndDB = _dbContext.Trains.Select(t => t.TrainId).ToList();
            var operationsNamesInDBRAM = _dbContext.OperationNames.ToList();
            var freightsInDBRAM = _dbContext.Freights.ToList();            
            var invoicesInDBRAM = _dbContext.Invoices.ToList();           

            List<Car> savedCars = new List<Car>();
            List<Operation> savedOperations = new List<Operation>();
            List<Composition> savedComposition = new List<Composition>();
            List<Train> savedTrainsRAM = new List<Train>();       

            var analyser = new Analyser(invList);

            //Parallel.For<> .//TODO сделать данный цикл параллельным  Parallel.For Loop

            foreach (var row in invList)
            {
                var messageToLog = $"Обработка строки. Вагон: {row.CarNumber}; Время операции: {row.WhenLastOperation}";
                LogStorage.Add(messageToLog);
                OnAddToLog?.Invoke(messageToLog); // TODO jquery ajax  динамическая загрузка строк лога на html страницу 
                UpdatePage();
                
                
                try
                {
                    Station? currentRowToStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.ToStationName);
                    Station? currentRowFromStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.FromStationName);
                    Station?  currentRowLastStation = stationInDBRAM.FirstOrDefault(s => s.Name == row.LastStationName);
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
                        if(currentRowTrain == null)
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
                    Freight? currentFreight = freightsInDBRAM.FirstOrDefault(f=>f.Name == row.FreightEtsngName); 
                    
                    if (currentFreight == null)
                    {
                        currentFreight = new Freight { Name = row.FreightEtsngName };
                        _dbContext.Freights.Add(currentFreight) ;
                        freightsInDBRAM.Add(currentFreight);
                    }


                    //work with Compositions (CombinedTrainIndex)
                    Composition? currentRowCompisition = savedComposition.FirstOrDefault(c => c.CombinedTrainIndex == row.TrainIndexCombined);
                    if (currentRowCompisition == null)                    {                        
                                              
                            currentRowCompisition = _dbContext.Compositions.FirstOrDefault(c => c.CombinedTrainIndex == row.TrainIndexCombined);
                        if (currentRowCompisition == null)
                        {                    
                        
                            currentRowCompisition = new Composition { 
                                CombinedTrainIndex = row.TrainIndexCombined,
                                TrainId = currentRowTrain.TrainId,
                                Train = currentRowTrain
                            };
                            _dbContext.Compositions.Add(currentRowCompisition);
                            savedComposition.Add(currentRowCompisition);
                        }
                    }

                    //process invoices 
                    Invoice? currentInvoice = invoicesInDBRAM.FirstOrDefault(i => i.InvoiceNumber == row.InvoiceNum);
                    if(currentInvoice == null)
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
                    


                    Operation? currentRowOperation = savedOperations.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == DateTime.Parse(row.WhenLastOperation));
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
                        savedOperations.Add(currentRowOperation);
                    }               
                  
                   
                }
                catch(Exception ex) 
                {
                    return View("Error");
                    //throw ex;
                }                        
                             

            } //foreach
            try
            {
                _dbContext.SaveChanges();
                ViewBag.Message = "Файл загружен на сервер"; 
            }
            catch(Exception ex)
            {
                throw;
            }

            return View();   
        }

        

        public async Task<IActionResult> UploadFileRAM() //TODO реализация обработки файла при этом не сохраняя его на жесткий диск
        {
            IFormFile file = Request.Form.Files[0];

            using (Stream stream = file.OpenReadStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string data = await reader.ReadToEndAsync();

                // Do something with file data
            }
            return View();
        }

    }
}