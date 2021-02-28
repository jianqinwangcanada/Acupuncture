using System;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction;
using Acupuncture.CommonFunction.CountryFunction;
using Acupuncture.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

namespace Acupuncture.Data
{
    public static class DataInitializer
    {
      
        public static async Task Initializer(ApplicationDbContext context,
            DataProtectionContext dpContext,
            ICommonFunction icommonFunction,ICountrySvc _countrySvc)
        {
            //check whether database exists
            if (!(context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
            {
                await context.Database.EnsureCreatedAsync();
            }
            if (!(dpContext.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
            {
                await dpContext.Database.EnsureCreatedAsync();

            }

            //await  dpContext.Database.MigrateAsync();

            if (context.appUsers.Any())
            { return; }
            //if (context.appUsers != null) { return; }

            await icommonFunction.CreateAdminUser();
            await icommonFunction.CreateAppUser();
            //Initialize the coutry datatbase table
            var countries = await _countrySvc.GetCountries();
            if (countries.Count > 0)
            {
                await context.countries.AddRangeAsync(countries);
                await context.SaveChangesAsync();
            }
        }
    }
}
