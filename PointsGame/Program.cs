using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PointsGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 导入配置文件
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            var host = WebHost.CreateDefaultBuilder(args)
                // 使用配置，可以在Startup的构造函数中接收到
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseUrls($"http://{config["AllowedHosts"]}:{config["Port"]}")
                .Build();

            host.Run();
        }
    }
}
