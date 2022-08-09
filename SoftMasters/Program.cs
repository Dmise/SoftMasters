using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using WebApp.Data;
using WebApp.Utilities;
using OfficeOpenXml;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("dmise.dev");
var serverVesion = new MySqlServerVersion(new Version(5, 7));
builder.Services.AddDbContext<SMDbContext>(options => options.UseMySql(connectionString, serverVesion), ServiceLifetime.Scoped, ServiceLifetime.Scoped); // Pomelo.EFC

//Можно и кестрел настроить https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ConfigureEndpointDefaults(listenOptions =>
//    {
//       // serverOptions.
//    });
//});

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//builder.Services.AddDbContext<DbTruncateContext>(options => options.UseMySQL(connectionString)); // MySql.EFC

var app = builder.Build();

//  Forwarded Headers Middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
