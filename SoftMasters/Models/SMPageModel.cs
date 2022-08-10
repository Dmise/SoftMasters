using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Xml.Serialization;
using WebApp.Data;
using WebApp.Utilities;

namespace WebApp.Models
{
    public class SMPageModel: PageModel
    {
        private SMDbContext _dbContext;
        public SMPageModel(SMDbContext context)
        {
            _dbContext = context;
        }
        public IFormFile? FormFile { get;set; }       
        public string OperationsInDB 
        { 
            get 
            {
                return _dbContext.Operations.Count().ToString();
                //return DBWorker.GetOperationInDBAmount.ToString();
            }
             
        }
        
        public int SelectedTrain { get; set; }
        public List<int> Trains
        {
            get
            {
                try
                {
                    return _dbContext.Trains.Select(t => t.TrainId).ToList();
                }
                catch
                {
                    return new List<int>(); 
                }
                
                //return DBWorker.GetTrainsIDs;
            }
        }
        public string Logtext
        {
            get
            {
                if (Log.Length != 0)
                {
                    StringBuilder logtext = new StringBuilder();
                    for (int i = 0; i < Log.Length; i++)
                    {
                        logtext.Append(Log[i]).Append("\n");
                    }
                    return logtext.ToString();
                }
                return "Лог не прогрузился";
            }
        }
        public string[] Log 
        {
            get
            {
                
                string[] log = LogStorage.Getlog;
                return log;
            }
            
        }

    }

   
}
