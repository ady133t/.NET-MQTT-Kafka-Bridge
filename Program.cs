// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mqtt_Kafka_Connector.Services;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using My_Dashboard.Models.DB;
using Mqtt_Kafka_Connector;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
       

        services.AddSingleton<IMqttClient>(instance => { return new MqttFactory().CreateMqttClient(); });
        services.AddSingleton<IProducer<Null,string>>(instance =>  new ProducerBuilder<Null, string>(
        new ProducerConfig
        {
           
            BootstrapServers = hostContext.Configuration["kafka:bootstrap.servers"],

        }
         ).Build());
       
      
        services.AddDbContext<MachineUtilizationContext>(options => 
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("MachineConnection")));
        

        services.AddHostedService<BridgeService>();

        services.AddTransient<BridgeConnector>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders(); // Clears default providers
        logging.AddConsole();    // Adds console logging
    });

var host = builder.Build();


// Create a cancellation token source to initiate gracefull shutdown
var cts = new CancellationTokenSource();
// Attach Ctrl+C event for graceful shutdown
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // Prevents the application from crashing
    cts.Cancel();
};

// Start the application, which also starts the background service
await host.RunAsync(cts.Token);

Console.WriteLine("Service run");
//Console.ReadLine();

