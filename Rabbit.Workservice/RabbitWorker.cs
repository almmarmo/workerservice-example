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
        private readonly ILogger<RabbitWorker> logger;
        private readonly ConnectionFactory factory;
        private readonly EnvironmentVariables environmentVariables;
        private IConnection connection;

        public RabbitWorker(ILogger<RabbitWorker> logger)
        {
            environmentVariables = new EnvironmentVariables();
            factory = new ConnectionFactory();
            this.logger = logger;

            factory.HostName = environmentVariables.BrokerHost;
            logger.LogInformation("Worker started.");
        }

        private void ConnectBroker()
        {
            if (connection == null || !connection.IsOpen)
            {
                connection = factory.CreateConnection();
                logger.LogInformation("Connection with broker established.");
            }
        }

        public void Execute()
        {
            Valid();
            ConnectBroker();
            SendMessage();
            CloseConnection();
        }

        private void Valid()
        {
            logger.LogInformation("Delay: {delay} ms", environmentVariables.Delay);
            logger.LogInformation("Exchange: {0}", environmentVariables.Exchange);
            logger.LogInformation("Routing Key: {0}", environmentVariables.RoutingKey);

            if (String.IsNullOrEmpty(environmentVariables.Exchange))
                throw new ArgumentException("No exchange defined. Verify environment variable ENV_EXCHANGE", "ENV_EXCHANGE");
        }

        private void CloseConnection()
        {
            connection.Close();
            connection.Dispose();
            logger.LogInformation("Connection closed.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Valid();

            ConnectBroker();

            while (!stoppingToken.IsCancellationRequested)
            {
                SendMessage();
                await Task.Delay(environmentVariables.Delay, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            CloseConnection();
            return base.StopAsync(cancellationToken);
        }

        private void SendMessage()
        {
            IModel channel = connection.CreateModel();
            byte[] message = System.Text.Encoding.UTF8.GetBytes($"{{\"date\":\"{DateTime.Now}\" \"name\":\"teste de fila\"}}");
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            properties.ContentType = "application/json";
            channel.BasicPublish(environmentVariables.Exchange, environmentVariables.RoutingKey, properties, message);

            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            channel.Close();
            channel.Dispose();

            logger.LogInformation("Message sended.");
        }
    }
}
