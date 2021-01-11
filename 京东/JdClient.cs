using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace 京东
{
    public class JdClient
    {
        public HttpClient Client { get; private set; }
        public CookieContainer CookieContainer { get; set; }

        public JdClient(HttpClient httpClient)
        {
            CookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler() { CookieContainer = CookieContainer };
            HttpClient client = new HttpClient(handler);
            Client = client;
        }
    }
}
