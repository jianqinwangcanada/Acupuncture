using System;
using Acupuncture.CommonFunction.WritebleAppSettingFunction;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Acupuncture.CommonFunction.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigWritableSetting<T>(this IServiceCollection services,IConfigurationSection section,
            string fileName) where T : class,new()
        {
            services.Configure<T>(section);
            services.AddTransient<IWritebleSettingSvc<T>>(provider => {
                var environment = provider.GetService<IWebHostEnvironment>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritebleSettingSvc<T>(environment, options, section.Key, fileName);


            }
                );
        }
    }
}
