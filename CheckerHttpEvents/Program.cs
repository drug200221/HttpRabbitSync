using System.Text.Json;
using System.Text.Json.Nodes;
using HttpClient = System.Net.Http.HttpClient;
using System.Text;

namespace CheckerHttpEvents;

public static class Program
{
    private static readonly string FilePath = GlobalVariable.FilePath;
    private static readonly RabbitMqProducer Producer = new();
    public static async Task Main()
    {
        IsFileExists(FilePath);

        string url = GetUrlFromConfig(FilePath) ?? throw new InvalidOperationException("URL не может быть пустым.");
        
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
           
            while (true)
            {
                try
                {
                    await ProcessHttpResponse(client, url, Producer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                    await Task.Delay(7000);
                }
            }
        }
    }
    
    private static async Task ProcessHttpResponse(HttpClient client, string url, RabbitMqProducer producer)
    {
        using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();

            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(contentStream))
            {
                StringBuilder messageBuilder = new StringBuilder();
                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line != "}")
                    {
                        messageBuilder.AppendLine(line);
                    }
                    else
                    {
                        messageBuilder.AppendLine(line);
                        await ProcessMessage(messageBuilder.ToString().TrimEnd('\r', '\n'), producer);
                        messageBuilder.Clear();
                    }
                }
            }
        }
    }

    private static async Task ProcessMessage(string message, RabbitMqProducer producer)
    {
        try
        {
            var jsonObject = JsonNode.Parse(message);
            if (jsonObject?["InitiatorName"]?.ToString() == "System")
            {
                Console.WriteLine(message);
            }
            else
            {
                await producer.SendMessageAsync(message);
                Console.WriteLine(message);
            }
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"Ошибка парсинга JSON: {jsonEx.Message}");
        }
    }

    private static void IsFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл app.config.json не был найден. Создаю...");

            var config = GetBaseConfig();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(config, options);
            File.WriteAllText(filePath, jsonString);
            
            Console.WriteLine("Файл app.config.json создан! Измените его.");
        }
    }

    private static object GetBaseConfig()
    {
        return new
        {
            Config = new
            {
                ConnectionUrl = "{protocol}://{address}:{port}/event?login={login}&password={password}&filter={filter}&responsetype=json",
                Branches = new[]
                {
                    new
                    {
                        id = "",
                        name = "Перевоз",
                        Dormitories = new[]
                        {
                            new
                            {
                                id = "",
                                name = "Общежитие 1",
                                Cameras = new[]
                                {
                                    new { type = "EntranceCamera", id = "" },
                                    new { type = "ExitCamera", id = "" }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static string? GetUrlFromConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            var jsonNode = JsonNode.Parse(File.ReadAllText(filePath));
            return jsonNode?["Config"]?["ConnectionUrl"]?.ToString();
        }

        throw new FileNotFoundException("Файл конфигурации не найден.", filePath);
    }
}
