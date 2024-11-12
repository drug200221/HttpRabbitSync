using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CheckerHttpEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageUpdater
{
    public class RabbitMqConsumer
    {
        private const string QueueName = "facesCompleteEventsQueue";
        private readonly ConnectionFactory _factory = GlobalVariable.ConnectionFactory;
        private static readonly HttpClient HttpClient = new();

        public async Task StartConsumingAsync(List<(string type, string id)> cameras)
        {
            var entranceCameras = cameras.Where(camera => camera.type == "EntranceCamera").ToList();
            var exitCameras = cameras.Where(camera => camera.type == "ExitCamera").ToList();
            
            await using var connection = await _factory.CreateConnectionAsync();
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
                try
                {
                    if (jsonNode!["ExternalId"]?.ToString() != "")
                    {
                        int extId = Convert.ToInt32(jsonNode!["ExternalId"]?.ToString());
                        
                        if (exitCameras.Any(camera => camera.id.ToString() == jsonNode["ChannelId"]?.ToString()))
                        {
                            await SendRequestAsync(
                                ApiUrls.RoomerStatusApiUrl(extId),
                                HttpMethod.Put, false);
                        }
                        else if (entranceCameras.Any(camera => camera.id.ToString() == jsonNode["ChannelId"]?.ToString()))
                        {
                            await SendRequestAsync(
                                ApiUrls.RoomerStatusApiUrl(extId),
                                HttpMethod.Put, true);
                        }
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            };
            await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
            Console.ReadLine();
        }

        private async Task SendRequestAsync(string url, HttpMethod method, bool isInRoom)
        {
            using var request = new HttpRequestMessage(method, url);
            var jsonBody = JsonSerializer.Serialize(new { isInRoom });
            
            if (!string.IsNullOrEmpty(jsonBody))
            {
                try
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await HttpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Error: " + e);
                }
            }
        }
    }
}