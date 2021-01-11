using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace 京东
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                services.AddHttpClient<JdClient>();
                services.AddHostedService<Main>();

            }).ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.AddFile("app.log");
            });
    }
}
