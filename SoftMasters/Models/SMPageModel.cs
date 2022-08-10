using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Xml.Serialization;
using WebApp.Utilities;

namespace WebApp.Models
{
    public class SMPageModel: PageModel
    {                
        public IFormFile? FormFile { get;set; }       
        public string OperationsInDB 
        { 
            get 
            {
                return DBWorker.GetOperationInDBAmount.ToString();
            }
             
        }
        
        public int SelectedTrain { get; set; }
        public List<int> Trains
        {
            get
            {
                return DBWorker._dbContext.Trains.Select(c=>c.TrainId).ToList();
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
