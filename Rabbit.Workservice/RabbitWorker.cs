using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Workservice
{
    public class RabbitWorker : BackgroundService
    {
        private const string WORKER_DELAY = "ENV_WORKER_DELAY";
        private const string EXCHANGE_NAME = "ENV_EXCHANGE";
        private const string ROUTINGKEY_NAME = "ENV_ROUNTINGKEY";
        private readonly ILogger<RabbitWorker> logger;
        private ConnectionFactory factory;
        private IConnection connection;

        public RabbitWorker(ILogger<RabbitWorker> logger)
        {
            factory = new ConnectionFactory();
            this.logger = logger;

            factory.HostName = "192.168.99.100";

        }

        private void ConnectBroker()
        {
            if(connection == null || !connection.IsOpen)
                connection = factory.CreateConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting worker...");

            string workerDelay = Environment.GetEnvironmentVariable(WORKER_DELAY);
            int delay = 1000;
            if (!String.IsNullOrEmpty(workerDelay))
                delay = int.Parse(workerDelay);
            string exchange = Environment.GetEnvironmentVariable(EXCHANGE_NAME);
            string routingKey = Environment.GetEnvironmentVariable(ROUTINGKEY_NAME) ?? "";

            logger.LogInformation("Delay: {delay} ms", delay);
            logger.LogInformation("Exchange: {0}", exchange);
            logger.LogInformation("Routing Key: {0}", routingKey);

            if (String.IsNullOrEmpty(exchange))
                throw new ArgumentException("No exchange defined. Verify environment variable ENV_EXCHANGE", "ENV_EXCHANGE");

            while (!stoppingToken.IsCancellationRequested)
            {
                ConnectBroker();

                IModel channel = connection.CreateModel();
                byte[] message = System.Text.Encoding.UTF8.GetBytes($"{{\"date\":\"{DateTime.Now}\" \"name\":\"teste de fila\"}}");
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                properties.ContentType = "application/json";
                channel.BasicPublish(exchange, routingKey, properties, message);

                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                channel.Close();
                channel.Dispose();

                await Task.Delay(delay, stoppingToken);
            }

            connection.Close();
            connection.Dispose();
        }
    }
}
