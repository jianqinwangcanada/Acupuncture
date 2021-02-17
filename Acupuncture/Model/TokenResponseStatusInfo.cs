using System;
using System.Net;

namespace Acupuncture.Model
{
    public class TokenResponseStatusInfo
    {
       public string Message { get; set; }
        public HttpStatusCode httpStatusCode { get; set; }
    }
}
