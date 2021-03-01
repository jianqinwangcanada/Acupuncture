using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acupuncture.Model;

namespace Acupuncture.CommonFunction.CountryFunction
{
    public interface ICountrySvc
    {
       Task<List<CountryModel>> GetCountries();
    }
}
