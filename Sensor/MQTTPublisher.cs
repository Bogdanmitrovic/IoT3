using System.Globalization;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;

namespace Sensor;

public static class MQTTPublisher
{
    private const string Format = "yyyy-MM-dd HH:mm:ss";
    public static async Task Publish_Application_Message()
    {
        var mqttFactory = new MqttFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("mosquitto")
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var lines = await File.ReadAllLinesAsync("data.csv");
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            var record = new Record
            {
                Timestamp = DateTime.ParseExact(parts[0], Format, CultureInfo.InvariantCulture),
                Junction = int.Parse(parts[1]),
                Vehicles = int.Parse(parts[2]),
                Id = parts[3]
            };
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("sensorData")
                .WithPayload(JsonConvert.SerializeObject(record))
                .Build();
            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            Console.WriteLine("sent " + line);
            await Task.Delay(1000);
        }

        await mqttClient.DisconnectAsync();
            
        Console.WriteLine("MQTT application message is published.");
    }

}