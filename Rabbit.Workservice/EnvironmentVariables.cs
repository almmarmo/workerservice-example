using System;
using System.Collections.Generic;
using System.Text;

namespace Rabbit.Workservice
{
    public class EnvironmentVariables
    {
        public const string WORKER_DELAY = "ENV_WORKER_DELAY";
        public const string EXCHANGE_NAME = "ENV_EXCHANGE";
        public const string ROUTINGKEY_NAME = "ENV_ROUNTINGKEY";
        public const string BROKER_HOST = "ENV_BROKER_HOST";
        private readonly System.Collections.IDictionary variables;

        public EnvironmentVariables() :
            this(Environment.GetEnvironmentVariables())
        {
            
        }

        public EnvironmentVariables(System.Collections.IDictionary variables)
        {
            this.variables = variables;
            Exchange = variables[EXCHANGE_NAME].ToString();
            int.TryParse(variables.Contains(WORKER_DELAY) ? variables[WORKER_DELAY].ToString() : "0", out int delay);
            Delay = delay;
            RoutingKey = variables.Contains(ROUTINGKEY_NAME) ? variables[ROUTINGKEY_NAME].ToString() : "";
            BrokerHost = variables[BROKER_HOST].ToString();
        }

        public int Delay
        {
            get;
            private set;
        }

        public string Exchange
        {
            get;
            private set;
        }

        public string RoutingKey
        {
            get;
            private set;
        }

        public string BrokerHost
        {
            get;
            private set;
        }
    }
}
