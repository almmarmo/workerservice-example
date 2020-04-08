using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Workservice;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = new EnvironmentVariables();
            var host = CreateHostBuilder(args).Build();
            
            if(env.Delay > 0)
                host.Run();
            else
            {
                var logger = host.Services.GetService<Microsoft.Extensions.Logging.ILogger<RabbitWorker>>();
                var worker = new RabbitWorker(logger);
                worker.Execute();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<RabbitWorker>();
                });
    }
}
