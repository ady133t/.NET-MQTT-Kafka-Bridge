using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mqtt_Kafka_Connector.Services;
using MQTTnet;
using MQTTnet.Client;
using My_Dashboard.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace Mqtt_Kafka_Connector
{
    public class BridgeConnector
    {
        ConsumerConfig config;
        MqttFactory factory = new MqttFactory();
        IMqttClient _mqttClient;
        IProducer<Null,string> _kafkaProducer;
        //IEnumerable<Machine> _machines;
        //IConfigurationRoot _config;
        private readonly IConfiguration _configuration;
        IServiceProvider _serviceProvider;
        private readonly ILogger<BridgeConnector> _logger;

        public BridgeConnector(IMqttClient mqttClient, IProducer<Null, string> kafkaProducer, IConfiguration configuration , IServiceProvider serviceProvider , ILogger<BridgeConnector> logger) {



            this._mqttClient = mqttClient;
            this._kafkaProducer = kafkaProducer;
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;



        }


        public async Task startBridge()
        {
            var options = new MqttClientOptionsBuilder()
             .WithClientId("mqttclient")
             .WithTcpServer(_configuration["mqtt:server"], Convert.ToInt32(_configuration["mqtt:port"]))
             .Build();
            // Connect to the MQTT broker
            await _mqttClient.ConnectAsync(options, CancellationToken.None);


            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineUtilizationContext>();
                var machines = dbContext.Machines.Select(x => x).Where(x => x.IsActive);


                bool valid = true;

                foreach (var machine in machines)
                {
                    await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                       .WithTopic(machine.Name + "_mqtt")
                       .Build());

                    var topicCreatedorExist = await createTopic(machine.Name + "_kafka");
                    if (!topicCreatedorExist) valid = false;
                }

                // Handle received messages

                if (valid)
                    _mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedAsync;

            }


                

       

        }


        private async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            
            WriteLine($"Received message on topic '{e.ApplicationMessage.Topic}': {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}" , (msg) => _logger.LogInformation(msg));
            //using (var producer = new ProducerBuilder<Null, string>(config).Build())
            //{
            try
            {
                var deliveryResult = await _kafkaProducer.ProduceAsync(e.ApplicationMessage.Topic.Split('_')[0] + "_kafka", new Message<Null, string> { Value = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment) });
                WriteLine($"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'", (msg) => _logger.LogInformation(msg));
            }
            catch (ProduceException<Null, string> ex)
            {
                WriteLine($"Delivery failed: {ex.Error.Reason}", (msg) => _logger.LogError(msg));
            }
        }

        private bool TopicExists(IAdminClient adminClient, string topicName)
        {
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                return metadata.Topics.Any(topic => topic.Topic == topicName);
            }
            catch (KafkaException e)
            {
                WriteLine($"An error occurred fetching metadata: {e.Message}", (msg) => _logger.LogError(msg));
                return false;
            }
        }
        private async Task<bool> createTopic(string topic)
        {

            //using var adminClient = new AdminClientBuilder(_configuration.AsEnumerable()).Build();

            using var adminClient = new AdminClientBuilder(new AdminClientConfig() { BootstrapServers = _configuration["kafka:bootstrap.servers"] }).Build();

            if (TopicExists(adminClient, topic)) return true;
            // Define topic specification


            var topicSpec = new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 1
            };

            // Create the topic
            try
            {
                await adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });
                WriteLine($"Topic '{topicSpec.Name}' created successfully.", (msg) => _logger.LogInformation(msg));
                return true;
            }
            catch (CreateTopicsException e)
            {
                foreach (var result in e.Results)
                {
                    WriteLine($"An error occurred creating topic {result.Topic}: {result.Error.Reason}",(msg)=> _logger.LogWarning(msg));
                }

                return false;
            }

        }


        private Queue<string> outputBuffer = new Queue<string>();
        const int MaxLines = 20;
        private void WriteLine(string line, Action<string> logger)
        {
            if (outputBuffer.Count >= MaxLines)
            {
                Console.Clear();
                outputBuffer.Dequeue();
            }

            outputBuffer.Enqueue(line);

            foreach (string outputLine in outputBuffer)
            {
                logger(outputLine);
            }
        }
    }
}
