using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Server;
using My_Dashboard.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mqtt_Kafka_Connector.Services
{
    public class BridgeService : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IProducer<Null, string> _kafkaProducer;
        IServiceProvider _serviceProvider;
   
        private readonly IConfiguration _configuration;
        private readonly ILogger<BridgeService> _logger;
        private readonly BridgeConnector _bridgeConnector;
        public BridgeService( BridgeConnector bridgeConnector, ILogger<BridgeService> logger) {

       
            _logger = logger;
            _bridgeConnector = bridgeConnector;


        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if(!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting Service..");

                await _bridgeConnector.startBridge();


            }

        }

    }
}
