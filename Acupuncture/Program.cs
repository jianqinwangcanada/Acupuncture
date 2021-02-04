using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction;
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
                try
                {
                    int a = 0;
                    int b = 3;
                    int z = b / a;
                }
                catch (Exception ex)
                {
                    Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
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
                      //.WriteTo.Console(theme: CustomConsoleTheme.VisualStudioMacLight)
                      .WriteTo.File(formatter: new LogFormat(), path: Path.Combine(webHostBuilderContext.HostingEnvironment.ContentRootPath + $"{Path.DirectorySeparatorChar}LogFile{Path.DirectorySeparatorChar}", $"_{DateTime.Now:yyyyMMdd}.txt"))
                     .ReadFrom.Configuration(webHostBuilderContext.Configuration));

                        webBuilder.UseStartup<Startup>();
                    
                });
    }
}
