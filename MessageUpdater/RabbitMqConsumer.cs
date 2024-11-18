using System.Net;
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
        private static readonly CookieContainer _cookieContainer = new CookieContainer();
        private static readonly HttpClientHandler _handler = new HttpClientHandler { CookieContainer = _cookieContainer };
        private static readonly HttpClient _httpClient = new HttpClient(_handler);
        private static bool _isAuthenticated = false;

        public async Task StartConsumingAsync(List<(string type, string id)> cameras)
        {
            var entranceCameras = cameras.Where(camera => camera.type == "EntranceCamera").ToList();
            var exitCameras = cameras.Where(camera => camera.type == "ExitCamera").ToList();

            while (true)
            {
                try
                {
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
                            Console.WriteLine("Сообщение обработано!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                        }
                    };

                    await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
                    Console.WriteLine("Подписался на очередь!");

                    await Task.Delay(Timeout.Infinite);  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка подключения: " + ex.Message);
                    Console.WriteLine("Бывает...");
                    await Task.Delay(5000);
                }
            }
        }

        private async Task SendRequestAsync(string url, HttpMethod method, bool isInRoom)
        {
            if (!_isAuthenticated || !AreCookiesValid())
            {
                await AuthenticateAsync();
            }

            using var request = new HttpRequestMessage(method, url);
            var jsonBody = JsonSerializer.Serialize(new { isInRoom });

            if (!string.IsNullOrEmpty(jsonBody))
            {
                try
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Ошибка: " + e);
                }
            }
        }
        
        private static async Task AuthenticateAsync()
        {
            string url = ApiUrls.LoginUrl();

            var parameters = new MultipartFormDataContent
            {
                { new StringContent(Environment.GetEnvironmentVariable("DEMON_USER_ID")), "id" },
                { new StringContent(Environment.GetEnvironmentVariable("DEMON_PASSWORD")), "password" }
            };

            try
            {
                var response = await _httpClient.PostAsync(url, parameters);
                response.EnsureSuccessStatusCode();
     

                _isAuthenticated = true; 
                Console.WriteLine("Авторизовался!");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }
        }

        private static bool AreCookiesValid()
        {
            var cookies = _cookieContainer.GetCookies(new Uri(ApiUrls.LoginUrl()));

            foreach (Cookie cookie in cookies)
            {
                if (cookie.Expired)
                {
                    return false;
                }
            }

            return cookies.Count > 0;
        }
    }
}