using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApp.Utilities;

namespace WebApp.Controllers
{
    public class FileController : Controller
    {
        private IWebHostEnvironment _env;
        public FileController(IWebHostEnvironment env)
        {
            _env = env;
        }
        
    }

    //public class DownloadFile : IHttpHandler
    //{
    //    public void ProcessRequest(HttpContext context)
    //    {
    //        HttpResponse response = System.Web.HttpContext.Current.Response;
    //        response.ClearContent();
    //        response.Clear();
    //        response.ContentType = "text/plain";
    //        response.AddHeader("Content-Disposition",
    //                           "attachment; filename=" + fileName + ";");
    //        response.TransmitFile(Server.MapPath("FileDownload.csv"));
    //        response.Flush();
    //        response.End();
    //    }

    //
    //    Response.ContentType = "application/pdf";  
    //Response.AppendHeader("Content-Disposition", "attachment; filename=MyFile.pdf");  
    //Response.TransmitFile(Server.MapPath("~/Files/MyFile.pdf"));  
    //Response.End();  
    //
    //
}
