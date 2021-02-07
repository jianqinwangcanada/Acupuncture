using System;
using System.Threading.Tasks;

namespace Acupuncture.CommonFunction
{
    public interface ICommonFunction
    {
         Task CreateAdminUser();
         Task CreateAppUser();
    }
}
