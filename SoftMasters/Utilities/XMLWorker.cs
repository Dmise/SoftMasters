using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Utilities
{
     
    public class XMLWorker 
    {
        private static IWebHostEnvironment? _webenv = null; //TODO использоватьвэтом классе IWebHostEnvironment.WebRootPath
        

        public static IWebHostEnvironment? WebEnv
        {
            get
            {
                return _webenv; 
                
            }
            set { _webenv = value; }
        }
        public static FileInfo? GetReportFI
        {
            get 
            {
                if(_webenv != null)
                {
                    return new FileInfo(Path.Combine(_webenv.WebRootPath, "appdata/report.xlsx"));
                }


                return null;
                
            }
        }
        public XMLWorker(IWebHostEnvironment webHostenv)
        {
            if (webHostenv == null)
                throw new ArgumentNullException("IWebHostEnvironment webHostenv");
            _webenv = webHostenv;
        }
        public static T Deserialize<T>(string path)
        {
            T result;
            using (var stream = System.IO.File.Open(path, FileMode.Open))
            {
                result = Deserialize<T>(stream);
            }
            return result;
        }

        public static void Serialize<T>(T root, string path)
        {
            using (var stream = System.IO.File.Open(path, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stream, root);
            }
        }

        public static T Deserialize<T>(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(stream);
        }
        public static List<Operation> GatherOperations(List<InvoiceXML> operations)
        {
            var operationList = new List<Operation>();
            foreach (var oper in operations) //TODO оптимальный поиск по двум параметрам (составной ключ)
            {
                var operTime = DateTime.Parse(oper.WhenLastOperation);
                var carId = oper.CarNumber;
                var thatOperation = operationList.Where(o => o.CarNumber == carId && o.WhenLastOperation == operTime).Count();
                if (thatOperation > 1)
                {
                    //мы сюда не должны попадать
                }
                if (thatOperation == 0)
                {

                    operationList.Add(new Operation
                    {
                        WhenLastOperation = operTime,
                        LastOperationName = oper.LastOperationName,
                        CarNumber = carId,
                        StationName = oper.LastStationName

                    });
                }
            }
            return operationList;
        }
        public static void CreateInvoicesList(string filepath, out List<InvoiceXML> output) //TODO  сделать нормальную дериализацию документа через Deserialize.
        {
            output = new List<InvoiceXML>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filepath);
            XmlElement? xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                foreach (XmlElement row in xRoot)
                {
                    InvoiceXML inv = new InvoiceXML();
                    inv.TrainNumber = Convert.ToInt32(row.GetElementsByTagName("TrainNumber")[0].InnerText);
                    inv.TrainIndexCombined = row.GetElementsByTagName("TrainIndexCombined")[0].InnerText;
                    inv.FromStationName = row.GetElementsByTagName("FromStationName")[0].InnerText;
                    inv.ToStationName = row.GetElementsByTagName("ToStationName")[0].InnerText;
                    inv.LastStationName = row.GetElementsByTagName("LastStationName")[0].InnerText;
                    inv.WhenLastOperation = row.GetElementsByTagName("WhenLastOperation")[0].InnerText;
                    inv.LastOperationName = row.GetElementsByTagName("LastOperationName")[0].InnerText;
                    inv.InvoiceNum = row.GetElementsByTagName("InvoiceNum")[0].InnerText;
                    inv.PositionInTrain = Convert.ToInt32(row.GetElementsByTagName("PositionInTrain")[0].InnerText);
                    inv.CarNumber = Convert.ToInt32(row.GetElementsByTagName("CarNumber")[0].InnerText);
                    inv.FreightEtsngName = row.GetElementsByTagName("FreightEtsngName")[0].InnerText;
                    inv.FreightTotalWeightKg = Convert.ToInt32(row.GetElementsByTagName("FreightTotalWeightKg")[0].InnerText);
                    output.Add(inv);                    
                }
            }
        }
        
        public static async Task CreateReportAsync(int trainId, IWebHostEnvironment env, SMDbContext context) // не нашёл я лучшего способа чем скинуть IWebHostEnvironment из контроллера.
        {
            var composition = context.Compositions.FirstOrDefault(c => c.TrainId == trainId);
            if (_webenv == null)
            {
                _webenv = env;
            }

            if(composition == null )
            {
                LogStorage.Add("У данного поезда нету составов");
                return;
            }
            else 
            {
                var tempalteFileName = "report_template.xlsx";
                var appdataDir = Path.Combine(env.WebRootPath, "appdata/");
                var combinedPath = Path.Combine(appdataDir, tempalteFileName);
                //var fullpath = Path.GetFullPath(fileRelative);                      
                var templateFullPath = PathCreator.Canonicalize(combinedPath);
                var templateFI = new FileInfo(templateFullPath);
                FileInfo reportFI;
                
                try
                {
                    reportFI = templateFI.CopyTo(Path.Combine(appdataDir, "report.xlsx"), overwrite: true); //TODO lock
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                //gridview
                string regComposition = "(?<=-)(.*)(?=-)";
                var compositionNumber = Regex.Match(composition.CombinedTrainIndex, regComposition).Value;
                context.Cars.Where(x => x.CompositionNumber == composition.CombinedTrainIndex).ToList();

                using var package = new ExcelPackage(reportFI);

                var rs = package.Workbook.Worksheets[0];
                var E6 = rs.Cells["E6"].Value;
                rs.Cells["C3"].Value = trainId; //C3 - поезд
                rs.Cells["C4"].Value = compositionNumber; //C4 - состав
                var train = context.Trains.FirstOrDefault(c => c.TrainId == trainId);
                rs.Cells["E3"].Value = train.FromStationName; //E3 - Станция отправления


                
                var cars = context.Cars.Where(c => c.CompositionNumber == composition.CombinedTrainIndex).ToList(); // все вагоны состава поезда
                var carNumbers = cars.Select(c => c.CarNumber); // номера вагонов

                var operationsOnTrainCars = context.Operations.Where(o => carNumbers.Contains(o.CarNumber));                
                var operationsTime = operationsOnTrainCars.Select(o => o.WhenLastOperation).ToList();
                var latestOperation = operationsTime.Max();  // самая поздняя операция
                rs.Cells["E4"].Value = latestOperation.ToString("dd.MM.yyyy");//E4 - дата отправления/

                for (int i = 0; i < cars.Count(); i++)
                {
                    var row = i + 7;  //7 строка - начало заполнения строк
                    rs.Cells[row, 1].Value = i + 1;  //А  - номер записи
                    rs.Cells[row, 2].Value = cars[i].CarNumber; //B  - номер вагона 
                    rs.Cells[row, 3].Value = cars[i].InvoiceNumber; //C  - накладная
                    rs.Cells[row, 4].Value = rs.Cells["E4"].Value; //D  - дата отправления  //TODO  не думаю ,что так
                    rs.Cells[row, 5].Value = cars[i].FreightName; //E  - груз
                    rs.Cells[row, 6].Value = cars[i].Weight; //F  - вес



                    var operationOnDepartureStation = operationsOnTrainCars.Where(o=>o.StationName == train.FromStationName).FirstOrDefault(o=>o.CarNumber == cars[i].CarNumber); // операция над вагонов на станции отправления
                    var lastCarOperation = operationsOnTrainCars.Where(o => o.CarNumber == cars[i].CarNumber).OrderByDescending(o => o.LastOperationName).First();
                    if (lastCarOperation != null)
                    {
                        rs.Cells[row, 7].Value = lastCarOperation.LastOperationName; //G  - последняя операция}
                    }                    
                    else
                    {
                        rs.Cells[row, 7].Value = @"ОШИБКА. Операций над вагоном не найдено";
                    }

                }

                await package.SaveAsync();
                LogStorage.Add("Отчет составлен");
                             

            }
            
            
        }
        
        private static FileInfo CreateReportFile()
        {
            throw new NotImplementedException("FileInfo CreateReportFile()");
            
        }

        private static void SaveExcelFile(List<CarInvoice> cars,FileInfo reportFI)
        {
            
        }

        public LocalStorage RowLoop() { return new LocalStorage(); }  // TODO функция для параллельной записи в БД.  Parallel.For Loop 
    }
}
