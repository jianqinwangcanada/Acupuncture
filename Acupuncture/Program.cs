using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction;
using Acupuncture.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Acupuncture
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope=host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var dpContext = services.GetRequiredService<DataProtectionContext>();
                    var commonService = services.GetRequiredService<ICommonFunction>();
                    DataInitializer.Initializer(context, dpContext, commonService).Wait();
                }
                catch (Exception ex)
                {
                    Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                }
      
            }


            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog((webHostBuilderContext,loggerConfiguration)=> 
                        loggerConfiguration.Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", "Acupuncture")
                      .Enrich.WithProperty("MachineName", Environment.MachineName)
                      .Enrich.WithProperty("CurrentManagedThreadId", Environment.CurrentManagedThreadId)
                      .Enrich.WithProperty("OSVersion", Environment.OSVersion)
                      .Enrich.WithProperty("Version", Environment.Version)
                      .Enrich.WithProperty("UserName", Environment.UserName)
                      .Enrich.WithProperty("ProcessId", Process.GetCurrentProcess().Id)
                      .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
                      .WriteTo.Console(theme: CustomerConsoleTheme.VisualStudioMacLight)
                     // .WriteTo.File(formatter: new LogFormat(), path: Path.Combine(webHostBuilderContext.HostingEnvironment.ContentRootPath + $"{Path.DirectorySeparatorChar}LogFile{Path.DirectorySeparatorChar}", $"_{DateTime.Now:yyyyMMdd}.txt"))
                     .ReadFrom.Configuration(webHostBuilderContext.Configuration)
                     );

                        webBuilder.UseStartup<Startup>();
                    
                });
    }
}
