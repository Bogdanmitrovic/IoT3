using System.Text;
using Microsoft.AspNetCore.Connections;
using MQTTnet;
using MQTTnet.Client;
using NATS.Client;
using Newtonsoft.Json;

namespace Filter;

public static class MQTTClient
{
    private static IConnection? _natsConnection;

    public static void InitNats()
    {
        Options opts = ConnectionFactory.GetDefaultOptions();
        opts.Url = "nats://nats:4222";
        var cf = new ConnectionFactory();
        var conn = cf.CreateConnection(opts);
        _natsConnection = conn;
    }

    public static async Task Connect_Client()
    {
        var mqttFactory = new MqttFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("mosquitto").Build();
        var sum = 0;
        var count = 0;
        DateTime? start = null;

        mqttClient.DisconnectedAsync+=(async e =>
        {
            Console.WriteLine("Disconnected from broker: " + e.Reason);
            if (e.Exception != null)
            {
                Console.WriteLine("Exception: " + e.Exception.Message);
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await mqttClient.ConnectAsync(mqttClientOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Reconnection failed: " + ex.Message);
            }
        });
        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            Console.WriteLine("Received application message.");
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            var record = JsonConvert.DeserializeObject<Record>(message);
            if (record == null)
            {
                Console.WriteLine("Failed to deserialize message.");
                return Task.CompletedTask;
            }

            if (start == null || record.Timestamp.Date != start.Value.Date)
            {
                if (start != null)
                {
                    Console.WriteLine($"Average vehicles: {(double)sum / count}");
                    var avgRecord = new AverageRecord
                    {
                        Timestamp = start.Value,
                        Vehicles = (double)sum / count,
                        Id = "avg"+start.Value.ToString("yyyy-MM-dd")
                    };
                    var serialized = JsonConvert.SerializeObject(avgRecord);
                    _natsConnection?.Publish("avg", Encoding.ASCII.GetBytes(serialized));
                }

                start = record.Timestamp;
                sum = 1;
                count = 1;
            }
            else
            {
                sum += record.Vehicles;
                count++;
            }

            return Task.CompletedTask;
        };

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        Console.WriteLine("connected");

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f => { f.WithTopic("sensorData"); })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine("MQTT client subscribed to topic.");
        while (true)
        {
            await Task.Delay(1000);
        }
    }
}