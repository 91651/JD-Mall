using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace 京东
{
    class Main : IHostedService
    {
        // private readonly IHttpClientFactory _httpClientFactory;
        private readonly JdClient _jdClient;

        public Main(JdClient jdClient)
        {
            _jdClient = jdClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await GetLoginPageAsync();
            await GetQrCode();
            await GetTicket();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        void Init()
        {


        }

        async Task GetLoginPageAsync()
        {
            var url = "https://passport.jd.com/new/login.aspx";
            var page = await _jdClient.Client.GetStringAsync(url);
            
        }

        async Task GetQrCode()
        {
            var url = $"https://qr.m.jd.com/show?appid=133&size=147&t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            // var payload = new { appid = 133, size = 147, t = "" };
            var resp = await _jdClient.Client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(Path.Combine("qr_code.png"), resp);
        }

        async Task GetTicket()
        {
            var token = _jdClient.CookieContainer.GetCookies(new Uri("https://jd.com/")).FirstOrDefault(f => f.Name == "wlfstk_smdl")?.Value;
            var url = $"https://qr.m.jd.com/check?appid=133&callback=jQuery{new Random().Next(1000000, 9999999)}&token={token}&_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            _jdClient.Client.DefaultRequestHeaders.Referrer = new Uri("https://passport.jd.com/new/login.aspx");
            var resp = await _jdClient.Client.GetStringAsync(url);
        }
    }
}
