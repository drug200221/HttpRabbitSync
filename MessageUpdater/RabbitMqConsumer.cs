using System.Text;
using System.Text.Json.Nodes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageUpdater
{
    public class RabbitMqConsumer
    {
        private const string QueueName = "facesCompleteEventsQueue";
        private static readonly HttpClient HttpClient = new();

        public async Task StartConsumingAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5673,
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            var consumer = new AsyncEventingBasicConsumer(channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var jsonNode = JsonNode.Parse(message);
                if (jsonNode!["ExternalId"]?.ToString() != null)
                {
                    int extId = Convert.ToInt32(jsonNode!["ExternalId"]?.ToString());
                
                    if (jsonNode["ChannelId"]?.ToString() == "408881fb-0f21-42b5-85e2-0a378c35c461") // камера на выход
                    {
                        await SendRequestAsync($"http://localhost:8080/student/dormitories/api/v1/roomers/status/{extId}",
                            HttpMethod.Put, "{\"isInRoom\": false}");
                    }
                    else if (jsonNode["ChannelId"]?.ToString() == "cc12228c-77c8-425e-a884-880172af394a") // камера на вход
                    {
                        await SendRequestAsync($"http://localhost:8080/student/dormitories/api/v1/roomers/status/{extId}",
                            HttpMethod.Put, "{\"isInRoom\": true}");
                    }
                }
                return;
            };
            await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
        }

        private async Task SendRequestAsync(string url, HttpMethod method, string jsonBody)
        {
            using var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(jsonBody))
            {
                try
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await HttpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine("Получено!");
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Error: " + e);
                }
            }
        }
    }
}