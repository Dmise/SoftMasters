using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace WebApp.Controllers
{
    public class DataBaseController : Controller
    {
        public SMDbContext _dbContext { get; }
        //public DbTruncateContext _dbtruncate { get; }

        public DataBaseController(SMDbContext dbContext)
        {
            _dbContext = dbContext;
            
        }

        public IActionResult ClearDataBase()
        {
            //_dbContext.Operations.FromSqlRaw("TRUNCATE TABLE SoftMasters.Operations");
            //_dbContext.OperationNames.FromSqlRaw("TRUNCATE TABLE SoftMasters.OperationNames");
            //_dbContext.Compositions.FromSqlRaw("TRUNCATE TABLE SoftMasters.Compositions");
            //_dbContext.Cars.FromSqlRaw("TRUNCATE TABLE SoftMasters.Cars");
            //_dbContext.Invoices.FromSqlRaw("TRUNCATE TABLE SoftMasters.Invoices");  // DELETE FROM SoftMasters.Invoices;
            //_dbContext.Freights.FromSqlRaw("TRUNCATE TABLE SoftMasters.Freights");
            //_dbContext.Stations.FromSqlRaw("TRUNCATE TABLE SoftMasters.Stations");
            //_dbContext.Trains.FromSqlRaw("TRUNCATE TABLE SoftMasters.Trains");

            ViewBag.Message = "База данных очищена";
            return View("~/Views/Home/Buffer.cshtml");
            //return RedirectToAction();

            //DELETE FROM SoftMasters.Operations;
            //DELETE FROM SoftMasters.Invoices;
            //DELETE FROM SoftMasters.Cars;
            //DELETE FROM SoftMasters.Compositions;
            //DELETE FROM SoftMasters.Freights;
            //DELETE FROM SoftMasters.OperationNames;
            //DELETE FROM SoftMasters.Trains;
            //DELETE FROM SoftMasters.Stations;
        }
    }
}
