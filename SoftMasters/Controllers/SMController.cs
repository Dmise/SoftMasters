using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using WebApp.Data;
using WebApp.Models;
using WebApp.Utilities;


using System.Data;

using System.Text.RegularExpressions;
using SoftMasters.test.Utilities;

namespace WebApp.Controllers
{
    [Route("/testtasks/softmasters")]
    public class SMController : Controller
    {
        private string? fileToProcess = null;


        public event Func<string, IActionResult> OnAddToLog;


       
        private SMDbContext _dbContext;
        private IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public SMController(
            SMDbContext dbContext, 
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            
            _dbContext = dbContext;
            _env = env;
            _configuration = configuration;
            
        }

        [Route("update")]
        public ActionResult UpdatePage()
        {
            var model = new SMPageModel(_dbContext);
            return View("SMPage",model);
        }

        [HttpGet, Route("refresh")]
        public void RefreshPageAsync()
        {          
            
            //await Task.Run(() =>
            //{
            //    var model = new SMPageModel();
            //    return View(model);
            //});
            Response.Redirect(Request.Path, true);
        }

        [Route("page")]
        public IActionResult SMPage()
        {
            DBWorker.Configure(_dbContext);
            var model = new SMPageModel(_dbContext);
            return View(model);
        }

        [Route("clear-database")]
        public IActionResult ClearDataBase()
        {
            
            LogStorage.Add("начало зачистки базы данных");
            //TODO DI 
            var conn = _configuration.GetConnectionString("dmise.dev");
            var regExp = "(?<= base =)(.*)(?=;)";
            var dbName = Regex.Match(conn, regExp).Value;
            if (dbName != null)
            {
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Operations");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Invoices");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Cars");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Compositions");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Freights");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.OperationNames");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Trains");
                _dbContext.Database.ExecuteSqlRaw($"DELETE FROM {dbName}.Stations");
            }
            

            _dbContext.SaveChanges();
            LogStorage.Add("База данных очищена");
            var model = new SMPageModel(_dbContext);
            return View("SMPage", model);

            //_dbContext.Stations.FromSqlRaw("DELETE FROM SoftMasters.Stations");
            //_dbContext.Stations.FromSqlRaw("TRUNCATE TABLE SoftMasters.Stations");

        }

        //public static string Delete<T>(this DbSet<T> dbSet) where T : class
        //{
        //    string cmd = $"DELETE FROM {AnnotationHelper.TableName(dbSet)}";
        //    var context = dbSet.GetService<ICurrentDbContext>().Context;
        //    context.Database.ExecuteSqlRaw(cmd);
        //    return cmd;
        //}


        [Route("create-report")]
        public async Task<IActionResult> CreateReport(int train) // TODO FromForm
        {

            XMLWorker.CreateReportAsync(train, _env, _dbContext);
            var model = new SMPageModel(_dbContext);
            model.SelectedTrain = train;
            return View("SMPage", model);
        }

        [Route("download-report")]
        public async Task<ActionResult> DownlodReport()
        {
            var reportFI = XMLWorker.GetReportFI;
            if (reportFI != null)
            {
                var contenType = "application/vnd.ms-excel"; // 
                Stream stream = new FileStream(reportFI.FullName, FileMode.Open);


                return new FileStreamResult(stream, contenType)
                {
                    FileDownloadName = reportFI.Name
                };
            }
            LogStorage.Add("Отчет не создан. Сначала сформируйте отчет");
            return RedirectToAction("UpdatePage", "SM");
        }

        [HttpPost]
        [Route("upload-file")]        
        public async Task<IActionResult> UploadFile()
        {

            var files = Request.Form.Files;
            if (files != null && files.Count != 0)
            {
                var file = files[0];
                var filePath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    fileToProcess = filePath;
                }

                try
                {
                    if (fileToProcess != null)
                    {
                        DBWorker.Configure(_dbContext);
                        await DBWorker.LoadToDataBaseAsync(fileToProcess);
                    }
                    else
                        LogStorage.Add("файл с исходными XML данными не выбран");
                }                
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return View("Error");
                }
                SMPageModel model = new SMPageModel(_dbContext);
                return View("SMPage", model);
            }
            else
            {
                // TODO  js alert
                //return Content("<script language='javascript' type='text/javascript'>alert('Thanks for Feedback!');</script>");
                //Page page = HttpContext.Current.CurrentHandler as Page;
                return BadRequest("Нужно выбрать файл, который вы хотите загрузить в базу данных");
            }
        }

        #region TODO not completed, not used
        [HttpGet, Route("load-to-database")]
        private IActionResult LoadToDataBaseAsync(string filePath)
        {
            List<InvoiceXML> invList;
            try
            {
                XMLWorker.CreateInvoicesList(filePath, out invList);
            }
            catch(Exception ex)
            {
                return View("Error");
            }
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


                    var operationTime = DateTimeParser.Create(row.WhenLastOperation);
                    Operation? currentRowOperation = savedOperations.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == operationTime);
                    if (currentRowOperation == null)
                    {
                        currentRowOperation = _dbContext.Operations.FirstOrDefault(o => o.CarNumber == row.CarNumber && o.WhenLastOperation == operationTime);
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


        [HttpGet, Route("upload-fileRAM")]
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
        #endregion
    }
}