using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace 京东
{
    class Main : IHostedService
    {
        // private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly JdClient _jdClient;
        private string SkuId = "10023427418705"; //"100012043978"

        public Main(ILogger<Main> logger, JdClient jdClient)
        {
            _logger = logger;
            _jdClient = jdClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ValidateCookies();


            await GetLoginPageAsync();
            await GetQrCode();
            var hasTicket = false;
            while (!hasTicket)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                var r = await GetTicket();
                _logger.LogInformation(r.Data);
                if (r.Status == 302)
                {
                    await GetQrCode();
                    continue;
                }
                if (r.Status != 200)
                {
                    continue;
                }

                await ValidateTicket(r.Data);
                hasTicket = true;

            }
            var cookieValid = await ValidateCookies();
            if (!cookieValid)
            {
                _logger.LogInformation("登录失败，请用网页打开京东验证是否可以正常登录。");
            }

            Cookie.Write(Path.Combine("cookie.bin"), _jdClient.CookieContainer);



            var userName = await GetUserName();
            _logger.LogInformation(userName);
            var skuTitle = await GetSkuTitle();
            _logger.LogInformation(skuTitle);
            await GetSeckillUrl();
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

        async Task<(int Status, string Data)> GetTicket()
        {
            var token = _jdClient.CookieContainer.GetCookies(new Uri("https://jd.com/")).FirstOrDefault(f => f.Name == "wlfstk_smdl")?.Value;
            var url = $"https://qr.m.jd.com/check?appid=133&callback=jQuery{new Random().Next(1000000, 9999999)}&token={token}&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            _jdClient.Client.DefaultRequestHeaders.Referrer = new Uri("https://passport.jd.com/new/login.aspx");
            var resp = await _jdClient.Client.GetStringAsync(url);
            var json = JObject.Parse(Regex.Match(resp, "({.*})", RegexOptions.Singleline).Value);
            var code = json.Value<int>("code");
            if (code == 200)
            {
                return (code, json.Value<string>("ticket"));
            }
            return (code, json.Value<string>("msg"));
        }

        async Task ValidateTicket(string ticket)
        {
            var url = $"https://passport.jd.com/uc/qrCodeTicketValidation?t={ticket}";
            _jdClient.Client.DefaultRequestHeaders.Referrer = new Uri("https://passport.jd.com/uc/login?ltype=logout");
            var resp = await _jdClient.Client.GetStringAsync(url);
        }

        async Task<bool> ValidateCookies()
        {
            var url = $"https://order.jd.com/center/list.action?rid={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var resp = await _jdClient.Client.GetAsync(url);
            return resp.StatusCode == HttpStatusCode.OK;
        }

        async Task RequestUrl()
        {
            
        }

        async Task GetSeckillUrl()
        {
            var url = $"https://itemko.jd.com/itemShowBtn?callback=jQuery{new Random().Next(1000000, 9999999)}&skuId={SkuId}&from=pc&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            _jdClient.Client.DefaultRequestHeaders.Host = "itemko.jd.com";
            _jdClient.Client.DefaultRequestHeaders.Referrer = new Uri($"https://item.jd.com/{SkuId}.html");
            var resp = await _jdClient.Client.GetStringAsync(url);
            var json = JObject.Parse(Regex.Match(resp, "({.*})", RegexOptions.Singleline).Value);
            _logger.LogInformation(json.Value<string>("url"));

        }

        async Task<string> GetUserName()
        {
            var url = $"https://passport.jd.com/user/petName/getUserInfoForMiniJd.action?callback=jQuery{new Random().Next(1000000, 9999999)}&_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            _jdClient.Client.DefaultRequestHeaders.Referrer = new Uri("https://order.jd.com/center/list.action");
            var resp = await _jdClient.Client.GetAsync(url);
            var json = JObject.Parse(Regex.Match(await resp.Content.ReadAsStringAsync(), "({.*})", RegexOptions.Singleline).Value);
            return json.Value<string>("nickName");
        }

        async Task<string> GetSkuTitle()
        {
            var url = $"https://item.jd.com/{SkuId}.html";
            var resp = await _jdClient.Client.GetStringAsync(url);
            var title = Regex.Match(resp, "<title>(.*)</title>", RegexOptions.Singleline).Groups[1].Value;
            return title;
        }
    }
}
