using System;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acupuncture.Controllers.Api.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CountryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CountryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _db.countries.OrderBy(x => x.Name).ToListAsync();
            foreach(var country in countries)
            {
                country.Flag = string.Concat(country.TwoDigitCode.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
            }

            return Ok(countries);
        }
    }
}
