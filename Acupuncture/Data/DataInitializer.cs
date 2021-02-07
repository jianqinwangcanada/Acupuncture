using System;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction;
using Acupuncture.Model;
using Microsoft.AspNetCore.Identity;

namespace Acupuncture.Data
{
    public static class DataInitializer
    {
      
        public static async Task Initializer(ApplicationDbContext context,
            DataProtectionContext dpContext,
            ICommonFunction icommonFunction)
        {
            await context.Database.EnsureCreatedAsync();
            await dpContext.Database.EnsureCreatedAsync();

            if (context.appUsers.Any())
            { return; }
            //if (context.appUsers != null) { return; }

            await icommonFunction.CreateAdminUser();
            await icommonFunction.CreateAppUser();
        }
    }
}
