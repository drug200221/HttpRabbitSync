using System.Text.Json.Nodes;
using System.Xml.Linq;
using HttpClient = System.Net.Http.HttpClient;

namespace CheckerHttpEvents;

static class Program
{
    static async Task Main()
    {
        RabbitMqProducer producer = new();
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "url.config");
        
        IsFileExists(filePath);

        string url = GetUrlFromConfig(filePath)!;
        
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Connection.Add("keep-alive");

            while (true)
            {
                try
                {
                    using (HttpResponseMessage response =
                           await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        await using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var reader = new StreamReader(contentStream))
                            {
                                string message = "";
                                while (!reader.EndOfStream)
                                {
                                    string? line = await reader.ReadLineAsync();
                                    if (line != "}")
                                    {
                                        message += line + "\n";
                                    }
                                    else
                                    {
                                        message += line + "\r";
                                        var jsonObject = JsonNode.Parse(message);
                                        if (jsonObject!["InitiatorName"]?.ToString() == "System")
                                        {
                                            Console.WriteLine(message);
                                        }
                                        else
                                        {
                                            await producer.SendMessageAsync(message);
                                            Console.WriteLine(message);
                                        }
                                        message = "";
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                }
            }
        }
    }

    private static void IsFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл url.config не был найден. Создаю...");
            var xmlContent = new XElement("Config",
                new XComment("Измените {параметры} на данные для подключения"),
                new XElement("ConnectionUrl",
                    "{protocol}://{host}:{port}/event?login={login}&password={password}&filter={filter}d&responsetype=json")
            );

            xmlContent.Save(filePath);
            Console.WriteLine("Файл url.config создан! Измените его.");
        }
    }

    private static string? GetUrlFromConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            XDocument xmlDoc = XDocument.Load(filePath);
            
            var urlElement = xmlDoc.Root!.Element("ConnectionUrl");
            return urlElement?.Value;
        }

        throw new FileNotFoundException("Файл конфигурации не найден.", filePath);
    }
}