using System;
using Newtonsoft.Json;

namespace Acupuncture.CommonFunction.CookieFunction
{
    public class IpInfo
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        //timezone
        [JsonProperty("timezone")]
        public string TimeZone { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }
    }
}
