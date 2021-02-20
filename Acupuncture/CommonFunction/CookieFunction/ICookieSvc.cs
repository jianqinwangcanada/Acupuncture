using System;
using System.Collections.Generic;

namespace Acupuncture.CommonFunction.CookieFunction
{
    public interface ICookieSvc
    {
        public string Get(string key);


        public void SetCookie(string key, string value, int? expireTime, bool isSecure, bool isHttpOnly);


        public void SetCookie(string key, string value, int? expireTime);


        public void DeleteCookie(string key);

        public void DeleteAllCookies(IEnumerable<string> cookiesToDelete);
        public string GetUserOS();
        public string GetUserIP();
        public string GetUserCountry();



    }
}
